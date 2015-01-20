using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using Budget2.Server.API.Interface.DataContracts;
using Common;
using Common.WCF;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;

using Budget2.Server.API.Interface.Faults;

namespace Budget2.Server.Business.Services
{
    public class CovenanteeUploadBuinessService : Budget2DataContextService, ICovenanteeUploadBuinessService
    {
        #region ICovenanteeUploadBuinessService Members

        public BaseFault UploadCovenantee(API.Interface.DataContracts.Covenantee covenantee)
        {
            return UploadCovenantee(new API.Interface.DataContracts.Covenantee[] { covenantee });
        }

        public BaseFault UploadCovenantee(IEnumerable<API.Interface.DataContracts.Covenantee> covenantees)
        {
            Logger.Log.Debug("Старт обновления контрагентов.");

            if (!Common.Settings.Instance.CurrentBudgetVersionId.HasValue)
            {
                Logger.Log.Error(string.Format("Не найдена версия текущего бюджета (Год:{0}).", DateTime.Now.Year));
                return new CovenanteeUploadFault();
            }
           

            Guid budgetVersionId = Common.Settings.Instance.CurrentBudgetVersionId.Value;
            
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    Guid? defaultFilialUid = null;

                    var defaultFilial = context.Filials.FirstOrDefault(p => p.Code == DefaultFilialCode && p.BudgetVersionId == budgetVersionId);
                    if (defaultFilial != null)
                        defaultFilialUid = defaultFilial.Id;
                    
                    var caList = context.Counteragents.Where(ca => ca.BudgetVersionId == budgetVersionId).ToList();

                    var currencyList = context.Currencies.Select(cur => cur).ToList();

                    foreach (var covenantee in covenantees)
                    {
                        Logger.Log.DebugFormat("Обновляем контрагента {0} (Id={1}).", covenantee.Name, covenantee.Id);

                        var currCounteragent = caList.Where(curr => curr.Number == covenantee.Id && !curr.IsPotential).FirstOrDefault();

                        if (currCounteragent != null && currCounteragent.IsProtected)
                            continue;

                        if (currCounteragent == null)
                        {
                            #region Если контрагент не найден, то создаём его в системе

                            currCounteragent = new Counteragent
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       Name = covenantee.Name,
                                                       IsDeleted = false,
                                                       IsProtected = false,
                                                       BudgetVersionId = budgetVersionId,
                                                       FilialId = defaultFilialUid,
                                                       AuthorID = Guid.Empty,
                                                       CreateDate = DateTime.Now
                                                   };
                            context.Counteragents.InsertOnSubmit(currCounteragent);

                            
                            #endregion

                            Logger.Log.DebugFormat("Котрагент {0} добавлен. ID={1}.", currCounteragent.Name, currCounteragent.Id);
                        }
                        currCounteragent.ChangedByID = Guid.Empty;
                        currCounteragent.ChangeDate = DateTime.Now;
                        currCounteragent.Number = covenantee.Id;
                        currCounteragent.Name = covenantee.Name;
                        currCounteragent.INN = covenantee.INN;
                        currCounteragent.KPP = covenantee.KPP;
                        currCounteragent.Address = covenantee.Address;
                        currCounteragent.PersonalAccount = covenantee.Account;
                        currCounteragent.ContactInfo = covenantee.ContactInfo;
                        currCounteragent.Director = covenantee.Director;
                        currCounteragent.BankName = covenantee.BankName;
                        currCounteragent.BankBIC = covenantee.BankBIC;
                        currCounteragent.IdCft = covenantee.IdCft;
                        currCounteragent.IsPotential = false;
                        currCounteragent.CurrencyId = 
                            (from c in currencyList where c.Code == covenantee.Currency && !c.IsDeleted select new Nullable<Guid>(c.Id)).FirstOrDefault();
                        
                        Logger.Log.DebugFormat("Контрагент {0} обновлен (Id={1}).", currCounteragent.Name, currCounteragent.Id);
                    }
                                                            
                    try
                    {
                        Logger.Log.Debug("Сохраняем информацию о контрагентах.");
                        context.SubmitChanges();
                        Logger.Log.Debug("Сохранение контрагентов прошло успешно.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error("Ошибка сохранения в БД.", ex);
                        return new CovenanteeUploadFault();
                    }
                }

                scope.Complete();
            }

            Logger.Log.Debug("Завершение обновления контрагентов.");
            
            return null;
        }

        #endregion

        private string DefaultFilialCode
        {
            get { return ConfigurationManager.AppSettings.Get("DefaultFilialCode"); }
        }
    }
}
