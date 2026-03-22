# WinMIDL Language Extensions

> **Status**: Research & ideation. Not yet approved for implementation.
> 
> These are language features that go beyond what original `midl.exe` supports.
> All produce valid WinRT metadata — none require runtime changes.

## Triage Summary

Reviewed with the assumption these ship behind `/ext` (opt-in) and emit standard WinRT metadata.

| # | Proposal | Verdict | Rationale |
|---|----------|---------|-----------|
| 1 | Nullable `T?` | 🟢 **Do it** | Pure sugar, zero ABI surprise, every C# dev expects this |
| 2 | Namespace aliases `using` | 🟡 **Maybe** | **Split this.** Qualified aliases (`using X = Y`) are 🟢. Wildcard imports (`using Windows.Foundation.Collections;`) are 🟡 — name collisions and readability risk |
| 3 | Async method sugar | 🔴 **Don't** | Method name and return type in IDL won't match the WinMD — debugging nightmare |
| 4 | Enum underlying type | 🟢 **Do it** | Trivial, makes intent explicit, no downside |
| 5 | Doc comments → WinMD | 🟡 **Maybe** | Valuable but needs ecosystem buy-in on attribute format; LSP tooltips alone are 🟢 |
| 6 | Default parameters → overloads | 🔴 **Don't** | Auto-generated overload names become ABI; renaming a parameter is a breaking change |
| 7 | Inline delegate types | 🟡 **Maybe** | Convenient but auto-generated delegate names become permanent public ABI surface |
| 8 | Partial classes across files | 🟢 **Do it** | Well-understood pattern, clear value for large APIs |
| 9 | Tuple return sugar | 🔴 **Don't** | Looks like tuples, emits out-params — misleading to every C#/Rust developer who reads it |
| 10 | Observable property sugar | 🔴 **Don't** | Only helps XAML, creates hard framework dependency, sugar gives metadata but not behavior |
| 11 | User-defined parameterized interfaces | 🟡 **Maybe** | Real gap in MIDL3, but complex (GUID computation, projection support) — high reward, high cost |
| 12 | Constructor forwarding | 🟡 **Maybe** | Niche; versioning hazard if base class adds constructors in a later contract |
| 13 | Conditional compilation | 🟡 **Maybe** | Reasonable need in strict MIDL3 mode, but `[if()]` masquerading as an attribute is confusing |
| 14 | Interface default implementations | 🔴 **Don't** | Actively misleading — WinRT can't dispatch these, the "default" is a lie |
| 15 | Struct companion methods | 🟡 **Maybe** | Follows existing Windows pattern (RectHelper), but projections need to agree on discoverability |

**Score: 3 clear wins, 7 worth exploring, 5 reject.**

The three green-light features (nullable, enum types, partial classes) share a common trait: the IDL still reads like what actually ends up in the WinMD. The five red-light features all share a different trait: they make the IDL say one thing while the WinMD contains something visibly different.

---

## 1. Nullable value types: `T?`

**Problem**: WinRT represents nullable value types as `IReference<T>`, but you must write the verbose `Windows.Foundation.IReference<Int32>`.

**Proposed syntax**:
```idl
runtimeclass Settings
{
    Int32? MaxRetries { get; set; };      // nullable Int32
    Double? Opacity { get; };             // nullable Double
    Boolean? IsEnabled { get; set; };     // nullable Boolean
    
    void Configure(Int32? timeout, Double? scale);
};
```

**Desugars to**:
```idl
Windows.Foundation.IReference<Int32> MaxRetries { get; set; };
Windows.Foundation.IReference<Double> Opacity { get; };
// etc.
```

**Scope**: Properties, method parameters, return types. Not valid for struct fields (IReference is a reference type).

**Open questions**:
- Should `String?` be valid? String is already a reference type in WinRT (always nullable). Could warn or just allow it as a no-op.
- Should `Guid?` work? Guid is a value type in WinRT.

**Critique:** 🟢 This is the single best proposal in this document. The desugaring is trivial and lossless — anyone inspecting the WinMD sees `IReference<Int32>` and immediately understands the intent. Every C# and Rust developer already expects `?` to mean nullable. C++/WinRT already projects `IReference<T>` as `std::optional<T>`, so the mental model is consistent end-to-end. `String?` should be a warning, not an error — it communicates intent even if it's a no-op. `Guid?` should absolutely work (it's a value type). This can ship behind `/ext` with zero risk, but honestly it's harmless enough to be always-on.

