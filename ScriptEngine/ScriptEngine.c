// MonoLib.cpp : 定义 DLL 应用程序的导出函数。
//

#include "main/runtime.h"
#include "main/Mediator.h"
#include <mono/metadata/exception.h>

// Macro to put before functions that need to be exposed to C#
#ifdef _WIN32
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT 
#endif

const char* il2cpp_exception = NULL;
const char* mono_exception = NULL;

DLLEXPORT void SetupMono(char* bundleDir, const char* dllName)
{
	mono_setup(bundleDir,dllName);
}

DLLEXPORT void CloseMono()
{
	mono_exit();
}

DLLEXPORT void SetFuncPointer(void * ptr)
{
	g_manageFuncPtr = ptr;
}

DLLEXPORT void* GetFuncPointer()
{
	return g_manageFuncPtr;
}

DLLEXPORT void OnExceptionIl2cpp(const char* msg)
{
	//raise_mono_exception_runtime(msg);
	il2cpp_exception = msg;
}
DLLEXPORT void CheckExceptionIl2cpp()
{
	if (il2cpp_exception)
	{
		MonoException* exc = mono_exception_from_name_msg(mono_get_corlib(), "System", "Exception", il2cpp_exception);
		il2cpp_exception = NULL;
		mono_raise_exception(exc);
	}
}

DLLEXPORT void OnExceptionMono(const char* msg)
{
	//raise_il2cpp_exception_runtime(msg);
	mono_exception = msg;
}

DLLEXPORT void CheckExceptionMono()
{
	if (mono_exception)
	{
		Il2CppException* exc = il2cpp_exception_from_name_msg(il2cpp_get_corlib(), "System", "Exception", mono_exception);
		mono_exception = NULL;
		il2cpp_raise_exception(exc);
	}
}
