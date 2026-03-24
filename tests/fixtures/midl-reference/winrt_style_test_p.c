

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.01.0628 */
/* at Mon Jan 18 19:14:07 2038
 */
/* Compiler settings for winrt_style_test.idl:
    Oicf, W1, Zp8, env=Win64 (32b run), target_arch=AMD64 8.01.0628 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#if defined(_M_AMD64)


#pragma warning( disable: 4049 )  /* more than 64k source lines */
#if _MSC_VER >= 1200
#pragma warning(push)
#endif

#pragma warning( disable: 4211 )  /* redefine extern to static */
#pragma warning( disable: 4232 )  /* dllimport identity*/
#pragma warning( disable: 4024 )  /* array to pointer mapping*/
#pragma warning( disable: 4152 )  /* function/data pointer conversion in expression */
#pragma warning( disable: 4100 ) /* unreferenced arguments in x86 call */

#pragma optimize("", off ) 

#define USE_STUBLESS_PROXY


/* verify that the <rpcproxy.h> version is high enough to compile this file*/
#ifndef __REDQ_RPCPROXY_H_VERSION__
#define __REQUIRED_RPCPROXY_H_VERSION__ 475
#endif


#include "rpcproxy.h"
#ifndef __RPCPROXY_H_VERSION__
#error this stub requires an updated version of <rpcproxy.h>
#endif /* __RPCPROXY_H_VERSION__ */


#include "winrt_style_test.h"

#define TYPE_FORMAT_STRING_SIZE   113                               
#define PROC_FORMAT_STRING_SIZE   449                               
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _winrt_style_test_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } winrt_style_test_MIDL_TYPE_FORMAT_STRING;

typedef struct _winrt_style_test_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } winrt_style_test_MIDL_PROC_FORMAT_STRING;

typedef struct _winrt_style_test_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } winrt_style_test_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax_2_0 = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};

#if defined(_CONTROL_FLOW_GUARD_XFG)
#define XFG_TRAMPOLINES(ObjectType)\
NDR_SHAREABLE unsigned long ObjectType ## _UserSize_XFG(unsigned long * pFlags, unsigned long Offset, void * pObject)\
{\
return  ObjectType ## _UserSize(pFlags, Offset, (ObjectType *)pObject);\
}\
NDR_SHAREABLE unsigned char * ObjectType ## _UserMarshal_XFG(unsigned long * pFlags, unsigned char * pBuffer, void * pObject)\
{\
return ObjectType ## _UserMarshal(pFlags, pBuffer, (ObjectType *)pObject);\
}\
NDR_SHAREABLE unsigned char * ObjectType ## _UserUnmarshal_XFG(unsigned long * pFlags, unsigned char * pBuffer, void * pObject)\
{\
return ObjectType ## _UserUnmarshal(pFlags, pBuffer, (ObjectType *)pObject);\
}\
NDR_SHAREABLE void ObjectType ## _UserFree_XFG(unsigned long * pFlags, void * pObject)\
{\
ObjectType ## _UserFree(pFlags, (ObjectType *)pObject);\
}
#define XFG_TRAMPOLINES64(ObjectType)\
NDR_SHAREABLE unsigned long ObjectType ## _UserSize64_XFG(unsigned long * pFlags, unsigned long Offset, void * pObject)\
{\
return  ObjectType ## _UserSize64(pFlags, Offset, (ObjectType *)pObject);\
}\
NDR_SHAREABLE unsigned char * ObjectType ## _UserMarshal64_XFG(unsigned long * pFlags, unsigned char * pBuffer, void * pObject)\
{\
return ObjectType ## _UserMarshal64(pFlags, pBuffer, (ObjectType *)pObject);\
}\
NDR_SHAREABLE unsigned char * ObjectType ## _UserUnmarshal64_XFG(unsigned long * pFlags, unsigned char * pBuffer, void * pObject)\
{\
return ObjectType ## _UserUnmarshal64(pFlags, pBuffer, (ObjectType *)pObject);\
}\
NDR_SHAREABLE void ObjectType ## _UserFree64_XFG(unsigned long * pFlags, void * pObject)\
{\
ObjectType ## _UserFree64(pFlags, (ObjectType *)pObject);\
}
#define XFG_BIND_TRAMPOLINES(HandleType, ObjectType)\
static void* ObjectType ## _bind_XFG(HandleType pObject)\
{\
return ObjectType ## _bind((ObjectType) pObject);\
}\
static void ObjectType ## _unbind_XFG(HandleType pObject, handle_t ServerHandle)\
{\
ObjectType ## _unbind((ObjectType) pObject, ServerHandle);\
}
#define XFG_TRAMPOLINE_FPTR(Function) Function ## _XFG
#define XFG_TRAMPOLINE_FPTR_DEPENDENT_SYMBOL(Symbol) Symbol ## _XFG
#else
#define XFG_TRAMPOLINES(ObjectType)
#define XFG_TRAMPOLINES64(ObjectType)
#define XFG_BIND_TRAMPOLINES(HandleType, ObjectType)
#define XFG_TRAMPOLINE_FPTR(Function) Function
#define XFG_TRAMPOLINE_FPTR_DEPENDENT_SYMBOL(Symbol) Symbol
#endif


