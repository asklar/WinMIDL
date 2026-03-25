/* Header file automatically generated from parity_deprecated_members_mixed.idl */
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
#ifndef __parity_deprecated_members_mixed_h__
#define __parity_deprecated_members_mixed_h__
#ifndef __parity_deprecated_members_mixed_p_h__
#define __parity_deprecated_members_mixed_p_h__


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
#if !defined(TESTAPP_PARITY_DEPMIXCONTRACT_VERSION)
#define TESTAPP_PARITY_DEPMIXCONTRACT_VERSION 0x30000
#endif // defined(TESTAPP_PARITY_DEPMIXCONTRACT_VERSION)

#endif // defined(SPECIFIC_API_CONTRACT_DEFINITIONS)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"
// Importing Collections header
#include <windows.foundation.collections.h>

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIDeprecatedMembersWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDeprecatedMembersWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDeprecatedMembersWidget TestApp::Parity::IDeprecatedMembersWidget

#endif // ____x_TestApp_CParity_CIDeprecatedMembersWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDeprecatedMembersWidget2_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget2_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDeprecatedMembersWidget2;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDeprecatedMembersWidget2 TestApp::Parity::IDeprecatedMembersWidget2

#endif // ____x_TestApp_CParity_CIDeprecatedMembersWidget2_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class DeprecatedMembersWidget;
    } /* Parity */
} /* TestApp */



#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000

#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("d3a15d5d-8312-5b03-81db-cb1476b75019"))
ITypedEventHandler<TestApp::Parity::DeprecatedMembersWidget*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::DeprecatedMembersWidget*, TestApp::Parity::IDeprecatedMembersWidget*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.DeprecatedMembersWidget, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::DeprecatedMembersWidget*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IDeprecatedMembersWidget*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IDeprecatedMembersWidget*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_USE */


#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000






/*
 *
 * Interface TestApp.Parity.IDeprecatedMembersWidget
 *
 * Introduced to TestApp.Parity.DepMixContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.DeprecatedMembersWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedMembersWidget:HRESULT CurrentMethod();HRESULT LegacyMethod();HRESULT get_CurrentCount(Int32*);HRESULT get_OldCount(Int32*);HRESULT add_CurrentEvent(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.DeprecatedMembersWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CurrentEvent(EventRegistrationToken);HRESULT add_OldEvent(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.DeprecatedMembersWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_OldEvent(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedMembersWidget[] = L"TestApp.Parity.IDeprecatedMembersWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("e76bfd97-9397-56d5-b192-6a643700b328"), contract, object, exclusiveto] */
        MIDL_INTERFACE("e76bfd97-9397-56d5-b192-6a643700b328")
        IDeprecatedMembersWidget : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CurrentMethod(void) = 0;
            /* [deprecated] */virtual HRESULT STDMETHODCALLTYPE LegacyMethod(void) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_CurrentCount(
                /* [out, retval] */int * value
                ) = 0;
            /* [propget, deprecated] */virtual HRESULT STDMETHODCALLTYPE get_OldCount(
                /* [out, retval] */int * value
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_CurrentEvent(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_CurrentEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd, deprecated] */virtual HRESULT STDMETHODCALLTYPE add_OldEvent(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove, deprecated] */virtual HRESULT STDMETHODCALLTYPE remove_OldEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDeprecatedMembersWidget=__uuidof(IDeprecatedMembersWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedMembersWidget;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IDeprecatedMembersWidget2
 *
 * Introduced to TestApp.Parity.DepMixContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.DeprecatedMembersWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedMembersWidget2:HRESULT V2Method();HRESULT V2OldMethod();
 *
 *
 */
#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedMembersWidget2[] = L"TestApp.Parity.IDeprecatedMembersWidget2";

namespace TestApp {
    namespace Parity {
        /* [uuid("4e03a2c0-0df9-5e8f-8567-e971353148a0"), contract, object, exclusiveto] */
        MIDL_INTERFACE("4e03a2c0-0df9-5e8f-8567-e971353148a0")
        IDeprecatedMembersWidget2 : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE V2Method(void) = 0;
            /* [deprecated] */virtual HRESULT STDMETHODCALLTYPE V2OldMethod(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDeprecatedMembersWidget2=__uuidof(IDeprecatedMembersWidget2);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedMembersWidget2;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x20000


/*
 *
 * Class TestApp.Parity.DeprecatedMembersWidget
 *
 * Introduced to TestApp.Parity.DepMixContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DepMixContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDeprecatedMembersWidget ** Default Interface **
 *    TestApp.Parity.IDeprecatedMembersWidget2
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_DeprecatedMembersWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DeprecatedMembersWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DeprecatedMembersWidget[] = L"TestApp.Parity.DeprecatedMembersWidget";
#endif
#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIDeprecatedMembersWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDeprecatedMembersWidget __x_TestApp_CParity_CIDeprecatedMembersWidget;

#endif // ____x_TestApp_CParity_CIDeprecatedMembersWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDeprecatedMembersWidget2_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget2_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDeprecatedMembersWidget2 __x_TestApp_CParity_CIDeprecatedMembersWidget2;

#endif // ____x_TestApp_CParity_CIDeprecatedMembersWidget2_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000
#if !defined(____FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIDeprecatedMembersWidget * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable_INTERFACE_DEFINED__

#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000





/*
 *
 * Interface TestApp.Parity.IDeprecatedMembersWidget
 *
 * Introduced to TestApp.Parity.DepMixContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.DeprecatedMembersWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedMembersWidget:HRESULT CurrentMethod();HRESULT LegacyMethod();HRESULT get_CurrentCount(Int32*);HRESULT get_OldCount(Int32*);HRESULT add_CurrentEvent(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.DeprecatedMembersWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CurrentEvent(EventRegistrationToken);HRESULT add_OldEvent(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.DeprecatedMembersWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_OldEvent(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedMembersWidget[] = L"TestApp.Parity.IDeprecatedMembersWidget";
/* [uuid("e76bfd97-9397-56d5-b192-6a643700b328"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDeprecatedMembersWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CurrentMethod )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This
        );
    /* [deprecated] */HRESULT ( STDMETHODCALLTYPE *LegacyMethod )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_CurrentCount )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
        /* [out, retval] */int * value
        );
    /* [propget, deprecated] */HRESULT ( STDMETHODCALLTYPE *get_OldCount )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
        /* [out, retval] */int * value
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_CurrentEvent )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_CurrentEvent )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd, deprecated] */HRESULT ( STDMETHODCALLTYPE *add_OldEvent )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CDeprecatedMembersWidget_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove, deprecated] */HRESULT ( STDMETHODCALLTYPE *remove_OldEvent )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDeprecatedMembersWidgetVtbl;

