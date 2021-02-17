#include "il2cpp_support.h"
#ifdef _WIN32
#include <Windows.h>
#endif



#define log_error(cat, msg, ...)




#if !defined(_WIN32)
void init_il2cpp() 
{
	
}
#else

#define DO_API(r, n, p)		_##n n
#include "il2cpp-api-functions.h"
#undef DO_API

void init_il2cpp()
{
	bool symbols_missing = FALSE;
	void* libHandle = LoadLibraryA("GameAssembly.dll");


#define DO_API(r, n, p)	\
	n = (_##n)GetProcAddress(libHandle, #n); \
	if (n == NULL) \
	{ \
			symbols_missing = TRUE; \
	}
#include "il2cpp-api-functions.h"
#undef DO_API

	if (symbols_missing)
	{
		log_error(LOG_DEFAULT, "Failed to load il2cpp");
	}
	//il2cpp_resolve_icall = (_il2cpp_resolve_icall)GetProcAddress(libHandle, "il2cpp_resolve_icall");
	//void* icall = il2cpp_resolve_icall("UnityEngine.GameObject::CreatePrimitive");

}
#endif

Il2CppImage* il2cpp_get_image(const char* assembly)
{
	Il2CppAssembly* mono_assembly = il2cpp_domain_assembly_open(il2cpp_domain_get(), assembly);

	//MonoAssemblyName* asm_name = mono_assembly_name_new(assembly);
	//MonoAssembly* mono_assembly = mono_assembly_loaded(asm_name);
	//mono_assembly_name_free(asm_name);

	 return il2cpp_assembly_get_image(mono_assembly);
}
Il2CppClass* il2cpp_get_class(Il2CppImage* image, const char* _namespace, const char* name)
{
	return il2cpp_class_from_name(image, _namespace, name);
}

Il2CppClass* il2cpp_search_class(const char* assembly, const char* _namespace,const char* name)
{
	Il2CppImage* image = il2cpp_get_image(assembly);
	Il2CppClass* klass = il2cpp_get_class(image, _namespace, name);
	
	return klass;
}

char*
il2cpp_array_addr_with_size(Il2CppArray *array, int32_t size, uintptr_t idx)
{
	const size_t kIl2CppSizeOfArray = 3 * sizeof(void*) + sizeof(uintptr_t);
	return ((char*)array) + kIl2CppSizeOfArray + size * idx;
}

typedef struct MethodHead
{
	Il2CppMethodPointer methodPtr;
}MethodHead;

Il2CppMethodPointer hook_method(const char* assembly, const char* _namespace, const char* name,const char* method,int32_t param_count, Il2CppMethodPointer hook)
{
	Il2CppClass* klass = il2cpp_search_class(assembly, _namespace, name);

	return hook_method2(klass, method, param_count, hook);
}

Il2CppMethodPointer hook_method2(Il2CppClass* klass, const char* method, int32_t param_count, Il2CppMethodPointer hook)
{
	MethodInfo* info = il2cpp_class_get_method_from_name(klass, method, param_count);//
	if (info == NULL)
		return NULL;

	MethodHead* mh = (MethodHead*)info;
	Il2CppMethodPointer orign = mh->methodPtr;
	mh->methodPtr = hook;
	return orign;
}

Il2CppClass* il2cpp_get_exception_class() 
{
	static Il2CppClass* klass = NULL;
	if (klass == NULL)
	{
		klass = il2cpp_get_class(il2cpp_get_corlib(), "System", "Exception");
	}
	return klass;
}

Il2CppObject * il2cpp_exception_property(Il2CppObject *obj, const char *name, char is_virtual)
{
	Il2CppObject *get = NULL;
	Il2CppObject *get_virt = NULL;
	Il2CppObject *exc = NULL;

	get = il2cpp_class_get_method_from_name(il2cpp_get_exception_class(), name, 0);
	if (get) {
		if (is_virtual) {
			get_virt = il2cpp_object_get_virtual_method(obj, get);
			if (get_virt)
				get = get_virt;
		}

		return (Il2CppObject *)il2cpp_runtime_invoke(get, obj, NULL, &exc);
	}

	return NULL;
}



