using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Runtime
{
    public enum SuggestionCategory
    {
        RuleParameter, ActionParameter, ConditionParameter
    }

    public interface IDesignerAutocompleteProvider
    {
        List<string> GetAutocompleteSuggestions(SuggestionCategory category, string value, string schemeCode);
    }
}
