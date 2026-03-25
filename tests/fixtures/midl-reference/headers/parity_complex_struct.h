/* Header file automatically generated from parity_complex_struct.idl */
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
#ifndef __parity_complex_struct_h__
#define __parity_complex_struct_h__
#ifndef __parity_complex_struct_p_h__
#define __parity_complex_struct_p_h__


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
 * Struct TestApp.Parity.RecordStatus
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum] */
        enum RecordStatus : int
        {
            RecordStatus_Active = 0,
            RecordStatus_Inactive = 1,
            RecordStatus_Pending = 2,
            RecordStatus_Archived = 3,
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.Point2D
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct Point2D
        {
            float X;
            float Y;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.Point3D
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct Point3D
        {
            double X;
            double Y;
            double Z;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.Dimensions
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct Dimensions
        {
            double Width;
            double Height;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.Rect
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct Rect
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
 * Struct TestApp.Parity.Color4
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct Color4
        {
            float R;
            float G;
            float B;
            float A;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.ComplexRecord
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct ComplexRecord
        {
            int Id;
            unsigned int Flags;
            __int64 Timestamp;
            unsigned __int64 Size;
            short SmallValue;
            unsigned short MediumValue;
            ::byte TinyValue;
            float Ratio;
            double Precision;
            ::boolean IsValid;
            HSTRING Name;
            HSTRING Description;
            GUID UniqueId;
            struct TestApp::Parity::Point2D Location;
            struct TestApp::Parity::Dimensions Bounds;
            TestApp::Parity::RecordStatus Status;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.NestedStruct
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct NestedStruct
        {
            struct TestApp::Parity::ComplexRecord Record;
            struct TestApp::Parity::Point3D Position;
            struct TestApp::Parity::Color4 Tint;
            struct TestApp::Parity::Rect ClipBounds;
        };
        
    } /* Parity */
} /* TestApp */


#else // !defined(__cplusplus)
/* Forward Declarations */

/*
 *
 * Struct TestApp.Parity.RecordStatus
 *
 */

/* [version, v1_enum] */
enum __x_TestApp_CParity_CRecordStatus
{
    RecordStatus_Active = 0,
    RecordStatus_Inactive = 1,
    RecordStatus_Pending = 2,
    RecordStatus_Archived = 3,
};


/*
 *
 * Struct TestApp.Parity.Point2D
 *
 */

/* [version] */
struct __x_TestApp_CParity_CPoint2D
{
    float X;
    float Y;
};


/*
 *
 * Struct TestApp.Parity.Point3D
 *
 */

/* [version] */
struct __x_TestApp_CParity_CPoint3D
{
    double X;
    double Y;
    double Z;
};


/*
 *
 * Struct TestApp.Parity.Dimensions
 *
 */

/* [version] */
struct __x_TestApp_CParity_CDimensions
{
    double Width;
    double Height;
};


/*
 *
 * Struct TestApp.Parity.Rect
 *
 */

/* [version] */
struct __x_TestApp_CParity_CRect
{
    float X;
    float Y;
    float Width;
    float Height;
};


/*
 *
 * Struct TestApp.Parity.Color4
 *
 */

/* [version] */
struct __x_TestApp_CParity_CColor4
{
    float R;
    float G;
    float B;
    float A;
};


/*
 *
 * Struct TestApp.Parity.ComplexRecord
 *
 */

/* [version] */
struct __x_TestApp_CParity_CComplexRecord
{
    int Id;
    unsigned int Flags;
    __int64 Timestamp;
    unsigned __int64 Size;
    short SmallValue;
    unsigned short MediumValue;
    byte TinyValue;
    float Ratio;
    double Precision;
    boolean IsValid;
    HSTRING Name;
    HSTRING Description;
    GUID UniqueId;
    struct __x_TestApp_CParity_CPoint2D Location;
    struct __x_TestApp_CParity_CDimensions Bounds;
    enum __x_TestApp_CParity_CRecordStatus Status;
};


/*
 *
 * Struct TestApp.Parity.NestedStruct
 *
 */

/* [version] */
struct __x_TestApp_CParity_CNestedStruct
{
    struct __x_TestApp_CParity_CComplexRecord Record;
    struct __x_TestApp_CParity_CPoint3D Position;
    struct __x_TestApp_CParity_CColor4 Tint;
    struct __x_TestApp_CParity_CRect ClipBounds;
};


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_complex_struct_p_h__

#endif // __parity_complex_struct_h__
