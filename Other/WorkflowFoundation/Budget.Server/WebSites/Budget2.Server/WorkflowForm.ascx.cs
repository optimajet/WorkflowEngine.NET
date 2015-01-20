using System;
using Common.Utils;
using DevExpress.XtraReports.UI;
using Microsoft.Practices.ObjectBuilder;

namespace Budget2.Server.Shell.Views
{
    public partial class WorkflowForm : Microsoft.Practices.CompositeWeb.Web.UI.UserControl, IWorkflowFormView
    {
        private WorkflowFormPresenter _presenter;

        protected void Page_Init(object sender, EventArgs e)
        {
            WorkflowButtons.OnDenialByTechnicalCausesClick += new EventHandler<EventArgs>(WorkflowButtons_OnDenialByTechnicalCausesClick);
            WorkflowButtons.OnDenialClick += new EventHandler<EventArgs>(WorkflowButtons_OnDenialClick);
            WorkflowButtons.OnSignClick += new EventHandler<EventArgs>(WorkflowButtons_OnSignClick);

            WorkflowButtonsUp.OnDenialByTechnicalCausesClick += new EventHandler<EventArgs>(WorkflowButtons_OnDenialByTechnicalCausesClick);
            WorkflowButtonsUp.OnDenialClick += new EventHandler<EventArgs>(WorkflowButtons_OnDenialClick);
            WorkflowButtonsUp.OnSignClick += new EventHandler<EventArgs>(WorkflowButtons_OnSignClick);

        }

        void WorkflowButtons_OnSignClick(object sender, EventArgs e)
        {
            Presenter.RaiseSign();
        }

        void WorkflowButtons_OnDenialClick(object sender, EventArgs e)
        {
            Presenter.RaiseDenial();
        }

        void WorkflowButtons_OnDenialByTechnicalCausesClick(object sender, EventArgs e)
        {
            Presenter.RaiseDenialByTechnicalCauses();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this._presenter.OnViewInitialized();
            }
            this._presenter.OnViewLoaded();
        }

        [CreateNew]
        public WorkflowFormPresenter Presenter
        {
            get
            {
                return this._presenter;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                this._presenter = value;
                this._presenter.View = this;
            }
        }

        private Guid? _entityId;

        public Guid? EntityId
        {

            set
            {
                _entityId = value;
                WorkflowButtons.EntityId = value;
                WorkflowButtonsUp.EntityId = value;
            }
            get
            {
                return _entityId;
            }
        }

        private Guid? _identityId;

        public void ShowPrintForm(Guid entityId)
        {
            if (GetForm == null)
                return;
            ReportViewer1.Report = GetForm.Invoke(entityId);
        }

        public Guid? IdentityId
        {

            set
            {
                _identityId = value;
                WorkflowButtons.IdentityId = value;
                WorkflowButtonsUp.IdentityId = value;
            }
            get
            {
                return _identityId;
            }
        }

        public bool ISAccessGranted
        {
            set { mvMain.SetActiveView(value ? vAccessGranted : vAccessDenied); }
        }

        public Guid? TicketId
        {
            get
            {
                return StringToNullableConverter.GetGuid(Request["tid"]);
            }
        }

        public void ShowError(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                lbError.Visible = false;
            }
            else
            {
                lbError.Visible = true;
                lbError.Text = string.Format("Ошибка.{0}", error);
            }
        }

        public string Comment
        {
            get { return tbComment.Text; }
            set { tbComment.Text = value; }
        }

        public void ShowCompleted()
        {
            mvMain.SetActiveView(vComplete);
        }

        public void ShowUsed()
        {
            mvMain.SetActiveView(vUsed);
        }

        public Func<Guid,XtraReport> GetForm
        {
            get; set;
        }

    }
}