---

## 2. Namespace aliases: `using`

**Problem**: Fully qualified names get long, especially with collections and foundation types.

**Proposed syntax**:
```idl
using Collections = Windows.Foundation.Collections;
using WUX = Microsoft.UI.Xaml;

namespace MyApp
{
    runtimeclass DataManager
    {
        Collections.IVector<String> GetItems();
        Collections.IMap<String, Collections.IVector<Int32>> GetGrouped();
        WUX.Controls.Control CreateControl();
    };
}
```

**Semantics**:
- File-scoped (applies to entire .idl file)
- Alias is a simple name substitution at the namespace level
- Does not affect WinMD output (aliases are resolved at compile time)
- Multiple `using` declarations allowed
- Collisions with real namespace names: alias takes priority within file

**Open questions**:
- Allow `using Collections = Windows.Foundation.Collections;` at namespace scope too (inside `namespace { }`)?
- Allow importing all types from a namespace: `using Windows.Foundation.Collections;` (unqualified access to IVector, IMap, etc.)?

**Critique:** 🟡 **This proposal should be split into two separate features.** (a) Qualified aliases (`using X = Some.Long.Namespace`) are 🟢 — they're pure compile-time name substitution, zero ABI impact, trivially reversible, and every developer understands them. Ship it. (b) Wildcard imports (`using Windows.Foundation.Collections;`) are 🟡 — they introduce C#-style "where did this name come from?" confusion, can cause ambiguity when two namespaces export the same type name, and make IDL files harder to read in isolation (you can't tell where `IVector` comes from without checking the imports). The collision rule ("alias takes priority") is a landmine: adding a new type to a referenced namespace could silently shadow a local name. If you do wildcard imports, at minimum require the compiler to warn on shadowing. Qualified aliases should be always-on; wildcard imports should require `/ext`.

---

## 3. Async method sugar

**Problem**: Every async WinRT method follows the same boilerplate pattern: return `IAsyncOperation<T>` or `IAsyncAction`, append `Async` to the name.

**Proposed syntax**:
```idl
runtimeclass Downloader
{
    async String Download(String url);            // → IAsyncOperation<String> DownloadAsync(String url)
    async void Cancel();                          // → IAsyncAction CancelAsync()
    async IVector<String> FetchAll(String query);  // → IAsyncOperation<IVector<String>> FetchAllAsync(String query)
};
```

**Desugars to**:
```idl
Windows.Foundation.IAsyncOperation<String> DownloadAsync(String url);
Windows.Foundation.IAsyncAction CancelAsync();
Windows.Foundation.IAsyncOperation<IVector<String>> FetchAllAsync(String query);
```

**Rules**:
- `async void` → `IAsyncAction`
- `async T` → `IAsyncOperation<T>`
- Method name gets `Async` suffix appended automatically
- If the user already wrote `Async` suffix, don't double it

**Open questions**:
- Support `async` on delegates too? `async delegate String Fetcher(String url);`
- Support progress variants? `async<T, TProgress>` → `IAsyncOperationWithProgress<T, TProgress>`?
- What if someone writes `async IAsyncOperation<String> Foo()`? Error or strip double-wrap?

**Critique:** 🔴 This is the most dangerous proposal here. It violates a core principle: **the IDL should describe the ABI contract, and this sugar makes the IDL lie about it**. The method name in source (`Download`) differs from the name in the WinMD (`DownloadAsync`). The return type in source (`String`) differs from the WinMD (`IAsyncOperation<String>`). When a C++/WinRT developer is debugging a failing call to `DownloadAsync`, they grep the IDL for `DownloadAsync` and find nothing. The "don't double Async" heuristic is a code smell — it means the feature can't even compose with its own naming convention. The progress variants (`IAsyncOperationWithProgress`, `IAsyncActionWithProgress`) further complicate the sugar without adding proportional value. The real cost of writing `IAsyncOperation<String> DownloadAsync()` is maybe 30 extra characters per method — this isn't a real pain point. Kill this proposal. If you want to reduce async boilerplate, add a snippet to the LSP instead.

