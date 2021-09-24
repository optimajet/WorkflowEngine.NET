using System.Threading.Tasks;
using AbpAngularSample.Configuration.Dto;

namespace AbpAngularSample.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
