using System;
using System.Collections.Generic;
using System.Web.UI;
using WF.Sample.Business.DataAccess;
using WF.Sample.Models;

namespace WF.Sample.Helpers
{
    public abstract partial class DocumentsPage<TDoc>
    {

        /// <summary>
        /// DocsTableRepeater control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify, move the field declaration from the designer file to a code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Repeater DocsTableRepeater;
        
        /// <summary>
        /// Master property.
        /// </summary>
        /// <remarks>
        /// Auto-generated property.
        /// </remarks>
        public new WF.Sample.SiteMaster Master {
            get {
                return ((WF.Sample.SiteMaster)(base.Master));
            }
        }
    }
}
