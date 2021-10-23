#include "engine_include.h"
#include "wrapper.h"

MonoObject* MonoGetObject(void* ptr)
{
	Il2CppObject * i2Obj = (Il2CppObject *)ptr;
	 return get_mono_object_impl(i2Obj, NULL,TRUE);
}

void* MonoStoreObject(MonoObject* obj, void* ptr)
{
	if (ptr == NULL)
	{
		Il2CppClass* i2Class = get_il2cpp_class(mono_object_get_class(obj));
		ptr = get_il2cpp_object(obj, i2Class);
		if (ptr == NULL)
			return NULL;
	}

	bind_mono_il2cpp_object(obj, (Il2CppObject*)ptr);
	return ptr;
}

Il2CppObject* Il2cppGetObject(void* ptr)
{
	return (Il2CppObject *)ptr;
}
void* Il2cppGetObjectPtr(Il2CppObject * obj)
{
	return obj;
}



/*
MonoObject* NewObject(MonoReflectionType* type)
{
	MonoType* monoType = mono_reflection_type_get_type(type);
	MonoClass * mclass = mono_class_from_mono_type(monoType);
	MonoObject* monoObj = mono_object_new(mono_domain_get(), mclass);
	return monoObj;
}*/

/*
MonoObject* UnityEngine_GameObject_CreatePrimitive(int32_t type)
{

	typedef Il2CppObject * (*ICallMethod) (int32_t);
	static ICallMethod icall;
	if (!icall)
		icall = (ICallMethod)il2cpp_resolve_icall("UnityEngine.GameObject::CreatePrimitive");
	Il2CppObject* il2cppObj = icall(type);
	
	MonoClass* m_class = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "GameObject");

	MonoObject* monoObj = get_mono_object(il2cppObj, m_class);
	return monoObj;
}*/

/*
void UnityEngine_DebugLogHandler_Internal_Log(int32_t logType, MonoString* str, MonoObject* obj)
{
	typedef void (*ICallMethod) (int32_t, Il2CppString*, Il2CppObject *);
	static ICallMethod icall;
	if (!icall)
		icall = (ICallMethod)il2cpp_resolve_icall("UnityEngine.DebugLogHandler::Internal_Log(UnityEngine.LogType,System.String,UnityEngine.Object)");
	icall(logType, get_il2cpp_string(str),obj);
	return ;
}*/

/*
MonoObject* UnityEngine_GameObject_get_transform(MonoObject* monoObj)
{
	typedef Il2CppObject * (*GameObject_get_transform) (Il2CppObject *);
	static GameObject_get_transform icall;
	if(!icall)
		icall = (GameObject_get_transform)il2cpp_resolve_icall("UnityEngine.GameObject::get_transform()");

	Il2CppObject * il2cppObj = get_il2cpp_object(monoObj);
	Il2CppObject * newObj = icall(il2cppObj);

	MonoClass* m_class = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "Transform");
	MonoObject* newMonoObj = get_mono_object(newObj, m_class);
	return newMonoObj;
}*/
/*
MonoObject* UnityEngine_Component_get_transform(MonoObject* monoObj)
{
	typedef Il2CppObject * (*Component_get_transform) (Il2CppObject *);
	static Component_get_transform icall;
	if (!icall)
		icall = (Component_get_transform)il2cpp_resolve_icall("UnityEngine.Component::get_transform()");

	Il2CppObject * il2cppObj = get_il2cpp_object(monoObj);
	Il2CppObject * newObj = icall(il2cppObj);

	MonoClass* m_class = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "Transform");
	MonoObject* newMonoObj = get_mono_object(newObj, m_class);
	return newMonoObj;
}*/

