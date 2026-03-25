/* Header file automatically generated from parity_composable.idl */
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
#ifndef __parity_composable_h__
#define __parity_composable_h__
#ifndef __parity_composable_p_h__
#define __parity_composable_p_h__


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

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIComposableWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IComposableWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIComposableWidget TestApp::Parity::IComposableWidget

#endif // ____x_TestApp_CParity_CIComposableWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableWidgetOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableWidgetOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IComposableWidgetOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIComposableWidgetOverrides TestApp::Parity::IComposableWidgetOverrides

#endif // ____x_TestApp_CParity_CIComposableWidgetOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IComposableWidgetProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory TestApp::Parity::IComposableWidgetProtectedFactory

#endif // ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMiddleComposable_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMiddleComposable_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMiddleComposable;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMiddleComposable TestApp::Parity::IMiddleComposable

#endif // ____x_TestApp_CParity_CIMiddleComposable_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMiddleComposableOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMiddleComposableOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMiddleComposableOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMiddleComposableOverrides TestApp::Parity::IMiddleComposableOverrides

#endif // ____x_TestApp_CParity_CIMiddleComposableOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMiddleComposableProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory TestApp::Parity::IMiddleComposableProtectedFactory

#endif // ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIFinalComposable_FWD_DEFINED__
#define ____x_TestApp_CParity_CIFinalComposable_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IFinalComposable;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIFinalComposable TestApp::Parity::IFinalComposable

#endif // ____x_TestApp_CParity_CIFinalComposable_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIFinalComposableFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIFinalComposableFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IFinalComposableFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIFinalComposableFactory TestApp::Parity::IFinalComposableFactory

#endif // ____x_TestApp_CParity_CIFinalComposableFactory_FWD_DEFINED__



