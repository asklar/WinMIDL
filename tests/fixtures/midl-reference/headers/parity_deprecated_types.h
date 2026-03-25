/* Header file automatically generated from parity_deprecated_types.idl */
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
#ifndef __parity_deprecated_types_h__
#define __parity_deprecated_types_h__
#ifndef __parity_deprecated_types_p_h__
#define __parity_deprecated_types_p_h__


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
#if !defined(TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION)
#define TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION 0x30000
#endif // defined(TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION)

#endif // defined(SPECIFIC_API_CONTRACT_DEFINITIONS)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"
// Importing Collections header
#include <windows.foundation.collections.h>

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIDeprecatedWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDeprecatedWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDeprecatedWidget TestApp::Parity::IDeprecatedWidget

#endif // ____x_TestApp_CParity_CIDeprecatedWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDeprecatedInterface_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedInterface_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDeprecatedInterface;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDeprecatedInterface TestApp::Parity::IDeprecatedInterface

#endif // ____x_TestApp_CParity_CIDeprecatedInterface_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMixedDeprecation_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMixedDeprecation_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMixedDeprecation;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMixedDeprecation TestApp::Parity::IMixedDeprecation

#endif // ____x_TestApp_CParity_CIMixedDeprecation_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIRemovedService_FWD_DEFINED__
#define ____x_TestApp_CParity_CIRemovedService_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IRemovedService;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIRemovedService TestApp::Parity::IRemovedService

#endif // ____x_TestApp_CParity_CIRemovedService_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class MixedDeprecation;
    } /* Parity */
} /* TestApp */



#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("13b65b33-3757-5adb-ae91-8ab7ab97b44c"))
ITypedEventHandler<TestApp::Parity::MixedDeprecation*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::MixedDeprecation*, TestApp::Parity::IMixedDeprecation*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.MixedDeprecation, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::MixedDeprecation*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IMixedDeprecation*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IMixedDeprecation*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_USE */


#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000





