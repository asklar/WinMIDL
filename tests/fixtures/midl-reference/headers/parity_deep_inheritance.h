/* Header file automatically generated from parity_deep_inheritance.idl */
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
#ifndef __parity_deep_inheritance_h__
#define __parity_deep_inheritance_h__
#ifndef __parity_deep_inheritance_p_h__
#define __parity_deep_inheritance_p_h__


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
#ifndef ____x_TestApp_CParity_CILevel0Base_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel0Base_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel0Base;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel0Base TestApp::Parity::ILevel0Base

#endif // ____x_TestApp_CParity_CILevel0Base_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel0BaseOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel0BaseOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel0BaseOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel0BaseOverrides TestApp::Parity::ILevel0BaseOverrides

#endif // ____x_TestApp_CParity_CILevel0BaseOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel0BaseProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel0BaseProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel0BaseProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel0BaseProtectedFactory TestApp::Parity::ILevel0BaseProtectedFactory

#endif // ____x_TestApp_CParity_CILevel0BaseProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel1Mid_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel1Mid_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel1Mid;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel1Mid TestApp::Parity::ILevel1Mid

#endif // ____x_TestApp_CParity_CILevel1Mid_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel1MidOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel1MidOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel1MidOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel1MidOverrides TestApp::Parity::ILevel1MidOverrides

#endif // ____x_TestApp_CParity_CILevel1MidOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel1MidProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel1MidProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel1MidProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel1MidProtectedFactory TestApp::Parity::ILevel1MidProtectedFactory

#endif // ____x_TestApp_CParity_CILevel1MidProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel2Upper_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel2Upper_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel2Upper;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel2Upper TestApp::Parity::ILevel2Upper

#endif // ____x_TestApp_CParity_CILevel2Upper_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel2UpperOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel2UpperOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel2UpperOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel2UpperOverrides TestApp::Parity::ILevel2UpperOverrides

#endif // ____x_TestApp_CParity_CILevel2UpperOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel2UpperProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel2UpperProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel2UpperProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel2UpperProtectedFactory TestApp::Parity::ILevel2UpperProtectedFactory

#endif // ____x_TestApp_CParity_CILevel2UpperProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel3Leaf_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel3Leaf_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel3Leaf;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel3Leaf TestApp::Parity::ILevel3Leaf

#endif // ____x_TestApp_CParity_CILevel3Leaf_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel3LeafFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel3LeafFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface ILevel3LeafFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CILevel3LeafFactory TestApp::Parity::ILevel3LeafFactory

#endif // ____x_TestApp_CParity_CILevel3LeafFactory_FWD_DEFINED__