extern const winrt_style_test_MIDL_TYPE_FORMAT_STRING winrt_style_test__MIDL_TypeFormatString;
extern const winrt_style_test_MIDL_PROC_FORMAT_STRING winrt_style_test__MIDL_ProcFormatString;
extern const winrt_style_test_MIDL_EXPR_FORMAT_STRING winrt_style_test__MIDL_ExprFormatString;

#ifdef __cplusplus
namespace {
#endif

extern const MIDL_STUB_DESC Object_StubDesc;
#ifdef __cplusplus
}
#endif


extern const MIDL_SERVER_INFO IWidgetProvider_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IWidgetProvider_ProxyInfo;

#ifdef __cplusplus
namespace {
#endif

extern const MIDL_STUB_DESC Object_StubDesc;
#ifdef __cplusplus
}
#endif


extern const MIDL_SERVER_INFO IWidgetFactory_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IWidgetFactory_ProxyInfo;


extern const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[ WIRE_MARSHAL_TABLE_SIZE ];

#if !defined(__RPC_WIN64__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT50_OR_LATER)
#error You need Windows 2000 or later to run this stub because it uses these features:
#error   /robust command line switch.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will fail with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const winrt_style_test_MIDL_PROC_FORMAT_STRING winrt_style_test__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure GetWidget */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x6 ),	/* 6 */
/*  8 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 16 */	0xa,		/* 10 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 18 */	NdrFcShort( 0x0 ),	/* 0 */
/* 20 */	NdrFcShort( 0x0 ),	/* 0 */
/* 22 */	NdrFcShort( 0x0 ),	/* 0 */
/* 24 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter widget */

/* 26 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 28 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 30 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 32 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 34 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 36 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetName */

/* 38 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 40 */	NdrFcLong( 0x0 ),	/* 0 */
/* 44 */	NdrFcShort( 0x7 ),	/* 7 */
/* 46 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 48 */	NdrFcShort( 0x0 ),	/* 0 */
/* 50 */	NdrFcShort( 0x8 ),	/* 8 */
/* 52 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 54 */	0xa,		/* 10 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 56 */	NdrFcShort( 0x0 ),	/* 0 */
/* 58 */	NdrFcShort( 0x1 ),	/* 1 */
/* 60 */	NdrFcShort( 0x0 ),	/* 0 */
/* 62 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter name */

/* 64 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 66 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 68 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Return value */

/* 70 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 72 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 74 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetName */

