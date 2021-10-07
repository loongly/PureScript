#include "Mediator.h"
#include "mono/metadata/object.h"
#include "mono/metadata/environment.h"
#include "mono/utils/mono-publib.h"
#include "mono/utils/mono-logger.h"
#include "mono/metadata/assembly.h"
#include "mono/metadata/mono-debug.h"
#include "mono/metadata/exception.h"
#include "mono/metadata/debug-helpers.h"
#include "mono/metadata/object-forward.h"
#include "mono/metadata/object.h"

#include <map>
#include <string>

#include "runtime.h"
//#include "internals.h"
//#include "../custom/Wrapper.h"

bool is_unity_name_space(const char* ns)
{
	return strncmp(ns, "UnityEngine", 11) == 0;
}
bool is_wrapper_name_space(const char* ns)
{
	return strncmp(ns, "PureScriptWrapper", 17) == 0;
}


bool is_wrapper_class(Il2CppClass* klass)
{
	//return il2cpp_check_flag(klass, CLASS_MASK_WRAPPER);
	//return klass->initialized & CLASS_MASK_WRAPPER;
	const char* ns = il2cpp_class_get_namespace(klass);
	return is_wrapper_name_space(ns);
}

bool is_unity_native(MonoClass* klass)
{
	return true;
	//return mono_check_flag(klass, CLASS_MASK_UNITY_NATIVE);
	//return  klass->inited & CLASS_MASK_UNITY_NATIVE;
}


#pragma region ObjectBind

typedef struct  BindInfo
{
	uint32_t mono_handle;
	uint32_t i2_handle;
} BindInfo;

static MonoReferenceQueue* gc_queue;
typedef std::map<uint64_t, BindInfo> Il2cppObjMap;
static Il2cppObjMap s_il2cppMap;

void on_mono_object_gc(void* user_data)
{
	if (user_data == NULL)
		return;

	uint64_t il2cppHandle = (uint64_t)user_data;
	Il2cppObjMap::iterator res = s_il2cppMap.find(il2cppHandle);
	if (res != s_il2cppMap.end())
	{
		il2cpp_gchandle_free(res->second.i2_handle);
		mono_gchandle_free(res->second.mono_handle);
		s_il2cppMap.erase(res);
	}
}

void bind_mono_il2cpp_object(MonoObject* mono, Il2CppObject* il2cpp)
{
	uint32_t i2Handle = il2cpp_gchandle_new(il2cpp, FALSE);
	uint32_t monoHandle = mono_gchandle_new_weakref(mono, FALSE);

	BindInfo info;
	info.mono_handle = monoHandle;
	info.i2_handle = i2Handle;
	s_il2cppMap[(uint64_t)il2cpp] = info;

	if (is_unity_native(mono_object_get_class(mono)))
	{
		UnityObjectHead* monoHead = (UnityObjectHead*)(mono);
		if (monoHead->objectPtr == NULL)
			monoHead->objectPtr = il2cpp;
	}

	if (gc_queue == NULL)
		gc_queue = mono_gc_reference_queue_new(on_mono_object_gc);

	mono_gc_reference_queue_add(gc_queue, mono, il2cpp);
}