---

## 4. Enum underlying type

**Problem**: MIDL3 enums are always Int32 unless `[flags]` is applied (which forces UInt32). No way to explicitly choose.

**Proposed syntax**:
```idl
enum ErrorCode : Int32
{
    NotFound = -1,
    OK = 0,
    Timeout = 1
};

[flags]
enum Permissions : UInt32
{
    None   = 0x0,
    Read   = 0x1,
    Write  = 0x2,
    Delete = 0x4
};
```

**Rules**:
- Only `Int32` and `UInt32` are valid (WinRT restriction)
- `[flags]` still implies `UInt32` if no explicit type given
- `: Int32` is the default (matches current behavior)
- Error if `[flags]` + `: Int32` (flags must be unsigned)

**Open questions**:
- Should we allow `: UInt32` without `[flags]`? WinRT metadata supports it but it's unusual.

**Critique:** 🟢 Trivial, safe, and makes the IDL more self-documenting. The WinMD already stores the underlying type, so this is just making explicit what was previously implicit. No projection is confused by this — they already handle both Int32 and UInt32 enums. `: UInt32` without `[flags]` should be allowed — WinRT supports it and there are legitimate uses (e.g., error codes that are HRESULTs). The only implementation concern is producing a good error message for invalid types (`: Int64`, `: Byte`, etc.). This could easily be always-on rather than behind `/ext` since it's strictly additive syntax.

---

## 5. Doc comments → WinMD metadata

