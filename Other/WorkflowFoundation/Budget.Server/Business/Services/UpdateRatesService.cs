using System;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.Server.API.Interface.Faults;
using Budget2.Server.Business.Interface.Services;
using System.Collections.Generic;
using Common;
using Common.WCF;
using System.Text;

namespace Budget2.Server.Business.Services
{
    public class UpdateRatesBuinessService : Budget2DataContextService, IUpdateRatesService
    {
        public BaseFault UpdateRates(IEnumerable<API.Interface.DataContracts.CurrencyRate> currencyRates)
        {
            Logger.Log.Debug("Старт обновление курсов валют.");

            Logger.Log.Debug("Выходящие данные.");

            foreach (var item in currencyRates)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var prop in item.GetType().GetProperties())
                {
                    sb.AppendFormat("{0}-'{1}';", prop.Name, prop.GetValue(item, null));
                }
                Logger.Log.DebugFormat(sb.ToString());
            }

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    var ccList = context.Currencies.ToList(); 

                    #region Создаём новые валюты

                    foreach (var currencyRate in currencyRates)
                    {   
                        CreateCurrency(context, ccList, currencyRate.Name, currencyRate.ISOCode, false);
                        CreateCurrency(context, ccList, currencyRate.BaseCurrencyName, currencyRate.BaseCurrencyISOCode, true);
                    }

                    try
                    {
                        Logger.Log.Debug("Сохраняем информацию о валютах.");
                        context.SubmitChanges();
                        Logger.Log.Debug("Сохранение валют прошло успено.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error("Ошибка сохранения в БД.", ex);
                        return new CurrencyUploadFault();
                    }

                    #endregion

                    #region Заводим курсы

                    foreach (var currencyRate in currencyRates)
                    {
                        var currency =
                            (ccList.Where(curr => curr.Code == currencyRate.ISOCode)).FirstOrDefault();
                        if (currency == null)
                        {
                            Logger.Log.ErrorFormat("Валюта {0} не найдена.", currencyRate.ISOCode);
                            continue;
                        }

                        CreateCurrencyRate(context, currencyRate, currency);
                    }

                    try
                    {
                        Logger.Log.Debug("Сохраняем информацию о курсах валют.");
                        context.SubmitChanges();
                        Logger.Log.Debug("Сохранение курсов валют прошло успено.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error("Ошибка сохранения в БД.", ex);
                        return new RateUploadFault();
                    }

                    #endregion
                }

                scope.Complete();
            }

            Logger.Log.Debug("Завершение обновления курсов валют.");
            return null;
        }

        

        private static void CreateCurrencyRate(Budget2DataContext context, API.Interface.DataContracts.CurrencyRate currencyRate, Currency currency)
        {
            var crs = context.CurrencyRates.Where(currR => currR.RateDate.Date == currencyRate.RevaluationDate.Date
                                                           && currR.CurrencyId == currency.Id);
            if (crs.Count() > 0)
                UpdateExistingCurrencyRate(currencyRate, crs);
            else
                CreateNewCurrencyRate(context, currencyRate, currency);
        }

        private static void CreateNewCurrencyRate(Budget2DataContext context, API.Interface.DataContracts.CurrencyRate currencyRate, Currency currency)
        {
            var newCurrRate = new CurrencyRate
                                           {
                                               CurrencyId = currency.Id,
                                               RateDate = currencyRate.RevaluationDate
                                           };

            newCurrRate.Rate = currencyRate.Measure == 0 ? (decimal)0 : currencyRate.Rate / currencyRate.Measure;
            newCurrRate.RevRate = newCurrRate.Rate == 0 ? 0 : 1/newCurrRate.Rate;
            
            context.CurrencyRates.InsertOnSubmit(newCurrRate);
        }

        private static void UpdateExistingCurrencyRate(API.Interface.DataContracts.CurrencyRate currencyRate, IEnumerable<CurrencyRate> crs)
        {
            foreach (var cr in crs)
            {
                cr.Rate = currencyRate.Measure == 0 ? (decimal)0  : currencyRate.Rate/currencyRate.Measure;
                cr.RevRate = cr.Rate == 0 ? (decimal) 0 : (decimal) 1/cr.Rate;
            }
        }

        private static void CreateCurrency(Budget2DataContext context, ICollection<Currency> ccList, string currencyName, string currencyISOCode, bool isBase)
        {
            var currency = ccList.FirstOrDefault(curr => curr.Code == currencyISOCode);
            if (currency == null)
            {   
                if (isBase)
                {
                    int count = ccList.Where(curr => curr.IsBase).Count();
                    if (count > 0)
                    {
                        Logger.Log.ErrorFormat("Валюта {0} - {1} не может быть создана в качестве базовой.", currencyISOCode, currencyName);
                        return;
                    }
                }

                var newCurrency = new Currency
                                   {
                                       Id = Guid.NewGuid(),
                                       Name =
                                           !string.IsNullOrEmpty(currencyName)
                                               ? currencyName
                                               : currencyISOCode,
                                       Code = currencyISOCode,
                                       IsBase = isBase,
                                       IsDeleted = false
                                   };

                ccList.Add(newCurrency);
                context.Currencies.InsertOnSubmit(newCurrency);

                Logger.Log.DebugFormat("{3} {0} - {1} добавлена. ID={2}.",
                    newCurrency.Code, newCurrency.Name, newCurrency.Id,
                    newCurrency.IsBase ? "Базовая валюта" : "Валюта");
            }
            else if (currency.Name != currencyName || currency.IsBase != isBase || currency.IsDeleted)
            {
                UpdateCurrency(context, ccList, currency, currencyISOCode, currencyName, isBase);
            }
        }

        private static bool UpdateCurrency(Budget2DataContext context, ICollection<Currency> ccList, Currency currency, string currencyISOCode, string currencyName, bool isBase)
        {
            if (isBase && !currency.IsBase)
            {
                int count = ccList.Where(curr => curr.IsBase).Count();
                if (count > 0)
                {
                    Logger.Log.ErrorFormat("Валюта {0} - {1} не может быть указана в качестве базовой.", currencyISOCode, currencyName);
                    return false;
                }
            }

            var upCurrency = (from c in context.Currencies where c.Code == currencyISOCode select c);
            foreach (var cur in upCurrency)
            {
                cur.Name = currencyName;
                cur.IsDeleted = false;
                cur.IsBase = isBase;

                ccList.Remove(currency);
                ccList.Add(cur);

                Logger.Log.DebugFormat("{0} {1} - {2} обновлена. ID={3}",
                isBase ? "Базовая валюта" : "Валюта", currencyISOCode, currencyName, cur.Id);
            }

            return true;
        }
    }
}