/*
MonoObject* UnityEngine_Component_get_gameobject(MonoObject* monoObj)
{
	typedef Il2CppObject * (*Component_get_transform) (Il2CppObject *);
	static Component_get_transform icall;
	if (!icall)
		icall = (Component_get_transform)il2cpp_resolve_icall("UnityEngine.Component::get_gameObject");

	Il2CppObject * il2cppObj = get_il2cpp_object(monoObj);
	Il2CppObject * newObj = icall(il2cppObj);

	MonoClass* m_class = mono_search_class("UnityEngine.CoreModule.dll", "UnityEngine", "GameObject");
	MonoObject* newMonoObj = get_mono_object(newObj, m_class);
	return newMonoObj;
}*/

/*
void UnityEngine_Transform_set_position_Injected(MonoObject* _thiz ,void* pos)
{
	typedef void (*Transform_set_position) (Il2CppObject*, void*);
	static Transform_set_position icall;
	if (!icall)
		icall = (Transform_set_position)il2cpp_resolve_icall("UnityEngine.Transform::set_position_Injected(UnityEngine.Vector3&)");

	Il2CppObject* il2cppObj = get_il2cpp_object(_thiz);

	icall(il2cppObj, pos);
	return;
}*/

/*
void UnityEngine_Transform_get_position_Injected(MonoObject* _thiz, void* pos)
{
	typedef void(*Transform_get_position) (Il2CppObject*, void*);
	static Transform_get_position icall;
	if (!icall)
		icall = (Transform_get_position)il2cpp_resolve_icall("UnityEngine.Transform::get_position_Injected(UnityEngine.Vector3&)");

	Il2CppObject* il2cppObj = get_il2cpp_object(_thiz);

	icall(il2cppObj, pos);
	return;
}*/

/*
MonoString* UnityEngine_Object_GetName(MonoObject* obj)
{
	typedef Il2CppString* (*Transform_set_position) (Il2CppObject*);
	static Transform_set_position icall;
	if (!icall)
		icall = (Transform_set_position)il2cpp_resolve_icall("UnityEngine.Object::GetName(UnityEngine.Object)");

	Il2CppObject* il2cppObj = get_il2cpp_object(obj);
	Il2CppString* res = icall(il2cppObj);
	MonoString* monoStr = get_mono_string(res);
	return monoStr;
}*/

/*
float UnityEngine_Time_Get_deltaTime()
{
	typedef float (*Time_Get_deltaTime) ();
	static Time_Get_deltaTime icall;
	if (!icall)
		icall = (Time_Get_deltaTime)il2cpp_resolve_icall("UnityEngine.Time::get_deltaTime()");

	float res = icall();
	return res;
}*/
/*
float UnityEngine_Time_GetTime()
{
	typedef float(*Time_Get_deltaTime) ();
	static Time_Get_deltaTime icall;
	if (!icall)
		icall = (Time_Get_deltaTime)il2cpp_resolve_icall("UnityEngine.Time::get_time()");

	float res = icall();
	return res;
}*/

MonoObject* UnityEngine_GameObject_Internal_AddComponentWithType(MonoObject* obj, MonoReflectionType* type)
{
	typedef Il2CppObject* (*AddComponentWithType) (Il2CppObject*, Il2CppReflectionType*);
	static AddComponentWithType icall;
	if (!icall)
		icall = (AddComponentWithType)il2cpp_resolve_icall("UnityEngine.GameObject::Internal_AddComponentWithType");

	Il2CppObject* il2cppObj = get_il2cpp_object(obj,NULL);
	
	MonoType* monoType = mono_reflection_type_get_type(type);
	MonoClass * mclass = mono_class_from_mono_type(monoType);

	Il2CppReflectionType* il2cppType = get_il2cpp_reflection_type(type);

	Il2CppObject* res = icall(il2cppObj, il2cppType);

	MonoObject* monoObj = get_mono_object(res, mclass);

	return monoObj;
}

