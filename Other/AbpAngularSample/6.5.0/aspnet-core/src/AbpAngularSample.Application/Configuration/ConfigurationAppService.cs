using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using AbpAngularSample.Configuration.Dto;

namespace AbpAngularSample.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : AbpAngularSampleAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
