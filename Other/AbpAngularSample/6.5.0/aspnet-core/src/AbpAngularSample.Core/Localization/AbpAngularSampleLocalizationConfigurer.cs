using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace AbpAngularSample.Localization
{
    public static class AbpAngularSampleLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(AbpAngularSampleConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(AbpAngularSampleLocalizationConfigurer).GetAssembly(),
                        "AbpAngularSample.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
