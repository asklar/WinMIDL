/* Header file automatically generated from parity_custom_delegate_events.idl */
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
#ifndef __parity_custom_delegate_events_h__
#define __parity_custom_delegate_events_h__
#ifndef __parity_custom_delegate_events_p_h__
#define __parity_custom_delegate_events_p_h__


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
#ifndef ____x_TestApp_CParity_CIStatusChangedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIStatusChangedHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IStatusChangedHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIStatusChangedHandler TestApp::Parity::IStatusChangedHandler

#endif // ____x_TestApp_CParity_CIStatusChangedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIErrorOccurredHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIErrorOccurredHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IErrorOccurredHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIErrorOccurredHandler TestApp::Parity::IErrorOccurredHandler

#endif // ____x_TestApp_CParity_CIErrorOccurredHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IProgressHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIProgressHandler TestApp::Parity::IProgressHandler

#endif // ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventPatternWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventPatternWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IEventPatternWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIEventPatternWidget TestApp::Parity::IEventPatternWidget

#endif // ____x_TestApp_CParity_CIEventPatternWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class EventPatternWidget;
    } /* Parity */
} /* TestApp */




#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("43138cc4-bd3e-5149-bdf4-a7912ceb31d7"))
ITypedEventHandler<TestApp::Parity::EventPatternWidget*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventPatternWidget*, TestApp::Parity::IEventPatternWidget*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventPatternWidget, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventPatternWidget*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventPatternWidget*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventPatternWidget*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_USE */





