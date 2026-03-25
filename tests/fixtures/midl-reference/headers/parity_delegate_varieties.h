/* Header file automatically generated from parity_delegate_varieties.idl */
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
#ifndef __parity_delegate_varieties_h__
#define __parity_delegate_varieties_h__
#ifndef __parity_delegate_varieties_p_h__
#define __parity_delegate_varieties_p_h__


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
#ifndef ____x_TestApp_CParity_CIVoidCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVoidCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IVoidCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIVoidCallback TestApp::Parity::IVoidCallback

#endif // ____x_TestApp_CParity_CIVoidCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISingleParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CISingleParamCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ISingleParamCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CISingleParamCallback TestApp::Parity::ISingleParamCallback

#endif // ____x_TestApp_CParity_CISingleParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITwoParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CITwoParamCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ITwoParamCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CITwoParamCallback TestApp::Parity::ITwoParamCallback

#endif // ____x_TestApp_CParity_CITwoParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIThreeParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIThreeParamCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IThreeParamCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIThreeParamCallback TestApp::Parity::IThreeParamCallback

#endif // ____x_TestApp_CParity_CIThreeParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMultiParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMultiParamCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMultiParamCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMultiParamCallback TestApp::Parity::IMultiParamCallback

#endif // ____x_TestApp_CParity_CIMultiParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIStringTransform_FWD_DEFINED__
#define ____x_TestApp_CParity_CIStringTransform_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IStringTransform;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIStringTransform TestApp::Parity::IStringTransform

#endif // ____x_TestApp_CParity_CIStringTransform_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIIntCompare_FWD_DEFINED__
#define ____x_TestApp_CParity_CIIntCompare_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IIntCompare;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIIntCompare TestApp::Parity::IIntCompare

#endif // ____x_TestApp_CParity_CIIntCompare_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIBoolPredicate_FWD_DEFINED__
#define ____x_TestApp_CParity_CIBoolPredicate_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IBoolPredicate;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIBoolPredicate TestApp::Parity::IBoolPredicate

#endif // ____x_TestApp_CParity_CIBoolPredicate_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMathOperation_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMathOperation_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMathOperation;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMathOperation TestApp::Parity::IMathOperation

#endif // ____x_TestApp_CParity_CIMathOperation_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIOutCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIOutCallback_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IOutCallback;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIOutCallback TestApp::Parity::IOutCallback

#endif // ____x_TestApp_CParity_CIOutCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITryParseDelegate_FWD_DEFINED__
#define ____x_TestApp_CParity_CITryParseDelegate_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ITryParseDelegate;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CITryParseDelegate TestApp::Parity::ITryParseDelegate

#endif // ____x_TestApp_CParity_CITryParseDelegate_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDelegateUser_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDelegateUser_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDelegateUser;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDelegateUser TestApp::Parity::IDelegateUser

#endif // ____x_TestApp_CParity_CIDelegateUser_FWD_DEFINED__



