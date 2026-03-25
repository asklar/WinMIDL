/* Header file automatically generated from parity_return_types.idl */
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
#ifndef __parity_return_types_h__
#define __parity_return_types_h__
#ifndef __parity_return_types_p_h__
#define __parity_return_types_p_h__


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
#ifndef ____x_TestApp_CParity_CIReturnCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIReturnCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IReturnCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIReturnCallback TestApp::Parity::IReturnCallback

#endif // ____x_TestApp_CParity_CIReturnCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIReturnTypeWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIReturnTypeWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IReturnTypeWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIReturnTypeWidget TestApp::Parity::IReturnTypeWidget

#endif // ____x_TestApp_CParity_CIReturnTypeWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        struct ReturnData;
        
    } /* Parity */
} /* TestApp */


#ifndef DEF___FIIterator_1_TestApp__CParity__CReturnData_USE
#define DEF___FIIterator_1_TestApp__CParity__CReturnData_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("ec4d8e80-535a-590f-bd34-ef2ad5123724"))
IIterator<struct TestApp::Parity::ReturnData> : IIterator_impl<struct TestApp::Parity::ReturnData> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<TestApp.Parity.ReturnData>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<struct TestApp::Parity::ReturnData> __FIIterator_1_TestApp__CParity__CReturnData_t;
#define __FIIterator_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::__FIIterator_1_TestApp__CParity__CReturnData_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::IIterator<TestApp::Parity::ReturnData>
//#define __FIIterator_1_TestApp__CParity__CReturnData_t Windows::Foundation::Collections::IIterator<TestApp::Parity::ReturnData>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1_TestApp__CParity__CReturnData_USE */





#ifndef DEF___FIIterable_1_TestApp__CParity__CReturnData_USE
#define DEF___FIIterable_1_TestApp__CParity__CReturnData_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("a8bb6926-eafe-5ae4-8541-3a8adb59a45d"))
IIterable<struct TestApp::Parity::ReturnData> : IIterable_impl<struct TestApp::Parity::ReturnData> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<TestApp.Parity.ReturnData>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<struct TestApp::Parity::ReturnData> __FIIterable_1_TestApp__CParity__CReturnData_t;
#define __FIIterable_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::__FIIterable_1_TestApp__CParity__CReturnData_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::IIterable<TestApp::Parity::ReturnData>
//#define __FIIterable_1_TestApp__CParity__CReturnData_t Windows::Foundation::Collections::IIterable<TestApp::Parity::ReturnData>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1_TestApp__CParity__CReturnData_USE */





#ifndef DEF___FIVectorView_1_TestApp__CParity__CReturnData_USE
#define DEF___FIVectorView_1_TestApp__CParity__CReturnData_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("23afcbdf-549f-558c-beb6-1958fc80cfb3"))
IVectorView<struct TestApp::Parity::ReturnData> : IVectorView_impl<struct TestApp::Parity::ReturnData> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVectorView`1<TestApp.Parity.ReturnData>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVectorView<struct TestApp::Parity::ReturnData> __FIVectorView_1_TestApp__CParity__CReturnData_t;
#define __FIVectorView_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::__FIVectorView_1_TestApp__CParity__CReturnData_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVectorView_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::IVectorView<TestApp::Parity::ReturnData>
//#define __FIVectorView_1_TestApp__CParity__CReturnData_t Windows::Foundation::Collections::IVectorView<TestApp::Parity::ReturnData>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVectorView_1_TestApp__CParity__CReturnData_USE */





#ifndef DEF___FIVector_1_TestApp__CParity__CReturnData_USE
#define DEF___FIVector_1_TestApp__CParity__CReturnData_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("e87e7276-655c-57e4-99d8-17a84f4c7213"))
IVector<struct TestApp::Parity::ReturnData> : IVector_impl<struct TestApp::Parity::ReturnData> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVector`1<TestApp.Parity.ReturnData>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVector<struct TestApp::Parity::ReturnData> __FIVector_1_TestApp__CParity__CReturnData_t;
#define __FIVector_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::__FIVector_1_TestApp__CParity__CReturnData_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVector_1_TestApp__CParity__CReturnData Windows::Foundation::Collections::IVector<TestApp::Parity::ReturnData>
//#define __FIVector_1_TestApp__CParity__CReturnData_t Windows::Foundation::Collections::IVector<TestApp::Parity::ReturnData>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVector_1_TestApp__CParity__CReturnData_USE */





