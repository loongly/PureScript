// MonoLib.cpp : 定义 DLL 应用程序的导出函数。
//

#include "main/runtime.h"
#include "main/Mediator.h"

// Macro to put before functions that need to be exposed to C#
#ifdef _WIN32
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT 
#endif

void* funcPtr = 0;

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
	raise_mono_exception_runtime(msg);
}

DLLEXPORT void OnExceptionMono(const char* msg)
{
	raise_il2cpp_exception_runtime(msg);
}

/*
#include <iostream>
#include <windows.h>
#include <direct.h>

void SetupMono()
{
	typedef void(*funcptr)(char*, const char*);

	funcptr func;

	char path[1024];
	char* bundle_path = (char *)malloc(MAX_PATH);
	memset(bundle_path, 0, MAX_PATH);
	GetModuleFileName(NULL, bundle_path, MAX_PATH); // 得到当前执行文件的文件名（包含路径）
	*(strrchr(bundle_path, '\\')) = '\0';   // 删除文件名，只留下目录

	const char* dllName = "MonoLib.dll";
	const char* funcName = "SetupMono";
	HMODULE hDLL = LoadLibrary(dllName);
	if (hDLL != NULL)
	{
		func = (funcptr)GetProcAddress(hDLL, funcName);
		if (func == NULL)
		{
			std::cout << "Cannot Find Function " << funcName << std::endl;
			return;
		}
	}
	else
	{
		std::cout << "Cannot Find " << dllName << std::endl;
		return;
	}

	(*func)(bundle_path, "test.exe");
}

*/