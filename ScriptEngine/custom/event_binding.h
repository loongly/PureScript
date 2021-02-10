#pragma once

#ifndef __h_event_binding__
#define __h_event_binding__

#include <mono/jit/jit.h>
#include "../main//il2cpp_support.h"
//#include "il2cpp-class-internals.h"

#if defined _MSC_VER
//#define THUNK_METHOD __stdcall
#define THUNK_METHOD 
#else
#define THUNK_METHOD  __attribute__((__stdcall))
#endif

#if defined(__cplusplus)
extern "C" {
#endif // __cplusplus
	//typedef void(*EventMethodPointer)();
	typedef struct
	{
		/*const char* module;
		const char* name_space;
		const char* class_name;*/
		//Il2CppClass* i2_klass;
		//MonoClass* mono_klass;
		//const char* method_name;
		//int32_t param_count;
		//EventMethodPointer hooked;
		MonoMethod* hooked;
		Il2CppMethodPointer orign;
	}EventMethodDesc;

	void init_event_method(EventMethodDesc* desc, MonoClass *monoklass, Il2CppClass* ilklass, const char* method_name, int param_count, Il2CppMethodPointer hook2);
	void init_event();

#if defined(__cplusplus)
	}
#endif // __cplusplus

#endif // !__h_event_binding__