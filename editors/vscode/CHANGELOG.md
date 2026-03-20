# Changelog

All notable changes to the MIDL Language Support extension will be documented in this file.

## 0.0.1 (Initial Release)

### Features

- **TextMate grammar** for MIDL3 and classic MIDL syntax highlighting — keywords, types, attributes, GUIDs, strings, numbers, comments, qualified type names, and storage modifiers
- **Language Server Protocol integration** with full semantic analysis via the MIDL compiler (`--lsp` mode over stdio)
- **19 code snippets** for common MIDL3 patterns — `namespace`, `runtimeclass`, `unsealedruntimeclass`, `staticruntimeclass`, `interface`, `interfacerequires`, `delegate`, `struct`, `enum`, `flagsenum`, `propget`, `proprw`, `event`, `asyncmethod`, `asyncaction`, `contract`, `apicontract`, `versionedblock`, `import`
- **Real-time diagnostics** — errors and warnings published as you type, powered by the MIDL semantic analysis engine
- **IntelliSense completions** — keywords, WinRT primitive types, common collection/async types, and external types from `.winmd` files in the workspace
- **Hover information** — keyword documentation, symbol kind and fully-qualified name for user-defined types, source `.winmd` path for external types
- **Go to Definition** — navigate to type and member definitions within the current file (Ctrl+Click / F12)
- **Find All References** — locate all usages of a symbol across the document (Shift+F12)
- **Signature Help** — parameter hints for method calls, constructors, and delegates (triggered by `(` and `,`)
- **Semantic token highlighting** — 14 token types (`namespace`, `type`, `interface`, `enum`, `enumMember`, `method`, `property`, `event`, `parameter`, `keyword`, `comment`, `string`, `number`, `struct`) with symbol-aware classification
- **Document formatting** — consistent 4-space indentation, normalized spacing around colons and commas, blank lines between declarations, trailing whitespace removal
- **Document outline / symbols** — hierarchical namespace → type → member tree in the Outline view and Breadcrumbs
- **Language configuration** — auto-closing pairs, bracket matching, comment toggling, smart indentation, code folding, doc comment continuation, dotted-identifier word pattern
- **Configurable server path** — `midl.serverPath` setting to point to the MIDL CLI executable