/* 76 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 78 */	NdrFcLong( 0x0 ),	/* 0 */
/* 82 */	NdrFcShort( 0x8 ),	/* 8 */
/* 84 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 86 */	NdrFcShort( 0x0 ),	/* 0 */
/* 88 */	NdrFcShort( 0x8 ),	/* 8 */
/* 90 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 92 */	0xa,		/* 10 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 94 */	NdrFcShort( 0x1 ),	/* 1 */
/* 96 */	NdrFcShort( 0x0 ),	/* 0 */
/* 98 */	NdrFcShort( 0x0 ),	/* 0 */
/* 100 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter name */

/* 102 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 104 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 106 */	NdrFcShort( 0x44 ),	/* Type Offset=68 */

	/* Return value */

/* 108 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 110 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 112 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetCount */

/* 114 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 116 */	NdrFcLong( 0x0 ),	/* 0 */
/* 120 */	NdrFcShort( 0x9 ),	/* 9 */
/* 122 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 124 */	NdrFcShort( 0x0 ),	/* 0 */
/* 126 */	NdrFcShort( 0x24 ),	/* 36 */
/* 128 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 130 */	0xa,		/* 10 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 132 */	NdrFcShort( 0x0 ),	/* 0 */
/* 134 */	NdrFcShort( 0x0 ),	/* 0 */
/* 136 */	NdrFcShort( 0x0 ),	/* 0 */
/* 138 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter count */

/* 140 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 142 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 144 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 146 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 148 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 150 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetEnabled */

/* 152 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 154 */	NdrFcLong( 0x0 ),	/* 0 */
/* 158 */	NdrFcShort( 0xa ),	/* 10 */
/* 160 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 162 */	NdrFcShort( 0x5 ),	/* 5 */
/* 164 */	NdrFcShort( 0x8 ),	/* 8 */
/* 166 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 168 */	0xa,		/* 10 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 170 */	NdrFcShort( 0x0 ),	/* 0 */
/* 172 */	NdrFcShort( 0x0 ),	/* 0 */
/* 174 */	NdrFcShort( 0x0 ),	/* 0 */
/* 176 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter enabled */

/* 178 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 180 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 182 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 184 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 186 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 188 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetEnabled */

/* 190 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 192 */	NdrFcLong( 0x0 ),	/* 0 */
/* 196 */	NdrFcShort( 0xb ),	/* 11 */
/* 198 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 200 */	NdrFcShort( 0x0 ),	/* 0 */
/* 202 */	NdrFcShort( 0x21 ),	/* 33 */
/* 204 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 206 */	0xa,		/* 10 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 208 */	NdrFcShort( 0x0 ),	/* 0 */
/* 210 */	NdrFcShort( 0x0 ),	/* 0 */
/* 212 */	NdrFcShort( 0x0 ),	/* 0 */
/* 214 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter enabled */

/* 216 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 218 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 220 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 222 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 224 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 226 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Configure */

/* 228 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 230 */	NdrFcLong( 0x0 ),	/* 0 */
/* 234 */	NdrFcShort( 0xc ),	/* 12 */
/* 236 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 238 */	NdrFcShort( 0xd ),	/* 13 */
/* 240 */	NdrFcShort( 0x8 ),	/* 8 */
/* 242 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x4,		/* 4 */
/* 244 */	0xa,		/* 10 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 246 */	NdrFcShort( 0x0 ),	/* 0 */
/* 248 */	NdrFcShort( 0x1 ),	/* 1 */
/* 250 */	NdrFcShort( 0x0 ),	/* 0 */
/* 252 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter name */

/* 254 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 256 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 258 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Parameter count */

/* 260 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 262 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 264 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter enabled */

/* 266 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 268 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 270 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 272 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 274 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 276 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetValue */

