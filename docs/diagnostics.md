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
| [MIDL1100](#midl1100) | Error | Unexpected code (skipped tokens) |
| [MIDL1101](#midl1101) | Error | Unexpected character |
| [MIDL1102](#midl1102) | Error | Expected token missing |
| [MIDL2001](#midl2001) | Warning | Classic MIDL construct (strict mode) |
| [MIDL2002](#midl2002) | Warning | HRESULT return type (strict mode) |
| [MIDL2003](#midl2003) | Warning | Classic MIDL attribute (strict mode) |
| [MIDL2004](#midl2004) | Warning | Pointer type (strict mode) |
| [MIDL2005](#midl2005) | Warning | Classic MIDL type name (strict mode) |

---

## Semantic Errors (MIDL1001–MIDL1013)

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