#ifndef DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_USE
#define DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("b451f256-cac4-5dd2-8a17-4d73e9a974e1"))
IAsyncOperationCompletedHandler<struct TestApp::Parity::ReturnData> : IAsyncOperationCompletedHandler_impl<struct TestApp::Parity::ReturnData> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.AsyncOperationCompletedHandler`1<TestApp.Parity.ReturnData>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IAsyncOperationCompletedHandler<struct TestApp::Parity::ReturnData> __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_t;
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData Windows::Foundation::__FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData Windows::Foundation::IAsyncOperationCompletedHandler<TestApp::Parity::ReturnData>
//#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_t Windows::Foundation::IAsyncOperationCompletedHandler<TestApp::Parity::ReturnData>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_USE */





#ifndef DEF___FIAsyncOperation_1_TestApp__CParity__CReturnData_USE
#define DEF___FIAsyncOperation_1_TestApp__CParity__CReturnData_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("2358d733-c929-52ee-af23-62fe2ddae61a"))
IAsyncOperation<struct TestApp::Parity::ReturnData> : IAsyncOperation_impl<struct TestApp::Parity::ReturnData> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IAsyncOperation`1<TestApp.Parity.ReturnData>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IAsyncOperation<struct TestApp::Parity::ReturnData> __FIAsyncOperation_1_TestApp__CParity__CReturnData_t;
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData Windows::Foundation::__FIAsyncOperation_1_TestApp__CParity__CReturnData_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIAsyncOperation_1_TestApp__CParity__CReturnData Windows::Foundation::IAsyncOperation<TestApp::Parity::ReturnData>
//#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_t Windows::Foundation::IAsyncOperation<TestApp::Parity::ReturnData>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIAsyncOperation_1_TestApp__CParity__CReturnData_USE */




namespace TestApp {
    namespace Parity {
        enum ReturnStatus : int;
    } /* Parity */
} /* TestApp */


#ifndef DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_USE
#define DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("bb2e7ba3-7bbd-5ae4-a861-77e293e23492"))
IAsyncOperationCompletedHandler<enum TestApp::Parity::ReturnStatus> : IAsyncOperationCompletedHandler_impl<enum TestApp::Parity::ReturnStatus> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.AsyncOperationCompletedHandler`1<TestApp.Parity.ReturnStatus>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IAsyncOperationCompletedHandler<enum TestApp::Parity::ReturnStatus> __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_t;
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus Windows::Foundation::__FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus Windows::Foundation::IAsyncOperationCompletedHandler<TestApp::Parity::ReturnStatus>
//#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_t Windows::Foundation::IAsyncOperationCompletedHandler<TestApp::Parity::ReturnStatus>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_USE */





#ifndef DEF___FIAsyncOperation_1_TestApp__CParity__CReturnStatus_USE
#define DEF___FIAsyncOperation_1_TestApp__CParity__CReturnStatus_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("39ecc4fd-096a-5920-8c23-6454c04a8da6"))
IAsyncOperation<enum TestApp::Parity::ReturnStatus> : IAsyncOperation_impl<enum TestApp::Parity::ReturnStatus> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IAsyncOperation`1<TestApp.Parity.ReturnStatus>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IAsyncOperation<enum TestApp::Parity::ReturnStatus> __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_t;
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus Windows::Foundation::__FIAsyncOperation_1_TestApp__CParity__CReturnStatus_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus Windows::Foundation::IAsyncOperation<TestApp::Parity::ReturnStatus>
//#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_t Windows::Foundation::IAsyncOperation<TestApp::Parity::ReturnStatus>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIAsyncOperation_1_TestApp__CParity__CReturnStatus_USE */






