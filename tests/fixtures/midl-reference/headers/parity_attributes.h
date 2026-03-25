/* Header file automatically generated from parity_attributes.idl */
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
#ifndef __parity_attributes_h__
#define __parity_attributes_h__
#ifndef __parity_attributes_p_h__
#define __parity_attributes_p_h__


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
#if !defined(TESTAPP_PARITY_ATTRCONTRACT_VERSION)
#define TESTAPP_PARITY_ATTRCONTRACT_VERSION 0x10000
#endif // defined(TESTAPP_PARITY_ATTRCONTRACT_VERSION)

#endif // defined(SPECIFIC_API_CONTRACT_DEFINITIONS)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0\Windows.h"

#if defined(__cplusplus) && !defined(CINTERFACE)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIMarkedInterface_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMarkedInterface_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMarkedInterface;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMarkedInterface TestApp::Parity::IMarkedInterface

#endif // ____x_TestApp_CParity_CIMarkedInterface_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIAttributedClass_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAttributedClass_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IAttributedClass;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIAttributedClass TestApp::Parity::IAttributedClass

#endif // ____x_TestApp_CParity_CIAttributedClass_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIOldClass_FWD_DEFINED__
#define ____x_TestApp_CParity_CIOldClass_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IOldClass;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIOldClass TestApp::Parity::IOldClass

#endif // ____x_TestApp_CParity_CIOldClass_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableBase_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableBase_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IComposableBase;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIComposableBase TestApp::Parity::IComposableBase

#endif // ____x_TestApp_CParity_CIComposableBase_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableBaseProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableBaseProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IComposableBaseProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIComposableBaseProtectedFactory TestApp::Parity::IComposableBaseProtectedFactory

#endif // ____x_TestApp_CParity_CIComposableBaseProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIActivatableWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIActivatableWidget_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IActivatableWidget;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIActivatableWidget TestApp::Parity::IActivatableWidget

#endif // ____x_TestApp_CParity_CIActivatableWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIActivatableWidgetFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIActivatableWidgetFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IActivatableWidgetFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIActivatableWidgetFactory TestApp::Parity::IActivatableWidgetFactory

#endif // ____x_TestApp_CParity_CIActivatableWidgetFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISettingsHelperStatics_FWD_DEFINED__
#define ____x_TestApp_CParity_CISettingsHelperStatics_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ISettingsHelperStatics;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CISettingsHelperStatics TestApp::Parity::ISettingsHelperStatics

#endif // ____x_TestApp_CParity_CISettingsHelperStatics_FWD_DEFINED__



/*
 *
 * Interface TestApp.Parity.IMarkedInterface
 *
 */
#if !defined(____x_TestApp_CParity_CIMarkedInterface_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMarkedInterface_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMarkedInterface[] = L"TestApp.Parity.IMarkedInterface";