#ifndef DEF___FIEventHandler_1_IInspectable_USE
#define DEF___FIEventHandler_1_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("c50898f6-c536-5f47-8583-8b2c2438a13b"))
IEventHandler<IInspectable*> : IEventHandler_impl<IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.EventHandler`1<Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IEventHandler<IInspectable*> __FIEventHandler_1_IInspectable_t;
#define __FIEventHandler_1_IInspectable Windows::Foundation::__FIEventHandler_1_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIEventHandler_1_IInspectable Windows::Foundation::IEventHandler<IInspectable*>
//#define __FIEventHandler_1_IInspectable_t Windows::Foundation::IEventHandler<IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIEventHandler_1_IInspectable_USE */






/*
 *
 * Delegate TestApp.Parity.StatusChangedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.StatusChangedHandler:HRESULT Invoke(String,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIStatusChangedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIStatusChangedHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("b0ec8bc3-e306-53e6-a643-414e967bb6af"), version, object] */
        MIDL_INTERFACE("b0ec8bc3-e306-53e6-a643-414e967bb6af")
        IStatusChangedHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING status,
                /* [in] */int code
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IStatusChangedHandler=__uuidof(IStatusChangedHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIStatusChangedHandler;
#endif /* !defined(____x_TestApp_CParity_CIStatusChangedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ErrorOccurredHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ErrorOccurredHandler:HRESULT Invoke(String,Boolean);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIErrorOccurredHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIErrorOccurredHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("9140491c-7cda-5536-b8e8-aef2599a06d2"), version, object] */
        MIDL_INTERFACE("9140491c-7cda-5536-b8e8-aef2599a06d2")
        IErrorOccurredHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING message,
                /* [in] */::boolean fatal
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IErrorOccurredHandler=__uuidof(IErrorOccurredHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIErrorOccurredHandler;
#endif /* !defined(____x_TestApp_CParity_CIErrorOccurredHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ProgressHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ProgressHandler:HRESULT Invoke(Double,String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("b293e556-3449-5022-9263-f0c1df0e2e27"), version, object] */
        MIDL_INTERFACE("b293e556-3449-5022-9263-f0c1df0e2e27")
        IProgressHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */double percent,
                /* [in] */HSTRING description
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IProgressHandler=__uuidof(IProgressHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIProgressHandler;
#endif /* !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventPatternWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventPatternWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventPatternWidget:HRESULT add_StatusChanged(TestApp.Parity.StatusChangedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_StatusChanged(EventRegistrationToken);HRESULT add_ErrorOccurred(TestApp.Parity.ErrorOccurredHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ErrorOccurred(EventRegistrationToken);HRESULT add_ProgressUpdated(TestApp.Parity.ProgressHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ProgressUpdated(EventRegistrationToken);HRESULT add_GenericChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventPatternWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_GenericChanged(EventRegistrationToken);HRESULT add_SimpleChanged(Windows.Foundation.EventHandler`1<Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_SimpleChanged(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventPatternWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventPatternWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventPatternWidget[] = L"TestApp.Parity.IEventPatternWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("377f1569-a22f-50fa-86d1-f23528c05534"), version, object, exclusiveto] */
        MIDL_INTERFACE("377f1569-a22f-50fa-86d1-f23528c05534")
        IEventPatternWidget : public IInspectable
        {
        public:
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_StatusChanged(
                /* [in] */TestApp::Parity::IStatusChangedHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_StatusChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ErrorOccurred(
                /* [in] */TestApp::Parity::IErrorOccurredHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ErrorOccurred(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ProgressUpdated(
                /* [in] */TestApp::Parity::IProgressHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ProgressUpdated(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_GenericChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_GenericChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_SimpleChanged(
                /* [in] */__FIEventHandler_1_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_SimpleChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IEventPatternWidget=__uuidof(IEventPatternWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIEventPatternWidget;
#endif /* !defined(____x_TestApp_CParity_CIEventPatternWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventPatternWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventPatternWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventPatternWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventPatternWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventPatternWidget[] = L"TestApp.Parity.EventPatternWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIStatusChangedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIStatusChangedHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIStatusChangedHandler __x_TestApp_CParity_CIStatusChangedHandler;

#endif // ____x_TestApp_CParity_CIStatusChangedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIErrorOccurredHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIErrorOccurredHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIErrorOccurredHandler __x_TestApp_CParity_CIErrorOccurredHandler;

#endif // ____x_TestApp_CParity_CIErrorOccurredHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIProgressHandler __x_TestApp_CParity_CIProgressHandler;

#endif // ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventPatternWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventPatternWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIEventPatternWidget __x_TestApp_CParity_CIEventPatternWidget;

#endif // ____x_TestApp_CParity_CIEventPatternWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventPatternWidget * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable_INTERFACE_DEFINED__



#if !defined(____FIEventHandler_1_IInspectable_INTERFACE_DEFINED__)
#define ____FIEventHandler_1_IInspectable_INTERFACE_DEFINED__

typedef interface __FIEventHandler_1_IInspectable __FIEventHandler_1_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIEventHandler_1_IInspectable;

typedef struct __FIEventHandler_1_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIEventHandler_1_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIEventHandler_1_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIEventHandler_1_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FIEventHandler_1_IInspectable * This,/* [in] */ __RPC__in_opt IInspectable *sender,/* [in] */ __RPC__in_opt IInspectable * *e);
    END_INTERFACE
} __FIEventHandler_1_IInspectableVtbl;

interface __FIEventHandler_1_IInspectable
{
    CONST_VTBL struct __FIEventHandler_1_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIEventHandler_1_IInspectable_QueryInterface(This,riid,ppvObject)	\
        ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIEventHandler_1_IInspectable_AddRef(This)	\
        ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIEventHandler_1_IInspectable_Release(This)	\
        ( (This)->lpVtbl -> Release(This) ) 

#define __FIEventHandler_1_IInspectable_Invoke(This,sender,e)	\
        ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */


#endif // ____FIEventHandler_1_IInspectable_INTERFACE_DEFINED__




/*
 *
 * Delegate TestApp.Parity.StatusChangedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.StatusChangedHandler:HRESULT Invoke(String,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIStatusChangedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIStatusChangedHandler_INTERFACE_DEFINED__
/* [uuid("b0ec8bc3-e306-53e6-a643-414e967bb6af"), version, object] */
typedef struct __x_TestApp_CParity_CIStatusChangedHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIStatusChangedHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIStatusChangedHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIStatusChangedHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIStatusChangedHandler * This,
        /* [in] */HSTRING status,
        /* [in] */int code
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIStatusChangedHandlerVtbl;

interface __x_TestApp_CParity_CIStatusChangedHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIStatusChangedHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIStatusChangedHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIStatusChangedHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIStatusChangedHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIStatusChangedHandler_Invoke(This,status,code) \
    ( (This)->lpVtbl->Invoke(This,status,code) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIStatusChangedHandler;
#endif /* !defined(____x_TestApp_CParity_CIStatusChangedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ErrorOccurredHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ErrorOccurredHandler:HRESULT Invoke(String,Boolean);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIErrorOccurredHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIErrorOccurredHandler_INTERFACE_DEFINED__
/* [uuid("9140491c-7cda-5536-b8e8-aef2599a06d2"), version, object] */
typedef struct __x_TestApp_CParity_CIErrorOccurredHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIErrorOccurredHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIErrorOccurredHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIErrorOccurredHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIErrorOccurredHandler * This,
        /* [in] */HSTRING message,
        /* [in] */boolean fatal
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIErrorOccurredHandlerVtbl;

interface __x_TestApp_CParity_CIErrorOccurredHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIErrorOccurredHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIErrorOccurredHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIErrorOccurredHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIErrorOccurredHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIErrorOccurredHandler_Invoke(This,message,fatal) \
    ( (This)->lpVtbl->Invoke(This,message,fatal) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIErrorOccurredHandler;
#endif /* !defined(____x_TestApp_CParity_CIErrorOccurredHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ProgressHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ProgressHandler:HRESULT Invoke(Double,String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__
/* [uuid("b293e556-3449-5022-9263-f0c1df0e2e27"), version, object] */
typedef struct __x_TestApp_CParity_CIProgressHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIProgressHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIProgressHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIProgressHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIProgressHandler * This,
        /* [in] */double percent,
        /* [in] */HSTRING description
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIProgressHandlerVtbl;

interface __x_TestApp_CParity_CIProgressHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIProgressHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIProgressHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIProgressHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIProgressHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIProgressHandler_Invoke(This,percent,description) \
    ( (This)->lpVtbl->Invoke(This,percent,description) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIProgressHandler;
#endif /* !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventPatternWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventPatternWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventPatternWidget:HRESULT add_StatusChanged(TestApp.Parity.StatusChangedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_StatusChanged(EventRegistrationToken);HRESULT add_ErrorOccurred(TestApp.Parity.ErrorOccurredHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ErrorOccurred(EventRegistrationToken);HRESULT add_ProgressUpdated(TestApp.Parity.ProgressHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ProgressUpdated(EventRegistrationToken);HRESULT add_GenericChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventPatternWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_GenericChanged(EventRegistrationToken);HRESULT add_SimpleChanged(Windows.Foundation.EventHandler`1<Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_SimpleChanged(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventPatternWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventPatternWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventPatternWidget[] = L"TestApp.Parity.IEventPatternWidget";
/* [uuid("377f1569-a22f-50fa-86d1-f23528c05534"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIEventPatternWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIEventPatternWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIEventPatternWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIEventPatternWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIEventPatternWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIEventPatternWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIEventPatternWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_StatusChanged )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */__x_TestApp_CParity_CIStatusChangedHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_StatusChanged )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ErrorOccurred )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */__x_TestApp_CParity_CIErrorOccurredHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ErrorOccurred )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ProgressUpdated )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */__x_TestApp_CParity_CIProgressHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ProgressUpdated )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_GenericChanged )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventPatternWidget_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_GenericChanged )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_SimpleChanged )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */__FIEventHandler_1_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_SimpleChanged )(
        __x_TestApp_CParity_CIEventPatternWidget * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIEventPatternWidgetVtbl;

interface __x_TestApp_CParity_CIEventPatternWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIEventPatternWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIEventPatternWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIEventPatternWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIEventPatternWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIEventPatternWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIEventPatternWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIEventPatternWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIEventPatternWidget_add_StatusChanged(This,handler,token) \
    ( (This)->lpVtbl->add_StatusChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_remove_StatusChanged(This,token) \
    ( (This)->lpVtbl->remove_StatusChanged(This,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_add_ErrorOccurred(This,handler,token) \
    ( (This)->lpVtbl->add_ErrorOccurred(This,handler,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_remove_ErrorOccurred(This,token) \
    ( (This)->lpVtbl->remove_ErrorOccurred(This,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_add_ProgressUpdated(This,handler,token) \
    ( (This)->lpVtbl->add_ProgressUpdated(This,handler,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_remove_ProgressUpdated(This,token) \
    ( (This)->lpVtbl->remove_ProgressUpdated(This,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_add_GenericChanged(This,handler,token) \
    ( (This)->lpVtbl->add_GenericChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_remove_GenericChanged(This,token) \
    ( (This)->lpVtbl->remove_GenericChanged(This,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_add_SimpleChanged(This,handler,token) \
    ( (This)->lpVtbl->add_SimpleChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventPatternWidget_remove_SimpleChanged(This,token) \
    ( (This)->lpVtbl->remove_SimpleChanged(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIEventPatternWidget;
#endif /* !defined(____x_TestApp_CParity_CIEventPatternWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventPatternWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventPatternWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventPatternWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventPatternWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventPatternWidget[] = L"TestApp.Parity.EventPatternWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_custom_delegate_events_p_h__

#endif // __parity_custom_delegate_events_h__
