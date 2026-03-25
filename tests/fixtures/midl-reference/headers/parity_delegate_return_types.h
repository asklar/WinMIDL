/* Header file automatically generated from parity_delegate_return_types.idl */
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
#ifndef __parity_delegate_return_types_h__
#define __parity_delegate_return_types_h__
#ifndef __parity_delegate_return_types_p_h__
#define __parity_delegate_return_types_p_h__


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
#ifndef ____x_TestApp_CParity_CIStringRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIStringRequestHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IStringRequestHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIStringRequestHandler TestApp::Parity::IStringRequestHandler

#endif // ____x_TestApp_CParity_CIStringRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIIntRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIIntRequestHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IIntRequestHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIIntRequestHandler TestApp::Parity::IIntRequestHandler

#endif // ____x_TestApp_CParity_CIIntRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIBoolRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIBoolRequestHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IBoolRequestHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIBoolRequestHandler TestApp::Parity::IBoolRequestHandler

#endif // ____x_TestApp_CParity_CIBoolRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDoubleRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDoubleRequestHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDoubleRequestHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDoubleRequestHandler TestApp::Parity::IDoubleRequestHandler

#endif // ____x_TestApp_CParity_CIDoubleRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIResultRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIResultRequestHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IResultRequestHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIResultRequestHandler TestApp::Parity::IResultRequestHandler

#endif // ____x_TestApp_CParity_CIResultRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIFactoryHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIFactoryHandler_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IFactoryHandler;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIFactoryHandler TestApp::Parity::IFactoryHandler

#endif // ____x_TestApp_CParity_CIFactoryHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDelegateReturnWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDelegateReturnWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDelegateReturnWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDelegateReturnWidget TestApp::Parity::IDelegateReturnWidget

#endif // ____x_TestApp_CParity_CIDelegateReturnWidget_FWD_DEFINED__



/*
 *
 * Struct TestApp.Parity.DelegateResult
 *
 */


namespace TestApp {
    namespace Parity {
        /* [version] */
        struct DelegateResult
        {
            HSTRING Name;
            int Code;
        };
        
    } /* Parity */
} /* TestApp */


