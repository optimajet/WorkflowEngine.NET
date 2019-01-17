using System.Threading.Tasks;
using AngularBPWorkflow.Configuration.Dto;

namespace AngularBPWorkflow.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