**Problem**: IDL documentation is lost during compilation. Language projections (C#, C++/WinRT, Rust) have no access to API documentation from the WinMD.

**Proposed syntax**:
```idl
/// <summary>Manages application settings.</summary>
/// <remarks>All methods are thread-safe.</remarks>
runtimeclass SettingsManager
{
    /// <summary>Creates a new instance with default settings.</summary>
    SettingsManager();
    
    /// <summary>Gets a setting value by key.</summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or empty string if not found.</returns>
    String GetValue(String key);
};
```

**Implementation**: Emit as custom attributes in the WinMD (similar to how .NET stores XML doc). Could use a well-known attribute type like `Windows.Foundation.Metadata.DocumentationAttribute`.

**Open questions**:
- What attribute type to use? Define our own, or use an existing convention?
- Should the LSP show these in hover tooltips? (Yes — trivially, since we already parse comments.)
- Markdown support in addition to XML?

**Critique:** 🟡 The feature has two separable parts with very different risk profiles. **Part A (LSP tooltips from doc comments):** 🟢 — you already parse comments, this is pure tooling improvement, zero WinMD impact, ship it immediately. **Part B (embed docs in WinMD metadata):** 🟡 — this is where it gets hard. There is no standard attribute for this. If WinMIDL invents `DocumentationAttribute`, no existing projection will surface it. You'd need C++/WinRT, CsWinRT, and windows-rs to all agree to read it. That's a multi-year ecosystem coordination problem, not a compiler feature. Recommend: ship Part A now, design Part B as a formal proposal that goes through the WinRT metadata spec process. Do not invent an attribute unilaterally — it'll become a de facto standard that's hard to fix later. On format: prefer Markdown over XML doc comments. XML doc is a .NET convention that C++ and Rust developers find alien.

---

## 6. Default parameters → overload chains

**Problem**: WinRT doesn't support default parameter values. The common workaround is to declare multiple overloads manually with `[overload]` and `[default_overload]`. This is tedious and error-prone.

**Proposed syntax**:
```idl
runtimeclass HttpClient
{
    void Connect(String host, UInt16 port = 443, Boolean useTls = true);
};
```

**Desugars to**:
```idl
[default_overload] void Connect(String host);
[overload("ConnectWithPort")] void Connect(String host, UInt16 port);
[overload("ConnectWithPortAndTls")] void Connect(String host, UInt16 port, Boolean useTls);
```

**Rules**:
- Default params must be trailing (same as C#)
- Values must be compile-time constants (integer literals, string literals, `true`/`false`, `null`, enum values)
- Overload names auto-generated from parameter names: `MethodWith{Param1}And{Param2}`
- Only the shortest overload gets `[default_overload]`

**Open questions**:
- Naming strategy for generated overloads — camelCase param names? Let user override with `[overload("name")]`?
- Can default values reference enum members? e.g., `Mode mode = Mode.Default`
- Should the LSP show the generated overloads in completion/hover?

**Critique:** 🔴 This is a footgun dressed as convenience. The fundamental problem: **auto-generated overload names become permanent ABI surface**. `ConnectWithPortAndTls` is now a name that C++/WinRT consumers call directly, that appears in stack traces, that shows up in WinMD dumps. If you rename the `useTls` parameter to `enableTls`, the ABI name changes — you've made a source-level parameter rename into a binary breaking change. The default values themselves are *not* stored in the WinMD (WinRT has no concept of defaults), so consumers using other projections see three overloads with no indication of what the "default" values should be. A C# consumer sees `Connect(host)` and has no idea that port defaults to 443. The LSP question ("show generated overloads?") reveals the core problem: you need tooling to explain what the sugar actually did, which means the sugar isn't pulling its weight. Write the overloads explicitly — they're part of your ABI contract and deserve explicit names.

---

## 7. Inline delegate types

**Problem**: Every event or callback needs a separately declared delegate type, even for one-off uses.

**Proposed syntax**:
```idl
runtimeclass Processor
{
    // Inline delegate for event
    event delegate void(String item, Int32 index) ItemProcessed;
    
    // Inline delegate for callback parameter
    void ForEach(delegate Boolean(String item) predicate);
};
```

**Desugars to**:
```idl
delegate void ProcessorItemProcessedHandler(String item, Int32 index);
delegate Boolean ProcessorForEachPredicate(String item);

runtimeclass Processor
{
    event ProcessorItemProcessedHandler ItemProcessed;
    void ForEach(ProcessorForEachPredicate predicate);
};
```

**Rules**:
- Auto-generated delegate name: `{ClassName}{MemberName}{Role}` (Handler for events, Delegate for params)
- Delegate placed in same namespace as containing type
- If identical signature delegate already exists, reuse it

**Open questions**:
- Should inline delegates be allowed at interface level too?
- Naming collisions with existing types?

**Critique:** 🟡 There's real value here — standalone delegate declarations for one-off events are genuinely tedious. But the auto-generated names are the hard part. `ProcessorItemProcessedHandler` becomes a public type in your namespace that consumers reference directly. If you rename the class or the event, the delegate name changes — another case of internal naming decisions leaking into ABI. The "reuse identical signature" heuristic is dangerous: two delegates with the same signature but different semantic meanings (e.g., a completion handler and a validation handler) should NOT be the same type. Recommendation: allow inline delegates but require the user to explicitly name them: `event delegate void ItemHandler(String item, Int32 index) ItemProcessed;` — you get the colocation benefit without the auto-naming risk. Should be behind `/ext`.

---

## 8. Partial classes across files

**Problem**: MIDL3 has the `partial` keyword but only within a single file. Large classes can't be split across multiple .idl files for organization.

**Proposed syntax**:
```idl
// Widget.idl
namespace MyApp
{
    partial runtimeclass Widget
    {
        Widget();
        void DoWork();
    };
}

// Widget.events.idl
namespace MyApp
{
    partial runtimeclass Widget
    {
        event TypedEventHandler<Widget, Object> Changed;
        event TypedEventHandler<Widget, Object> Loaded;
    };
}
```

**Implementation**: `winmidl.json` lists both files as inputs. Compiler merges all `partial` declarations of the same type before binding.

**Rules**:
- All partials must be in the same namespace
- Attributes can be on any partial (merged)
- Base class can only be specified on one partial
- Interface implementations can be on any partial (merged)
- Conflicting attributes → error

**Open questions**:
- Should non-partial declarations be an error if the type appears in multiple files?
- How does this interact with `// @ref:` directives?

**Critique:** 🟢 This is well-understood, genuinely useful, and low-risk. Large WinRT APIs (think `Windows.UI.Xaml.Controls.Control`) have dozens of interfaces across multiple contract versions — splitting the IDL across files is a real organizational need. The merge-before-bind implementation is straightforward and produces the same WinMD as a single-file declaration. The rules are sensible (same namespace, merge attributes, single base class). One concern: "conflicting attributes → error" needs to be precisely defined. What about `[version(2)]` on one partial and `[version(3)]` on another — is that a conflict or valid versioning? Answer: attributes that can only appear once should error on duplicates; attributes that are additive (like interface implementations) should merge. Non-partial declarations appearing in multiple files should absolutely be an error. This could be always-on.

---

## 9. Tuple return sugar

**Problem**: WinRT uses `out` parameters for multiple return values, which is awkward to write and read.

**Proposed syntax**:
```idl
runtimeclass Parser
{
    (Boolean success, Int32 value) TryParseInt(String input);
    (String name, Int32 age, Boolean active) GetUserInfo(String id);
};
```

**Desugars to**:
```idl
Boolean TryParseInt(String input, out Int32 value);
String GetUserInfo(String id, out Int32 age, out Boolean active);
```

**Rules**:
- First tuple element becomes the return type
- Remaining elements become `out` parameters
- Parameter names come from the tuple field names

**Open questions**:
- What if there's only one element? `(Int32 value)` — just a regular return?
- Should the decompiler recognize this pattern and show tuple syntax?
- Edge case: `(void, Int32 result)` — should this be allowed? It would mean a void-returning method with one out param.

**Critique:** 🔴 This sugar is actively misleading. A developer reading `(Boolean success, Int32 value) TryParseInt(String input)` will expect the method to return a tuple — that's what this syntax means in C#, Rust, Python, and every other modern language. Instead, it emits a method that returns `Boolean` with an `out Int32 value` parameter. These are not the same thing. In C#, the projected signature would be `bool TryParseInt(string input, out int value)` — the caller's code looks nothing like the IDL. The "first element is the return type" rule is an arbitrary convention that must be memorized. The `(void, Int32 result)` edge case demonstrates the conceptual incoherence. The decompiler question ("should it recognize this pattern?") reveals a round-trip problem: you can't reliably distinguish "this was written as a tuple" from "this was written as a method with out params." Don't add syntax that requires a decompiler to maintain a fiction.

---

## 10. Observable property sugar

**Problem**: XAML data binding requires implementing `INotifyPropertyChanged`. Every observable property needs manual event raising boilerplate.

**Proposed syntax**:
```idl
[observable]
runtimeclass ViewModel
{
    String Name { get; set; };
    Int32 Count { get; set; };
    Boolean IsLoading { get; };  // read-only, still observable
};
```

**Desugars to**:
```idl
runtimeclass ViewModel : [default] IViewModel
{
    String Name { get; set; };
    Int32 Count { get; set; };
    Boolean IsLoading { get; };
    event Microsoft.UI.Xaml.Data.PropertyChangedEventHandler PropertyChanged;
};
```

**Open questions**:
- Which `PropertyChanged` event type? `System.ComponentModel.INotifyPropertyChanged` or `Microsoft.UI.Xaml.Data.INotifyPropertyChanged`?
- Does this require a reference to the XAML WinMD?
- Should individual properties be opt-out? `[not_observable] String CachedValue { get; set; };`

**Critique:** 🔴 This proposal has three fatal problems. **First**, it only adds the *event declaration* to the metadata — it doesn't (and can't) generate the code that *raises* the event when a property changes. The developer still has to write all the `PropertyChanged?.Invoke(this, ...)` boilerplate in their implementation. The sugar gives you 10% of the work and pretends it's the whole thing. **Second**, the first open question ("which PropertyChanged?") is a showstopper: there are multiple incompatible `INotifyPropertyChanged` interfaces across WinUI, UWP, and .NET, and the compiler must pick one, creating a hard framework dependency. **Third**, this is too narrow — it only helps XAML developers. WinRT components are consumed from C++, Rust, JavaScript, Python, and other contexts where XAML data binding doesn't exist. A language extension should be broadly useful. If XAML developers want observable boilerplate reduction, that's a job for source generators in their projection, not the IDL compiler.

---

## 11. User-defined parameterized interfaces

**Problem**: WinRT metadata supports parameterized types (generics), but MIDL3 only lets you *instantiate* existing ones (`IVector<String>`), not *define* new ones.

**Proposed syntax**:
```idl
namespace MyLib
{
    interface IRepository<T>
    {
        T GetById(String id);
        void Save(T item);
        IVectorView<T> GetAll();
        event TypedEventHandler<IRepository<T>, T> ItemAdded;
    };
    
    interface IConverter<TSource, TTarget>
    {
        TTarget Convert(TSource source);
        TSource ConvertBack(TTarget target);
    };
}
```

**Implementation**: Requires emitting `GenericParam` table entries and `TypeSpec` signatures with generic parameter references.

**Open questions**:
- Constraints? WinRT doesn't support generic constraints in metadata. Should we add compile-time-only constraints?
- Should generic runtimeclasses be allowed? WinRT doesn't support them (only interfaces and delegates).
- How does this interact with `[uuid]`? Parameterized interfaces use computed GUIDs (pinterface pattern).

**Critique:** 🟡 This is the highest-value proposal in the "Maybe" tier. MIDL3's inability to define parameterized interfaces is a genuine gap — library authors must currently hand-author WinMD files or use workarounds. The WinRT metadata format already supports this (GenericParam table, pinterface GUID computation), so the output is standard. However, implementation complexity is substantial: you need correct TypeSpec signatures, parametric GUID computation (`pinterface({guid}, T)`), and proper instantiation validation. The projection story is mixed: C#/WinRT handles parameterized interfaces well, C++/WinRT has good support, but Rust/WinRT support may be incomplete. Do NOT add compile-time-only constraints — they create a false promise (the constraint exists in the IDL but not in the metadata, so projections can't enforce it). Do NOT allow generic runtimeclasses — WinRT fundamentally doesn't support them, and pretending otherwise is misleading. Recommend: implement this, but as a later-phase feature after the 🟢 proposals ship. Must be behind `/ext`.

