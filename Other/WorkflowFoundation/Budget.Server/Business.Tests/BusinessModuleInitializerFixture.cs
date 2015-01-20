using System;
using System.Collections.Generic;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.Business.Interface;
using Budget2.Server.Business.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Budget2.Server.Business.Tests
{
    /// <summary>
    /// Summary description for BusinessModuleInitializerFixture
    /// </summary>
    [TestClass]
    public class BusinessModuleInitializerFixture
    {
        public BusinessModuleInitializerFixture()
        {
        }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        #region TestUpdateRates

        [TestMethod]
        public void TestUpdateRates()
        {
            UpdateRatesBuinessService updateRates = new UpdateRatesBuinessService();

            List<CurrencyRate> list = new List<CurrencyRate>();
            CurrencyRate cr = new CurrencyRate();
            cr.ISOCode = "123";
            cr.Name = "USD";
            cr.BaseCurrencyISOCode = "RUR";
            cr.BaseCurrencyName = "Рубль.";
            cr.Measure = 1;
            cr.Rate = 1000M;
            cr.RevaluationDate = DateTime.Today.AddDays(3);
            list.Add(cr);

            cr = new CurrencyRate();
            cr.ISOCode = "USD";
            cr.BaseCurrencyISOCode = "RUR";
            cr.Measure = 1;
            cr.Rate = 70M;
            cr.RevaluationDate = DateTime.Today.AddDays(-9);
            list.Add(cr);

            cr = new CurrencyRate();
            cr.ISOCode = "USD";
            cr.BaseCurrencyISOCode = "RUR";
            cr.Measure = 1;
            cr.Rate = 80M;
            cr.RevaluationDate = DateTime.Today;
            list.Add(cr);

            cr = new CurrencyRate();
            cr.ISOCode = "EUR";
            cr.BaseCurrencyISOCode = "RUR";
            cr.Measure = 1;
            cr.Rate = 60M;
            cr.RevaluationDate = DateTime.Today.AddDays(1);
            list.Add(cr);

            cr = new CurrencyRate();
            cr.ISOCode = "EUR";
            cr.BaseCurrencyISOCode = "RUR";
            cr.Measure = 1;
            cr.Rate = 1M;
            cr.RevaluationDate = DateTime.Today.AddDays(3);
            list.Add(cr);

            cr = new CurrencyRate();
            cr.ISOCode = "EUR";
            cr.BaseCurrencyISOCode = "RUR";
            cr.Measure = 1;
            cr.Rate = 80M;
            cr.RevaluationDate = DateTime.Today;
            list.Add(cr);

            var result = updateRates.UpdateRates(list);
            Assert.IsNull(result);
        }

        #endregion

        #region TestCovenanteeUpload

        [TestMethod]
        public void TestCovenanteeUpload()
        {
            CovenanteeUploadBuinessService uploadCovenantee = new CovenanteeUploadBuinessService();

            List<Covenantee> list = new List<Covenantee>();
            Covenantee c = new Covenantee();
            c.Account = "account";
            c.Address = "г. Москва, ул. Широкая, д.6";
            c.BankBIC = "";
            c.BankName = "Банк Москвы";
            c.ContactInfo = "Иванов Иван Иванович, Менеджер";
            c.Currency = "USD";
            c.Director = "Иванов Иван Иванович";
            c.Id = 120;
            c.INN = "1234567891";
            c.KPP = "23456";
            c.Name = "ООО Рога и копыта";
            c.IdCft = 66613;
            list.Add(c);

            var result = uploadCovenantee.UploadCovenantee(list);
            Assert.IsNull(result);
        }

        #endregion

        #region TestContractUpload

        [TestMethod]
        public void TestContractUpload()
        {
            ContractUploadBuinessService uploadContract = new ContractUploadBuinessService();

            List<Contract> list = new List<Contract>();
            Contract c = new Contract();
            c.Amount = 100;
            c.Comment = "UR1";
            //c.CompletionDate = DateTime.Now;
            c.Currency = "USD";
            c.CustomerDetails = "Новое ООО Денис";
            c.CustomerId = 0;
            c.RateCBRPercents = null;
            c.Rate = 1;
            c.Date = DateTime.Now;
            c.DateFrom = DateTime.Now.AddDays(-10);
            c.DateTo = DateTime.Now.AddDays(10);
            c.ExecutorDetails = "ExecutorDetails";
            c.ExecutorId = 0;
            c.Id = 24;
            c.Manager = "test";
            c.Number = 212042011;
            c.PaymentTerms = "Условия оплаты";
            c.Prefix = "GH";
            Contract sc1 = new Contract();
            sc1.Amount = 5;
            sc1.Comment = "UR1_sub";
            sc1.CompletionDate = DateTime.Now;
            sc1.Currency = "USD";
            sc1.CustomerDetails = "Новое ООО Денис";
            sc1.CustomerId = 0;
            sc1.Date = DateTime.Now;
            sc1.DateFrom = DateTime.Now.AddDays(-10);
            sc1.DateTo = DateTime.Now.AddDays(10);
            sc1.ExecutorDetails = "ExecutorDetails";
            sc1.ExecutorId = 0;
            sc1.Id = 25;
            sc1.Manager = "test";
            sc1.Number = 212042011;
            sc1.PaymentTerms = "Условия оплаты";
            sc1.Prefix = "GH";
            c.Subcontracts = new List<Contract>() {sc1};
            var pi1 = new PaymentPlanItem()
                          {
                              Amount = 10,
                              Comment = "PI1-New",
                              Date = DateTime.Now,
                              DateFrom = DateTime.Now,
                              DateTo = DateTime.Now,
                              FromEmployee = 0,
                              ToEmployee = 0,
                              Id = 10050217,
                              Number = 100,
                              PaymentTerms = "PT1-000",
                              Prefix = "P1",
                              FactPay = true,
                              DogId = 02042012
                          };
            var pi2 = new PaymentPlanItem()
                          {
                              Amount = 12,
                              Comment = "PI2",
                              Date = DateTime.Now,
                              DateFrom = DateTime.Now,
                              DateTo = DateTime.Now,
                              FromEmployee = 0,
                              ToEmployee = 0,
                              Id = 1005022,
                              Number = 102,
                              PaymentTerms = "PT2-000",
                              Prefix = "P2",
                              FactPay = true,
                               DogId = 02042012
                          };
            var spi1 = new PaymentPlanItem()
            {
                Amount = 1000,
                Comment = "spi1",
                Date = DateTime.Now,
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now,
                FromEmployee = 0,
                ToEmployee = 0,
                Id = 123,
                Number = 100,
                PaymentTerms = "PT1-000",
                Prefix = "P1",
                FactPay = true,
                DogId = 02042011
            };
            var spi2 = new PaymentPlanItem()
            {
                Amount = 1000,
                Comment = "spi2",
                Date = DateTime.Now,
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now,
                FromEmployee = 0,
                ToEmployee = 0,
                Id = 124,
                Number = 102,
                PaymentTerms = "PT2-000",
                Prefix = "P2",
                FactPay = false,
                 DogId = 02042011
            };
            c.PaymentPlan = new List<PaymentPlanItem>() {pi1, pi2,spi1,spi2};
            //sc1.PaymentPlan = new List<PaymentPlanItem>() { spi1, spi2 };
            list.Add(c);

            var result = uploadContract.UploadContract(list);

            Assert.IsNull(result);
        }

        #endregion

        [TestMethod]
        public void TestExternalServicesInitialization()
        {
            var billDemandExportService = new BillDemandExportService();
            try
            {
                billDemandExportService.GetBillDemandExternalStaus(Guid.NewGuid());
            }
            catch (Exception ex)
            {
            }

            try
            {                
                billDemandExportService.ExportBillDemand(new BillDemandForExport() {Id = Guid.NewGuid()});
            }
            catch (Exception ex)
            {
            }
        }

        [TestMethod]
        public void TestExternalServicesBillDemandBusinessService()
        {
            var serv = new BillDemandBusinessService();
            serv.GetBillDemandForExport(new Guid("DBCAC0BF-0B7F-4C3D-A17E-FCD8A2A74427"));            
            
        }
    }
}