/*
 *
 * Delegate TestApp.Parity.VoidCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.VoidCallback:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIVoidCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVoidCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("6300ca2f-fcb0-59bf-bfd3-d83fbb4b4e0e"), version, object] */
        MIDL_INTERFACE("6300ca2f-fcb0-59bf-bfd3-d83fbb4b4e0e")
        IVoidCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IVoidCallback=__uuidof(IVoidCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIVoidCallback;
#endif /* !defined(____x_TestApp_CParity_CIVoidCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.SingleParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.SingleParamCallback:HRESULT Invoke(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISingleParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISingleParamCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("7202752a-44e7-5dd2-aadc-14bf4ce8caf6"), version, object] */
        MIDL_INTERFACE("7202752a-44e7-5dd2-aadc-14bf4ce8caf6")
        ISingleParamCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ISingleParamCallback=__uuidof(ISingleParamCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CISingleParamCallback;
#endif /* !defined(____x_TestApp_CParity_CISingleParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.TwoParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.TwoParamCallback:HRESULT Invoke(String,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CITwoParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITwoParamCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("04084542-a866-54d2-87a9-86cb1c720535"), version, object] */
        MIDL_INTERFACE("04084542-a866-54d2-87a9-86cb1c720535")
        ITwoParamCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING name,
                /* [in] */int value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ITwoParamCallback=__uuidof(ITwoParamCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CITwoParamCallback;
#endif /* !defined(____x_TestApp_CParity_CITwoParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ThreeParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ThreeParamCallback:HRESULT Invoke(String,Int32,Boolean);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIThreeParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIThreeParamCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("5cec491b-7b13-52c7-9734-d5373dcd527a"), version, object] */
        MIDL_INTERFACE("5cec491b-7b13-52c7-9734-d5373dcd527a")
        IThreeParamCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING name,
                /* [in] */int value,
                /* [in] */::boolean enabled
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IThreeParamCallback=__uuidof(IThreeParamCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIThreeParamCallback;
#endif /* !defined(____x_TestApp_CParity_CIThreeParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.MultiParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.MultiParamCallback:HRESULT Invoke(String,Int32,Double,Boolean,Single);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMultiParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMultiParamCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("34501b3b-be64-5ef4-80b1-245513a267f9"), version, object] */
        MIDL_INTERFACE("34501b3b-be64-5ef4-80b1-245513a267f9")
        IMultiParamCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING a,
                /* [in] */int b,
                /* [in] */double c,
                /* [in] */::boolean d,
                /* [in] */float e
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMultiParamCallback=__uuidof(IMultiParamCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMultiParamCallback;
#endif /* !defined(____x_TestApp_CParity_CIMultiParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.StringTransform
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.StringTransform:HRESULT Invoke(String,String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIStringTransform_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIStringTransform_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("05ebe2a2-00c6-5ff2-8705-b206de8d3991"), version, object] */
        MIDL_INTERFACE("05ebe2a2-00c6-5ff2-8705-b206de8d3991")
        IStringTransform : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING input,
                /* [out, retval] */HSTRING * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IStringTransform=__uuidof(IStringTransform);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIStringTransform;
#endif /* !defined(____x_TestApp_CParity_CIStringTransform_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.IntCompare
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IntCompare:HRESULT Invoke(String,String,Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIIntCompare_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIIntCompare_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("da2d1926-5a90-5368-8884-fb43fd22c8c5"), version, object] */
        MIDL_INTERFACE("da2d1926-5a90-5368-8884-fb43fd22c8c5")
        IIntCompare : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING a,
                /* [in] */HSTRING b,
                /* [out, retval] */int * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IIntCompare=__uuidof(IIntCompare);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIIntCompare;
#endif /* !defined(____x_TestApp_CParity_CIIntCompare_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.BoolPredicate
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.BoolPredicate:HRESULT Invoke(Int32,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIBoolPredicate_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIBoolPredicate_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("42927cd5-8127-5536-b275-981cd965551f"), version, object] */
        MIDL_INTERFACE("42927cd5-8127-5536-b275-981cd965551f")
        IBoolPredicate : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */int value,
                /* [out, retval] */::boolean * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IBoolPredicate=__uuidof(IBoolPredicate);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIBoolPredicate;
#endif /* !defined(____x_TestApp_CParity_CIBoolPredicate_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.MathOperation
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.MathOperation:HRESULT Invoke(Double,Double,Double*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMathOperation_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMathOperation_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("ae37f7e0-ccc1-5b46-b079-9b35c96f6ce8"), version, object] */
        MIDL_INTERFACE("ae37f7e0-ccc1-5b46-b079-9b35c96f6ce8")
        IMathOperation : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */double x,
                /* [in] */double y,
                /* [out, retval] */double * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMathOperation=__uuidof(IMathOperation);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMathOperation;
#endif /* !defined(____x_TestApp_CParity_CIMathOperation_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.OutCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.OutCallback:HRESULT Invoke(String,Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIOutCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIOutCallback_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("3d867d11-6448-5112-af00-af74c592e84d"), version, object] */
        MIDL_INTERFACE("3d867d11-6448-5112-af00-af74c592e84d")
        IOutCallback : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING input,
                /* [out] */int * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IOutCallback=__uuidof(IOutCallback);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIOutCallback;
#endif /* !defined(____x_TestApp_CParity_CIOutCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.TryParseDelegate
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.TryParseDelegate:HRESULT Invoke(String,Double*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CITryParseDelegate_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITryParseDelegate_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("5db9ab3b-669b-5b0d-9ff2-6adaee8c9fc1"), version, object] */
        MIDL_INTERFACE("5db9ab3b-669b-5b0d-9ff2-6adaee8c9fc1")
        ITryParseDelegate : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING input,
                /* [out] */double * value,
                /* [out, retval] */::boolean * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ITryParseDelegate=__uuidof(ITryParseDelegate);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CITryParseDelegate;
#endif /* !defined(____x_TestApp_CParity_CITryParseDelegate_INTERFACE_DEFINED__) */


namespace TestApp {
    namespace Parity {
        class DelegateUser;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IDelegateUser
 *
 * Interface is a part of the implementation of type TestApp.Parity.DelegateUser
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDelegateUser:HRESULT add_SimpleEvent(TestApp.Parity.VoidCallback*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_SimpleEvent(EventRegistrationToken);HRESULT add_MessageReceived(TestApp.Parity.SingleParamCallback*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_MessageReceived(EventRegistrationToken);HRESULT add_TransformRequested(TestApp.Parity.StringTransform*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_TransformRequested(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDelegateUser_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDelegateUser_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDelegateUser[] = L"TestApp.Parity.IDelegateUser";

namespace TestApp {
    namespace Parity {
        /* [uuid("b748a1de-2652-5cb2-bae4-446d078e84d3"), version, object, exclusiveto] */
        MIDL_INTERFACE("b748a1de-2652-5cb2-bae4-446d078e84d3")
        IDelegateUser : public IInspectable
        {
        public:
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_SimpleEvent(
                /* [in] */TestApp::Parity::IVoidCallback  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_SimpleEvent(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_MessageReceived(
                /* [in] */TestApp::Parity::ISingleParamCallback  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_MessageReceived(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_TransformRequested(
                /* [in] */TestApp::Parity::IStringTransform  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_TransformRequested(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDelegateUser=__uuidof(IDelegateUser);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDelegateUser;
#endif /* !defined(____x_TestApp_CParity_CIDelegateUser_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.DelegateUser
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDelegateUser ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_DelegateUser_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DelegateUser_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DelegateUser[] = L"TestApp.Parity.DelegateUser";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIVoidCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVoidCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIVoidCallback __x_TestApp_CParity_CIVoidCallback;

#endif // ____x_TestApp_CParity_CIVoidCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISingleParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CISingleParamCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CISingleParamCallback __x_TestApp_CParity_CISingleParamCallback;

#endif // ____x_TestApp_CParity_CISingleParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITwoParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CITwoParamCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CITwoParamCallback __x_TestApp_CParity_CITwoParamCallback;

#endif // ____x_TestApp_CParity_CITwoParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIThreeParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIThreeParamCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIThreeParamCallback __x_TestApp_CParity_CIThreeParamCallback;

#endif // ____x_TestApp_CParity_CIThreeParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMultiParamCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMultiParamCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMultiParamCallback __x_TestApp_CParity_CIMultiParamCallback;

#endif // ____x_TestApp_CParity_CIMultiParamCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIStringTransform_FWD_DEFINED__
#define ____x_TestApp_CParity_CIStringTransform_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIStringTransform __x_TestApp_CParity_CIStringTransform;

#endif // ____x_TestApp_CParity_CIStringTransform_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIIntCompare_FWD_DEFINED__
#define ____x_TestApp_CParity_CIIntCompare_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIIntCompare __x_TestApp_CParity_CIIntCompare;

#endif // ____x_TestApp_CParity_CIIntCompare_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIBoolPredicate_FWD_DEFINED__
#define ____x_TestApp_CParity_CIBoolPredicate_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIBoolPredicate __x_TestApp_CParity_CIBoolPredicate;

#endif // ____x_TestApp_CParity_CIBoolPredicate_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMathOperation_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMathOperation_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMathOperation __x_TestApp_CParity_CIMathOperation;

#endif // ____x_TestApp_CParity_CIMathOperation_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIOutCallback_FWD_DEFINED__
#define ____x_TestApp_CParity_CIOutCallback_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIOutCallback __x_TestApp_CParity_CIOutCallback;

#endif // ____x_TestApp_CParity_CIOutCallback_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITryParseDelegate_FWD_DEFINED__
#define ____x_TestApp_CParity_CITryParseDelegate_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CITryParseDelegate __x_TestApp_CParity_CITryParseDelegate;

#endif // ____x_TestApp_CParity_CITryParseDelegate_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDelegateUser_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDelegateUser_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDelegateUser __x_TestApp_CParity_CIDelegateUser;

#endif // ____x_TestApp_CParity_CIDelegateUser_FWD_DEFINED__


/*
 *
 * Delegate TestApp.Parity.VoidCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.VoidCallback:HRESULT Invoke();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIVoidCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVoidCallback_INTERFACE_DEFINED__
/* [uuid("6300ca2f-fcb0-59bf-bfd3-d83fbb4b4e0e"), version, object] */
typedef struct __x_TestApp_CParity_CIVoidCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIVoidCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIVoidCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIVoidCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIVoidCallback * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIVoidCallbackVtbl;

interface __x_TestApp_CParity_CIVoidCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CIVoidCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIVoidCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIVoidCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIVoidCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIVoidCallback_Invoke(This) \
    ( (This)->lpVtbl->Invoke(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIVoidCallback;
#endif /* !defined(____x_TestApp_CParity_CIVoidCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.SingleParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.SingleParamCallback:HRESULT Invoke(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISingleParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISingleParamCallback_INTERFACE_DEFINED__
/* [uuid("7202752a-44e7-5dd2-aadc-14bf4ce8caf6"), version, object] */
typedef struct __x_TestApp_CParity_CISingleParamCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CISingleParamCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CISingleParamCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CISingleParamCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CISingleParamCallback * This,
        /* [in] */HSTRING value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CISingleParamCallbackVtbl;

interface __x_TestApp_CParity_CISingleParamCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CISingleParamCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CISingleParamCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CISingleParamCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CISingleParamCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CISingleParamCallback_Invoke(This,value) \
    ( (This)->lpVtbl->Invoke(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CISingleParamCallback;
#endif /* !defined(____x_TestApp_CParity_CISingleParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.TwoParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.TwoParamCallback:HRESULT Invoke(String,Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CITwoParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITwoParamCallback_INTERFACE_DEFINED__
/* [uuid("04084542-a866-54d2-87a9-86cb1c720535"), version, object] */
typedef struct __x_TestApp_CParity_CITwoParamCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CITwoParamCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CITwoParamCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CITwoParamCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CITwoParamCallback * This,
        /* [in] */HSTRING name,
        /* [in] */int value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CITwoParamCallbackVtbl;

interface __x_TestApp_CParity_CITwoParamCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CITwoParamCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CITwoParamCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CITwoParamCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CITwoParamCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CITwoParamCallback_Invoke(This,name,value) \
    ( (This)->lpVtbl->Invoke(This,name,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CITwoParamCallback;
#endif /* !defined(____x_TestApp_CParity_CITwoParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ThreeParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ThreeParamCallback:HRESULT Invoke(String,Int32,Boolean);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIThreeParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIThreeParamCallback_INTERFACE_DEFINED__
/* [uuid("5cec491b-7b13-52c7-9734-d5373dcd527a"), version, object] */
typedef struct __x_TestApp_CParity_CIThreeParamCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIThreeParamCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIThreeParamCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIThreeParamCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIThreeParamCallback * This,
        /* [in] */HSTRING name,
        /* [in] */int value,
        /* [in] */boolean enabled
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIThreeParamCallbackVtbl;

interface __x_TestApp_CParity_CIThreeParamCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CIThreeParamCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIThreeParamCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIThreeParamCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIThreeParamCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIThreeParamCallback_Invoke(This,name,value,enabled) \
    ( (This)->lpVtbl->Invoke(This,name,value,enabled) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIThreeParamCallback;
#endif /* !defined(____x_TestApp_CParity_CIThreeParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.MultiParamCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.MultiParamCallback:HRESULT Invoke(String,Int32,Double,Boolean,Single);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMultiParamCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMultiParamCallback_INTERFACE_DEFINED__
/* [uuid("34501b3b-be64-5ef4-80b1-245513a267f9"), version, object] */
typedef struct __x_TestApp_CParity_CIMultiParamCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIMultiParamCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMultiParamCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMultiParamCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIMultiParamCallback * This,
        /* [in] */HSTRING a,
        /* [in] */int b,
        /* [in] */double c,
        /* [in] */boolean d,
        /* [in] */float e
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMultiParamCallbackVtbl;

interface __x_TestApp_CParity_CIMultiParamCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CIMultiParamCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMultiParamCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMultiParamCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMultiParamCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMultiParamCallback_Invoke(This,a,b,c,d,e) \
    ( (This)->lpVtbl->Invoke(This,a,b,c,d,e) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMultiParamCallback;
#endif /* !defined(____x_TestApp_CParity_CIMultiParamCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.StringTransform
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.StringTransform:HRESULT Invoke(String,String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIStringTransform_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIStringTransform_INTERFACE_DEFINED__
/* [uuid("05ebe2a2-00c6-5ff2-8705-b206de8d3991"), version, object] */
typedef struct __x_TestApp_CParity_CIStringTransformVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIStringTransform * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIStringTransform * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIStringTransform * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIStringTransform * This,
        /* [in] */HSTRING input,
        /* [out, retval] */HSTRING * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIStringTransformVtbl;

interface __x_TestApp_CParity_CIStringTransform
{
    CONST_VTBL struct __x_TestApp_CParity_CIStringTransformVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIStringTransform_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIStringTransform_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIStringTransform_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIStringTransform_Invoke(This,input,result) \
    ( (This)->lpVtbl->Invoke(This,input,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIStringTransform;
#endif /* !defined(____x_TestApp_CParity_CIStringTransform_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.IntCompare
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IntCompare:HRESULT Invoke(String,String,Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIIntCompare_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIIntCompare_INTERFACE_DEFINED__
/* [uuid("da2d1926-5a90-5368-8884-fb43fd22c8c5"), version, object] */
typedef struct __x_TestApp_CParity_CIIntCompareVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIIntCompare * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIIntCompare * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIIntCompare * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIIntCompare * This,
        /* [in] */HSTRING a,
        /* [in] */HSTRING b,
        /* [out, retval] */int * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIIntCompareVtbl;

interface __x_TestApp_CParity_CIIntCompare
{
    CONST_VTBL struct __x_TestApp_CParity_CIIntCompareVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIIntCompare_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIIntCompare_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIIntCompare_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIIntCompare_Invoke(This,a,b,result) \
    ( (This)->lpVtbl->Invoke(This,a,b,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIIntCompare;
#endif /* !defined(____x_TestApp_CParity_CIIntCompare_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.BoolPredicate
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.BoolPredicate:HRESULT Invoke(Int32,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIBoolPredicate_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIBoolPredicate_INTERFACE_DEFINED__
/* [uuid("42927cd5-8127-5536-b275-981cd965551f"), version, object] */
typedef struct __x_TestApp_CParity_CIBoolPredicateVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIBoolPredicate * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIBoolPredicate * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIBoolPredicate * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIBoolPredicate * This,
        /* [in] */int value,
        /* [out, retval] */boolean * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIBoolPredicateVtbl;

interface __x_TestApp_CParity_CIBoolPredicate
{
    CONST_VTBL struct __x_TestApp_CParity_CIBoolPredicateVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIBoolPredicate_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIBoolPredicate_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIBoolPredicate_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIBoolPredicate_Invoke(This,value,result) \
    ( (This)->lpVtbl->Invoke(This,value,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIBoolPredicate;
#endif /* !defined(____x_TestApp_CParity_CIBoolPredicate_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.MathOperation
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.MathOperation:HRESULT Invoke(Double,Double,Double*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMathOperation_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMathOperation_INTERFACE_DEFINED__
/* [uuid("ae37f7e0-ccc1-5b46-b079-9b35c96f6ce8"), version, object] */
typedef struct __x_TestApp_CParity_CIMathOperationVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIMathOperation * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMathOperation * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMathOperation * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIMathOperation * This,
        /* [in] */double x,
        /* [in] */double y,
        /* [out, retval] */double * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMathOperationVtbl;

interface __x_TestApp_CParity_CIMathOperation
{
    CONST_VTBL struct __x_TestApp_CParity_CIMathOperationVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMathOperation_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMathOperation_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMathOperation_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMathOperation_Invoke(This,x,y,result) \
    ( (This)->lpVtbl->Invoke(This,x,y,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMathOperation;
#endif /* !defined(____x_TestApp_CParity_CIMathOperation_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.OutCallback
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.OutCallback:HRESULT Invoke(String,Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIOutCallback_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIOutCallback_INTERFACE_DEFINED__
/* [uuid("3d867d11-6448-5112-af00-af74c592e84d"), version, object] */
typedef struct __x_TestApp_CParity_CIOutCallbackVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIOutCallback * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIOutCallback * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIOutCallback * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIOutCallback * This,
        /* [in] */HSTRING input,
        /* [out] */int * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIOutCallbackVtbl;

interface __x_TestApp_CParity_CIOutCallback
{
    CONST_VTBL struct __x_TestApp_CParity_CIOutCallbackVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIOutCallback_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIOutCallback_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIOutCallback_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIOutCallback_Invoke(This,input,result) \
    ( (This)->lpVtbl->Invoke(This,input,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIOutCallback;
#endif /* !defined(____x_TestApp_CParity_CIOutCallback_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.TryParseDelegate
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.TryParseDelegate:HRESULT Invoke(String,Double*,Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CITryParseDelegate_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITryParseDelegate_INTERFACE_DEFINED__
/* [uuid("5db9ab3b-669b-5b0d-9ff2-6adaee8c9fc1"), version, object] */
typedef struct __x_TestApp_CParity_CITryParseDelegateVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CITryParseDelegate * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CITryParseDelegate * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CITryParseDelegate * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CITryParseDelegate * This,
        /* [in] */HSTRING input,
        /* [out] */double * value,
        /* [out, retval] */boolean * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CITryParseDelegateVtbl;

interface __x_TestApp_CParity_CITryParseDelegate
{
    CONST_VTBL struct __x_TestApp_CParity_CITryParseDelegateVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CITryParseDelegate_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CITryParseDelegate_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CITryParseDelegate_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CITryParseDelegate_Invoke(This,input,value,result) \
    ( (This)->lpVtbl->Invoke(This,input,value,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CITryParseDelegate;
#endif /* !defined(____x_TestApp_CParity_CITryParseDelegate_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IDelegateUser
 *
 * Interface is a part of the implementation of type TestApp.Parity.DelegateUser
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDelegateUser:HRESULT add_SimpleEvent(TestApp.Parity.VoidCallback*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_SimpleEvent(EventRegistrationToken);HRESULT add_MessageReceived(TestApp.Parity.SingleParamCallback*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_MessageReceived(EventRegistrationToken);HRESULT add_TransformRequested(TestApp.Parity.StringTransform*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_TransformRequested(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDelegateUser_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDelegateUser_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDelegateUser[] = L"TestApp.Parity.IDelegateUser";
/* [uuid("b748a1de-2652-5cb2-bae4-446d078e84d3"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDelegateUserVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDelegateUser * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDelegateUser * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDelegateUser * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDelegateUser * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDelegateUser * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDelegateUser * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_SimpleEvent )(
        __x_TestApp_CParity_CIDelegateUser * This,
        /* [in] */__x_TestApp_CParity_CIVoidCallback  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_SimpleEvent )(
        __x_TestApp_CParity_CIDelegateUser * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_MessageReceived )(
        __x_TestApp_CParity_CIDelegateUser * This,
        /* [in] */__x_TestApp_CParity_CISingleParamCallback  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_MessageReceived )(
        __x_TestApp_CParity_CIDelegateUser * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_TransformRequested )(
        __x_TestApp_CParity_CIDelegateUser * This,
        /* [in] */__x_TestApp_CParity_CIStringTransform  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_TransformRequested )(
        __x_TestApp_CParity_CIDelegateUser * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDelegateUserVtbl;

interface __x_TestApp_CParity_CIDelegateUser
{
    CONST_VTBL struct __x_TestApp_CParity_CIDelegateUserVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDelegateUser_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDelegateUser_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDelegateUser_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDelegateUser_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDelegateUser_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDelegateUser_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDelegateUser_add_SimpleEvent(This,handler,token) \
    ( (This)->lpVtbl->add_SimpleEvent(This,handler,token) )

#define __x_TestApp_CParity_CIDelegateUser_remove_SimpleEvent(This,token) \
    ( (This)->lpVtbl->remove_SimpleEvent(This,token) )

#define __x_TestApp_CParity_CIDelegateUser_add_MessageReceived(This,handler,token) \
    ( (This)->lpVtbl->add_MessageReceived(This,handler,token) )

#define __x_TestApp_CParity_CIDelegateUser_remove_MessageReceived(This,token) \
    ( (This)->lpVtbl->remove_MessageReceived(This,token) )

#define __x_TestApp_CParity_CIDelegateUser_add_TransformRequested(This,handler,token) \
    ( (This)->lpVtbl->add_TransformRequested(This,handler,token) )

#define __x_TestApp_CParity_CIDelegateUser_remove_TransformRequested(This,token) \
    ( (This)->lpVtbl->remove_TransformRequested(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDelegateUser;
#endif /* !defined(____x_TestApp_CParity_CIDelegateUser_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.DelegateUser
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDelegateUser ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_DelegateUser_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DelegateUser_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DelegateUser[] = L"TestApp.Parity.DelegateUser";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_delegate_varieties_p_h__

#endif // __parity_delegate_varieties_h__