---

## 12. Constructor forwarding

**Problem**: When deriving from an unsealed class, you must manually redeclare and forward every base constructor.

**Proposed syntax**:
```idl
unsealed runtimeclass Base
{
    protected Base();
    protected Base(String name);
    protected Base(String name, Int32 priority);
};

runtimeclass Derived : Base
{
    inherits constructors;  // forwards all base constructors
    void DoExtra();
};
```

**Open questions**:
- Should `inherits constructors` forward ALL base constructors or only `protected` ones?
- Can you add additional constructors alongside `inherits constructors`?
- What if base adds constructors in a later contract version?

**Critique:** 🟡 The feature solves a real annoyance, but the versioning concern in the third open question is the dealbreaker in its current form. If `Base` adds a new constructor in contract version 3, does `Derived` silently gain that constructor too? If yes, you've created a hidden ABI change — the derived class's public surface changed without its IDL file being edited. If no, `inherits constructors` is a point-in-time snapshot that becomes stale. Either answer is bad. A safer alternative: have the compiler *suggest* the forwarding declarations (as an LSP quick-fix) rather than implicitly generating them. That gives the convenience without the versioning hazard. Also, WinRT inheritance is relatively uncommon — this benefits a narrow audience. If implemented, must be behind `/ext`, and should only forward constructors from the *current* contract version (explicit, not implicit).

