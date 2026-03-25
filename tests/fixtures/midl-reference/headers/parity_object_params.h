/* Header file automatically generated from parity_object_params.idl */
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
#ifndef __parity_object_params_h__
#define __parity_object_params_h__
#ifndef __parity_object_params_p_h__
#define __parity_object_params_p_h__


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
#ifndef ____x_TestApp_CParity_CIObjectWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIObjectWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IObjectWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIObjectWidget TestApp::Parity::IObjectWidget

#endif // ____x_TestApp_CParity_CIObjectWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions


#ifndef DEF___FIIterator_1_IInspectable_USE
#define DEF___FIIterator_1_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("44a94f2d-04f8-5091-b336-be7892dd10be"))
IIterator<IInspectable*> : IIterator_impl<IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<IInspectable*> __FIIterator_1_IInspectable_t;
#define __FIIterator_1_IInspectable Windows::Foundation::Collections::__FIIterator_1_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1_IInspectable Windows::Foundation::Collections::IIterator<IInspectable*>
//#define __FIIterator_1_IInspectable_t Windows::Foundation::Collections::IIterator<IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1_IInspectable_USE */





#ifndef DEF___FIIterable_1_IInspectable_USE
#define DEF___FIIterable_1_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("092b849b-60b1-52be-a44a-6fe8e933cbe4"))
IIterable<IInspectable*> : IIterable_impl<IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<IInspectable*> __FIIterable_1_IInspectable_t;
#define __FIIterable_1_IInspectable Windows::Foundation::Collections::__FIIterable_1_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1_IInspectable Windows::Foundation::Collections::IIterable<IInspectable*>
//#define __FIIterable_1_IInspectable_t Windows::Foundation::Collections::IIterable<IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1_IInspectable_USE */





#ifndef DEF___FIVectorView_1_IInspectable_USE
#define DEF___FIVectorView_1_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("a6487363-b074-5c60-ab16-866dce4ee54d"))
IVectorView<IInspectable*> : IVectorView_impl<IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVectorView`1<Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVectorView<IInspectable*> __FIVectorView_1_IInspectable_t;
#define __FIVectorView_1_IInspectable Windows::Foundation::Collections::__FIVectorView_1_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVectorView_1_IInspectable Windows::Foundation::Collections::IVectorView<IInspectable*>
//#define __FIVectorView_1_IInspectable_t Windows::Foundation::Collections::IVectorView<IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVectorView_1_IInspectable_USE */





#ifndef DEF___FIVector_1_IInspectable_USE
#define DEF___FIVector_1_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("b32bdca4-5e52-5b27-bc5d-d66a1a268c2a"))
IVector<IInspectable*> : IVector_impl<IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVector`1<Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVector<IInspectable*> __FIVector_1_IInspectable_t;
#define __FIVector_1_IInspectable Windows::Foundation::Collections::__FIVector_1_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVector_1_IInspectable Windows::Foundation::Collections::IVector<IInspectable*>
//#define __FIVector_1_IInspectable_t Windows::Foundation::Collections::IVector<IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVector_1_IInspectable_USE */