namespace TestApp {
    namespace Parity {
        class Level0Base;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.ILevel0Base
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level0Base
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel0Base:HRESULT get_BaseLabel(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel0Base_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel0Base_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel0Base[] = L"TestApp.Parity.ILevel0Base";

namespace TestApp {
    namespace Parity {
        /* [uuid("91015105-6111-515b-8dde-6ce0eed2cd06"), version, object, exclusiveto] */
        MIDL_INTERFACE("91015105-6111-515b-8dde-6ce0eed2cd06")
        ILevel0Base : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_BaseLabel(
                /* [out, retval] */HSTRING * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel0Base=__uuidof(ILevel0Base);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel0Base;
#endif /* !defined(____x_TestApp_CParity_CILevel0Base_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel0BaseOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level0Base
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel0BaseOverrides:HRESULT OnInit();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel0BaseOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel0BaseOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel0BaseOverrides[] = L"TestApp.Parity.ILevel0BaseOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("8e91a0cb-548c-5262-9996-0fbf0cc268e2"), version, object, exclusiveto] */
        MIDL_INTERFACE("8e91a0cb-548c-5262-9996-0fbf0cc268e2")
        ILevel0BaseOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE OnInit(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel0BaseOverrides=__uuidof(ILevel0BaseOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel0BaseOverrides;
#endif /* !defined(____x_TestApp_CParity_CILevel0BaseOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel0BaseProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level0Base
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel0BaseProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Level0Base**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel0BaseProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel0BaseProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel0BaseProtectedFactory[] = L"TestApp.Parity.ILevel0BaseProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("21d02669-8427-55f8-bad9-bf6369b57521"), version, object, exclusiveto] */
        MIDL_INTERFACE("21d02669-8427-55f8-bad9-bf6369b57521")
        ILevel0BaseProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::ILevel0Base * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel0BaseProtectedFactory=__uuidof(ILevel0BaseProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel0BaseProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel0BaseProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level0Base
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel0Base ** Default Interface **
 *    TestApp.Parity.ILevel0BaseOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level0Base_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level0Base_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level0Base[] = L"TestApp.Parity.Level0Base";
#endif


namespace TestApp {
    namespace Parity {
        class Level1Mid;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.ILevel1Mid
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level1Mid
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel1Mid:HRESULT get_MidValue(Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel1Mid_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel1Mid_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel1Mid[] = L"TestApp.Parity.ILevel1Mid";

namespace TestApp {
    namespace Parity {
        /* [uuid("c37e8cbc-7017-5559-88c4-e9b0e60d78ba"), version, object, exclusiveto] */
        MIDL_INTERFACE("c37e8cbc-7017-5559-88c4-e9b0e60d78ba")
        ILevel1Mid : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_MidValue(
                /* [out, retval] */int * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel1Mid=__uuidof(ILevel1Mid);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel1Mid;
#endif /* !defined(____x_TestApp_CParity_CILevel1Mid_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel1MidOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level1Mid
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel1MidOverrides:HRESULT OnProcess();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel1MidOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel1MidOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel1MidOverrides[] = L"TestApp.Parity.ILevel1MidOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("c113685d-4885-57e8-a4c7-abb5bb9a526b"), version, object, exclusiveto] */
        MIDL_INTERFACE("c113685d-4885-57e8-a4c7-abb5bb9a526b")
        ILevel1MidOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE OnProcess(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel1MidOverrides=__uuidof(ILevel1MidOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel1MidOverrides;
#endif /* !defined(____x_TestApp_CParity_CILevel1MidOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel1MidProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level1Mid
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel1MidProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Level1Mid**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel1MidProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel1MidProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel1MidProtectedFactory[] = L"TestApp.Parity.ILevel1MidProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("880062fa-51d3-52c0-b1f2-ed4ff3720950"), version, object, exclusiveto] */
        MIDL_INTERFACE("880062fa-51d3-52c0-b1f2-ed4ff3720950")
        ILevel1MidProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::ILevel1Mid * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel1MidProtectedFactory=__uuidof(ILevel1MidProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel1MidProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel1MidProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level1Mid
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel1Mid ** Default Interface **
 *    TestApp.Parity.ILevel1MidOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level1Mid_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level1Mid_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level1Mid[] = L"TestApp.Parity.Level1Mid";
#endif


namespace TestApp {
    namespace Parity {
        class Level2Upper;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.ILevel2Upper
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level2Upper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel2Upper:HRESULT get_UpperFlag(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel2Upper_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel2Upper_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel2Upper[] = L"TestApp.Parity.ILevel2Upper";

namespace TestApp {
    namespace Parity {
        /* [uuid("69bdf725-88f9-5fa9-8531-9e6c89f7428f"), version, object, exclusiveto] */
        MIDL_INTERFACE("69bdf725-88f9-5fa9-8531-9e6c89f7428f")
        ILevel2Upper : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_UpperFlag(
                /* [out, retval] */::boolean * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel2Upper=__uuidof(ILevel2Upper);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel2Upper;
#endif /* !defined(____x_TestApp_CParity_CILevel2Upper_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel2UpperOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level2Upper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel2UpperOverrides:HRESULT OnComplete();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel2UpperOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel2UpperOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel2UpperOverrides[] = L"TestApp.Parity.ILevel2UpperOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("ed1f480f-38c6-5054-bc5a-45f2fca407b8"), version, object, exclusiveto] */
        MIDL_INTERFACE("ed1f480f-38c6-5054-bc5a-45f2fca407b8")
        ILevel2UpperOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE OnComplete(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel2UpperOverrides=__uuidof(ILevel2UpperOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel2UpperOverrides;
#endif /* !defined(____x_TestApp_CParity_CILevel2UpperOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel2UpperProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level2Upper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel2UpperProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Level2Upper**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel2UpperProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel2UpperProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel2UpperProtectedFactory[] = L"TestApp.Parity.ILevel2UpperProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("768c8abe-0d50-5fe9-8fde-6924c6d11c59"), version, object, exclusiveto] */
        MIDL_INTERFACE("768c8abe-0d50-5fe9-8fde-6924c6d11c59")
        ILevel2UpperProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::ILevel2Upper * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel2UpperProtectedFactory=__uuidof(ILevel2UpperProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel2UpperProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel2UpperProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level2Upper
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel2Upper ** Default Interface **
 *    TestApp.Parity.ILevel2UpperOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level2Upper_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level2Upper_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level2Upper[] = L"TestApp.Parity.Level2Upper";
#endif


namespace TestApp {
    namespace Parity {
        class Level3Leaf;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.ILevel3Leaf
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level3Leaf
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel3Leaf:HRESULT get_LeafMetric(Double*);HRESULT Execute();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel3Leaf_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel3Leaf_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel3Leaf[] = L"TestApp.Parity.ILevel3Leaf";

namespace TestApp {
    namespace Parity {
        /* [uuid("70d1617e-84d5-5b96-a29b-c0fc8ee07c14"), version, object, exclusiveto] */
        MIDL_INTERFACE("70d1617e-84d5-5b96-a29b-c0fc8ee07c14")
        ILevel3Leaf : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_LeafMetric(
                /* [out, retval] */double * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Execute(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel3Leaf=__uuidof(ILevel3Leaf);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel3Leaf;
#endif /* !defined(____x_TestApp_CParity_CILevel3Leaf_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel3LeafFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level3Leaf
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel3LeafFactory:HRESULT CreateInstance(String,TestApp.Parity.Level3Leaf**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel3LeafFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel3LeafFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel3LeafFactory[] = L"TestApp.Parity.ILevel3LeafFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("c35f4d58-9123-579c-b3b2-992a9c5f5d5f"), version, object, exclusiveto] */
        MIDL_INTERFACE("c35f4d58-9123-579c-b3b2-992a9c5f5d5f")
        ILevel3LeafFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                /* [in] */HSTRING name,
                /* [retval, out] */TestApp::Parity::ILevel3Leaf * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_ILevel3LeafFactory=__uuidof(ILevel3LeafFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CILevel3LeafFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel3LeafFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level3Leaf
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel3Leaf ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level3Leaf_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level3Leaf_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level3Leaf[] = L"TestApp.Parity.Level3Leaf";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CILevel0Base_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel0Base_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel0Base __x_TestApp_CParity_CILevel0Base;

#endif // ____x_TestApp_CParity_CILevel0Base_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel0BaseOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel0BaseOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel0BaseOverrides __x_TestApp_CParity_CILevel0BaseOverrides;

#endif // ____x_TestApp_CParity_CILevel0BaseOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel0BaseProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel0BaseProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel0BaseProtectedFactory __x_TestApp_CParity_CILevel0BaseProtectedFactory;

#endif // ____x_TestApp_CParity_CILevel0BaseProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel1Mid_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel1Mid_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel1Mid __x_TestApp_CParity_CILevel1Mid;

#endif // ____x_TestApp_CParity_CILevel1Mid_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel1MidOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel1MidOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel1MidOverrides __x_TestApp_CParity_CILevel1MidOverrides;

#endif // ____x_TestApp_CParity_CILevel1MidOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel1MidProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel1MidProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel1MidProtectedFactory __x_TestApp_CParity_CILevel1MidProtectedFactory;

#endif // ____x_TestApp_CParity_CILevel1MidProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel2Upper_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel2Upper_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel2Upper __x_TestApp_CParity_CILevel2Upper;

#endif // ____x_TestApp_CParity_CILevel2Upper_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel2UpperOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel2UpperOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel2UpperOverrides __x_TestApp_CParity_CILevel2UpperOverrides;

#endif // ____x_TestApp_CParity_CILevel2UpperOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel2UpperProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel2UpperProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel2UpperProtectedFactory __x_TestApp_CParity_CILevel2UpperProtectedFactory;

#endif // ____x_TestApp_CParity_CILevel2UpperProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel3Leaf_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel3Leaf_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel3Leaf __x_TestApp_CParity_CILevel3Leaf;

#endif // ____x_TestApp_CParity_CILevel3Leaf_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CILevel3LeafFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CILevel3LeafFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CILevel3LeafFactory __x_TestApp_CParity_CILevel3LeafFactory;

#endif // ____x_TestApp_CParity_CILevel3LeafFactory_FWD_DEFINED__



/*
 *
 * Interface TestApp.Parity.ILevel0Base
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level0Base
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel0Base:HRESULT get_BaseLabel(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel0Base_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel0Base_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel0Base[] = L"TestApp.Parity.ILevel0Base";
/* [uuid("91015105-6111-515b-8dde-6ce0eed2cd06"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel0BaseVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel0Base * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel0Base * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel0Base * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel0Base * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel0Base * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel0Base * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_BaseLabel )(
        __x_TestApp_CParity_CILevel0Base * This,
        /* [out, retval] */HSTRING * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel0BaseVtbl;

interface __x_TestApp_CParity_CILevel0Base
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel0BaseVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel0Base_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel0Base_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel0Base_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel0Base_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel0Base_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel0Base_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel0Base_get_BaseLabel(This,value) \
    ( (This)->lpVtbl->get_BaseLabel(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel0Base;
#endif /* !defined(____x_TestApp_CParity_CILevel0Base_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel0BaseOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level0Base
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel0BaseOverrides:HRESULT OnInit();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel0BaseOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel0BaseOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel0BaseOverrides[] = L"TestApp.Parity.ILevel0BaseOverrides";
/* [uuid("8e91a0cb-548c-5262-9996-0fbf0cc268e2"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel0BaseOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel0BaseOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *OnInit )(
        __x_TestApp_CParity_CILevel0BaseOverrides * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel0BaseOverridesVtbl;

interface __x_TestApp_CParity_CILevel0BaseOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel0BaseOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel0BaseOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel0BaseOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel0BaseOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel0BaseOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel0BaseOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel0BaseOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel0BaseOverrides_OnInit(This) \
    ( (This)->lpVtbl->OnInit(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel0BaseOverrides;
#endif /* !defined(____x_TestApp_CParity_CILevel0BaseOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel0BaseProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level0Base
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel0BaseProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Level0Base**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel0BaseProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel0BaseProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel0BaseProtectedFactory[] = L"TestApp.Parity.ILevel0BaseProtectedFactory";
/* [uuid("21d02669-8427-55f8-bad9-bf6369b57521"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel0BaseProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel0BaseProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel0BaseProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CILevel0BaseProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CILevel0Base * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel0BaseProtectedFactoryVtbl;

interface __x_TestApp_CParity_CILevel0BaseProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel0BaseProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel0BaseProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel0BaseProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel0BaseProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level0Base
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel0Base ** Default Interface **
 *    TestApp.Parity.ILevel0BaseOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level0Base_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level0Base_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level0Base[] = L"TestApp.Parity.Level0Base";
#endif



/*
 *
 * Interface TestApp.Parity.ILevel1Mid
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level1Mid
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel1Mid:HRESULT get_MidValue(Int32*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel1Mid_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel1Mid_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel1Mid[] = L"TestApp.Parity.ILevel1Mid";
/* [uuid("c37e8cbc-7017-5559-88c4-e9b0e60d78ba"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel1MidVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel1Mid * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel1Mid * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel1Mid * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel1Mid * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel1Mid * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel1Mid * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_MidValue )(
        __x_TestApp_CParity_CILevel1Mid * This,
        /* [out, retval] */int * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel1MidVtbl;

interface __x_TestApp_CParity_CILevel1Mid
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel1MidVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel1Mid_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel1Mid_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel1Mid_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel1Mid_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel1Mid_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel1Mid_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel1Mid_get_MidValue(This,value) \
    ( (This)->lpVtbl->get_MidValue(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel1Mid;
#endif /* !defined(____x_TestApp_CParity_CILevel1Mid_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel1MidOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level1Mid
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel1MidOverrides:HRESULT OnProcess();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel1MidOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel1MidOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel1MidOverrides[] = L"TestApp.Parity.ILevel1MidOverrides";
/* [uuid("c113685d-4885-57e8-a4c7-abb5bb9a526b"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel1MidOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel1MidOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel1MidOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel1MidOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel1MidOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel1MidOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel1MidOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *OnProcess )(
        __x_TestApp_CParity_CILevel1MidOverrides * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel1MidOverridesVtbl;

interface __x_TestApp_CParity_CILevel1MidOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel1MidOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel1MidOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel1MidOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel1MidOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel1MidOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel1MidOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel1MidOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel1MidOverrides_OnProcess(This) \
    ( (This)->lpVtbl->OnProcess(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel1MidOverrides;
#endif /* !defined(____x_TestApp_CParity_CILevel1MidOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel1MidProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level1Mid
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel1MidProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Level1Mid**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel1MidProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel1MidProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel1MidProtectedFactory[] = L"TestApp.Parity.ILevel1MidProtectedFactory";
/* [uuid("880062fa-51d3-52c0-b1f2-ed4ff3720950"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel1MidProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel1MidProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel1MidProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel1MidProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel1MidProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel1MidProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel1MidProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CILevel1MidProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CILevel1Mid * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel1MidProtectedFactoryVtbl;

interface __x_TestApp_CParity_CILevel1MidProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel1MidProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel1MidProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel1MidProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel1MidProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel1MidProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel1MidProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel1MidProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel1MidProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel1MidProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel1MidProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level1Mid
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel1Mid ** Default Interface **
 *    TestApp.Parity.ILevel1MidOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level1Mid_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level1Mid_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level1Mid[] = L"TestApp.Parity.Level1Mid";
#endif



/*
 *
 * Interface TestApp.Parity.ILevel2Upper
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level2Upper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel2Upper:HRESULT get_UpperFlag(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel2Upper_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel2Upper_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel2Upper[] = L"TestApp.Parity.ILevel2Upper";
/* [uuid("69bdf725-88f9-5fa9-8531-9e6c89f7428f"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel2UpperVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel2Upper * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel2Upper * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel2Upper * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel2Upper * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel2Upper * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel2Upper * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_UpperFlag )(
        __x_TestApp_CParity_CILevel2Upper * This,
        /* [out, retval] */boolean * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel2UpperVtbl;

interface __x_TestApp_CParity_CILevel2Upper
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel2UpperVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel2Upper_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel2Upper_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel2Upper_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel2Upper_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel2Upper_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel2Upper_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel2Upper_get_UpperFlag(This,value) \
    ( (This)->lpVtbl->get_UpperFlag(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel2Upper;
#endif /* !defined(____x_TestApp_CParity_CILevel2Upper_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel2UpperOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level2Upper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel2UpperOverrides:HRESULT OnComplete();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel2UpperOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel2UpperOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel2UpperOverrides[] = L"TestApp.Parity.ILevel2UpperOverrides";
/* [uuid("ed1f480f-38c6-5054-bc5a-45f2fca407b8"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel2UpperOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel2UpperOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *OnComplete )(
        __x_TestApp_CParity_CILevel2UpperOverrides * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel2UpperOverridesVtbl;

interface __x_TestApp_CParity_CILevel2UpperOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel2UpperOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel2UpperOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel2UpperOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel2UpperOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel2UpperOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel2UpperOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel2UpperOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel2UpperOverrides_OnComplete(This) \
    ( (This)->lpVtbl->OnComplete(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel2UpperOverrides;
#endif /* !defined(____x_TestApp_CParity_CILevel2UpperOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel2UpperProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level2Upper
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel2UpperProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Level2Upper**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel2UpperProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel2UpperProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel2UpperProtectedFactory[] = L"TestApp.Parity.ILevel2UpperProtectedFactory";
/* [uuid("768c8abe-0d50-5fe9-8fde-6924c6d11c59"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel2UpperProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel2UpperProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel2UpperProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CILevel2UpperProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CILevel2Upper * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel2UpperProtectedFactoryVtbl;

interface __x_TestApp_CParity_CILevel2UpperProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel2UpperProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel2UpperProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel2UpperProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel2UpperProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level2Upper
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel2Upper ** Default Interface **
 *    TestApp.Parity.ILevel2UpperOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level2Upper_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level2Upper_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level2Upper[] = L"TestApp.Parity.Level2Upper";
#endif



/*
 *
 * Interface TestApp.Parity.ILevel3Leaf
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level3Leaf
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel3Leaf:HRESULT get_LeafMetric(Double*);HRESULT Execute();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel3Leaf_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel3Leaf_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel3Leaf[] = L"TestApp.Parity.ILevel3Leaf";
/* [uuid("70d1617e-84d5-5b96-a29b-c0fc8ee07c14"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel3LeafVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel3Leaf * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel3Leaf * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel3Leaf * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel3Leaf * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel3Leaf * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel3Leaf * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_LeafMetric )(
        __x_TestApp_CParity_CILevel3Leaf * This,
        /* [out, retval] */double * value
        );
    HRESULT ( STDMETHODCALLTYPE *Execute )(
        __x_TestApp_CParity_CILevel3Leaf * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel3LeafVtbl;

interface __x_TestApp_CParity_CILevel3Leaf
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel3LeafVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel3Leaf_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel3Leaf_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel3Leaf_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel3Leaf_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel3Leaf_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel3Leaf_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel3Leaf_get_LeafMetric(This,value) \
    ( (This)->lpVtbl->get_LeafMetric(This,value) )

#define __x_TestApp_CParity_CILevel3Leaf_Execute(This) \
    ( (This)->lpVtbl->Execute(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel3Leaf;
#endif /* !defined(____x_TestApp_CParity_CILevel3Leaf_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.ILevel3LeafFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Level3Leaf
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.ILevel3LeafFactory:HRESULT CreateInstance(String,TestApp.Parity.Level3Leaf**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CILevel3LeafFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CILevel3LeafFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_ILevel3LeafFactory[] = L"TestApp.Parity.ILevel3LeafFactory";
/* [uuid("c35f4d58-9123-579c-b3b2-992a9c5f5d5f"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CILevel3LeafFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CILevel3LeafFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CILevel3LeafFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CILevel3LeafFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CILevel3LeafFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CILevel3LeafFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CILevel3LeafFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CILevel3LeafFactory * This,
        /* [in] */HSTRING name,
        /* [retval, out] */__x_TestApp_CParity_CILevel3Leaf * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CILevel3LeafFactoryVtbl;

interface __x_TestApp_CParity_CILevel3LeafFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CILevel3LeafFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CILevel3LeafFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CILevel3LeafFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CILevel3LeafFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CILevel3LeafFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CILevel3LeafFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CILevel3LeafFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CILevel3LeafFactory_CreateInstance(This,name,value) \
    ( (This)->lpVtbl->CreateInstance(This,name,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CILevel3LeafFactory;
#endif /* !defined(____x_TestApp_CParity_CILevel3LeafFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Level3Leaf
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.ILevel3Leaf ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Level3Leaf_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Level3Leaf_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Level3Leaf[] = L"TestApp.Parity.Level3Leaf";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_deep_inheritance_p_h__

#endif // __parity_deep_inheritance_h__