---

## 13. Conditional compilation (MIDL3-native)

**Problem**: `#ifdef` works in compat mode via the C preprocessor, but strict MIDL3 mode doesn't support preprocessor directives.

**Proposed syntax**:
```idl
[if(ENABLE_EXPERIMENTAL)]
runtimeclass ExperimentalWidget
{
    ExperimentalWidget();
    void DoExperiment();
};

runtimeclass Widget
{
    Widget();
    void DoWork();
    
    [if(ENABLE_PREVIEW)]
    void DoPreviewThing();
};
```

**Semantics**:
- Conditions evaluated at compile time via `/D` flags or `winmidl.json` defines
- Can apply to: types, members, entire namespace blocks
- Nested conditions allowed

**Open questions**:
- Support expressions? `[if(A && !B)]`
- Else clause? `[if(X)] ... [else] ...`
- Should undefined symbols evaluate to false (like C preprocessor) or error?

**Critique:** 🟡 The need is real — strict MIDL3 mode drops `#ifdef` and there's no replacement. But the proposed solution has a design smell: `[if(ENABLE_EXPERIMENTAL)]` *looks* like a metadata attribute but behaves as a preprocessor directive. A developer reading the IDL might think this is a runtime attribute. Consider an explicitly different syntax that doesn't masquerade as an attribute — for example, `#[cfg(ENABLE_EXPERIMENTAL)]` (Rust-style) or `@if(ENABLE_EXPERIMENTAL)` to visually distinguish compile-time directives from metadata attributes. On the open questions: undefined symbols should be an error (silent false is a classic C preprocessor footgun — typo in `ENBALE_EXPERIMENTAL` silently drops your type). Boolean expressions (`&&`, `!`) are reasonable. `[else]` is fine but keep it simple — no `[elif]` chains. Must be behind `/ext` since it's new syntax.