#ifndef DEF___FIKeyValuePair_2_HSTRING_IInspectable_USE
#define DEF___FIKeyValuePair_2_HSTRING_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("09335560-6c6b-5a26-9348-97b781132b20"))
IKeyValuePair<HSTRING,IInspectable*> : IKeyValuePair_impl<HSTRING,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IKeyValuePair`2<String, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IKeyValuePair<HSTRING,IInspectable*> __FIKeyValuePair_2_HSTRING_IInspectable_t;
#define __FIKeyValuePair_2_HSTRING_IInspectable Windows::Foundation::Collections::__FIKeyValuePair_2_HSTRING_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIKeyValuePair_2_HSTRING_IInspectable Windows::Foundation::Collections::IKeyValuePair<HSTRING,IInspectable*>
//#define __FIKeyValuePair_2_HSTRING_IInspectable_t Windows::Foundation::Collections::IKeyValuePair<HSTRING,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIKeyValuePair_2_HSTRING_IInspectable_USE */





#ifndef DEF___FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_USE
#define DEF___FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("5db5fa32-707c-5849-a06b-91c8eb9d10e8"))
IIterator<__FIKeyValuePair_2_HSTRING_IInspectable*> : IIterator_impl<__FIKeyValuePair_2_HSTRING_IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Object>>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<__FIKeyValuePair_2_HSTRING_IInspectable*> __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_t;
#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable Windows::Foundation::Collections::__FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable Windows::Foundation::Collections::IIterator<Windows::Foundation::Collections::IKeyValuePair<HSTRING,IInspectable*>*>
//#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_t Windows::Foundation::Collections::IIterator<Windows::Foundation::Collections::IKeyValuePair<HSTRING,IInspectable*>*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_USE */





#ifndef DEF___FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_USE
#define DEF___FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("fe2f3d47-5d47-5499-8374-430c7cda0204"))
IIterable<__FIKeyValuePair_2_HSTRING_IInspectable*> : IIterable_impl<__FIKeyValuePair_2_HSTRING_IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Object>>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<__FIKeyValuePair_2_HSTRING_IInspectable*> __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_t;
#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable Windows::Foundation::Collections::__FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable Windows::Foundation::Collections::IIterable<Windows::Foundation::Collections::IKeyValuePair<HSTRING,IInspectable*>*>
//#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_t Windows::Foundation::Collections::IIterable<Windows::Foundation::Collections::IKeyValuePair<HSTRING,IInspectable*>*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_USE */





#ifndef DEF___FIMapView_2_HSTRING_IInspectable_USE
#define DEF___FIMapView_2_HSTRING_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("bb78502a-f79d-54fa-92c9-90c5039fdf7e"))
IMapView<HSTRING,IInspectable*> : IMapView_impl<HSTRING,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IMapView`2<String, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IMapView<HSTRING,IInspectable*> __FIMapView_2_HSTRING_IInspectable_t;
#define __FIMapView_2_HSTRING_IInspectable Windows::Foundation::Collections::__FIMapView_2_HSTRING_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIMapView_2_HSTRING_IInspectable Windows::Foundation::Collections::IMapView<HSTRING,IInspectable*>
//#define __FIMapView_2_HSTRING_IInspectable_t Windows::Foundation::Collections::IMapView<HSTRING,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIMapView_2_HSTRING_IInspectable_USE */





