/* Header file automatically generated from parity_large_enum.idl */
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
#ifndef __parity_large_enum_h__
#define __parity_large_enum_h__
#ifndef __parity_large_enum_p_h__
#define __parity_large_enum_p_h__


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
 * Struct TestApp.Parity.LargeEnum
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum] */
        enum LargeEnum : int
        {
            LargeEnum_Value0 = 0,
            LargeEnum_Value1 = 1,
            LargeEnum_Value2 = 2,
            LargeEnum_Value3 = 3,
            LargeEnum_Value4 = 4,
            LargeEnum_Value5 = 5,
            LargeEnum_Value6 = 6,
            LargeEnum_Value7 = 7,
            LargeEnum_Value8 = 8,
            LargeEnum_Value9 = 9,
            LargeEnum_Value10 = 10,
            LargeEnum_Value11 = 11,
            LargeEnum_Value12 = 12,
            LargeEnum_Value13 = 13,
            LargeEnum_Value14 = 14,
            LargeEnum_Value15 = 15,
            LargeEnum_Value16 = 16,
            LargeEnum_Value17 = 17,
            LargeEnum_Value18 = 18,
            LargeEnum_Value19 = 19,
            LargeEnum_Value20 = 20,
            LargeEnum_Value21 = 21,
            LargeEnum_Value22 = 22,
            LargeEnum_Value23 = 23,
            LargeEnum_Value24 = 24,
            LargeEnum_Value25 = 25,
            LargeEnum_Value26 = 26,
            LargeEnum_Value27 = 27,
            LargeEnum_Value28 = 28,
            LargeEnum_Value29 = 29,
            LargeEnum_Value30 = 30,
            LargeEnum_Value31 = 31,
            LargeEnum_Value32 = 32,
            LargeEnum_Value33 = 33,
            LargeEnum_Value34 = 34,
            LargeEnum_Value35 = 35,
            LargeEnum_Value36 = 36,
            LargeEnum_Value37 = 37,
            LargeEnum_Value38 = 38,
            LargeEnum_Value39 = 39,
            LargeEnum_Value40 = 40,
            LargeEnum_Value41 = 41,
            LargeEnum_Value42 = 42,
            LargeEnum_Value43 = 43,
            LargeEnum_Value44 = 44,
            LargeEnum_Value45 = 45,
            LargeEnum_Value46 = 46,
            LargeEnum_Value47 = 47,
            LargeEnum_Value48 = 48,
            LargeEnum_Value49 = 49,
            LargeEnum_Value50 = 50,
            LargeEnum_Value51 = 51,
            LargeEnum_Value52 = 52,
            LargeEnum_Value53 = 53,
            LargeEnum_Value54 = 54,
        };
        
    } /* Parity */
} /* TestApp */


#else // !defined(__cplusplus)
/* Forward Declarations */

/*
 *
 * Struct TestApp.Parity.LargeEnum
 *
 */

/* [version, v1_enum] */
enum __x_TestApp_CParity_CLargeEnum
{
    LargeEnum_Value0 = 0,
    LargeEnum_Value1 = 1,
    LargeEnum_Value2 = 2,
    LargeEnum_Value3 = 3,
    LargeEnum_Value4 = 4,
    LargeEnum_Value5 = 5,
    LargeEnum_Value6 = 6,
    LargeEnum_Value7 = 7,
    LargeEnum_Value8 = 8,
    LargeEnum_Value9 = 9,
    LargeEnum_Value10 = 10,
    LargeEnum_Value11 = 11,
    LargeEnum_Value12 = 12,
    LargeEnum_Value13 = 13,
    LargeEnum_Value14 = 14,
    LargeEnum_Value15 = 15,
    LargeEnum_Value16 = 16,
    LargeEnum_Value17 = 17,
    LargeEnum_Value18 = 18,
    LargeEnum_Value19 = 19,
    LargeEnum_Value20 = 20,
    LargeEnum_Value21 = 21,
    LargeEnum_Value22 = 22,
    LargeEnum_Value23 = 23,
    LargeEnum_Value24 = 24,
    LargeEnum_Value25 = 25,
    LargeEnum_Value26 = 26,
    LargeEnum_Value27 = 27,
    LargeEnum_Value28 = 28,
    LargeEnum_Value29 = 29,
    LargeEnum_Value30 = 30,
    LargeEnum_Value31 = 31,
    LargeEnum_Value32 = 32,
    LargeEnum_Value33 = 33,
    LargeEnum_Value34 = 34,
    LargeEnum_Value35 = 35,
    LargeEnum_Value36 = 36,
    LargeEnum_Value37 = 37,
    LargeEnum_Value38 = 38,
    LargeEnum_Value39 = 39,
    LargeEnum_Value40 = 40,
    LargeEnum_Value41 = 41,
    LargeEnum_Value42 = 42,
    LargeEnum_Value43 = 43,
    LargeEnum_Value44 = 44,
    LargeEnum_Value45 = 45,
    LargeEnum_Value46 = 46,
    LargeEnum_Value47 = 47,
    LargeEnum_Value48 = 48,
    LargeEnum_Value49 = 49,
    LargeEnum_Value50 = 50,
    LargeEnum_Value51 = 51,
    LargeEnum_Value52 = 52,
    LargeEnum_Value53 = 53,
    LargeEnum_Value54 = 54,
};


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_large_enum_p_h__

#endif // __parity_large_enum_h__
