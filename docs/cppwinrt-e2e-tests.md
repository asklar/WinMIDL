# Design: C++/WinRT E2E Tests

## Goal

Validate that our emitted `.winmd` files work correctly with `cppwinrt` projections — not just raw COM. This will surface any metadata gaps and prove end-to-end compatibility with the standard WinRT toolchain.

## Current State

Our E2E tests (`tests/e2e-custom-marshal/`) use raw COM/C++:
- Server implements `IUnknown`/`IInspectable` manually
- Client uses `CoCreateInstance` + raw interface pointers
- No `winrt::` types or C++/WinRT projections

This validates our proxy/stub and ABI header generation, but doesn't validate the `.winmd` → projection path that real-world consumers use.

## Proposed Flow

```
IDL → WinMIDL → .winmd + .h + _i.c + _proxy.cpp
                   ↓
               cppwinrt.exe → projection headers (winrt/MyNamespace.MyClass.h)
                   ↓
         Server (C++/WinRT authoring) + Client (C++/WinRT consumption)
                   ↓
         Build → Register → Run → Assert
```

## Prerequisites

- `cppwinrt.exe` — ships with Windows SDK (`%WindowsSdkDir%\bin\<version>\x64\cppwinrt.exe`) or via NuGet (`Microsoft.Windows.CppWinRT`)
- Our `.winmd` must contain standard WinRT metadata that cppwinrt can consume

## Test 1: In-proc with C++/WinRT

### IDL
A simple runtimeclass exercising common patterns:
```idl
namespace TestCppWinRT
{
    enum Priority { Low, Medium, High };
    
    struct Point { Single X; Single Y; };
    
    runtimeclass Widget
    {
        Widget();
        Widget(String name);
        
        String Name { get; set; };
        Priority Level { get; };
        
        Point GetPosition();
        void MoveTo(Point position);
        
        event Windows.Foundation.TypedEventHandler<Widget, String> NameChanged;
    };
}
```

### Server (C++/WinRT authoring)
```cpp
#include <winrt/TestCppWinRT.h>

namespace winrt::TestCppWinRT::implementation
{
    struct Widget : WidgetT<Widget>
    {
        Widget() = default;
        Widget(hstring const& name) : m_name(name) {}
        
        hstring Name() { return m_name; }
        void Name(hstring const& value) { m_name = value; m_nameChanged(*this, value); }
        
        Priority Level() { return m_level; }
        Point GetPosition() { return m_position; }
        void MoveTo(Point const& pos) { m_position = pos; }
        
        auto NameChanged(auto const& handler) { return m_nameChanged.add(handler); }
        void NameChanged(winrt::event_token token) { m_nameChanged.remove(token); }
        
    private:
        hstring m_name;
        Priority m_level = Priority::Medium;
        Point m_position{};
        winrt::event<Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring>> m_nameChanged;
    };
}
```

### Client (C++/WinRT consumption)
```cpp
#include <winrt/TestCppWinRT.h>

void RunTests()
{
    auto widget = winrt::TestCppWinRT::Widget();
    
    // Property
    widget.Name(L"TestWidget");
    assert(widget.Name() == L"TestWidget");
    
    // Struct
    widget.MoveTo({ 10.0f, 20.0f });
    auto pos = widget.GetPosition();
    assert(pos.X == 10.0f);
    
    // Event
    bool fired = false;
    auto token = widget.NameChanged([&](auto&&, auto&& name) { fired = true; });
    widget.Name(L"Changed");
    assert(fired);
    widget.NameChanged(token);
}
```

### Build steps
1. `Midl.Cli /winrt /winmd TestCppWinRT.winmd /h TestCppWinRT.h /iid TestCppWinRT_i.c TestCppWinRT.idl`
2. `cppwinrt -in TestCppWinRT.winmd -ref %WindowsSdkDir%\... -out generated/`
3. Compile server DLL against cppwinrt-generated headers
4. Compile client EXE against cppwinrt-generated headers
5. Register DLL, run client, assert results

## Test 2: Out-of-proc with C++/WinRT

Same IDL and patterns as Test 1, but:
- Server is an EXE with class factory registration
- Client uses `CLSCTX_LOCAL_SERVER`
- Validates cross-process marshaling with projected types
- Uses our proxy/stub DLL for marshaling

## Potential WinMD Compatibility Issues

| Issue | Detection | Fix |
|-------|-----------|-----|
| Missing `GuidAttribute` on interfaces | cppwinrt error: "type does not have a GUID" | Verify all interfaces get deterministic GUIDs |
| Missing `DefaultAttribute` on InterfaceImpl | Wrong default interface in projection | Check InterfaceImpl custom attributes |
| Missing `ExclusiveToAttribute` | Factory interfaces not wired correctly | Verify on synthesized interfaces |
| Missing `ActivatableAttribute` | No activation factory in projection | Verify on runtimeclasses with constructors |
| PropertyMap/EventMap row ordering | cppwinrt may crash or skip props/events | Verify metadata table ordering |
| TypeSpec for generic instantiations | Incorrect signatures for IVector\<T\> etc. | Check TypeSpec blob encoding |
| Method parameter names | Projected API has wrong parameter names | Verify Parameter table entries |

## Approach

1. **Step 1**: Run `cppwinrt -in our.winmd` and see what errors/warnings it produces
2. **Step 2**: Fix each WinMD gap iteratively
3. **Step 3**: Build the in-proc test once cppwinrt succeeds
4. **Step 4**: Build the out-of-proc test
5. **Step 5**: Integrate into `tests/Midl.E2E.Tests/` with `E2ETestHelper`

## Test Infrastructure

Add to `E2ETestHelper`:
- `FindCppWinRT()` — locate `cppwinrt.exe` from Windows SDK or PATH
- `GenerateProjection(winmdPath, outputDir)` — run cppwinrt to generate headers
- Skip tests if cppwinrt is not available

## Success Criteria

- `cppwinrt.exe` consumes our `.winmd` with no errors
- C++/WinRT authored server compiles and runs
- C++/WinRT client calls server through projected types
- Cross-apartment (in-proc) and cross-process (out-of-proc) both work
- Events, properties, structs, enums all round-trip correctly