MonoArray* UnityEngine_GameObject_GetComponentsInternal(MonoObject* obj, MonoReflectionType* type, bool useSearchTypeAsArrayReturnType, bool recursive, bool includeInactive, bool reverse, MonoObject* resultList)
{
	typedef Il2CppArray* (*GetComponentsInternal) (Il2CppObject*, Il2CppReflectionType*, bool , bool , bool , bool , Il2CppObject* );
	static GetComponentsInternal icall;
	if (!icall)
		icall = (GetComponentsInternal)il2cpp_resolve_icall("UnityEngine.GameObject::GetComponentsInternal");

	Il2CppObject* il2cppObj = get_il2cpp_object(obj, NULL);
	Il2CppReflectionType* il2cppType = get_il2cpp_reflection_type(type);
	
	Il2CppArray* res = icall(il2cppObj, il2cppType, useSearchTypeAsArrayReturnType, recursive, includeInactive, reverse, NULL);//resultList

	MonoArray* monoRes = get_mono_array(res);
	return monoRes;
}

void UnityEngine_GameObject_GetComponentFastPath(MonoObject* obj, MonoReflectionType* type, intptr_t ptr)
{
	MonoType* monoType = mono_reflection_type_get_type(type);
	MonoClass * mclass = mono_class_from_mono_type(monoType);
	//ptr = *ptr;
	void* objPtr = (char*)ptr - sizeof(void*);
	//objPtr = NULL;

	MonoArray* objs = UnityEngine_GameObject_GetComponentsInternal(obj, type, TRUE, FALSE, TRUE, FALSE, NULL);

	if (objs != NULL)
	{
		size_t len = mono_array_length(objs);

		for (int i = 0; i < len; i++)
		{
			MonoObject* monoObj = mono_array_get(objs, MonoObject*, i);
			if (monoObj == NULL)
				continue;
			MonoClass * tclass = mono_object_get_class(monoObj);
			if (mono_object_isinst(monoObj, mclass))
			{
				mono_gc_wbarrier_generic_store(objPtr, monoObj);
				break;
			}
		}
	}
}

/*
bool UnityEngine_MonoBehaviour_IsObjectMonoBehaviour(MonoObject* obj)
{
	Il2CppObject* il2cpp = get_il2cpp_object(obj);

	typedef bool(*IsObjectMonoBehaviour) (Il2CppObject*);
	static IsObjectMonoBehaviour icall;
	if (!icall)
		icall = (IsObjectMonoBehaviour)il2cpp_resolve_icall("UnityEngine.MonoBehaviour::IsObjectMonoBehaviour");

	bool res = icall(il2cpp);
	return res;
}*/

MonoObject* UnityEngine_MonoBehaviour_StartCoroutineManaged2(MonoObject* obj,MonoObject* enumerator)
{
	Il2CppObject* il2cpp = get_il2cpp_object(obj, NULL);

	typedef Il2CppObject*(*StartCoroutineManaged2) (Il2CppObject*, Il2CppObject*);
	static StartCoroutineManaged2 icall;
	if (!icall)
		icall = (StartCoroutineManaged2)il2cpp_resolve_icall("UnityEngine.MonoBehaviour::StartCoroutineManaged2");

	Il2CppObject* il2cppEnum = create_il2cpp_enumerator_wrapper(enumerator);

	Il2CppObject* res = icall(il2cpp, il2cppEnum);

	MonoClass* m_class = get_coroutine_class();
	MonoObject* newMonoObj = get_mono_object(res, m_class);
	
	return newMonoObj;
}

void UnityEngine_Coroutine_ReleaseCoroutine(void* ptr)
{
	typedef void(*ReleaseCoroutine) (void*);
	static ReleaseCoroutine icall;
	if (!icall)
		icall = (ReleaseCoroutine)il2cpp_resolve_icall("UnityEngine.Coroutine::ReleaseCoroutine");

	/*Il2CppObject* il2cppObj = get_il2cpp_object_with_ptr(ptr);
	void* internal_ptr = get_il2cpp_internal_ptr(il2cppObj);*/

	icall(ptr);
	return;
}

