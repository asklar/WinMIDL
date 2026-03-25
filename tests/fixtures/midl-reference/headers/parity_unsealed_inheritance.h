/* Header file automatically generated from parity_unsealed_inheritance.idl */
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
#ifndef __parity_unsealed_inheritance_h__
#define __parity_unsealed_inheritance_h__
#ifndef __parity_unsealed_inheritance_p_h__
#define __parity_unsealed_inheritance_p_h__


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
#ifndef ____x_TestApp_CParity_CIAnimal_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAnimal_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IAnimal;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIAnimal TestApp::Parity::IAnimal

#endif // ____x_TestApp_CParity_CIAnimal_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIAnimalOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAnimalOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IAnimalOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIAnimalOverrides TestApp::Parity::IAnimalOverrides

#endif // ____x_TestApp_CParity_CIAnimalOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIAnimalProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAnimalProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IAnimalProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIAnimalProtectedFactory TestApp::Parity::IAnimalProtectedFactory

#endif // ____x_TestApp_CParity_CIAnimalProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMammal_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMammal_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMammal;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMammal TestApp::Parity::IMammal

#endif // ____x_TestApp_CParity_CIMammal_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMammalOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMammalOverrides_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMammalOverrides;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMammalOverrides TestApp::Parity::IMammalOverrides

#endif // ____x_TestApp_CParity_CIMammalOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMammalProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMammalProtectedFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IMammalProtectedFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIMammalProtectedFactory TestApp::Parity::IMammalProtectedFactory

#endif // ____x_TestApp_CParity_CIMammalProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDog_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDog_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDog;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDog TestApp::Parity::IDog

#endif // ____x_TestApp_CParity_CIDog_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDogFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDogFactory_FWD_DEFINED__

namespace TestApp {
    namespace Parity {
        interface IDogFactory;
    } /* Parity */
} /* TestApp */
#define __x_TestApp_CParity_CIDogFactory TestApp::Parity::IDogFactory

#endif // ____x_TestApp_CParity_CIDogFactory_FWD_DEFINED__



