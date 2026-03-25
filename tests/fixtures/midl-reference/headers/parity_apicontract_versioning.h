/* Header file automatically generated from parity_apicontract_versioning.idl */
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
#ifndef __parity_apicontract_versioning_h__
#define __parity_apicontract_versioning_h__
#ifndef __parity_apicontract_versioning_p_h__
#define __parity_apicontract_versioning_p_h__


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


//  API Contract Inclusion Definitions
#if !defined(SPECIFIC_API_CONTRACT_DEFINITIONS)
#if !defined(TESTAPP_PARITY_SECONDARYCONTRACT_VERSION)
#define TESTAPP_PARITY_SECONDARYCONTRACT_VERSION 0x20000
#endif // defined(TESTAPP_PARITY_SECONDARYCONTRACT_VERSION)

#if !defined(TESTAPP_PARITY_TESTAPICONTRACT_VERSION)
#define TESTAPP_PARITY_TESTAPICONTRACT_VERSION 0x30000
#endif // defined(TESTAPP_PARITY_TESTAPICONTRACT_VERSION)

#endif // defined(SPECIFIC_API_CONTRACT_DEFINITIONS)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"
// Importing Collections header
#include <windows.foundation.collections.h>

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIApiCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IApiCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIApiCallback TestApp::Parity::IApiCallback

#endif // ____x_TestApp_CParity_CIApiCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiEndpoint_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiEndpoint_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IApiEndpoint;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIApiEndpoint TestApp::Parity::IApiEndpoint

#endif // ____x_TestApp_CParity_CIApiEndpoint_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiService_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiService_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IApiService;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIApiService TestApp::Parity::IApiService

#endif // ____x_TestApp_CParity_CIApiService_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiService2_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiService2_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IApiService2;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIApiService2 TestApp::Parity::IApiService2

#endif // ____x_TestApp_CParity_CIApiService2_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiService3_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiService3_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IApiService3;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIApiService3 TestApp::Parity::IApiService3

#endif // ____x_TestApp_CParity_CIApiService3_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiServiceStatics_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiServiceStatics_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IApiServiceStatics;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIApiServiceStatics TestApp::Parity::IApiServiceStatics

#endif // ____x_TestApp_CParity_CIApiServiceStatics_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISecondaryService_FWD_DEFINED__
#define ____x_TestApp_CParity_CISecondaryService_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ISecondaryService;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CISecondaryService TestApp::Parity::ISecondaryService

#endif // ____x_TestApp_CParity_CISecondaryService_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISecondaryService2_FWD_DEFINED__
#define ____x_TestApp_CParity_CISecondaryService2_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ISecondaryService2;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CISecondaryService2 TestApp::Parity::ISecondaryService2

#endif // ____x_TestApp_CParity_CISecondaryService2_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class ApiService;
    } /* Parity */
} /* TestApp */



#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000

#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("57760f70-483b-503f-a2f0-225d33736721"))
ITypedEventHandler<TestApp::Parity::ApiService*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::ApiService*, TestApp::Parity::IApiService*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.ApiService, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::ApiService*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IApiService*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IApiService*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_USE */


#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000






/*
 *
 * Struct TestApp.Parity.ApiStatus
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 */

#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000

namespace TestApp {
    namespace Parity {
        /* [v1_enum, contract] */
        enum ApiStatus : int
        {
            ApiStatus_Ready = 0,
            ApiStatus_Busy = 1,
            ApiStatus_Error = 2,
        };
        
    } /* Parity */
} /* TestApp */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Struct TestApp.Parity.ApiSettings
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


