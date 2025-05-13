using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.Rules; // For CopyRule, CombineFieldsRule, IgnoreRule
using Microsoft.Extensions.Logging.Abstractions; // Added for NullLogger
using System.Collections.Generic;
using System.Linq;
using System;

namespace AutomatedRealms.DataImportUtility.Tests.SampleData;

internal static partial class ImportDataObjects
{
    private static List<MappingRuleBase>? _mappingRules;
    /// <summary>
    /// The list of mapping rules to use for testing.
    /// </summary>
    internal static List<MappingRuleBase> MappingRules
    {
        get
        {
            PrepareMappingRuleInstances();
            return _mappingRules ?? throw new InvalidOperationException("Mapping rules failed to initialize. Please check the test setup.");
        }
    }

    /// <summary>
    /// The copy rule instance to use for testing.
    /// </summary>
    internal static CopyRule CopyRule => (CopyRule)MappingRules.OfType<CopyRule>().First().Clone();
    /// <summary>
    /// The combine fields rule instance to use for testing.
    /// </summary>
    internal static CombineFieldsRule CombineFieldsRule => (CombineFieldsRule)MappingRules.OfType<CombineFieldsRule>().First().Clone();

    private static readonly object _mappingRulePreparationLock = new();

    private static void PrepareMappingRuleInstances()
    {
        lock (_mappingRulePreparationLock)
        {
            if (_mappingRules is { Count: >0 }) { return; }

            // Temporarily manually define rule types for testing as ApplicationConstants might be internal
            _mappingRules = 
            [
                new CopyRule(),
                new CombineFieldsRule(NullLogger<CombineFieldsRule>.Instance),
                new IgnoreRule()
                // Add other specific rule types here if needed for tests
            ];

            // Initialize properties for these specific instances if needed for general tests
            // The specific properties like SourceField for CopyRule or InputFields for CombineFieldsRule
            // are often set per-test case. This section will set up some defaults.

            foreach (var rule in _mappingRules)
            {
                switch (rule)
                {
                    case CopyRule copyRule:
                        // Set a default source field for the general CopyRule instance
                        copyRule.SourceField = GetImportField(0).FieldName;
                        // copyRule.SourceValueTransformations.Clear(); // Ensure no prior transformations if this instance is reused
                        break;
                    case CombineFieldsRule combineFieldsRule:
                        // Set a default combination format and input fields
                        combineFieldsRule.CombinationFormat = "${0}-${1}";
                        combineFieldsRule.InputFields.Clear();
                        combineFieldsRule.InputFields.Add(new ConfiguredInputField { FieldName = GetImportField(0).FieldName });
                        combineFieldsRule.InputFields.Add(new ConfiguredInputField { FieldName = GetImportField(1).FieldName });
                        break;
                    case IgnoreRule _:
                        // Nothing to configure for IgnoreRule by default
                        break;
                }
            }
        }
    }
}
