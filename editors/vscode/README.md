# MIDL Language Support for VS Code

Full language support for Microsoft Interface Definition Language (MIDL/MIDL3) `.idl` files — syntax highlighting, IntelliSense, diagnostics, and more.

---

## Features

### 🎨 Syntax Highlighting

Full TextMate grammar covering both MIDL3 and classic MIDL syntax:

- **Keywords** — `namespace`, `runtimeclass`, `interface`, `delegate`, `struct`, `enum`, `event`, `import`, `apicontract`, and classic keywords like `coclass`, `library`, `typedef`, `union`
- **Types** — WinRT primitives (`Boolean`, `Int32`, `String`, `Guid`, `Object`, …) and classic types (`HRESULT`, `BSTR`, `IUnknown`, `IDispatch`, …)
- **Qualified type names** — Dotted names like `Windows.Foundation.Uri` highlighted as types
- **Attributes** — Keywords inside `[...]` brackets (e.g., `uuid`, `contract`, `activatable`, `composable`, `threading`, `deprecated`, and many more)
- **GUIDs** — Inline GUID literals colorized as constants
- **Strings, numbers, comments** — Double-quoted strings with escape sequences, hex (`0xFF`) and decimal literals, line (`//`) and block (`/* */`) comments
- **Storage modifiers** — `static`, `partial`, `unsealed`, `protected`, `overridable`, `get`, `set`, `out`, `ref`, `const`

### ⚡ IntelliSense / Completions

Context-aware auto-complete powered by the MIDL language server:

- **Declaration keywords** — `namespace`, `runtimeclass`, `interface`, `delegate`, `struct`, `enum`, `event`, `apicontract`, `import`
- **Modifier keywords** — `static`, `partial`, `unsealed`, `protected`, `overridable`
- **Built-in WinRT types** — `Boolean`, `Char`, `Int16`, `Int32`, `Int64`, `UInt8`, `UInt16`, `UInt32`, `UInt64`, `Single`, `Double`, `String`, `Guid`, `Object`
- **Common WinRT collection/async types** — `IVector`, `IVectorView`, `IMap`, `IMapView`, `IIterable`, `IAsyncOperation`, `IAsyncAction`, `EventHandler`, `TypedEventHandler`
- **Property/parameter keywords** — `get`, `set`, `out`, `ref`, `void`
- **External types** — Types from referenced `.winmd` files in the workspace directory appear with their fully-qualified names
- **Trigger characters** — Completions trigger on `.` and space

### 📝 Code Snippets

19 snippets for common MIDL3 patterns — type a prefix and press Tab:

| Prefix | Description |
|--------|-------------|
| `namespace` | Namespace declaration |
| `runtimeclass` | Runtime class with default constructor |
| `unsealedruntimeclass` | Unsealed (composable) runtime class with base class |
| `staticruntimeclass` | Static-only runtime class |
| `interface` | Interface declaration |
| `interfacerequires` | Interface with `requires` clause |
| `delegate` | Delegate type |
| `struct` | Struct with fields |
| `enum` | Enumeration |
| `flagsenum` | Flags enumeration with `[flags]` attribute |
| `propget` | Read-only property (`{ get; }`) |
| `proprw` | Read-write property (shorthand) |
| `event` | Typed event (`TypedEventHandler`) |
| `asyncmethod` | Async method returning `IAsyncOperation<T>` |
| `asyncaction` | Async method returning `IAsyncAction` |
| `contract` | Contract version attribute |
| `apicontract` | API contract definition with `[contractversion]` |
| `versionedblock` | Versioned member block with contract attribute |
| `import` | Import directive |

### 🔍 Real-Time Diagnostics

Errors and warnings appear as you type, powered by the MIDL semantic analysis engine:

- Syntax errors with precise locations
- Undefined type detection
- Duplicate definition warnings
- Internal compiler error reporting (graceful fallback)

Diagnostics are published on every document open and change via the Language Server Protocol.

### 🖱️ Hover Information

Hover over any token for contextual documentation:

- **Keywords** — Markdown-formatted documentation for all MIDL3 keywords (`namespace`, `runtimeclass`, `interface`, `delegate`, `struct`, `enum`, `event`, `static`, `partial`, `unsealed`, `protected`, `overridable`, `apicontract`, `import`, `requires`, `get`, `set`, `out`, `ref`, `void`)
- **Built-in types** — Descriptions for all WinRT primitive types (`Boolean`, `Int32`, `String`, `Guid`, `Object`, etc.)
- **User-defined types** — Shows the symbol kind and fully-qualified name (e.g., `**RuntimeClass** \`MyNamespace.MyClass\``)
- **External types** — Shows the fully-qualified name and source `.winmd` file path
- **Identifiers** — Unresolved identifiers shown with their text

### 🔗 Go to Definition

Navigate to where a symbol is defined:

