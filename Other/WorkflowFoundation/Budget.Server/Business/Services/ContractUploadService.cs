using System;
using System.Configuration;
using System.Linq;
using Budget2.DAL;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;
using System.Collections.Generic;
using Common;
using Common.Utils;
using Common.WCF;
using Budget2.Server.API.Interface.Faults;
using System.Transactions;
using Contract = Budget2.DAL.Contract;

namespace Budget2.Server.Business.Services
{
    public class ContractUploadBuinessService : Budget2DataContextService, IContractUploadBuinessService
    {
        private DictionaryCache<Guid, Guid?> _defaultFilialCache;

        public ContractUploadBuinessService ()
        {
            _defaultFilialCache = new DictionaryCache<Guid, Guid?>(new TimeSpan(1, 0, 0), FillDefaultFilials);
        }

        private Dictionary<Guid, Guid?> FillDefaultFilials()
        {
            var ret = new Dictionary<Guid, Guid?>();
            using (var context = CreateContext())
            {
                var defaultFilials = context.Filials.Where(p => p.Code == DefaultFilialCode);

                foreach (var filial in defaultFilials)
                {
                    try
                    {
                        ret.Add(filial.BudgetVersionId, filial.Id);
                    }
                    catch (ArgumentException){}
                }

            }

            return ret;
        }

        private Guid? DefaultFilialKey
        {
            get {
                if (Settings.Instance.CurrentBudgetVersionId.HasValue)
                {
                    var fk = _defaultFilialCache.GetDictionary().Where(
                        p => p.Key == Settings.Instance.CurrentBudgetVersionId.Value).ToList();
                    return fk.Count > 0 ? fk[0].Value : null;
                }
                else
                    return null;
            }
        }

        #region IContractUploadBuinessService Members

        public BaseFault UploadContract(API.Interface.DataContracts.Contract contract)
        {
            return UploadContract(new API.Interface.DataContracts.Contract[] { contract });
        }

        public BaseFault UploadContract(IEnumerable<API.Interface.DataContracts.Contract> contracts)
        {
            Logger.Log.Debug("Старт обновления контрактов.");

            if (!Common.Settings.Instance.CurrentBudgetVersionId.HasValue)
            {
                Logger.Log.Error(string.Format("Не найдена версия текущего бюджета (Год:{0}).", DateTime.Now.Year));
                return new ContractUploadFault();
            }

            Guid budgetVersionId = Common.Settings.Instance.CurrentBudgetVersionId.Value;

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = CreateContext())
                {
                    var caList = (from ca in context.Contracts where ca.BudgetVersionId == budgetVersionId select ca).ToList();
                    var currencyList = (from cur in context.Currencies select cur).ToList();

                    foreach (var contract in contracts)
                    {
                        try
                        {
                            Logger.Log.DebugFormat("Обновляем контракт №{0} {1} (Id={2}).", contract.Prefix, contract.Number, contract.Id);

                            var parentContractId = CreateContract(context, budgetVersionId, contract, caList, currencyList, null);

                            if (parentContractId == null)
                                continue;

                            if (contract.Subcontracts != null)
                                UploadSubcontracts(context, budgetVersionId, contract, caList, currencyList, parentContractId.Value);

                            if (contract.PaymentPlan != null)
                                UploadPaymentPlan(context, budgetVersionId, contract, parentContractId.Value, contract.PaymentPlan.Where(ppi => ppi.DogId == contract.Id));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error(string.Format("Ошибка обновления контракта №{0} {1} (Id={2}).", contract.Prefix, contract.Number, contract.Id), ex);
                            ContractUploadFault fail = new ContractUploadFault();
                            fail.ErrorCode = 301;
                            fail.Description = ex.Message;
                            return fail;
                        }
                    }

                    try
                    {
                        Logger.Log.Debug("Сохраняем информацию о контрактах.");
                        context.SubmitChanges();
                        Logger.Log.Debug("Сохранение контрактов прошло успешно.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error("Ошибка сохранения в БД.", ex);
                        ContractUploadFault fail = new ContractUploadFault();
                        fail.ErrorCode = 301;
                        fail.Description = ex.Message;
                        return fail;
                    }
                }

                scope.Complete();
            }