namespace TestApp {
    namespace Parity {
        class Animal;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IAnimal
 *
 * Interface is a part of the implementation of type TestApp.Parity.Animal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAnimal:HRESULT get_Name(String*);HRESULT get_Age(Int32*);HRESULT put_Age(Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAnimal_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAnimal_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAnimal[] = L"TestApp.Parity.IAnimal";

namespace TestApp {
    namespace Parity {
        /* [uuid("86175b7a-2f34-5707-a0aa-bc4db787484b"), version, object, exclusiveto] */
        MIDL_INTERFACE("86175b7a-2f34-5707-a0aa-bc4db787484b")
        IAnimal : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Name(
                /* [out, retval] */HSTRING * value
                ) = 0;
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Age(
                /* [out, retval] */int * value
                ) = 0;
            /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_Age(
                /* [in] */int value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IAnimal=__uuidof(IAnimal);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIAnimal;
#endif /* !defined(____x_TestApp_CParity_CIAnimal_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IAnimalOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Animal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAnimalOverrides:HRESULT MakeSound();HRESULT Describe(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAnimalOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAnimalOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAnimalOverrides[] = L"TestApp.Parity.IAnimalOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("e2d91a62-212b-57b5-a701-01c1740e21c8"), version, object, exclusiveto] */
        MIDL_INTERFACE("e2d91a62-212b-57b5-a701-01c1740e21c8")
        IAnimalOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE MakeSound(void) = 0;
            virtual HRESULT STDMETHODCALLTYPE Describe(
                /* [out, retval] */HSTRING * result
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IAnimalOverrides=__uuidof(IAnimalOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIAnimalOverrides;
#endif /* !defined(____x_TestApp_CParity_CIAnimalOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IAnimalProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Animal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAnimalProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Animal**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.Animal**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAnimalProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAnimalProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAnimalProtectedFactory[] = L"TestApp.Parity.IAnimalProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("1f0270d9-3405-5946-b829-1a785ae42ef2"), version, object, exclusiveto] */
        MIDL_INTERFACE("1f0270d9-3405-5946-b829-1a785ae42ef2")
        IAnimalProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IAnimal * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CreateInstance2(
                /* [in] */HSTRING name,
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IAnimal * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IAnimalProtectedFactory=__uuidof(IAnimalProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIAnimalProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIAnimalProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Animal
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IAnimal ** Default Interface **
 *    TestApp.Parity.IAnimalOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Animal_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Animal_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Animal[] = L"TestApp.Parity.Animal";
#endif


namespace TestApp {
    namespace Parity {
        class Mammal;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IMammal
 *
 * Interface is a part of the implementation of type TestApp.Parity.Mammal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMammal:HRESULT get_IsWarmBlooded(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMammal_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMammal_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMammal[] = L"TestApp.Parity.IMammal";

namespace TestApp {
    namespace Parity {
        /* [uuid("49e3acb2-c737-5111-a70e-ffe7fd034274"), version, object, exclusiveto] */
        MIDL_INTERFACE("49e3acb2-c737-5111-a70e-ffe7fd034274")
        IMammal : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_IsWarmBlooded(
                /* [out, retval] */::boolean * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMammal=__uuidof(IMammal);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMammal;
#endif /* !defined(____x_TestApp_CParity_CIMammal_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMammalOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Mammal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMammalOverrides:HRESULT Breathe();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMammalOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMammalOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMammalOverrides[] = L"TestApp.Parity.IMammalOverrides";

namespace TestApp {
    namespace Parity {
        /* [uuid("e6532ac5-3f9e-56ad-a9dc-6ce5c57be856"), version, object, exclusiveto] */
        MIDL_INTERFACE("e6532ac5-3f9e-56ad-a9dc-6ce5c57be856")
        IMammalOverrides : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE Breathe(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMammalOverrides=__uuidof(IMammalOverrides);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMammalOverrides;
#endif /* !defined(____x_TestApp_CParity_CIMammalOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMammalProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Mammal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMammalProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Mammal**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.Mammal**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMammalProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMammalProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMammalProtectedFactory[] = L"TestApp.Parity.IMammalProtectedFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("53c7a9b7-d78f-55a5-8051-23ef088fdafb"), version, object, exclusiveto] */
        MIDL_INTERFACE("53c7a9b7-d78f-55a5-8051-23ef088fdafb")
        IMammalProtectedFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IMammal * * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE CreateInstance2(
                /* [in] */HSTRING name,
                IInspectable * baseInterface,
                /* [out] */IInspectable * * innerInterface,
                /* [retval, out] */TestApp::Parity::IMammal * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IMammalProtectedFactory=__uuidof(IMammalProtectedFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIMammalProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIMammalProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Mammal
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMammal ** Default Interface **
 *    TestApp.Parity.IMammalOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Mammal_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Mammal_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Mammal[] = L"TestApp.Parity.Mammal";
#endif


namespace TestApp {
    namespace Parity {
        class Dog;
    } /* Parity */
} /* TestApp */



/*
 *
 * Interface TestApp.Parity.IDog
 *
 * Interface is a part of the implementation of type TestApp.Parity.Dog
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDog:HRESULT get_Breed(String*);HRESULT Fetch();HRESULT Sit();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDog_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDog_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDog[] = L"TestApp.Parity.IDog";

namespace TestApp {
    namespace Parity {
        /* [uuid("1eaff1c2-66e4-5d2f-8f88-4624023d88a6"), version, object, exclusiveto] */
        MIDL_INTERFACE("1eaff1c2-66e4-5d2f-8f88-4624023d88a6")
        IDog : public IInspectable
        {
        public:
            /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Breed(
                /* [out, retval] */HSTRING * value
                ) = 0;
            virtual HRESULT STDMETHODCALLTYPE Fetch(void) = 0;
            virtual HRESULT STDMETHODCALLTYPE Sit(void) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDog=__uuidof(IDog);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDog;
#endif /* !defined(____x_TestApp_CParity_CIDog_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IDogFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Dog
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDogFactory:HRESULT CreateInstance(String,TestApp.Parity.Dog**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDogFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDogFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDogFactory[] = L"TestApp.Parity.IDogFactory";

namespace TestApp {
    namespace Parity {
        /* [uuid("14bea965-06c9-582e-b0e3-c0bfcc6cc911"), version, object, exclusiveto] */
        MIDL_INTERFACE("14bea965-06c9-582e-b0e3-c0bfcc6cc911")
        IDogFactory : public IInspectable
        {
        public:
            virtual HRESULT STDMETHODCALLTYPE CreateInstance(
                /* [in] */HSTRING name,
                /* [retval, out] */TestApp::Parity::IDog * * value
                ) = 0;
            
        };

        MIDL_CONST_ID IID & IID_IDogFactory=__uuidof(IDogFactory);
        
    } /* Parity */
} /* TestApp */

EXTERN_C const IID IID___x_TestApp_CParity_CIDogFactory;
#endif /* !defined(____x_TestApp_CParity_CIDogFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Dog
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDog ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Dog_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Dog_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Dog[] = L"TestApp.Parity.Dog";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_TestApp_CParity_CIAnimal_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAnimal_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIAnimal __x_TestApp_CParity_CIAnimal;

#endif // ____x_TestApp_CParity_CIAnimal_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIAnimalOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAnimalOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIAnimalOverrides __x_TestApp_CParity_CIAnimalOverrides;

#endif // ____x_TestApp_CParity_CIAnimalOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIAnimalProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIAnimalProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIAnimalProtectedFactory __x_TestApp_CParity_CIAnimalProtectedFactory;

#endif // ____x_TestApp_CParity_CIAnimalProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMammal_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMammal_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMammal __x_TestApp_CParity_CIMammal;

#endif // ____x_TestApp_CParity_CIMammal_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMammalOverrides_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMammalOverrides_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMammalOverrides __x_TestApp_CParity_CIMammalOverrides;

#endif // ____x_TestApp_CParity_CIMammalOverrides_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIMammalProtectedFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIMammalProtectedFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIMammalProtectedFactory __x_TestApp_CParity_CIMammalProtectedFactory;

#endif // ____x_TestApp_CParity_CIMammalProtectedFactory_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDog_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDog_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDog __x_TestApp_CParity_CIDog;

#endif // ____x_TestApp_CParity_CIDog_FWD_DEFINED__

#ifndef ____x_TestApp_CParity_CIDogFactory_FWD_DEFINED__
#define ____x_TestApp_CParity_CIDogFactory_FWD_DEFINED__
typedef interface __x_TestApp_CParity_CIDogFactory __x_TestApp_CParity_CIDogFactory;

#endif // ____x_TestApp_CParity_CIDogFactory_FWD_DEFINED__



/*
 *
 * Interface TestApp.Parity.IAnimal
 *
 * Interface is a part of the implementation of type TestApp.Parity.Animal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAnimal:HRESULT get_Name(String*);HRESULT get_Age(Int32*);HRESULT put_Age(Int32);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAnimal_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAnimal_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAnimal[] = L"TestApp.Parity.IAnimal";
/* [uuid("86175b7a-2f34-5707-a0aa-bc4db787484b"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIAnimalVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIAnimal * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIAnimal * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIAnimal * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIAnimal * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIAnimal * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIAnimal * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Name )(
        __x_TestApp_CParity_CIAnimal * This,
        /* [out, retval] */HSTRING * value
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Age )(
        __x_TestApp_CParity_CIAnimal * This,
        /* [out, retval] */int * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_Age )(
        __x_TestApp_CParity_CIAnimal * This,
        /* [in] */int value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIAnimalVtbl;

interface __x_TestApp_CParity_CIAnimal
{
    CONST_VTBL struct __x_TestApp_CParity_CIAnimalVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIAnimal_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIAnimal_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIAnimal_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIAnimal_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIAnimal_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIAnimal_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIAnimal_get_Name(This,value) \
    ( (This)->lpVtbl->get_Name(This,value) )

#define __x_TestApp_CParity_CIAnimal_get_Age(This,value) \
    ( (This)->lpVtbl->get_Age(This,value) )

#define __x_TestApp_CParity_CIAnimal_put_Age(This,value) \
    ( (This)->lpVtbl->put_Age(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIAnimal;
#endif /* !defined(____x_TestApp_CParity_CIAnimal_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IAnimalOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Animal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAnimalOverrides:HRESULT MakeSound();HRESULT Describe(String*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAnimalOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAnimalOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAnimalOverrides[] = L"TestApp.Parity.IAnimalOverrides";
/* [uuid("e2d91a62-212b-57b5-a701-01c1740e21c8"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIAnimalOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIAnimalOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIAnimalOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIAnimalOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIAnimalOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIAnimalOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIAnimalOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *MakeSound )(
        __x_TestApp_CParity_CIAnimalOverrides * This
        );
    HRESULT ( STDMETHODCALLTYPE *Describe )(
        __x_TestApp_CParity_CIAnimalOverrides * This,
        /* [out, retval] */HSTRING * result
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIAnimalOverridesVtbl;

interface __x_TestApp_CParity_CIAnimalOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CIAnimalOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIAnimalOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIAnimalOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIAnimalOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIAnimalOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIAnimalOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIAnimalOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIAnimalOverrides_MakeSound(This) \
    ( (This)->lpVtbl->MakeSound(This) )

#define __x_TestApp_CParity_CIAnimalOverrides_Describe(This,result) \
    ( (This)->lpVtbl->Describe(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIAnimalOverrides;
#endif /* !defined(____x_TestApp_CParity_CIAnimalOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IAnimalProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Animal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IAnimalProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Animal**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.Animal**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIAnimalProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIAnimalProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IAnimalProtectedFactory[] = L"TestApp.Parity.IAnimalProtectedFactory";
/* [uuid("1f0270d9-3405-5946-b829-1a785ae42ef2"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIAnimalProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIAnimalProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIAnimalProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIAnimalProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIAnimalProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIAnimalProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIAnimalProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIAnimalProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIAnimal * * value
        );
    HRESULT ( STDMETHODCALLTYPE *CreateInstance2 )(
        __x_TestApp_CParity_CIAnimalProtectedFactory * This,
        /* [in] */HSTRING name,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIAnimal * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIAnimalProtectedFactoryVtbl;

interface __x_TestApp_CParity_CIAnimalProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIAnimalProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIAnimalProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )

#define __x_TestApp_CParity_CIAnimalProtectedFactory_CreateInstance2(This,name,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance2(This,name,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIAnimalProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIAnimalProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Animal
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IAnimal ** Default Interface **
 *    TestApp.Parity.IAnimalOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Animal_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Animal_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Animal[] = L"TestApp.Parity.Animal";
#endif



/*
 *
 * Interface TestApp.Parity.IMammal
 *
 * Interface is a part of the implementation of type TestApp.Parity.Mammal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMammal:HRESULT get_IsWarmBlooded(Boolean*);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMammal_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMammal_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMammal[] = L"TestApp.Parity.IMammal";
/* [uuid("49e3acb2-c737-5111-a70e-ffe7fd034274"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMammalVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMammal * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMammal * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMammal * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMammal * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMammal * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMammal * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_IsWarmBlooded )(
        __x_TestApp_CParity_CIMammal * This,
        /* [out, retval] */boolean * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMammalVtbl;

interface __x_TestApp_CParity_CIMammal
{
    CONST_VTBL struct __x_TestApp_CParity_CIMammalVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMammal_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMammal_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMammal_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMammal_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMammal_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMammal_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMammal_get_IsWarmBlooded(This,value) \
    ( (This)->lpVtbl->get_IsWarmBlooded(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMammal;
#endif /* !defined(____x_TestApp_CParity_CIMammal_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMammalOverrides
 *
 * Interface is a part of the implementation of type TestApp.Parity.Mammal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMammalOverrides:HRESULT Breathe();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMammalOverrides_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMammalOverrides_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMammalOverrides[] = L"TestApp.Parity.IMammalOverrides";
/* [uuid("e6532ac5-3f9e-56ad-a9dc-6ce5c57be856"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMammalOverridesVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMammalOverrides * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMammalOverrides * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMammalOverrides * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMammalOverrides * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMammalOverrides * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMammalOverrides * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Breathe )(
        __x_TestApp_CParity_CIMammalOverrides * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMammalOverridesVtbl;

interface __x_TestApp_CParity_CIMammalOverrides
{
    CONST_VTBL struct __x_TestApp_CParity_CIMammalOverridesVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMammalOverrides_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMammalOverrides_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMammalOverrides_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMammalOverrides_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMammalOverrides_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMammalOverrides_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMammalOverrides_Breathe(This) \
    ( (This)->lpVtbl->Breathe(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMammalOverrides;
#endif /* !defined(____x_TestApp_CParity_CIMammalOverrides_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IMammalProtectedFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Mammal
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IMammalProtectedFactory:HRESULT CreateInstance(Object*,Object**,TestApp.Parity.Mammal**);HRESULT CreateInstance2(String,Object*,Object**,TestApp.Parity.Mammal**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIMammalProtectedFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIMammalProtectedFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IMammalProtectedFactory[] = L"TestApp.Parity.IMammalProtectedFactory";
/* [uuid("53c7a9b7-d78f-55a5-8051-23ef088fdafb"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIMammalProtectedFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIMammalProtectedFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIMammalProtectedFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIMammalProtectedFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIMammalProtectedFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIMammalProtectedFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIMammalProtectedFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIMammalProtectedFactory * This,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIMammal * * value
        );
    HRESULT ( STDMETHODCALLTYPE *CreateInstance2 )(
        __x_TestApp_CParity_CIMammalProtectedFactory * This,
        /* [in] */HSTRING name,
        IInspectable * baseInterface,
        /* [out] */IInspectable * * innerInterface,
        /* [retval, out] */__x_TestApp_CParity_CIMammal * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIMammalProtectedFactoryVtbl;

interface __x_TestApp_CParity_CIMammalProtectedFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIMammalProtectedFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIMammalProtectedFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_CreateInstance(This,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance(This,baseInterface,innerInterface,value) )

#define __x_TestApp_CParity_CIMammalProtectedFactory_CreateInstance2(This,name,baseInterface,innerInterface,value) \
    ( (This)->lpVtbl->CreateInstance2(This,name,baseInterface,innerInterface,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIMammalProtectedFactory;
#endif /* !defined(____x_TestApp_CParity_CIMammalProtectedFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Mammal
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IMammal ** Default Interface **
 *    TestApp.Parity.IMammalOverrides
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Mammal_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Mammal_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Mammal[] = L"TestApp.Parity.Mammal";
#endif



/*
 *
 * Interface TestApp.Parity.IDog
 *
 * Interface is a part of the implementation of type TestApp.Parity.Dog
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDog:HRESULT get_Breed(String*);HRESULT Fetch();HRESULT Sit();
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDog_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDog_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDog[] = L"TestApp.Parity.IDog";
/* [uuid("1eaff1c2-66e4-5d2f-8f88-4624023d88a6"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDogVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDog * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDog * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDog * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDog * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDog * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDog * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Breed )(
        __x_TestApp_CParity_CIDog * This,
        /* [out, retval] */HSTRING * value
        );
    HRESULT ( STDMETHODCALLTYPE *Fetch )(
        __x_TestApp_CParity_CIDog * This
        );
    HRESULT ( STDMETHODCALLTYPE *Sit )(
        __x_TestApp_CParity_CIDog * This
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDogVtbl;

interface __x_TestApp_CParity_CIDog
{
    CONST_VTBL struct __x_TestApp_CParity_CIDogVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDog_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDog_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDog_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDog_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDog_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDog_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDog_get_Breed(This,value) \
    ( (This)->lpVtbl->get_Breed(This,value) )

#define __x_TestApp_CParity_CIDog_Fetch(This) \
    ( (This)->lpVtbl->Fetch(This) )

#define __x_TestApp_CParity_CIDog_Sit(This) \
    ( (This)->lpVtbl->Sit(This) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDog;
#endif /* !defined(____x_TestApp_CParity_CIDog_INTERFACE_DEFINED__) */


/*
 *
 * Interface TestApp.Parity.IDogFactory
 *
 * Interface is a part of the implementation of type TestApp.Parity.Dog
 *
 *
 * The IID for this interface was automatically generated by MIDLRT.
 *
 * Interface IID generation seed: TestApp.Parity.IDogFactory:HRESULT CreateInstance(String,TestApp.Parity.Dog**);
 *
 *
 */
#if !defined(____x_TestApp_CParity_CIDogFactory_INTERFACE_DEFINED__)
#define ____x_TestApp_CParity_CIDogFactory_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_TestApp_Parity_IDogFactory[] = L"TestApp.Parity.IDogFactory";
/* [uuid("14bea965-06c9-582e-b0e3-c0bfcc6cc911"), version, object, exclusiveto] */
typedef struct __x_TestApp_CParity_CIDogFactoryVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_TestApp_CParity_CIDogFactory * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_TestApp_CParity_CIDogFactory * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_TestApp_CParity_CIDogFactory * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_TestApp_CParity_CIDogFactory * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_TestApp_CParity_CIDogFactory * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_TestApp_CParity_CIDogFactory * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateInstance )(
        __x_TestApp_CParity_CIDogFactory * This,
        /* [in] */HSTRING name,
        /* [retval, out] */__x_TestApp_CParity_CIDog * * value
        );
    END_INTERFACE
    
} __x_TestApp_CParity_CIDogFactoryVtbl;

interface __x_TestApp_CParity_CIDogFactory
{
    CONST_VTBL struct __x_TestApp_CParity_CIDogFactoryVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_TestApp_CParity_CIDogFactory_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_TestApp_CParity_CIDogFactory_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_TestApp_CParity_CIDogFactory_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_TestApp_CParity_CIDogFactory_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_TestApp_CParity_CIDogFactory_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_TestApp_CParity_CIDogFactory_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_TestApp_CParity_CIDogFactory_CreateInstance(This,name,value) \
    ( (This)->lpVtbl->CreateInstance(This,name,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_TestApp_CParity_CIDogFactory;
#endif /* !defined(____x_TestApp_CParity_CIDogFactory_INTERFACE_DEFINED__) */


/*
 *
 * Class TestApp.Parity.Dog
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    TestApp.Parity.IDog ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_TestApp_Parity_Dog_DEFINED
#define RUNTIMECLASS_TestApp_Parity_Dog_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_TestApp_Parity_Dog[] = L"TestApp.Parity.Dog";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __parity_unsealed_inheritance_p_h__

#endif // __parity_unsealed_inheritance_h__
