/* Header file automatically generated from parity_nullable_types.idl */
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
#ifndef __parity_nullable_types_h__
#define __parity_nullable_types_h__
#ifndef __parity_nullable_types_p_h__
#define __parity_nullable_types_p_h__


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
// Importing Collections header
#include <windows.foundation.collections.h>

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CINullableWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CINullableWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface INullableWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CINullableWidget TestApp::Parity::INullableWidget

#endif // ____x_TestApp_CParity_CINullableWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

#ifndef DEF___FIReference_1_int_USE
#define DEF___FIReference_1_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("548cefbd-bc8a-5fa0-8df2-957440fc8bf4"))
IReference<int> : IReference_impl<int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IReference`1<Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IReference<int> __FIReference_1_int_t;
#define __FIReference_1_int Windows::Foundation::__FIReference_1_int_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIReference_1_int Windows::Foundation::IReference<int>
//#define __FIReference_1_int_t Windows::Foundation::IReference<int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIReference_1_int_USE */




#ifndef DEF___FIReference_1_boolean_USE
#define DEF___FIReference_1_boolean_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("3c00fd60-2950-5939-a21a-2d12c5a01b8a"))
IReference<bool> : IReference_impl<Windows::Foundation::Internal::AggregateType<bool, boolean>> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IReference`1<Boolean>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IReference<bool> __FIReference_1_boolean_t;
#define __FIReference_1_boolean Windows::Foundation::__FIReference_1_boolean_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIReference_1_boolean Windows::Foundation::IReference<boolean>
//#define __FIReference_1_boolean_t Windows::Foundation::IReference<boolean>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIReference_1_boolean_USE */




#ifndef DEF___FIReference_1_double_USE
#define DEF___FIReference_1_double_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("2f2d6c29-5473-5f3e-92e7-96572bb990e2"))
IReference<double> : IReference_impl<double> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IReference`1<Double>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IReference<double> __FIReference_1_double_t;
#define __FIReference_1_double Windows::Foundation::__FIReference_1_double_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIReference_1_double Windows::Foundation::IReference<double>
//#define __FIReference_1_double_t Windows::Foundation::IReference<double>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIReference_1_double_USE */




#ifndef DEF___FIReference_1_float_USE
#define DEF___FIReference_1_float_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("719cc2ba-3e76-5def-9f1a-38d85a145ea8"))
IReference<float> : IReference_impl<float> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IReference`1<Single>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IReference<float> __FIReference_1_float_t;
#define __FIReference_1_float Windows::Foundation::__FIReference_1_float_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIReference_1_float Windows::Foundation::IReference<float>
//#define __FIReference_1_float_t Windows::Foundation::IReference<float>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIReference_1_float_USE */