/*
 *
 * Struct TestApp.Parity.ReturnData
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct ReturnData
        {
            HSTRING Name;
            int Value;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Struct TestApp.Parity.ReturnStatus
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version, v1_enum] */
        enum ReturnStatus : int
        {
            ReturnStatus_Ok = 0,
            ReturnStatus_Error = 1,
            ReturnStatus_Pending = 2,
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Delegate TestApp.Parity.ReturnCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ReturnCallback:HRESULT Invoke(TestApp.Parity.ReturnData);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIReturnCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIReturnCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("af7dbf50-b5d2-547c-a18b-b024c232bad0"), version, object] */
        MIDL_INTERFACE("af7dbf50-b5d2-547c-a18b-b024c232bad0")
        IReturnCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */struct TestApp::Parity::ReturnData data
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IReturnCallback=__uuidof(IReturnCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIReturnCallback;
#endif /* !defined(____x_TestApp_CParity_CIReturnCallback_INTERFACE_DEFINED__) */


namespace TestApp {
    namespace Parity {
        class ReturnTypeWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IReturnTypeWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ReturnTypeWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IReturnTypeWidget:HRESULT GetData(TestApp.Parity.ReturnData*);HRESULT GetStatus(TestApp.Parity.ReturnStatus*);HRESULT GetAllData(Windows.Foundation.Collections.IVector`1<TestApp.Parity.ReturnData>**);HRESULT GetDataAsync(Windows.Foundation.IAsyncOperation`1<TestApp.Parity.ReturnData>**);HRESULT GetStatusAsync(Windows.Foundation.IAsyncOperation`1<TestApp.Parity.ReturnStatus>**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIReturnTypeWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIReturnTypeWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IReturnTypeWidget[] = L"TestApp.Parity.IReturnTypeWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("910688b7-fdcb-5130-bccb-703bfbb2172a"), version, object, exclusiveto] */
        MIDL_INTERFACE("910688b7-fdcb-5130-bccb-703bfbb2172a")
        IReturnTypeWidget : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE GetData(
                /* [out, retval] */struct TestApp::Parity::ReturnData * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetStatus(
                /* [out, retval] */TestApp::Parity::ReturnStatus * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetAllData(
                /* [out, retval] */__FIVector_1_TestApp__CParity__CReturnData * * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetDataAsync(
                /* [out, retval] */__FIAsyncOperation_1_TestApp__CParity__CReturnData * * operation
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetStatusAsync(
                /* [out, retval] */__FIAsyncOperation_1_TestApp__CParity__CReturnStatus * * operation
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IReturnTypeWidget=__uuidof(IReturnTypeWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIReturnTypeWidget;
#endif /* !defined(____x_TestApp_CParity_CIReturnTypeWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ReturnTypeWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IReturnTypeWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ReturnTypeWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ReturnTypeWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ReturnTypeWidget[] = L"TestApp.Parity.ReturnTypeWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIReturnCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIReturnCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIReturnCallback __x_TestApp_CParity_CIReturnCallback;

#endif // ____x_TestApp_CParity_CIReturnCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIReturnTypeWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIReturnTypeWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIReturnTypeWidget __x_TestApp_CParity_CIReturnTypeWidget;

#endif // ____x_TestApp_CParity_CIReturnTypeWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions
struct __x_TestApp_CParity_CReturnData;

#if !defined(____FIIterator_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__)
#define ____FIIterator_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__

typedef interface __FIIterator_1_TestApp__CParity__CReturnData __FIIterator_1_TestApp__CParity__CReturnData;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1_TestApp__CParity__CReturnData;

typedef struct __FIIterator_1_TestApp__CParity__CReturnDataVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__out struct __x_TestApp_CParity_CReturnData *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1_TestApp__CParity__CReturnData * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) struct __x_TestApp_CParity_CReturnData *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1_TestApp__CParity__CReturnDataVtbl;

interface __FIIterator_1_TestApp__CParity__CReturnData
{
    CONST_VTBL struct __FIIterator_1_TestApp__CParity__CReturnDataVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1_TestApp__CParity__CReturnData_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1_TestApp__CParity__CReturnData_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1_TestApp__CParity__CReturnData_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1_TestApp__CParity__CReturnData_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__



#if !defined(____FIIterable_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__)
#define ____FIIterable_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__

typedef interface __FIIterable_1_TestApp__CParity__CReturnData __FIIterable_1_TestApp__CParity__CReturnData;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1_TestApp__CParity__CReturnData;

typedef  struct __FIIterable_1_TestApp__CParity__CReturnDataVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1_TestApp__CParity__CReturnData **first);

    END_INTERFACE
} __FIIterable_1_TestApp__CParity__CReturnDataVtbl;

interface __FIIterable_1_TestApp__CParity__CReturnData
{
    CONST_VTBL struct __FIIterable_1_TestApp__CParity__CReturnDataVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1_TestApp__CParity__CReturnData_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1_TestApp__CParity__CReturnData_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1_TestApp__CParity__CReturnData_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1_TestApp__CParity__CReturnData_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1_TestApp__CParity__CReturnData_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1_TestApp__CParity__CReturnData_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1_TestApp__CParity__CReturnData_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__



#if !defined(____FIVectorView_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__)
#define ____FIVectorView_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__

typedef interface __FIVectorView_1_TestApp__CParity__CReturnData __FIVectorView_1_TestApp__CParity__CReturnData;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVectorView_1_TestApp__CParity__CReturnData;

typedef struct __FIVectorView_1_TestApp__CParity__CReturnDataVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This);

    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
        __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
            /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
        __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
            /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )( 
                                         __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
                                         /* [in] */ unsigned int index,
                                         /* [retval][out] */ __RPC__out struct __x_TestApp_CParity_CReturnData *item);

        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
            __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
            /* [retval][out] */ __RPC__out unsigned int *size);

        HRESULT ( STDMETHODCALLTYPE *IndexOf )( 
                                               __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
            /* [in] */ struct __x_TestApp_CParity_CReturnData item,
            /* [out] */ __RPC__out unsigned int *index,
            /* [retval][out] */ __RPC__out boolean *found);

        HRESULT ( STDMETHODCALLTYPE *GetMany )( 
                                               __RPC__in __FIVectorView_1_TestApp__CParity__CReturnData * This,
            /* [in] */ unsigned int startIndex,
            /* [in] */ unsigned int capacity,
            /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) struct __x_TestApp_CParity_CReturnData *items,
            /* [retval][out] */ __RPC__out unsigned int *actual);

        END_INTERFACE
} __FIVectorView_1_TestApp__CParity__CReturnDataVtbl;

interface __FIVectorView_1_TestApp__CParity__CReturnData
{
    CONST_VTBL struct __FIVectorView_1_TestApp__CParity__CReturnDataVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVectorView_1_TestApp__CParity__CReturnData_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVectorView_1_TestApp__CParity__CReturnData_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVectorView_1_TestApp__CParity__CReturnData_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVectorView_1_TestApp__CParity__CReturnData_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#endif /* COBJMACROS */



#endif // ____FIVectorView_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__



#if !defined(____FIVector_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__)
#define ____FIVector_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__

typedef interface __FIVector_1_TestApp__CParity__CReturnData __FIVector_1_TestApp__CParity__CReturnData;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVector_1_TestApp__CParity__CReturnData;

typedef struct __FIVector_1_TestApp__CParity__CReturnDataVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
            /* [in] */ __RPC__in REFIID riid,
            /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__deref_out_opt struct __x_TestApp_CParity_CReturnData *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [in] */ unsigned int index,
        /* [retval][out] */ __RPC__deref_out_opt struct __x_TestApp_CParity_CReturnData *item);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
        __RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [retval][out] */ __RPC__out unsigned int *size);

    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__deref_out_opt __FIVectorView_1_TestApp__CParity__CReturnData **view);

    HRESULT ( STDMETHODCALLTYPE *IndexOf )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [in] */ __RPC__in struct __x_TestApp_CParity_CReturnData item,
        /* [out] */ __RPC__out unsigned int *index,
        /* [retval][out] */ __RPC__out boolean *found);

    HRESULT ( STDMETHODCALLTYPE *SetAt )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in struct __x_TestApp_CParity_CReturnData item);

    HRESULT ( STDMETHODCALLTYPE *InsertAt )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in struct __x_TestApp_CParity_CReturnData item);

    HRESULT ( STDMETHODCALLTYPE *RemoveAt )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This, /* [in] */ unsigned int index);
    HRESULT ( STDMETHODCALLTYPE *Append )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This, /* [in] */ __RPC__in struct __x_TestApp_CParity_CReturnData item);
    HRESULT ( STDMETHODCALLTYPE *RemoveAtEnd )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [in] */ unsigned int startIndex,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) struct __x_TestApp_CParity_CReturnData *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    HRESULT ( STDMETHODCALLTYPE *ReplaceAll )(__RPC__in __FIVector_1_TestApp__CParity__CReturnData * This,
        /* [in] */ unsigned int count,
        /* [size_is][in] */ __RPC__in_ecount_full(count) struct __x_TestApp_CParity_CReturnData *value);

    END_INTERFACE
} __FIVector_1_TestApp__CParity__CReturnDataVtbl;

interface __FIVector_1_TestApp__CParity__CReturnData
{
    CONST_VTBL struct __FIVector_1_TestApp__CParity__CReturnDataVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVector_1_TestApp__CParity__CReturnData_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVector_1_TestApp__CParity__CReturnData_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVector_1_TestApp__CParity__CReturnData_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_SetAt(This,index,item)	\
    ( (This)->lpVtbl -> SetAt(This,index,item) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_InsertAt(This,index,item)	\
    ( (This)->lpVtbl -> InsertAt(This,index,item) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_RemoveAt(This,index)	\
    ( (This)->lpVtbl -> RemoveAt(This,index) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_Append(This,item)	\
    ( (This)->lpVtbl -> Append(This,item) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_RemoveAtEnd(This)	\
    ( (This)->lpVtbl -> RemoveAtEnd(This) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#define __FIVector_1_TestApp__CParity__CReturnData_ReplaceAll(This,count,value)	\
    ( (This)->lpVtbl -> ReplaceAll(This,count,value) ) 

#endif /* COBJMACROS */



#endif // ____FIVector_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__



#if !defined(____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__)
#define ____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__

typedef interface __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData;

// Forward declare the async operation.
typedef interface __FIAsyncOperation_1_TestApp__CParity__CReturnData __FIAsyncOperation_1_TestApp__CParity__CReturnData;

typedef struct __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnDataVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData * This,/* [in] */ __RPC__in_opt __FIAsyncOperation_1_TestApp__CParity__CReturnData *asyncInfo, /* [in] */ AsyncStatus status);
    END_INTERFACE
} __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnDataVtbl;

interface __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData
{
    CONST_VTBL struct __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnDataVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_Invoke(This,asyncInfo,status)	\
    ( (This)->lpVtbl -> Invoke(This,asyncInfo,status) ) 
#endif /* COBJMACROS */


#endif // ____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__



#if !defined(____FIAsyncOperation_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__)
#define ____FIAsyncOperation_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__

typedef interface __FIAsyncOperation_1_TestApp__CParity__CReturnData __FIAsyncOperation_1_TestApp__CParity__CReturnData;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIAsyncOperation_1_TestApp__CParity__CReturnData;

typedef struct __FIAsyncOperation_1_TestApp__CParity__CReturnDataVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_Completed )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This, /* [in] */ __RPC__in_opt __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData *handler);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Completed )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__deref_out_opt __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnData **handler);
    HRESULT ( STDMETHODCALLTYPE *GetResults )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnData * This, /* [retval][out] */ __RPC__out struct __x_TestApp_CParity_CReturnData *results);
    END_INTERFACE
} __FIAsyncOperation_1_TestApp__CParity__CReturnDataVtbl;

interface __FIAsyncOperation_1_TestApp__CParity__CReturnData
{
    CONST_VTBL struct __FIAsyncOperation_1_TestApp__CParity__CReturnDataVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_put_Completed(This,handler)	\
    ( (This)->lpVtbl -> put_Completed(This,handler) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_get_Completed(This,handler)	\
    ( (This)->lpVtbl -> get_Completed(This,handler) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnData_GetResults(This,results)	\
    ( (This)->lpVtbl -> GetResults(This,results) ) 
#endif /* COBJMACROS */


#endif // ____FIAsyncOperation_1_TestApp__CParity__CReturnData_INTERFACE_DEFINED__


enum __x_TestApp_CParity_CReturnStatus;
#if !defined(____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_INTERFACE_DEFINED__)
#define ____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_INTERFACE_DEFINED__

typedef interface __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus;

// Forward declare the async operation.
typedef interface __FIAsyncOperation_1_TestApp__CParity__CReturnStatus __FIAsyncOperation_1_TestApp__CParity__CReturnStatus;

typedef struct __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatusVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus * This,/* [in] */ __RPC__in_opt __FIAsyncOperation_1_TestApp__CParity__CReturnStatus *asyncInfo, /* [in] */ AsyncStatus status);
    END_INTERFACE
} __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatusVtbl;

interface __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus
{
    CONST_VTBL struct __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatusVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_Invoke(This,asyncInfo,status)	\
    ( (This)->lpVtbl -> Invoke(This,asyncInfo,status) ) 
#endif /* COBJMACROS */


#endif // ____FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus_INTERFACE_DEFINED__



#if !defined(____FIAsyncOperation_1_TestApp__CParity__CReturnStatus_INTERFACE_DEFINED__)
#define ____FIAsyncOperation_1_TestApp__CParity__CReturnStatus_INTERFACE_DEFINED__

typedef interface __FIAsyncOperation_1_TestApp__CParity__CReturnStatus __FIAsyncOperation_1_TestApp__CParity__CReturnStatus;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIAsyncOperation_1_TestApp__CParity__CReturnStatus;

typedef struct __FIAsyncOperation_1_TestApp__CParity__CReturnStatusVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_Completed )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This, /* [in] */ __RPC__in_opt __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus *handler);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Completed )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This, /* [retval][out] */ __RPC__deref_out_opt __FIAsyncOperationCompletedHandler_1_TestApp__CParity__CReturnStatus **handler);
    HRESULT ( STDMETHODCALLTYPE *GetResults )(__RPC__in __FIAsyncOperation_1_TestApp__CParity__CReturnStatus * This, /* [retval][out] */ __RPC__out enum __x_TestApp_CParity_CReturnStatus *results);
    END_INTERFACE
} __FIAsyncOperation_1_TestApp__CParity__CReturnStatusVtbl;

interface __FIAsyncOperation_1_TestApp__CParity__CReturnStatus
{
    CONST_VTBL struct __FIAsyncOperation_1_TestApp__CParity__CReturnStatusVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_put_Completed(This,handler)	\
    ( (This)->lpVtbl -> put_Completed(This,handler) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_get_Completed(This,handler)	\
    ( (This)->lpVtbl -> get_Completed(This,handler) ) 
#define __FIAsyncOperation_1_TestApp__CParity__CReturnStatus_GetResults(This,results)	\
    ( (This)->lpVtbl -> GetResults(This,results) ) 
#endif /* COBJMACROS */


#endif // ____FIAsyncOperation_1_TestApp__CParity__CReturnStatus_INTERFACE_DEFINED__




/*
 *
 * Struct TestApp.Parity.ReturnData
 *
 */

/* [version] */
struct __x_TestApp_CParity_CReturnData
{
    HSTRING Name;
    int Value;
};


/*
 *
 * Struct TestApp.Parity.ReturnStatus
 *
 */

/* [version, v1_enum] */
enum __x_TestApp_CParity_CReturnStatus
{
    ReturnStatus_Ok = 0,
    ReturnStatus_Error = 1,
    ReturnStatus_Pending = 2,
};


/*
 *
 * Delegate TestApp.Parity.ReturnCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ReturnCallback:HRESULT Invoke(TestApp.Parity.ReturnData);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIReturnCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIReturnCallback_INTERFACE_DEFINED__
/* [uuid("af7dbf50-b5d2-547c-a18b-b024c232bad0"), version, object] */
typedef struct __x_TestApp_CParity_CIReturnCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIReturnCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIReturnCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIReturnCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIReturnCallback * This,
        /* [in] */struct __x_TestApp_CParity_CReturnData data
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIReturnCallbackVtbl;

interface __x_TestApp_CParity_CIReturnCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CIReturnCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIReturnCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIReturnCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIReturnCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIReturnCallback_Invoke(This,data) \
    ( (This)->lpVtbl->Invoke(This,data) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIReturnCallback;
#endif /* !defined(____x_TestApp_CParity_CIReturnCallback_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IReturnTypeWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ReturnTypeWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IReturnTypeWidget:HRESULT GetData(TestApp.Parity.ReturnData*);HRESULT GetStatus(TestApp.Parity.ReturnStatus*);HRESULT GetAllData(Windows.Foundation.Collections.IVector`1<TestApp.Parity.ReturnData>**);HRESULT GetDataAsync(Windows.Foundation.IAsyncOperation`1<TestApp.Parity.ReturnData>**);HRESULT GetStatusAsync(Windows.Foundation.IAsyncOperation`1<TestApp.Parity.ReturnStatus>**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIReturnTypeWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIReturnTypeWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IReturnTypeWidget[] = L"TestApp.Parity.IReturnTypeWidget";
/* [uuid("910688b7-fdcb-5130-bccb-703bfbb2172a"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIReturnTypeWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIReturnTypeWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIReturnTypeWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIReturnTypeWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIReturnTypeWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIReturnTypeWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIReturnTypeWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *GetData )(
        __x_TestApp_CParity_CIReturnTypeWidget * This,
        /* [out, retval] */struct __x_TestApp_CParity_CReturnData * result
        );
    HRESULT ( STDMETHODCALLTYPE *GetStatus )(
        __x_TestApp_CParity_CIReturnTypeWidget * This,
        /* [out, retval] */enum __x_TestApp_CParity_CReturnStatus * result
        );
    HRESULT ( STDMETHODCALLTYPE *GetAllData )(
        __x_TestApp_CParity_CIReturnTypeWidget * This,
        /* [out, retval] */__FIVector_1_TestApp__CParity__CReturnData * * result
        );
    HRESULT ( STDMETHODCALLTYPE *GetDataAsync )(
        __x_TestApp_CParity_CIReturnTypeWidget * This,
        /* [out, retval] */__FIAsyncOperation_1_TestApp__CParity__CReturnData * * operation
        );
    HRESULT ( STDMETHODCALLTYPE *GetStatusAsync )(
        __x_TestApp_CParity_CIReturnTypeWidget * This,
        /* [out, retval] */__FIAsyncOperation_1_TestApp__CParity__CReturnStatus * * operation
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIReturnTypeWidgetVtbl;

interface __x_TestApp_CParity_CIReturnTypeWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIReturnTypeWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIReturnTypeWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIReturnTypeWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIReturnTypeWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetData(This,result) \
    ( (This)->lpVtbl->GetData(This,result) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetStatus(This,result) \
    ( (This)->lpVtbl->GetStatus(This,result) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetAllData(This,result) \
    ( (This)->lpVtbl->GetAllData(This,result) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetDataAsync(This,operation) \
    ( (This)->lpVtbl->GetDataAsync(This,operation) )

#define __x_TestApp_CParity_CIReturnTypeWidget_GetStatusAsync(This,operation) \
    ( (This)->lpVtbl->GetStatusAsync(This,operation) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIReturnTypeWidget;
#endif /* !defined(____x_TestApp_CParity_CIReturnTypeWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ReturnTypeWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IReturnTypeWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ReturnTypeWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ReturnTypeWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ReturnTypeWidget[] = L"TestApp.Parity.ReturnTypeWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_return_types_p_h__

#endif // __parity_return_types_h__
