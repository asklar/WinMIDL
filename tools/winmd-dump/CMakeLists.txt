cmake_minimum_required(VERSION 3.18)
project(winmd-dump CXX)

# The dumper depends on the header-only WinMD reader from microsoft/winmd.
# Fetch it at configure time so no local clone or absolute path is required.
include(FetchContent)
FetchContent_Declare(
    winmd
    GIT_REPOSITORY https://github.com/microsoft/winmd.git
    GIT_TAG        a363c851a65c4e15743d6589acc1723e9e21b143
)
FetchContent_MakeAvailable(winmd)

add_executable(winmd-dump main.cpp)
target_include_directories(winmd-dump PRIVATE ${winmd_SOURCE_DIR}/src)
target_compile_features(winmd-dump PRIVATE cxx_std_20)
