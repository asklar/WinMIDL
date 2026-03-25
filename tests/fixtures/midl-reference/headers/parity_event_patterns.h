/* Header file automatically generated from parity_event_patterns.idl */
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
#ifndef __parity_event_patterns_h__
#define __parity_event_patterns_h__
#ifndef __parity_event_patterns_p_h__
#define __parity_event_patterns_p_h__


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
#ifndef ____x_TestApp_CParity_CISimpleNotify_FWD_DEFINED__
#define ____x_TestApp_CParity_CISimpleNotify_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ISimpleNotify;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CISimpleNotify TestApp::Parity::ISimpleNotify

#endif // ____x_TestApp_CParity_CISimpleNotify_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIValueUpdated_FWD_DEFINED__
#define ____x_TestApp_CParity_CIValueUpdated_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IValueUpdated;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIValueUpdated TestApp::Parity::IValueUpdated

#endif // ____x_TestApp_CParity_CIValueUpdated_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IProgressHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIProgressHandler TestApp::Parity::IProgressHandler

#endif // ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIErrorHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIErrorHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IErrorHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIErrorHandler TestApp::Parity::IErrorHandler

#endif // ____x_TestApp_CParity_CIErrorHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventEmitter_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventEmitter_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IEventEmitter;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIEventEmitter TestApp::Parity::IEventEmitter

#endif // ____x_TestApp_CParity_CIEventEmitter_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class EventEmitter;
    } /* Parity */
} /* TestApp */




#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("da493661-ccde-5e37-8c16-5e2d7c5cc457"))
ITypedEventHandler<TestApp::Parity::EventEmitter*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventEmitter*, TestApp::Parity::IEventEmitter*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventEmitter*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_USE */





#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("b4b00e32-77c1-50e8-ba08-52cc71b332e9"))
ITypedEventHandler<TestApp::Parity::EventEmitter*,HSTRING> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventEmitter*, TestApp::Parity::IEventEmitter*>,HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventEmitter*,HSTRING> __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,HSTRING>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_USE */





#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("f983ec94-4552-544d-97f7-7967cec35447"))
ITypedEventHandler<TestApp::Parity::EventEmitter*,int> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventEmitter*, TestApp::Parity::IEventEmitter*>,int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventEmitter*,int> __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,int>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_USE */





#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("3b8417fb-303d-5102-8c79-006325acf3e8"))
ITypedEventHandler<TestApp::Parity::EventEmitter*,bool> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::EventEmitter*, TestApp::Parity::IEventEmitter*>,Windows::Foundation::Internal::AggregateType<bool, boolean>> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Boolean>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::EventEmitter*,bool> __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_t;
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,boolean>
//#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IEventEmitter*,boolean>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_USE */





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




