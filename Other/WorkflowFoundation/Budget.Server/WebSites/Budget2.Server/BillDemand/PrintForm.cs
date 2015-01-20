using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Budget2.Server.BillDemand
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
                string.Format("BillDemandId='{0}'", ((DataSet)this.DataSource).Tables["V_Report_BillDemand"].Rows[e.CurrentRow]["Id"]);
        }

        public void SetBillDemand(Guid id)
        {
            Budget2.DAL.DataSets.BillDemand ds = new DAL.DataSets.BillDemand();
            
            Budget2.DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandTableAdapter bdAdapter = new DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandTableAdapter();
            bdAdapter.Fill(ds.V_Report_BillDemand, id);
            
            Budget2.DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandDistributionTableAdapter bddAdapter = new DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandDistributionTableAdapter();
            bddAdapter.Fill(ds.V_Report_BillDemandDistribution, id);

            Budget2.DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandAllocationTableAdapter bdaAdapter = new DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandAllocationTableAdapter();
            bdaAdapter.Fill(ds.V_Report_BillDemandAllocation, id);

            Budget2.DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandTransitionHistoryTableAdapter bdthAdapter = new DAL.DataSets.BillDemandTableAdapters.V_Report_BillDemandTransitionHistoryTableAdapter();
            bdthAdapter.Fill(ds.V_Report_BillDemandTransitionHistory, id);

            this.DataSource = ds;
        }
    }
}