/* 278 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 280 */	NdrFcLong( 0x0 ),	/* 0 */
/* 284 */	NdrFcShort( 0xd ),	/* 13 */
/* 286 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 288 */	NdrFcShort( 0x0 ),	/* 0 */
/* 290 */	NdrFcShort( 0x2c ),	/* 44 */
/* 292 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 294 */	0xa,		/* 10 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 296 */	NdrFcShort( 0x0 ),	/* 0 */
/* 298 */	NdrFcShort( 0x0 ),	/* 0 */
/* 300 */	NdrFcShort( 0x0 ),	/* 0 */
/* 302 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter value */

/* 304 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 306 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 308 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Return value */

/* 310 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 312 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 314 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetGuid */

/* 316 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 318 */	NdrFcLong( 0x0 ),	/* 0 */
/* 322 */	NdrFcShort( 0xe ),	/* 14 */
/* 324 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 326 */	NdrFcShort( 0x44 ),	/* 68 */
/* 328 */	NdrFcShort( 0x8 ),	/* 8 */
/* 330 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 332 */	0xa,		/* 10 */
			0x81,		/* Ext Flags:  new corr desc, has big byval param */
/* 334 */	NdrFcShort( 0x0 ),	/* 0 */
/* 336 */	NdrFcShort( 0x0 ),	/* 0 */
/* 338 */	NdrFcShort( 0x0 ),	/* 0 */
/* 340 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter id */

/* 342 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 344 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 346 */	NdrFcShort( 0x64 ),	/* Type Offset=100 */

	/* Return value */

/* 348 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 350 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 352 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateWidget */

/* 354 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 356 */	NdrFcLong( 0x0 ),	/* 0 */
/* 360 */	NdrFcShort( 0x6 ),	/* 6 */
/* 362 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 364 */	NdrFcShort( 0x0 ),	/* 0 */
/* 366 */	NdrFcShort( 0x8 ),	/* 8 */
/* 368 */	0x47,		/* Oi2 Flags:  srv must size, clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 370 */	0xa,		/* 10 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 372 */	NdrFcShort( 0x0 ),	/* 0 */
/* 374 */	NdrFcShort( 0x1 ),	/* 1 */
/* 376 */	NdrFcShort( 0x0 ),	/* 0 */
/* 378 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter name */

/* 380 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 382 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 384 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Parameter widget */

/* 386 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 388 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 390 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 392 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 394 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 396 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateWithCount */

/* 398 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 400 */	NdrFcLong( 0x0 ),	/* 0 */
/* 404 */	NdrFcShort( 0x7 ),	/* 7 */
/* 406 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 408 */	NdrFcShort( 0x8 ),	/* 8 */
/* 410 */	NdrFcShort( 0x8 ),	/* 8 */
/* 412 */	0x47,		/* Oi2 Flags:  srv must size, clt must size, has return, has ext, */
			0x4,		/* 4 */
/* 414 */	0xa,		/* 10 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 416 */	NdrFcShort( 0x0 ),	/* 0 */
/* 418 */	NdrFcShort( 0x1 ),	/* 1 */
/* 420 */	NdrFcShort( 0x0 ),	/* 0 */
/* 422 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter name */

/* 424 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 426 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 428 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Parameter count */

/* 430 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 432 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 434 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter widget */

/* 436 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 438 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 440 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 442 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 444 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 446 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const winrt_style_test_MIDL_TYPE_FORMAT_STRING winrt_style_test__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/*  4 */	NdrFcShort( 0x2 ),	/* Offset= 2 (6) */
/*  6 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/*  8 */	NdrFcLong( 0xaf86e2e0 ),	/* -1350114592 */
/* 12 */	NdrFcShort( 0xb12d ),	/* -20179 */
/* 14 */	NdrFcShort( 0x4c6a ),	/* 19562 */
/* 16 */	0x9c,		/* 156 */
			0x5a,		/* 90 */
/* 18 */	0xd7,		/* 215 */
			0xaa,		/* 170 */
/* 20 */	0x65,		/* 101 */
			0x10,		/* 16 */
/* 22 */	0x1e,		/* 30 */
			0x90,		/* 144 */
/* 24 */	
			0x12, 0x0,	/* FC_UP */
