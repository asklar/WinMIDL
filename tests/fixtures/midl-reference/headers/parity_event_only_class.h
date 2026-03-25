/* Header file automatically generated from parity_event_only_class.idl */
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
#ifndef __parity_event_only_class_h__
#define __parity_event_only_class_h__
#ifndef __parity_event_only_class_p_h__
#define __parity_event_only_class_p_h__


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
#ifndef ____x_TestApp_CParity_CIItemAddedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIItemAddedHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IItemAddedHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIItemAddedHandler TestApp::Parity::IItemAddedHandler

#endif // ____x_TestApp_CParity_CIItemAddedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIItemRemovedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIItemRemovedHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IItemRemovedHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIItemRemovedHandler TestApp::Parity::IItemRemovedHandler

#endif // ____x_TestApp_CParity_CIItemRemovedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CICollectionClearedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CICollectionClearedHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ICollectionClearedHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CICollectionClearedHandler TestApp::Parity::ICollectionClearedHandler

#endif // ____x_TestApp_CParity_CICollectionClearedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIItemMovedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIItemMovedHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IItemMovedHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIItemMovedHandler TestApp::Parity::IItemMovedHandler

#endif // ____x_TestApp_CParity_CIItemMovedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventSourceWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventSourceWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IEventSourceWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIEventSourceWidget TestApp::Parity::IEventSourceWidget

#endif // ____x_TestApp_CParity_CIEventSourceWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class EventSourceWidget;
    } /* Parity */
} /* TestApp */




#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("e74f8644-5154-5335-952f-9bf66471020f"))
ITypedEventHandler<TestApp::Parity::EventSourceWidget*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventSourceWidget*, TestApp::Parity::IEventSourceWidget*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventSourceWidget, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventSourceWidget*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventSourceWidget*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventSourceWidget*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_USE */






