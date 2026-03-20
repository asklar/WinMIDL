# MIDL Compiler Diagnostic Reference

This document lists all diagnostics emitted by the MIDL compiler, organized by category.

## Quick Reference

| Code | Severity | Summary |
|------|----------|---------|
| [MIDL1001](#midl1001) | Error | Undefined type |
| [MIDL1002](#midl1002) | Error | Duplicate definition |
| [MIDL1003](#midl1003) | Error | Namespace required |
| [MIDL1004](#midl1004) | Error | Invalid base class |
| [MIDL1005](#midl1005) | Error | Struct field not a value type |
| [MIDL1006](#midl1006) | Error | Invalid enum underlying type |
| [MIDL1007](#midl1007) | Error | Instance member in static class |
| [MIDL1008](#midl1008) | Error | \[flags\] enum must be UInt32 |
| [MIDL1009](#midl1009) | Error | Empty struct |
| [MIDL1010](#midl1010) | Error | Case-insensitive type name collision |
| [MIDL1011](#midl1011) | Warning | Same-arity method overload |
| [MIDL1012](#midl1012) | Error | Write-only property |
| [MIDL1013](#midl1013) | Warning | Reserved parameter name |
| [MIDL1033](#midl1033) | Error | Protected/overridable on sealed class |
| [MIDL1034](#midl1034) | Error | Undefined required interface |
| [MIDL1035](#midl1035) | Error | Base type is not a runtimeclass |
| [MIDL1036](#midl1036) | Error | Base class is sealed |
| [MIDL1038](#midl1038) | Error | Runtimeclass in requires clause |
| [MIDL1039](#midl1039) | Error | Constructor in interface |
| [MIDL1040](#midl1040) | Error | Static member in interface |
| [MIDL1041](#midl1041) | Error | Property or event in struct |
| [MIDL1042](#midl1042) | Error | Event with non-delegate type |
| [MIDL1043](#midl1043) | Error | Void parameter type |
| [MIDL1044](#midl1044) | Warning | Duplicate method name without \[overload\] |
| [MIDL1045](#midl1045) | Error | Circular struct reference |
| [MIDL1046](#midl1046) | Error | Duplicate enum member |
| [MIDL1047](#midl1047) | Error | Import file not found |
| [MIDL1048](#midl1048) | Error | C-only type in strict mode |
| [MIDL1049](#midl1049) | Error | Nested type declaration |
| [MIDL1050](#midl1050) | Warning | Bidirectional (ref) parameter |
| [MIDL1051](#midl1051) | Error | Array of arrays |
| [MIDL1052](#midl1052) | Error | Property type inconsistency |
| [MIDL1053](#midl1053) | Error | Delegate non-void return |
| [MIDL1054](#midl1054) | Warning | Duplicate interface member name |
| [MIDL1055](#midl1055) | Warning | Same-arity overload without [default_overload] |
| [MIDL1056](#midl1056) | Warning | Deprecated base class inheritance |
| [MIDL1057](#midl1057) | Info | Interface method not implemented |
| [MIDL1058](#midl1058) | Error | Duplicate [method_name] value |
| [MIDL1059](#midl1059) | Error | Multiple [default_overload] on same arity |
| [MIDL1060](#midl1060) | Error | Constructor arity conflict per version |
| [MIDL1061](#midl1061) | Error | Static + unsealed conflict || [MIDL1100](#midl1100) | Error | Unexpected code (skipped tokens) |
| [MIDL1101](#midl1101) | Error | Unexpected character |
| [MIDL1102](#midl1102) | Error | Expected token missing |
| [MIDL2001](#midl2001) | Warning | Classic MIDL construct (strict mode) |
| [MIDL2002](#midl2002) | Warning | HRESULT return type (strict mode) |
| [MIDL2003](#midl2003) | Warning | Classic MIDL attribute (strict mode) |
| [MIDL2004](#midl2004) | Warning | Pointer type (strict mode) |
| [MIDL2005](#midl2005) | Warning | Classic MIDL type name (strict mode) |

---


### MIDL1048
**C-only type in strict mode** — Types like `wchar_t`, `handle_t`, `error_status_t` are not valid WinRT types in strict MIDL3 mode.

### MIDL1049
**Nested type declaration** — WinRT does not support nested type declarations (e.g., a runtimeclass inside another runtimeclass).

### MIDL1050
**Bidirectional parameter** — The `ref` (bidirectional) parameter direction is not recommended in WinRT. Use `in` or `out` instead.

### MIDL1051
**Array of arrays** — WinRT does not support jagged arrays (`T[][]`). Use a single-dimensional array instead.

### MIDL1052
**Property type inconsistency** — A property's getter return type must match its setter parameter type.

### MIDL1053
**Delegate non-void return** — WinRT delegates must return `void`. In compat mode, `HRESULT` is also allowed.

### MIDL1054
**Duplicate interface member name** — Interface members must have unique names. Use `[overload]` to disambiguate methods with the same name.

### MIDL1055
**Same-arity overload needs [default_overload]** — When methods share the same name and parameter count, exactly one must have `[default_overload]`.

### MIDL1056
**Deprecated base class** — Consider marking the derived class as deprecated when the base class is deprecated.

### MIDL1057
**Interface method info** — Informational diagnostic noting interface method declarations on implemented interfaces.

### MIDL1058
**Duplicate method_name** — Each `[method_name("X")]` value must be unique within an interface.

### MIDL1059
**Multiple default_overload** — For same-name same-arity methods, exactly one must have `[default_overload]`, not more.

### MIDL1060
**Constructor arity conflict** — Cannot have two constructors with the same parameter count on the same contract version.

### MIDL1061
**Static + unsealed conflict** — A runtimeclass cannot be both `static` and `unsealed`.
## Semantic Errors (MIDL1001–MIDL1061)

These errors are emitted during semantic analysis (binding phase).

### MIDL1001

**Undefined type**

```
error MIDL1001: Undefined type 'NonExistentType'
```

A type referenced in a declaration could not be found. The type may be misspelled, missing an `import`, or missing a `/reference` to a `.winmd` file.

**Fix:** Check the spelling, add an `import "file.idl"` directive, or add `/reference path.winmd` to reference external types.

---

### MIDL1002

**Duplicate definition**

```
error MIDL1002: Duplicate definition 'Widget'
```

Two types with the same name are defined in the same namespace.

**Fix:** Rename one of the types, or move it to a different namespace.

---

### MIDL1003

**Namespace required**

```
error MIDL1003: Namespace required for WinRT type 'MyClass'
```

All WinRT types must be declared inside a `namespace` block. Types at the global scope are not valid.

**Fix:** Wrap the type declaration in a namespace:

```idl
namespace MyApp
{
    runtimeclass MyClass { }
}
```

---

### MIDL1004

**Invalid base class**

```
error MIDL1004: Invalid base class 'SealedClass' (not unsealed)
```

A runtime class is trying to derive from a class that is not marked as `unsealed`.

**Fix:** Mark the base class as `unsealed runtimeclass`, or remove the inheritance.

---

### MIDL1005

**Struct field not a value type**

```
error MIDL1005: Struct field 'Data' must be a value type, string, or IReference<T>
```

WinRT structs can only contain fields of value types (integers, floats, enums, other structs), `String`, or `Windows.Foundation.IReference<T>`. Reference types (interfaces, runtime classes) are not allowed.

**Fix:** Change the field type to a value type, or use a runtime class instead of a struct.

---

### MIDL1006

**Invalid enum underlying type**

```
error MIDL1006: Enum 'MyEnum' underlying type must be Int32 or UInt32
```

WinRT enums can only have `Int32` (for standard enums) or `UInt32` (for `[flags]` enums) as their underlying type.

**Fix:** Use a standard enum (Int32) or a `[flags]` enum (UInt32).

---

### MIDL1007

**Instance member in static class**

```
error MIDL1007: Static class cannot have instance member 'DoWork'
```

A class declared as `static runtimeclass` cannot have instance members — all members must be `static`.

**Fix:** Add the `static` keyword to the member, or remove `static` from the class.

---

### MIDL1008

**\[flags\] enum must be UInt32**

```
error MIDL1008: [flags] enum 'Permissions' must have UInt32 underlying type
```

Enums marked with `[flags]` must use `UInt32` as their underlying type. This is a WinRT requirement.

**Fix:** This is automatically handled — `[flags]` enums use UInt32, standard enums use Int32.

---

### MIDL1009

**Empty struct**

```
error MIDL1009: Empty struct 'EmptyStruct' — WinRT structs must have at least one field
```

WinRT structs must contain at least one field. Empty structs are not valid.

**Fix:** Add at least one field, or use a runtime class instead.

---

### MIDL1010

**Case-insensitive type name collision**

```
error MIDL1010: Type 'foo' conflicts with 'Foo' (WinRT type names are case-insensitive)
```

WinRT type names are case-insensitive. Two types that differ only by casing are not allowed in the same namespace.

**Fix:** Rename one of the types to have a distinct name.

---

### MIDL1011

**Same-arity method overload**

```
warning MIDL1011: Method 'DoWork' has same arity as 'DoWork' — use [default_overload] to disambiguate
```

Two methods with the same name and the same number of parameters exist on the same interface. Weakly-typed languages (like JavaScript) cannot distinguish between them.

**Fix:** Mark one overload with `[default_overload]`, or rename overloads using `[method_name("...")]`:

```idl
[default_overload]
void DoWork(Int32 x);
[method_name("DoWorkWithLabel")]
void DoWork(String label);
```

---

### MIDL1012

**Write-only property**

```
error MIDL1012: Property 'Value' cannot be write-only — WinRT does not support write-only properties
```

WinRT properties must have at least a getter. Write-only properties (`{ set; }` without `{ get; }`) are not supported.

**Fix:** Add a getter, or change to a method:

```idl
String Value { get; set; };  // read-write property
void SetValue(String v);     // or use a method
```

---

### MIDL1013

**Reserved parameter name**

```
warning MIDL1013: Reserved parameter name 'result' — use [return_name("alt")] to override
```

The parameter name is reserved by the MIDL compiler for auto-generated ABI parameters:

| Reserved name | Used by | Can override? |
|---------------|---------|---------------|
| `value` | Constructor return | Yes, via `[return_name]` |
| `result` | Method return | Yes, via `[return_name]` |
| `operation` | IAsyncXxx return | Yes, via `[return_name]` |
| `baseInterface` | Unsealed constructor | No |
| `innerInterface` | Unsealed constructor | No |

**Fix:** Rename the parameter, or use `[return_name("altName")]` to change the auto-generated name.

---

### MIDL1033

**Protected or overridable on sealed class**

```
error MIDL1033: 'DoWork' cannot be protected in sealed runtimeclass 'MyClass'. Use 'unsealed runtimeclass' to allow inheritance.
error MIDL1033: 'OnChanged' cannot be overridable in sealed runtimeclass 'MyClass'. Use 'unsealed runtimeclass' to allow inheritance.
```

A member marked `protected` or `overridable` can only appear in an `unsealed runtimeclass`. Sealed classes (the default) cannot participate in inheritance, so these modifiers are invalid.

**Fix:** Mark the containing class as `unsealed runtimeclass`, or remove the `protected`/`overridable` modifier from the member.

---

### MIDL1034

**Undefined required interface**

```
error MIDL1034: Required interface 'IUndefined' is undefined.
```

An interface's `requires` clause references an interface name that cannot be resolved.

**Fix:** Check the spelling, add an `import` directive, or add a `/reference` to a `.winmd` file that defines the interface.

---

### MIDL1035

**Base type is not a runtimeclass**

```
error MIDL1035: Base type 'MyStruct' is not a runtimeclass. Use ':' for implemented interfaces.
```

A runtimeclass specifies a base type in its inheritance list, but the resolved type is a struct, enum, delegate, or other non-runtimeclass type. Only runtimeclasses can be base classes.

**Fix:** Use a runtimeclass as the base, or if you meant to implement an interface, ensure the type is declared as an `interface`.

---

### MIDL1036

**Base class is sealed**

```
error MIDL1036: Cannot derive from sealed runtimeclass 'Base'. The base class must be declared as 'unsealed runtimeclass'.
```

A runtimeclass is trying to derive from another runtimeclass that is not marked as `unsealed`.

**Fix:** Mark the base class as `unsealed runtimeclass`:

```idl
unsealed runtimeclass Base { }
runtimeclass Derived : Base { }
```

---

### MIDL1038

**Runtimeclass in requires clause**

```
error MIDL1038: 'MyClass' is a runtimeclass. Only interfaces are allowed in 'requires' clauses.
```

An interface's `requires` clause references a runtimeclass instead of an interface. The `requires` clause is for specifying interface dependencies only.

**Fix:** Replace the runtimeclass with the interface that the runtimeclass implements, or remove it from the `requires` clause.

---

### MIDL1039

**Constructor in interface**

```
error MIDL1039: Constructors are not allowed in interfaces. Only runtimeclasses can have constructors.
```

An interface declaration contains a constructor. WinRT interfaces cannot have constructors — only `runtimeclass` types can declare constructors.

**Fix:** Move the constructor to a `runtimeclass`, or remove it:

```idl
// Wrong:
interface IFoo { IFoo(); }

// Right:
runtimeclass Foo { Foo(); }
```

---

### MIDL1040

**Static member in interface**

```
error MIDL1040: Static members are not allowed in interfaces.
```

An interface contains a member declared as `static`. WinRT interfaces cannot have static members — static members belong on `runtimeclass` types.

**Fix:** Move the static member to a `runtimeclass`, or remove the `static` keyword:

```idl
// Wrong:
interface IFoo { static void Reset(); }

// Right:
runtimeclass Foo { static void Reset(); }
```

---

### MIDL1041

**Property or event in struct**

```
error MIDL1041: Properties are not allowed in structs. Structs can only contain fields.
error MIDL1041: Events are not allowed in structs.
```

A struct declaration contains a property or event. WinRT structs can only contain fields (plain data members). Properties and events belong on `interface` or `runtimeclass` types.

**Fix:** Use a plain field instead of a property, or move to a `runtimeclass`:

```idl
// Wrong:
struct S { String Name { get; }; }

// Right:
struct S { String Name; }
```

---

### MIDL1042

**Event with non-delegate type**

```
error MIDL1042: Event type 'Int32' is not a delegate type.
```

An event is declared with a type that is not a delegate. WinRT events must use a delegate type (e.g., `EventHandler`, `TypedEventHandler<TSender, TArgs>`, or a custom delegate).

**Fix:** Declare a delegate type and use it for the event:

```idl
// Wrong:
event Int32 Changed;

// Right:
delegate void ChangedHandler(Object sender);
event ChangedHandler Changed;
```

---

### MIDL1043

**Void parameter type**

```
error MIDL1043: 'void' is not valid as a parameter type.
```

A method parameter is declared with type `void`. The `void` keyword is only valid as a return type (meaning the method returns nothing), not as a parameter type.

**Fix:** Use an actual type for the parameter:

```idl
// Wrong:
void M(void x);

// Right:
void M(Int32 x);
```

---

### MIDL1044

**Duplicate method name without \[overload\]**

```
warning MIDL1044: Methods 'Do' have same name but different signatures. Use [overload("name")] and [method_name("name")] to disambiguate.
```

A runtimeclass has multiple methods with the same name but no `[overload]` attribute. WinRT requires overloaded methods to have unique ABI names, which are specified via `[overload]` and `[method_name]` attributes.

**Fix:** Add `[overload]` and `[method_name]` attributes:

```idl
[overload("DoWithInt")]
[method_name("DoWithInt")]
void Do(Int32 x);

[overload("DoWithString")]
[method_name("DoWithString")]
void Do(String x);
```

---

### MIDL1045

**Circular struct reference**

```
error MIDL1045: Circular reference between struct 'A' and struct 'B'.
```

Two or more structs form a circular reference through their fields. WinRT structs are value types and cannot contain circular references (this would require infinite storage).

**Fix:** Break the cycle by using a `runtimeclass` (reference type) for one of the types, or restructure the data:

```idl
// Wrong:
struct A { B b; }
struct B { A a; }

// Right:
struct A { Int32 bId; }
struct B { Int32 aId; }
```

---

### MIDL1046

**Duplicate enum member**

```
error MIDL1046: Duplicate enum member 'A'.
```

An enum contains two members with the same name.

**Fix:** Remove the duplicate or rename one of the members:

```idl
enum E { A, B, C }
```

---

### MIDL1047

**Import file not found**

```
error MIDL1047: Cannot find imported file 'missing.idl'.
```

An `import` directive references a file that cannot be found relative to the current file or any include path (`/I`).

**Fix:** Check the file path spelling, ensure the file exists, or add the directory to include paths with `/I path`:

```idl
import "types.idl";  // Must exist relative to current file or /I paths
```

---

## Syntax Errors (MIDL1100–MIDL1102)

These errors are emitted when the parser encounters invalid or unexpected tokens.

### MIDL1100

**Unexpected code (skipped tokens)**

```
error MIDL1100: Unexpected code 'foo bar'. Did you mean to put this inside a namespace or runtimeclass?
```

The parser found tokens that don't form a valid MIDL construct in this context. The tokens were skipped to allow parsing to continue.

**Fix:** Check for typos, missing braces, or code that should be inside a declaration block.

---

### MIDL1101

**Unexpected character**

```
error MIDL1101: Unexpected character '@'. MIDL does not use this character.
```

The lexer encountered a character that is not valid in MIDL syntax.

**Fix:** Remove the invalid character.

---

### MIDL1102

**Expected token missing**

```
error MIDL1102: Expected '}'
error MIDL1102: Expected ';'
error MIDL1102: Expected 'identifier'
```

The parser expected a specific token but didn't find it. A synthetic "missing" token was inserted to allow parsing to continue.

**Fix:** Add the missing token. Common cases:
- Missing `}` — an unclosed brace block
- Missing `;` — a statement or declaration without a terminating semicolon
- Missing identifier — a keyword like `namespace` or `runtimeclass` without a name

---

## Strict Mode Warnings (MIDL2001–MIDL2005)

These warnings are emitted in **strict MIDL3 mode** (the default). They indicate the use of classic MIDL constructs that are not part of MIDL3 syntax. Disable with `/no_strict` or `/compat`.

### MIDL2001

**Classic MIDL construct**

```
warning MIDL2001: 'coclass' is a classic MIDL construct and is not allowed in strict MIDL3 mode.
                  Use /compat to enable classic syntax.
```

The following classic constructs trigger this warning:

| Construct | MIDL3 equivalent |
|-----------|-----------------|
| `coclass` | `runtimeclass` with `[activatable]` |
| `library` | `namespace` |
| `module` | `static runtimeclass` |
| `typedef` | (not needed in MIDL3) |
| `dispinterface` | `interface` |
| `cpp_quote` | (not needed in MIDL3) |
| `importlib` | `import` |
| `union` | (use structs or enums) |

**Fix:** Convert to MIDL3 syntax, or use `/compat` mode.

📖 [MIDL3 documentation](https://learn.microsoft.com/en-us/uwp/midl-3/)

---

### MIDL2002

**HRESULT return type**

```
warning MIDL2002: HRESULT return type is not used in MIDL3. Methods should return the actual type directly.
```

In classic MIDL, methods return `HRESULT` and use `[out, retval]` parameters for the actual return value. In MIDL3, methods return the value directly.

**Fix:**

```idl
// Classic MIDL (MIDL2):
HRESULT GetName([out, retval] HSTRING* name);

// Modern MIDL3:
String GetName();
```

📖 [MIDL3 documentation](https://learn.microsoft.com/en-us/uwp/midl-3/)

---

### MIDL2003

**Classic MIDL attribute**

```
warning MIDL2003: '[in]' is a classic MIDL attribute. In MIDL3, use the equivalent modern syntax.
```

Classic `[in]` and `[out]` parameter attributes are not needed in MIDL3. Parameters are `in` by default; use the `out` keyword for output parameters.

**Fix:**

```idl
// Classic: void M([in] Int32 x, [out] Int32* y);
// MIDL3:  void M(Int32 x, out Int32 y);
```

📖 [MIDL3 documentation](https://learn.microsoft.com/en-us/uwp/midl-3/)

---

### MIDL2004

**Pointer type**

```
warning MIDL2004: Pointer types ('IFoo*') are not used in MIDL3. Reference types are passed directly.
```

MIDL3 does not use pointer syntax (`*`). Reference types (interfaces, runtime classes) are passed directly by name.

**Fix:**

```idl
// Classic: void M(IFoo* pFoo);
// MIDL3:  void M(IFoo foo);
```

📖 [MIDL3 documentation](https://learn.microsoft.com/en-us/uwp/midl-3/)

---

### MIDL2005

**Classic MIDL type name** ⚡ *Quick fix available*

```
warning MIDL2005: Lowercase 'int' is MIDL2 syntax. Use 'Int32' (capitalized) for MIDL3.
```

MIDL3 uses capitalized type names that match WinRT naming conventions. The following replacements are available as **one-click quick fixes** in VS Code:

| Classic type | MIDL3 type | Notes |
|-------------|-----------|-------|
| `int` | `Int32` | |
| `long` | `Int32` | Note: `long` maps to 32-bit in MIDL |
| `short` | `Int16` | |
| `byte` | `UInt8` | |
| `small` | `UInt8` | |
| `hyper` | `Int64` | |
| `float` | `Single` | |
| `double` | `Double` | |
| `boolean` | `Boolean` | Lowercase `boolean` is MIDL2 |
| `char` | `Char` | |
| `wchar_t` | `Char` | |
| `BSTR` | `String` | |
| `LPSTR` | `String` | |
| `LPWSTR` | `String` | |
| `VARIANT` | `Object` | |
| `unsigned` | `UInt8`/`UInt16`/`UInt32`/`UInt64` | Use specific size |
| `signed` | `Int16`/`Int32`/`Int64` | Use specific size |
| `handle_t` | *(not available)* | Not used in MIDL3 |
| `error_status_t` | *(not available)* | Not used in MIDL3 |

**Fix:** Click the 💡 lightbulb in VS Code to auto-replace, or manually change the type name.

📖 [MIDL3 documentation](https://learn.microsoft.com/en-us/uwp/midl-3/)

---

## Suppressing Warnings

### CLI

```bash
# Suppress all warnings
midl /no_warn input.idl

# Disable strict mode (suppresses MIDL2xxx)
midl /no_strict input.idl
midl /compat input.idl

# Treat warnings as errors
midl /WX input.idl
```

### VS Code

In VS Code Settings, set `midl.strictMode` to `false` to disable MIDL2xxx warnings.

---

## See Also

- [Introduction to MIDL 3.0](https://learn.microsoft.com/en-us/uwp/midl-3/intro)
- [Transition from classic MIDLRT to MIDL 3.0](https://learn.microsoft.com/en-us/uwp/midl-3/from-midlrt)
- [Reserved keywords](https://learn.microsoft.com/en-us/uwp/midl-3/reserved-keywords)
- [Troubleshooting MIDL 3.0](https://learn.microsoft.com/en-us/uwp/midl-3/troubleshooting)
