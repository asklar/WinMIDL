/* Header file automatically generated from parity_interface_method_variety.idl */
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
#ifndef __parity_interface_method_variety_h__
#define __parity_interface_method_variety_h__
#ifndef __parity_interface_method_variety_p_h__
#define __parity_interface_method_variety_p_h__


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
#ifndef ____x_TestApp_CParity_CIGeocoderService_FWD_DEFINED__
#define ____x_TestApp_CParity_CIGeocoderService_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IGeocoderService;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIGeocoderService TestApp::Parity::IGeocoderService

#endif // ____x_TestApp_CParity_CIGeocoderService_FWD_DEFINED__

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




namespace TestApp {
    namespace Parity {
        struct Coordinate;
        
    } /* Parity */
} /* TestApp */


#ifndef DEF___FIIterator_1_TestApp__CParity__CCoordinate_USE
#define DEF___FIIterator_1_TestApp__CParity__CCoordinate_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("649c5c77-b294-547b-90c1-0985b1f21bd3"))
IIterator<struct TestApp::Parity::Coordinate> : IIterator_impl<struct TestApp::Parity::Coordinate> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<TestApp.Parity.Coordinate>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<struct TestApp::Parity::Coordinate> __FIIterator_1_TestApp__CParity__CCoordinate_t;
#define __FIIterator_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::__FIIterator_1_TestApp__CParity__CCoordinate_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::IIterator<TestApp::Parity::Coordinate>
//#define __FIIterator_1_TestApp__CParity__CCoordinate_t Windows::Foundation::Collections::IIterator<TestApp::Parity::Coordinate>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1_TestApp__CParity__CCoordinate_USE */





#ifndef DEF___FIIterable_1_TestApp__CParity__CCoordinate_USE
#define DEF___FIIterable_1_TestApp__CParity__CCoordinate_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("705609b8-a26c-5cf7-8ee4-69d160cdfa22"))
IIterable<struct TestApp::Parity::Coordinate> : IIterable_impl<struct TestApp::Parity::Coordinate> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<TestApp.Parity.Coordinate>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<struct TestApp::Parity::Coordinate> __FIIterable_1_TestApp__CParity__CCoordinate_t;
#define __FIIterable_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::__FIIterable_1_TestApp__CParity__CCoordinate_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::IIterable<TestApp::Parity::Coordinate>
//#define __FIIterable_1_TestApp__CParity__CCoordinate_t Windows::Foundation::Collections::IIterable<TestApp::Parity::Coordinate>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1_TestApp__CParity__CCoordinate_USE */





#ifndef DEF___FIVectorView_1_TestApp__CParity__CCoordinate_USE
#define DEF___FIVectorView_1_TestApp__CParity__CCoordinate_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("ffe7b0e6-8a50-51e3-aff2-0c4839c61ffd"))
IVectorView<struct TestApp::Parity::Coordinate> : IVectorView_impl<struct TestApp::Parity::Coordinate> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVectorView`1<TestApp.Parity.Coordinate>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVectorView<struct TestApp::Parity::Coordinate> __FIVectorView_1_TestApp__CParity__CCoordinate_t;
#define __FIVectorView_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::__FIVectorView_1_TestApp__CParity__CCoordinate_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVectorView_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::IVectorView<TestApp::Parity::Coordinate>
//#define __FIVectorView_1_TestApp__CParity__CCoordinate_t Windows::Foundation::Collections::IVectorView<TestApp::Parity::Coordinate>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVectorView_1_TestApp__CParity__CCoordinate_USE */





#ifndef DEF___FIVector_1_TestApp__CParity__CCoordinate_USE
#define DEF___FIVector_1_TestApp__CParity__CCoordinate_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("f73b94d7-fcfe-509a-8115-4e25b7f03678"))
IVector<struct TestApp::Parity::Coordinate> : IVector_impl<struct TestApp::Parity::Coordinate> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVector`1<TestApp.Parity.Coordinate>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVector<struct TestApp::Parity::Coordinate> __FIVector_1_TestApp__CParity__CCoordinate_t;
#define __FIVector_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::__FIVector_1_TestApp__CParity__CCoordinate_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVector_1_TestApp__CParity__CCoordinate Windows::Foundation::Collections::IVector<TestApp::Parity::Coordinate>
//#define __FIVector_1_TestApp__CParity__CCoordinate_t Windows::Foundation::Collections::IVector<TestApp::Parity::Coordinate>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVector_1_TestApp__CParity__CCoordinate_USE */





