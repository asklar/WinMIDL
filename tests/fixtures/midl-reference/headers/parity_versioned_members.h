/* Header file automatically generated from parity_versioned_members.idl */
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
#ifndef __parity_versioned_members_h__
#define __parity_versioned_members_h__
#ifndef __parity_versioned_members_p_h__
#define __parity_versioned_members_p_h__


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
#if !defined(TESTAPP_PARITY_WIDGETCONTRACT_VERSION)
#define TESTAPP_PARITY_WIDGETCONTRACT_VERSION 0x40000
#endif // defined(TESTAPP_PARITY_WIDGETCONTRACT_VERSION)

#endif // defined(SPECIFIC_API_CONTRACT_DEFINITIONS)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"
// Importing Collections header
#include <windows.foundation.collections.h>

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIVersionedMember_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IVersionedMember;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIVersionedMember TestApp::Parity::IVersionedMember

#endif // ____x_TestApp_CParity_CIVersionedMember_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMember2_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember2_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IVersionedMember2;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIVersionedMember2 TestApp::Parity::IVersionedMember2

#endif // ____x_TestApp_CParity_CIVersionedMember2_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMember3_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember3_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IVersionedMember3;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIVersionedMember3 TestApp::Parity::IVersionedMember3

#endif // ____x_TestApp_CParity_CIVersionedMember3_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMember4_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember4_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IVersionedMember4;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIVersionedMember4 TestApp::Parity::IVersionedMember4

#endif // ____x_TestApp_CParity_CIVersionedMember4_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMemberStatics_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMemberStatics_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IVersionedMemberStatics;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIVersionedMemberStatics TestApp::Parity::IVersionedMemberStatics

#endif // ____x_TestApp_CParity_CIVersionedMemberStatics_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

namespace TestApp {
    namespace Parity {
        class VersionedMember;
    } /* Parity */
} /* TestApp */



#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000