#ifndef DEF___FIReference_1_GUID_USE
#define DEF___FIReference_1_GUID_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("7d50f649-632c-51f9-849a-ee49428933ea"))
IReference<GUID> : IReference_impl<GUID> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IReference`1<Guid>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IReference<GUID> __FIReference_1_GUID_t;
#define __FIReference_1_GUID Windows::Foundation::__FIReference_1_GUID_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIReference_1_GUID Windows::Foundation::IReference<GUID>
//#define __FIReference_1_GUID_t Windows::Foundation::IReference<GUID>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIReference_1_GUID_USE */






namespace TestApp {
    namespace Parity {
        class NullableWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.INullableWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.NullableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.INullableWidget:HRESULT get_NullableInt(Windows.Foundation.IReference`1<Int32>**);HRESULT put_NullableInt(Windows.Foundation.IReference`1<Int32>*);HRESULT get_NullableBool(Windows.Foundation.IReference`1<Boolean>**);HRESULT put_NullableBool(Windows.Foundation.IReference`1<Boolean>*);HRESULT get_NullableDouble(Windows.Foundation.IReference`1<Double>**);HRESULT put_NullableDouble(Windows.Foundation.IReference`1<Double>*);HRESULT get_NullableSingle(Windows.Foundation.IReference`1<Single>**);HRESULT put_NullableSingle(Windows.Foundation.IReference`1<Single>*);HRESULT get_NullableGuid(Windows.Foundation.IReference`1<Guid>**);HRESULT SetNullableValue(Windows.Foundation.IReference`1<Int32>*);HRESULT GetNullableValue(Windows.Foundation.IReference`1<Int32>**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CINullableWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CINullableWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_INullableWidget[] = L"TestApp.Parity.INullableWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("28249459-e43a-5d74-ac46-2451d59c7b4f"), version, object, exclusiveto] */
        MIDL_INTERFACE("28249459-e43a-5d74-ac46-2451d59c7b4f")
        INullableWidget : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NullableInt(
                /* [out, retval] */__FIReference_1_int * * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_NullableInt(
                /* [in] */__FIReference_1_int * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NullableBool(
                /* [out, retval] */__FIReference_1_boolean * * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_NullableBool(
                /* [in] */__FIReference_1_boolean * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NullableDouble(
                /* [out, retval] */__FIReference_1_double * * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_NullableDouble(
                /* [in] */__FIReference_1_double * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NullableSingle(
                /* [out, retval] */__FIReference_1_float * * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_NullableSingle(
                /* [in] */__FIReference_1_float * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NullableGuid(
                /* [out, retval] */__FIReference_1_GUID * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE SetNullableValue(
                /* [in] */__FIReference_1_int * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetNullableValue(
                /* [out, retval] */__FIReference_1_int * * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_INullableWidget=__uuidof(INullableWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CINullableWidget;
#endif /* !defined(____x_TestApp_CParity_CINullableWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.NullableWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.INullableWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_NullableWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_NullableWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_NullableWidget[] = L"TestApp.Parity.NullableWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CINullableWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CINullableWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CINullableWidget __x_TestApp_CParity_CINullableWidget;

#endif // ____x_TestApp_CParity_CINullableWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions
#if !defined(____FIReference_1_int_INTERFACE_DEFINED__)
#define ____FIReference_1_int_INTERFACE_DEFINED__

typedef interface __FIReference_1_int __FIReference_1_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIReference_1_int;

typedef struct __FIReference_1_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIReference_1_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIReference_1_int * This );
    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIReference_1_int * This );

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIReference_1_int * This, 
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( __RPC__in __FIReference_1_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( __RPC__in __FIReference_1_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIReference_1_int * This, /* [retval][out] */ __RPC__out int *value);
    END_INTERFACE
} __FIReference_1_intVtbl;

interface __FIReference_1_int
{
    CONST_VTBL struct __FIReference_1_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIReference_1_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIReference_1_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIReference_1_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIReference_1_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIReference_1_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIReference_1_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIReference_1_int_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIReference_1_int_INTERFACE_DEFINED__


#if !defined(____FIReference_1_boolean_INTERFACE_DEFINED__)
#define ____FIReference_1_boolean_INTERFACE_DEFINED__

typedef interface __FIReference_1_boolean __FIReference_1_boolean;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIReference_1_boolean;

typedef struct __FIReference_1_booleanVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIReference_1_boolean * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIReference_1_boolean * This );
    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIReference_1_boolean * This );

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIReference_1_boolean * This, 
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( __RPC__in __FIReference_1_boolean * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( __RPC__in __FIReference_1_boolean * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIReference_1_boolean * This, /* [retval][out] */ __RPC__out boolean *value);
    END_INTERFACE
} __FIReference_1_booleanVtbl;

interface __FIReference_1_boolean
{
    CONST_VTBL struct __FIReference_1_booleanVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIReference_1_boolean_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIReference_1_boolean_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIReference_1_boolean_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIReference_1_boolean_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIReference_1_boolean_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIReference_1_boolean_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIReference_1_boolean_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIReference_1_boolean_INTERFACE_DEFINED__


#if !defined(____FIReference_1_double_INTERFACE_DEFINED__)
#define ____FIReference_1_double_INTERFACE_DEFINED__

typedef interface __FIReference_1_double __FIReference_1_double;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIReference_1_double;

typedef struct __FIReference_1_doubleVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIReference_1_double * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIReference_1_double * This );
    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIReference_1_double * This );

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIReference_1_double * This, 
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( __RPC__in __FIReference_1_double * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( __RPC__in __FIReference_1_double * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIReference_1_double * This, /* [retval][out] */ __RPC__out double *value);
    END_INTERFACE
} __FIReference_1_doubleVtbl;

interface __FIReference_1_double
{
    CONST_VTBL struct __FIReference_1_doubleVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIReference_1_double_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIReference_1_double_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIReference_1_double_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIReference_1_double_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIReference_1_double_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIReference_1_double_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIReference_1_double_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIReference_1_double_INTERFACE_DEFINED__


#if !defined(____FIReference_1_float_INTERFACE_DEFINED__)
#define ____FIReference_1_float_INTERFACE_DEFINED__

typedef interface __FIReference_1_float __FIReference_1_float;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIReference_1_float;

typedef struct __FIReference_1_floatVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIReference_1_float * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIReference_1_float * This );
    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIReference_1_float * This );

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIReference_1_float * This, 
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( __RPC__in __FIReference_1_float * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( __RPC__in __FIReference_1_float * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIReference_1_float * This, /* [retval][out] */ __RPC__out float *value);
    END_INTERFACE
} __FIReference_1_floatVtbl;

interface __FIReference_1_float
{
    CONST_VTBL struct __FIReference_1_floatVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIReference_1_float_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIReference_1_float_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIReference_1_float_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIReference_1_float_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIReference_1_float_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIReference_1_float_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIReference_1_float_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIReference_1_float_INTERFACE_DEFINED__


#if !defined(____FIReference_1_GUID_INTERFACE_DEFINED__)
#define ____FIReference_1_GUID_INTERFACE_DEFINED__

typedef interface __FIReference_1_GUID __FIReference_1_GUID;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIReference_1_GUID;

typedef struct __FIReference_1_GUIDVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIReference_1_GUID * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIReference_1_GUID * This );
    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIReference_1_GUID * This );

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIReference_1_GUID * This, 
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( __RPC__in __FIReference_1_GUID * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( __RPC__in __FIReference_1_GUID * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIReference_1_GUID * This, /* [retval][out] */ __RPC__out GUID *value);
    END_INTERFACE
} __FIReference_1_GUIDVtbl;

interface __FIReference_1_GUID
{
    CONST_VTBL struct __FIReference_1_GUIDVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIReference_1_GUID_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIReference_1_GUID_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIReference_1_GUID_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIReference_1_GUID_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIReference_1_GUID_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIReference_1_GUID_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIReference_1_GUID_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIReference_1_GUID_INTERFACE_DEFINED__





/*
 *
 * Interface TestApp.Parity.INullableWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.NullableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.INullableWidget:HRESULT get_NullableInt(Windows.Foundation.IReference`1<Int32>**);HRESULT put_NullableInt(Windows.Foundation.IReference`1<Int32>*);HRESULT get_NullableBool(Windows.Foundation.IReference`1<Boolean>**);HRESULT put_NullableBool(Windows.Foundation.IReference`1<Boolean>*);HRESULT get_NullableDouble(Windows.Foundation.IReference`1<Double>**);HRESULT put_NullableDouble(Windows.Foundation.IReference`1<Double>*);HRESULT get_NullableSingle(Windows.Foundation.IReference`1<Single>**);HRESULT put_NullableSingle(Windows.Foundation.IReference`1<Single>*);HRESULT get_NullableGuid(Windows.Foundation.IReference`1<Guid>**);HRESULT SetNullableValue(Windows.Foundation.IReference`1<Int32>*);HRESULT GetNullableValue(Windows.Foundation.IReference`1<Int32>**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CINullableWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CINullableWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_INullableWidget[] = L"TestApp.Parity.INullableWidget";
/* [uuid("28249459-e43a-5d74-ac46-2451d59c7b4f"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CINullableWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CINullableWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CINullableWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CINullableWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CINullableWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CINullableWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CINullableWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NullableInt )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [out, retval] */__FIReference_1_int * * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_NullableInt )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [in] */__FIReference_1_int * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NullableBool )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [out, retval] */__FIReference_1_boolean * * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_NullableBool )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [in] */__FIReference_1_boolean * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NullableDouble )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [out, retval] */__FIReference_1_double * * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_NullableDouble )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [in] */__FIReference_1_double * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NullableSingle )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [out, retval] */__FIReference_1_float * * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_NullableSingle )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [in] */__FIReference_1_float * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NullableGuid )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [out, retval] */__FIReference_1_GUID * * value
        );
    HRESULT ( STDMETHODCALLTYPE *SetNullableValue )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [in] */__FIReference_1_int * value
        );
    HRESULT ( STDMETHODCALLTYPE *GetNullableValue )(
        __x_TestApp_CParity_CINullableWidget * This,
        /* [out, retval] */__FIReference_1_int * * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CINullableWidgetVtbl;

interface __x_TestApp_CParity_CINullableWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CINullableWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CINullableWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CINullableWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CINullableWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CINullableWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CINullableWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CINullableWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CINullableWidget_get_NullableInt(This,value) \
    ( (This)->lpVtbl->get_NullableInt(This,value) )

#define __x_TestApp_CParity_CINullableWidget_put_NullableInt(This,value) \
    ( (This)->lpVtbl->put_NullableInt(This,value) )

#define __x_TestApp_CParity_CINullableWidget_get_NullableBool(This,value) \
    ( (This)->lpVtbl->get_NullableBool(This,value) )

#define __x_TestApp_CParity_CINullableWidget_put_NullableBool(This,value) \
    ( (This)->lpVtbl->put_NullableBool(This,value) )

#define __x_TestApp_CParity_CINullableWidget_get_NullableDouble(This,value) \
    ( (This)->lpVtbl->get_NullableDouble(This,value) )

#define __x_TestApp_CParity_CINullableWidget_put_NullableDouble(This,value) \
    ( (This)->lpVtbl->put_NullableDouble(This,value) )

#define __x_TestApp_CParity_CINullableWidget_get_NullableSingle(This,value) \
    ( (This)->lpVtbl->get_NullableSingle(This,value) )

#define __x_TestApp_CParity_CINullableWidget_put_NullableSingle(This,value) \
    ( (This)->lpVtbl->put_NullableSingle(This,value) )

#define __x_TestApp_CParity_CINullableWidget_get_NullableGuid(This,value) \
    ( (This)->lpVtbl->get_NullableGuid(This,value) )

#define __x_TestApp_CParity_CINullableWidget_SetNullableValue(This,value) \
    ( (This)->lpVtbl->SetNullableValue(This,value) )

#define __x_TestApp_CParity_CINullableWidget_GetNullableValue(This,result) \
    ( (This)->lpVtbl->GetNullableValue(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CINullableWidget;
#endif /* !defined(____x_TestApp_CParity_CINullableWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.NullableWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.INullableWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_NullableWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_NullableWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_NullableWidget[] = L"TestApp.Parity.NullableWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_nullable_types_p_h__

#endif // __parity_nullable_types_h__