#ifndef DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_USE
#define DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("c85d4697-658d-5492-a606-fbb242a513a3"))
IAsyncOperationCompletedHandler<struct TestApp::Parity::Coordinate> : IAsyncOperationCompletedHandler_impl<struct TestApp::Parity::Coordinate> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.AsyncOperationCompletedHandler`1<TestApp.Parity.Coordinate>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IAsyncOperationCompletedHandler<struct TestApp::Parity::Coordinate> __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_t;
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate Windows::Foundation::__FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate Windows::Foundation::IAsyncOperationCompletedHandler<TestApp::Parity::Coordinate>
//#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_t Windows::Foundation::IAsyncOperationCompletedHandler<TestApp::Parity::Coordinate>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_USE */





#ifndef DEF___FIAsyncOperation_1_TestApp__CParity__CCoordinate_USE
#define DEF___FIAsyncOperation_1_TestApp__CParity__CCoordinate_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("334300a8-8f15-580c-b1f5-a4d91e8c1cf9"))
IAsyncOperation<struct TestApp::Parity::Coordinate> : IAsyncOperation_impl<struct TestApp::Parity::Coordinate> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IAsyncOperation`1<TestApp.Parity.Coordinate>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IAsyncOperation<struct TestApp::Parity::Coordinate> __FIAsyncOperation_1_TestApp__CParity__CCoordinate_t;
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate Windows::Foundation::__FIAsyncOperation_1_TestApp__CParity__CCoordinate_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate Windows::Foundation::IAsyncOperation<TestApp::Parity::Coordinate>
//#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_t Windows::Foundation::IAsyncOperation<TestApp::Parity::Coordinate>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIAsyncOperation_1_TestApp__CParity__CCoordinate_USE */






/*
 *
 * Struct TestApp.Parity.Coordinate
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct Coordinate
        {
            double Lat;
            double Lon;
        };
        
    } /* Parity */
} /* TestApp */