#ifndef DEF___FIEventHandler_1_HSTRING_USE
#define DEF___FIEventHandler_1_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("ac4cb24b-bf86-5ab0-bf9d-555e7d89764a"))
IEventHandler<HSTRING> : IEventHandler_impl<HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.EventHandler`1<String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IEventHandler<HSTRING> __FIEventHandler_1_HSTRING_t;
#define __FIEventHandler_1_HSTRING Windows::Foundation::__FIEventHandler_1_HSTRING_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIEventHandler_1_HSTRING Windows::Foundation::IEventHandler<HSTRING>
//#define __FIEventHandler_1_HSTRING_t Windows::Foundation::IEventHandler<HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIEventHandler_1_HSTRING_USE */






/*
 *
 * Delegate TestApp.Parity.SimpleNotify
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.SimpleNotify:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISimpleNotify_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISimpleNotify_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("9ea0f663-8e9f-5ac2-ace4-6e1460b7ddfc"), version, object] */
        MIDL_INTERFACE("9ea0f663-8e9f-5ac2-ace4-6e1460b7ddfc")
        ISimpleNotify : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ISimpleNotify=__uuidof(ISimpleNotify);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CISimpleNotify;
#endif /* !defined(____x_TestApp_CParity_CISimpleNotify_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ValueUpdated
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ValueUpdated:HRESULT Invoke(String,Object*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIValueUpdated_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIValueUpdated_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("956a3abd-a91f-5d83-8b0c-942fa68c508a"), version, object] */
        MIDL_INTERFACE("956a3abd-a91f-5d83-8b0c-942fa68c508a")
        IValueUpdated : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING propertyName,
                /* [in] */IInspectable * newValue
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IValueUpdated=__uuidof(IValueUpdated);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIValueUpdated;
#endif /* !defined(____x_TestApp_CParity_CIValueUpdated_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ProgressHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ProgressHandler:HRESULT Invoke(Int32,String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("71a367d4-2774-5fb8-a5c2-57e63d29c9fa"), version, object] */
        MIDL_INTERFACE("71a367d4-2774-5fb8-a5c2-57e63d29c9fa")
        IProgressHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */int percent,
                /* [in] */HSTRING message
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IProgressHandler=__uuidof(IProgressHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIProgressHandler;
#endif /* !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ErrorHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ErrorHandler:HRESULT Invoke(Int32,String,Boolean);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIErrorHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIErrorHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("da3e2844-7e4d-5bc8-b321-bb025a1c77fc"), version, object] */
        MIDL_INTERFACE("da3e2844-7e4d-5bc8-b321-bb025a1c77fc")
        IErrorHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */int code,
                /* [in] */HSTRING description,
                /* [in] */::boolean isFatal
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IErrorHandler=__uuidof(IErrorHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIErrorHandler;
#endif /* !defined(____x_TestApp_CParity_CIErrorHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventEmitter
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventEmitter
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventEmitter:HRESULT add_GenericEvent(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_GenericEvent(EventRegistrationToken);HRESULT add_NameChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_NameChanged(EventRegistrationToken);HRESULT add_CountChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Int32>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CountChanged(EventRegistrationToken);HRESULT add_EnabledChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Boolean>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_EnabledChanged(EventRegistrationToken);HRESULT add_SimpleObjectEvent(Windows.Foundation.EventHandler`1<Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_SimpleObjectEvent(EventRegistrationToken);HRESULT add_IntEvent(Windows.Foundation.EventHandler`1<Int32>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_IntEvent(EventRegistrationToken);HRESULT add_StringEvent(Windows.Foundation.EventHandler`1<String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_StringEvent(EventRegistrationToken);HRESULT add_Notified(TestApp.Parity.SimpleNotify*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Notified(EventRegistrationToken);HRESULT add_PropertyChanged(TestApp.Parity.ValueUpdated*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_PropertyChanged(EventRegistrationToken);HRESULT add_ProgressUpdated(TestApp.Parity.ProgressHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ProgressUpdated(EventRegistrationToken);HRESULT add_ErrorOccurred(TestApp.Parity.ErrorHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ErrorOccurred(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventEmitter_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventEmitter_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventEmitter[] = L"TestApp.Parity.IEventEmitter";

namespace TestApp {
    namespace Parity {
        /* [uuid("5f39207a-13fe-5246-b4f1-f8d39d1e4010"), version, object, exclusiveto] */
        MIDL_INTERFACE("5f39207a-13fe-5246-b4f1-f8d39d1e4010")
        IEventEmitter : public IInspectable
        {
        public:
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_GenericEvent(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_GenericEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_NameChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_NameChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_CountChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_CountChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_EnabledChanged(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_EnabledChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_SimpleObjectEvent(
                /* [in] */__FIEventHandler_1_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_SimpleObjectEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_IntEvent(
                /* [in] */__FIEventHandler_1_int * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_IntEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_StringEvent(
                /* [in] */__FIEventHandler_1_HSTRING * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_StringEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_Notified(
                /* [in] */TestApp::Parity::ISimpleNotify  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_Notified(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_PropertyChanged(
                /* [in] */TestApp::Parity::IValueUpdated  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_PropertyChanged(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ProgressUpdated(
                /* [in] */TestApp::Parity::IProgressHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ProgressUpdated(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ErrorOccurred(
                /* [in] */TestApp::Parity::IErrorHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ErrorOccurred(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IEventEmitter=__uuidof(IEventEmitter);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIEventEmitter;
#endif /* !defined(____x_TestApp_CParity_CIEventEmitter_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventEmitter
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventEmitter ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventEmitter_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventEmitter_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventEmitter[] = L"TestApp.Parity.EventEmitter";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CISimpleNotify_FWD_DEFINED__
#define ____x_TestApp_CParity_CISimpleNotify_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CISimpleNotify __x_TestApp_CParity_CISimpleNotify;

#endif // ____x_TestApp_CParity_CISimpleNotify_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIValueUpdated_FWD_DEFINED__
#define ____x_TestApp_CParity_CIValueUpdated_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIValueUpdated __x_TestApp_CParity_CIValueUpdated;

#endif // ____x_TestApp_CParity_CIValueUpdated_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIProgressHandler __x_TestApp_CParity_CIProgressHandler;

#endif // ____x_TestApp_CParity_CIProgressHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIErrorHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIErrorHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIErrorHandler __x_TestApp_CParity_CIErrorHandler;

#endif // ____x_TestApp_CParity_CIErrorHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIEventEmitter_FWD_DEFINED__
#define ____x_TestApp_CParity_CIEventEmitter_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIEventEmitter __x_TestApp_CParity_CIEventEmitter;

#endif // ____x_TestApp_CParity_CIEventEmitter_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventEmitter * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable_INTERFACE_DEFINED__



#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRINGVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventEmitter * sender,/* [in] */ __RPC__in_opt HSTRING e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRINGVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRINGVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING_INTERFACE_DEFINED__



#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventEmitter * sender,/* [in] */ __RPC__in_opt int e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_intVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int_INTERFACE_DEFINED__



#if !defined(____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_booleanVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIEventEmitter * sender,/* [in] */ __RPC__in_opt boolean e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_booleanVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_booleanVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean_INTERFACE_DEFINED__



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


#if !defined(____FIEventHandler_1_HSTRING_INTERFACE_DEFINED__)
#define ____FIEventHandler_1_HSTRING_INTERFACE_DEFINED__

typedef interface __FIEventHandler_1_HSTRING __FIEventHandler_1_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIEventHandler_1_HSTRING;

typedef struct __FIEventHandler_1_HSTRINGVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIEventHandler_1_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIEventHandler_1_HSTRING * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIEventHandler_1_HSTRING * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FIEventHandler_1_HSTRING * This,/* [in] */ __RPC__in_opt IInspectable *sender,/* [in] */ __RPC__in_opt HSTRING *e);
    END_INTERFACE
} __FIEventHandler_1_HSTRINGVtbl;

interface __FIEventHandler_1_HSTRING
{
    CONST_VTBL struct __FIEventHandler_1_HSTRINGVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIEventHandler_1_HSTRING_QueryInterface(This,riid,ppvObject)	\
        ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIEventHandler_1_HSTRING_AddRef(This)	\
        ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIEventHandler_1_HSTRING_Release(This)	\
        ( (This)->lpVtbl -> Release(This) ) 

#define __FIEventHandler_1_HSTRING_Invoke(This,sender,e)	\
        ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */


#endif // ____FIEventHandler_1_HSTRING_INTERFACE_DEFINED__




/*
 *
 * Delegate TestApp.Parity.SimpleNotify
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.SimpleNotify:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISimpleNotify_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISimpleNotify_INTERFACE_DEFINED__
/* [uuid("9ea0f663-8e9f-5ac2-ace4-6e1460b7ddfc"), version, object] */
typedef struct __x_TestApp_CParity_CISimpleNotifyVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CISimpleNotify * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CISimpleNotify * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CISimpleNotify * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CISimpleNotify * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CISimpleNotifyVtbl;

interface __x_TestApp_CParity_CISimpleNotify
{
    CONST_VTBL struct __x_TestApp_CParity_CISimpleNotifyVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CISimpleNotify_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CISimpleNotify_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CISimpleNotify_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CISimpleNotify_Invoke(This) \
    ( (This)->lpVtbl->Invoke(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CISimpleNotify;
#endif /* !defined(____x_TestApp_CParity_CISimpleNotify_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ValueUpdated
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ValueUpdated:HRESULT Invoke(String,Object*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIValueUpdated_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIValueUpdated_INTERFACE_DEFINED__
/* [uuid("956a3abd-a91f-5d83-8b0c-942fa68c508a"), version, object] */
typedef struct __x_TestApp_CParity_CIValueUpdatedVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIValueUpdated * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIValueUpdated * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIValueUpdated * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIValueUpdated * This,
        /* [in] */HSTRING propertyName,
        /* [in] */IInspectable * newValue
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIValueUpdatedVtbl;

interface __x_TestApp_CParity_CIValueUpdated
{
    CONST_VTBL struct __x_TestApp_CParity_CIValueUpdatedVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIValueUpdated_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIValueUpdated_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIValueUpdated_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIValueUpdated_Invoke(This,propertyName,newValue) \
    ( (This)->lpVtbl->Invoke(This,propertyName,newValue) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIValueUpdated;
#endif /* !defined(____x_TestApp_CParity_CIValueUpdated_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ProgressHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ProgressHandler:HRESULT Invoke(Int32,String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__
/* [uuid("71a367d4-2774-5fb8-a5c2-57e63d29c9fa"), version, object] */
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
        /* [in] */int percent,
        /* [in] */HSTRING message
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

#define __x_TestApp_CParity_CIProgressHandler_Invoke(This,percent,message) \
    ( (This)->lpVtbl->Invoke(This,percent,message) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIProgressHandler;
#endif /* !defined(____x_TestApp_CParity_CIProgressHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ErrorHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ErrorHandler:HRESULT Invoke(Int32,String,Boolean);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIErrorHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIErrorHandler_INTERFACE_DEFINED__
/* [uuid("da3e2844-7e4d-5bc8-b321-bb025a1c77fc"), version, object] */
typedef struct __x_TestApp_CParity_CIErrorHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIErrorHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIErrorHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIErrorHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIErrorHandler * This,
        /* [in] */int code,
        /* [in] */HSTRING description,
        /* [in] */boolean isFatal
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIErrorHandlerVtbl;

interface __x_TestApp_CParity_CIErrorHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIErrorHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIErrorHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIErrorHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIErrorHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIErrorHandler_Invoke(This,code,description,isFatal) \
    ( (This)->lpVtbl->Invoke(This,code,description,isFatal) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIErrorHandler;
#endif /* !defined(____x_TestApp_CParity_CIErrorHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IEventEmitter
 *
 * Interface is a part of the implementation of type TestApp.Parity.EventEmitter
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IEventEmitter:HRESULT add_GenericEvent(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_GenericEvent(EventRegistrationToken);HRESULT add_NameChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_NameChanged(EventRegistrationToken);HRESULT add_CountChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Int32>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_CountChanged(EventRegistrationToken);HRESULT add_EnabledChanged(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.EventEmitter, Boolean>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_EnabledChanged(EventRegistrationToken);HRESULT add_SimpleObjectEvent(Windows.Foundation.EventHandler`1<Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_SimpleObjectEvent(EventRegistrationToken);HRESULT add_IntEvent(Windows.Foundation.EventHandler`1<Int32>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_IntEvent(EventRegistrationToken);HRESULT add_StringEvent(Windows.Foundation.EventHandler`1<String>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_StringEvent(EventRegistrationToken);HRESULT add_Notified(TestApp.Parity.SimpleNotify*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Notified(EventRegistrationToken);HRESULT add_PropertyChanged(TestApp.Parity.ValueUpdated*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_PropertyChanged(EventRegistrationToken);HRESULT add_ProgressUpdated(TestApp.Parity.ProgressHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ProgressUpdated(EventRegistrationToken);HRESULT add_ErrorOccurred(TestApp.Parity.ErrorHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ErrorOccurred(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIEventEmitter_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIEventEmitter_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IEventEmitter[] = L"TestApp.Parity.IEventEmitter";
/* [uuid("5f39207a-13fe-5246-b4f1-f8d39d1e4010"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIEventEmitterVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIEventEmitter * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIEventEmitter * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIEventEmitter * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIEventEmitter * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIEventEmitter * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIEventEmitter * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_GenericEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_GenericEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_NameChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_HSTRING * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_NameChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_CountChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_int * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_CountChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_EnabledChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CEventEmitter_boolean * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_EnabledChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_SimpleObjectEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FIEventHandler_1_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_SimpleObjectEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_IntEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FIEventHandler_1_int * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_IntEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_StringEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__FIEventHandler_1_HSTRING * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_StringEvent )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_Notified )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__x_TestApp_CParity_CISimpleNotify  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_Notified )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_PropertyChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__x_TestApp_CParity_CIValueUpdated  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_PropertyChanged )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ProgressUpdated )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__x_TestApp_CParity_CIProgressHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ProgressUpdated )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ErrorOccurred )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */__x_TestApp_CParity_CIErrorHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ErrorOccurred )(
        __x_TestApp_CParity_CIEventEmitter * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIEventEmitterVtbl;

interface __x_TestApp_CParity_CIEventEmitter
{
    CONST_VTBL struct __x_TestApp_CParity_CIEventEmitterVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIEventEmitter_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIEventEmitter_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIEventEmitter_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIEventEmitter_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIEventEmitter_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIEventEmitter_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIEventEmitter_add_GenericEvent(This,handler,token) \
    ( (This)->lpVtbl->add_GenericEvent(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_GenericEvent(This,token) \
    ( (This)->lpVtbl->remove_GenericEvent(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_NameChanged(This,handler,token) \
    ( (This)->lpVtbl->add_NameChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_NameChanged(This,token) \
    ( (This)->lpVtbl->remove_NameChanged(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_CountChanged(This,handler,token) \
    ( (This)->lpVtbl->add_CountChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_CountChanged(This,token) \
    ( (This)->lpVtbl->remove_CountChanged(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_EnabledChanged(This,handler,token) \
    ( (This)->lpVtbl->add_EnabledChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_EnabledChanged(This,token) \
    ( (This)->lpVtbl->remove_EnabledChanged(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_SimpleObjectEvent(This,handler,token) \
    ( (This)->lpVtbl->add_SimpleObjectEvent(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_SimpleObjectEvent(This,token) \
    ( (This)->lpVtbl->remove_SimpleObjectEvent(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_IntEvent(This,handler,token) \
    ( (This)->lpVtbl->add_IntEvent(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_IntEvent(This,token) \
    ( (This)->lpVtbl->remove_IntEvent(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_StringEvent(This,handler,token) \
    ( (This)->lpVtbl->add_StringEvent(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_StringEvent(This,token) \
    ( (This)->lpVtbl->remove_StringEvent(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_Notified(This,handler,token) \
    ( (This)->lpVtbl->add_Notified(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_Notified(This,token) \
    ( (This)->lpVtbl->remove_Notified(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_PropertyChanged(This,handler,token) \
    ( (This)->lpVtbl->add_PropertyChanged(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_PropertyChanged(This,token) \
    ( (This)->lpVtbl->remove_PropertyChanged(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_ProgressUpdated(This,handler,token) \
    ( (This)->lpVtbl->add_ProgressUpdated(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_ProgressUpdated(This,token) \
    ( (This)->lpVtbl->remove_ProgressUpdated(This,token) )

#define __x_TestApp_CParity_CIEventEmitter_add_ErrorOccurred(This,handler,token) \
    ( (This)->lpVtbl->add_ErrorOccurred(This,handler,token) )

#define __x_TestApp_CParity_CIEventEmitter_remove_ErrorOccurred(This,token) \
    ( (This)->lpVtbl->remove_ErrorOccurred(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIEventEmitter;
#endif /* !defined(____x_TestApp_CParity_CIEventEmitter_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.EventEmitter
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IEventEmitter ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_EventEmitter_DEFINED
#define RUNTIMECLASS_TestApp_Parity_EventEmitter_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_EventEmitter[] = L"TestApp.Parity.EventEmitter";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_event_patterns_p_h__

#endif // __parity_event_patterns_h__
