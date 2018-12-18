using System.Threading.Tasks;
using Workflow.Configuration.Dto;

namespace Workflow.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
