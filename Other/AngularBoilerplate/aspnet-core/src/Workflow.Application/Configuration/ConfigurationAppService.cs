using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Workflow.Configuration.Dto;

namespace Workflow.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : WorkflowAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
