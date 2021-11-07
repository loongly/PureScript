#include "wrapper.h"
#include "engine_include.h"



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
		return ;
	WrapperHead* il2cppHead = (WrapperHead*)(il2cpp);

	int32_t curHandle = il2cppHead->handle;

	if (curHandle != 0)
	{
		mono_gchandle_free(curHandle);
		il2cppHead->handle = 0;
	}
}

#pragma region MonoBehaviourWrapper

void InvokeMonoBehaviourFunction(Il2CppObject* obj, void* methodPtr)
{
	//MonoObject * mono_obj = get_mono_object(obj, NULL);
	MonoObject* mono_obj = get_mono_wrapper_object(obj, NULL);
	MonoMethod* method = (MonoMethod*)methodPtr;

	MonoObject *exc = NULL;
	MonoObject* res = mono_runtime_invoke(method, mono_obj, NULL, &exc);

	check_mono_exception(exc);
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

	check_mono_exception(exc);

	if (monoRes == NULL)
		return NULL;

	debug_mono_obj(monoRes);

	MonoClass* monoClass = mono_object_get_class(monoRes);
	Il2CppObject* il2cppRes = NULL;

	//Enumerator // EnumeratorWrapper //CustomYieldInstruction // MoveNext
	if (mono_class_is_subclass_of(monoClass, get_ienumerator_class(), TRUE))
	{
		il2cppRes = create_il2cpp_enumerator_wrapper(monoRes);//, get_enumerator_wrapper_class()
	}
	else
	{
		const char* ns = mono_class_get_namespace(monoClass);
		if (is_unity_name_space(ns))
		{
			il2cppRes = get_il2cpp_object(monoRes, NULL);

			uint32_t handle = mono_gchandle_new(monoRes, FALSE); // resist gc until the next "moveNext" call
			WObjectHead* il2cppHead = (WObjectHead*)(obj);
			if (il2cppHead->objectPtr != NULL)
				mono_gchandle_free((uint32_t)il2cppHead->objectPtr);

			il2cppHead->objectPtr = (void*)handle; // save handle to an unused field

			debug_il2cpp_obj(il2cppRes);
		}
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

	WObjectHead* il2cppHead = (WObjectHead*)(obj); // now we can free the last 'Current' monoObj handle
	if (il2cppHead->objectPtr != NULL)
	{
		mono_gchandle_free((uint32_t)il2cppHead->objectPtr);
		il2cppHead->objectPtr = NULL;
	}

	MonoMethod* method = (MonoMethod*)methodPtr;

	MonoObject *exc = NULL;

	if (monoObj)
		method = mono_object_get_virtual_method(monoObj, method);

	MonoObject* monoRes = mono_runtime_invoke(method, monoObj, NULL, &exc);

	check_mono_exception(exc);

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