namespace TestApp {
    namespace Parity {
        class GeocoderService;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IGeocoderService
 *
 * Interface is a part of the implementation of type TestApp.Parity.GeocoderService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IGeocoderService:HRESULT Geocode(String,TestApp.Parity.Coordinate*);HRESULT ReverseGeocode(TestApp.Parity.Coordinate,String*);HRESULT GeocodeMultiple(Windows.Foundation.Collections.IVector`1<String>*,Windows.Foundation.Collections.IVector`1<TestApp.Parity.Coordinate>**);HRESULT GeocodeAsync(String,Windows.Foundation.IAsyncOperation`1<TestApp.Parity.Coordinate>**);HRESULT IsValidCoordinate(TestApp.Parity.Coordinate,Boolean*);HRESULT Distance(TestApp.Parity.Coordinate,TestApp.Parity.Coordinate,Double*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIGeocoderService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIGeocoderService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IGeocoderService[] = L"TestApp.Parity.IGeocoderService";

namespace TestApp {
    namespace Parity {
        /* [uuid("e531f636-e1fe-5b63-802c-f7ef0782b782"), version, object, exclusiveto] */
        MIDL_INTERFACE("e531f636-e1fe-5b63-802c-f7ef0782b782")
        IGeocoderService : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Geocode(
                /* [in] */HSTRING address,
                /* [out, retval] */struct TestApp::Parity::Coordinate * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE ReverseGeocode(
                /* [in] */struct TestApp::Parity::Coordinate coord,
                /* [out, retval] */HSTRING * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GeocodeMultiple(
                /* [in] */__FIVector_1_HSTRING * addresses,
                /* [out, retval] */__FIVector_1_TestApp__CParity__CCoordinate * * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GeocodeAsync(
                /* [in] */HSTRING address,
                /* [out, retval] */__FIAsyncOperation_1_TestApp__CParity__CCoordinate * * operation
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE IsValidCoordinate(
                /* [in] */struct TestApp::Parity::Coordinate coord,
                /* [out, retval] */::boolean * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Distance(
                /* [in] */struct TestApp::Parity::Coordinate from,
                /* [in] */struct TestApp::Parity::Coordinate to,
                /* [out, retval] */double * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IGeocoderService=__uuidof(IGeocoderService);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIGeocoderService;
#endif /* !defined(____x_TestApp_CParity_CIGeocoderService_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.GeocoderService
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IGeocoderService ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_GeocoderService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_GeocoderService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_GeocoderService[] = L"TestApp.Parity.GeocoderService";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIGeocoderService_FWD_DEFINED__
#define ____x_TestApp_CParity_CIGeocoderService_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIGeocoderService __x_TestApp_CParity_CIGeocoderService;

#endif // ____x_TestApp_CParity_CIGeocoderService_FWD_DEFINED__

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


struct __x_TestApp_CParity_CCoordinate;

#if !defined(____FIIterator_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__)
#define ____FIIterator_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__

typedef interface __FIIterator_1_TestApp__CParity__CCoordinate __FIIterator_1_TestApp__CParity__CCoordinate;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1_TestApp__CParity__CCoordinate;

typedef struct __FIIterator_1_TestApp__CParity__CCoordinateVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__out struct __x_TestApp_CParity_CCoordinate *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) struct __x_TestApp_CParity_CCoordinate *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1_TestApp__CParity__CCoordinateVtbl;

interface __FIIterator_1_TestApp__CParity__CCoordinate
{
    CONST_VTBL struct __FIIterator_1_TestApp__CParity__CCoordinateVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1_TestApp__CParity__CCoordinate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1_TestApp__CParity__CCoordinate_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1_TestApp__CParity__CCoordinate_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1_TestApp__CParity__CCoordinate_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__



#if !defined(____FIIterable_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__)
#define ____FIIterable_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__

typedef interface __FIIterable_1_TestApp__CParity__CCoordinate __FIIterable_1_TestApp__CParity__CCoordinate;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1_TestApp__CParity__CCoordinate;

typedef  struct __FIIterable_1_TestApp__CParity__CCoordinateVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1_TestApp__CParity__CCoordinate **first);

    END_INTERFACE
} __FIIterable_1_TestApp__CParity__CCoordinateVtbl;

interface __FIIterable_1_TestApp__CParity__CCoordinate
{
    CONST_VTBL struct __FIIterable_1_TestApp__CParity__CCoordinateVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1_TestApp__CParity__CCoordinate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1_TestApp__CParity__CCoordinate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1_TestApp__CParity__CCoordinate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1_TestApp__CParity__CCoordinate_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1_TestApp__CParity__CCoordinate_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1_TestApp__CParity__CCoordinate_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1_TestApp__CParity__CCoordinate_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__



#if !defined(____FIVectorView_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__)
#define ____FIVectorView_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__

typedef interface __FIVectorView_1_TestApp__CParity__CCoordinate __FIVectorView_1_TestApp__CParity__CCoordinate;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVectorView_1_TestApp__CParity__CCoordinate;

typedef struct __FIVectorView_1_TestApp__CParity__CCoordinateVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This);

    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
        __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
            /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
        __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
            /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )( 
                                         __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
                                         /* [in] */ unsigned int index,
                                         /* [retval][out] */ __RPC__out struct __x_TestApp_CParity_CCoordinate *item);

        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
            __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
            /* [retval][out] */ __RPC__out unsigned int *size);

        HRESULT ( STDMETHODCALLTYPE *IndexOf )( 
                                               __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
            /* [in] */ struct __x_TestApp_CParity_CCoordinate item,
            /* [out] */ __RPC__out unsigned int *index,
            /* [retval][out] */ __RPC__out boolean *found);

        HRESULT ( STDMETHODCALLTYPE *GetMany )( 
                                               __RPC__in __FIVectorView_1_TestApp__CParity__CCoordinate * This,
            /* [in] */ unsigned int startIndex,
            /* [in] */ unsigned int capacity,
            /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) struct __x_TestApp_CParity_CCoordinate *items,
            /* [retval][out] */ __RPC__out unsigned int *actual);

        END_INTERFACE
} __FIVectorView_1_TestApp__CParity__CCoordinateVtbl;

interface __FIVectorView_1_TestApp__CParity__CCoordinate
{
    CONST_VTBL struct __FIVectorView_1_TestApp__CParity__CCoordinateVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVectorView_1_TestApp__CParity__CCoordinate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVectorView_1_TestApp__CParity__CCoordinate_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVectorView_1_TestApp__CParity__CCoordinate_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVectorView_1_TestApp__CParity__CCoordinate_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#endif /* COBJMACROS */



#endif // ____FIVectorView_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__



#if !defined(____FIVector_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__)
#define ____FIVector_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__

typedef interface __FIVector_1_TestApp__CParity__CCoordinate __FIVector_1_TestApp__CParity__CCoordinate;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVector_1_TestApp__CParity__CCoordinate;

typedef struct __FIVector_1_TestApp__CParity__CCoordinateVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
            /* [in] */ __RPC__in REFIID riid,
            /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__deref_out_opt struct __x_TestApp_CParity_CCoordinate *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ unsigned int index,
        /* [retval][out] */ __RPC__deref_out_opt struct __x_TestApp_CParity_CCoordinate *item);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
        __RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [retval][out] */ __RPC__out unsigned int *size);

    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__deref_out_opt __FIVectorView_1_TestApp__CParity__CCoordinate **view);

    HRESULT ( STDMETHODCALLTYPE *IndexOf )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ __RPC__in struct __x_TestApp_CParity_CCoordinate item,
        /* [out] */ __RPC__out unsigned int *index,
        /* [retval][out] */ __RPC__out boolean *found);

    HRESULT ( STDMETHODCALLTYPE *SetAt )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in struct __x_TestApp_CParity_CCoordinate item);

    HRESULT ( STDMETHODCALLTYPE *InsertAt )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in struct __x_TestApp_CParity_CCoordinate item);

    HRESULT ( STDMETHODCALLTYPE *RemoveAt )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This, /* [in] */ unsigned int index);
    HRESULT ( STDMETHODCALLTYPE *Append )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This, /* [in] */ __RPC__in struct __x_TestApp_CParity_CCoordinate item);
    HRESULT ( STDMETHODCALLTYPE *RemoveAtEnd )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ unsigned int startIndex,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) struct __x_TestApp_CParity_CCoordinate *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    HRESULT ( STDMETHODCALLTYPE *ReplaceAll )(__RPC__in __FIVector_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ unsigned int count,
        /* [size_is][in] */ __RPC__in_ecount_full(count) struct __x_TestApp_CParity_CCoordinate *value);

    END_INTERFACE
} __FIVector_1_TestApp__CParity__CCoordinateVtbl;

interface __FIVector_1_TestApp__CParity__CCoordinate
{
    CONST_VTBL struct __FIVector_1_TestApp__CParity__CCoordinateVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVector_1_TestApp__CParity__CCoordinate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVector_1_TestApp__CParity__CCoordinate_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVector_1_TestApp__CParity__CCoordinate_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_SetAt(This,index,item)	\
    ( (This)->lpVtbl -> SetAt(This,index,item) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_InsertAt(This,index,item)	\
    ( (This)->lpVtbl -> InsertAt(This,index,item) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_RemoveAt(This,index)	\
    ( (This)->lpVtbl -> RemoveAt(This,index) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_Append(This,item)	\
    ( (This)->lpVtbl -> Append(This,item) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_RemoveAtEnd(This)	\
    ( (This)->lpVtbl -> RemoveAtEnd(This) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#define __FIVector_1_TestApp__CParity__CCoordinate_ReplaceAll(This,count,value)	\
    ( (This)->lpVtbl -> ReplaceAll(This,count,value) ) 

#endif /* COBJMACROS */



#endif // ____FIVector_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__



#if !defined(____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__)
#define ____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__

typedef interface __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate;

// Forward declare the async operation.
typedef interface __FIAsyncOperation_1_TestApp__CParity__CCoordinate __FIAsyncOperation_1_TestApp__CParity__CCoordinate;

typedef struct __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinateVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate * This,/* [in] */ __RPC__in_opt __FIAsyncOperation_1_TestApp__CParity__CCoordinate *asyncInfo, /* [in] */ AsyncStatus status);
    END_INTERFACE
} __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinateVtbl;

interface __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate
{
    CONST_VTBL struct __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinateVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_Invoke(This,asyncInfo,status)	\
    ( (This)->lpVtbl -> Invoke(This,asyncInfo,status) ) 
#endif /* COBJMACROS */


#endif // ____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__



#if !defined(____FIAsyncOperation_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__)
#define ____FIAsyncOperation_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__

typedef interface __FIAsyncOperation_1_TestApp__CParity__CCoordinate __FIAsyncOperation_1_TestApp__CParity__CCoordinate;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIAsyncOperation_1_TestApp__CParity__CCoordinate;

typedef struct __FIAsyncOperation_1_TestApp__CParity__CCoordinateVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_Completed )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This, /* [in] */ __RPC__in_opt __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate *handler);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Completed )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__deref_out_opt __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CCoordinate **handler);
    HRESULT ( STDMETHODCALLTYPE *GetResults )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CCoordinate * This, /* [retval][out] */ __RPC__out struct __x_TestApp_CParity_CCoordinate *results);
    END_INTERFACE
} __FIAsyncOperation_1_TestApp__CParity__CCoordinateVtbl;

interface __FIAsyncOperation_1_TestApp__CParity__CCoordinate
{
    CONST_VTBL struct __FIAsyncOperation_1_TestApp__CParity__CCoordinateVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_put_Completed(This,handler)	\
    ( (This)->lpVtbl -> put_Completed(This,handler) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_get_Completed(This,handler)	\
    ( (This)->lpVtbl -> get_Completed(This,handler) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CCoordinate_GetResults(This,results)	\
    ( (This)->lpVtbl -> GetResults(This,results) ) 
#endif /* COBJMACROS */


#endif // ____FIAsyncOperation_1_TestApp__CParity__CCoordinate_INTERFACE_DEFINED__




/*
 *
 * Struct TestApp.Parity.Coordinate
 *
 */

/* [version] */
struct __x_TestApp_CParity_CCoordinate
{
    double Lat;
    double Lon;
};



/*
 *
 * Interface TestApp.Parity.IGeocoderService
 *
 * Interface is a part of the implementation of type TestApp.Parity.GeocoderService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IGeocoderService:HRESULT Geocode(String,TestApp.Parity.Coordinate*);HRESULT ReverseGeocode(TestApp.Parity.Coordinate,String*);HRESULT GeocodeMultiple(Windows.Foundation.Collections.IVector`1<String>*,Windows.Foundation.Collections.IVector`1<TestApp.Parity.Coordinate>**);HRESULT GeocodeAsync(String,Windows.Foundation.IAsyncOperation`1<TestApp.Parity.Coordinate>**);HRESULT IsValidCoordinate(TestApp.Parity.Coordinate,Boolean*);HRESULT Distance(TestApp.Parity.Coordinate,TestApp.Parity.Coordinate,Double*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIGeocoderService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIGeocoderService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IGeocoderService[] = L"TestApp.Parity.IGeocoderService";
/* [uuid("e531f636-e1fe-5b63-802c-f7ef0782b782"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIGeocoderServiceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIGeocoderService * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIGeocoderService * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIGeocoderService * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIGeocoderService * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIGeocoderService * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIGeocoderService * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Geocode )(
        __x_TestApp_CParity_CIGeocoderService * This,
        /* [in] */HSTRING address,
        /* [out, retval] */struct __x_TestApp_CParity_CCoordinate * result
        );
    HRESULT ( STDMETHODCALLTYPE *ReverseGeocode )(
        __x_TestApp_CParity_CIGeocoderService * This,
        /* [in] */struct __x_TestApp_CParity_CCoordinate coord,
        /* [out, retval] */HSTRING * result
        );
    HRESULT ( STDMETHODCALLTYPE *GeocodeMultiple )(
        __x_TestApp_CParity_CIGeocoderService * This,
        /* [in] */__FIVector_1_HSTRING * addresses,
        /* [out, retval] */__FIVector_1_TestApp__CParity__CCoordinate * * result
        );
    HRESULT ( STDMETHODCALLTYPE *GeocodeAsync )(
        __x_TestApp_CParity_CIGeocoderService * This,
        /* [in] */HSTRING address,
        /* [out, retval] */__FIAsyncOperation_1_TestApp__CParity__CCoordinate * * operation
        );
    HRESULT ( STDMETHODCALLTYPE *IsValidCoordinate )(
        __x_TestApp_CParity_CIGeocoderService * This,
        /* [in] */struct __x_TestApp_CParity_CCoordinate coord,
        /* [out, retval] */boolean * result
        );
    HRESULT ( STDMETHODCALLTYPE *Distance )(
        __x_TestApp_CParity_CIGeocoderService * This,
        /* [in] */struct __x_TestApp_CParity_CCoordinate from,
        /* [in] */struct __x_TestApp_CParity_CCoordinate to,
        /* [out, retval] */double * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIGeocoderServiceVtbl;

interface __x_TestApp_CParity_CIGeocoderService
{
    CONST_VTBL struct __x_TestApp_CParity_CIGeocoderServiceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIGeocoderService_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIGeocoderService_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIGeocoderService_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIGeocoderService_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIGeocoderService_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIGeocoderService_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIGeocoderService_Geocode(This,address,result) \
    ( (This)->lpVtbl->Geocode(This,address,result) )

#define __x_TestApp_CParity_CIGeocoderService_ReverseGeocode(This,coord,result) \
    ( (This)->lpVtbl->ReverseGeocode(This,coord,result) )

#define __x_TestApp_CParity_CIGeocoderService_GeocodeMultiple(This,addresses,result) \
    ( (This)->lpVtbl->GeocodeMultiple(This,addresses,result) )

#define __x_TestApp_CParity_CIGeocoderService_GeocodeAsync(This,address,operation) \
    ( (This)->lpVtbl->GeocodeAsync(This,address,operation) )

#define __x_TestApp_CParity_CIGeocoderService_IsValidCoordinate(This,coord,result) \
    ( (This)->lpVtbl->IsValidCoordinate(This,coord,result) )

#define __x_TestApp_CParity_CIGeocoderService_Distance(This,from,to,result) \
    ( (This)->lpVtbl->Distance(This,from,to,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIGeocoderService;
#endif /* !defined(____x_TestApp_CParity_CIGeocoderService_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.GeocoderService
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IGeocoderService ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_GeocoderService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_GeocoderService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_GeocoderService[] = L"TestApp.Parity.GeocoderService";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_interface_method_variety_p_h__

#endif // __parity_interface_method_variety_h__
