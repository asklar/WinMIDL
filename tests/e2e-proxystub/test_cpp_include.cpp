// Validates the generated header compiles in C++ mode
#include <windows.h>
#include <unknwn.h>
#include <hstring.h>
#include "TestWidget.h"

// Verify interface type is accessible in C++ (inside TestApp namespace)
static_assert(sizeof(TestApp::IWidget) > 0, "Interface type should be defined");

// Verify enum is accessible (at global scope, per emitter output)
static_assert(static_cast<int>(Priority_Low) == 0, "Enum should start at 0");

// Verify struct is accessible (at global scope, per emitter output)
static_assert(sizeof(Point) == 2 * sizeof(float), "Point should have 2 floats");

// Verify IID is declared
EXTERN_C const IID IID___x_TestApp_CIWidget;

int main() {
    return 0;
}