            Logger.Log.Debug("Завершение обновления контрактов.");

            return null;
        }

        private void UploadPaymentPlan(Budget2DataContext context, Guid budgetVersionId, API.Interface.DataContracts.Contract contract, Guid parentContractId, IEnumerable<PaymentPlanItem> paymentPlan)
        {
            var contractMoneyList =
                context.ContractMoneys.Where(p => p.ContractId == parentContractId).ToList();

            context.ContractMoneys.DeleteAllOnSubmit(contractMoneyList);

            if (paymentPlan != null)
                SavePaymentPlan(context, budgetVersionId, paymentPlan, parentContractId);
        }

        private void UploadSubcontracts(Budget2DataContext context, Guid budgetVersionId, API.Interface.DataContracts.Contract contract, List<Contract> caList, List<Currency> currencyList, Guid parentContractId)
        {
            foreach (var subcontract in contract.Subcontracts)
            {
                Logger.Log.DebugFormat("Обновляем допсоглашение №{0} {1} (Id={2}).", subcontract.Prefix,
                                       subcontract.Number, subcontract.Id);
                var parentContractId1 = CreateContract(context, budgetVersionId, subcontract, caList, currencyList,
                                                       parentContractId);
                if (!parentContractId1.HasValue)
                    continue;
                UploadPaymentPlan(context, budgetVersionId, subcontract, parentContractId1.Value, contract.PaymentPlan.Where(ppi => ppi.DogId == subcontract.Id));
            }
        }

        private void SavePaymentPlan(Budget2DataContext context, Guid budgetVersionId, IEnumerable<PaymentPlanItem> paymentPlanItems, Guid contractId)
        {
            foreach (var paymentPlanItem in paymentPlanItems)
            {

                var currPaymentPlanItem = new ContractMoney()
                                              {
                                                  Id = Guid.NewGuid(),
                                                  Number = paymentPlanItem.Prefix,
                                                  NumberId = paymentPlanItem.Number,
                                                  ContractId = contractId,
                                                  ExternalId = paymentPlanItem.Id
                                              };

                context.ContractMoneys.InsertOnSubmit(currPaymentPlanItem);



                if (paymentPlanItem.Date == DateTime.MinValue)
                    throw new Exception(string.Format("Значение поля PaymentPlanItem.Date ({0}) некорректно.", paymentPlanItem.Date));

                currPaymentPlanItem.Date = paymentPlanItem.Date;
                currPaymentPlanItem.MonthId = (byte)paymentPlanItem.Date.Month;
                currPaymentPlanItem.Year = paymentPlanItem.Date.Year;
                currPaymentPlanItem.DateFrom = paymentPlanItem.DateFrom;
                currPaymentPlanItem.DateTo = paymentPlanItem.DateTo;
                currPaymentPlanItem.Comment = paymentPlanItem.Comment;
                currPaymentPlanItem.ConditionPayment = paymentPlanItem.PaymentTerms;
                currPaymentPlanItem.Sum = paymentPlanItem.Amount;
                currPaymentPlanItem.ExternalFromEmployeeId = paymentPlanItem.FromEmployee;
                currPaymentPlanItem.ExternaToEmployeeId = paymentPlanItem.ToEmployee;
                currPaymentPlanItem.PaidState = paymentPlanItem.FactPay;

                currPaymentPlanItem.FromCounteragentId =
                        (from c in context.Counteragents
                         where c.Number == paymentPlanItem.FromEmployee && c.BudgetVersionId == budgetVersionId && !c.IsDeleted && !c.IsPotential
                         select new Nullable<Guid>(c.Id)).FirstOrDefault();

                currPaymentPlanItem.ToCounteragentId =
                       (from c in context.Counteragents
                        where c.Number == paymentPlanItem.ToEmployee && c.BudgetVersionId == budgetVersionId && !c.IsDeleted && !c.IsPotential
                        select new Nullable<Guid>(c.Id)).FirstOrDefault();

                Logger.Log.DebugFormat("План платежей {0} обновлен (Id={1}).", currPaymentPlanItem.NumberId, currPaymentPlanItem.Id);
            }
        }