namespace TestApp {
    namespace Parity {
        /* [version, object, uuid("12345678-1234-5678-9ABC-DEF012345678")] */
        MIDL_INTERFACE("12345678-1234-5678-9ABC-DEF012345678")
        IMarkedInterface : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE DoSomething(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMarkedInterface=__uuidof(IMarkedInterface);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMarkedInterface;
#endif /* !defined(____x_TestApp_CParity_CIMarkedInterface_INTERFACE_DEFINED__) */


namespace TestApp {
    namespace Parity {
        class AttributedClass;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IAttributedClass
 *
 * Interface is a part of the implementation of type TestApp.Parity.AttributedClass
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAttributedClass:HRESULT get_Label(String*);HRESULT put_Label(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAttributedClass_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAttributedClass_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAttributedClass[] = L"TestApp.Parity.IAttributedClass";

namespace TestApp {
    namespace Parity {
        /* [uuid("8d27b17b-ee98-5bbc-ad8c-58e6b6174efa"), version, object, exclusiveto] */
        MIDL_INTERFACE("8d27b17b-ee98-5bbc-ad8c-58e6b6174efa")
        IAttributedClass : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Label(
                /* [out, retval] */HSTRING * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_Label(
                /* [in] */HSTRING value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IAttributedClass=__uuidof(IAttributedClass);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIAttributedClass;
#endif /* !defined(____x_TestApp_CParity_CIAttributedClass_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.AttributedClass
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IAttributedClass ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_AttributedClass_DEFINED
#define RUNTIMECLASS_TestApp_Parity_AttributedClass_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_AttributedClass[] = L"TestApp.Parity.AttributedClass";
#endif



namespace TestApp {
    namespace Parity {
        class OldClass;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IOldClass
 *
 * Interface is a part of the implementation of type TestApp.Parity.OldClass
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IOldClass:
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIOldClass_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIOldClass_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IOldClass[] = L"TestApp.Parity.IOldClass";

namespace TestApp {
    namespace Parity {
        /* [uuid("04733890-2db5-5f8a-9eb8-eab4590427a6"), deprecated, version, object, exclusiveto] */
        MIDL_INTERFACE("04733890-2db5-5f8a-9eb8-eab4590427a6")
        IOldClass : public IInspectable
        {
        public:
            
        };

        MIDL_CONST_ID IID & IID_IOldClass=__uuidof(IOldClass);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIOldClass;
#endif /* !defined(____x_TestApp_CParity_CIOldClass_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.OldClass
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IOldClass ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_OldClass_DEFINED
#define RUNTIMECLASS_TestApp_Parity_OldClass_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_OldClass[] = L"TestApp.Parity.OldClass";
#endif


namespace TestApp {
    namespace Parity {
        class ComposableBase;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IComposableBase
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableBase
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableBase:HRESULT get_Name(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableBase_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableBase_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableBase[] = L"TestApp.Parity.IComposableBase";

namespace TestApp {
    namespace Parity {
        /* [uuid("eac9c74a-2061-5609-a7a7-0173cfdbfb5f"), version, object, exclusiveto] */
        MIDL_INTERFACE("eac9c74a-2061-5609-a7a7-0173cfdbfb5f")
        IComposableBase : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Name(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IComposableBase=__uuidof(IComposableBase);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIComposableBase;
#endif /* !defined(____x_TestApp_CParity_CIComposableBase_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IComposableBaseProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableBase
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableBaseProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.ComposableBase**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.ComposableBase**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableBaseProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableBaseProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableBaseProtectedFactory[] = L"TestApp.Parity.IComposableBaseProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("2290ced5-f877-5826-beeb-acd89f91f9f1"), version, object, exclusiveto] */
        MIDL_INTERFACE("2290ced5-f877-5826-beeb-acd89f91f9f1")
        IComposableBaseProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IComposableBase * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CreateInstance2(
                /* [in] */HSTRING name,
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IComposableBase * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IComposableBaseProtectedFactory=__uuidof(IComposableBaseProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIComposableBaseProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIComposableBaseProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ComposableBase
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IComposableBase ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ComposableBase_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ComposableBase_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ComposableBase[] = L"TestApp.Parity.ComposableBase";
#endif


namespace TestApp {
    namespace Parity {
        class ActivatableWidget;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IActivatableWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ActivatableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IActivatableWidget:HRESULT Activate();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIActivatableWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIActivatableWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IActivatableWidget[] = L"TestApp.Parity.IActivatableWidget";

namespace TestApp {
    namespace Parity {
        /* [uuid("79db58e0-36b5-519b-9142-7811ef55264a"), version, object, exclusiveto] */
        MIDL_INTERFACE("79db58e0-36b5-519b-9142-7811ef55264a")
        IActivatableWidget : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Activate(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IActivatableWidget=__uuidof(IActivatableWidget);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIActivatableWidget;
#endif /* !defined(____x_TestApp_CParity_CIActivatableWidget_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IActivatableWidgetFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.ActivatableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IActivatableWidgetFactory:HRESULT CreateInstance(String,TestApp.Parity.ActivatableWidget**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIActivatableWidgetFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIActivatableWidgetFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IActivatableWidgetFactory[] = L"TestApp.Parity.IActivatableWidgetFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("83476af4-06b0-5173-8d12-92c238fdf65e"), version, object, exclusiveto] */
        MIDL_INTERFACE("83476af4-06b0-5173-8d12-92c238fdf65e")
        IActivatableWidgetFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                /* [in] */HSTRING label,
                /* [retval, out] */TestApp::Parity::IActivatableWidget * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IActivatableWidgetFactory=__uuidof(IActivatableWidgetFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIActivatableWidgetFactory;
#endif /* !defined(____x_TestApp_CParity_CIActivatableWidgetFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ActivatableWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IActivatableWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ActivatableWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ActivatableWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ActivatableWidget[] = L"TestApp.Parity.ActivatableWidget";
#endif



/*
 *
 * Interface TestApp.Parity.ISettingsHelperStatics
 *
 * Interface is a part of the implementation of type TestApp.Parity.SettingsHelper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ISettingsHelperStatics:HRESULT Read(String,String*);HRESULT Write(String,String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISettingsHelperStatics_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISettingsHelperStatics_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ISettingsHelperStatics[] = L"TestApp.Parity.ISettingsHelperStatics";

namespace TestApp {
    namespace Parity {
        /* [uuid("70248eac-4fd4-5e34-8ea4-6087ddc9fb06"), version, object, exclusiveto] */
        MIDL_INTERFACE("70248eac-4fd4-5e34-8ea4-6087ddc9fb06")
        ISettingsHelperStatics : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Read(
                /* [in] */HSTRING key,
                /* [out, retval] */HSTRING * result
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Write(
                /* [in] */HSTRING key,
                /* [in] */HSTRING data
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ISettingsHelperStatics=__uuidof(ISettingsHelperStatics);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CISettingsHelperStatics;
#endif /* !defined(____x_TestApp_CParity_CISettingsHelperStatics_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.SettingsHelper
 *
 * RuntimeClass contains static methods.
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#ifndef RUNTIMECLASS_TestApp_Parity_SettingsHelper_DEFINED
#define RUNTIMECLASS_TestApp_Parity_SettingsHelper_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_SettingsHelper[] = L"TestApp.Parity.SettingsHelper";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIMarkedInterface_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMarkedInterface_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMarkedInterface __x_TestApp_CParity_CIMarkedInterface;

#endif // ____x_TestApp_CParity_CIMarkedInterface_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIAttributedClass_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAttributedClass_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIAttributedClass __x_TestApp_CParity_CIAttributedClass;

#endif // ____x_TestApp_CParity_CIAttributedClass_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIOldClass_FWD_DEFINED__
#define ____x_TestApp_CParity_CIOldClass_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIOldClass __x_TestApp_CParity_CIOldClass;

#endif // ____x_TestApp_CParity_CIOldClass_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableBase_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableBase_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIComposableBase __x_TestApp_CParity_CIComposableBase;

#endif // ____x_TestApp_CParity_CIComposableBase_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIComposableBaseProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIComposableBaseProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIComposableBaseProtectedFactory __x_TestApp_CParity_CIComposableBaseProtectedFactory;

#endif // ____x_TestApp_CParity_CIComposableBaseProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIActivatableWidget_FWD_DEFINED__
#define ____x_TestApp_CParity_CIActivatableWidget_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIActivatableWidget __x_TestApp_CParity_CIActivatableWidget;

#endif // ____x_TestApp_CParity_CIActivatableWidget_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIActivatableWidgetFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIActivatableWidgetFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIActivatableWidgetFactory __x_TestApp_CParity_CIActivatableWidgetFactory;

#endif // ____x_TestApp_CParity_CIActivatableWidgetFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CISettingsHelperStatics_FWD_DEFINED__
#define ____x_TestApp_CParity_CISettingsHelperStatics_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CISettingsHelperStatics __x_TestApp_CParity_CISettingsHelperStatics;

#endif // ____x_TestApp_CParity_CISettingsHelperStatics_FWD_DEFINED__


/*
 *
 * Interface TestApp.Parity.IMarkedInterface
 *
 */
#if !defined(____x_TestApp_CParity_CIMarkedInterface_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMarkedInterface_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMarkedInterface[] = L"TestApp.Parity.IMarkedInterface";
/* [version, object, uuid("12345678-1234-5678-9ABC-DEF012345678")] */
typedef struct __x_TestApp_CParity_CIMarkedInterfaceVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMarkedInterface * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMarkedInterface * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMarkedInterface * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMarkedInterface * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMarkedInterface * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMarkedInterface * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *DoSomething )(
        __x_TestApp_CParity_CIMarkedInterface * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMarkedInterfaceVtbl;

interface __x_TestApp_CParity_CIMarkedInterface
{
    CONST_VTBL struct __x_TestApp_CParity_CIMarkedInterfaceVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMarkedInterface_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMarkedInterface_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMarkedInterface_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMarkedInterface_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMarkedInterface_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMarkedInterface_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMarkedInterface_DoSomething(This) \
    ( (This)->lpVtbl->DoSomething(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMarkedInterface;
#endif /* !defined(____x_TestApp_CParity_CIMarkedInterface_INTERFACE_DEFINED__) */



/*
 *
 * Interface TestApp.Parity.IAttributedClass
 *
 * Interface is a part of the implementation of type TestApp.Parity.AttributedClass
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAttributedClass:HRESULT get_Label(String*);HRESULT put_Label(String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAttributedClass_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAttributedClass_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAttributedClass[] = L"TestApp.Parity.IAttributedClass";
/* [uuid("8d27b17b-ee98-5bbc-ad8c-58e6b6174efa"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIAttributedClassVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIAttributedClass * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIAttributedClass * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIAttributedClass * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIAttributedClass * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIAttributedClass * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIAttributedClass * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Label )(
        __x_TestApp_CParity_CIAttributedClass * This,
        /* [out, retval] */HSTRING * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_Label )(
        __x_TestApp_CParity_CIAttributedClass * This,
        /* [in] */HSTRING value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIAttributedClassVtbl;

interface __x_TestApp_CParity_CIAttributedClass
{
    CONST_VTBL struct __x_TestApp_CParity_CIAttributedClassVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIAttributedClass_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIAttributedClass_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIAttributedClass_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIAttributedClass_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIAttributedClass_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIAttributedClass_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIAttributedClass_get_Label(This,value) \
    ( (This)->lpVtbl->get_Label(This,value) )

#define __x_TestApp_CParity_CIAttributedClass_put_Label(This,value) \
    ( (This)->lpVtbl->put_Label(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIAttributedClass;
#endif /* !defined(____x_TestApp_CParity_CIAttributedClass_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.AttributedClass
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IAttributedClass ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_AttributedClass_DEFINED
#define RUNTIMECLASS_TestApp_Parity_AttributedClass_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_AttributedClass[] = L"TestApp.Parity.AttributedClass";
#endif




/*
 *
 * Interface TestApp.Parity.IOldClass
 *
 * Interface is a part of the implementation of type TestApp.Parity.OldClass
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IOldClass:
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIOldClass_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIOldClass_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IOldClass[] = L"TestApp.Parity.IOldClass";
/* [uuid("04733890-2db5-5f8a-9eb8-eab4590427a6"), deprecated, version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIOldClassVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIOldClass * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIOldClass * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIOldClass * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIOldClass * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIOldClass * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIOldClass * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
END_INTERFACE
    
} __x_TestApp_CParity_CIOldClassVtbl;

interface __x_TestApp_CParity_CIOldClass
{
    CONST_VTBL struct __x_TestApp_CParity_CIOldClassVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIOldClass_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIOldClass_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIOldClass_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIOldClass_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIOldClass_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIOldClass_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIOldClass;
#endif /* !defined(____x_TestApp_CParity_CIOldClass_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.OldClass
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IOldClass ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_OldClass_DEFINED
#define RUNTIMECLASS_TestApp_Parity_OldClass_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_OldClass[] = L"TestApp.Parity.OldClass";
#endif



/*
 *
 * Interface TestApp.Parity.IComposableBase
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableBase
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableBase:HRESULT get_Name(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableBase_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableBase_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableBase[] = L"TestApp.Parity.IComposableBase";
/* [uuid("eac9c74a-2061-5609-a7a7-0173cfdbfb5f"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIComposableBaseVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIComposableBase * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIComposableBase * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIComposableBase * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIComposableBase * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIComposableBase * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIComposableBase * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Name )(
        __x_TestApp_CParity_CIComposableBase * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIComposableBaseVtbl;

interface __x_TestApp_CParity_CIComposableBase
{
    CONST_VTBL struct __x_TestApp_CParity_CIComposableBaseVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIComposableBase_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIComposableBase_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIComposableBase_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIComposableBase_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIComposableBase_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIComposableBase_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIComposableBase_get_Name(This,value) \
    ( (This)->lpVtbl->get_Name(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIComposableBase;
#endif /* !defined(____x_TestApp_CParity_CIComposableBase_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IComposableBaseProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.ComposableBase
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IComposableBaseProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.ComposableBase**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.ComposableBase**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIComposableBaseProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIComposableBaseProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IComposableBaseProtectedFactory[] = L"TestApp.Parity.IComposableBaseProtectedFactory";
/* [uuid("2290ced5-f877-5826-beeb-acd89f91f9f1"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIComposableBaseProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIComposableBaseProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIComposableBaseProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIComposableBaseProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIComposableBaseProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIComposableBaseProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIComposableBaseProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIComposableBaseProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIComposableBase * * value
        );
    HRESULT ( STDMETHODCALLTYPE *CreateInstance2 )(
        __x_TestApp_CParity_CIComposableBaseProtectedFactory * This,
        /* [in] */HSTRING name,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIComposableBase * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIComposableBaseProtectedFactoryVtbl;

interface __x_TestApp_CParity_CIComposableBaseProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIComposableBaseProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )

#define __x_TestApp_CParity_CIComposableBaseProtectedFactory_CreateInstance2(This,name,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance2(This,name,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIComposableBaseProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIComposableBaseProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ComposableBase
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IComposableBase ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ComposableBase_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ComposableBase_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ComposableBase[] = L"TestApp.Parity.ComposableBase";
#endif



/*
 *
 * Interface TestApp.Parity.IActivatableWidget
 *
 * Interface is a part of the implementation of type TestApp.Parity.ActivatableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IActivatableWidget:HRESULT Activate();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIActivatableWidget_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIActivatableWidget_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IActivatableWidget[] = L"TestApp.Parity.IActivatableWidget";
/* [uuid("79db58e0-36b5-519b-9142-7811ef55264a"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIActivatableWidgetVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIActivatableWidget * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidget * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidget * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidget * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidget * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidget * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Activate )(
        __x_TestApp_CParity_CIActivatableWidget * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIActivatableWidgetVtbl;

interface __x_TestApp_CParity_CIActivatableWidget
{
    CONST_VTBL struct __x_TestApp_CParity_CIActivatableWidgetVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIActivatableWidget_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIActivatableWidget_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIActivatableWidget_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIActivatableWidget_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIActivatableWidget_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIActivatableWidget_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIActivatableWidget_Activate(This) \
    ( (This)->lpVtbl->Activate(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIActivatableWidget;
#endif /* !defined(____x_TestApp_CParity_CIActivatableWidget_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IActivatableWidgetFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.ActivatableWidget
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IActivatableWidgetFactory:HRESULT CreateInstance(String,TestApp.Parity.ActivatableWidget**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIActivatableWidgetFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIActivatableWidgetFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IActivatableWidgetFactory[] = L"TestApp.Parity.IActivatableWidgetFactory";
/* [uuid("83476af4-06b0-5173-8d12-92c238fdf65e"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIActivatableWidgetFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIActivatableWidgetFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidgetFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidgetFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidgetFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidgetFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIActivatableWidgetFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIActivatableWidgetFactory * This,
        /* [in] */HSTRING label,
        /* [retval, out] */__x_TestApp_CParity_CIActivatableWidget * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIActivatableWidgetFactoryVtbl;

interface __x_TestApp_CParity_CIActivatableWidgetFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIActivatableWidgetFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIActivatableWidgetFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIActivatableWidgetFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIActivatableWidgetFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIActivatableWidgetFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIActivatableWidgetFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIActivatableWidgetFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIActivatableWidgetFactory_CreateInstance(This,label,value) \
    ( (This)->lpVtbl->CreateInstance(This,label,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIActivatableWidgetFactory;
#endif /* !defined(____x_TestApp_CParity_CIActivatableWidgetFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.ActivatableWidget
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IActivatableWidget ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_ActivatableWidget_DEFINED
#define RUNTIMECLASS_TestApp_Parity_ActivatableWidget_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_ActivatableWidget[] = L"TestApp.Parity.ActivatableWidget";
#endif



/*
 *
 * Interface TestApp.Parity.ISettingsHelperStatics
 *
 * Interface is a part of the implementation of type TestApp.Parity.SettingsHelper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ISettingsHelperStatics:HRESULT Read(String,String*);HRESULT Write(String,String);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CISettingsHelperStatics_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CISettingsHelperStatics_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ISettingsHelperStatics[] = L"TestApp.Parity.ISettingsHelperStatics";
/* [uuid("70248eac-4fd4-5e34-8ea4-6087ddc9fb06"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CISettingsHelperStaticsVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CISettingsHelperStatics * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CISettingsHelperStatics * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CISettingsHelperStatics * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CISettingsHelperStatics * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CISettingsHelperStatics * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CISettingsHelperStatics * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Read )(
        __x_TestApp_CParity_CISettingsHelperStatics * This,
        /* [in] */HSTRING key,
        /* [out, retval] */HSTRING * result
        );
    HRESULT ( STDMETHODCALLTYPE *Write )(
        __x_TestApp_CParity_CISettingsHelperStatics * This,
        /* [in] */HSTRING key,
        /* [in] */HSTRING data
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CISettingsHelperStaticsVtbl;

interface __x_TestApp_CParity_CISettingsHelperStatics
{
    CONST_VTBL struct __x_TestApp_CParity_CISettingsHelperStaticsVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CISettingsHelperStatics_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CISettingsHelperStatics_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CISettingsHelperStatics_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CISettingsHelperStatics_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CISettingsHelperStatics_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CISettingsHelperStatics_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CISettingsHelperStatics_Read(This,key,result) \
    ( (This)->lpVtbl->Read(This,key,result) )

#define __x_TestApp_CParity_CISettingsHelperStatics_Write(This,key,data) \
    ( (This)->lpVtbl->Write(This,key,data) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CISettingsHelperStatics;
#endif /* !defined(____x_TestApp_CParity_CISettingsHelperStatics_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.SettingsHelper
 *
 * RuntimeClass contains static methods.
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */
#ifndef RUNTIMECLASS_TestApp_Parity_SettingsHelper_DEFINED
#define RUNTIMECLASS_TestApp_Parity_SettingsHelper_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_SettingsHelper[] = L"TestApp.Parity.SettingsHelper";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_attributes_p_h__

#endif // __parity_attributes_h__