MonoObject* get_mono_object(Il2CppObject* il2cpp, MonoClass* m_class)
{
	if (il2cpp == NULL)
		return NULL;

	bool isWrapper = is_wrapper_class(il2cpp_object_get_class(il2cpp));
	if(isWrapper)
		return get_mono_wrapper_object(il2cpp, m_class);

	uint32_t monoHandle = 0;
	MonoObject* monoObj = NULL;
	
	uint64_t il2cppHandle = (uint64_t)il2cpp;
	Il2cppObjMap::iterator res = s_il2cppMap.find(il2cppHandle);
	if (res != s_il2cppMap.end())
		monoHandle = res->second.mono_handle;

	if (monoHandle != 0)
	{
		monoObj = mono_gchandle_get_target(monoHandle);
		if (monoObj == NULL)
			mono_gchandle_free(monoHandle);
	}
	if (monoObj == NULL && m_class != NULL)
	{
		monoObj = mono_object_new(g_domain, m_class);
		bind_mono_il2cpp_object(monoObj, il2cpp);
	}
	return monoObj;
}
Il2CppObject* get_il2cpp_object(MonoObject* mono, Il2CppClass* m_class)
{
	if (mono == NULL)
		return NULL;

	Il2CppObject* il2cpp = NULL;
	if (is_unity_native(mono_object_get_class(mono)))
	{
		UnityObjectHead* monoHead = (UnityObjectHead*)(mono);
		if (monoHead->objectPtr == NULL)
		{
			il2cpp = il2cpp_object_new(m_class);
			bind_mono_il2cpp_object(mono, il2cpp);
			return il2cpp;
			//return NULL;
		}

		return get_il2cpp_object_with_ptr(monoHead->objectPtr);
	}
	return NULL;
}

Il2CppObject* get_il2cpp_object_with_ptr(void* objPtr)
{
	if (objPtr == NULL)
		return NULL;

	return (Il2CppObject*)objPtr; // no move gc

	//uint32_t handle = (uint32_t)objPtr;
	//return il2cpp_gchandle_get_target(handle);
}

void* get_il2cpp_internal_ptr(Il2CppObject* obj)
{
	UnityObjectHead* head = (UnityObjectHead*)(obj);
	return head->objectPtr;
}
#pragma endregion



MonoString* get_mono_string(Il2CppString* str)
{
    if(str == NULL)
        return NULL;
	/*char* ptr = (char*)str;
	int32_t* lenPtr = (int32_t*)(ptr + sizeof(void *) * 2);
	int32_t len = *lenPtr;
	char* chars = (char*)(ptr + sizeof(void *) * 2 + sizeof(int32_t));*/

	int32_t len = il2cpp_string_length(str);
	Il2CppChar* chars = il2cpp_string_chars(str);

	return mono_string_new_utf16(g_domain, (mono_unichar2*)chars,len);
}
Il2CppString* get_il2cpp_string(MonoString* str)
{
    if(str == NULL)
        return NULL;
    
    int32_t len = mono_string_length(str);
    Il2CppChar* chars = (Il2CppChar* )mono_string_to_utf16(str);

    return  il2cpp_string_new_utf16(chars, len);

}

MonoArray* get_mono_array(Il2CppArray * array)
{
	MonoArray* monoArray = NULL;
	if (array == NULL)
		return monoArray;


	Il2CppClass* eklass = il2cpp_class_get_element_class(il2cpp_object_get_class((Il2CppObject*)array));
	MonoClass* monoklass = get_mono_class(eklass);
    if(monoklass == NULL)
    {
        Il2CppObject* il2cppObj = il2cpp_array_get(array, Il2CppObject*, 0);
        MonoObject* monoObj = get_mono_object(il2cppObj, NULL);
        if(monoObj != NULL)
            monoklass = mono_object_get_class(monoObj);
    }
    
	int32_t len = il2cpp_array_length(array);
	monoArray = mono_array_new(g_domain, monoklass, len);
	if (len == 0)
		return monoArray;

	for (int i = 0; i < len; i++)
	{
		Il2CppObject* il2cppObj = il2cpp_array_get(array, Il2CppObject*, i);
		MonoObject* monoObj = get_mono_object(il2cppObj, monoklass);
		mono_array_setref(monoArray, i,monoObj);
	}
	return monoArray;
}