namespace TestApp {
    namespace Parity {
        /* [contract] */
        struct ApiSettings
        {
            HSTRING Name;
            int Version;
            ::boolean Debug;
        };
        
    } /* Parity */
} /* TestApp */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Delegate TestApp.Parity.ApiCallback
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ApiCallback:HRESULT Invoke(String);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIApiCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("cda79bc6-905c-56c5-8880-069f6b8349da"), object, contract] */
        MIDL_INTERFACE("cda79bc6-905c-56c5-8880-069f6b8349da")
        IApiCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IApiCallback=__uuidof(IApiCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIApiCallback;
#endif /* !defined(____x_TestApp_CParity_CIApiCallback_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IApiEndpoint
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiEndpoint:HRESULT get_Url(String*);HRESULT Connect();
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIApiEndpoint_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiEndpoint_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiEndpoint[] = L"TestApp.Parity.IApiEndpoint";

namespace TestApp {
    namespace Parity {
        /* [uuid("91cc8a7d-3fda-5b2d-85a9-bad2c26d335c"), object, contract] */
        MIDL_INTERFACE("91cc8a7d-3fda-5b2d-85a9-bad2c26d335c")
        IApiEndpoint : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Url(
                /* [out, retval] */HSTRING * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Connect(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IApiEndpoint=__uuidof(IApiEndpoint);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIApiEndpoint;
#endif /* !defined(____x_TestApp_CParity_CIApiEndpoint_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.IApiService
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiService:HRESULT get_ServiceName(String*);HRESULT Start();
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIApiService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiService[] = L"TestApp.Parity.IApiService";

namespace TestApp {
    namespace Parity {
        /* [uuid("9115eadb-1eb7-5833-a56c-2eb0960a6b1e"), contract, object, exclusiveto] */
        MIDL_INTERFACE("9115eadb-1eb7-5833-a56c-2eb0960a6b1e")
        IApiService : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_ServiceName(
                /* [out, retval] */HSTRING * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Start(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IApiService=__uuidof(IApiService);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIApiService;
#endif /* !defined(____x_TestApp_CParity_CIApiService_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IApiService2
 *
 * Introduced to TestApp.Parity.TestApiContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiService2:HRESULT get_ConnectionCount(Int32*);HRESULT Restart();HRESULT add_Restarted(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.ApiService, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Restarted(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CIApiService2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiService2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiService2[] = L"TestApp.Parity.IApiService2";

namespace TestApp {
    namespace Parity {
        /* [uuid("0df443b0-2baa-5e6d-9e00-ca3bd0a03b3c"), contract, object, exclusiveto] */
        MIDL_INTERFACE("0df443b0-2baa-5e6d-9e00-ca3bd0a03b3c")
        IApiService2 : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_ConnectionCount(
                /* [out, retval] */int * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Restart(void) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_Restarted(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_Restarted(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IApiService2=__uuidof(IApiService2);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIApiService2;
#endif /* !defined(____x_TestApp_CParity_CIApiService2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x20000


/*
 *
 * Interface TestApp.Parity.IApiService3
 *
 * Introduced to TestApp.Parity.TestApiContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiService3:HRESULT get_IsHealthy(Boolean*);HRESULT GetStatus(TestApp.Parity.ApiStatus*);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIApiService3_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiService3_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiService3[] = L"TestApp.Parity.IApiService3";

namespace TestApp {
    namespace Parity {
        /* [uuid("320dcdcc-7993-5dab-9a90-5fb225033e56"), contract, object, exclusiveto] */
        MIDL_INTERFACE("320dcdcc-7993-5dab-9a90-5fb225033e56")
        IApiService3 : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_IsHealthy(
                /* [out, retval] */::boolean * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetStatus(
                /* [out, retval] */TestApp::Parity::ApiStatus * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IApiService3=__uuidof(IApiService3);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIApiService3;
#endif /* !defined(____x_TestApp_CParity_CIApiService3_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000


/*
 *
 * Interface TestApp.Parity.IApiServiceStatics
 *
 * Introduced to TestApp.Parity.TestApiContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiServiceStatics:HRESULT CreateWithSettings(TestApp.Parity.ApiSettings,TestApp.Parity.ApiService**);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIApiServiceStatics_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiServiceStatics_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiServiceStatics[] = L"TestApp.Parity.IApiServiceStatics";

namespace TestApp {
    namespace Parity {
        /* [uuid("313f7bbb-a5eb-5833-a353-51a0c5391d67"), contract, object, exclusiveto] */
        MIDL_INTERFACE("313f7bbb-a5eb-5833-a353-51a0c5391d67")
        IApiServiceStatics : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateWithSettings(
                /* [in] */struct TestApp::Parity::ApiSettings settings,
                /* [out, retval] */TestApp::Parity::IApiService * * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IApiServiceStatics=__uuidof(IApiServiceStatics);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIApiServiceStatics;
#endif /* !defined(____x_TestApp_CParity_CIApiServiceStatics_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000


/*
 *
 * Class TestApp.Parity.ApiService
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.TestApiContract API contract
 *
 * RuntimeClass contains static methods.
 *   Static Methods exist on the TestApp.Parity.IApiServiceStatics interface starting with version 3.0 of the TestApp.Parity.TestApiContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IApiService ** Default Interface **
 *    TestApp.Parity.IApiService2
 *    TestApp.Parity.IApiService3
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_ApiService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ApiService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ApiService[] = L"TestApp.Parity.ApiService";
#endif
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


namespace TestApp {
    namespace Parity {
        class SecondaryService;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.ISecondaryService
 *
 * Introduced to TestApp.Parity.SecondaryContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.SecondaryService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ISecondaryService:HRESULT Execute();
 *
 *
 */
#if TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CISecondaryService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISecondaryService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ISecondaryService[] = L"TestApp.Parity.ISecondaryService";

namespace TestApp {
    namespace Parity {
        /* [uuid("f62af64c-10df-55cd-9b55-eb4940d9fce5"), contract, object, exclusiveto] */
        MIDL_INTERFACE("f62af64c-10df-55cd-9b55-eb4940d9fce5")
        ISecondaryService : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Execute(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ISecondaryService=__uuidof(ISecondaryService);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CISecondaryService;
#endif /* !defined(____x_TestApp_CParity_CISecondaryService_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.ISecondaryService2
 *
 * Introduced to TestApp.Parity.SecondaryContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.SecondaryService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ISecondaryService2:HRESULT get_Result(String*);
 *
 *
 */
#if TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CISecondaryService2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISecondaryService2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ISecondaryService2[] = L"TestApp.Parity.ISecondaryService2";

namespace TestApp {
    namespace Parity {
        /* [uuid("2a88ddd5-86d8-5e7b-aa68-cad0d1088852"), contract, object, exclusiveto] */
        MIDL_INTERFACE("2a88ddd5-86d8-5e7b-aa68-cad0d1088852")
        ISecondaryService2 : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Result(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ISecondaryService2=__uuidof(ISecondaryService2);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CISecondaryService2;
#endif /* !defined(____x_TestApp_CParity_CISecondaryService2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x20000


/*
 *
 * Class TestApp.Parity.SecondaryService
 *
 * Introduced to TestApp.Parity.SecondaryContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.SecondaryContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ISecondaryService ** Default Interface **
 *    TestApp.Parity.ISecondaryService2
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_SecondaryService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_SecondaryService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_SecondaryService[] = L"TestApp.Parity.SecondaryService";
#endif
#endif // TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIApiCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIApiCallback __x_TestApp_CParity_CIApiCallback;

#endif // ____x_TestApp_CParity_CIApiCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiEndpoint_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiEndpoint_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIApiEndpoint __x_TestApp_CParity_CIApiEndpoint;

#endif // ____x_TestApp_CParity_CIApiEndpoint_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiService_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiService_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIApiService __x_TestApp_CParity_CIApiService;

#endif // ____x_TestApp_CParity_CIApiService_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiService2_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiService2_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIApiService2 __x_TestApp_CParity_CIApiService2;

#endif // ____x_TestApp_CParity_CIApiService2_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiService3_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiService3_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIApiService3 __x_TestApp_CParity_CIApiService3;

#endif // ____x_TestApp_CParity_CIApiService3_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIApiServiceStatics_FWD_DEFINED__
#define ____x_TestApp_CParity_CIApiServiceStatics_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIApiServiceStatics __x_TestApp_CParity_CIApiServiceStatics;

#endif // ____x_TestApp_CParity_CIApiServiceStatics_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISecondaryService_FWD_DEFINED__
#define ____x_TestApp_CParity_CISecondaryService_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CISecondaryService __x_TestApp_CParity_CISecondaryService;

#endif // ____x_TestApp_CParity_CISecondaryService_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISecondaryService2_FWD_DEFINED__
#define ____x_TestApp_CParity_CISecondaryService2_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CISecondaryService2 __x_TestApp_CParity_CISecondaryService2;

#endif // ____x_TestApp_CParity_CISecondaryService2_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIApiService * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable_INTERFACE_DEFINED__

#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000





/*
 *
 * Struct TestApp.Parity.ApiStatus
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 */

#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
/* [v1_enum, contract] */
enum __x_TestApp_CParity_CApiStatus
{
    ApiStatus_Ready = 0,
    ApiStatus_Busy = 1,
    ApiStatus_Error = 2,
};
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Struct TestApp.Parity.ApiSettings
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000

/* [contract] */
struct __x_TestApp_CParity_CApiSettings
{
    HSTRING Name;
    int Version;
    boolean Debug;
};
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Delegate TestApp.Parity.ApiCallback
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ApiCallback:HRESULT Invoke(String);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIApiCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiCallback_INTERFACE_DEFINED__
/* [uuid("cda79bc6-905c-56c5-8880-069f6b8349da"), object, contract] */
typedef struct __x_TestApp_CParity_CIApiCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIApiCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIApiCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIApiCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIApiCallback * This,
        /* [in] */HSTRING result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIApiCallbackVtbl;

interface __x_TestApp_CParity_CIApiCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CIApiCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIApiCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIApiCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIApiCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIApiCallback_Invoke(This,result) \
    ( (This)->lpVtbl->Invoke(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIApiCallback;
#endif /* !defined(____x_TestApp_CParity_CIApiCallback_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IApiEndpoint
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiEndpoint:HRESULT get_Url(String*);HRESULT Connect();
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIApiEndpoint_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiEndpoint_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiEndpoint[] = L"TestApp.Parity.IApiEndpoint";
/* [uuid("91cc8a7d-3fda-5b2d-85a9-bad2c26d335c"), object, contract] */
typedef struct __x_TestApp_CParity_CIApiEndpointVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIApiEndpoint * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIApiEndpoint * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIApiEndpoint * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIApiEndpoint * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIApiEndpoint * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIApiEndpoint * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Url )(
        __x_TestApp_CParity_CIApiEndpoint * This,
        /* [out, retval] */HSTRING * value
        );
    HRESULT ( STDMETHODCALLTYPE *Connect )(
        __x_TestApp_CParity_CIApiEndpoint * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIApiEndpointVtbl;

interface __x_TestApp_CParity_CIApiEndpoint
{
    CONST_VTBL struct __x_TestApp_CParity_CIApiEndpointVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIApiEndpoint_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIApiEndpoint_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIApiEndpoint_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIApiEndpoint_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIApiEndpoint_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIApiEndpoint_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIApiEndpoint_get_Url(This,value) \
    ( (This)->lpVtbl->get_Url(This,value) )

#define __x_TestApp_CParity_CIApiEndpoint_Connect(This) \
    ( (This)->lpVtbl->Connect(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIApiEndpoint;
#endif /* !defined(____x_TestApp_CParity_CIApiEndpoint_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.IApiService
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiService:HRESULT get_ServiceName(String*);HRESULT Start();
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIApiService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiService[] = L"TestApp.Parity.IApiService";
/* [uuid("9115eadb-1eb7-5833-a56c-2eb0960a6b1e"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIApiServiceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIApiService * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIApiService * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIApiService * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIApiService * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIApiService * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIApiService * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_ServiceName )(
        __x_TestApp_CParity_CIApiService * This,
        /* [out, retval] */HSTRING * value
        );
    HRESULT ( STDMETHODCALLTYPE *Start )(
        __x_TestApp_CParity_CIApiService * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIApiServiceVtbl;

interface __x_TestApp_CParity_CIApiService
{
    CONST_VTBL struct __x_TestApp_CParity_CIApiServiceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIApiService_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIApiService_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIApiService_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIApiService_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIApiService_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIApiService_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIApiService_get_ServiceName(This,value) \
    ( (This)->lpVtbl->get_ServiceName(This,value) )

#define __x_TestApp_CParity_CIApiService_Start(This) \
    ( (This)->lpVtbl->Start(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIApiService;
#endif /* !defined(____x_TestApp_CParity_CIApiService_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IApiService2
 *
 * Introduced to TestApp.Parity.TestApiContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiService2:HRESULT get_ConnectionCount(Int32*);HRESULT Restart();HRESULT add_Restarted(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.ApiService, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Restarted(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CIApiService2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiService2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiService2[] = L"TestApp.Parity.IApiService2";
/* [uuid("0df443b0-2baa-5e6d-9e00-ca3bd0a03b3c"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIApiService2Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIApiService2 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIApiService2 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIApiService2 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIApiService2 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIApiService2 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIApiService2 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_ConnectionCount )(
        __x_TestApp_CParity_CIApiService2 * This,
        /* [out, retval] */int * value
        );
    HRESULT ( STDMETHODCALLTYPE *Restart )(
        __x_TestApp_CParity_CIApiService2 * This
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_Restarted )(
        __x_TestApp_CParity_CIApiService2 * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CApiService_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_Restarted )(
        __x_TestApp_CParity_CIApiService2 * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIApiService2Vtbl;

interface __x_TestApp_CParity_CIApiService2
{
    CONST_VTBL struct __x_TestApp_CParity_CIApiService2Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIApiService2_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIApiService2_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIApiService2_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIApiService2_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIApiService2_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIApiService2_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIApiService2_get_ConnectionCount(This,value) \
    ( (This)->lpVtbl->get_ConnectionCount(This,value) )

#define __x_TestApp_CParity_CIApiService2_Restart(This) \
    ( (This)->lpVtbl->Restart(This) )

#define __x_TestApp_CParity_CIApiService2_add_Restarted(This,handler,token) \
    ( (This)->lpVtbl->add_Restarted(This,handler,token) )

#define __x_TestApp_CParity_CIApiService2_remove_Restarted(This,token) \
    ( (This)->lpVtbl->remove_Restarted(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIApiService2;
#endif /* !defined(____x_TestApp_CParity_CIApiService2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x20000


/*
 *
 * Interface TestApp.Parity.IApiService3
 *
 * Introduced to TestApp.Parity.TestApiContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiService3:HRESULT get_IsHealthy(Boolean*);HRESULT GetStatus(TestApp.Parity.ApiStatus*);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIApiService3_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiService3_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiService3[] = L"TestApp.Parity.IApiService3";
/* [uuid("320dcdcc-7993-5dab-9a90-5fb225033e56"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIApiService3Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIApiService3 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIApiService3 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIApiService3 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIApiService3 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIApiService3 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIApiService3 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_IsHealthy )(
        __x_TestApp_CParity_CIApiService3 * This,
        /* [out, retval] */boolean * value
        );
    HRESULT ( STDMETHODCALLTYPE *GetStatus )(
        __x_TestApp_CParity_CIApiService3 * This,
        /* [out, retval] */enum __x_TestApp_CParity_CApiStatus * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIApiService3Vtbl;

interface __x_TestApp_CParity_CIApiService3
{
    CONST_VTBL struct __x_TestApp_CParity_CIApiService3Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIApiService3_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIApiService3_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIApiService3_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIApiService3_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIApiService3_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIApiService3_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIApiService3_get_IsHealthy(This,value) \
    ( (This)->lpVtbl->get_IsHealthy(This,value) )

#define __x_TestApp_CParity_CIApiService3_GetStatus(This,result) \
    ( (This)->lpVtbl->GetStatus(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIApiService3;
#endif /* !defined(____x_TestApp_CParity_CIApiService3_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000


/*
 *
 * Interface TestApp.Parity.IApiServiceStatics
 *
 * Introduced to TestApp.Parity.TestApiContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.ApiService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IApiServiceStatics:HRESULT CreateWithSettings(TestApp.Parity.ApiSettings,TestApp.Parity.ApiService**);
 *
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIApiServiceStatics_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIApiServiceStatics_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IApiServiceStatics[] = L"TestApp.Parity.IApiServiceStatics";
/* [uuid("313f7bbb-a5eb-5833-a353-51a0c5391d67"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIApiServiceStaticsVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIApiServiceStatics * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIApiServiceStatics * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIApiServiceStatics * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIApiServiceStatics * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIApiServiceStatics * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIApiServiceStatics * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateWithSettings )(
        __x_TestApp_CParity_CIApiServiceStatics * This,
        /* [in] */struct __x_TestApp_CParity_CApiSettings settings,
        /* [out, retval] */__x_TestApp_CParity_CIApiService * * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIApiServiceStaticsVtbl;

interface __x_TestApp_CParity_CIApiServiceStatics
{
    CONST_VTBL struct __x_TestApp_CParity_CIApiServiceStaticsVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIApiServiceStatics_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIApiServiceStatics_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIApiServiceStatics_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIApiServiceStatics_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIApiServiceStatics_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIApiServiceStatics_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIApiServiceStatics_CreateWithSettings(This,settings,result) \
    ( (This)->lpVtbl->CreateWithSettings(This,settings,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIApiServiceStatics;
#endif /* !defined(____x_TestApp_CParity_CIApiServiceStatics_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x30000


/*
 *
 * Class TestApp.Parity.ApiService
 *
 * Introduced to TestApp.Parity.TestApiContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.TestApiContract API contract
 *
 * RuntimeClass contains static methods.
 *   Static Methods exist on the TestApp.Parity.IApiServiceStatics interface starting with version 3.0 of the TestApp.Parity.TestApiContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IApiService ** Default Interface **
 *    TestApp.Parity.IApiService2
 *    TestApp.Parity.IApiService3
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_ApiService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ApiService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ApiService[] = L"TestApp.Parity.ApiService";
#endif
#endif // TESTAPP_PARITY_TESTAPICONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.ISecondaryService
 *
 * Introduced to TestApp.Parity.SecondaryContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.SecondaryService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ISecondaryService:HRESULT Execute();
 *
 *
 */
#if TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CISecondaryService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISecondaryService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ISecondaryService[] = L"TestApp.Parity.ISecondaryService";
/* [uuid("f62af64c-10df-55cd-9b55-eb4940d9fce5"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CISecondaryServiceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CISecondaryService * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CISecondaryService * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CISecondaryService * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CISecondaryService * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CISecondaryService * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CISecondaryService * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Execute )(
        __x_TestApp_CParity_CISecondaryService * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CISecondaryServiceVtbl;

interface __x_TestApp_CParity_CISecondaryService
{
    CONST_VTBL struct __x_TestApp_CParity_CISecondaryServiceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CISecondaryService_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CISecondaryService_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CISecondaryService_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CISecondaryService_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CISecondaryService_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CISecondaryService_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CISecondaryService_Execute(This) \
    ( (This)->lpVtbl->Execute(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CISecondaryService;
#endif /* !defined(____x_TestApp_CParity_CISecondaryService_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.ISecondaryService2
 *
 * Introduced to TestApp.Parity.SecondaryContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.SecondaryService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ISecondaryService2:HRESULT get_Result(String*);
 *
 *
 */
#if TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CISecondaryService2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISecondaryService2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ISecondaryService2[] = L"TestApp.Parity.ISecondaryService2";
/* [uuid("2a88ddd5-86d8-5e7b-aa68-cad0d1088852"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CISecondaryService2Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CISecondaryService2 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CISecondaryService2 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CISecondaryService2 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CISecondaryService2 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CISecondaryService2 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CISecondaryService2 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Result )(
        __x_TestApp_CParity_CISecondaryService2 * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CISecondaryService2Vtbl;

interface __x_TestApp_CParity_CISecondaryService2
{
    CONST_VTBL struct __x_TestApp_CParity_CISecondaryService2Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CISecondaryService2_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CISecondaryService2_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CISecondaryService2_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CISecondaryService2_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CISecondaryService2_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CISecondaryService2_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CISecondaryService2_get_Result(This,value) \
    ( (This)->lpVtbl->get_Result(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CISecondaryService2;
#endif /* !defined(____x_TestApp_CParity_CISecondaryService2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x20000


/*
 *
 * Class TestApp.Parity.SecondaryService
 *
 * Introduced to TestApp.Parity.SecondaryContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.SecondaryContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ISecondaryService ** Default Interface **
 *    TestApp.Parity.ISecondaryService2
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_SecondaryService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_SecondaryService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_SecondaryService[] = L"TestApp.Parity.SecondaryService";
#endif
#endif // TESTAPP_PARITY_SECONDARYCONTRACT_VERSION >= 0x10000


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_apicontract_versioning_p_h__

#endif // __parity_apicontract_versioning_h__
