using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace AngularBPWorkflow.Localization
{
    public static class AngularBPWorkflowLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(AngularBPWorkflowConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(AngularBPWorkflowLocalizationConfigurer).GetAssembly(),
                        "AngularBPWorkflow.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
