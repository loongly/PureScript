#include "Marshal.h"
#include <mono/metadata/object.h>
#include <mono/metadata/environment.h>
#include <mono/utils/mono-publib.h>
#include <mono/utils/mono-logger.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/object-forward.h>
#include <mono/metadata/object.h>

#include <map>
#include <string>

#include "runtime.h"
#include "internals.h"
#include "../custom/Wrapper.h"
//#include "il2cpp-object-internals.h"


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
	return il2cpp_check_flag(klass, CLASS_MASK_WRAPPER);
	//return klass->initialized & CLASS_MASK_WRAPPER;
	//const char* ns = il2cpp_class_get_namespace(mclass);
	//return is_wrapper_name_space(ns);
}

bool is_unity_native(MonoClass* klass)
{
	return mono_check_flag(klass, CLASS_MASK_UNITY_NATIVE);
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
	}
	if (monoObj == NULL && m_class != NULL)
	{
		monoObj = mono_object_new(g_domain, m_class);
		bind_mono_il2cpp_object(monoObj, il2cpp);
	}
	return monoObj;
}
Il2CppObject* get_il2cpp_object(MonoObject* mono)
{
	if (mono == NULL)
		return NULL;

	if (is_unity_native(mono_object_get_class(mono)))
	{
		UnityObjectHead* monoHead = (UnityObjectHead*)(mono);
		if (monoHead->objectPtr == NULL)
			return NULL;

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
	return (Il2CppString*)str;
}

MonoArray* get_mono_array(Il2CppArray * array)
{
	MonoArray* monoArray = NULL;
	if (array == NULL)
		return monoArray;


	Il2CppClass* eklass = il2cpp_class_get_element_class(il2cpp_object_get_class((Il2CppObject*)array));
	MonoClass* monoklass = get_mono_class(eklass);
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
		Il2CppObject* i2Obj = get_il2cpp_object(monoObj);
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

/*
MonoClass* castClass = get_casthelp_class();
MonoObject* castObj = mono_value_box(g_domain, castClass, objPtr);
MonoClassField * field = mono_class_get_field_from_name(castClass, "t");
mono_field_set_value(castObj, field, monoObj);
MonoClass* get_casthelp_class() 
{
	MonoClass* klass = mono_embeddinator_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "CastHelper`1");
	return klass;
}*/



void call_wrapper_init(Il2CppObject* obj)
{
	Il2CppClass* klass = il2cpp_object_get_class(obj);

	/*const char* ns = il2cpp_class_get_namespace(kclass);
	if (!is_wrapper_name_space(ns))
		return;*/

	if (!is_wrapper_class(klass))
		return;

	const MethodInfo* method = il2cpp_class_get_method_from_name(klass, "Init", 0);

	Il2CppException* exc = NULL;
	il2cpp_runtime_invoke(method, obj, NULL, &exc);
	if (exc != NULL)
	{
		debug_il2cpp_obj((Il2CppObject*)exc);
	}
}

/*
void bind_monobehaviour_function(MonoClass * kclass, const char* func_name,  int method_index, Il2CppObject * il2cppObj)
{
	MonoMethod* method = mono_embeddinator_lookup_method(func_name, kclass);
	DataOnHead* scriptHead = dataOnHead(il2cppObj);
	scriptHead->methods[method_index] = method;
}

// NOTE: structure members match with "MonoBehaviourWrapper" Intptr field.  MUST NOT CHANGE ORDER. 
//static char* monobehaviour_func[] = { "Awake()","OnEnable()", "Start()","Update()", "OnDisable()","OnDestroy()", "OnApplicationQuit()" };

MonoObject* create_monobehaviour(MonoClass * kclass,Il2CppObject * il2cppObj)
{
	MonoObject* monoObj = get_mono_object(il2cppObj, kclass);
	const char* cname = mono_class_get_name(kclass);
	int func_len = sizeof(monobehaviour_func) / sizeof(char*);
	for (int i = 0; i < func_len; i++)
	{
		bind_monobehaviour_function(kclass, monobehaviour_func[i], i, il2cppObj);
	}
	return monoObj;
}*/



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


