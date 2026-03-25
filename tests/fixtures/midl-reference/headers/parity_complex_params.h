/* Header file automatically generated from parity_complex_params.idl */
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
#ifndef __parity_complex_params_h__
#define __parity_complex_params_h__
#ifndef __parity_complex_params_p_h__
#define __parity_complex_params_p_h__


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
#ifndef ____x_TestApp_CParity_CIComplexParamWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComplexParamWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IComplexParamWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIComplexParamWidget TestApp::Parity::IComplexParamWidget

#endif // ____x_TestApp_CParity_CIComplexParamWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

#ifndef DEF___FIIterator_1_HSTRING_USE
#define DEF___FIIterator_1_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("8c304ebb-6615-50a4-8829-879ecd443236"))
IIterator<HSTRING> : IIterator_impl<HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<HSTRING> __FIIterator_1_HSTRING_t;
#define __FIIterator_1_HSTRING Windows::Foundation::Collections::__FIIterator_1_HSTRING_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1_HSTRING Windows::Foundation::Collections::IIterator<HSTRING>
//#define __FIIterator_1_HSTRING_t Windows::Foundation::Collections::IIterator<HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1_HSTRING_USE */




#ifndef DEF___FIIterable_1_HSTRING_USE
#define DEF___FIIterable_1_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("e2fcc7c1-3bfc-5a0b-b2b0-72e769d1cb7e"))
IIterable<HSTRING> : IIterable_impl<HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<HSTRING> __FIIterable_1_HSTRING_t;
#define __FIIterable_1_HSTRING Windows::Foundation::Collections::__FIIterable_1_HSTRING_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1_HSTRING Windows::Foundation::Collections::IIterable<HSTRING>
//#define __FIIterable_1_HSTRING_t Windows::Foundation::Collections::IIterable<HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1_HSTRING_USE */




#ifndef DEF___FIVectorView_1_HSTRING_USE
#define DEF___FIVectorView_1_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("2f13c006-a03a-5f69-b090-75a43e33423e"))
IVectorView<HSTRING> : IVectorView_impl<HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVectorView`1<String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVectorView<HSTRING> __FIVectorView_1_HSTRING_t;
#define __FIVectorView_1_HSTRING Windows::Foundation::Collections::__FIVectorView_1_HSTRING_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVectorView_1_HSTRING Windows::Foundation::Collections::IVectorView<HSTRING>
//#define __FIVectorView_1_HSTRING_t Windows::Foundation::Collections::IVectorView<HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVectorView_1_HSTRING_USE */




#ifndef DEF___FIVector_1_HSTRING_USE
#define DEF___FIVector_1_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("98b9acc1-4b56-532e-ac73-03d5291cca90"))
IVector<HSTRING> : IVector_impl<HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVector`1<String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVector<HSTRING> __FIVector_1_HSTRING_t;
#define __FIVector_1_HSTRING Windows::Foundation::Collections::__FIVector_1_HSTRING_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVector_1_HSTRING Windows::Foundation::Collections::IVector<HSTRING>
//#define __FIVector_1_HSTRING_t Windows::Foundation::Collections::IVector<HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVector_1_HSTRING_USE */