Il2CppArray* get_il2cpp_array(MonoArray* array)
{
	Il2CppArray* i2Array = NULL;
	if (array == NULL)
		return i2Array;

	MonoClass *klass = mono_object_get_class((MonoObject*)array);
	MonoClass *eklass = mono_class_get_element_class(klass);
	Il2CppClass* i2klass = get_il2cpp_class(eklass);
	int32_t len = mono_array_length(array);
	i2Array = il2cpp_array_new(i2klass, len);
	if (len == 0)
		return i2Array;

	for (int i = 0; i < len; i++)
	{
		MonoObject* monoObj = mono_array_get(array, MonoObject*, i);
		Il2CppObject* i2Obj = get_il2cpp_object(monoObj, NULL);
		il2cpp_array_setref(i2Array, i, i2Obj);
	}
	return i2Array;
}

void copy_array_i2_mono(Il2CppArray* i2_array, MonoArray* mono_array)
{
	int32_t len0 = il2cpp_array_length(i2_array);
	int32_t len1 = mono_array_length(mono_array);

	MonoClass *klass = mono_object_get_class((MonoObject*)mono_array);
	MonoClass *eklass = mono_class_get_element_class(klass);

	for (int i = 0; i < len0 && i< len1; i++)
	{
		Il2CppObject* il2cppObj = il2cpp_array_get(i2_array, Il2CppObject*, i);
		MonoObject* monoObj = get_mono_object(il2cppObj, eklass);
		mono_array_setref(mono_array, i, monoObj);
	}
}

Il2CppClass* get_il2cpp_class(MonoClass* mclass)
{
	const char* ns = mono_class_get_namespace(mclass);
	const char* cname = mono_class_get_name(mclass);
	MonoImage* mimage = mono_class_get_image(mclass);
	const char* asmName = mono_image_get_name(mimage);

	Il2CppClass* il2cppClass = il2cpp_search_class(asmName, ns, cname);
	return il2cppClass;
}

MonoClass* get_mono_class(Il2CppClass* mclass) 
{
	const char* ns = il2cpp_class_get_namespace(mclass);
    if(is_wrapper_name_space(ns))
        return NULL;
	const char* cname = il2cpp_class_get_name(mclass);
	const Il2CppImage* mimage = il2cpp_class_get_image(mclass);
	const char* asmName = il2cpp_image_get_name(mimage);

	MonoClass* monoClass = mono_search_class(asmName, ns, cname);
	return monoClass;
}

Il2CppReflectionType* get_il2cpp_reflection_type(MonoReflectionType * type)
{
	MonoType* monoType = mono_reflection_type_get_type(type);
	MonoClass * mclass = mono_class_from_mono_type(monoType);

	const char* ns = mono_class_get_namespace(mclass);
	const char* cname = mono_class_get_name(mclass);

	if (!is_unity_name_space(ns))
		return get_monobehaviour_wrapper_rtype();//

	MonoImage* mimage = mono_class_get_image(mclass);
	const char* asmName = mono_image_get_name(mimage);
	//const char* asmName = "Assembly-CSharp.dll";//TODO:the asmName must be same in mono and il2cpp

	Il2CppClass* il2cppClass = il2cpp_search_class(asmName, ns, cname);

	const Il2CppType* ktype = il2cpp_class_get_type(il2cppClass);
	//Il2CppReflectionType* rtype = il2cpp::vm::Reflection::GetTypeObject(ktype);
	Il2CppReflectionType* rtype = (Il2CppReflectionType*)il2cpp_type_get_object(ktype);
	return rtype;
}

MonoReflectionType* get_mono_reflection_type(Il2CppReflectionType * type) 
{
	//TODO: get_mono_reflection_type
	return NULL;
}


MonoImage* mono_get_image(const char* assembly)
{
	MonoAssembly* mono_assembly = load_assembly(assembly, NULL);
	return mono_assembly_get_image(mono_assembly);
}

MonoClass* mono_get_class(MonoImage* image, const char* _namespace, const char* name)
{
	return mono_class_from_name(image, _namespace, name);
}

