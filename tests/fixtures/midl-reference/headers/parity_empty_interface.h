/* Header file automatically generated from parity_empty_interface.idl */
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
#ifndef __parity_empty_interface_h__
#define __parity_empty_interface_h__
#ifndef __parity_empty_interface_p_h__
#define __parity_empty_interface_p_h__


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
#if !defined(TESTAPP_PARITY_MARKERCONTRACT_VERSION)
#define TESTAPP_PARITY_MARKERCONTRACT_VERSION 0x10000
#endif // defined(TESTAPP_PARITY_MARKERCONTRACT_VERSION)

#endif // defined(SPECIFIC_API_CONTRACT_DEFINITIONS)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIMarker_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMarker_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMarker;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMarker TestApp::Parity::IMarker

#endif // ____x_TestApp_CParity_CIMarker_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITagged_FWD_DEFINED__
#define ____x_TestApp_CParity_CITagged_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ITagged;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CITagged TestApp::Parity::ITagged

#endif // ____x_TestApp_CParity_CITagged_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIExtendedMarker_FWD_DEFINED__
#define ____x_TestApp_CParity_CIExtendedMarker_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IExtendedMarker;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIExtendedMarker TestApp::Parity::IExtendedMarker

#endif // ____x_TestApp_CParity_CIExtendedMarker_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMarkerWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMarkerWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMarkerWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMarkerWidget TestApp::Parity::IMarkerWidget

#endif // ____x_TestApp_CParity_CIMarkerWidget_FWD_DEFINED__




/*
 *
 * Interface TestApp.Parity.IMarker
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIMarker_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMarker_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMarker[] = L"TestApp.Parity.IMarker";

namespace TestApp {
    namespace Parity {
        /* [object, uuid("A1B2C3D4-E5F6-7890-ABCD-EF0123456789"), contract] */
        MIDL_INTERFACE("A1B2C3D4-E5F6-7890-ABCD-EF0123456789")
        IMarker : public IInspectable
        {
        public:
            
        };

        MIDL_CONST_ID IID & IID_IMarker=__uuidof(IMarker);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMarker;
#endif /* !defined(____x_TestApp_CParity_CIMarker_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.ITagged
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CITagged_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITagged_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ITagged[] = L"TestApp.Parity.ITagged";

namespace TestApp {
    namespace Parity {
        /* [object, uuid("B2C3D4E5-F6A7-8901-BCDE-F01234567890"), contract] */
        MIDL_INTERFACE("B2C3D4E5-F6A7-8901-BCDE-F01234567890")
        ITagged : public IInspectable
        {
        public:
            
        };

        MIDL_CONST_ID IID & IID_ITagged=__uuidof(ITagged);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CITagged;
#endif /* !defined(____x_TestApp_CParity_CITagged_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IExtendedMarker
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 * Any object which implements this interface must also implement the following interfaces:
 *     TestApp.Parity.IMarker
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIExtendedMarker_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIExtendedMarker_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IExtendedMarker[] = L"TestApp.Parity.IExtendedMarker";

namespace TestApp {
    namespace Parity {
        /* [object, uuid("C3D4E5F6-A7B8-9012-CDEF-012345678901"), contract] */
        MIDL_INTERFACE("C3D4E5F6-A7B8-9012-CDEF-012345678901")
        IExtendedMarker : public IInspectable
        {
        public:
            
        };