/*
 *
 * Delegate TestApp.Parity.ItemAddedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ItemAddedHandler:HRESULT Invoke(String,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIItemAddedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIItemAddedHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("9d3876dc-86c3-598f-8ba1-df7be86b52eb"), version, object] */
        MIDL_INTERFACE("9d3876dc-86c3-598f-8ba1-df7be86b52eb")
        IItemAddedHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING item,
                /* [in] */int index
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IItemAddedHandler=__uuidof(IItemAddedHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIItemAddedHandler;
#endif /* !defined(____x_TestApp_CParity_CIItemAddedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ItemRemovedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ItemRemovedHandler:HRESULT Invoke(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIItemRemovedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIItemRemovedHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("0c4a5b80-e523-59ef-86b2-476ac82fb340"), version, object] */
        MIDL_INTERFACE("0c4a5b80-e523-59ef-86b2-476ac82fb340")
        IItemRemovedHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING item
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IItemRemovedHandler=__uuidof(IItemRemovedHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIItemRemovedHandler;
#endif /* !defined(____x_TestApp_CParity_CIItemRemovedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.CollectionClearedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.CollectionClearedHandler:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CICollectionClearedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CICollectionClearedHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("58c21e0b-5367-5905-a3e3-f53ed1c36ac8"), version, object] */
        MIDL_INTERFACE("58c21e0b-5367-5905-a3e3-f53ed1c36ac8")
        ICollectionClearedHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ICollectionClearedHandler=__uuidof(ICollectionClearedHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CICollectionClearedHandler;
#endif /* !defined(____x_TestApp_CParity_CICollectionClearedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ItemMovedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ItemMovedHandler:HRESULT Invoke(Int32,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIItemMovedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIItemMovedHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("c37ba4cc-f54f-547b-a75d-5d816ecdc195"), version, object] */
        MIDL_INTERFACE("c37ba4cc-f54f-547b-a75d-5d816ecdc195")
        IItemMovedHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */int oldIndex,
                /* [in] */int newIndex
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IItemMovedHandler=__uuidof(IItemMovedHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIItemMovedHandler;
#endif /* !defined(____x_TestApp_CParity_CIItemMovedHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventSourceWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventSourceWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventSourceWidget:HRESULT add_ItemAdded(TestApp.Parity.ItemAddedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemAdded(EventRegistrationToken);HRESULT add_ItemRemoved(TestApp.Parity.ItemRemovedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemRemoved(EventRegistrationToken);HRESULT add_CollectionCleared(TestApp.Parity.CollectionClearedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CollectionCleared(EventRegistrationToken);HRESULT add_ItemMoved(TestApp.Parity.ItemMovedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemMoved(EventRegistrationToken);HRESULT add_StateChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventSourceWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_StateChanged(EventRegistrationToken);HRESULT AddItem(String);HRESULT RemoveItem(String);HRESULT Clear();HRESULT MoveItem(Int32,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventSourceWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventSourceWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventSourceWidget[] = L"TestApp.Parity.IEventSourceWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("9d73828b-18b8-5a8a-aee6-9315b1c48b7a"), version, object, exclusiveto] */
        MIDL_INTERFACE("9d73828b-18b8-5a8a-aee6-9315b1c48b7a")
        IEventSourceWidget : public IInspectable
        {
        public:
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ItemAdded(
                /* [in] */TestApp::Parity::IItemAddedHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ItemAdded(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ItemRemoved(
                /* [in] */TestApp::Parity::IItemRemovedHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ItemRemoved(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_CollectionCleared(
                /* [in] */TestApp::Parity::ICollectionClearedHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_CollectionCleared(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ItemMoved(
                /* [in] */TestApp::Parity::IItemMovedHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ItemMoved(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_StateChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_StateChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE AddItem(
                /* [in] */HSTRING item
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE RemoveItem(
                /* [in] */HSTRING item
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Clear(void) = 0;
            virtual HRESULT STDMETHODCALLTYPE MoveItem(
                /* [in] */int fromIndex,
                /* [in] */int toIndex
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IEventSourceWidget=__uuidof(IEventSourceWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIEventSourceWidget;
#endif /* !defined(____x_TestApp_CParity_CIEventSourceWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventSourceWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventSourceWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventSourceWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventSourceWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventSourceWidget[] = L"TestApp.Parity.EventSourceWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIItemAddedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIItemAddedHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIItemAddedHandler __x_TestApp_CParity_CIItemAddedHandler;

#endif // ____x_TestApp_CParity_CIItemAddedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIItemRemovedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIItemRemovedHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIItemRemovedHandler __x_TestApp_CParity_CIItemRemovedHandler;

#endif // ____x_TestApp_CParity_CIItemRemovedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CICollectionClearedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CICollectionClearedHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CICollectionClearedHandler __x_TestApp_CParity_CICollectionClearedHandler;

#endif // ____x_TestApp_CParity_CICollectionClearedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIItemMovedHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIItemMovedHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIItemMovedHandler __x_TestApp_CParity_CIItemMovedHandler;

#endif // ____x_TestApp_CParity_CIItemMovedHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventSourceWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventSourceWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIEventSourceWidget __x_TestApp_CParity_CIEventSourceWidget;

#endif // ____x_TestApp_CParity_CIEventSourceWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventSourceWidget * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable_INTERFACE_DEFINED__




/*
 *
 * Delegate TestApp.Parity.ItemAddedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ItemAddedHandler:HRESULT Invoke(String,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIItemAddedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIItemAddedHandler_INTERFACE_DEFINED__
/* [uuid("9d3876dc-86c3-598f-8ba1-df7be86b52eb"), version, object] */
typedef struct __x_TestApp_CParity_CIItemAddedHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIItemAddedHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIItemAddedHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIItemAddedHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIItemAddedHandler * This,
        /* [in] */HSTRING item,
        /* [in] */int index
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIItemAddedHandlerVtbl;

interface __x_TestApp_CParity_CIItemAddedHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIItemAddedHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIItemAddedHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIItemAddedHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIItemAddedHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIItemAddedHandler_Invoke(This,item,index) \
    ( (This)->lpVtbl->Invoke(This,item,index) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIItemAddedHandler;
#endif /* !defined(____x_TestApp_CParity_CIItemAddedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ItemRemovedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ItemRemovedHandler:HRESULT Invoke(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIItemRemovedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIItemRemovedHandler_INTERFACE_DEFINED__
/* [uuid("0c4a5b80-e523-59ef-86b2-476ac82fb340"), version, object] */
typedef struct __x_TestApp_CParity_CIItemRemovedHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIItemRemovedHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIItemRemovedHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIItemRemovedHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIItemRemovedHandler * This,
        /* [in] */HSTRING item
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIItemRemovedHandlerVtbl;

interface __x_TestApp_CParity_CIItemRemovedHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIItemRemovedHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIItemRemovedHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIItemRemovedHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIItemRemovedHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIItemRemovedHandler_Invoke(This,item) \
    ( (This)->lpVtbl->Invoke(This,item) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIItemRemovedHandler;
#endif /* !defined(____x_TestApp_CParity_CIItemRemovedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.CollectionClearedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.CollectionClearedHandler:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CICollectionClearedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CICollectionClearedHandler_INTERFACE_DEFINED__
/* [uuid("58c21e0b-5367-5905-a3e3-f53ed1c36ac8"), version, object] */
typedef struct __x_TestApp_CParity_CICollectionClearedHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CICollectionClearedHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CICollectionClearedHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CICollectionClearedHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CICollectionClearedHandler * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CICollectionClearedHandlerVtbl;

interface __x_TestApp_CParity_CICollectionClearedHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CICollectionClearedHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CICollectionClearedHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CICollectionClearedHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CICollectionClearedHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CICollectionClearedHandler_Invoke(This) \
    ( (This)->lpVtbl->Invoke(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CICollectionClearedHandler;
#endif /* !defined(____x_TestApp_CParity_CICollectionClearedHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ItemMovedHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ItemMovedHandler:HRESULT Invoke(Int32,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIItemMovedHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIItemMovedHandler_INTERFACE_DEFINED__
/* [uuid("c37ba4cc-f54f-547b-a75d-5d816ecdc195"), version, object] */
typedef struct __x_TestApp_CParity_CIItemMovedHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIItemMovedHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIItemMovedHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIItemMovedHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIItemMovedHandler * This,
        /* [in] */int oldIndex,
        /* [in] */int newIndex
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIItemMovedHandlerVtbl;

interface __x_TestApp_CParity_CIItemMovedHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIItemMovedHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIItemMovedHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIItemMovedHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIItemMovedHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIItemMovedHandler_Invoke(This,oldIndex,newIndex) \
    ( (This)->lpVtbl->Invoke(This,oldIndex,newIndex) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIItemMovedHandler;
#endif /* !defined(____x_TestApp_CParity_CIItemMovedHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventSourceWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventSourceWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventSourceWidget:HRESULT add_ItemAdded(TestApp.Parity.ItemAddedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemAdded(EventRegistrationToken);HRESULT add_ItemRemoved(TestApp.Parity.ItemRemovedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemRemoved(EventRegistrationToken);HRESULT add_CollectionCleared(TestApp.Parity.CollectionClearedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CollectionCleared(EventRegistrationToken);HRESULT add_ItemMoved(TestApp.Parity.ItemMovedHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ItemMoved(EventRegistrationToken);HRESULT add_StateChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventSourceWidget, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_StateChanged(EventRegistrationToken);HRESULT AddItem(String);HRESULT RemoveItem(String);HRESULT Clear();HRESULT MoveItem(Int32,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventSourceWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventSourceWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventSourceWidget[] = L"TestApp.Parity.IEventSourceWidget";
/* [uuid("9d73828b-18b8-5a8a-aee6-9315b1c48b7a"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIEventSourceWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIEventSourceWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIEventSourceWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIEventSourceWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIEventSourceWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIEventSourceWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIEventSourceWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ItemAdded )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */__x_TestApp_CParity_CIItemAddedHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ItemAdded )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ItemRemoved )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */__x_TestApp_CParity_CIItemRemovedHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ItemRemoved )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_CollectionCleared )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */__x_TestApp_CParity_CICollectionClearedHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_CollectionCleared )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ItemMoved )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */__x_TestApp_CParity_CIItemMovedHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ItemMoved )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_StateChanged )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventSourceWidget_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_StateChanged )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */EventRegistrationToken token
        );
    HRESULT ( STDMETHODCALLTYPE *AddItem )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */HSTRING item
        );
    HRESULT ( STDMETHODCALLTYPE *RemoveItem )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */HSTRING item
        );
    HRESULT ( STDMETHODCALLTYPE *Clear )(
        __x_TestApp_CParity_CIEventSourceWidget * This
        );
    HRESULT ( STDMETHODCALLTYPE *MoveItem )(
        __x_TestApp_CParity_CIEventSourceWidget * This,
        /* [in] */int fromIndex,
        /* [in] */int toIndex
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIEventSourceWidgetVtbl;

interface __x_TestApp_CParity_CIEventSourceWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIEventSourceWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIEventSourceWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIEventSourceWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIEventSourceWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIEventSourceWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIEventSourceWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIEventSourceWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIEventSourceWidget_add_ItemAdded(This,handler,token) \
    ( (This)->lpVtbl->add_ItemAdded(This,handler,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_remove_ItemAdded(This,token) \
    ( (This)->lpVtbl->remove_ItemAdded(This,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_add_ItemRemoved(This,handler,token) \
    ( (This)->lpVtbl->add_ItemRemoved(This,handler,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_remove_ItemRemoved(This,token) \
    ( (This)->lpVtbl->remove_ItemRemoved(This,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_add_CollectionCleared(This,handler,token) \
    ( (This)->lpVtbl->add_CollectionCleared(This,handler,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_remove_CollectionCleared(This,token) \
    ( (This)->lpVtbl->remove_CollectionCleared(This,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_add_ItemMoved(This,handler,token) \
    ( (This)->lpVtbl->add_ItemMoved(This,handler,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_remove_ItemMoved(This,token) \
    ( (This)->lpVtbl->remove_ItemMoved(This,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_add_StateChanged(This,handler,token) \
    ( (This)->lpVtbl->add_StateChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_remove_StateChanged(This,token) \
    ( (This)->lpVtbl->remove_StateChanged(This,token) )

#define __x_TestApp_CParity_CIEventSourceWidget_AddItem(This,item) \
    ( (This)->lpVtbl->AddItem(This,item) )

#define __x_TestApp_CParity_CIEventSourceWidget_RemoveItem(This,item) \
    ( (This)->lpVtbl->RemoveItem(This,item) )

#define __x_TestApp_CParity_CIEventSourceWidget_Clear(This) \
    ( (This)->lpVtbl->Clear(This) )

#define __x_TestApp_CParity_CIEventSourceWidget_MoveItem(This,fromIndex,toIndex) \
    ( (This)->lpVtbl->MoveItem(This,fromIndex,toIndex) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIEventSourceWidget;
#endif /* !defined(____x_TestApp_CParity_CIEventSourceWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventSourceWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventSourceWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventSourceWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventSourceWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventSourceWidget[] = L"TestApp.Parity.EventSourceWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_event_only_class_p_h__

#endif // __parity_event_only_class_h__
