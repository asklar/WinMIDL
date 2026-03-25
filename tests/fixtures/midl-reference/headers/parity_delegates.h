/* Header file automatically generated from parity_delegates.idl */
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
#ifndef __parity_delegates_h__
#define __parity_delegates_h__
#ifndef __parity_delegates_p_h__
#define __parity_delegates_p_h__


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
#ifndef ____x_TestApp_CParity_CISimpleCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CISimpleCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ISimpleCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CISimpleCallback TestApp::Parity::ISimpleCallback

#endif // ____x_TestApp_CParity_CISimpleCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMessageHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMessageHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMessageHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMessageHandler TestApp::Parity::IMessageHandler

#endif // ____x_TestApp_CParity_CIMessageHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITransformer_FWD_DEFINED__
#define ____x_TestApp_CParity_CITransformer_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ITransformer;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CITransformer TestApp::Parity::ITransformer

#endif // ____x_TestApp_CParity_CITransformer_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIPredicate_FWD_DEFINED__
#define ____x_TestApp_CParity_CIPredicate_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IPredicate;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIPredicate TestApp::Parity::IPredicate

#endif // ____x_TestApp_CParity_CIPredicate_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventSource_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventSource_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IEventSource;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIEventSource TestApp::Parity::IEventSource

#endif // ____x_TestApp_CParity_CIEventSource_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class EventSource;
    } /* Parity */
} /* TestApp */



#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("38ae2e98-0d6d-5c5f-b57e-46f50abd2440"))
ITypedEventHandler<TestApp::Parity::EventSource*,HSTRING> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventSource*, TestApp::Parity::IEventSource*>,HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventSource, String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventSource*,HSTRING> __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventSource*,HSTRING>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventSource*,HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_USE */