/*
 *
 * Delegate TestApp.Parity.StringRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.StringRequestHandler:HRESULT Invoke(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIStringRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIStringRequestHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("c62f3cb8-0ad8-51e1-9d38-76e9a6770416"), version, object] */
        MIDL_INTERFACE("c62f3cb8-0ad8-51e1-9d38-76e9a6770416")
        IStringRequestHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [out] */HSTRING * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IStringRequestHandler=__uuidof(IStringRequestHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIStringRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIStringRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.IntRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IntRequestHandler:HRESULT Invoke(Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIIntRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIIntRequestHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("b535fed7-f5f5-5694-b32d-eabf5733dbc1"), version, object] */
        MIDL_INTERFACE("b535fed7-f5f5-5694-b32d-eabf5733dbc1")
        IIntRequestHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [out] */int * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IIntRequestHandler=__uuidof(IIntRequestHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIIntRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIIntRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.BoolRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.BoolRequestHandler:HRESULT Invoke(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIBoolRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIBoolRequestHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("d9001b88-5479-5a87-8706-c12cabc3ae07"), version, object] */
        MIDL_INTERFACE("d9001b88-5479-5a87-8706-c12cabc3ae07")
        IBoolRequestHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [out] */::boolean * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IBoolRequestHandler=__uuidof(IBoolRequestHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIBoolRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIBoolRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.DoubleRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.DoubleRequestHandler:HRESULT Invoke(Double*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDoubleRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDoubleRequestHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("1702057d-ff62-5f5f-9bd2-81e22ec3cca4"), version, object] */
        MIDL_INTERFACE("1702057d-ff62-5f5f-9bd2-81e22ec3cca4")
        IDoubleRequestHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [out] */double * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDoubleRequestHandler=__uuidof(IDoubleRequestHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDoubleRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIDoubleRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ResultRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ResultRequestHandler:HRESULT Invoke(TestApp.Parity.DelegateResult*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIResultRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIResultRequestHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("db3d0d9d-03b9-57cc-bfc8-498db0643497"), version, object] */
        MIDL_INTERFACE("db3d0d9d-03b9-57cc-bfc8-498db0643497")
        IResultRequestHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [out] */struct TestApp::Parity::DelegateResult * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IResultRequestHandler=__uuidof(IResultRequestHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIResultRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIResultRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.FactoryHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.FactoryHandler:HRESULT Invoke(String,Object**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIFactoryHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIFactoryHandler_INTERFACE_DEFINED__

namespace TestApp {
    namespace Parity {
        /* [uuid("f2ad6c11-a7a8-5a17-adde-a8a7661ab837"), version, object] */
        MIDL_INTERFACE("f2ad6c11-a7a8-5a17-adde-a8a7661ab837")
        IFactoryHandler : public IUnknown
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Invoke(
                /* [in] */HSTRING name,
                /* [out] */IInspectable * * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IFactoryHandler=__uuidof(IFactoryHandler);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIFactoryHandler;
#endif /* !defined(____x_TestApp_CParity_CIFactoryHandler_INTERFACE_DEFINED__) */


namespace TestApp {
    namespace Parity {
        class DelegateReturnWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IDelegateReturnWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.DelegateReturnWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDelegateReturnWidget:HRESULT add_NameRequested(TestApp.Parity.StringRequestHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_NameRequested(EventRegistrationToken);HRESULT add_ConfirmationRequested(TestApp.Parity.BoolRequestHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ConfirmationRequested(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDelegateReturnWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDelegateReturnWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDelegateReturnWidget[] = L"TestApp.Parity.IDelegateReturnWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("611b9b68-2a1a-5686-bfd0-831de147b913"), version, object, exclusiveto] */
        MIDL_INTERFACE("611b9b68-2a1a-5686-bfd0-831de147b913")
        IDelegateReturnWidget : public IInspectable
        {
        public:
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_NameRequested(
                /* [in] */TestApp::Parity::IStringRequestHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_NameRequested(
                /* [in] */EventRegistrationToken token
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_ConfirmationRequested(
                /* [in] */TestApp::Parity::IBoolRequestHandler  * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_ConfirmationRequested(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDelegateReturnWidget=__uuidof(IDelegateReturnWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDelegateReturnWidget;
#endif /* !defined(____x_TestApp_CParity_CIDelegateReturnWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.DelegateReturnWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDelegateReturnWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_DelegateReturnWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DelegateReturnWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DelegateReturnWidget[] = L"TestApp.Parity.DelegateReturnWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIStringRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIStringRequestHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIStringRequestHandler __x_TestApp_CParity_CIStringRequestHandler;

#endif // ____x_TestApp_CParity_CIStringRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIIntRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIIntRequestHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIIntRequestHandler __x_TestApp_CParity_CIIntRequestHandler;

#endif // ____x_TestApp_CParity_CIIntRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIBoolRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIBoolRequestHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIBoolRequestHandler __x_TestApp_CParity_CIBoolRequestHandler;

#endif // ____x_TestApp_CParity_CIBoolRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDoubleRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDoubleRequestHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDoubleRequestHandler __x_TestApp_CParity_CIDoubleRequestHandler;

#endif // ____x_TestApp_CParity_CIDoubleRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIResultRequestHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIResultRequestHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIResultRequestHandler __x_TestApp_CParity_CIResultRequestHandler;

#endif // ____x_TestApp_CParity_CIResultRequestHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIFactoryHandler_FWD_DEFINED__
#define ____x_TestApp_CParity_CIFactoryHandler_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIFactoryHandler __x_TestApp_CParity_CIFactoryHandler;

#endif // ____x_TestApp_CParity_CIFactoryHandler_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDelegateReturnWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDelegateReturnWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDelegateReturnWidget __x_TestApp_CParity_CIDelegateReturnWidget;

#endif // ____x_TestApp_CParity_CIDelegateReturnWidget_FWD_DEFINED__


/*
 *
 * Struct TestApp.Parity.DelegateResult
 *
 */

/* [version] */
struct __x_TestApp_CParity_CDelegateResult
{
    HSTRING Name;
    int Code;
};


/*
 *
 * Delegate TestApp.Parity.StringRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.StringRequestHandler:HRESULT Invoke(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIStringRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIStringRequestHandler_INTERFACE_DEFINED__
/* [uuid("c62f3cb8-0ad8-51e1-9d38-76e9a6770416"), version, object] */
typedef struct __x_TestApp_CParity_CIStringRequestHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIStringRequestHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIStringRequestHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIStringRequestHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIStringRequestHandler * This,
        /* [out] */HSTRING * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIStringRequestHandlerVtbl;

interface __x_TestApp_CParity_CIStringRequestHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIStringRequestHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIStringRequestHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIStringRequestHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIStringRequestHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIStringRequestHandler_Invoke(This,result) \
    ( (This)->lpVtbl->Invoke(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIStringRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIStringRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.IntRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IntRequestHandler:HRESULT Invoke(Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIIntRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIIntRequestHandler_INTERFACE_DEFINED__
/* [uuid("b535fed7-f5f5-5694-b32d-eabf5733dbc1"), version, object] */
typedef struct __x_TestApp_CParity_CIIntRequestHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIIntRequestHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIIntRequestHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIIntRequestHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIIntRequestHandler * This,
        /* [out] */int * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIIntRequestHandlerVtbl;

interface __x_TestApp_CParity_CIIntRequestHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIIntRequestHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIIntRequestHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIIntRequestHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIIntRequestHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIIntRequestHandler_Invoke(This,result) \
    ( (This)->lpVtbl->Invoke(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIIntRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIIntRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.BoolRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.BoolRequestHandler:HRESULT Invoke(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIBoolRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIBoolRequestHandler_INTERFACE_DEFINED__
/* [uuid("d9001b88-5479-5a87-8706-c12cabc3ae07"), version, object] */
typedef struct __x_TestApp_CParity_CIBoolRequestHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIBoolRequestHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIBoolRequestHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIBoolRequestHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIBoolRequestHandler * This,
        /* [out] */boolean * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIBoolRequestHandlerVtbl;

interface __x_TestApp_CParity_CIBoolRequestHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIBoolRequestHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIBoolRequestHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIBoolRequestHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIBoolRequestHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIBoolRequestHandler_Invoke(This,result) \
    ( (This)->lpVtbl->Invoke(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIBoolRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIBoolRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.DoubleRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.DoubleRequestHandler:HRESULT Invoke(Double*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDoubleRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDoubleRequestHandler_INTERFACE_DEFINED__
/* [uuid("1702057d-ff62-5f5f-9bd2-81e22ec3cca4"), version, object] */
typedef struct __x_TestApp_CParity_CIDoubleRequestHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIDoubleRequestHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDoubleRequestHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDoubleRequestHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIDoubleRequestHandler * This,
        /* [out] */double * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDoubleRequestHandlerVtbl;

interface __x_TestApp_CParity_CIDoubleRequestHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIDoubleRequestHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDoubleRequestHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDoubleRequestHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDoubleRequestHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDoubleRequestHandler_Invoke(This,result) \
    ( (This)->lpVtbl->Invoke(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDoubleRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIDoubleRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.ResultRequestHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ResultRequestHandler:HRESULT Invoke(TestApp.Parity.DelegateResult*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIResultRequestHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIResultRequestHandler_INTERFACE_DEFINED__
/* [uuid("db3d0d9d-03b9-57cc-bfc8-498db0643497"), version, object] */
typedef struct __x_TestApp_CParity_CIResultRequestHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIResultRequestHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIResultRequestHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIResultRequestHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIResultRequestHandler * This,
        /* [out] */struct __x_TestApp_CParity_CDelegateResult * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIResultRequestHandlerVtbl;

interface __x_TestApp_CParity_CIResultRequestHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIResultRequestHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIResultRequestHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIResultRequestHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIResultRequestHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIResultRequestHandler_Invoke(This,result) \
    ( (This)->lpVtbl->Invoke(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIResultRequestHandler;
#endif /* !defined(____x_TestApp_CParity_CIResultRequestHandler_INTERFACE_DEFINED__) */


/*
 *
 * Delegate TestApp.Parity.FactoryHandler
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.FactoryHandler:HRESULT Invoke(String,Object**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIFactoryHandler_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIFactoryHandler_INTERFACE_DEFINED__
/* [uuid("f2ad6c11-a7a8-5a17-adde-a8a7661ab837"), version, object] */
typedef struct __x_TestApp_CParity_CIFactoryHandlerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(
    __RPC__in __x_TestApp_CParity_CIFactoryHandler * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject);

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIFactoryHandler * This);

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIFactoryHandler * This);
HRESULT ( STDMETHODCALLTYPE *Invoke )(
        __x_TestApp_CParity_CIFactoryHandler * This,
        /* [in] */HSTRING name,
        /* [out] */IInspectable * * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIFactoryHandlerVtbl;

interface __x_TestApp_CParity_CIFactoryHandler
{
    CONST_VTBL struct __x_TestApp_CParity_CIFactoryHandlerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIFactoryHandler_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIFactoryHandler_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIFactoryHandler_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIFactoryHandler_Invoke(This,name,result) \
    ( (This)->lpVtbl->Invoke(This,name,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIFactoryHandler;
#endif /* !defined(____x_TestApp_CParity_CIFactoryHandler_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IDelegateReturnWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.DelegateReturnWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDelegateReturnWidget:HRESULT add_NameRequested(TestApp.Parity.StringRequestHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_NameRequested(EventRegistrationToken);HRESULT add_ConfirmationRequested(TestApp.Parity.BoolRequestHandler*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_ConfirmationRequested(EventRegistrationToken);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDelegateReturnWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDelegateReturnWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDelegateReturnWidget[] = L"TestApp.Parity.IDelegateReturnWidget";
/* [uuid("611b9b68-2a1a-5686-bfd0-831de147b913"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDelegateReturnWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDelegateReturnWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDelegateReturnWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDelegateReturnWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDelegateReturnWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDelegateReturnWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDelegateReturnWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_NameRequested )(
        __x_TestApp_CParity_CIDelegateReturnWidget * This,
        /* [in] */__x_TestApp_CParity_CIStringRequestHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_NameRequested )(
        __x_TestApp_CParity_CIDelegateReturnWidget * This,
        /* [in] */EventRegistrationToken token
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_ConfirmationRequested )(
        __x_TestApp_CParity_CIDelegateReturnWidget * This,
        /* [in] */__x_TestApp_CParity_CIBoolRequestHandler  * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_ConfirmationRequested )(
        __x_TestApp_CParity_CIDelegateReturnWidget * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDelegateReturnWidgetVtbl;

interface __x_TestApp_CParity_CIDelegateReturnWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIDelegateReturnWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDelegateReturnWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_add_NameRequested(This,handler,token) \
    ( (This)->lpVtbl->add_NameRequested(This,handler,token) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_remove_NameRequested(This,token) \
    ( (This)->lpVtbl->remove_NameRequested(This,token) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_add_ConfirmationRequested(This,handler,token) \
    ( (This)->lpVtbl->add_ConfirmationRequested(This,handler,token) )

#define __x_TestApp_CParity_CIDelegateReturnWidget_remove_ConfirmationRequested(This,token) \
    ( (This)->lpVtbl->remove_ConfirmationRequested(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDelegateReturnWidget;
#endif /* !defined(____x_TestApp_CParity_CIDelegateReturnWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.DelegateReturnWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDelegateReturnWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_DelegateReturnWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_DelegateReturnWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_DelegateReturnWidget[] = L"TestApp.Parity.DelegateReturnWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_delegate_return_types_p_h__

#endif // __parity_delegate_return_types_h__