/* 26 */	NdrFcShort( 0xe ),	/* Offset= 14 (40) */
/* 28 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 30 */	NdrFcShort( 0x2 ),	/* 2 */
/* 32 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 34 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 36 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 38 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 40 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 42 */	NdrFcShort( 0x8 ),	/* 8 */
/* 44 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (28) */
/* 46 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 48 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 50 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 52 */	NdrFcShort( 0x0 ),	/* 0 */
/* 54 */	NdrFcShort( 0x8 ),	/* 8 */
/* 56 */	NdrFcShort( 0x0 ),	/* 0 */
/* 58 */	NdrFcShort( 0xffde ),	/* Offset= -34 (24) */
/* 60 */	
			0x11, 0x4,	/* FC_RP [alloced_on_stack] */
/* 62 */	NdrFcShort( 0x6 ),	/* Offset= 6 (68) */
/* 64 */	
			0x13, 0x0,	/* FC_OP */
/* 66 */	NdrFcShort( 0xffe6 ),	/* Offset= -26 (40) */
/* 68 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 70 */	NdrFcShort( 0x0 ),	/* 0 */
/* 72 */	NdrFcShort( 0x8 ),	/* 8 */
/* 74 */	NdrFcShort( 0x0 ),	/* 0 */
/* 76 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (64) */
/* 78 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 80 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 82 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 84 */	0x3,		/* FC_SMALL */
			0x5c,		/* FC_PAD */
/* 86 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 88 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 90 */	
			0x11, 0x0,	/* FC_RP */
/* 92 */	NdrFcShort( 0x8 ),	/* Offset= 8 (100) */
/* 94 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 96 */	NdrFcShort( 0x8 ),	/* 8 */
/* 98 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 100 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 102 */	NdrFcShort( 0x10 ),	/* 16 */
/* 104 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 106 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 108 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (94) */
			0x5b,		/* FC_END */

			0x0
        }
    };

XFG_TRAMPOLINES(HSTRING)

static const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[ WIRE_MARSHAL_TABLE_SIZE ] = 
        {
            
            {
            (USER_MARSHAL_SIZING_ROUTINE)XFG_TRAMPOLINE_FPTR(HSTRING_UserSize)
            ,(USER_MARSHAL_MARSHALLING_ROUTINE)XFG_TRAMPOLINE_FPTR(HSTRING_UserMarshal)
            ,(USER_MARSHAL_UNMARSHALLING_ROUTINE)XFG_TRAMPOLINE_FPTR(HSTRING_UserUnmarshal)
            ,(USER_MARSHAL_FREEING_ROUTINE)XFG_TRAMPOLINE_FPTR(HSTRING_UserFree)
            
            }
            

        };



/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: IInspectable, ver. 0.0,
   GUID={0xAF86E2E0,0xB12D,0x4c6a,{0x9C,0x5A,0xD7,0xAA,0x65,0x10,0x1E,0x90}} */


/* Object interface: IWidgetProvider, ver. 0.0,
   GUID={0xA1B2C3D4,0xE5F6,0x7890,{0xAB,0xCD,0xEF,0x01,0x23,0x45,0x67,0x89}} */

#pragma code_seg(".orpc")
static const unsigned short IWidgetProvider_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    0,
    38,
    76,
    114,
    152,
    190,
    228,
    278,
    316
    };

