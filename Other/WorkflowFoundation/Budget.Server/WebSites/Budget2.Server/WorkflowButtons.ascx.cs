using System;
using Microsoft.Practices.ObjectBuilder;

namespace Budget2.Server.Shell.Views
{
    public partial class WorkflowButtons : Microsoft.Practices.CompositeWeb.Web.UI.UserControl, IWorkflowButtonsView
    {
        private WorkflowButtonsPresenter _presenter;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this._presenter.OnViewInitialized();
            }
            this._presenter.OnViewLoaded();
        }

        [CreateNew]
        public WorkflowButtonsPresenter Presenter
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

        // TODO: Forward events to the presenter and show state to the user.
        // For examples of this, see the View-Presenter (with Application Controller) QuickStart:
        //	

        public bool SignButtonVisible
        {
            get { return cSign.Visible; }
            set { cSign.Visible = value; }
        }

        public bool DenialButtonVisible
        {
            get { return cDenial.Visible; }
            set { cDenial.Visible = value; }
        }

        public bool DenialByTechnicalCausesButtonVisible
        {
            get { return cDenialByTechnicalCauses.Visible; }
            set { cDenialByTechnicalCauses.Visible = value; }
        }

        public Guid? EntityId
        {
            get; set;
        }

        public Guid? IdentityId
        {
            get; set;
        }

        public event EventHandler<EventArgs> OnSignClick;
        public event EventHandler<EventArgs> OnDenialClick;
        public event EventHandler<EventArgs> OnDenialByTechnicalCausesClick;

        protected void cSign_Click(object sender, EventArgs e)
        {
            if (OnSignClick != null)
                OnSignClick(this, e);
        }

        protected void cDenial_Click(object sender, EventArgs e)
        {
            if (OnDenialClick != null)
                OnDenialClick(this, e);
        }

        protected void cDenialByTechnicalCauses_Click(object sender, EventArgs e)
        {
            if (OnDenialByTechnicalCausesClick != null)
                OnDenialByTechnicalCausesClick(this, e);
        }
    }
}

