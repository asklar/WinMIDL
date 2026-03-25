/* Header file automatically generated from parity_structs.idl */
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
#ifndef __parity_structs_h__
#define __parity_structs_h__
#ifndef __parity_structs_p_h__
#define __parity_structs_p_h__


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
 * Struct TestApp.Parity.SimpleRect
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct SimpleRect
        {
            float X;
            float Y;
            float Width;
            float Height;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.MixedFields
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct MixedFields
        {
            HSTRING Name;
            int Id;
            unsigned int Flags;
            double Value;
            ::boolean IsActive;
            __int64 Timestamp;
            ::byte Priority;
            GUID Token;
        };
        
    } /* Parity */
} /* TestApp */


#else // !defined(__cplusplus)
/* Forward Declarations */

/*
 *
 * Struct TestApp.Parity.SimpleRect
 *
 */

/* [version] */
struct __x_TestApp_CParity_CSimpleRect
{
    float X;
    float Y;
    float Width;
    float Height;
};


/*
 *
 * Struct TestApp.Parity.MixedFields
 *
 */

/* [version] */
struct __x_TestApp_CParity_CMixedFields
{
    HSTRING Name;
    int Id;
    unsigned int Flags;
    double Value;
    boolean IsActive;
    __int64 Timestamp;
    byte Priority;
    GUID Token;
};


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_structs_p_h__

#endif // __parity_structs_h__
