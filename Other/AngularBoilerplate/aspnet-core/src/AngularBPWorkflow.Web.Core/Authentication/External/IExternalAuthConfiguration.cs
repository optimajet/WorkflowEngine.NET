using System.Collections.Generic;

namespace AngularBPWorkflow.Authentication.External
{
    public interface IExternalAuthConfiguration
    {
        List<ExternalLoginProviderInfo> Providers { get; }
    }
}
