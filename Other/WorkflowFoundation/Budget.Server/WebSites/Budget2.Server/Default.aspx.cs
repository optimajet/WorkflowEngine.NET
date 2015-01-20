using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Budget2.Server.Shell.Views;
using Budget2.Server.Shell.Views.BillDemand;
using Microsoft.Practices.ObjectBuilder;

public partial class ShellDefault : Microsoft.Practices.CompositeWeb.Web.UI.Page, IDefaultView
{
    private DefaultViewPresenter _presenter;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            this._presenter.OnViewInitialized();
        }
        this._presenter.OnViewLoaded();
    }

    [CreateNew]
    public DefaultViewPresenter Presenter
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
}
