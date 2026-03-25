/* Header file automatically generated from parity_guid_usage.idl */
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
#ifndef __parity_guid_usage_h__
#define __parity_guid_usage_h__
#ifndef __parity_guid_usage_p_h__
#define __parity_guid_usage_p_h__


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
#ifndef ____x_TestApp_CParity_CIGuidWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIGuidWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IGuidWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIGuidWidget TestApp::Parity::IGuidWidget

#endif // ____x_TestApp_CParity_CIGuidWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

#ifndef DEF___FIIterator_1_GUID_USE
#define DEF___FIIterator_1_GUID_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("d3d64048-82b3-53c7-9285-b0be18368482"))
IIterator<GUID> : IIterator_impl<GUID> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<Guid>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<GUID> __FIIterator_1_GUID_t;
#define __FIIterator_1_GUID Windows::Foundation::Collections::__FIIterator_1_GUID_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1_GUID Windows::Foundation::Collections::IIterator<GUID>
//#define __FIIterator_1_GUID_t Windows::Foundation::Collections::IIterator<GUID>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1_GUID_USE */




#ifndef DEF___FIIterable_1_GUID_USE
#define DEF___FIIterable_1_GUID_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("f4ca3045-5dd7-54be-982e-d88d8ca0876e"))
IIterable<GUID> : IIterable_impl<GUID> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<Guid>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<GUID> __FIIterable_1_GUID_t;
#define __FIIterable_1_GUID Windows::Foundation::Collections::__FIIterable_1_GUID_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1_GUID Windows::Foundation::Collections::IIterable<GUID>
//#define __FIIterable_1_GUID_t Windows::Foundation::Collections::IIterable<GUID>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1_GUID_USE */




#ifndef DEF___FIVectorView_1_GUID_USE
#define DEF___FIVectorView_1_GUID_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("9520e64b-15b2-52a6-98ed-3191fa6cf68a"))
IVectorView<GUID> : IVectorView_impl<GUID> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVectorView`1<Guid>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVectorView<GUID> __FIVectorView_1_GUID_t;
#define __FIVectorView_1_GUID Windows::Foundation::Collections::__FIVectorView_1_GUID_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVectorView_1_GUID Windows::Foundation::Collections::IVectorView<GUID>
//#define __FIVectorView_1_GUID_t Windows::Foundation::Collections::IVectorView<GUID>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVectorView_1_GUID_USE */




#ifndef DEF___FIVector_1_GUID_USE
#define DEF___FIVector_1_GUID_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("482e676d-b913-5ec1-afa8-5f96922e94ae"))
IVector<GUID> : IVector_impl<GUID> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IVector`1<Guid>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IVector<GUID> __FIVector_1_GUID_t;
#define __FIVector_1_GUID Windows::Foundation::Collections::__FIVector_1_GUID_t
/* Collections */ } /* Foundation */ } /* Windows */ } 

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIVector_1_GUID Windows::Foundation::Collections::IVector<GUID>
//#define __FIVector_1_GUID_t Windows::Foundation::Collections::IVector<GUID>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIVector_1_GUID_USE */