#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_USE
#define DEF___FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("08db1618-b788-5d9a-a14e-cc347e25a960"))
ITypedEventHandler<TestApp::Parity::VersionedMember*,IInspectable*> : ITypedEventHandler_impl<Windows::Foundation::Internal::AggregateType<TestApp::Parity::VersionedMember*, TestApp::Parity::IVersionedMember*>,IInspectable*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.TypedEventHandler`2<TestApp.Parity.VersionedMember, Object>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef ITypedEventHandler<TestApp::Parity::VersionedMember*,IInspectable*> __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_t;
#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable Windows::Foundation::__FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_t
/* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable Windows::Foundation::ITypedEventHandler<TestApp::Parity::IVersionedMember*,IInspectable*>
//#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_t Windows::Foundation::ITypedEventHandler<TestApp::Parity::IVersionedMember*,IInspectable*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_USE */


#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000






/*
 *
 * Interface TestApp.Parity.IVersionedMember
 *
 * Introduced to TestApp.Parity.WidgetContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember:HRESULT get_Name(String*);HRESULT DoBasicWork();HRESULT get_IsReady(Boolean*);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIVersionedMember_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember[] = L"TestApp.Parity.IVersionedMember";

namespace TestApp {
    namespace Parity {
        /* [uuid("14d12410-9308-55e3-b8d3-ac13dc41c448"), contract, object, exclusiveto] */
        MIDL_INTERFACE("14d12410-9308-55e3-b8d3-ac13dc41c448")
        IVersionedMember : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Name(
                /* [out, retval] */HSTRING * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE DoBasicWork(void) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_IsReady(
                /* [out, retval] */::boolean * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IVersionedMember=__uuidof(IVersionedMember);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IVersionedMember2
 *
 * Introduced to TestApp.Parity.WidgetContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember2:HRESULT get_Count(Int32*);HRESULT put_Count(Int32);HRESULT DoAdvancedWork();HRESULT get_Efficiency(Double*);HRESULT add_Updated(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.VersionedMember, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Updated(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CIVersionedMember2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember2[] = L"TestApp.Parity.IVersionedMember2";

namespace TestApp {
    namespace Parity {
        /* [uuid("f2826cb2-82f1-5701-8da6-60f66afd09a0"), contract, object, exclusiveto] */
        MIDL_INTERFACE("f2826cb2-82f1-5701-8da6-60f66afd09a0")
        IVersionedMember2 : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Count(
                /* [out, retval] */int * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_Count(
                /* [in] */int value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE DoAdvancedWork(void) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Efficiency(
                /* [out, retval] */double * value
                ) = 0;
            /* [eventadd] */virtual HRESULT STDMETHODCALLTYPE add_Updated(
                /* [in] */__FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable * handler,
                /* [retval, out] */EventRegistrationToken * token
                ) = 0;
            /* [eventremove] */virtual HRESULT STDMETHODCALLTYPE remove_Updated(
                /* [in] */EventRegistrationToken token
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IVersionedMember2=__uuidof(IVersionedMember2);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember2;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x20000


/*
 *
 * Interface TestApp.Parity.IVersionedMember3
 *
 * Introduced to TestApp.Parity.WidgetContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember3:HRESULT get_IsOptimized(Boolean*);HRESULT Optimize(Int32);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIVersionedMember3_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember3_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember3[] = L"TestApp.Parity.IVersionedMember3";

namespace TestApp {
    namespace Parity {
        /* [uuid("90b67286-d40e-5f4e-ae9c-9a109c4a8f48"), contract, object, exclusiveto] */
        MIDL_INTERFACE("90b67286-d40e-5f4e-ae9c-9a109c4a8f48")
        IVersionedMember3 : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_IsOptimized(
                /* [out, retval] */::boolean * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Optimize(
                /* [in] */int level
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IVersionedMember3=__uuidof(IVersionedMember3);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember3;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember3_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000


/*
 *
 * Interface TestApp.Parity.IVersionedMember4
 *
 * Introduced to TestApp.Parity.WidgetContract in version 4.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember4:HRESULT Reset();HRESULT GetDiagnostics(String*);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x40000
#if !defined(____x_TestApp_CParity_CIVersionedMember4_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember4_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember4[] = L"TestApp.Parity.IVersionedMember4";

namespace TestApp {
    namespace Parity {
        /* [uuid("b8e73904-6656-5335-be86-c539753bfc26"), contract, object, exclusiveto] */
        MIDL_INTERFACE("b8e73904-6656-5335-be86-c539753bfc26")
        IVersionedMember4 : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Reset(void) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetDiagnostics(
                /* [out, retval] */HSTRING * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IVersionedMember4=__uuidof(IVersionedMember4);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember4;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember4_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x40000


/*
 *
 * Interface TestApp.Parity.IVersionedMemberStatics
 *
 * Introduced to TestApp.Parity.WidgetContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMemberStatics:HRESULT CreateOptimized(String,TestApp.Parity.VersionedMember**);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIVersionedMemberStatics_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMemberStatics_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMemberStatics[] = L"TestApp.Parity.IVersionedMemberStatics";

namespace TestApp {
    namespace Parity {
        /* [uuid("4853231a-ba8a-5096-b029-ebbb96b41321"), contract, object, exclusiveto] */
        MIDL_INTERFACE("4853231a-ba8a-5096-b029-ebbb96b41321")
        IVersionedMemberStatics : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateOptimized(
                /* [in] */HSTRING name,
                /* [out, retval] */TestApp::Parity::IVersionedMember * * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IVersionedMemberStatics=__uuidof(IVersionedMemberStatics);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMemberStatics;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMemberStatics_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000


/*
 *
 * Class TestApp.Parity.VersionedMember
 *
 * Introduced to TestApp.Parity.WidgetContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.WidgetContract API contract
 *
 * RuntimeClass contains static methods.
 *   Static Methods exist on the TestApp.Parity.IVersionedMemberStatics interface starting with version 3.0 of the TestApp.Parity.WidgetContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IVersionedMember ** Default Interface **
 *    TestApp.Parity.IVersionedMember2
 *    TestApp.Parity.IVersionedMember3
 *    TestApp.Parity.IVersionedMember4
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_VersionedMember_DEFINED
#define RUNTIMECLASS_TestApp_Parity_VersionedMember_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_VersionedMember[] = L"TestApp.Parity.VersionedMember";
#endif
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIVersionedMember_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIVersionedMember __x_TestApp_CParity_CIVersionedMember;

#endif // ____x_TestApp_CParity_CIVersionedMember_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMember2_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember2_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIVersionedMember2 __x_TestApp_CParity_CIVersionedMember2;

#endif // ____x_TestApp_CParity_CIVersionedMember2_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMember3_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember3_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIVersionedMember3 __x_TestApp_CParity_CIVersionedMember3;

#endif // ____x_TestApp_CParity_CIVersionedMember3_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMember4_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMember4_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIVersionedMember4 __x_TestApp_CParity_CIVersionedMember4;

#endif // ____x_TestApp_CParity_CIVersionedMember4_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIVersionedMemberStatics_FWD_DEFINED__
#define ____x_TestApp_CParity_CIVersionedMemberStatics_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIVersionedMemberStatics __x_TestApp_CParity_CIVersionedMemberStatics;

#endif // ____x_TestApp_CParity_CIVersionedMemberStatics_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions


#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000
#if !defined(____FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_INTERFACE_DEFINED__)
#define ____FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_INTERFACE_DEFINED__

typedef interface __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable;

typedef struct __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectableVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable * This);

    HRESULT ( STDMETHODCALLTYPE *Invoke )(__RPC__in __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable * This,/* [in] */ __RPC__in_opt __x_TestApp_CParity_CIVersionedMember * sender,/* [in] */ __RPC__in_opt IInspectable * e);
    END_INTERFACE
} __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectableVtbl;

interface __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable
{
    CONST_VTBL struct __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectableVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_Invoke(This,sender,e)	\
    ( (This)->lpVtbl -> Invoke(This,sender,e) ) 
#endif /* COBJMACROS */



#endif // ____FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable_INTERFACE_DEFINED__

#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000





/*
 *
 * Interface TestApp.Parity.IVersionedMember
 *
 * Introduced to TestApp.Parity.WidgetContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember:HRESULT get_Name(String*);HRESULT DoBasicWork();HRESULT get_IsReady(Boolean*);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIVersionedMember_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember[] = L"TestApp.Parity.IVersionedMember";
/* [uuid("14d12410-9308-55e3-b8d3-ac13dc41c448"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIVersionedMemberVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIVersionedMember * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Name )(
        __x_TestApp_CParity_CIVersionedMember * This,
        /* [out, retval] */HSTRING * value
        );
    HRESULT ( STDMETHODCALLTYPE *DoBasicWork )(
        __x_TestApp_CParity_CIVersionedMember * This
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_IsReady )(
        __x_TestApp_CParity_CIVersionedMember * This,
        /* [out, retval] */boolean * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIVersionedMemberVtbl;

interface __x_TestApp_CParity_CIVersionedMember
{
    CONST_VTBL struct __x_TestApp_CParity_CIVersionedMemberVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIVersionedMember_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIVersionedMember_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIVersionedMember_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIVersionedMember_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIVersionedMember_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIVersionedMember_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIVersionedMember_get_Name(This,value) \
    ( (This)->lpVtbl->get_Name(This,value) )

#define __x_TestApp_CParity_CIVersionedMember_DoBasicWork(This) \
    ( (This)->lpVtbl->DoBasicWork(This) )

#define __x_TestApp_CParity_CIVersionedMember_get_IsReady(This,value) \
    ( (This)->lpVtbl->get_IsReady(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IVersionedMember2
 *
 * Introduced to TestApp.Parity.WidgetContract in version 2.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember2:HRESULT get_Count(Int32*);HRESULT put_Count(Int32);HRESULT DoAdvancedWork();HRESULT get_Efficiency(Double*);HRESULT add_Updated(Windows.Foundation.TypedEventHandler`2<TestApp.Parity.VersionedMember, Object>*,Windows.Foundation.EventRegistrationToken*);HRESULT remove_Updated(EventRegistrationToken);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x20000