static const MIDL_STUBLESS_PROXY_INFO IWidgetProvider_ProxyInfo =
    {
    &Object_StubDesc,
    winrt_style_test__MIDL_ProcFormatString.Format,
    &IWidgetProvider_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IWidgetProvider_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    winrt_style_test__MIDL_ProcFormatString.Format,
    &IWidgetProvider_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(15) _IWidgetProviderProxyVtbl = 
{
    &IWidgetProvider_ProxyInfo,
    &IID_IWidgetProvider,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::GetWidget */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::SetName */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::GetName */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::GetCount */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::SetEnabled */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::GetEnabled */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::Configure */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::GetValue */ ,
    (void *) (INT_PTR) -1 /* IWidgetProvider::SetGuid */
};


EXTERN_C DECLSPEC_SELECTANY const PRPC_STUB_FUNCTION IWidgetProvider_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl _IWidgetProviderStubVtbl =
{
    &IID_IWidgetProvider,
    &IWidgetProvider_ServerInfo,
    15,
    &IWidgetProvider_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Object interface: IWidgetFactory, ver. 0.0,
   GUID={0xB2C3D4E5,0xF6A7,0x8901,{0xBC,0xDE,0xF0,0x12,0x34,0x56,0x78,0x90}} */

#pragma code_seg(".orpc")
static const unsigned short IWidgetFactory_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    354,
    398
    };

static const MIDL_STUBLESS_PROXY_INFO IWidgetFactory_ProxyInfo =
    {
    &Object_StubDesc,
    winrt_style_test__MIDL_ProcFormatString.Format,
    &IWidgetFactory_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IWidgetFactory_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    winrt_style_test__MIDL_ProcFormatString.Format,
    &IWidgetFactory_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) _IWidgetFactoryProxyVtbl = 
{
    &IWidgetFactory_ProxyInfo,
    &IID_IWidgetFactory,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* IWidgetFactory::CreateWidget */ ,
    (void *) (INT_PTR) -1 /* IWidgetFactory::CreateWithCount */
};


EXTERN_C DECLSPEC_SELECTANY const PRPC_STUB_FUNCTION IWidgetFactory_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl _IWidgetFactoryStubVtbl =
{
    &IID_IWidgetFactory,
    &IWidgetFactory_ServerInfo,
    8,
    &IWidgetFactory_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};

#ifdef __cplusplus
namespace {
#endif
static const MIDL_STUB_DESC Object_StubDesc = 
    {
    0,
    NdrOleAllocate,
    NdrOleFree,
    0,
    0,
    0,
    0,
    0,
    winrt_style_test__MIDL_TypeFormatString.Format,
    1, /* -error bounds_check flag */
    0x50002, /* Ndr library version */
    0,
    0x8010274, /* MIDL Version 8.1.628 */
    0,
    UserMarshalRoutines,
    0,  /* notify & notify_flag routine table */
    0x1, /* MIDL flag */
    0, /* cs routines */
    0,   /* proxy/server info */
    0
    };
#ifdef __cplusplus
}
#endif

const CInterfaceProxyVtbl * const _winrt_style_test_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IWidgetProviderProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IWidgetFactoryProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _winrt_style_test_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IWidgetProviderStubVtbl,
    ( CInterfaceStubVtbl *) &_IWidgetFactoryStubVtbl,
    0
};

PCInterfaceName const _winrt_style_test_InterfaceNamesList[] = 
{
    "IWidgetProvider",
    "IWidgetFactory",
    0
};

const IID *  const _winrt_style_test_BaseIIDList[] = 
{
    &IID_IInspectable,
    &IID_IInspectable,
    0
};


#define _winrt_style_test_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _winrt_style_test, pIID, n)

int __stdcall _winrt_style_test_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _winrt_style_test, 2, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _winrt_style_test, 2, *pIndex )
    
}

EXTERN_C const ExtendedProxyFileInfo winrt_style_test_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _winrt_style_test_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _winrt_style_test_StubVtblList,
    (const PCInterfaceName * ) & _winrt_style_test_InterfaceNamesList,
    (const IID ** ) & _winrt_style_test_BaseIIDList,
    & _winrt_style_test_IID_Lookup, 
    2,
    2,
    0, /* table of [async_uuid] interfaces */
    0, /* Filler1 */
    0, /* Filler2 */
    0  /* Filler3 */
};
#if _MSC_VER >= 1200
#pragma warning(pop)
#endif


#endif /* defined(_M_AMD64)*/