#ifndef DEF___FIEventHandler_1_int_USE
#define DEF___FIEventHandler_1_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("12ecedac-1aee-5ba5-bd66-959a0fb2b1ff"))
IEventHandler<int> : IEventHandler_impl<int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.EventHandler`1<Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IEventHandler<int> __FIEventHandler_1_int_t;
#define __FIEventHandler_1_int Windows::Foundation::__FIEventHandler_1_int_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIEventHandler_1_int Windows::Foundation::IEventHandler<int>
//#define __FIEventHandler_1_int_t Windows::Foundation::IEventHandler<int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIEventHandler_1_int_USE */






/*
 *
 * Delegate TestApp.Parity.SimpleCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.SimpleCallback:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISimpleCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISimpleCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("65d97cec-4707-5cd8-a342-ad8c102a34bc"), version, object] */
        MIDL_INTERFACE("65d97cec-4707-5cd8-a342-ad8c102a34bc")
        ISimpleCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ISimpleCallback=__uuidof(ISimpleCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CISimpleCallback;
#endif /* !defined(____x_TestApp_CParity_CISimpleCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.MessageHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.MessageHandler:HRESULT Invoke(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMessageHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMessageHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("a196f933-fc8a-5ff8-8917-e5083038b5ff"), version, object] */
        MIDL_INTERFACE("a196f933-fc8a-5ff8-8917-e5083038b5ff")
        IMessageHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING message
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMessageHandler=__uuidof(IMessageHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMessageHandler;
#endif /* !defined(____x_TestApp_CParity_CIMessageHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.Transformer
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.Transformer:HRESULT Invoke(String,Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CITransformer_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITransformer_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("9aeb0272-e289-5216-817a-2c9663ea6a91"), version, object] */
        MIDL_INTERFACE("9aeb0272-e289-5216-817a-2c9663ea6a91")
        ITransformer : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING input,
                /* [out] */int * output
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ITransformer=__uuidof(ITransformer);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CITransformer;
#endif /* !defined(____x_TestApp_CParity_CITransformer_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.Predicate
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.Predicate:HRESULT Invoke(Object*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIPredicate_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIPredicate_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("d00bacd3-19bc-5876-9adf-c38bec4064ad"), version, object] */
        MIDL_INTERFACE("d00bacd3-19bc-5876-9adf-c38bec4064ad")
        IPredicate : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */IInspectable * item,
                /* [out] */::boolean * matched
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IPredicate=__uuidof(IPredicate);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIPredicate;
#endif /* !defined(____x_TestApp_CParity_CIPredicate_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventSource
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventSource
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventSource:HRESULT add_ItemAdded(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventSource, String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemAdded(EventRegistrationToken);HRESULT add_CountChanged(Windows.Foundation.EventHandler`1<Int32>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CountChanged(EventRegistrationToken);HRESULT add_Completed(TestApp.Parity.SimpleCallback*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Completed(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventSource_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventSource_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventSource[] = L"TestApp.Parity.IEventSource";

namespace TestApp {
    namespace Parity {
        /* [uuid("dc38a0fc-fc39-5a6a-b9ff-6fe4598dddc7"), version, object, exclusiveto] */
        MIDL_INTERFACE("dc38a0fc-fc39-5a6a-b9ff-6fe4598dddc7")
        IEventSource : public IInspectable
        {
        public:
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ItemAdded(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ItemAdded(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_CountChanged(
                /* [in] */__FIEventHandler_1_int * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_CountChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_Completed(
                /* [in] */TestApp::Parity::ISimpleCallback  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_Completed(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IEventSource=__uuidof(IEventSource);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIEventSource;
#endif /* !defined(____x_TestApp_CParity_CIEventSource_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventSource
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventSource ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventSource_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventSource_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventSource[] = L"TestApp.Parity.EventSource";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CISimpleCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CISimpleCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CISimpleCallback __x_TestApp_CParity_CISimpleCallback;

#endif // ____x_TestApp_CParity_CISimpleCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMessageHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMessageHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMessageHandler __x_TestApp_CParity_CIMessageHandler;

#endif // ____x_TestApp_CParity_CIMessageHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITransformer_FWD_DEFINED__
#define ____x_TestApp_CParity_CITransformer_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CITransformer __x_TestApp_CParity_CITransformer;

#endif // ____x_TestApp_CParity_CITransformer_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIPredicate_FWD_DEFINED__
#define ____x_TestApp_CParity_CIPredicate_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIPredicate __x_TestApp_CParity_CIPredicate;

#endif // ____x_TestApp_CParity_CIPredicate_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventSource_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventSource_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIEventSource __x_TestApp_CParity_CIEventSource;

#endif // ____x_TestApp_CParity_CIEventSource_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions

#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRINGVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventSource * sender,/* [in] */ __RPC__in_opt HSTRING e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRINGVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRINGVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING_INTERFACE_DEFINED__


#if !defined(____FIEventHandler_1_int_INTERFACE_DEFINED__)
#define ____FIEventHandler_1_int_INTERFACE_DEFINED__

typedef interface __FIEventHandler_1_int __FIEventHandler_1_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIEventHandler_1_int;

typedef struct __FIEventHandler_1_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIEventHandler_1_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIEventHandler_1_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIEventHandler_1_int * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FIEventHandler_1_int * This,/* [in] */ __RPC__in_opt IInspectable *sender,/* [in] */ __RPC__in_opt int *e);
    END_INTERFACE
} __FIEventHandler_1_intVtbl;

interface __FIEventHandler_1_int
{
    CONST_VTBL struct __FIEventHandler_1_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIEventHandler_1_int_QueryInterface(This,riid,ppvObject)	\
        ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIEventHandler_1_int_AddRef(This)	\
        ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIEventHandler_1_int_Release(This)	\
        ( (This)->lpVtbl -> Release(This) ) 

#define __FIEventHandler_1_int_Invoke(This,sender,e)	\
        ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */


#endif // ____FIEventHandler_1_int_INTERFACE_DEFINED__




/*
 *
 * Delegate TestApp.Parity.SimpleCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.SimpleCallback:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISimpleCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISimpleCallback_INTERFACE_DEFINED__
/* [uuid("65d97cec-4707-5cd8-a342-ad8c102a34bc"), version, object] */
typedef struct __x_TestApp_CParity_CISimpleCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CISimpleCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CISimpleCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CISimpleCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CISimpleCallback * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CISimpleCallbackVtbl;

interface __x_TestApp_CParity_CISimpleCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CISimpleCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CISimpleCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CISimpleCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CISimpleCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CISimpleCallback_Invoke(This) \
    ( (This)->lpVtbl->Invoke(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CISimpleCallback;
#endif /* !defined(____x_TestApp_CParity_CISimpleCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.MessageHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.MessageHandler:HRESULT Invoke(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMessageHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMessageHandler_INTERFACE_DEFINED__
/* [uuid("a196f933-fc8a-5ff8-8917-e5083038b5ff"), version, object] */
typedef struct __x_TestApp_CParity_CIMessageHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIMessageHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMessageHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMessageHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIMessageHandler * This,
        /* [in] */HSTRING message
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMessageHandlerVtbl;

interface __x_TestApp_CParity_CIMessageHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIMessageHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMessageHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMessageHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMessageHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMessageHandler_Invoke(This,message) \
    ( (This)->lpVtbl->Invoke(This,message) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMessageHandler;
#endif /* !defined(____x_TestApp_CParity_CIMessageHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.Transformer
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.Transformer:HRESULT Invoke(String,Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CITransformer_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITransformer_INTERFACE_DEFINED__
/* [uuid("9aeb0272-e289-5216-817a-2c9663ea6a91"), version, object] */
typedef struct __x_TestApp_CParity_CITransformerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CITransformer * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CITransformer * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CITransformer * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CITransformer * This,
        /* [in] */HSTRING input,
        /* [out] */int * output
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CITransformerVtbl;

interface __x_TestApp_CParity_CITransformer
{
    CONST_VTBL struct __x_TestApp_CParity_CITransformerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CITransformer_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CITransformer_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CITransformer_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CITransformer_Invoke(This,input,output) \
    ( (This)->lpVtbl->Invoke(This,input,output) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CITransformer;
#endif /* !defined(____x_TestApp_CParity_CITransformer_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.Predicate
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.Predicate:HRESULT Invoke(Object*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIPredicate_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIPredicate_INTERFACE_DEFINED__
/* [uuid("d00bacd3-19bc-5876-9adf-c38bec4064ad"), version, object] */
typedef struct __x_TestApp_CParity_CIPredicateVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIPredicate * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIPredicate * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIPredicate * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIPredicate * This,
        /* [in] */IInspectable * item,
        /* [out] */boolean * matched
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIPredicateVtbl;

interface __x_TestApp_CParity_CIPredicate
{
    CONST_VTBL struct __x_TestApp_CParity_CIPredicateVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIPredicate_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIPredicate_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIPredicate_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIPredicate_Invoke(This,item,matched) \
    ( (This)->lpVtbl->Invoke(This,item,matched) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIPredicate;
#endif /* !defined(____x_TestApp_CParity_CIPredicate_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventSource
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventSource
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventSource:HRESULT add_ItemAdded(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventSource, String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemAdded(EventRegistrationToken);HRESULT add_CountChanged(Windows.Foundation.EventHandler`1<Int32>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CountChanged(EventRegistrationToken);HRESULT add_Completed(TestApp.Parity.SimpleCallback*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Completed(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventSource_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventSource_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventSource[] = L"TestApp.Parity.IEventSource";
/* [uuid("dc38a0fc-fc39-5a6a-b9ff-6fe4598dddc7"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIEventSourceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIEventSource * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIEventSource * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIEventSource * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIEventSource * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIEventSource * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIEventSource * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ItemAdded )(
        __x_TestApp_CParity_CIEventSource * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventSource_HSTRING * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ItemAdded )(
        __x_TestApp_CParity_CIEventSource * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_CountChanged )(
        __x_TestApp_CParity_CIEventSource * This,
        /* [in] */__FIEventHandler_1_int * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_CountChanged )(
        __x_TestApp_CParity_CIEventSource * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_Completed )(
        __x_TestApp_CParity_CIEventSource * This,
        /* [in] */__x_TestApp_CParity_CISimpleCallback  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_Completed )(
        __x_TestApp_CParity_CIEventSource * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIEventSourceVtbl;

interface __x_TestApp_CParity_CIEventSource
{
    CONST_VTBL struct __x_TestApp_CParity_CIEventSourceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIEventSource_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIEventSource_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIEventSource_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIEventSource_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIEventSource_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIEventSource_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIEventSource_add_ItemAdded(This,handler,token) \
    ( (This)->lpVtbl->add_ItemAdded(This,handler,token) )

#define __x_TestApp_CParity_CIEventSource_remove_ItemAdded(This,token) \
    ( (This)->lpVtbl->remove_ItemAdded(This,token) )

#define __x_TestApp_CParity_CIEventSource_add_CountChanged(This,handler,token) \
    ( (This)->lpVtbl->add_CountChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventSource_remove_CountChanged(This,token) \
    ( (This)->lpVtbl->remove_CountChanged(This,token) )

#define __x_TestApp_CParity_CIEventSource_add_Completed(This,handler,token) \
    ( (This)->lpVtbl->add_Completed(This,handler,token) )

#define __x_TestApp_CParity_CIEventSource_remove_Completed(This,token) \
    ( (This)->lpVtbl->remove_Completed(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIEventSource;
#endif /* !defined(____x_TestApp_CParity_CIEventSource_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventSource
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventSource ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventSource_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventSource_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventSource[] = L"TestApp.Parity.EventSource";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_delegates_p_h__

#endif // __parity_delegates_h__