- **Ctrl+Click** or **F12** on any type name or member name
- Jumps to the defining `runtimeclass`, `interface`, `struct`, `enum`, `delegate`, `apicontract`, `namespace`, method, or constructor declaration
- Works for top-level types and nested members (methods, properties, constructors)
- External (`.winmd`) symbols are excluded — navigation is within the current file

### 🔎 Find All References

Find every usage of a symbol in the document:

- **Shift+F12** on any identifier
- Locates all occurrences of the identifier token across the file
- Verifies the symbol exists in the compilation before reporting results
- Returns precise ranges for each reference location

### ✍️ Signature Help

Parameter hints when typing inside method calls and constructors:

- Triggered by `(` and `,`
- Shows full method/constructor/delegate signature with parameter list
- Highlights the active parameter based on comma position
- Supports methods, constructors, and delegate invocations

### 🎯 Semantic Highlighting

Rich semantic token classification beyond TextMate grammars:

- **14 token types** — `namespace`, `type`, `interface`, `enum`, `enumMember`, `method`, `property`, `event`, `parameter`, `keyword`, `comment`, `string`, `number`, `struct`
- **Token modifiers** — `declaration`, `static`
- Symbol-aware classification: identifiers are resolved against the compilation to determine their exact kind
- Comments in leading trivia are classified for accurate highlighting
- Works on top of the TextMate grammar for enhanced highlighting in supported themes

### 📐 Document Formatting

Auto-format MIDL documents with **Format Document** (Shift+Alt+F):

- Consistent 4-space indentation based on brace nesting
- Normalizes spacing after commas
- Normalizes spacing around `:` (preserving `::` scope resolution)
- Blank lines inserted between top-level declarations
- Trailing whitespace removed from all lines
- Ensures trailing newline

### 📋 Document Outline

See all symbols in the **Outline** view (Ctrl+Shift+O) and **Breadcrumbs**:

- Namespaces, runtime classes, interfaces, structs, enums, delegates, API contracts
- Nested members: methods, properties, events, constructors, fields, enum members
- Hierarchical tree structure matching your code's namespace/type/member nesting

### 📄 Language Configuration

Editor behaviors configured out of the box:

- **Comment toggling** — Ctrl+/ for line comments (`//`), Shift+Alt+A for block comments (`/* */`)
- **Bracket matching** — `{}`, `[]`, `()`, `<>`
- **Auto-closing pairs** — Braces, brackets, parentheses, angle brackets, quotes
- **Folding** — Fold code blocks on `{` / `}`
- **Smart indentation** — Auto-indent after `{`, auto-outdent on `}`
- **Word pattern** — Recognizes dotted identifiers (e.g., `Windows.Foundation.Uri`) as single words for double-click selection
- **Doc comment continuation** — Pressing Enter after `///` or inside `/** */` continues the comment prefix

---

## Configuration

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `midl.serverPath` | `string` | `"midl"` | Path to the MIDL CLI executable. The extension launches this with `--lsp` to start the language server over stdio. |

---

## Getting Started

### 1. Install the Extension

Install **MIDL Language Support** from the VS Code marketplace or from a `.vsix` package.

### 2. Build the MIDL Compiler

```bash
cd E:\midl
dotnet build
```

### 3. Configure the Server Path

Open VS Code settings (**Ctrl+,**) and set the path to the compiled MIDL CLI binary:

```json
{
    "midl.serverPath": "E:\\midl\\src\\Midl.Cli\\bin\\Debug\\net10.0\\Midl.Cli.exe"
}
```

### 4. Open a `.idl` File

The extension activates automatically when you open any `.idl` file. All features — syntax highlighting, diagnostics, completions, hover, and more — are available immediately.

---

## Requirements

- **.NET 10 SDK** — Required to build and run the MIDL language server
- **VS Code 1.85+** — Minimum supported VS Code version

---

## Known Limitations

- **Single-file analysis** — The language server analyzes each file independently; cross-file imports are not yet resolved in the LSP
- **Go to Definition** — Works within the current file only; external `.winmd` types cannot be navigated to
- **Full document sync** — The entire document is re-sent and reparsed on every keystroke (incremental text sync planned)
- **No rename/refactoring** — Symbol rename and code actions are not yet implemented
- **No code folding from LSP** — Folding relies on the regex-based language configuration, not semantic ranges
- **External type completions** — `.winmd` files are only discovered in the same directory as the open file

---

## Development

To work on the extension itself:

1. Open `E:\midl\editors\vscode` in VS Code
2. Run `npm install` to install dependencies
3. Press **F5** to launch the Extension Development Host (uses the `Launch Extension` debug configuration)
4. Edit `.idl` files in the new window to test

### Building

```bash
npm run compile    # One-time TypeScript build
npm run watch      # Watch mode for development
npm run package    # Package as .vsix
```

---

## License

See the [repository](https://github.com/microsoft/midl) for license information.