namespace TestApp {
    namespace Parity {
        class DeprecatedWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IDeprecatedWidget
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.DeprecatedWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedWidget:HRESULT get_Name(String*);
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIDeprecatedWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedWidget[] = L"TestApp.Parity.IDeprecatedWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("7ea28274-d635-525c-a05f-f0c97cee0b81"), deprecated, contract, object, exclusiveto] */
        MIDL_INTERFACE("7ea28274-d635-525c-a05f-f0c97cee0b81")
        IDeprecatedWidget : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Name(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDeprecatedWidget=__uuidof(IDeprecatedWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedWidget;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedWidget_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.DeprecatedWidget
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DeprecationContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDeprecatedWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_DeprecatedWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DeprecatedWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DeprecatedWidget[] = L"TestApp.Parity.DeprecatedWidget";
#endif
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IDeprecatedInterface
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedInterface:HRESULT OldMethod();HRESULT NewMethod();HRESULT get_OldProp(String*);HRESULT get_NewProp(String*);
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIDeprecatedInterface_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedInterface_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedInterface[] = L"TestApp.Parity.IDeprecatedInterface";

namespace TestApp {
    namespace Parity {
        /* [uuid("5a1e5485-1d0c-590e-9c32-a912e98fdcbb"), object, contract] */
        MIDL_INTERFACE("5a1e5485-1d0c-590e-9c32-a912e98fdcbb")
        IDeprecatedInterface : public IInspectable
        {
        public:
            /* [deprecated] */virtual HRESULT STDMETHODCALLTYPE OldMethod(void) = 0;
            virtual HRESULT STDMETHODCALLTYPE NewMethod(void) = 0;
            /* [propget, deprecated] */virtual HRESULT STDMETHODCALLTYPE get_OldProp(
                /* [out, retval] */HSTRING * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NewProp(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDeprecatedInterface=__uuidof(IDeprecatedInterface);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedInterface;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedInterface_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.IMixedDeprecation
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.MixedDeprecation
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMixedDeprecation:HRESULT GetOldValue(Int32*);HRESULT GetNewValue(Int32*);HRESULT get_OldCount(Int32*);HRESULT get_NewCount(Int32*);HRESULT add_LegacyChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.MixedDeprecation, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_LegacyChanged(EventRegistrationToken);HRESULT add_CurrentChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.MixedDeprecation, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CurrentChanged(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIMixedDeprecation_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMixedDeprecation_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMixedDeprecation[] = L"TestApp.Parity.IMixedDeprecation";

namespace TestApp {
    namespace Parity {
        /* [uuid("e9272e7d-d799-52e1-80cb-817cdf5093a5"), contract, object, exclusiveto] */
        MIDL_INTERFACE("e9272e7d-d799-52e1-80cb-817cdf5093a5")
        IMixedDeprecation : public IInspectable
        {
        public:
            /* [deprecated] */virtual HRESULT STDMETHODCALLTYPE GetOldValue(
                /* [out, retval] */int * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetNewValue(
                /* [out, retval] */int * result
                ) = 0;
            /* [propget, deprecated] */virtual HRESULT STDMETHODCALLTYPE get_OldCount(
                /* [out, retval] */int * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_NewCount(
                /* [out, retval] */int * value
                ) = 0;
            /* [eventadd, deprecated] */virtual HRESULT STDMETHODCALLTYPE add_LegacyChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove, deprecated] */virtual HRESULT STDMETHODCALLTYPE remove_LegacyChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_CurrentChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_CurrentChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMixedDeprecation=__uuidof(IMixedDeprecation);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMixedDeprecation;
#endif /* !defined(____x_TestApp_CParity_CIMixedDeprecation_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.MixedDeprecation
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DeprecationContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMixedDeprecation ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_MixedDeprecation_DEFINED
#define RUNTIMECLASS_TestApp_Parity_MixedDeprecation_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_MixedDeprecation[] = L"TestApp.Parity.MixedDeprecation";
#endif
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


namespace TestApp {
    namespace Parity {
        class RemovedService;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IRemovedService
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.RemovedService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IRemovedService:HRESULT DoWork();
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIRemovedService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIRemovedService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IRemovedService[] = L"TestApp.Parity.IRemovedService";

namespace TestApp {
    namespace Parity {
        /* [uuid("19c66218-7ef7-525e-b751-c84617bbe954"), deprecated, contract, object, exclusiveto] */
        MIDL_INTERFACE("19c66218-7ef7-525e-b751-c84617bbe954")
        IRemovedService : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE DoWork(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IRemovedService=__uuidof(IRemovedService);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIRemovedService;
#endif /* !defined(____x_TestApp_CParity_CIRemovedService_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.RemovedService
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DeprecationContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IRemovedService ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_RemovedService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_RemovedService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_RemovedService[] = L"TestApp.Parity.RemovedService";
#endif
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIDeprecatedWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDeprecatedWidget __x_TestApp_CParity_CIDeprecatedWidget;

#endif // ____x_TestApp_CParity_CIDeprecatedWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDeprecatedInterface_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedInterface_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDeprecatedInterface __x_TestApp_CParity_CIDeprecatedInterface;

#endif // ____x_TestApp_CParity_CIDeprecatedInterface_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMixedDeprecation_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMixedDeprecation_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMixedDeprecation __x_TestApp_CParity_CIMixedDeprecation;

#endif // ____x_TestApp_CParity_CIMixedDeprecation_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIRemovedService_FWD_DEFINED__
#define ____x_TestApp_CParity_CIRemovedService_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIRemovedService __x_TestApp_CParity_CIRemovedService;

#endif // ____x_TestApp_CParity_CIRemovedService_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIMixedDeprecation * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable_INTERFACE_DEFINED__

#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000





/*
 *
 * Interface TestApp.Parity.IDeprecatedWidget
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.DeprecatedWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedWidget:HRESULT get_Name(String*);
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIDeprecatedWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedWidget[] = L"TestApp.Parity.IDeprecatedWidget";
/* [uuid("7ea28274-d635-525c-a05f-f0c97cee0b81"), deprecated, contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDeprecatedWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDeprecatedWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Name )(
        __x_TestApp_CParity_CIDeprecatedWidget * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDeprecatedWidgetVtbl;

interface __x_TestApp_CParity_CIDeprecatedWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIDeprecatedWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDeprecatedWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDeprecatedWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDeprecatedWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDeprecatedWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDeprecatedWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDeprecatedWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDeprecatedWidget_get_Name(This,value) \
    ( (This)->lpVtbl->get_Name(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedWidget;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedWidget_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.DeprecatedWidget
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DeprecationContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDeprecatedWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_DeprecatedWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DeprecatedWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DeprecatedWidget[] = L"TestApp.Parity.DeprecatedWidget";
#endif
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IDeprecatedInterface
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedInterface:HRESULT OldMethod();HRESULT NewMethod();HRESULT get_OldProp(String*);HRESULT get_NewProp(String*);
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIDeprecatedInterface_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedInterface_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedInterface[] = L"TestApp.Parity.IDeprecatedInterface";
/* [uuid("5a1e5485-1d0c-590e-9c32-a912e98fdcbb"), object, contract] */
typedef struct __x_TestApp_CParity_CIDeprecatedInterfaceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDeprecatedInterface * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedInterface * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedInterface * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedInterface * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedInterface * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedInterface * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [deprecated] */HRESULT ( STDMETHODCALLTYPE *OldMethod )(
        __x_TestApp_CParity_CIDeprecatedInterface * This
        );
    HRESULT ( STDMETHODCALLTYPE *NewMethod )(
        __x_TestApp_CParity_CIDeprecatedInterface * This
        );
    /* [propget, deprecated] */HRESULT ( STDMETHODCALLTYPE *get_OldProp )(
        __x_TestApp_CParity_CIDeprecatedInterface * This,
        /* [out, retval] */HSTRING * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NewProp )(
        __x_TestApp_CParity_CIDeprecatedInterface * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDeprecatedInterfaceVtbl;

interface __x_TestApp_CParity_CIDeprecatedInterface
{
    CONST_VTBL struct __x_TestApp_CParity_CIDeprecatedInterfaceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDeprecatedInterface_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDeprecatedInterface_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDeprecatedInterface_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDeprecatedInterface_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDeprecatedInterface_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDeprecatedInterface_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDeprecatedInterface_OldMethod(This) \
    ( (This)->lpVtbl->OldMethod(This) )

#define __x_TestApp_CParity_CIDeprecatedInterface_NewMethod(This) \
    ( (This)->lpVtbl->NewMethod(This) )

#define __x_TestApp_CParity_CIDeprecatedInterface_get_OldProp(This,value) \
    ( (This)->lpVtbl->get_OldProp(This,value) )

#define __x_TestApp_CParity_CIDeprecatedInterface_get_NewProp(This,value) \
    ( (This)->lpVtbl->get_NewProp(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedInterface;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedInterface_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.IMixedDeprecation
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.MixedDeprecation
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMixedDeprecation:HRESULT GetOldValue(Int32*);HRESULT GetNewValue(Int32*);HRESULT get_OldCount(Int32*);HRESULT get_NewCount(Int32*);HRESULT add_LegacyChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.MixedDeprecation, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_LegacyChanged(EventRegistrationToken);HRESULT add_CurrentChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.MixedDeprecation, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CurrentChanged(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIMixedDeprecation_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMixedDeprecation_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMixedDeprecation[] = L"TestApp.Parity.IMixedDeprecation";
/* [uuid("e9272e7d-d799-52e1-80cb-817cdf5093a5"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMixedDeprecationVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMixedDeprecation * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMixedDeprecation * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMixedDeprecation * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMixedDeprecation * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMixedDeprecation * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMixedDeprecation * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [deprecated] */HRESULT ( STDMETHODCALLTYPE *GetOldValue )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [out, retval] */int * result
        );
    HRESULT ( STDMETHODCALLTYPE *GetNewValue )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [out, retval] */int * result
        );
    /* [propget, deprecated] */HRESULT ( STDMETHODCALLTYPE *get_OldCount )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [out, retval] */int * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_NewCount )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [out, retval] */int * value
        );
    /* [eventadd, deprecated] */HRESULT ( STDMETHODCALLTYPE *add_LegacyChanged )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove, deprecated] */HRESULT ( STDMETHODCALLTYPE *remove_LegacyChanged )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_CurrentChanged )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CMixedDeprecation_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_CurrentChanged )(
        __x_TestApp_CParity_CIMixedDeprecation * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMixedDeprecationVtbl;

interface __x_TestApp_CParity_CIMixedDeprecation
{
    CONST_VTBL struct __x_TestApp_CParity_CIMixedDeprecationVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMixedDeprecation_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMixedDeprecation_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMixedDeprecation_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMixedDeprecation_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMixedDeprecation_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMixedDeprecation_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMixedDeprecation_GetOldValue(This,result) \
    ( (This)->lpVtbl->GetOldValue(This,result) )

#define __x_TestApp_CParity_CIMixedDeprecation_GetNewValue(This,result) \
    ( (This)->lpVtbl->GetNewValue(This,result) )

#define __x_TestApp_CParity_CIMixedDeprecation_get_OldCount(This,value) \
    ( (This)->lpVtbl->get_OldCount(This,value) )

#define __x_TestApp_CParity_CIMixedDeprecation_get_NewCount(This,value) \
    ( (This)->lpVtbl->get_NewCount(This,value) )

#define __x_TestApp_CParity_CIMixedDeprecation_add_LegacyChanged(This,handler,token) \
    ( (This)->lpVtbl->add_LegacyChanged(This,handler,token) )

#define __x_TestApp_CParity_CIMixedDeprecation_remove_LegacyChanged(This,token) \
    ( (This)->lpVtbl->remove_LegacyChanged(This,token) )

#define __x_TestApp_CParity_CIMixedDeprecation_add_CurrentChanged(This,handler,token) \
    ( (This)->lpVtbl->add_CurrentChanged(This,handler,token) )

#define __x_TestApp_CParity_CIMixedDeprecation_remove_CurrentChanged(This,token) \
    ( (This)->lpVtbl->remove_CurrentChanged(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMixedDeprecation;
#endif /* !defined(____x_TestApp_CParity_CIMixedDeprecation_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.MixedDeprecation
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DeprecationContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMixedDeprecation ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_MixedDeprecation_DEFINED
#define RUNTIMECLASS_TestApp_Parity_MixedDeprecation_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_MixedDeprecation[] = L"TestApp.Parity.MixedDeprecation";
#endif
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.IRemovedService
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.RemovedService
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IRemovedService:HRESULT DoWork();
 *
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIRemovedService_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIRemovedService_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IRemovedService[] = L"TestApp.Parity.IRemovedService";
/* [uuid("19c66218-7ef7-525e-b751-c84617bbe954"), deprecated, contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIRemovedServiceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIRemovedService * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIRemovedService * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIRemovedService * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIRemovedService * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIRemovedService * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIRemovedService * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *DoWork )(
        __x_TestApp_CParity_CIRemovedService * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIRemovedServiceVtbl;

interface __x_TestApp_CParity_CIRemovedService
{
    CONST_VTBL struct __x_TestApp_CParity_CIRemovedServiceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIRemovedService_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIRemovedService_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIRemovedService_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIRemovedService_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIRemovedService_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIRemovedService_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIRemovedService_DoWork(This) \
    ( (This)->lpVtbl->DoWork(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIRemovedService;
#endif /* !defined(____x_TestApp_CParity_CIRemovedService_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.RemovedService
 *
 * Introduced to TestApp.Parity.DeprecationContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DeprecationContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IRemovedService ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_RemovedService_DEFINED
#define RUNTIMECLASS_TestApp_Parity_RemovedService_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_RemovedService[] = L"TestApp.Parity.RemovedService";
#endif
#endif // TESTAPP_PARITY_DEPRECATIONCONTRACT_VERSION >= 0x10000


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_deprecated_types_p_h__

#endif // __parity_deprecated_types_h__
