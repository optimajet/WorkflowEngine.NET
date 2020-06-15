using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using AngularBPWorkflow.Configuration.Dto;

namespace AngularBPWorkflow.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : AngularBPWorkflowAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