interface __x_TestApp_CParity_CIDeprecatedMembersWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIDeprecatedMembersWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDeprecatedMembersWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_CurrentMethod(This) \
    ( (This)->lpVtbl->CurrentMethod(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_LegacyMethod(This) \
    ( (This)->lpVtbl->LegacyMethod(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_get_CurrentCount(This,value) \
    ( (This)->lpVtbl->get_CurrentCount(This,value) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_get_OldCount(This,value) \
    ( (This)->lpVtbl->get_OldCount(This,value) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_add_CurrentEvent(This,handler,token) \
    ( (This)->lpVtbl->add_CurrentEvent(This,handler,token) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_remove_CurrentEvent(This,token) \
    ( (This)->lpVtbl->remove_CurrentEvent(This,token) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_add_OldEvent(This,handler,token) \
    ( (This)->lpVtbl->add_OldEvent(This,handler,token) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget_remove_OldEvent(This,token) \
    ( (This)->lpVtbl->remove_OldEvent(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedMembersWidget;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IDeprecatedMembersWidget2
 *
 * Introduced to TestApp.Parity.DepMixContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.DeprecatedMembersWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDeprecatedMembersWidget2:HRESULT V2Method();HRESULT V2OldMethod();
 *
 *
 */
#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDeprecatedMembersWidget2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDeprecatedMembersWidget2[] = L"TestApp.Parity.IDeprecatedMembersWidget2";
/* [uuid("4e03a2c0-0df9-5e8f-8567-e971353148a0"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDeprecatedMembersWidget2Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *V2Method )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This
        );
    /* [deprecated] */HRESULT ( STDMETHODCALLTYPE *V2OldMethod )(
        __x_TestApp_CParity_CIDeprecatedMembersWidget2 * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDeprecatedMembersWidget2Vtbl;

interface __x_TestApp_CParity_CIDeprecatedMembersWidget2
{
    CONST_VTBL struct __x_TestApp_CParity_CIDeprecatedMembersWidget2Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_V2Method(This) \
    ( (This)->lpVtbl->V2Method(This) )

#define __x_TestApp_CParity_CIDeprecatedMembersWidget2_V2OldMethod(This) \
    ( (This)->lpVtbl->V2OldMethod(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDeprecatedMembersWidget2;
#endif /* !defined(____x_TestApp_CParity_CIDeprecatedMembersWidget2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x20000


/*
 *
 * Class TestApp.Parity.DeprecatedMembersWidget
 *
 * Introduced to TestApp.Parity.DepMixContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.DepMixContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDeprecatedMembersWidget ** Default Interface **
 *    TestApp.Parity.IDeprecatedMembersWidget2
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_DeprecatedMembersWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DeprecatedMembersWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DeprecatedMembersWidget[] = L"TestApp.Parity.DeprecatedMembersWidget";
#endif
#endif // TESTAPP_PARITY_DEPMIXCONTRACT_VERSION >= 0x10000


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_deprecated_members_mixed_p_h__

#endif // __parity_deprecated_members_mixed_h__