/*
void UnityEngine_Application_SetLogCallbackDefined(bool defined)
{

	typedef void(*SetLogCallbackDefined) (bool);
	static SetLogCallbackDefined icall;
	if (!icall)
		icall = (SetLogCallbackDefined)il2cpp_resolve_icall("UnityEngine.Application::SetLogCallbackDefined");

	icall(defined);
}*/
/*
MonoString* UnityEngine_Application_get_persistentDataPath()
{

	typedef Il2CppString* (*get_persistentDataPath) ();
	static get_persistentDataPath icall;
	if (!icall)
		icall = (get_persistentDataPath)il2cpp_resolve_icall("UnityEngine.Application::get_persistentDataPath");

	Il2CppString* res = icall();
	MonoString* monoRes = get_mono_string(res);
	return monoRes;
}*/

void init_event();
void regist_icall_gen();
void mono_register_icall(void)
{
	init_il2cpp();
	init_wrapper();
	init_event();

	regist_icall_gen();
	

	il2cpp_add_internal_call("ObjectStore::GetObject", (Il2CppMethodPointer)Il2cppGetObject);
	il2cpp_add_internal_call("ObjectStore::GetObjectPtr", (Il2CppMethodPointer)Il2cppGetObjectPtr);

	mono_add_internal_call("ObjectStore::GetObject", MonoGetObject);
	mono_add_internal_call("ObjectStore::StoreObject", MonoStoreObject);

	//Aono_add_internal_call("UnityEngine.GameObject::CreatePrimitive", (void*)UnityEngine_GameObject_CreatePrimitive);
	//Aono_add_internal_call("UnityEngine.DebugLogHandler::Internal_Log", (void*)UnityEngine_DebugLogHandler_Internal_Log);
	//Aono_add_internal_call("UnityEngine.GameObject::get_transform", (void*)UnityEngine_GameObject_get_transform);
	//Aono_add_internal_call("UnityEngine.Component::get_transform", (void*)UnityEngine_Component_get_transform);
	//Aono_add_internal_call("UnityEngine.Component::get_gameObject", (void*)UnityEngine_Component_get_gameobject);

	//Aono_add_internal_call("UnityEngine.Transform::set_position_Injected", (void*)UnityEngine_Transform_set_position_Injected);
	//Aono_add_internal_call("UnityEngine.Transform::get_position_Injected", (void*)UnityEngine_Transform_get_position_Injected);

	//Aono_add_internal_call("UnityEngine.Object::GetName", (void*)UnityEngine_Object_GetName);
	//Aono_add_internal_call("UnityEngine.Time::get_deltaTime", (void*)UnityEngine_Time_Get_deltaTime);
	//Aono_add_internal_call("UnityEngine.Time::get_time", (void*)UnityEngine_Time_GetTime);
	
	
	mono_add_internal_call("UnityEngine.GameObject::Internal_AddComponentWithType", (void*)UnityEngine_GameObject_Internal_AddComponentWithType);
	mono_add_internal_call("UnityEngine.GameObject::GetComponentFastPath", (void*)UnityEngine_GameObject_GetComponentFastPath);
	mono_add_internal_call("UnityEngine.Component::GetComponentFastPath", (void*)UnityEngine_GameObject_GetComponentFastPath);
	mono_add_internal_call("UnityEngine.GameObject::GetComponentsInternal", (void*)UnityEngine_GameObject_GetComponentsInternal);

	//Aono_add_internal_call("UnityEngine.MonoBehaviour::IsObjectMonoBehaviour", (void*)UnityEngine_MonoBehaviour_IsObjectMonoBehaviour);
	mono_add_internal_call("UnityEngine.MonoBehaviour::StartCoroutineManaged2", (void*)UnityEngine_MonoBehaviour_StartCoroutineManaged2);
	mono_add_internal_call("UnityEngine.Coroutine::ReleaseCoroutine", (void*)UnityEngine_Coroutine_ReleaseCoroutine);
	
	//Aono_add_internal_call("UnityEngine.Application::SetLogCallbackDefined", (void*)UnityEngine_Application_SetLogCallbackDefined);
	//Aono_add_internal_call("UnityEngine.Application::get_persistentDataPath", (void*)UnityEngine_Application_get_persistentDataPath);
	

}