MonoClass* mono_search_class(const char* assembly, const char* _namespace,
	const char* name)
{
/*
	MonoAssembly* mono_assembly = load_assembly(assembly, NULL);

	//MonoAssemblyName* asm_name = mono_assembly_name_new(assembly);
	//MonoAssembly* mono_assembly = mono_assembly_loaded(asm_name);
	//mono_assembly_name_free(asm_name);

	if (mono_assembly == 0)
	{
		int c = 0;
		//TODO: exception..
	}*/

	MonoImage* image = mono_get_image(assembly);
	MonoClass* klass = mono_get_class(image, _namespace, name);

	if (klass == 0)
	{
		int c = 0;
		//TODO: exception..
	}

	return klass;
}


MonoMethod* mono_lookup_method(const char* method_name, MonoClass *kclass, int param_count)
{

	MonoMethod *method = NULL;
	MonoClass *clazz = kclass;
	while (clazz != NULL && method == NULL) {
		method = mono_class_get_method_from_name(clazz, method_name, param_count);
		if (method == NULL)
			clazz = mono_class_get_parent(clazz);
	}


	/*MonoMethodDesc* desc = mono_method_desc_new(method_name, / *include_namespace=* /FALSE);
	if (desc == NULL)
		return NULL;

	MonoMethod* method = mono_method_desc_search_in_class(desc, kclass);
	mono_method_desc_free(desc);*/

	if (!method)
	{
		int c = 0;
		//TODO: exception..
	}

	return method;
}

//Exception:
MonoException* get_mono_exception(Il2CppException* il2cpp)
{
	Il2CppString* trace = (Il2CppString*)il2cpp_exception_property((Il2CppObject*)il2cpp, "get_StackTrace", 1);
	Il2CppString* message = (Il2CppString*)il2cpp_exception_property((Il2CppObject*)il2cpp, "get_Message", 1);

	return mono_exception_from_name_two_strings(mono_get_corlib(), "System", "Exception", get_mono_string(message), get_mono_string(trace));
}
Il2CppException* get_il2cpp_exception(MonoException* mono)
{
	MonoString* trace = (MonoString*)mono_exception_property((MonoObject*)mono, "get_StackTrace", 1);
	MonoString* message = (MonoString*)mono_exception_property((MonoObject*)mono, "get_Message", 1);
	char *traceStr = mono_string_to_utf8(trace);
	char *msgStr = mono_string_to_utf8(message);
	std::string outputMsg = std::string(msgStr) +"\n"+ std::string(traceStr);
	Il2CppException* exc = il2cpp_exception_from_name_msg(il2cpp_get_corlib(), "System", "Exception", outputMsg.c_str());

	mono_free(traceStr);
	mono_free(msgStr);
	return	exc;
}

void check_il2cpp_exception(Il2CppException* il2cpp)
{
	if (il2cpp != NULL)
	{
		MonoException* mono = get_mono_exception(il2cpp);
		mono_raise_exception(mono);
	}
}

void check_mono_exception(MonoException* mono)
{
	if (mono != NULL)
	{
		Il2CppException* il2cpp = get_il2cpp_exception(mono);
		il2cpp_raise_exception(il2cpp);
	}
}

void raise_mono_exception_runtime(const char* msg)
{
	MonoException* exc = mono_exception_from_name_msg(mono_get_corlib(), "System", "Exception", msg);
	mono_raise_exception(exc);
	//mono_reraise_exception(exc);
}

void raise_il2cpp_exception_runtime(const char* msg)
{
	Il2CppException* exc = il2cpp_exception_from_name_msg(il2cpp_get_corlib(), "System", "Exception", msg);
	il2cpp_raise_exception(exc);
}

#pragma region WrapperBind


