/* Header file automatically generated from parity_enums_flags.idl */
/*
 * File built with Microsoft(R) MIDLRT Compiler Engine Version 10.00.0231 
 */

#pragma warning( disable: 4049 )  /* more than 64k source lines */

/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 500
#endif

/* verify that the <rpcsal.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCSAL_H_VERSION__
#define __REQUIRED_RPCSAL_H_VERSION__ 100
#endif

#include <rpc.h>
#include <rpcndr.h>

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif /* __RPCNDR_H_VERSION__ */

#ifndef COM_NO_WINDOWS_H
#include <windows.h>
#include <ole2.h>
#endif /*COM_NO_WINDOWS_H*/
#ifndef __parity_enums_flags_h__
#define __parity_enums_flags_h__
#ifndef __parity_enums_flags_p_h__
#define __parity_enums_flags_p_h__


#pragma once

// Ensure that the setting of the /ns_prefix command line switch is consistent for all headers.
// If you get an error from the compiler indicating "warning C4005: 'CHECK_NS_PREFIX_STATE': macro redefinition", this
// indicates that you have included two different headers with different settings for the /ns_prefix MIDL command line switch
#if !defined(DISABLE_NS_PREFIX_CHECKS)
#define CHECK_NS_PREFIX_STATE "never"
#endif // !defined(DISABLE_NS_PREFIX_CHECKS)


#pragma push_macro("MIDL_CONST_ID")
#undef MIDL_CONST_ID
#define MIDL_CONST_ID const __declspec(selectany)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */


/*
 *
 * Struct TestApp.Parity.Color
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum] */
        enum Color : int
        {
            Color_Red,
            Color_Green,
            Color_Blue,
            Color_Yellow,
            Color_Cyan,
            Color_Magenta,
            Color_White,
            Color_Black,
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.Priority
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum] */
        enum Priority : int
        {
            Priority_Lowest = 0,
            Priority_Low = 1,
            Priority_Normal = 2,
            Priority_High = 3,
            Priority_Highest = 4,
            Priority_Critical = 5,
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.FileAccess
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum, flags] */
        enum FileAccess : unsigned int
        {
            FileAccess_Read = 0x1,
            FileAccess_Write = 0x2,
            FileAccess_Execute = 0x4,
            FileAccess_ReadWrite = 0x3,
            FileAccess_ReadExecute = 0x5,
            FileAccess_WriteExecute = 0x6,
            FileAccess_All = 0x7,
        };
        
        DEFINE_ENUM_FLAG_OPERATORS(FileAccess)
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.DayOfWeek
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum, flags] */
        enum DayOfWeek : unsigned int
        {
            DayOfWeek_None = 0,
            DayOfWeek_Monday = 0x1,
            DayOfWeek_Tuesday = 0x2,
            DayOfWeek_Wednesday = 0x4,
            DayOfWeek_Thursday = 0x8,
            DayOfWeek_Friday = 0x10,
            DayOfWeek_Saturday = 0x20,
            DayOfWeek_Sunday = 0x40,
            DayOfWeek_Weekday = 0x1f,
            DayOfWeek_Weekend = 0x60,
            DayOfWeek_All = 0x7f,
        };
        
        DEFINE_ENUM_FLAG_OPERATORS(DayOfWeek)
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.Planet
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum] */
        enum Planet : int
        {
            Planet_Mercury = 1,
            Planet_Venus = 2,
            Planet_Earth = 3,
            Planet_Mars = 4,
            Planet_Jupiter = 5,
            Planet_Saturn = 6,
            Planet_Uranus = 7,
            Planet_Neptune = 8,
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.RenderOptions
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum, flags] */
        enum RenderOptions : unsigned int
        {
            RenderOptions_None = 0,
            RenderOptions_Antialias = 0x1,
            RenderOptions_HighQuality = 0x2,
            RenderOptions_UseCache = 0x4,
            RenderOptions_Animate = 0x8,
            RenderOptions_AllEffects = 0xf,
        };
        
        DEFINE_ENUM_FLAG_OPERATORS(RenderOptions)
        
    } /* Parity */
} /* TestApp */


#else // !defined(__cplusplus)
/* Forward Declarations */

/*
 *
 * Struct TestApp.Parity.Color
 *
 */

/* [version, v1_enum] */
enum __x_TestApp_CParity_CColor
{
    Color_Red,
    Color_Green,
    Color_Blue,
    Color_Yellow,
    Color_Cyan,
    Color_Magenta,
    Color_White,
    Color_Black,
};


/*
 *
 * Struct TestApp.Parity.Priority
 *
 */

/* [version, v1_enum] */
enum __x_TestApp_CParity_CPriority
{
    Priority_Lowest = 0,
    Priority_Low = 1,
    Priority_Normal = 2,
    Priority_High = 3,
    Priority_Highest = 4,
    Priority_Critical = 5,
};


/*
 *
 * Struct TestApp.Parity.FileAccess
 *
 */

/* [version, v1_enum, flags] */
enum __x_TestApp_CParity_CFileAccess
{
    FileAccess_Read = 0x1,
    FileAccess_Write = 0x2,
    FileAccess_Execute = 0x4,
    FileAccess_ReadWrite = 0x3,
    FileAccess_ReadExecute = 0x5,
    FileAccess_WriteExecute = 0x6,
    FileAccess_All = 0x7,
};


/*
 *
 * Struct TestApp.Parity.DayOfWeek
 *
 */

/* [version, v1_enum, flags] */
enum __x_TestApp_CParity_CDayOfWeek
{
    DayOfWeek_None = 0,
    DayOfWeek_Monday = 0x1,
    DayOfWeek_Tuesday = 0x2,
    DayOfWeek_Wednesday = 0x4,
    DayOfWeek_Thursday = 0x8,
    DayOfWeek_Friday = 0x10,
    DayOfWeek_Saturday = 0x20,
    DayOfWeek_Sunday = 0x40,
    DayOfWeek_Weekday = 0x1f,
    DayOfWeek_Weekend = 0x60,
    DayOfWeek_All = 0x7f,
};


/*
 *
 * Struct TestApp.Parity.Planet
 *
 */

/* [version, v1_enum] */
enum __x_TestApp_CParity_CPlanet
{
    Planet_Mercury = 1,
    Planet_Venus = 2,
    Planet_Earth = 3,
    Planet_Mars = 4,
    Planet_Jupiter = 5,
    Planet_Saturn = 6,
    Planet_Uranus = 7,
    Planet_Neptune = 8,
};


/*
 *
 * Struct TestApp.Parity.RenderOptions
 *
 */

/* [version, v1_enum, flags] */
enum __x_TestApp_CParity_CRenderOptions
{
    RenderOptions_None = 0,
    RenderOptions_Antialias = 0x1,
    RenderOptions_HighQuality = 0x2,
    RenderOptions_UseCache = 0x4,
    RenderOptions_Animate = 0x8,
    RenderOptions_AllEffects = 0xf,
};


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_enums_flags_p_h__

#endif // __parity_enums_flags_h__
