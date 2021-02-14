#include "Wrapper.h"
#include "../main/Mediator.h"


#pragma region WrapperBind

typedef struct WrapperHead
{
	UnityObjectHead objHead;
	int32_t handle;
}WrapperHead;

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
	if (exc != NULL)
	{
		debug_il2cpp_obj((Il2CppObject*)exc);
	}
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
		call_wrapper_init(il2cpp, mono);
	}
	return mono;
}

#pragma endregion

MonoMethod* get_mono_function(Il2CppObject* obj, Il2CppString* name, int param_count)
{
	MonoObject* mono_obj = get_mono_wrapper_object(obj, NULL);
	const char* func_name = mono_string_to_utf8((MonoString*)name);
	if (mono_obj == NULL)
	{
		mono_obj = get_mono_object(obj, NULL);
		if (mono_obj == NULL)
			return NULL;
	}

	MonoClass* kclass = mono_object_get_class(mono_obj);
	//MonoMethod* method = mono_class_get_method_from_name(kclass, func_name, param_count);
	MonoMethod* method = mono_lookup_method(func_name, kclass, param_count);
	return method;
}

void dispose_wrapp_object(Il2CppObject* il2cpp)
{
	if (il2cpp == NULL)
		return NULL;
	WrapperHead* il2cppHead = (WrapperHead*)(il2cpp);

	int32_t curHandle = il2cppHead->handle;

	if (curHandle != 0)
	{
		mono_gchandle_free(curHandle);
		il2cppHead->handle = 0;
	}
}

#pragma region MonoBehaviourWrapper

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

void InvokeMonoBehaviourFunction(Il2CppObject* obj, void* methodPtr)
{
	//MonoObject * mono_obj = get_mono_object(obj, NULL);
	MonoObject* mono_obj = get_mono_wrapper_object(obj, NULL);
	MonoMethod* method = (MonoMethod*)methodPtr;

	MonoObject *exc = NULL;
	MonoObject* res = mono_runtime_invoke(method, mono_obj, NULL, &exc);

	if (exc != NULL)
	{
		int c = 0;
		//TODO: exception..
	}
}
#pragma endregion

#pragma region EnumeratorWrapper


Il2CppObject* create_il2cpp_enumerator_wrapper(MonoObject* mono)
{
	Il2CppClass* m_class = get_enumerator_wrapper_class();
	if (mono == NULL)
		return NULL;

	Il2CppObject* il2cpp = il2cpp_object_new(m_class);
	
	debug_mono_obj(mono);
	call_wrapper_init(il2cpp,mono);
	return il2cpp;
}

Il2CppClass* get_enumerator_wrapper_class()
{
	static Il2CppClass* enumerator_wrapper_class;
	if (enumerator_wrapper_class == NULL)
	{
		enumerator_wrapper_class = il2cpp_search_class("PureScript.dll", "PureScriptWrapper", "EnumeratorWrapper");
		//il2cpp_add_flag(enumerator_wrapper_class, CLASS_MASK_WRAPPER);
	}
	return enumerator_wrapper_class;
}


MonoClass* get_ienumerator_class()
{
	static MonoClass* ienumerator_class;
	if (ienumerator_class == NULL)
		ienumerator_class = mono_search_class("mscorlib.dll", "System.Collections", "IEnumerator");
	return ienumerator_class;
}


MonoClass* get_YieldInstruction_class()
{
	static MonoClass* kclass;
	if (kclass == NULL)
		kclass = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "YieldInstruction");

	return kclass;
}
MonoClass* get_CustomYieldInstruction_class()
{
	static MonoClass* kclass;
	if (kclass == NULL)
		kclass = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "CustomYieldInstruction");

	return kclass;
}

MonoClass* get_coroutine_class()
{
	static MonoClass* kclass;
	if (kclass == NULL)
		kclass = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "Coroutine");

	return kclass;
}
MonoClass* get_AsyncOperation_class()
{
	static MonoClass* kclass;
	if (kclass == NULL)
		kclass = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "AsyncOperation");

	return kclass;
}


