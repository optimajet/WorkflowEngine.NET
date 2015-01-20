using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Budget2.Server.Demand
{
    public partial class PrintForm : DevExpress.XtraReports.UI.XtraReport
    {
        public PrintForm()
        {
            InitializeComponent();

            this.DataSourceRowChanged += new DataSourceRowEventHandler(PrintForm_DataSourceRowChanged);
        }

        void PrintForm_DataSourceRowChanged(object sender, DataSourceRowEventArgs e)
        {
            this.DetailReport.FilterString =
            this.DetailReport1.FilterString =
            this.DetailReport2.FilterString =
            this.DetailReport3.FilterString =
                string.Format("DemandId='{0}'", ((DataSet)this.DataSource).Tables["V_Report_Demand"].Rows[e.CurrentRow]["Id"]);
        }

        public void SetDemand(Guid id)
        {
            Budget2.DAL.DataSets.Demand ds = new DAL.DataSets.Demand();
            
            Budget2.DAL.DataSets.DemandTableAdapters.V_Report_DemandTableAdapter bdAdapter = new DAL.DataSets.DemandTableAdapters.V_Report_DemandTableAdapter();
            bdAdapter.Fill(ds.V_Report_Demand, id);
            
            Budget2.DAL.DataSets.DemandTableAdapters.V_Report_DemandMoneyTableAdapter bddAdapter = new DAL.DataSets.DemandTableAdapters.V_Report_DemandMoneyTableAdapter();
            bddAdapter.Fill(ds.V_Report_DemandMoney, id);

            Budget2.DAL.DataSets.DemandTableAdapters.V_Report_DemandAllocationTableAdapter bdaAdapter = new DAL.DataSets.DemandTableAdapters.V_Report_DemandAllocationTableAdapter();
            bdaAdapter.Fill(ds.V_Report_DemandAllocation, id);

            Budget2.DAL.DataSets.DemandTableAdapters.V_Report_DemandTransitionHistoryTableAdapter bdthAdapter = new DAL.DataSets.DemandTableAdapters.V_Report_DemandTransitionHistoryTableAdapter();
            bdthAdapter.Fill(ds.V_Report_DemandTransitionHistory, id);

            Budget2.DAL.DataSets.DemandTableAdapters.V_Report_DemandGoodsTypeTableAdapter bdgtAdapter = new DAL.DataSets.DemandTableAdapters.V_Report_DemandGoodsTypeTableAdapter();
            bdgtAdapter.Fill(ds.V_Report_DemandGoodsType, id);

            this.DataSource = ds;
        }
    }
}
