// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if FEATURE_STAGING
namespace Midl.Analysis;

/// <summary>
/// Feature staging level, maps to Windows.Foundation.Metadata.FeatureStage.
/// </summary>
public enum FeatureStage
{
    AlwaysDisabled = 0,
    DisabledByDefault = 1,
    EnabledByDefault = 2,
    AlwaysEnabled = 3
}

/// <summary>
/// Parsed feature_name declaration: <c>feature_name Name = {Stage, ValidInAllBranches};</c>
/// </summary>
public record FeatureNameInfo(string Name, FeatureStage Stage, bool ValidInAllBranches);
#endif
