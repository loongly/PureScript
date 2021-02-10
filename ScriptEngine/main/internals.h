#pragma once
#include "runtime.h"
#include "glib.h"
#include <mono/metadata/class.h>

#include "il2cpp_support.h"


#ifdef __cplusplus
extern "C" {
#endif

	bool il2cpp_check_flag(Il2CppClass* klass, int value);
	void il2cpp_add_flag(Il2CppClass* klass, int value);

	bool mono_check_flag(MonoClass* klass, int value);
	void mono_add_flag(MonoClass* klass, int value);

#ifdef __cplusplus
}
#endif