        private Guid? CreateContract(Budget2DataContext context, Guid budgetVersionId, API.Interface.DataContracts.Contract contract, List<Contract> caList, List<Currency> currencyList, Guid? parentContractUid)
        {
            var currContract =
                (from curr in caList where curr.ExternalId == contract.Id && (!parentContractUid.HasValue || curr.ParentId == parentContractUid.Value) select curr).FirstOrDefault();

            if (currContract != null && currContract.IsProtected)
                return null;

            if (currContract == null)
            {
                #region Если контрагент не найден, то создаём его в системе

                currContract = new Contract
                                   {
                                       Id = Guid.NewGuid(),
                                       Number = contract.Prefix,
                                       NumberId = contract.Number,
                                       IsDeleted = false,
                                       BudgetVersionId = budgetVersionId,
                                       FilialId = DefaultFilialKey,
                                       AuthorId = Guid.Empty,
                                       CreateDate = DateTime.Now
                                       
                                   };
                context.Contracts.InsertOnSubmit(currContract);

                #endregion

                Logger.Log.DebugFormat("Контракт {0} {1} добавлен. ID={2}.", currContract.Number, currContract.NumberId, currContract.Id);
            }
            currContract.ChangedByID = Guid.Empty;
            currContract.ChangeDate = DateTime.Now;
            currContract.Number = contract.Prefix;
            currContract.NumberId = contract.Number;
            currContract.Date = contract.Date;
            currContract.DateFrom = contract.DateFrom;
            currContract.DateTo = contract.DateTo;
            currContract.DistributionDate = contract.CompletionDate;
            currContract.Subject = contract.Comment;
            currContract.CurrencySum = contract.Amount;
            currContract.ConditionPayment = contract.PaymentTerms;
            currContract.CurrencyRateCBPercents = (double?)(contract.RateCBRPercents.HasValue ? contract.RateCBRPercents.Value / 100 : (decimal?)null);
            currContract.CurrencyRate = (double?)contract.Rate;
            currContract.StatusId = (byte)0;
            currContract.ExternalId = contract.Id;
            currContract.ExternalCustomerId = contract.CustomerId;
            currContract.ExternalExecutorId = contract.ExecutorId;
            currContract.ExternalCustomerDetails = contract.CustomerDetails;
            currContract.ExternalExecutorDetails = contract.ExecutorDetails;
            currContract.ExternalManager = contract.Manager;
            if (parentContractUid.HasValue)
                currContract.ParentId = parentContractUid.Value;

            currContract.ExecutorCounteragentId =
                (from c in context.Counteragents
                 where c.Number == contract.ExecutorId && c.BudgetVersionId == budgetVersionId && !c.IsDeleted && !c.IsPotential
                 select new Nullable<Guid>(c.Id)).FirstOrDefault();

            currContract.CustomerCounteragentId =
                (from c in context.Counteragents
                 where c.Number == contract.CustomerId && c.BudgetVersionId == budgetVersionId && !c.IsDeleted && !c.IsPotential
                 select new Nullable<Guid>(c.Id)).FirstOrDefault();

            currContract.CurrencyId =
                (from c in currencyList
                 where c.Code == contract.Currency && !c.IsDeleted
                 select new Nullable<Guid>(c.Id)).FirstOrDefault();

            Logger.Log.DebugFormat("Контракт {0} обновлен (Id={1}).", currContract.Name, currContract.Id);

            return currContract.Id;
        }

        #endregion

        private string DefaultFilialCode
        {
            get { return ConfigurationManager.AppSettings.Get("DefaultFilialCode"); }
        }

    }

}