using Abp.AutoMapper;
using AbpAngularSample.Authentication.External;

namespace AbpAngularSample.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }
    }
}