Il2CppObject* invoke_enumerator_current(Il2CppObject* obj, void* methodPtr)
{
	MonoObject * monoObj = get_mono_wrapper_object(obj, NULL);
	if (monoObj == NULL)
		return NULL;

	MonoMethod* method = (MonoMethod*)methodPtr;

	if (monoObj)
		method = mono_object_get_virtual_method(monoObj, method);

	debug_il2cpp_obj(obj);
	debug_mono_obj(monoObj);
	debug_mono_method(method);

	MonoObject *exc = NULL;
	MonoObject* monoRes = mono_runtime_invoke(method, monoObj, NULL, &exc);

	if (exc != NULL)
	{
		int c = 0;
		//TODO: exception..
	}

	if (monoRes == NULL)
		return NULL;

	debug_mono_obj(monoRes);


	MonoClass* monoClass = mono_object_get_class(monoRes);
	Il2CppObject* il2cppRes = NULL;
	uint32_t flag = mono_class_get_flags(monoClass);
	//Coroutine / AsyncOperation 
	bool isCoroutine = mono_class_is_subclass_of(monoClass, get_coroutine_class(), FALSE);
	bool isAsyncOperation = mono_class_is_subclass_of(monoClass, get_AsyncOperation_class(), FALSE);
	if (isCoroutine || isAsyncOperation)
	{
		il2cppRes = get_il2cpp_object(monoRes);
		debug_il2cpp_obj(il2cppRes);
	}
	//YieldInstruction	WaitForEndOfFrame / WaitForFixedUpdate / WaitForSeconds	//  float m_Seconds;*/
	else if (mono_class_is_subclass_of(monoClass, get_YieldInstruction_class(), FALSE))
	{
		Il2CppClass* il2cppClass = get_il2cpp_class(monoClass);
		il2cppRes = il2cpp_object_new(il2cppClass);
		debug_il2cpp_obj(il2cppRes);

		const size_t headSize = sizeof(void*) * 2;
		int value_area = il2cpp_object_get_size(il2cppRes) - headSize;
		if (value_area > 0)
		{
			memcpy((char*)il2cppRes + headSize, (char*)monoRes + headSize, value_area);
		}
	}
	/* else if (mono_class_is_subclass_of(resClass, get_CustomYieldInstruction_class(), FALSE))
		{
			get_il2cpp_object()
		}*/
		//Enumerator // EnumeratorWrapper //CustomYieldInstruction // MoveNext
	else if (mono_class_is_subclass_of(monoClass, get_ienumerator_class(), TRUE))
	{
		il2cppRes = create_il2cpp_enumerator_wrapper(monoRes);//, get_enumerator_wrapper_class()
	}

	return il2cppRes;
}
#pragma endregion

bool invoke_enumerator_moveNext(Il2CppObject* obj, void* methodPtr)
{
	MonoObject * monoObj = get_mono_wrapper_object(obj, NULL);
	if (monoObj == NULL || methodPtr == NULL)
		return TRUE;

	debug_mono_obj(monoObj);

	MonoMethod* method = (MonoMethod*)methodPtr;

	MonoObject *exc = NULL;

	if (monoObj)
		method = mono_object_get_virtual_method(monoObj, method);

	MonoObject* monoRes = mono_runtime_invoke(method, monoObj, NULL, &exc);


	if (exc != NULL)
	{
		unhandled_exception_handler(exc, NULL);
		int c = 0;
		//TODO: exception..
	}

	bool value = *(bool*)mono_object_unbox(monoRes);

	return value;
}

void invoke_enumerator_reset(Il2CppObject* obj, void* methodPtr)
{
	MonoObject * monoObj = get_mono_wrapper_object(obj, NULL);
	if (monoObj == NULL)
		return;

	MonoMethod* method = (MonoMethod*)methodPtr;

	MonoObject *exc = NULL;
	MonoObject* monoRes = mono_runtime_invoke(method, monoObj, NULL, &exc);

	if (exc != NULL)
	{
		int c = 0;
		//TODO: exception..
	}
}


void init_wrapper()
{
	il2cpp_add_internal_call("PureScriptWrapper.WrapperUtils::GetFuncPtr", (Il2CppMethodPointer)get_mono_function);
	il2cpp_add_internal_call("PureScriptWrapper.WrapperUtils::Dispose", (Il2CppMethodPointer)dispose_wrapp_object);

	il2cpp_add_internal_call("PureScriptWrapper.MonoBehaviourWrapper::InvokeFunction", (Il2CppMethodPointer)InvokeMonoBehaviourFunction);

	il2cpp_add_internal_call("PureScriptWrapper.EnumeratorWrapper::InvokeCurrent", (Il2CppMethodPointer)invoke_enumerator_current);
	il2cpp_add_internal_call("PureScriptWrapper.EnumeratorWrapper::InvokeMoveNext", (Il2CppMethodPointer)invoke_enumerator_moveNext);
	il2cpp_add_internal_call("PureScriptWrapper.EnumeratorWrapper::InvokeReset", (Il2CppMethodPointer)invoke_enumerator_reset);
}