namespace TestApp {
    namespace Parity {
        class ComposableWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IComposableWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableWidget:HRESULT get_Label(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableWidget[] = L"TestApp.Parity.IComposableWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("2a29c645-0830-58d1-ae49-ad0d7cc02eea"), version, object, exclusiveto] */
        MIDL_INTERFACE("2a29c645-0830-58d1-ae49-ad0d7cc02eea")
        IComposableWidget : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Label(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IComposableWidget=__uuidof(IComposableWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIComposableWidget;
#endif /* !defined(____x_TestApp_CParity_CIComposableWidget_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IComposableWidgetOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableWidgetOverrides:HRESULT OnActivated();HRESULT CanActivate(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableWidgetOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableWidgetOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableWidgetOverrides[] = L"TestApp.Parity.IComposableWidgetOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("43aeaf21-b2c1-5b8d-b1a9-825fab8b41a1"), version, object, exclusiveto] */
        MIDL_INTERFACE("43aeaf21-b2c1-5b8d-b1a9-825fab8b41a1")
        IComposableWidgetOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE OnActivated(void) = 0;
            virtual HRESULT STDMETHODCALLTYPE CanActivate(
                /* [out, retval] */::boolean * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IComposableWidgetOverrides=__uuidof(IComposableWidgetOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIComposableWidgetOverrides;
#endif /* !defined(____x_TestApp_CParity_CIComposableWidgetOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IComposableWidgetProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableWidgetProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.ComposableWidget**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.ComposableWidget**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableWidgetProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableWidgetProtectedFactory[] = L"TestApp.Parity.IComposableWidgetProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("1764a5bb-8571-5971-bcb0-96349798954b"), version, object, exclusiveto] */
        MIDL_INTERFACE("1764a5bb-8571-5971-bcb0-96349798954b")
        IComposableWidgetProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IComposableWidget * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CreateInstance2(
                /* [in] */HSTRING label,
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IComposableWidget * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IComposableWidgetProtectedFactory=__uuidof(IComposableWidgetProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIComposableWidgetProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIComposableWidgetProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ComposableWidget
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IComposableWidget ** Default Interface **
 *    TestApp.Parity.IComposableWidgetOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ComposableWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ComposableWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ComposableWidget[] = L"TestApp.Parity.ComposableWidget";
#endif


namespace TestApp {
    namespace Parity {
        class MiddleComposable;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IMiddleComposable
 *
 * Interface is a part of the implementation of type TestApp.Parity.MiddleComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMiddleComposable:HRESULT get_Priority(Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMiddleComposable_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMiddleComposable_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMiddleComposable[] = L"TestApp.Parity.IMiddleComposable";

namespace TestApp {
    namespace Parity {
        /* [uuid("e42987cd-f125-5e46-90c7-7923b90c6b3d"), version, object, exclusiveto] */
        MIDL_INTERFACE("e42987cd-f125-5e46-90c7-7923b90c6b3d")
        IMiddleComposable : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Priority(
                /* [out, retval] */int * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMiddleComposable=__uuidof(IMiddleComposable);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMiddleComposable;
#endif /* !defined(____x_TestApp_CParity_CIMiddleComposable_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMiddleComposableOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.MiddleComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMiddleComposableOverrides:HRESULT OnPriorityChanged();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMiddleComposableOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMiddleComposableOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMiddleComposableOverrides[] = L"TestApp.Parity.IMiddleComposableOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("f427b1bb-c5ab-5464-8e0d-fd3c9c1f7bb2"), version, object, exclusiveto] */
        MIDL_INTERFACE("f427b1bb-c5ab-5464-8e0d-fd3c9c1f7bb2")
        IMiddleComposableOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE OnPriorityChanged(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMiddleComposableOverrides=__uuidof(IMiddleComposableOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMiddleComposableOverrides;
#endif /* !defined(____x_TestApp_CParity_CIMiddleComposableOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMiddleComposableProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.MiddleComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMiddleComposableProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.MiddleComposable**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.MiddleComposable**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMiddleComposableProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMiddleComposableProtectedFactory[] = L"TestApp.Parity.IMiddleComposableProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("db0f4915-84b6-5602-a4b9-ca5ed3eeac1c"), version, object, exclusiveto] */
        MIDL_INTERFACE("db0f4915-84b6-5602-a4b9-ca5ed3eeac1c")
        IMiddleComposableProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IMiddleComposable * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CreateInstance2(
                /* [in] */HSTRING label,
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IMiddleComposable * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMiddleComposableProtectedFactory=__uuidof(IMiddleComposableProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMiddleComposableProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIMiddleComposableProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.MiddleComposable
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMiddleComposable ** Default Interface **
 *    TestApp.Parity.IMiddleComposableOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_MiddleComposable_DEFINED
#define RUNTIMECLASS_TestApp_Parity_MiddleComposable_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_MiddleComposable[] = L"TestApp.Parity.MiddleComposable";
#endif


namespace TestApp {
    namespace Parity {
        class FinalComposable;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IFinalComposable
 *
 * Interface is a part of the implementation of type TestApp.Parity.FinalComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IFinalComposable:HRESULT get_IsReady(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIFinalComposable_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIFinalComposable_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IFinalComposable[] = L"TestApp.Parity.IFinalComposable";

namespace TestApp {
    namespace Parity {
        /* [uuid("1ec79745-5359-5972-bb55-13ae48379cab"), version, object, exclusiveto] */
        MIDL_INTERFACE("1ec79745-5359-5972-bb55-13ae48379cab")
        IFinalComposable : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_IsReady(
                /* [out, retval] */::boolean * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IFinalComposable=__uuidof(IFinalComposable);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIFinalComposable;
#endif /* !defined(____x_TestApp_CParity_CIFinalComposable_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IFinalComposableFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.FinalComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IFinalComposableFactory:HRESULT CreateInstance(String,TestApp.Parity.FinalComposable**);HRESULT CreateInstance2(String,Int32,TestApp.Parity.FinalComposable**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIFinalComposableFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIFinalComposableFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IFinalComposableFactory[] = L"TestApp.Parity.IFinalComposableFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("8179a449-a941-52eb-9042-fbcd5a3ac2ae"), version, object, exclusiveto] */
        MIDL_INTERFACE("8179a449-a941-52eb-9042-fbcd5a3ac2ae")
        IFinalComposableFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                /* [in] */HSTRING label,
                /* [retval, out] */TestApp::Parity::IFinalComposable * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CreateInstance2(
                /* [in] */HSTRING label,
                /* [in] */int priority,
                /* [retval, out] */TestApp::Parity::IFinalComposable * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IFinalComposableFactory=__uuidof(IFinalComposableFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIFinalComposableFactory;
#endif /* !defined(____x_TestApp_CParity_CIFinalComposableFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.FinalComposable
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IFinalComposable ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_FinalComposable_DEFINED
#define RUNTIMECLASS_TestApp_Parity_FinalComposable_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_FinalComposable[] = L"TestApp.Parity.FinalComposable";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIComposableWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIComposableWidget __x_TestApp_CParity_CIComposableWidget;

#endif // ____x_TestApp_CParity_CIComposableWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableWidgetOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableWidgetOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIComposableWidgetOverrides __x_TestApp_CParity_CIComposableWidgetOverrides;

#endif // ____x_TestApp_CParity_CIComposableWidgetOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIComposableWidgetProtectedFactory __x_TestApp_CParity_CIComposableWidgetProtectedFactory;

#endif // ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMiddleComposable_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMiddleComposable_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMiddleComposable __x_TestApp_CParity_CIMiddleComposable;

#endif // ____x_TestApp_CParity_CIMiddleComposable_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMiddleComposableOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMiddleComposableOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMiddleComposableOverrides __x_TestApp_CParity_CIMiddleComposableOverrides;

#endif // ____x_TestApp_CParity_CIMiddleComposableOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMiddleComposableProtectedFactory __x_TestApp_CParity_CIMiddleComposableProtectedFactory;

#endif // ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIFinalComposable_FWD_DEFINED__
#define ____x_TestApp_CParity_CIFinalComposable_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIFinalComposable __x_TestApp_CParity_CIFinalComposable;

#endif // ____x_TestApp_CParity_CIFinalComposable_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIFinalComposableFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIFinalComposableFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIFinalComposableFactory __x_TestApp_CParity_CIFinalComposableFactory;

#endif // ____x_TestApp_CParity_CIFinalComposableFactory_FWD_DEFINED__



/*
 *
 * Interface TestApp.Parity.IComposableWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableWidget:HRESULT get_Label(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableWidget[] = L"TestApp.Parity.IComposableWidget";
/* [uuid("2a29c645-0830-58d1-ae49-ad0d7cc02eea"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIComposableWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIComposableWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIComposableWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIComposableWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIComposableWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIComposableWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIComposableWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Label )(
        __x_TestApp_CParity_CIComposableWidget * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIComposableWidgetVtbl;

interface __x_TestApp_CParity_CIComposableWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIComposableWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIComposableWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIComposableWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIComposableWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIComposableWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIComposableWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIComposableWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIComposableWidget_get_Label(This,value) \
    ( (This)->lpVtbl->get_Label(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIComposableWidget;
#endif /* !defined(____x_TestApp_CParity_CIComposableWidget_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IComposableWidgetOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableWidgetOverrides:HRESULT OnActivated();HRESULT CanActivate(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableWidgetOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableWidgetOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableWidgetOverrides[] = L"TestApp.Parity.IComposableWidgetOverrides";
/* [uuid("43aeaf21-b2c1-5b8d-b1a9-825fab8b41a1"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIComposableWidgetOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *OnActivated )(
        __x_TestApp_CParity_CIComposableWidgetOverrides * This
        );
    HRESULT ( STDMETHODCALLTYPE *CanActivate )(
        __x_TestApp_CParity_CIComposableWidgetOverrides * This,
        /* [out, retval] */boolean * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIComposableWidgetOverridesVtbl;

interface __x_TestApp_CParity_CIComposableWidgetOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CIComposableWidgetOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIComposableWidgetOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_OnActivated(This) \
    ( (This)->lpVtbl->OnActivated(This) )

#define __x_TestApp_CParity_CIComposableWidgetOverrides_CanActivate(This,result) \
    ( (This)->lpVtbl->CanActivate(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIComposableWidgetOverrides;
#endif /* !defined(____x_TestApp_CParity_CIComposableWidgetOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IComposableWidgetProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableWidgetProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.ComposableWidget**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.ComposableWidget**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableWidgetProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableWidgetProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableWidgetProtectedFactory[] = L"TestApp.Parity.IComposableWidgetProtectedFactory";
/* [uuid("1764a5bb-8571-5971-bcb0-96349798954b"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIComposableWidgetProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIComposableWidget * * value
        );
    HRESULT ( STDMETHODCALLTYPE *CreateInstance2 )(
        __x_TestApp_CParity_CIComposableWidgetProtectedFactory * This,
        /* [in] */HSTRING label,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIComposableWidget * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIComposableWidgetProtectedFactoryVtbl;

interface __x_TestApp_CParity_CIComposableWidgetProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIComposableWidgetProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )

#define __x_TestApp_CParity_CIComposableWidgetProtectedFactory_CreateInstance2(This,label,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance2(This,label,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIComposableWidgetProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIComposableWidgetProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ComposableWidget
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IComposableWidget ** Default Interface **
 *    TestApp.Parity.IComposableWidgetOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ComposableWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ComposableWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ComposableWidget[] = L"TestApp.Parity.ComposableWidget";
#endif



/*
 *
 * Interface TestApp.Parity.IMiddleComposable
 *
 * Interface is a part of the implementation of type TestApp.Parity.MiddleComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMiddleComposable:HRESULT get_Priority(Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMiddleComposable_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMiddleComposable_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMiddleComposable[] = L"TestApp.Parity.IMiddleComposable";
/* [uuid("e42987cd-f125-5e46-90c7-7923b90c6b3d"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMiddleComposableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMiddleComposable * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposable * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposable * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposable * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposable * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposable * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Priority )(
        __x_TestApp_CParity_CIMiddleComposable * This,
        /* [out, retval] */int * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMiddleComposableVtbl;

interface __x_TestApp_CParity_CIMiddleComposable
{
    CONST_VTBL struct __x_TestApp_CParity_CIMiddleComposableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMiddleComposable_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMiddleComposable_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMiddleComposable_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMiddleComposable_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMiddleComposable_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMiddleComposable_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMiddleComposable_get_Priority(This,value) \
    ( (This)->lpVtbl->get_Priority(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMiddleComposable;
#endif /* !defined(____x_TestApp_CParity_CIMiddleComposable_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMiddleComposableOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.MiddleComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMiddleComposableOverrides:HRESULT OnPriorityChanged();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMiddleComposableOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMiddleComposableOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMiddleComposableOverrides[] = L"TestApp.Parity.IMiddleComposableOverrides";
/* [uuid("f427b1bb-c5ab-5464-8e0d-fd3c9c1f7bb2"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMiddleComposableOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *OnPriorityChanged )(
        __x_TestApp_CParity_CIMiddleComposableOverrides * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMiddleComposableOverridesVtbl;

interface __x_TestApp_CParity_CIMiddleComposableOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CIMiddleComposableOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMiddleComposableOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMiddleComposableOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMiddleComposableOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMiddleComposableOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMiddleComposableOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMiddleComposableOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMiddleComposableOverrides_OnPriorityChanged(This) \
    ( (This)->lpVtbl->OnPriorityChanged(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMiddleComposableOverrides;
#endif /* !defined(____x_TestApp_CParity_CIMiddleComposableOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMiddleComposableProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.MiddleComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMiddleComposableProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.MiddleComposable**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.MiddleComposable**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMiddleComposableProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMiddleComposableProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMiddleComposableProtectedFactory[] = L"TestApp.Parity.IMiddleComposableProtectedFactory";
/* [uuid("db0f4915-84b6-5602-a4b9-ca5ed3eeac1c"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMiddleComposableProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIMiddleComposable * * value
        );
    HRESULT ( STDMETHODCALLTYPE *CreateInstance2 )(
        __x_TestApp_CParity_CIMiddleComposableProtectedFactory * This,
        /* [in] */HSTRING label,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIMiddleComposable * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMiddleComposableProtectedFactoryVtbl;

interface __x_TestApp_CParity_CIMiddleComposableProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIMiddleComposableProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )

#define __x_TestApp_CParity_CIMiddleComposableProtectedFactory_CreateInstance2(This,label,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance2(This,label,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMiddleComposableProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIMiddleComposableProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.MiddleComposable
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMiddleComposable ** Default Interface **
 *    TestApp.Parity.IMiddleComposableOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_MiddleComposable_DEFINED
#define RUNTIMECLASS_TestApp_Parity_MiddleComposable_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_MiddleComposable[] = L"TestApp.Parity.MiddleComposable";
#endif



/*
 *
 * Interface TestApp.Parity.IFinalComposable
 *
 * Interface is a part of the implementation of type TestApp.Parity.FinalComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IFinalComposable:HRESULT get_IsReady(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIFinalComposable_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIFinalComposable_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IFinalComposable[] = L"TestApp.Parity.IFinalComposable";
/* [uuid("1ec79745-5359-5972-bb55-13ae48379cab"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIFinalComposableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIFinalComposable * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIFinalComposable * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIFinalComposable * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIFinalComposable * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIFinalComposable * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIFinalComposable * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_IsReady )(
        __x_TestApp_CParity_CIFinalComposable * This,
        /* [out, retval] */boolean * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIFinalComposableVtbl;

interface __x_TestApp_CParity_CIFinalComposable
{
    CONST_VTBL struct __x_TestApp_CParity_CIFinalComposableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIFinalComposable_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIFinalComposable_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIFinalComposable_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIFinalComposable_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIFinalComposable_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIFinalComposable_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIFinalComposable_get_IsReady(This,value) \
    ( (This)->lpVtbl->get_IsReady(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIFinalComposable;
#endif /* !defined(____x_TestApp_CParity_CIFinalComposable_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IFinalComposableFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.FinalComposable
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IFinalComposableFactory:HRESULT CreateInstance(String,TestApp.Parity.FinalComposable**);HRESULT CreateInstance2(String,Int32,TestApp.Parity.FinalComposable**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIFinalComposableFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIFinalComposableFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IFinalComposableFactory[] = L"TestApp.Parity.IFinalComposableFactory";
/* [uuid("8179a449-a941-52eb-9042-fbcd5a3ac2ae"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIFinalComposableFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIFinalComposableFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIFinalComposableFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIFinalComposableFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIFinalComposableFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIFinalComposableFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIFinalComposableFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIFinalComposableFactory * This,
        /* [in] */HSTRING label,
        /* [retval, out] */__x_TestApp_CParity_CIFinalComposable * * value
        );
    HRESULT ( STDMETHODCALLTYPE *CreateInstance2 )(
        __x_TestApp_CParity_CIFinalComposableFactory * This,
        /* [in] */HSTRING label,
        /* [in] */int priority,
        /* [retval, out] */__x_TestApp_CParity_CIFinalComposable * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIFinalComposableFactoryVtbl;

interface __x_TestApp_CParity_CIFinalComposableFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIFinalComposableFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIFinalComposableFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIFinalComposableFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIFinalComposableFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIFinalComposableFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIFinalComposableFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIFinalComposableFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIFinalComposableFactory_CreateInstance(This,label,value) \
    ( (This)->lpVtbl->CreateInstance(This,label,value) )

#define __x_TestApp_CParity_CIFinalComposableFactory_CreateInstance2(This,label,priority,value) \
    ( (This)->lpVtbl->CreateInstance2(This,label,priority,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIFinalComposableFactory;
#endif /* !defined(____x_TestApp_CParity_CIFinalComposableFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.FinalComposable
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IFinalComposable ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_FinalComposable_DEFINED
#define RUNTIMECLASS_TestApp_Parity_FinalComposable_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_FinalComposable[] = L"TestApp.Parity.FinalComposable";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_composable_p_h__

#endif // __parity_composable_h__