#ifndef DEF___FIKeyValuePair_2_HSTRING_int_USE
#define DEF___FIKeyValuePair_2_HSTRING_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("40e7e72d-cbab-588b-a227-9e60532f0121"))
IKeyValuePair<HSTRING,int> : IKeyValuePair_impl<HSTRING,int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IKeyValuePair`2<String, Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IKeyValuePair<HSTRING,int> __FIKeyValuePair_2_HSTRING_int_t;
#define __FIKeyValuePair_2_HSTRING_int Windows::Foundation::Collections::__FIKeyValuePair_2_HSTRING_int_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIKeyValuePair_2_HSTRING_int Windows::Foundation::Collections::IKeyValuePair<HSTRING,int>
//#define __FIKeyValuePair_2_HSTRING_int_t Windows::Foundation::Collections::IKeyValuePair<HSTRING,int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIKeyValuePair_2_HSTRING_int_USE */





#ifndef DEF___FIIterator_1___FIKeyValuePair_2_HSTRING_int_USE
#define DEF___FIIterator_1___FIKeyValuePair_2_HSTRING_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("96c8b304-4108-5f67-8b2f-213975f085b2"))
IIterator<__FIKeyValuePair_2_HSTRING_int*> : IIterator_impl<__FIKeyValuePair_2_HSTRING_int*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Int32>>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<__FIKeyValuePair_2_HSTRING_int*> __FIIterator_1___FIKeyValuePair_2_HSTRING_int_t;
#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int Windows::Foundation::Collections::__FIIterator_1___FIKeyValuePair_2_HSTRING_int_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int Windows::Foundation::Collections::IIterator<Windows::Foundation::Collections::IKeyValuePair<HSTRING,int>*>
//#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_t Windows::Foundation::Collections::IIterator<Windows::Foundation::Collections::IKeyValuePair<HSTRING,int>*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1___FIKeyValuePair_2_HSTRING_int_USE */





#ifndef DEF___FIIterable_1___FIKeyValuePair_2_HSTRING_int_USE
#define DEF___FIIterable_1___FIKeyValuePair_2_HSTRING_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("2aa69c56-c3a4-58f9-b14c-465bcaf8c7ba"))
IIterable<__FIKeyValuePair_2_HSTRING_int*> : IIterable_impl<__FIKeyValuePair_2_HSTRING_int*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Int32>>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<__FIKeyValuePair_2_HSTRING_int*> __FIIterable_1___FIKeyValuePair_2_HSTRING_int_t;
#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int Windows::Foundation::Collections::__FIIterable_1___FIKeyValuePair_2_HSTRING_int_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int Windows::Foundation::Collections::IIterable<Windows::Foundation::Collections::IKeyValuePair<HSTRING,int>*>
//#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_t Windows::Foundation::Collections::IIterable<Windows::Foundation::Collections::IKeyValuePair<HSTRING,int>*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1___FIKeyValuePair_2_HSTRING_int_USE */




#ifndef DEF___FIMapView_2_HSTRING_int_USE
#define DEF___FIMapView_2_HSTRING_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("06c17849-dfc8-501a-bf47-16152fa21d4b"))
IMapView<HSTRING,int> : IMapView_impl<HSTRING,int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IMapView`2<String, Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IMapView<HSTRING,int> __FIMapView_2_HSTRING_int_t;
#define __FIMapView_2_HSTRING_int Windows::Foundation::Collections::__FIMapView_2_HSTRING_int_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIMapView_2_HSTRING_int Windows::Foundation::Collections::IMapView<HSTRING,int>
//#define __FIMapView_2_HSTRING_int_t Windows::Foundation::Collections::IMapView<HSTRING,int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIMapView_2_HSTRING_int_USE */