void call_wrapper_init(Il2CppObject* il2cpp, MonoObject* mono)
{
    Il2CppClass* klass = il2cpp_object_get_class(il2cpp);
    
    /*if (!is_wrapper_class(klass))
     return;*/
    
    WrapperHead* il2cppHead = (WrapperHead*)(il2cpp);
    if(il2cppHead->handle != 0)
        mono_gchandle_free(il2cppHead->handle);
    il2cppHead->handle = mono_gchandle_new(mono, FALSE);
    
    const char* ns = mono_class_get_namespace(mono_object_get_class(mono));
    if (klass == get_monobehaviour_wrapper_class())
    {
        UnityObjectHead* monoHead = (UnityObjectHead*)(mono);
        if (monoHead->objectPtr == NULL)
            monoHead->objectPtr = il2cpp;
    }
    
    const MethodInfo* method = il2cpp_class_get_method_from_name(klass, "Init", 0);
    
    //void* args[1] = { il2cpp_value_box(handle) };
    Il2CppException* exc = NULL;
    il2cpp_runtime_invoke(method, il2cpp, NULL, &exc);
    check_il2cpp_exception(exc);
}

//more efficient "get_mono_object" for wrapper
MonoObject* get_mono_wrapper_object(Il2CppObject* il2cpp, MonoClass* m_class)
{
    if (il2cpp == NULL)
        return NULL;
    WrapperHead* il2cppHead = (WrapperHead*)(il2cpp);
    MonoObject* mono = NULL;
    
    int32_t curHandle = il2cppHead->handle;
    
    if (curHandle != 0)
        mono = mono_gchandle_get_target(curHandle);
    
    if (mono == NULL && m_class != NULL)
    {
        mono = mono_object_new(g_domain, m_class);
        mono_runtime_object_init (mono);
        call_wrapper_init(il2cpp, mono);
    }
    return mono;
}

Il2CppClass* get_monobehaviour_wrapper_class()
{
    static Il2CppClass* monobehaviour_wrapper_class;
    if (monobehaviour_wrapper_class == NULL)
    {
        monobehaviour_wrapper_class = il2cpp_search_class("PureScript.dll", "PureScriptWrapper", "MonoBehaviourWrapper");
        //il2cpp_add_flag(monobehaviour_wrapper_class, CLASS_MASK_WRAPPER);
    }
    return monobehaviour_wrapper_class;
}

Il2CppReflectionType* get_monobehaviour_wrapper_rtype()
{
    static Il2CppReflectionType* monobehaviour_wrapper_rtype;
    if (monobehaviour_wrapper_rtype == NULL)
    {
        Il2CppClass* kclass = get_monobehaviour_wrapper_class();
        const Il2CppType* ktype = il2cpp_class_get_type(kclass);
        monobehaviour_wrapper_rtype = (Il2CppReflectionType*)il2cpp_type_get_object(ktype);
    }
    
    return monobehaviour_wrapper_rtype;
}
#pragma endregion

#pragma region assembly map

static std::map<std::string , const char* > assembly_map;

void insert_assembly_map(const char* src, const char* tar) 
{
	assembly_map[std::string(src)] = tar;
}

extern "C" const char* resolve_assembly(const char* request)
{
	std::map<std::string, const char*>::iterator res = assembly_map.find(std::string(request));
	if (res != assembly_map.end())
		return res->second;

	return request;
}

#pragma endregion


const char* debug_mono_obj(MonoObject* obj)
{
	if (obj == NULL)
		return NULL;

	int res_size = mono_object_get_size(obj);
	MonoClass *type = mono_object_get_class(obj);
	const char* type_name = mono_class_get_name(type);
	return type_name;
}

const char* debug_mono_method(MonoMethod* method)
{
	if (method == NULL)
		return NULL;

	const char* name = mono_method_get_name(method);
	char* fullname = mono_method_full_name(method,true);
	return fullname;
}

const char* debug_il2cpp_obj(Il2CppObject* obj)
{
	if (obj == NULL)
		return NULL;

	Il2CppClass *type = il2cpp_object_get_class(obj);
	const char* type_name = il2cpp_class_get_name(type);
	return type_name;
}


