using Budget2.Server.Business.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Budget2.DAL.DataContracts;

namespace Budget2.Server.Business.Tests
{
    
    
    /// <summary>
    ///This is a test class for BillDemandBuinessServiceTest and is intended
    ///to contain all BillDemandBuinessServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BillDemandBuinessServiceTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void MSDTCActiveTest()
        {
           

        }

        /// <summary>
        ///A test for GetBillDemandValue
        ///</summary>
        [TestMethod()]
        public void GetBillDemandValueTest()
        {
            BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            Decimal expected = new Decimal(); // TODO: Initialize to an appropriate value
            Decimal actual;
            actual = target.GetBillDemandValue(billDemandUid);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LimitExecutorResetSights
        ///</summary>
        [TestMethod()]
        public void LimitExecutorResetSightsTest()
        {
            BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            target.LimitExecutorResetSights(billDemandUid);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for LimitExecutorSight
        ///</summary>
        [TestMethod()]
        public void LimitExecutorSightTest()
        {
            //BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            //Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            //Guid initiatorId = new Guid(); // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            //bool actual;
            //actual = target.LimitExecutorSight(billDemandUid, initiatorId);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LimitManagerResetSights
        ///</summary>
        [TestMethod()]
        public void LimitManagerResetSightsTest()
        {
            BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            target.LimitManagerResetSights(billDemandUid);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for LimitManagerSight
        ///</summary>
        [TestMethod()]
        public void LimitManagerSightTest()
        {
            //BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            //Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            //Guid initiatorId = new Guid(); // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            //bool actual;
            //actual = target.LimitManagerSight(billDemandUid, initiatorId);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UpdateBillDemandState
        ///</summary>
        [TestMethod()]
        public void UpdateBillDemandStateTest()
        {
            BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            WorkflowState state = null; // TODO: Initialize to an appropriate value
            Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            target.UpdateBillDemandState(state, billDemandUid);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UpdateBillDemandState
        ///</summary>
        [TestMethod()]
        public void UpdateBillDemandStateTest1()
        {
            BillDemandBusinessService target = new BillDemandBusinessService(); // TODO: Initialize to an appropriate value
            WorkflowState initialState = null; // TODO: Initialize to an appropriate value
            WorkflowState destinationState = null; // TODO: Initialize to an appropriate value
            Guid billDemandUid = new Guid(); // TODO: Initialize to an appropriate value
            Guid initiatorId = new Guid(); // TODO: Initialize to an appropriate value
            string comment = string.Empty; // TODO: Initialize to an appropriate value
            //target.UpdateBillDemandState(initialState, destinationState, billDemandUid, initiatorId, comment);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
