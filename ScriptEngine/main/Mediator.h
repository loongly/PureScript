#ifndef ___marshal_h
#define ___marshal_h

#include "runtime.h"
#include "il2cpp_support.h"

#define CLASS_MASK_UNITY_NATIVE (1<<2)
#define CLASS_MASK_WRAPPER (1<<3)

#if defined(__cplusplus)
extern "C"
{
#endif // __cplusplus

	typedef struct UnityObjectHead
	{
		void* __align1;
		void* __align2;
		void* objectPtr;
	}UnityObjectHead;

	void bind_mono_il2cpp_object(MonoObject* mono, Il2CppObject* il2cpp);
	MonoObject* get_mono_object(Il2CppObject* il2cpp, MonoClass* m_class);
	Il2CppObject* get_il2cpp_object(MonoObject* mono,Il2CppClass* m_class);
	Il2CppObject* get_il2cpp_object_with_ptr(void* objPtr);
	void* get_il2cpp_internal_ptr(Il2CppObject* obj);

	MonoString* get_mono_string(Il2CppString* str);
	Il2CppString* get_il2cpp_string(MonoString* str);
	MonoArray* get_mono_array(Il2CppArray * array);
	Il2CppArray* get_il2cpp_array(MonoArray* array);
	void copy_array_i2_mono(Il2CppArray* i2_array, MonoArray* mono_array);

	MonoImage* mono_get_image(const char* assembly);
	MonoClass* mono_get_class(MonoImage* image, const char* _namespace, const char* name);
	MonoClass* mono_search_class(const char* assembly, const char* _namespace, const char* name);
	MonoMethod* mono_lookup_method(const char* method_name, MonoClass *kclass, int param_count);
	
	Il2CppClass* get_il2cpp_class(MonoClass* mclass);
	MonoClass* get_mono_class(Il2CppClass* mclass);

	Il2CppReflectionType* get_il2cpp_reflection_type(MonoReflectionType * type);
	MonoReflectionType* get_mono_reflection_type(Il2CppReflectionType * type);

	bool is_unity_name_space(const char* ns);
	const char* debug_mono_obj(MonoObject* obj);
	const char* debug_mono_method(MonoMethod* method);
	const char* debug_il2cpp_obj(Il2CppObject* obj);

#if defined(__cplusplus)
}
#endif // __cplusplus
#endif