---

## 14. Interface default implementations

**Problem**: Adding methods to an existing interface is a breaking change. Default implementations would allow non-breaking evolution.

**Proposed syntax**:
```idl
interface ILogger
{
    void Log(String message, Int32 level);
    
    default void LogInfo(String message)
    {
        Log(message, 0);
    };
    
    default void LogError(String message)
    {
        Log(message, 2);
    };
};
```

**Implementation**: Since WinRT doesn't support default interface methods at runtime, this would generate a separate static helper class:
```idl
static runtimeclass ILoggerDefaults
{
    static void LogInfo(ILogger self, String message);
    static void LogError(ILogger self, String message);
};
```

**Open questions**:
- Is a static helper class the right pattern? Or extension methods?
- Should projections (C#, C++/WinRT) be able to surface these as actual default implementations?
- This fundamentally changes the contract — is it really non-breaking?

**Critique:** 🔴 This proposal is the most misleading one in the document. The syntax says `default void LogInfo(...)` with a body — every developer will read this as "if the implementer doesn't provide `LogInfo`, this default runs." **That is not what happens.** WinRT has no runtime mechanism for default interface dispatch. The generated `ILoggerDefaults` static helper class is a completely different thing: it's a utility class that consumers must explicitly call, not an automatic fallback. The third open question answers itself: no, this is NOT non-breaking interface evolution. Adding `LogInfo` to `ILogger` still requires all existing implementors to implement it — the static helper just gives them a convenient forwarding implementation to copy. If you want non-breaking interface evolution in WinRT, the actual pattern is versioned interfaces (`ILogger2`), and no amount of syntax sugar changes that fundamental ABI constraint. This feature would teach developers the wrong mental model and produce confused bug reports when "default" methods aren't called. Hard no.

---

## 15. Struct companion methods

**Problem**: WinRT structs are pure value types with no methods. Common operations like `Equals`, `ToString`, or factory methods require a separate helper class.

**Proposed syntax**:
```idl
struct Rect
{
    Single X;
    Single Y;
    Single Width;
    Single Height;
    
    static Rect FromLTRB(Single left, Single top, Single right, Single bottom);
    static Boolean Intersects(Rect a, Rect b);
};
```

**Desugars to**:
```idl
struct Rect { Single X; Single Y; Single Width; Single Height; };

static runtimeclass RectHelper
{
    static Rect FromLTRB(Single left, Single top, Single right, Single bottom);
    static Boolean Intersects(Rect a, Rect b);
};
```

**Open questions**:
- Naming convention: `{Struct}Helper` or `{Struct}Extensions`?
- Should projections surface these as instance methods on the struct?
- `Windows.Foundation.Rect` already has `RectHelper` — should we detect and reuse that pattern?

**Critique:** 🟡 This is a reasonable convenience that follows an established Windows pattern (`RectHelper`, `PointHelper`, `ColorHelper`). The desugaring is predictable and the generated class is a real type in the WinMD — no hidden magic. The main concern is discoverability: a C++/WinRT consumer sees `RectHelper::FromLTRB()` which is fine, but they won't know it was declared inside the struct in the IDL. That's acceptable — it's how the existing helpers already work. Use `{Struct}Helper` (not `Extensions`) to match the existing Windows SDK convention. The "detect and reuse" question for existing helpers like `Windows.Foundation.RectHelper` is a nice-to-have but not essential — you're defining new types, not extending system ones. One risk: if someone later adds instance methods to WinRT structs (a long-requested runtime feature), this sugar becomes a confusing legacy. That seems unlikely enough to not block this. Should be behind `/ext`.