#if !defined(____x_TestApp_CParity_CIVersionedMember2_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember2_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember2[] = L"TestApp.Parity.IVersionedMember2";
/* [uuid("f2826cb2-82f1-5701-8da6-60f66afd09a0"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIVersionedMember2Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIVersionedMember2 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember2 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember2 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember2 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember2 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember2 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Count )(
        __x_TestApp_CParity_CIVersionedMember2 * This,
        /* [out, retval] */int * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_Count )(
        __x_TestApp_CParity_CIVersionedMember2 * This,
        /* [in] */int value
        );
    HRESULT ( STDMETHODCALLTYPE *DoAdvancedWork )(
        __x_TestApp_CParity_CIVersionedMember2 * This
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Efficiency )(
        __x_TestApp_CParity_CIVersionedMember2 * This,
        /* [out, retval] */double * value
        );
    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_Updated )(
        __x_TestApp_CParity_CIVersionedMember2 * This,
        /* [in] */__FITypedEventHandler_2_TestApp__CParity__CVersionedMember_IInspectable * handler,
        /* [retval, out] */EventRegistrationToken * token
        );
    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_Updated )(
        __x_TestApp_CParity_CIVersionedMember2 * This,
        /* [in] */EventRegistrationToken token
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIVersionedMember2Vtbl;

interface __x_TestApp_CParity_CIVersionedMember2
{
    CONST_VTBL struct __x_TestApp_CParity_CIVersionedMember2Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIVersionedMember2_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIVersionedMember2_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIVersionedMember2_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIVersionedMember2_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIVersionedMember2_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIVersionedMember2_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIVersionedMember2_get_Count(This,value) \
    ( (This)->lpVtbl->get_Count(This,value) )

#define __x_TestApp_CParity_CIVersionedMember2_put_Count(This,value) \
    ( (This)->lpVtbl->put_Count(This,value) )

#define __x_TestApp_CParity_CIVersionedMember2_DoAdvancedWork(This) \
    ( (This)->lpVtbl->DoAdvancedWork(This) )

#define __x_TestApp_CParity_CIVersionedMember2_get_Efficiency(This,value) \
    ( (This)->lpVtbl->get_Efficiency(This,value) )

#define __x_TestApp_CParity_CIVersionedMember2_add_Updated(This,handler,token) \
    ( (This)->lpVtbl->add_Updated(This,handler,token) )

#define __x_TestApp_CParity_CIVersionedMember2_remove_Updated(This,token) \
    ( (This)->lpVtbl->remove_Updated(This,token) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember2;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember2_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x20000


/*
 *
 * Interface TestApp.Parity.IVersionedMember3
 *
 * Introduced to TestApp.Parity.WidgetContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember3:HRESULT get_IsOptimized(Boolean*);HRESULT Optimize(Int32);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIVersionedMember3_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember3_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember3[] = L"TestApp.Parity.IVersionedMember3";
/* [uuid("90b67286-d40e-5f4e-ae9c-9a109c4a8f48"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIVersionedMember3Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIVersionedMember3 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember3 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember3 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember3 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember3 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember3 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_IsOptimized )(
        __x_TestApp_CParity_CIVersionedMember3 * This,
        /* [out, retval] */boolean * value
        );
    HRESULT ( STDMETHODCALLTYPE *Optimize )(
        __x_TestApp_CParity_CIVersionedMember3 * This,
        /* [in] */int level
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIVersionedMember3Vtbl;

interface __x_TestApp_CParity_CIVersionedMember3
{
    CONST_VTBL struct __x_TestApp_CParity_CIVersionedMember3Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIVersionedMember3_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIVersionedMember3_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIVersionedMember3_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIVersionedMember3_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIVersionedMember3_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIVersionedMember3_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIVersionedMember3_get_IsOptimized(This,value) \
    ( (This)->lpVtbl->get_IsOptimized(This,value) )

#define __x_TestApp_CParity_CIVersionedMember3_Optimize(This,level) \
    ( (This)->lpVtbl->Optimize(This,level) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember3;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember3_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000


/*
 *
 * Interface TestApp.Parity.IVersionedMember4
 *
 * Introduced to TestApp.Parity.WidgetContract in version 4.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMember4:HRESULT Reset();HRESULT GetDiagnostics(String*);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x40000
#if !defined(____x_TestApp_CParity_CIVersionedMember4_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMember4_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMember4[] = L"TestApp.Parity.IVersionedMember4";
/* [uuid("b8e73904-6656-5335-be86-c539753bfc26"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIVersionedMember4Vtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIVersionedMember4 * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember4 * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember4 * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember4 * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember4 * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIVersionedMember4 * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Reset )(
        __x_TestApp_CParity_CIVersionedMember4 * This
        );
    HRESULT ( STDMETHODCALLTYPE *GetDiagnostics )(
        __x_TestApp_CParity_CIVersionedMember4 * This,
        /* [out, retval] */HSTRING * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIVersionedMember4Vtbl;

interface __x_TestApp_CParity_CIVersionedMember4
{
    CONST_VTBL struct __x_TestApp_CParity_CIVersionedMember4Vtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIVersionedMember4_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIVersionedMember4_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIVersionedMember4_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIVersionedMember4_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIVersionedMember4_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIVersionedMember4_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIVersionedMember4_Reset(This) \
    ( (This)->lpVtbl->Reset(This) )

#define __x_TestApp_CParity_CIVersionedMember4_GetDiagnostics(This,result) \
    ( (This)->lpVtbl->GetDiagnostics(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMember4;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMember4_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x40000


/*
 *
 * Interface TestApp.Parity.IVersionedMemberStatics
 *
 * Introduced to TestApp.Parity.WidgetContract in version 3.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.VersionedMember
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IVersionedMemberStatics:HRESULT CreateOptimized(String,TestApp.Parity.VersionedMember**);
 *
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000
#if !defined(____x_TestApp_CParity_CIVersionedMemberStatics_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIVersionedMemberStatics_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IVersionedMemberStatics[] = L"TestApp.Parity.IVersionedMemberStatics";
/* [uuid("4853231a-ba8a-5096-b029-ebbb96b41321"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIVersionedMemberStaticsVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIVersionedMemberStatics * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIVersionedMemberStatics * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIVersionedMemberStatics * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIVersionedMemberStatics * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIVersionedMemberStatics * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIVersionedMemberStatics * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateOptimized )(
        __x_TestApp_CParity_CIVersionedMemberStatics * This,
        /* [in] */HSTRING name,
        /* [out, retval] */__x_TestApp_CParity_CIVersionedMember * * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIVersionedMemberStaticsVtbl;

interface __x_TestApp_CParity_CIVersionedMemberStatics
{
    CONST_VTBL struct __x_TestApp_CParity_CIVersionedMemberStaticsVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIVersionedMemberStatics_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIVersionedMemberStatics_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIVersionedMemberStatics_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIVersionedMemberStatics_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIVersionedMemberStatics_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIVersionedMemberStatics_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIVersionedMemberStatics_CreateOptimized(This,name,result) \
    ( (This)->lpVtbl->CreateOptimized(This,name,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIVersionedMemberStatics;
#endif /* !defined(____x_TestApp_CParity_CIVersionedMemberStatics_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x30000


/*
 *
 * Class TestApp.Parity.VersionedMember
 *
 * Introduced to TestApp.Parity.WidgetContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.WidgetContract API contract
 *
 * RuntimeClass contains static methods.
 *   Static Methods exist on the TestApp.Parity.IVersionedMemberStatics interface starting with version 3.0 of the TestApp.Parity.WidgetContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IVersionedMember ** Default Interface **
 *    TestApp.Parity.IVersionedMember2
 *    TestApp.Parity.IVersionedMember3
 *    TestApp.Parity.IVersionedMember4
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_VersionedMember_DEFINED
#define RUNTIMECLASS_TestApp_Parity_VersionedMember_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_VersionedMember[] = L"TestApp.Parity.VersionedMember";
#endif
#endif // TESTAPP_PARITY_WIDGETCONTRACT_VERSION >= 0x10000


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_versioned_members_p_h__

#endif // __parity_versioned_members_h__
