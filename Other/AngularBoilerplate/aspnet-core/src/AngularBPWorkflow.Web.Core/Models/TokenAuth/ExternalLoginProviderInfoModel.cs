using Abp.AutoMapper;
using AngularBPWorkflow.Authentication.External;

namespace AngularBPWorkflow.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }
    }
}
