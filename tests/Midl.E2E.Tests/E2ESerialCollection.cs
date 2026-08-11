// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.E2E.Tests;

/// <summary>
/// Forces the end-to-end tests to run one at a time.
///
/// Each of them registers COM classes and interface proxy/stub mappings under
/// <c>HKCU\Software\Classes</c>, builds native binaries into a fixed directory in the repo,
/// and starts server processes — all machine-global state. xUnit puts every test class in
/// its own collection and runs collections in parallel, so by default all three started
/// within milliseconds of each other and raced.
///
/// The symptom was a CI failure in which five of fifteen classes failed
/// <c>CoCreateInstance</c> while the rest passed, and which did not reproduce on re-run of
/// the same commit. Measuring start and end times showed the three tests overlapping
/// completely; on a fast machine they usually get away with it, on a two-core CI runner they
/// do not.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class E2ESerialCollection
{
    public const string Name = "E2E (serial)";
}