#ifndef DEF___FIMap_2_HSTRING_int_USE
#define DEF___FIMap_2_HSTRING_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("ae681871-dd82-5299-93ea-0275e4e073e7"))
IMap<HSTRING,int> : IMap_impl<HSTRING,int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IMap`2<String, Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IMap<HSTRING,int> __FIMap_2_HSTRING_int_t;
#define __FIMap_2_HSTRING_int Windows::Foundation::Collections::__FIMap_2_HSTRING_int_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIMap_2_HSTRING_int Windows::Foundation::Collections::IMap<HSTRING,int>
//#define __FIMap_2_HSTRING_int_t Windows::Foundation::Collections::IMap<HSTRING,int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIMap_2_HSTRING_int_USE */






/*
 *
 * Struct TestApp.Parity.ComplexParam
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct ComplexParam
        {
            HSTRING Name;
            int Id;
            ::boolean Active;
        };
        
    } /* Parity */
} /* TestApp */


namespace TestApp {
    namespace Parity {
        class ComplexParamWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IComplexParamWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComplexParamWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComplexParamWidget:HRESULT ProcessAll(String,Int32,Boolean,Double,TestApp.Parity.ComplexParam);HRESULT AcceptCollections(Windows.Foundation.Collections.IVector`1<String>*,Windows.Foundation.Collections.IMap`2<String, Int32>*);HRESULT MixedParams(String,Int32*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComplexParamWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComplexParamWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComplexParamWidget[] = L"TestApp.Parity.IComplexParamWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("632fb5b0-d7b0-5b4c-b685-eb8ce98419b1"), version, object, exclusiveto] */
        MIDL_INTERFACE("632fb5b0-d7b0-5b4c-b685-eb8ce98419b1")
        IComplexParamWidget : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE ProcessAll(
                /* [in] */HSTRING name,
                /* [in] */int count,
                /* [in] */::boolean flag,
                /* [in] */double value,
                /* [in] */struct TestApp::Parity::ComplexParam param
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE AcceptCollections(
                /* [in] */__FIVector_1_HSTRING * names,
                /* [in] */__FIMap_2_HSTRING_int * counts
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE MixedParams(
                /* [in] */HSTRING name,
                /* [out] */int * result,
                /* [out] */::boolean * success
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IComplexParamWidget=__uuidof(IComplexParamWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIComplexParamWidget;
#endif /* !defined(____x_TestApp_CParity_CIComplexParamWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ComplexParamWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IComplexParamWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ComplexParamWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ComplexParamWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ComplexParamWidget[] = L"TestApp.Parity.ComplexParamWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIComplexParamWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComplexParamWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIComplexParamWidget __x_TestApp_CParity_CIComplexParamWidget;

#endif // ____x_TestApp_CParity_CIComplexParamWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions
#if !defined(____FIIterator_1_HSTRING_INTERFACE_DEFINED__)
#define ____FIIterator_1_HSTRING_INTERFACE_DEFINED__

typedef interface __FIIterator_1_HSTRING __FIIterator_1_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1_HSTRING;

typedef struct __FIIterator_1_HSTRINGVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1_HSTRING * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1_HSTRING * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1_HSTRING * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1_HSTRING * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1_HSTRING * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1_HSTRING * This, /* [retval][out] */ __RPC__out HSTRING *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1_HSTRING * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1_HSTRING * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1_HSTRING * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) HSTRING *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1_HSTRINGVtbl;

interface __FIIterator_1_HSTRING
{
    CONST_VTBL struct __FIIterator_1_HSTRINGVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1_HSTRING_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1_HSTRING_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1_HSTRING_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1_HSTRING_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1_HSTRING_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1_HSTRING_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1_HSTRING_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1_HSTRING_INTERFACE_DEFINED__


#if !defined(____FIIterable_1_HSTRING_INTERFACE_DEFINED__)
#define ____FIIterable_1_HSTRING_INTERFACE_DEFINED__

typedef interface __FIIterable_1_HSTRING __FIIterable_1_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1_HSTRING;

typedef  struct __FIIterable_1_HSTRINGVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1_HSTRING * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1_HSTRING * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1_HSTRING * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1_HSTRING * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1_HSTRING * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1_HSTRING * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1_HSTRING **first);

    END_INTERFACE
} __FIIterable_1_HSTRINGVtbl;

interface __FIIterable_1_HSTRING
{
    CONST_VTBL struct __FIIterable_1_HSTRINGVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1_HSTRING_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1_HSTRING_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1_HSTRING_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1_HSTRING_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1_HSTRING_INTERFACE_DEFINED__


#if !defined(____FIVectorView_1_HSTRING_INTERFACE_DEFINED__)
#define ____FIVectorView_1_HSTRING_INTERFACE_DEFINED__

typedef interface __FIVectorView_1_HSTRING __FIVectorView_1_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVectorView_1_HSTRING;

typedef struct __FIVectorView_1_HSTRINGVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVectorView_1_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIVectorView_1_HSTRING * This);

    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIVectorView_1_HSTRING * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIVectorView_1_HSTRING * This,
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
        __RPC__in __FIVectorView_1_HSTRING * This,
            /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
        __RPC__in __FIVectorView_1_HSTRING * This,
            /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )( 
                                         __RPC__in __FIVectorView_1_HSTRING * This,
                                         /* [in] */ unsigned int index,
                                         /* [retval][out] */ __RPC__out HSTRING *item);

        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
            __RPC__in __FIVectorView_1_HSTRING * This,
            /* [retval][out] */ __RPC__out unsigned int *size);

        HRESULT ( STDMETHODCALLTYPE *IndexOf )( 
                                               __RPC__in __FIVectorView_1_HSTRING * This,
            /* [in] */ HSTRING item,
            /* [out] */ __RPC__out unsigned int *index,
            /* [retval][out] */ __RPC__out boolean *found);

        HRESULT ( STDMETHODCALLTYPE *GetMany )( 
                                               __RPC__in __FIVectorView_1_HSTRING * This,
            /* [in] */ unsigned int startIndex,
            /* [in] */ unsigned int capacity,
            /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) HSTRING *items,
            /* [retval][out] */ __RPC__out unsigned int *actual);

        END_INTERFACE
} __FIVectorView_1_HSTRINGVtbl;

interface __FIVectorView_1_HSTRING
{
    CONST_VTBL struct __FIVectorView_1_HSTRINGVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVectorView_1_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVectorView_1_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVectorView_1_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVectorView_1_HSTRING_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVectorView_1_HSTRING_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVectorView_1_HSTRING_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVectorView_1_HSTRING_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVectorView_1_HSTRING_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVectorView_1_HSTRING_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVectorView_1_HSTRING_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#endif /* COBJMACROS */



#endif // ____FIVectorView_1_HSTRING_INTERFACE_DEFINED__


#if !defined(____FIVector_1_HSTRING_INTERFACE_DEFINED__)
#define ____FIVector_1_HSTRING_INTERFACE_DEFINED__

typedef interface __FIVector_1_HSTRING __FIVector_1_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVector_1_HSTRING;

typedef struct __FIVector_1_HSTRINGVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVector_1_HSTRING * This,
            /* [in] */ __RPC__in REFIID riid,
            /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIVector_1_HSTRING * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIVector_1_HSTRING * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIVector_1_HSTRING * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIVector_1_HSTRING * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIVector_1_HSTRING * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )(__RPC__in __FIVector_1_HSTRING * This,
        /* [in] */ unsigned int index,
        /* [retval][out] */ __RPC__deref_out_opt HSTRING *item);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
        __RPC__in __FIVector_1_HSTRING * This,
        /* [retval][out] */ __RPC__out unsigned int *size);

    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIVector_1_HSTRING * This, /* [retval][out] */ __RPC__deref_out_opt __FIVectorView_1_HSTRING **view);

    HRESULT ( STDMETHODCALLTYPE *IndexOf )(__RPC__in __FIVector_1_HSTRING * This,
        /* [in] */ __RPC__in HSTRING item,
        /* [out] */ __RPC__out unsigned int *index,
        /* [retval][out] */ __RPC__out boolean *found);

    HRESULT ( STDMETHODCALLTYPE *SetAt )(__RPC__in __FIVector_1_HSTRING * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in HSTRING item);

    HRESULT ( STDMETHODCALLTYPE *InsertAt )(__RPC__in __FIVector_1_HSTRING * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in HSTRING item);

    HRESULT ( STDMETHODCALLTYPE *RemoveAt )(__RPC__in __FIVector_1_HSTRING * This, /* [in] */ unsigned int index);
    HRESULT ( STDMETHODCALLTYPE *Append )(__RPC__in __FIVector_1_HSTRING * This, /* [in] */ __RPC__in HSTRING item);
    HRESULT ( STDMETHODCALLTYPE *RemoveAtEnd )(__RPC__in __FIVector_1_HSTRING * This);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIVector_1_HSTRING * This);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIVector_1_HSTRING * This,
        /* [in] */ unsigned int startIndex,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) HSTRING *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    HRESULT ( STDMETHODCALLTYPE *ReplaceAll )(__RPC__in __FIVector_1_HSTRING * This,
        /* [in] */ unsigned int count,
        /* [size_is][in] */ __RPC__in_ecount_full(count) HSTRING *value);

    END_INTERFACE
} __FIVector_1_HSTRINGVtbl;

interface __FIVector_1_HSTRING
{
    CONST_VTBL struct __FIVector_1_HSTRINGVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVector_1_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVector_1_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVector_1_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVector_1_HSTRING_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVector_1_HSTRING_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVector_1_HSTRING_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVector_1_HSTRING_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVector_1_HSTRING_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVector_1_HSTRING_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIVector_1_HSTRING_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVector_1_HSTRING_SetAt(This,index,item)	\
    ( (This)->lpVtbl -> SetAt(This,index,item) ) 

#define __FIVector_1_HSTRING_InsertAt(This,index,item)	\
    ( (This)->lpVtbl -> InsertAt(This,index,item) ) 

#define __FIVector_1_HSTRING_RemoveAt(This,index)	\
    ( (This)->lpVtbl -> RemoveAt(This,index) ) 

#define __FIVector_1_HSTRING_Append(This,item)	\
    ( (This)->lpVtbl -> Append(This,item) ) 

#define __FIVector_1_HSTRING_RemoveAtEnd(This)	\
    ( (This)->lpVtbl -> RemoveAtEnd(This) ) 

#define __FIVector_1_HSTRING_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 

#define __FIVector_1_HSTRING_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#define __FIVector_1_HSTRING_ReplaceAll(This,count,value)	\
    ( (This)->lpVtbl -> ReplaceAll(This,count,value) ) 

#endif /* COBJMACROS */



#endif // ____FIVector_1_HSTRING_INTERFACE_DEFINED__


#if !defined(____FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__)
#define ____FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__

typedef interface __FIKeyValuePair_2_HSTRING_int __FIKeyValuePair_2_HSTRING_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIKeyValuePair_2_HSTRING_int;

typedef struct __FIKeyValuePair_2_HSTRING_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This,
            /* [out] */ __RPC__out ULONG *iidCount,
            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Key )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This, /* [retval][out] */ __RPC__out HSTRING *key);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIKeyValuePair_2_HSTRING_int * This, /* [retval][out] */ __RPC__deref_out_opt int *value);
    END_INTERFACE
} __FIKeyValuePair_2_HSTRING_intVtbl;

interface __FIKeyValuePair_2_HSTRING_int
{
    CONST_VTBL struct __FIKeyValuePair_2_HSTRING_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIKeyValuePair_2_HSTRING_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIKeyValuePair_2_HSTRING_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIKeyValuePair_2_HSTRING_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIKeyValuePair_2_HSTRING_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIKeyValuePair_2_HSTRING_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIKeyValuePair_2_HSTRING_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIKeyValuePair_2_HSTRING_int_get_Key(This,key)	\
    ( (This)->lpVtbl -> get_Key(This,key) ) 

#define __FIKeyValuePair_2_HSTRING_int_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__



#if !defined(____FIIterator_1___FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__)
#define ____FIIterator_1___FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__

typedef interface __FIIterator_1___FIKeyValuePair_2_HSTRING_int __FIIterator_1___FIKeyValuePair_2_HSTRING_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1___FIKeyValuePair_2_HSTRING_int;

typedef struct __FIIterator_1___FIKeyValuePair_2_HSTRING_intVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This, /* [retval][out] */ __RPC__out __FIKeyValuePair_2_HSTRING_int * *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1___FIKeyValuePair_2_HSTRING_int * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) __FIKeyValuePair_2_HSTRING_int * *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1___FIKeyValuePair_2_HSTRING_intVtbl;

interface __FIIterator_1___FIKeyValuePair_2_HSTRING_int
{
    CONST_VTBL struct __FIIterator_1___FIKeyValuePair_2_HSTRING_intVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1___FIKeyValuePair_2_HSTRING_int_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1___FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__



#if !defined(____FIIterable_1___FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__)
#define ____FIIterable_1___FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__

typedef interface __FIIterable_1___FIKeyValuePair_2_HSTRING_int __FIIterable_1___FIKeyValuePair_2_HSTRING_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1___FIKeyValuePair_2_HSTRING_int;

typedef  struct __FIIterable_1___FIKeyValuePair_2_HSTRING_intVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1___FIKeyValuePair_2_HSTRING_int * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1___FIKeyValuePair_2_HSTRING_int **first);

    END_INTERFACE
} __FIIterable_1___FIKeyValuePair_2_HSTRING_intVtbl;

interface __FIIterable_1___FIKeyValuePair_2_HSTRING_int
{
    CONST_VTBL struct __FIIterable_1___FIKeyValuePair_2_HSTRING_intVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1___FIKeyValuePair_2_HSTRING_int_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1___FIKeyValuePair_2_HSTRING_int_INTERFACE_DEFINED__


#if !defined(____FIMapView_2_HSTRING_int_INTERFACE_DEFINED__)
#define ____FIMapView_2_HSTRING_int_INTERFACE_DEFINED__

typedef interface __FIMapView_2_HSTRING_int __FIMapView_2_HSTRING_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIMapView_2_HSTRING_int;

typedef struct __FIMapView_2_HSTRING_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIMapView_2_HSTRING_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIMapView_2_HSTRING_int * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIMapView_2_HSTRING_int * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIMapView_2_HSTRING_int * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIMapView_2_HSTRING_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIMapView_2_HSTRING_int * This,/* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *Lookup )(__RPC__in __FIMapView_2_HSTRING_int * This,
        /* [in] */ __RPC__in HSTRING key,
        /* [retval][out] */ __RPC__deref_out_opt int *value);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )(__RPC__in __FIMapView_2_HSTRING_int * This, /* [retval][out] */ __RPC__out unsigned int *size);
    HRESULT ( STDMETHODCALLTYPE *HasKey )(__RPC__in __FIMapView_2_HSTRING_int * This, /* [in] */ __RPC__in HSTRING key, /* [retval][out] */ __RPC__out boolean *found);
    HRESULT ( STDMETHODCALLTYPE *Split )(__RPC__in __FIMapView_2_HSTRING_int * This,/* [out] */ __RPC__deref_out_opt __FIMapView_2_HSTRING_int **firstPartition,
        /* [out] */ __RPC__deref_out_opt __FIMapView_2_HSTRING_int **secondPartition);
    END_INTERFACE
} __FIMapView_2_HSTRING_intVtbl;

interface __FIMapView_2_HSTRING_int
{
    CONST_VTBL struct __FIMapView_2_HSTRING_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIMapView_2_HSTRING_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIMapView_2_HSTRING_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIMapView_2_HSTRING_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIMapView_2_HSTRING_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIMapView_2_HSTRING_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIMapView_2_HSTRING_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIMapView_2_HSTRING_int_Lookup(This,key,value)	\
    ( (This)->lpVtbl -> Lookup(This,key,value) ) 
#define __FIMapView_2_HSTRING_int_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 
#define __FIMapView_2_HSTRING_int_HasKey(This,key,found)	\
    ( (This)->lpVtbl -> HasKey(This,key,found) ) 
#define __FIMapView_2_HSTRING_int_Split(This,firstPartition,secondPartition)	\
    ( (This)->lpVtbl -> Split(This,firstPartition,secondPartition) ) 
#endif /* COBJMACROS */


#endif // ____FIMapView_2_HSTRING_int_INTERFACE_DEFINED__


#if !defined(____FIMap_2_HSTRING_int_INTERFACE_DEFINED__)
#define ____FIMap_2_HSTRING_int_INTERFACE_DEFINED__

typedef interface __FIMap_2_HSTRING_int __FIMap_2_HSTRING_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIMap_2_HSTRING_int;

typedef struct __FIMap_2_HSTRING_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIMap_2_HSTRING_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIMap_2_HSTRING_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIMap_2_HSTRING_int * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIMap_2_HSTRING_int * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIMap_2_HSTRING_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIMap_2_HSTRING_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *Lookup )(__RPC__in __FIMap_2_HSTRING_int * This,
        /* [in] */ HSTRING key,
        /* [retval][out] */ __RPC__deref_out_opt int **value);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )(__RPC__in __FIMap_2_HSTRING_int * This, /* [retval][out] */ __RPC__out unsigned int *size);
    HRESULT ( STDMETHODCALLTYPE *HasKey )(__RPC__in __FIMap_2_HSTRING_int * This, /* [in] */ HSTRING key, /* [retval][out] */ __RPC__out boolean *found);
    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIMap_2_HSTRING_int * This, /* [retval][out] */ __RPC__deref_out_opt __FIMapView_2_HSTRING_int **view);
    HRESULT ( STDMETHODCALLTYPE *Insert )(__RPC__in __FIMap_2_HSTRING_int * This,
        /* [in] */ HSTRING key,
        /* [in] */ __RPC__in_opt int *value,
        /* [retval][out] */ __RPC__out boolean *replaced);
    HRESULT ( STDMETHODCALLTYPE *Remove )(__RPC__in __FIMap_2_HSTRING_int * This,/* [in] */ HSTRING key);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIMap_2_HSTRING_int * This);
    END_INTERFACE
} __FIMap_2_HSTRING_intVtbl;

interface __FIMap_2_HSTRING_int
{
    CONST_VTBL struct __FIMap_2_HSTRING_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIMap_2_HSTRING_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIMap_2_HSTRING_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIMap_2_HSTRING_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIMap_2_HSTRING_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIMap_2_HSTRING_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIMap_2_HSTRING_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIMap_2_HSTRING_int_Lookup(This,key,value)	\
    ( (This)->lpVtbl -> Lookup(This,key,value) ) 

#define __FIMap_2_HSTRING_int_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIMap_2_HSTRING_int_HasKey(This,key,found)	\
    ( (This)->lpVtbl -> HasKey(This,key,found) ) 

#define __FIMap_2_HSTRING_int_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIMap_2_HSTRING_int_Insert(This,key,value,replaced)	\
    ( (This)->lpVtbl -> Insert(This,key,value,replaced) ) 

#define __FIMap_2_HSTRING_int_Remove(This,key)	\
    ( (This)->lpVtbl -> Remove(This,key) ) 

#define __FIMap_2_HSTRING_int_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 
#endif /* COBJMACROS */



#endif // ____FIMap_2_HSTRING_int_INTERFACE_DEFINED__




/*
 *
 * Struct TestApp.Parity.ComplexParam
 *
 */

/* [version] */
struct __x_TestApp_CParity_CComplexParam
{
    HSTRING Name;
    int Id;
    boolean Active;
};



/*
 *
 * Interface TestApp.Parity.IComplexParamWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComplexParamWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComplexParamWidget:HRESULT ProcessAll(String,Int32,Boolean,Double,TestApp.Parity.ComplexParam);HRESULT AcceptCollections(Windows.Foundation.Collections.IVector`1<String>*,Windows.Foundation.Collections.IMap`2<String, Int32>*);HRESULT MixedParams(String,Int32*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComplexParamWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComplexParamWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComplexParamWidget[] = L"TestApp.Parity.IComplexParamWidget";
/* [uuid("632fb5b0-d7b0-5b4c-b685-eb8ce98419b1"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIComplexParamWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIComplexParamWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIComplexParamWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIComplexParamWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIComplexParamWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIComplexParamWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIComplexParamWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *ProcessAll )(
        __x_TestApp_CParity_CIComplexParamWidget * This,
        /* [in] */HSTRING name,
        /* [in] */int count,
        /* [in] */boolean flag,
        /* [in] */double value,
        /* [in] */struct __x_TestApp_CParity_CComplexParam param
        );
    HRESULT ( STDMETHODCALLTYPE *AcceptCollections )(
        __x_TestApp_CParity_CIComplexParamWidget * This,
        /* [in] */__FIVector_1_HSTRING * names,
        /* [in] */__FIMap_2_HSTRING_int * counts
        );
    HRESULT ( STDMETHODCALLTYPE *MixedParams )(
        __x_TestApp_CParity_CIComplexParamWidget * This,
        /* [in] */HSTRING name,
        /* [out] */int * result,
        /* [out] */boolean * success
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIComplexParamWidgetVtbl;

interface __x_TestApp_CParity_CIComplexParamWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIComplexParamWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIComplexParamWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIComplexParamWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIComplexParamWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIComplexParamWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIComplexParamWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIComplexParamWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIComplexParamWidget_ProcessAll(This,name,count,flag,value,param) \
    ( (This)->lpVtbl->ProcessAll(This,name,count,flag,value,param) )

#define __x_TestApp_CParity_CIComplexParamWidget_AcceptCollections(This,names,counts) \
    ( (This)->lpVtbl->AcceptCollections(This,names,counts) )

#define __x_TestApp_CParity_CIComplexParamWidget_MixedParams(This,name,result,success) \
    ( (This)->lpVtbl->MixedParams(This,name,result,success) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIComplexParamWidget;
#endif /* !defined(____x_TestApp_CParity_CIComplexParamWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ComplexParamWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IComplexParamWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ComplexParamWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ComplexParamWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ComplexParamWidget[] = L"TestApp.Parity.ComplexParamWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_complex_params_p_h__

#endif // __parity_complex_params_h__