namespace TestApp {
    namespace Parity {
        class GuidWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IGuidWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.GuidWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IGuidWidget:HRESULT get_Id(Guid*);HRESULT GenerateId(Guid*);HRESULT SetId(__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001);HRESULT CompareIds(__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,Boolean*);HRESULT ProcessIds(__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001);HRESULT GetAllIds(Windows.Foundation.Collections.IVector`1<Guid>**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIGuidWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIGuidWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IGuidWidget[] = L"TestApp.Parity.IGuidWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("4a295ac4-4596-588d-8c8b-25c624e5ecd5"), version, object, exclusiveto] */
        MIDL_INTERFACE("4a295ac4-4596-588d-8c8b-25c624e5ecd5")
        IGuidWidget : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Id(
                /* [out, retval] */GUID * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GenerateId(
                /* [out, retval] */GUID * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE SetId(
                /* [in] */GUID id
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CompareIds(
                /* [in] */GUID a,
                /* [in] */GUID b,
                /* [out, retval] */::boolean * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE ProcessIds(
                /* [in] */GUID id1,
                /* [in] */GUID id2,
                /* [in] */GUID id3
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE GetAllIds(
                /* [out, retval] */__FIVector_1_GUID * * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IGuidWidget=__uuidof(IGuidWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIGuidWidget;
#endif /* !defined(____x_TestApp_CParity_CIGuidWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.GuidWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IGuidWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_GuidWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_GuidWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_GuidWidget[] = L"TestApp.Parity.GuidWidget";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIGuidWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIGuidWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIGuidWidget __x_TestApp_CParity_CIGuidWidget;

#endif // ____x_TestApp_CParity_CIGuidWidget_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions
#if !defined(____FIIterator_1_GUID_INTERFACE_DEFINED__)
#define ____FIIterator_1_GUID_INTERFACE_DEFINED__

typedef interface __FIIterator_1_GUID __FIIterator_1_GUID;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1_GUID;

typedef struct __FIIterator_1_GUIDVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1_GUID * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1_GUID * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1_GUID * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1_GUID * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1_GUID * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1_GUID * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1_GUID * This, /* [retval][out] */ __RPC__out GUID *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1_GUID * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1_GUID * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1_GUID * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) GUID *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1_GUIDVtbl;

interface __FIIterator_1_GUID
{
    CONST_VTBL struct __FIIterator_1_GUIDVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1_GUID_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1_GUID_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1_GUID_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1_GUID_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1_GUID_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1_GUID_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1_GUID_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1_GUID_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1_GUID_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1_GUID_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1_GUID_INTERFACE_DEFINED__


#if !defined(____FIIterable_1_GUID_INTERFACE_DEFINED__)
#define ____FIIterable_1_GUID_INTERFACE_DEFINED__

typedef interface __FIIterable_1_GUID __FIIterable_1_GUID;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1_GUID;

typedef  struct __FIIterable_1_GUIDVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1_GUID * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1_GUID * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1_GUID * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1_GUID * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1_GUID * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1_GUID * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1_GUID * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1_GUID **first);

    END_INTERFACE
} __FIIterable_1_GUIDVtbl;

interface __FIIterable_1_GUID
{
    CONST_VTBL struct __FIIterable_1_GUIDVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1_GUID_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1_GUID_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1_GUID_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1_GUID_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1_GUID_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1_GUID_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1_GUID_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1_GUID_INTERFACE_DEFINED__


#if !defined(____FIVectorView_1_GUID_INTERFACE_DEFINED__)
#define ____FIVectorView_1_GUID_INTERFACE_DEFINED__

typedef interface __FIVectorView_1_GUID __FIVectorView_1_GUID;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVectorView_1_GUID;

typedef struct __FIVectorView_1_GUIDVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVectorView_1_GUID * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIVectorView_1_GUID * This);

    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIVectorView_1_GUID * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIVectorView_1_GUID * This,
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
        __RPC__in __FIVectorView_1_GUID * This,
            /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
        __RPC__in __FIVectorView_1_GUID * This,
            /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )( 
                                         __RPC__in __FIVectorView_1_GUID * This,
                                         /* [in] */ unsigned int index,
                                         /* [retval][out] */ __RPC__out GUID *item);

        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
            __RPC__in __FIVectorView_1_GUID * This,
            /* [retval][out] */ __RPC__out unsigned int *size);

        HRESULT ( STDMETHODCALLTYPE *IndexOf )( 
                                               __RPC__in __FIVectorView_1_GUID * This,
            /* [in] */ GUID item,
            /* [out] */ __RPC__out unsigned int *index,
            /* [retval][out] */ __RPC__out boolean *found);

        HRESULT ( STDMETHODCALLTYPE *GetMany )( 
                                               __RPC__in __FIVectorView_1_GUID * This,
            /* [in] */ unsigned int startIndex,
            /* [in] */ unsigned int capacity,
            /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) GUID *items,
            /* [retval][out] */ __RPC__out unsigned int *actual);

        END_INTERFACE
} __FIVectorView_1_GUIDVtbl;

interface __FIVectorView_1_GUID
{
    CONST_VTBL struct __FIVectorView_1_GUIDVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVectorView_1_GUID_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVectorView_1_GUID_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVectorView_1_GUID_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVectorView_1_GUID_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVectorView_1_GUID_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVectorView_1_GUID_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVectorView_1_GUID_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVectorView_1_GUID_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVectorView_1_GUID_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVectorView_1_GUID_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#endif /* COBJMACROS */



#endif // ____FIVectorView_1_GUID_INTERFACE_DEFINED__


#if !defined(____FIVector_1_GUID_INTERFACE_DEFINED__)
#define ____FIVector_1_GUID_INTERFACE_DEFINED__

typedef interface __FIVector_1_GUID __FIVector_1_GUID;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIVector_1_GUID;

typedef struct __FIVector_1_GUIDVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIVector_1_GUID * This,
            /* [in] */ __RPC__in REFIID riid,
            /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIVector_1_GUID * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIVector_1_GUID * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIVector_1_GUID * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIVector_1_GUID * This, /* [out] */ __RPC__deref_out_opt GUID *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIVector_1_GUID * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *GetAt )(__RPC__in __FIVector_1_GUID * This,
        /* [in] */ unsigned int index,
        /* [retval][out] */ __RPC__deref_out_opt GUID *item);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Size )( 
        __RPC__in __FIVector_1_GUID * This,
        /* [retval][out] */ __RPC__out unsigned int *size);

    HRESULT ( STDMETHODCALLTYPE *GetView )(__RPC__in __FIVector_1_GUID * This, /* [retval][out] */ __RPC__deref_out_opt __FIVectorView_1_GUID **view);

    HRESULT ( STDMETHODCALLTYPE *IndexOf )(__RPC__in __FIVector_1_GUID * This,
        /* [in] */ __RPC__in GUID item,
        /* [out] */ __RPC__out unsigned int *index,
        /* [retval][out] */ __RPC__out boolean *found);

    HRESULT ( STDMETHODCALLTYPE *SetAt )(__RPC__in __FIVector_1_GUID * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in GUID item);

    HRESULT ( STDMETHODCALLTYPE *InsertAt )(__RPC__in __FIVector_1_GUID * This,
        /* [in] */ unsigned int index,
        /* [in] */ __RPC__in GUID item);

    HRESULT ( STDMETHODCALLTYPE *RemoveAt )(__RPC__in __FIVector_1_GUID * This, /* [in] */ unsigned int index);
    HRESULT ( STDMETHODCALLTYPE *Append )(__RPC__in __FIVector_1_GUID * This, /* [in] */ __RPC__in GUID item);
    HRESULT ( STDMETHODCALLTYPE *RemoveAtEnd )(__RPC__in __FIVector_1_GUID * This);
    HRESULT ( STDMETHODCALLTYPE *Clear )(__RPC__in __FIVector_1_GUID * This);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIVector_1_GUID * This,
        /* [in] */ unsigned int startIndex,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) GUID *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    HRESULT ( STDMETHODCALLTYPE *ReplaceAll )(__RPC__in __FIVector_1_GUID * This,
        /* [in] */ unsigned int count,
        /* [size_is][in] */ __RPC__in_ecount_full(count) GUID *value);

    END_INTERFACE
} __FIVector_1_GUIDVtbl;

interface __FIVector_1_GUID
{
    CONST_VTBL struct __FIVector_1_GUIDVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIVector_1_GUID_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIVector_1_GUID_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIVector_1_GUID_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIVector_1_GUID_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIVector_1_GUID_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIVector_1_GUID_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIVector_1_GUID_GetAt(This,index,item)	\
    ( (This)->lpVtbl -> GetAt(This,index,item) ) 

#define __FIVector_1_GUID_get_Size(This,size)	\
    ( (This)->lpVtbl -> get_Size(This,size) ) 

#define __FIVector_1_GUID_GetView(This,view)	\
    ( (This)->lpVtbl -> GetView(This,view) ) 

#define __FIVector_1_GUID_IndexOf(This,item,index,found)	\
    ( (This)->lpVtbl -> IndexOf(This,item,index,found) ) 

#define __FIVector_1_GUID_SetAt(This,index,item)	\
    ( (This)->lpVtbl -> SetAt(This,index,item) ) 

#define __FIVector_1_GUID_InsertAt(This,index,item)	\
    ( (This)->lpVtbl -> InsertAt(This,index,item) ) 

#define __FIVector_1_GUID_RemoveAt(This,index)	\
    ( (This)->lpVtbl -> RemoveAt(This,index) ) 

#define __FIVector_1_GUID_Append(This,item)	\
    ( (This)->lpVtbl -> Append(This,item) ) 

#define __FIVector_1_GUID_RemoveAtEnd(This)	\
    ( (This)->lpVtbl -> RemoveAtEnd(This) ) 

#define __FIVector_1_GUID_Clear(This)	\
    ( (This)->lpVtbl -> Clear(This) ) 

#define __FIVector_1_GUID_GetMany(This,startIndex,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,startIndex,capacity,items,actual) ) 

#define __FIVector_1_GUID_ReplaceAll(This,count,value)	\
    ( (This)->lpVtbl -> ReplaceAll(This,count,value) ) 

#endif /* COBJMACROS */



#endif // ____FIVector_1_GUID_INTERFACE_DEFINED__





/*
 *
 * Interface TestApp.Parity.IGuidWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.GuidWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IGuidWidget:HRESULT get_Id(Guid*);HRESULT GenerateId(Guid*);HRESULT SetId(__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001);HRESULT CompareIds(__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,Boolean*);HRESULT ProcessIds(__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001,__MIDL___MIDL_itf_parity_guid_usage2Eidl_0002_0000_0001);HRESULT GetAllIds(Windows.Foundation.Collections.IVector`1<Guid>**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIGuidWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIGuidWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IGuidWidget[] = L"TestApp.Parity.IGuidWidget";
/* [uuid("4a295ac4-4596-588d-8c8b-25c624e5ecd5"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIGuidWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIGuidWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIGuidWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIGuidWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIGuidWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIGuidWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIGuidWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Id )(
        __x_TestApp_CParity_CIGuidWidget * This,
        /* [out, retval] */GUID * value
        );
    HRESULT ( STDMETHODCALLTYPE *GenerateId )(
        __x_TestApp_CParity_CIGuidWidget * This,
        /* [out, retval] */GUID * result
        );
    HRESULT ( STDMETHODCALLTYPE *SetId )(
        __x_TestApp_CParity_CIGuidWidget * This,
        /* [in] */GUID id
        );
    HRESULT ( STDMETHODCALLTYPE *CompareIds )(
        __x_TestApp_CParity_CIGuidWidget * This,
        /* [in] */GUID a,
        /* [in] */GUID b,
        /* [out, retval] */boolean * result
        );
    HRESULT ( STDMETHODCALLTYPE *ProcessIds )(
        __x_TestApp_CParity_CIGuidWidget * This,
        /* [in] */GUID id1,
        /* [in] */GUID id2,
        /* [in] */GUID id3
        );
    HRESULT ( STDMETHODCALLTYPE *GetAllIds )(
        __x_TestApp_CParity_CIGuidWidget * This,
        /* [out, retval] */__FIVector_1_GUID * * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIGuidWidgetVtbl;

interface __x_TestApp_CParity_CIGuidWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIGuidWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIGuidWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIGuidWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIGuidWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIGuidWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIGuidWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIGuidWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIGuidWidget_get_Id(This,value) \
    ( (This)->lpVtbl->get_Id(This,value) )

#define __x_TestApp_CParity_CIGuidWidget_GenerateId(This,result) \
    ( (This)->lpVtbl->GenerateId(This,result) )

#define __x_TestApp_CParity_CIGuidWidget_SetId(This,id) \
    ( (This)->lpVtbl->SetId(This,id) )

#define __x_TestApp_CParity_CIGuidWidget_CompareIds(This,a,b,result) \
    ( (This)->lpVtbl->CompareIds(This,a,b,result) )

#define __x_TestApp_CParity_CIGuidWidget_ProcessIds(This,id1,id2,id3) \
    ( (This)->lpVtbl->ProcessIds(This,id1,id2,id3) )

#define __x_TestApp_CParity_CIGuidWidget_GetAllIds(This,result) \
    ( (This)->lpVtbl->GetAllIds(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIGuidWidget;
#endif /* !defined(____x_TestApp_CParity_CIGuidWidget_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.GuidWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IGuidWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_GuidWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_GuidWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_GuidWidget[] = L"TestApp.Parity.GuidWidget";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_guid_usage_p_h__

#endif // __parity_guid_usage_h__
