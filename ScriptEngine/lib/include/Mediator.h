#ifndef ___marshal_h
#define ___marshal_h

#include "runtime.h"
#include "il2cpp_support.h"

#define CLASS_MASK_UNITY_NATIVE (1<<2)
#define CLASS_MASK_WRAPPER (1<<3)
#define ENABLE_DEBUG 0

#if defined(__cplusplus)
extern "C"
{
#endif // __cplusplus

	typedef struct WObjectHead
	{
		void* __align1;
		void* __align2;
		void* objectPtr;
	}WObjectHead;

    typedef struct WrapperHead
    {
        WObjectHead objHead;
        int32_t handle;
    }WrapperHead;

    
	void bind_mono_il2cpp_object(MonoObject* mono, Il2CppObject* il2cpp);
	MonoObject* get_mono_object_impl(Il2CppObject* il2cpp, MonoClass* m_class,bool decide_class);
	inline MonoObject* get_mono_object(Il2CppObject* il2cpp, MonoClass* m_class) {
		return get_mono_object_impl(il2cpp, m_class, FALSE);
	}
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

	//System.Type
	Il2CppReflectionType* get_il2cpp_reflection_type(MonoReflectionType * type);
	MonoReflectionType* get_mono_reflection_type(Il2CppReflectionType * type);

    //wrapper
    MonoObject* get_mono_wrapper_object(Il2CppObject* il2cpp, MonoClass* m_class);
    Il2CppClass* get_monobehaviour_wrapper_class();
    Il2CppReflectionType* get_monobehaviour_wrapper_rtype();
    void call_wrapper_init(Il2CppObject* il2cpp, MonoObject* mono);
	bool need_monobehaviour_wrap(const char* asm_name, MonoClass* m_class);

	//Exception
	MonoException* get_mono_exception(Il2CppException* il2cpp);
	Il2CppException* get_il2cpp_exception(MonoException* mono);
	void check_il2cpp_exception(Il2CppException* il2cpp);
	void check_mono_exception(MonoException* mono);
	void raise_mono_exception_runtime(const char* msg);
	void raise_il2cpp_exception_runtime(const char* msg);


	bool is_unity_name_space(const char* ns);

#if ENABLE_DEBUG
	const char* debug_mono_obj(MonoObject* obj);
	const char* debug_mono_method(MonoMethod* method);
	const char* debug_il2cpp_obj(Il2CppObject* obj);
#else
#define debug_mono_obj(obj)  
#define debug_mono_method(method)   
#define debug_il2cpp_obj(obj) 
#endif
  
	

	void insert_assembly_map(const char* src, const char* tar);
	void insert_reloadable(const char* assembly, const char* info);
	bool is_reloadable(const char* assembly);

#if defined(__cplusplus)
}
#endif // __cplusplus
#endif