        MIDL_CONST_ID IID & IID_IExtendedMarker=__uuidof(IExtendedMarker);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIExtendedMarker;
#endif /* !defined(____x_TestApp_CParity_CIExtendedMarker_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


namespace TestApp {
    namespace Parity {
        class MarkerWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IMarkerWidget
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.MarkerWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMarkerWidget:HRESULT get_Name(String*);
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIMarkerWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMarkerWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMarkerWidget[] = L"TestApp.Parity.IMarkerWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("f3a853f5-5052-53d0-a41b-512d64cc4b6e"), contract, object, exclusiveto] */
        MIDL_INTERFACE("f3a853f5-5052-53d0-a41b-512d64cc4b6e")
        IMarkerWidget : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Name(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMarkerWidget=__uuidof(IMarkerWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMarkerWidget;
#endif /* !defined(____x_TestApp_CParity_CIMarkerWidget_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.MarkerWidget
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.MarkerContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMarkerWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_MarkerWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_MarkerWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_MarkerWidget[] = L"TestApp.Parity.MarkerWidget";
#endif
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIMarker_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMarker_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMarker __x_TestApp_CParity_CIMarker;

#endif // ____x_TestApp_CParity_CIMarker_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CITagged_FWD_DEFINED__
#define ____x_TestApp_CParity_CITagged_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CITagged __x_TestApp_CParity_CITagged;

#endif // ____x_TestApp_CParity_CITagged_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIExtendedMarker_FWD_DEFINED__
#define ____x_TestApp_CParity_CIExtendedMarker_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIExtendedMarker __x_TestApp_CParity_CIExtendedMarker;

#endif // ____x_TestApp_CParity_CIExtendedMarker_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMarkerWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMarkerWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMarkerWidget __x_TestApp_CParity_CIMarkerWidget;

#endif // ____x_TestApp_CParity_CIMarkerWidget_FWD_DEFINED__



/*
 *
 * Interface TestApp.Parity.IMarker
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIMarker_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMarker_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMarker[] = L"TestApp.Parity.IMarker";
/* [object, uuid("A1B2C3D4-E5F6-7890-ABCD-EF0123456789"), contract] */
typedef struct __x_TestApp_CParity_CIMarkerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMarker * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMarker * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMarker * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMarker * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMarker * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMarker * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
END_INTERFACE
    
} __x_TestApp_CParity_CIMarkerVtbl;

interface __x_TestApp_CParity_CIMarker
{
    CONST_VTBL struct __x_TestApp_CParity_CIMarkerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMarker_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMarker_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMarker_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMarker_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMarker_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMarker_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMarker;
#endif /* !defined(____x_TestApp_CParity_CIMarker_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.ITagged
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CITagged_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CITagged_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ITagged[] = L"TestApp.Parity.ITagged";
/* [object, uuid("B2C3D4E5-F6A7-8901-BCDE-F01234567890"), contract] */
typedef struct __x_TestApp_CParity_CITaggedVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CITagged * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CITagged * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CITagged * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CITagged * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CITagged * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CITagged * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
END_INTERFACE
    
} __x_TestApp_CParity_CITaggedVtbl;

interface __x_TestApp_CParity_CITagged
{
    CONST_VTBL struct __x_TestApp_CParity_CITaggedVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CITagged_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CITagged_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CITagged_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CITagged_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CITagged_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CITagged_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CITagged;
#endif /* !defined(____x_TestApp_CParity_CITagged_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


/*
 *
 * Interface TestApp.Parity.IExtendedMarker
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 * Any object which implements this interface must also implement the following interfaces:
 *     TestApp.Parity.IMarker
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIExtendedMarker_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIExtendedMarker_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IExtendedMarker[] = L"TestApp.Parity.IExtendedMarker";
/* [object, uuid("C3D4E5F6-A7B8-9012-CDEF-012345678901"), contract] */
typedef struct __x_TestApp_CParity_CIExtendedMarkerVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIExtendedMarker * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIExtendedMarker * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIExtendedMarker * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIExtendedMarker * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIExtendedMarker * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIExtendedMarker * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
END_INTERFACE
    
} __x_TestApp_CParity_CIExtendedMarkerVtbl;

interface __x_TestApp_CParity_CIExtendedMarker
{
    CONST_VTBL struct __x_TestApp_CParity_CIExtendedMarkerVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIExtendedMarker_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIExtendedMarker_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIExtendedMarker_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIExtendedMarker_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIExtendedMarker_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIExtendedMarker_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIExtendedMarker;
#endif /* !defined(____x_TestApp_CParity_CIExtendedMarker_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000



/*
 *
 * Interface TestApp.Parity.IMarkerWidget
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 * Interface is a part of the implementation of type TestApp.Parity.MarkerWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMarkerWidget:HRESULT get_Name(String*);
 *
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000
#if !defined(____x_TestApp_CParity_CIMarkerWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMarkerWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMarkerWidget[] = L"TestApp.Parity.IMarkerWidget";
/* [uuid("f3a853f5-5052-53d0-a41b-512d64cc4b6e"), contract, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMarkerWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMarkerWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMarkerWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMarkerWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMarkerWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMarkerWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMarkerWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Name )(
        __x_TestApp_CParity_CIMarkerWidget * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMarkerWidgetVtbl;

interface __x_TestApp_CParity_CIMarkerWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIMarkerWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMarkerWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMarkerWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMarkerWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMarkerWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMarkerWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMarkerWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMarkerWidget_get_Name(This,value) \
    ( (This)->lpVtbl->get_Name(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMarkerWidget;
#endif /* !defined(____x_TestApp_CParity_CIMarkerWidget_INTERFACE_DEFINED__) */
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


/*
 *
 * Class TestApp.Parity.MarkerWidget
 *
 * Introduced to TestApp.Parity.MarkerContract in version 1.0
 *
 *
 * RuntimeClass can be activated.
 *   Type can be activated via RoActivateInstance starting with version 1.0 of the TestApp.Parity.MarkerContract API contract
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMarkerWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#if TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000

#ifndef RUNTIMECLASS_TestApp_Parity_MarkerWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_MarkerWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_MarkerWidget[] = L"TestApp.Parity.MarkerWidget";
#endif
#endif // TESTAPP_PARITY_MARKERCONTRACT_VERSION >= 0x10000


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_empty_interface_p_h__

#endif // __parity_empty_interface_h__
