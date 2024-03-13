using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.Model;

namespace WF.Sample.Business.DataAccess
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
    }
}
