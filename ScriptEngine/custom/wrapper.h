#pragma once

#include "engine_include.h"


#if defined(__cplusplus)
extern "C" {
#endif // __cplusplus

	void init_wrapper();

	Il2CppReflectionType* get_monobehaviour_wrapper_rtype();
	Il2CppClass* get_monobehaviour_wrapper_class();
	void call_wrapper_init(Il2CppObject* il2cpp, MonoObject* mono);

	//more efficient "get_mono_object" for wrapper
	MonoObject* get_mono_wrapper_object(Il2CppObject* il2cpp, MonoClass* m_class);

	Il2CppObject* create_il2cpp_enumerator_wrapper(MonoObject* mono);
	Il2CppClass* get_enumerator_wrapper_class();
	MonoClass* get_coroutine_class();

	//MonoObject* create_monobehaviour(MonoClass * kclass, Il2CppObject * il2cppObj);



#if defined(__cplusplus)
}
#endif // __cplusplus