#ifndef DEF___FIMap_2_HSTRING_IInspectable_USE
#define DEF___FIMap_2_HSTRING_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("1b0d3570-0877-5ec2-8a2c-3b9539506aca"))
IMap<HSTRING,IInspectable*> : IMap_impl<HSTRING,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IMap`2<String, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IMap<HSTRING,IInspectable*> __FIMap_2_HSTRING_IInspectable_t;
#define __FIMap_2_HSTRING_IInspectable Windows::Foundation::Collections::__FIMap_2_HSTRING_IInspectable_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIMap_2_HSTRING_IInspectable Windows::Foundation::Collections::IMap<HSTRING,IInspectable*>
//#define __FIMap_2_HSTRING_IInspectable_t Windows::Foundation::Collections::IMap<HSTRING,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIMap_2_HSTRING_IInspectable_USE */






namespace TestApp {
    namespace Parity {
        class ObjectWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IObjectWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ObjectWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IObjectWidget:HRESULT GetValue(Object**);HRESULT SetValue(Object*);HRESULT get_Tag(Object**);HRESULT put_Tag(Object*);HRESULT GetAll(Windows.Foundation.Collections.IVector`1<Object>**);HRESULT GetProperties(Windows.Foundation.Collections.IMap`2<String, Object>**);HRESULT Equals(Object*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIObjectWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIObjectWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IObjectWidget[] = L"TestApp.Parity.IObjectWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("31020752-c7b7-5b88-90ba-4ca8c7a35a52"), version, object, exclusiveto] */
        MIDL_INTERFACE("31020752-c7b7-5b88-90ba-4ca8c7a35a52")
        IObjectWidget : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE GetValue(
                /* [out, retval] */IInspectable * * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE SetValue(
                /* [in] */IInspectable * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Tag(
                /* [out, retval] */IInspectable * * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_Tag(
                /* [in] */IInspectable * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetAll(
                /* [out, retval] */__FIVector_1_IInspectable * * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetProperties(
                /* [out, retval] */__FIMap_2_HSTRING_IInspectable * * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Equals(
                /* [in] */IInspectable * other,
                /* [out, retval] */::boolean * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IObjectWidget=__uuidof(IObjectWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIObjectWidget;
#endif /* !defined(____x_TestApp_CParity_CIObjectWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ObjectWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IObjectWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ObjectWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ObjectWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ObjectWidget[] = L"TestApp.Parity.ObjectWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIObjectWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIObjectWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIObjectWidget __x_TestApp_CParity_CIObjectWidget;

#endif // ____x_TestApp_CParity_CIObjectWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions

#if !defined(____FIIterator_1_IInspectable_INTERFACE_DEFINED__)
#define ____FIIterator_1_IInspectable_INTERFACE_DEFINED__

typedef interface __FIIterator_1_IInspectable __FIIterator_1_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1_IInspectable;

typedef struct __FIIterator_1_IInspectableVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1_IInspectable * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1_IInspectable * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1_IInspectable * This, /* [retval][out] */ __RPC__out IInspectable * *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1_IInspectable * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1_IInspectable * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1_IInspectable * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) IInspectable * *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1_IInspectableVtbl;

interface __FIIterator_1_IInspectable
{
    CONST_VTBL struct __FIIterator_1_IInspectableVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1_IInspectable_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1_IInspectable_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1_IInspectable_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1_IInspectable_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIIterable_1_IInspectable_INTERFACE_DEFINED__)
#define ____FIIterable_1_IInspectable_INTERFACE_DEFINED__

typedef interface __FIIterable_1_IInspectable __FIIterable_1_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1_IInspectable;

typedef  struct __FIIterable_1_IInspectableVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1_IInspectable * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1_IInspectable * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1_IInspectable * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1_IInspectable **first);

    END_INTERFACE
} __FIIterable_1_IInspectableVtbl;

interface __FIIterable_1_IInspectable
{
    CONST_VTBL struct __FIIterable_1_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1_IInspectable_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIVectorView_1_IInspectable_INTERFACE_DEFINED__)
#define ____FIVectorView_1_IInspectable_INTERFACE_DEFINED__

typedef interface __FIVectorView_1_IInspectable __FIVectorView_1_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVectorView_1_IInspectable;

typedef struct __FIVectorView_1_IInspectableVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVectorView_1_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIVectorView_1_IInspectable * This);

    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIVectorView_1_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIVectorView_1_IInspectable * This,
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
        __RPC__in __FIVectorView_1_IInspectable * This,
            /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
        __RPC__in __FIVectorView_1_IInspectable * This,
            /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )( 
                                         __RPC__in __FIVectorView_1_IInspectable * This,
                                         /* [in] */ unsigned int index,
                                         /* [retval][out] */ __RPC__out IInspectable * *item);

        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
            __RPC__in __FIVectorView_1_IInspectable * This,
            /* [retval][out] */ __RPC__out unsigned int *size);

        HRESULT ( STDMETHODCALLTYPE *IndexOf )( 
                                               __RPC__in __FIVectorView_1_IInspectable * This,
            /* [in] */ IInspectable * item,
            /* [out] */ __RPC__out unsigned int *index,
            /* [retval][out] */ __RPC__out boolean *found);

        HRESULT ( STDMETHODCALLTYPE *GetMany )( 
                                               __RPC__in __FIVectorView_1_IInspectable * This,
            /* [in] */ unsigned int startIndex,
            /* [in] */ unsigned int capacity,
            /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) IInspectable * *items,
            /* [retval][out] */ __RPC__out unsigned int *actual);

        END_INTERFACE
} __FIVectorView_1_IInspectableVtbl;

interface __FIVectorView_1_IInspectable
{
    CONST_VTBL struct __FIVectorView_1_IInspectableVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVectorView_1_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVectorView_1_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVectorView_1_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVectorView_1_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVectorView_1_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVectorView_1_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVectorView_1_IInspectable_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVectorView_1_IInspectable_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVectorView_1_IInspectable_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVectorView_1_IInspectable_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#endif /* COBJMACROS */



#endif // ____FIVectorView_1_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIVector_1_IInspectable_INTERFACE_DEFINED__)
#define ____FIVector_1_IInspectable_INTERFACE_DEFINED__

typedef interface __FIVector_1_IInspectable __FIVector_1_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVector_1_IInspectable;

typedef struct __FIVector_1_IInspectableVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVector_1_IInspectable * This,
            /* [in] */ __RPC__in REFIID riid,
            /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIVector_1_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIVector_1_IInspectable * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIVector_1_IInspectable * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIVector_1_IInspectable * This, /* [out] */ __RPC__deref_out_opt IInspectable * *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIVector_1_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )(__RPC__in __FIVector_1_IInspectable * This,
        /* [in] */ unsigned int index,
        /* [retval][out] */ __RPC__deref_out_opt IInspectable * *item);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
        __RPC__in __FIVector_1_IInspectable * This,
        /* [retval][out] */ __RPC__out unsigned int *size);

    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIVector_1_IInspectable * This, /* [retval][out] */ __RPC__deref_out_opt __FIVectorView_1_IInspectable **view);

    HRESULT ( STDMETHODCALLTYPE *IndexOf )(__RPC__in __FIVector_1_IInspectable * This,
        /* [in] */ __RPC__in IInspectable * item,
        /* [out] */ __RPC__out unsigned int *index,
        /* [retval][out] */ __RPC__out boolean *found);

    HRESULT ( STDMETHODCALLTYPE *SetAt )(__RPC__in __FIVector_1_IInspectable * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in IInspectable * item);

    HRESULT ( STDMETHODCALLTYPE *InsertAt )(__RPC__in __FIVector_1_IInspectable * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in IInspectable * item);

    HRESULT ( STDMETHODCALLTYPE *RemoveAt )(__RPC__in __FIVector_1_IInspectable * This, /* [in] */ unsigned int index);
    HRESULT ( STDMETHODCALLTYPE *Append )(__RPC__in __FIVector_1_IInspectable * This, /* [in] */ __RPC__in IInspectable * item);
    HRESULT ( STDMETHODCALLTYPE *RemoveAtEnd )(__RPC__in __FIVector_1_IInspectable * This);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIVector_1_IInspectable * This);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIVector_1_IInspectable * This,
        /* [in] */ unsigned int startIndex,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) IInspectable * *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    HRESULT ( STDMETHODCALLTYPE *ReplaceAll )(__RPC__in __FIVector_1_IInspectable * This,
        /* [in] */ unsigned int count,
        /* [size_is][in] */ __RPC__in_ecount_full(count) IInspectable * *value);

    END_INTERFACE
} __FIVector_1_IInspectableVtbl;

interface __FIVector_1_IInspectable
{
    CONST_VTBL struct __FIVector_1_IInspectableVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVector_1_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVector_1_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVector_1_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVector_1_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVector_1_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVector_1_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVector_1_IInspectable_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVector_1_IInspectable_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVector_1_IInspectable_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIVector_1_IInspectable_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVector_1_IInspectable_SetAt(This,index,item)	\
    ( (This)->lpVtbl -> SetAt(This,index,item) ) 

#define __FIVector_1_IInspectable_InsertAt(This,index,item)	\
    ( (This)->lpVtbl -> InsertAt(This,index,item) ) 

#define __FIVector_1_IInspectable_RemoveAt(This,index)	\
    ( (This)->lpVtbl -> RemoveAt(This,index) ) 

#define __FIVector_1_IInspectable_Append(This,item)	\
    ( (This)->lpVtbl -> Append(This,item) ) 

#define __FIVector_1_IInspectable_RemoveAtEnd(This)	\
    ( (This)->lpVtbl -> RemoveAtEnd(This) ) 

#define __FIVector_1_IInspectable_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 

#define __FIVector_1_IInspectable_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#define __FIVector_1_IInspectable_ReplaceAll(This,count,value)	\
    ( (This)->lpVtbl -> ReplaceAll(This,count,value) ) 

#endif /* COBJMACROS */



#endif // ____FIVector_1_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__)
#define ____FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__

typedef interface __FIKeyValuePair_2_HSTRING_IInspectable __FIKeyValuePair_2_HSTRING_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIKeyValuePair_2_HSTRING_IInspectable;

typedef struct __FIKeyValuePair_2_HSTRING_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This,
            /* [out] */ __RPC__out ULONG *iidCount,
            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Key )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__out HSTRING *key);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIKeyValuePair_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__deref_out_opt IInspectable * *value);
    END_INTERFACE
} __FIKeyValuePair_2_HSTRING_IInspectableVtbl;

interface __FIKeyValuePair_2_HSTRING_IInspectable
{
    CONST_VTBL struct __FIKeyValuePair_2_HSTRING_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIKeyValuePair_2_HSTRING_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIKeyValuePair_2_HSTRING_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIKeyValuePair_2_HSTRING_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIKeyValuePair_2_HSTRING_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIKeyValuePair_2_HSTRING_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIKeyValuePair_2_HSTRING_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIKeyValuePair_2_HSTRING_IInspectable_get_Key(This,key)	\
    ( (This)->lpVtbl -> get_Key(This,key) ) 

#define __FIKeyValuePair_2_HSTRING_IInspectable_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__)
#define ____FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__

typedef interface __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable;

typedef struct __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectableVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__out __FIKeyValuePair_2_HSTRING_IInspectable * *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) __FIKeyValuePair_2_HSTRING_IInspectable * *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectableVtbl;

interface __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable
{
    CONST_VTBL struct __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectableVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__)
#define ____FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__

typedef interface __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable;

typedef  struct __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectableVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1___FIKeyValuePair_2_HSTRING_IInspectable **first);

    END_INTERFACE
} __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectableVtbl;

interface __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable
{
    CONST_VTBL struct __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1___FIKeyValuePair_2_HSTRING_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIMapView_2_HSTRING_IInspectable_INTERFACE_DEFINED__)
#define ____FIMapView_2_HSTRING_IInspectable_INTERFACE_DEFINED__

typedef interface __FIMapView_2_HSTRING_IInspectable __FIMapView_2_HSTRING_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIMapView_2_HSTRING_IInspectable;

typedef struct __FIMapView_2_HSTRING_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This,/* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *Lookup )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This,
        /* [in] */ __RPC__in HSTRING key,
        /* [retval][out] */ __RPC__deref_out_opt IInspectable * *value);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__out unsigned int *size);
    HRESULT ( STDMETHODCALLTYPE *HasKey )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This, /* [in] */ __RPC__in HSTRING key, /* [retval][out] */ __RPC__out boolean *found);
    HRESULT ( STDMETHODCALLTYPE *Split )(__RPC__in __FIMapView_2_HSTRING_IInspectable * This,/* [out] */ __RPC__deref_out_opt __FIMapView_2_HSTRING_IInspectable **firstPartition,
        /* [out] */ __RPC__deref_out_opt __FIMapView_2_HSTRING_IInspectable **secondPartition);
    END_INTERFACE
} __FIMapView_2_HSTRING_IInspectableVtbl;

interface __FIMapView_2_HSTRING_IInspectable
{
    CONST_VTBL struct __FIMapView_2_HSTRING_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIMapView_2_HSTRING_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIMapView_2_HSTRING_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIMapView_2_HSTRING_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIMapView_2_HSTRING_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIMapView_2_HSTRING_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIMapView_2_HSTRING_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIMapView_2_HSTRING_IInspectable_Lookup(This,key,value)	\
    ( (This)->lpVtbl -> Lookup(This,key,value) ) 
#define __FIMapView_2_HSTRING_IInspectable_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 
#define __FIMapView_2_HSTRING_IInspectable_HasKey(This,key,found)	\
    ( (This)->lpVtbl -> HasKey(This,key,found) ) 
#define __FIMapView_2_HSTRING_IInspectable_Split(This,firstPartition,secondPartition)	\
    ( (This)->lpVtbl -> Split(This,firstPartition,secondPartition) ) 
#endif /* COBJMACROS */


#endif // ____FIMapView_2_HSTRING_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIMap_2_HSTRING_IInspectable_INTERFACE_DEFINED__)
#define ____FIMap_2_HSTRING_IInspectable_INTERFACE_DEFINED__

typedef interface __FIMap_2_HSTRING_IInspectable __FIMap_2_HSTRING_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIMap_2_HSTRING_IInspectable;

typedef struct __FIMap_2_HSTRING_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIMap_2_HSTRING_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIMap_2_HSTRING_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIMap_2_HSTRING_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIMap_2_HSTRING_IInspectable * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIMap_2_HSTRING_IInspectable * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIMap_2_HSTRING_IInspectable * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *Lookup )(__RPC__in __FIMap_2_HSTRING_IInspectable * This,
        /* [in] */ HSTRING key,
        /* [retval][out] */ __RPC__deref_out_opt IInspectable * **value);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )(__RPC__in __FIMap_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__out unsigned int *size);
    HRESULT ( STDMETHODCALLTYPE *HasKey )(__RPC__in __FIMap_2_HSTRING_IInspectable * This, /* [in] */ HSTRING key, /* [retval][out] */ __RPC__out boolean *found);
    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIMap_2_HSTRING_IInspectable * This, /* [retval][out] */ __RPC__deref_out_opt __FIMapView_2_HSTRING_IInspectable **view);
    HRESULT ( STDMETHODCALLTYPE *Insert )(__RPC__in __FIMap_2_HSTRING_IInspectable * This,
        /* [in] */ HSTRING key,
        /* [in] */ __RPC__in_opt IInspectable * *value,
        /* [retval][out] */ __RPC__out boolean *replaced);
    HRESULT ( STDMETHODCALLTYPE *Remove )(__RPC__in __FIMap_2_HSTRING_IInspectable * This,/* [in] */ HSTRING key);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIMap_2_HSTRING_IInspectable * This);
    END_INTERFACE
} __FIMap_2_HSTRING_IInspectableVtbl;

interface __FIMap_2_HSTRING_IInspectable
{
    CONST_VTBL struct __FIMap_2_HSTRING_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIMap_2_HSTRING_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIMap_2_HSTRING_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIMap_2_HSTRING_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIMap_2_HSTRING_IInspectable_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIMap_2_HSTRING_IInspectable_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIMap_2_HSTRING_IInspectable_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIMap_2_HSTRING_IInspectable_Lookup(This,key,value)	\
    ( (This)->lpVtbl -> Lookup(This,key,value) ) 

#define __FIMap_2_HSTRING_IInspectable_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIMap_2_HSTRING_IInspectable_HasKey(This,key,found)	\
    ( (This)->lpVtbl -> HasKey(This,key,found) ) 

#define __FIMap_2_HSTRING_IInspectable_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIMap_2_HSTRING_IInspectable_Insert(This,key,value,replaced)	\
    ( (This)->lpVtbl -> Insert(This,key,value,replaced) ) 

#define __FIMap_2_HSTRING_IInspectable_Remove(This,key)	\
    ( (This)->lpVtbl -> Remove(This,key) ) 

#define __FIMap_2_HSTRING_IInspectable_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 
#endif /* COBJMACROS */



#endif // ____FIMap_2_HSTRING_IInspectable_INTERFACE_DEFINED__





/*
 *
 * Interface TestApp.Parity.IObjectWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ObjectWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IObjectWidget:HRESULT GetValue(Object**);HRESULT SetValue(Object*);HRESULT get_Tag(Object**);HRESULT put_Tag(Object*);HRESULT GetAll(Windows.Foundation.Collections.IVector`1<Object>**);HRESULT GetProperties(Windows.Foundation.Collections.IMap`2<String, Object>**);HRESULT Equals(Object*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIObjectWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIObjectWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IObjectWidget[] = L"TestApp.Parity.IObjectWidget";
/* [uuid("31020752-c7b7-5b88-90ba-4ca8c7a35a52"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIObjectWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIObjectWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIObjectWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIObjectWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIObjectWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIObjectWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIObjectWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *GetValue )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [out, retval] */IInspectable * * result
        );
    HRESULT ( STDMETHODCALLTYPE *SetValue )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [in] */IInspectable * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Tag )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [out, retval] */IInspectable * * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_Tag )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [in] */IInspectable * value
        );
    HRESULT ( STDMETHODCALLTYPE *GetAll )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [out, retval] */__FIVector_1_IInspectable * * result
        );
    HRESULT ( STDMETHODCALLTYPE *GetProperties )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [out, retval] */__FIMap_2_HSTRING_IInspectable * * result
        );
    HRESULT ( STDMETHODCALLTYPE *Equals )(
        __x_TestApp_CParity_CIObjectWidget * This,
        /* [in] */IInspectable * other,
        /* [out, retval] */boolean * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIObjectWidgetVtbl;

interface __x_TestApp_CParity_CIObjectWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIObjectWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIObjectWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIObjectWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIObjectWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIObjectWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIObjectWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIObjectWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIObjectWidget_GetValue(This,result) \
    ( (This)->lpVtbl->GetValue(This,result) )

#define __x_TestApp_CParity_CIObjectWidget_SetValue(This,value) \
    ( (This)->lpVtbl->SetValue(This,value) )

#define __x_TestApp_CParity_CIObjectWidget_get_Tag(This,value) \
    ( (This)->lpVtbl->get_Tag(This,value) )

#define __x_TestApp_CParity_CIObjectWidget_put_Tag(This,value) \
    ( (This)->lpVtbl->put_Tag(This,value) )

#define __x_TestApp_CParity_CIObjectWidget_GetAll(This,result) \
    ( (This)->lpVtbl->GetAll(This,result) )

#define __x_TestApp_CParity_CIObjectWidget_GetProperties(This,result) \
    ( (This)->lpVtbl->GetProperties(This,result) )

#define __x_TestApp_CParity_CIObjectWidget_Equals(This,other,result) \
    ( (This)->lpVtbl->Equals(This,other,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIObjectWidget;
#endif /* !defined(____x_TestApp_CParity_CIObjectWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ObjectWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IObjectWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ObjectWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ObjectWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ObjectWidget[] = L"TestApp.Parity.ObjectWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_object_params_p_h__

#endif // __parity_object_params_h__
