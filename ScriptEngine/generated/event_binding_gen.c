
#include "event_binding.h"
#include "class_cache_gen.h"
EventMethodDesc methods[65];
void UnityEngine_AI_NavMesh_Internal_CallOnNavMeshPreUpdate(const MethodInfo* imethod) 
{
	const int index = 0;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_AnimatorOverrideController_OnInvalidateOverrideController(Il2CppObject* controller, const MethodInfo* imethod) 
{
	const int index = 1;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* controller, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monocontroller = get_mono_object(controller,mono_get_class_UnityEngine_AnimatorOverrideController());
	thunk(monocontroller,&exc);
	check_mono_exception(exc);
}
void UnityEngine_AudioSettings_InvokeOnAudioConfigurationChanged(bool deviceWasChanged, const MethodInfo* imethod) 
{
	const int index = 2;
	typedef void (* THUNK_METHOD EventMethod) (bool deviceWasChanged, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(deviceWasChanged,&exc);
	check_mono_exception(exc);
}
void UnityEngine_AudioClip_InvokePCMReaderCallback_Internal(Il2CppObject* thiz, void* data, const MethodInfo* imethod) 
{
	const int index = 3;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* thiz, void* data, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monothiz = get_mono_object(thiz,mono_get_class_UnityEngine_AudioClip());
	thunk(monothiz,data,&exc);
	check_mono_exception(exc);
}
void UnityEngine_AudioClip_InvokePCMSetPositionCallback_Internal(Il2CppObject* thiz, int32_t position, const MethodInfo* imethod) 
{
	const int index = 4;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* thiz, int32_t position, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monothiz = get_mono_object(thiz,mono_get_class_UnityEngine_AudioClip());
	thunk(monothiz,position,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Application_CallLowMemory(const MethodInfo* imethod) 
{
	const int index = 5;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_Application_CallLogCallback(Il2CppString* logString, Il2CppString* stackTrace, int32_t type, bool invokedOnMainThread, const MethodInfo* imethod) 
{
	const int index = 6;
	typedef void (* THUNK_METHOD EventMethod) (MonoString* logString, MonoString* stackTrace, int32_t type, bool invokedOnMainThread, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monologString = get_mono_string(logString);
	MonoString* monostackTrace = get_mono_string(stackTrace);
	thunk(monologString,monostackTrace,type,invokedOnMainThread,&exc);
	check_mono_exception(exc);
}
bool UnityEngine_Application_Internal_ApplicationWantsToQuit(const MethodInfo* imethod) 
{
	const int index = 7;
	typedef bool (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	bool res = thunk(&exc);
	check_mono_exception(exc);
	return res;
}
void UnityEngine_Application_Internal_ApplicationQuit(const MethodInfo* imethod) 
{
	const int index = 8;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_Application_InvokeFocusChanged(bool focus, const MethodInfo* imethod) 
{
	const int index = 9;
	typedef void (* THUNK_METHOD EventMethod) (bool focus, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(focus,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Application_InvokeDeepLinkActivated(Il2CppString* url, const MethodInfo* imethod) 
{
	const int index = 10;
	typedef void (* THUNK_METHOD EventMethod) (MonoString* url, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monourl = get_mono_string(url);
	thunk(monourl,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Camera_FireOnPreCull(Il2CppObject* cam, const MethodInfo* imethod) 
{
	const int index = 11;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* cam, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monocam = get_mono_object(cam,mono_get_class_UnityEngine_Camera());
	thunk(monocam,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Camera_FireOnPreRender(Il2CppObject* cam, const MethodInfo* imethod) 
{
	const int index = 12;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* cam, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monocam = get_mono_object(cam,mono_get_class_UnityEngine_Camera());
	thunk(monocam,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Camera_FireOnPostRender(Il2CppObject* cam, const MethodInfo* imethod) 
{
	const int index = 13;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* cam, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monocam = get_mono_object(cam,mono_get_class_UnityEngine_Camera());
	thunk(monocam,&exc);
	check_mono_exception(exc);
}
void UnityEngine_CullingGroup_SendEvents(Il2CppObject* cullingGroup, void * eventsPtr, int32_t count, const MethodInfo* imethod) 
{
	const int index = 14;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* cullingGroup, void * eventsPtr, int32_t count, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monocullingGroup = get_mono_object(cullingGroup,mono_get_class_UnityEngine_CullingGroup());
	thunk(monocullingGroup,eventsPtr,count,&exc);
	check_mono_exception(exc);
}
void UnityEngine_ReflectionProbe_CallReflectionProbeEvent(Il2CppObject* probe, int32_t probeEvent, const MethodInfo* imethod) 
{
	const int index = 15;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* probe, int32_t probeEvent, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monoprobe = get_mono_object(probe,mono_get_class_UnityEngine_ReflectionProbe());
	thunk(monoprobe,probeEvent,&exc);
	check_mono_exception(exc);
}
void UnityEngine_ReflectionProbe_CallSetDefaultReflection(Il2CppObject* defaultReflectionCubemap, const MethodInfo* imethod) 
{
	const int index = 16;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* defaultReflectionCubemap, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monodefaultReflectionCubemap = get_mono_object(defaultReflectionCubemap,mono_get_class_UnityEngine_Cubemap());
	thunk(monodefaultReflectionCubemap,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Display_FireDisplaysUpdated(const MethodInfo* imethod) 
{
	const int index = 17;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_AsyncOperation_InvokeCompletionEvent(Il2CppObject* thiz, const MethodInfo* imethod) 
{
	const int index = 18;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* thiz, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monothiz = get_mono_object(thiz,mono_get_class_UnityEngine_AsyncOperation());
	thunk(monothiz,&exc);
	check_mono_exception(exc);
}
void UnityEngine_RectTransform_SendReapplyDrivenProperties(Il2CppObject* driven, const MethodInfo* imethod) 
{
	const int index = 19;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* driven, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monodriven = get_mono_object(driven,mono_get_class_UnityEngine_RectTransform());
	thunk(monodriven,&exc);
	check_mono_exception(exc);
}
bool UnityEngine_U2D_SpriteAtlasManager_RequestAtlas(Il2CppString* tag, const MethodInfo* imethod) 
{
	const int index = 20;
	typedef bool (* THUNK_METHOD EventMethod) (MonoString* tag, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monotag = get_mono_string(tag);
	bool res = thunk(monotag,&exc);
	check_mono_exception(exc);
	return res;
}
void UnityEngine_U2D_SpriteAtlasManager_PostRegisteredAtlas(Il2CppObject* spriteAtlas, const MethodInfo* imethod) 
{
	const int index = 21;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* spriteAtlas, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monospriteAtlas = get_mono_object(spriteAtlas,mono_get_class_UnityEngine_U2D_SpriteAtlas());
	thunk(monospriteAtlas,&exc);
	check_mono_exception(exc);
}
void* UnityEngine_Profiling_Memory_Experimental_MemoryProfiler_PrepareMetadata(const MethodInfo* imethod) 
{
	const int index = 22;
	typedef void* (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	void* res = thunk(&exc);
	check_mono_exception(exc);
	return res;
}
void UnityEngine_Profiling_Memory_Experimental_MemoryProfiler_FinalizeSnapshot(Il2CppString* path, bool result, const MethodInfo* imethod) 
{
	const int index = 23;
	typedef void (* THUNK_METHOD EventMethod) (MonoString* path, bool result, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monopath = get_mono_string(path);
	thunk(monopath,result,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Profiling_Memory_Experimental_MemoryProfiler_SaveScreenshotToDisk(Il2CppString* path, bool result, void * pixelsPtr, int32_t pixelsCount, int32_t format, int32_t width, int32_t height, const MethodInfo* imethod) 
{
	const int index = 24;
	typedef void (* THUNK_METHOD EventMethod) (MonoString* path, bool result, void * pixelsPtr, int32_t pixelsCount, int32_t format, int32_t width, int32_t height, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monopath = get_mono_string(path);
	thunk(monopath,result,pixelsPtr,pixelsCount,format,width,height,&exc);
	check_mono_exception(exc);
}
void UnityEngine_SceneManagement_SceneManager_Internal_SceneLoaded(void * scene, int32_t mode, const MethodInfo* imethod) 
{
	const int index = 25;
	typedef void (* THUNK_METHOD EventMethod) (void * scene, int32_t mode, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(scene,mode,&exc);
	check_mono_exception(exc);
}
void UnityEngine_SceneManagement_SceneManager_Internal_SceneUnloaded(void * scene, const MethodInfo* imethod) 
{
	const int index = 26;
	typedef void (* THUNK_METHOD EventMethod) (void * scene, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(scene,&exc);
	check_mono_exception(exc);
}
void UnityEngine_SceneManagement_SceneManager_Internal_ActiveSceneChanged(void * previousActiveScene, void * newActiveScene, const MethodInfo* imethod) 
{
	const int index = 27;
	typedef void (* THUNK_METHOD EventMethod) (void * previousActiveScene, void * newActiveScene, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(previousActiveScene,newActiveScene,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Rendering_BatchRendererGroup_InvokeOnPerformCulling(Il2CppObject* group, void * context, void * lodParameters, const MethodInfo* imethod) 
{
	const int index = 28;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* group, void * context, void * lodParameters, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monogroup = get_mono_object(group,mono_get_class_UnityEngine_Rendering_BatchRendererGroup());
	thunk(monogroup,context,lodParameters,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Experimental_GlobalIllumination_Lightmapping_RequestLights(Il2CppArray* lights, void * outLightsPtr, int32_t outLightsCount, const MethodInfo* imethod) 
{
	const int index = 29;
	typedef void (* THUNK_METHOD EventMethod) (MonoArray* lights, void * outLightsPtr, int32_t outLightsCount, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoArray* monolights = get_mono_array(lights);
	thunk(monolights,outLightsPtr,outLightsCount,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Playables_PlayableDirector_SendOnPlayableDirectorPlay(Il2CppObject* thiz, const MethodInfo* imethod) 
{
	const int index = 30;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* thiz, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monothiz = get_mono_object(thiz,mono_get_class_UnityEngine_Playables_PlayableDirector());
	thunk(monothiz,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Playables_PlayableDirector_SendOnPlayableDirectorPause(Il2CppObject* thiz, const MethodInfo* imethod) 
{
	const int index = 31;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* thiz, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monothiz = get_mono_object(thiz,mono_get_class_UnityEngine_Playables_PlayableDirector());
	thunk(monothiz,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Playables_PlayableDirector_SendOnPlayableDirectorStop(Il2CppObject* thiz, const MethodInfo* imethod) 
{
	const int index = 32;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* thiz, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monothiz = get_mono_object(thiz,mono_get_class_UnityEngine_Playables_PlayableDirector());
	thunk(monothiz,&exc);
	check_mono_exception(exc);
}
void UnityEngine_GUIUtility_MarkGUIChanged(const MethodInfo* imethod) 
{
	const int index = 33;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_GUIUtility_TakeCapture(const MethodInfo* imethod) 
{
	const int index = 34;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_GUIUtility_RemoveCapture(const MethodInfo* imethod) 
{
	const int index = 35;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
bool UnityEngine_GUIUtility_ProcessEvent(int32_t instanceID, void * nativeEventPtr, const MethodInfo* imethod) 
{
	const int index = 36;
	typedef bool (* THUNK_METHOD EventMethod) (int32_t instanceID, void * nativeEventPtr, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	bool res = thunk(instanceID,nativeEventPtr,&exc);
	check_mono_exception(exc);
	return res;
}
bool UnityEngine_GUIUtility_EndContainerGUIFromException(Il2CppObject* exception, const MethodInfo* imethod) 
{
	const int index = 37;
	typedef bool (* THUNK_METHOD EventMethod) (MonoObject* exception, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monoexception = get_mono_object(exception,mono_get_class_System_Exception());
	bool res = thunk(monoexception,&exc);
	check_mono_exception(exc);
	return res;
}
void UnityEngineInternal_Input_NativeInputSystem_NotifyBeforeUpdate(int32_t updateType, const MethodInfo* imethod) 
{
	const int index = 38;
	typedef void (* THUNK_METHOD EventMethod) (int32_t updateType, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(updateType,&exc);
	check_mono_exception(exc);
}
void UnityEngineInternal_Input_NativeInputSystem_NotifyUpdate(int32_t updateType, void * eventBuffer, const MethodInfo* imethod) 
{
	const int index = 39;
	typedef void (* THUNK_METHOD EventMethod) (int32_t updateType, void * eventBuffer, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(updateType,eventBuffer,&exc);
	check_mono_exception(exc);
}
void UnityEngineInternal_Input_NativeInputSystem_NotifyDeviceDiscovered(int32_t deviceId, Il2CppString* deviceDescriptor, const MethodInfo* imethod) 
{
	const int index = 40;
	typedef void (* THUNK_METHOD EventMethod) (int32_t deviceId, MonoString* deviceDescriptor, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monodeviceDescriptor = get_mono_string(deviceDescriptor);
	thunk(deviceId,monodeviceDescriptor,&exc);
	check_mono_exception(exc);
}
void UnityEngineInternal_Input_NativeInputSystem_ShouldRunUpdate(int32_t updateType, void * retval, const MethodInfo* imethod) 
{
	const int index = 41;
	typedef void (* THUNK_METHOD EventMethod) (int32_t updateType, void * retval, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(updateType,retval,&exc);
	check_mono_exception(exc);
}
void UnityEngine_SubsystemManager_Internal_ReloadSubsystemsStarted(const MethodInfo* imethod) 
{
	const int index = 42;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_SubsystemManager_Internal_ReloadSubsystemsCompleted(const MethodInfo* imethod) 
{
	const int index = 43;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_Experimental_TerrainAPI_TerrainCallbacks_InvokeHeightmapChangedCallback(Il2CppObject* terrainData, void * heightRegion, bool synched, const MethodInfo* imethod) 
{
	const int index = 44;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* terrainData, void * heightRegion, bool synched, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monoterrainData = get_mono_object(terrainData,mono_get_class_UnityEngine_TerrainData());
	thunk(monoterrainData,heightRegion,synched,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Experimental_TerrainAPI_TerrainCallbacks_InvokeTextureChangedCallback(Il2CppObject* terrainData, Il2CppString* textureName, void * texelRegion, bool synched, const MethodInfo* imethod) 
{
	const int index = 45;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* terrainData, MonoString* textureName, void * texelRegion, bool synched, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monoterrainData = get_mono_object(terrainData,mono_get_class_UnityEngine_TerrainData());
	MonoString* monotextureName = get_mono_string(textureName);
	thunk(monoterrainData,monotextureName,texelRegion,synched,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Font_InvokeTextureRebuilt_Internal(Il2CppObject* font, const MethodInfo* imethod) 
{
	const int index = 46;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* font, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monofont = get_mono_object(font,mono_get_class_UnityEngine_Font());
	thunk(monofont,&exc);
	check_mono_exception(exc);
}
void UnityEngine_UIElements_UIR_Utility_RaiseGraphicsResourcesRecreate(bool recreate, const MethodInfo* imethod) 
{
	const int index = 47;
	typedef void (* THUNK_METHOD EventMethod) (bool recreate, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(recreate,&exc);
	check_mono_exception(exc);
}
void UnityEngine_UIElements_UIR_Utility_RaiseEngineUpdate(const MethodInfo* imethod) 
{
	const int index = 48;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_UIElements_UIR_Utility_RaiseFlushPendingResources(const MethodInfo* imethod) 
{
	const int index = 49;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_Canvas_SendWillRenderCanvases(const MethodInfo* imethod) 
{
	const int index = 50;
	typedef void (* THUNK_METHOD EventMethod) (MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(&exc);
	check_mono_exception(exc);
}
void UnityEngine_XR_XRDevice_InvokeDeviceLoaded(Il2CppString* loadedDeviceName, const MethodInfo* imethod) 
{
	const int index = 51;
	typedef void (* THUNK_METHOD EventMethod) (MonoString* loadedDeviceName, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoString* monoloadedDeviceName = get_mono_string(loadedDeviceName);
	thunk(monoloadedDeviceName,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokePrepareCompletedCallback_Internal(Il2CppObject* source, const MethodInfo* imethod) 
{
	const int index = 52;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeFrameReadyCallback_Internal(Il2CppObject* source, int64_t frameIdx, const MethodInfo* imethod) 
{
	const int index = 53;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, int64_t frameIdx, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,frameIdx,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeLoopPointReachedCallback_Internal(Il2CppObject* source, const MethodInfo* imethod) 
{
	const int index = 54;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeStartedCallback_Internal(Il2CppObject* source, const MethodInfo* imethod) 
{
	const int index = 55;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeFrameDroppedCallback_Internal(Il2CppObject* source, const MethodInfo* imethod) 
{
	const int index = 56;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeErrorReceivedCallback_Internal(Il2CppObject* source, Il2CppString* errorStr, const MethodInfo* imethod) 
{
	const int index = 57;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, MonoString* errorStr, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	MonoString* monoerrorStr = get_mono_string(errorStr);
	thunk(monosource,monoerrorStr,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeSeekCompletedCallback_Internal(Il2CppObject* source, const MethodInfo* imethod) 
{
	const int index = 58;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,&exc);
	check_mono_exception(exc);
}
void UnityEngine_Video_VideoPlayer_InvokeClockResyncOccurredCallback_Internal(Il2CppObject* source, double seconds, const MethodInfo* imethod) 
{
	const int index = 59;
	typedef void (* THUNK_METHOD EventMethod) (MonoObject* source, double seconds, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	MonoObject* monosource = get_mono_object(source,mono_get_class_UnityEngine_Video_VideoPlayer());
	thunk(monosource,seconds,&exc);
	check_mono_exception(exc);
}
void UnityEngine_XR_InputTracking_InvokeTrackingEvent(int32_t eventType, int32_t nodeType, int64_t uniqueID, bool tracked, const MethodInfo* imethod) 
{
	const int index = 60;
	typedef void (* THUNK_METHOD EventMethod) (int32_t eventType, int32_t nodeType, int64_t uniqueID, bool tracked, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(eventType,nodeType,uniqueID,tracked,&exc);
	check_mono_exception(exc);
}
void UnityEngine_XR_InputDevices_InvokeConnectionEvent(uint64_t deviceId, int32_t change, const MethodInfo* imethod) 
{
	const int index = 61;
	typedef void (* THUNK_METHOD EventMethod) (uint64_t deviceId, int32_t change, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(deviceId,change,&exc);
	check_mono_exception(exc);
}
void UnityEngine_XR_XRDisplaySubsystem_InvokeDisplayFocusChanged(bool focus, const MethodInfo* imethod) 
{
	const int index = 62;
	typedef void (* THUNK_METHOD EventMethod) (bool focus, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(focus,&exc);
	check_mono_exception(exc);
}
void UnityEngine_XR_XRInputSubsystem_InvokeTrackingOriginUpdatedEvent(void * internalPtr, const MethodInfo* imethod) 
{
	const int index = 63;
	typedef void (* THUNK_METHOD EventMethod) (void * internalPtr, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(internalPtr,&exc);
	check_mono_exception(exc);
}
void UnityEngine_XR_XRInputSubsystem_InvokeBoundaryChangedEvent(void * internalPtr, const MethodInfo* imethod) 
{
	const int index = 64;
	typedef void (* THUNK_METHOD EventMethod) (void * internalPtr, MonoException** exc);
	static EventMethod thunk;
	if(!thunk)
		thunk = mono_method_get_unmanaged_thunk(methods[index].hooked);
	MonoException *exc = NULL;
	thunk(internalPtr,&exc);
	check_mono_exception(exc);
}
void init_event_gen()
{
	init_event_method(&methods[0],mono_get_class_UnityEngine_AI_NavMesh(),il2cpp_get_class_UnityEngine_AI_NavMesh(),"Internal_CallOnNavMeshPreUpdate",0,(Il2CppMethodPointer) UnityEngine_AI_NavMesh_Internal_CallOnNavMeshPreUpdate);
	init_event_method(&methods[1],mono_get_class_UnityEngine_AnimatorOverrideController(),il2cpp_get_class_UnityEngine_AnimatorOverrideController(),"OnInvalidateOverrideController",1,(Il2CppMethodPointer) UnityEngine_AnimatorOverrideController_OnInvalidateOverrideController);
	init_event_method(&methods[2],mono_get_class_UnityEngine_AudioSettings(),il2cpp_get_class_UnityEngine_AudioSettings(),"InvokeOnAudioConfigurationChanged",1,(Il2CppMethodPointer) UnityEngine_AudioSettings_InvokeOnAudioConfigurationChanged);
	init_event_method(&methods[3],mono_get_class_UnityEngine_AudioClip(),il2cpp_get_class_UnityEngine_AudioClip(),"InvokePCMReaderCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_AudioClip_InvokePCMReaderCallback_Internal);
	init_event_method(&methods[4],mono_get_class_UnityEngine_AudioClip(),il2cpp_get_class_UnityEngine_AudioClip(),"InvokePCMSetPositionCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_AudioClip_InvokePCMSetPositionCallback_Internal);
	init_event_method(&methods[5],mono_get_class_UnityEngine_Application(),il2cpp_get_class_UnityEngine_Application(),"CallLowMemory",0,(Il2CppMethodPointer) UnityEngine_Application_CallLowMemory);
	init_event_method(&methods[6],mono_get_class_UnityEngine_Application(),il2cpp_get_class_UnityEngine_Application(),"CallLogCallback",4,(Il2CppMethodPointer) UnityEngine_Application_CallLogCallback);
	init_event_method(&methods[7],mono_get_class_UnityEngine_Application(),il2cpp_get_class_UnityEngine_Application(),"Internal_ApplicationWantsToQuit",0,(Il2CppMethodPointer) UnityEngine_Application_Internal_ApplicationWantsToQuit);
	init_event_method(&methods[8],mono_get_class_UnityEngine_Application(),il2cpp_get_class_UnityEngine_Application(),"Internal_ApplicationQuit",0,(Il2CppMethodPointer) UnityEngine_Application_Internal_ApplicationQuit);
	init_event_method(&methods[9],mono_get_class_UnityEngine_Application(),il2cpp_get_class_UnityEngine_Application(),"InvokeFocusChanged",1,(Il2CppMethodPointer) UnityEngine_Application_InvokeFocusChanged);
	init_event_method(&methods[10],mono_get_class_UnityEngine_Application(),il2cpp_get_class_UnityEngine_Application(),"InvokeDeepLinkActivated",1,(Il2CppMethodPointer) UnityEngine_Application_InvokeDeepLinkActivated);
	init_event_method(&methods[11],mono_get_class_UnityEngine_Camera(),il2cpp_get_class_UnityEngine_Camera(),"FireOnPreCull",1,(Il2CppMethodPointer) UnityEngine_Camera_FireOnPreCull);
	init_event_method(&methods[12],mono_get_class_UnityEngine_Camera(),il2cpp_get_class_UnityEngine_Camera(),"FireOnPreRender",1,(Il2CppMethodPointer) UnityEngine_Camera_FireOnPreRender);
	init_event_method(&methods[13],mono_get_class_UnityEngine_Camera(),il2cpp_get_class_UnityEngine_Camera(),"FireOnPostRender",1,(Il2CppMethodPointer) UnityEngine_Camera_FireOnPostRender);
	init_event_method(&methods[14],mono_get_class_UnityEngine_CullingGroup(),il2cpp_get_class_UnityEngine_CullingGroup(),"SendEvents",3,(Il2CppMethodPointer) UnityEngine_CullingGroup_SendEvents);
	init_event_method(&methods[15],mono_get_class_UnityEngine_ReflectionProbe(),il2cpp_get_class_UnityEngine_ReflectionProbe(),"CallReflectionProbeEvent",2,(Il2CppMethodPointer) UnityEngine_ReflectionProbe_CallReflectionProbeEvent);
	init_event_method(&methods[16],mono_get_class_UnityEngine_ReflectionProbe(),il2cpp_get_class_UnityEngine_ReflectionProbe(),"CallSetDefaultReflection",1,(Il2CppMethodPointer) UnityEngine_ReflectionProbe_CallSetDefaultReflection);
	init_event_method(&methods[17],mono_get_class_UnityEngine_Display(),il2cpp_get_class_UnityEngine_Display(),"FireDisplaysUpdated",0,(Il2CppMethodPointer) UnityEngine_Display_FireDisplaysUpdated);
	init_event_method(&methods[18],mono_get_class_UnityEngine_AsyncOperation(),il2cpp_get_class_UnityEngine_AsyncOperation(),"InvokeCompletionEvent",0,(Il2CppMethodPointer) UnityEngine_AsyncOperation_InvokeCompletionEvent);
	init_event_method(&methods[19],mono_get_class_UnityEngine_RectTransform(),il2cpp_get_class_UnityEngine_RectTransform(),"SendReapplyDrivenProperties",1,(Il2CppMethodPointer) UnityEngine_RectTransform_SendReapplyDrivenProperties);
	init_event_method(&methods[20],mono_get_class_UnityEngine_U2D_SpriteAtlasManager(),il2cpp_get_class_UnityEngine_U2D_SpriteAtlasManager(),"RequestAtlas",1,(Il2CppMethodPointer) UnityEngine_U2D_SpriteAtlasManager_RequestAtlas);
	init_event_method(&methods[21],mono_get_class_UnityEngine_U2D_SpriteAtlasManager(),il2cpp_get_class_UnityEngine_U2D_SpriteAtlasManager(),"PostRegisteredAtlas",1,(Il2CppMethodPointer) UnityEngine_U2D_SpriteAtlasManager_PostRegisteredAtlas);
	init_event_method(&methods[22],mono_get_class_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler(),il2cpp_get_class_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler(),"PrepareMetadata",0,(Il2CppMethodPointer) UnityEngine_Profiling_Memory_Experimental_MemoryProfiler_PrepareMetadata);
	init_event_method(&methods[23],mono_get_class_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler(),il2cpp_get_class_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler(),"FinalizeSnapshot",2,(Il2CppMethodPointer) UnityEngine_Profiling_Memory_Experimental_MemoryProfiler_FinalizeSnapshot);
	init_event_method(&methods[24],mono_get_class_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler(),il2cpp_get_class_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler(),"SaveScreenshotToDisk",7,(Il2CppMethodPointer) UnityEngine_Profiling_Memory_Experimental_MemoryProfiler_SaveScreenshotToDisk);
	init_event_method(&methods[25],mono_get_class_UnityEngine_SceneManagement_SceneManager(),il2cpp_get_class_UnityEngine_SceneManagement_SceneManager(),"Internal_SceneLoaded",2,(Il2CppMethodPointer) UnityEngine_SceneManagement_SceneManager_Internal_SceneLoaded);
	init_event_method(&methods[26],mono_get_class_UnityEngine_SceneManagement_SceneManager(),il2cpp_get_class_UnityEngine_SceneManagement_SceneManager(),"Internal_SceneUnloaded",1,(Il2CppMethodPointer) UnityEngine_SceneManagement_SceneManager_Internal_SceneUnloaded);
	init_event_method(&methods[27],mono_get_class_UnityEngine_SceneManagement_SceneManager(),il2cpp_get_class_UnityEngine_SceneManagement_SceneManager(),"Internal_ActiveSceneChanged",2,(Il2CppMethodPointer) UnityEngine_SceneManagement_SceneManager_Internal_ActiveSceneChanged);
	init_event_method(&methods[28],mono_get_class_UnityEngine_Rendering_BatchRendererGroup(),il2cpp_get_class_UnityEngine_Rendering_BatchRendererGroup(),"InvokeOnPerformCulling",3,(Il2CppMethodPointer) UnityEngine_Rendering_BatchRendererGroup_InvokeOnPerformCulling);
	init_event_method(&methods[29],mono_get_class_UnityEngine_Experimental_GlobalIllumination_Lightmapping(),il2cpp_get_class_UnityEngine_Experimental_GlobalIllumination_Lightmapping(),"RequestLights",3,(Il2CppMethodPointer) UnityEngine_Experimental_GlobalIllumination_Lightmapping_RequestLights);
	init_event_method(&methods[30],mono_get_class_UnityEngine_Playables_PlayableDirector(),il2cpp_get_class_UnityEngine_Playables_PlayableDirector(),"SendOnPlayableDirectorPlay",0,(Il2CppMethodPointer) UnityEngine_Playables_PlayableDirector_SendOnPlayableDirectorPlay);
	init_event_method(&methods[31],mono_get_class_UnityEngine_Playables_PlayableDirector(),il2cpp_get_class_UnityEngine_Playables_PlayableDirector(),"SendOnPlayableDirectorPause",0,(Il2CppMethodPointer) UnityEngine_Playables_PlayableDirector_SendOnPlayableDirectorPause);
	init_event_method(&methods[32],mono_get_class_UnityEngine_Playables_PlayableDirector(),il2cpp_get_class_UnityEngine_Playables_PlayableDirector(),"SendOnPlayableDirectorStop",0,(Il2CppMethodPointer) UnityEngine_Playables_PlayableDirector_SendOnPlayableDirectorStop);
	init_event_method(&methods[33],mono_get_class_UnityEngine_GUIUtility(),il2cpp_get_class_UnityEngine_GUIUtility(),"MarkGUIChanged",0,(Il2CppMethodPointer) UnityEngine_GUIUtility_MarkGUIChanged);
	init_event_method(&methods[34],mono_get_class_UnityEngine_GUIUtility(),il2cpp_get_class_UnityEngine_GUIUtility(),"TakeCapture",0,(Il2CppMethodPointer) UnityEngine_GUIUtility_TakeCapture);
	init_event_method(&methods[35],mono_get_class_UnityEngine_GUIUtility(),il2cpp_get_class_UnityEngine_GUIUtility(),"RemoveCapture",0,(Il2CppMethodPointer) UnityEngine_GUIUtility_RemoveCapture);
	init_event_method(&methods[36],mono_get_class_UnityEngine_GUIUtility(),il2cpp_get_class_UnityEngine_GUIUtility(),"ProcessEvent",2,(Il2CppMethodPointer) UnityEngine_GUIUtility_ProcessEvent);
	init_event_method(&methods[37],mono_get_class_UnityEngine_GUIUtility(),il2cpp_get_class_UnityEngine_GUIUtility(),"EndContainerGUIFromException",1,(Il2CppMethodPointer) UnityEngine_GUIUtility_EndContainerGUIFromException);
	init_event_method(&methods[38],mono_get_class_UnityEngineInternal_Input_NativeInputSystem(),il2cpp_get_class_UnityEngineInternal_Input_NativeInputSystem(),"NotifyBeforeUpdate",1,(Il2CppMethodPointer) UnityEngineInternal_Input_NativeInputSystem_NotifyBeforeUpdate);
	init_event_method(&methods[39],mono_get_class_UnityEngineInternal_Input_NativeInputSystem(),il2cpp_get_class_UnityEngineInternal_Input_NativeInputSystem(),"NotifyUpdate",2,(Il2CppMethodPointer) UnityEngineInternal_Input_NativeInputSystem_NotifyUpdate);
	init_event_method(&methods[40],mono_get_class_UnityEngineInternal_Input_NativeInputSystem(),il2cpp_get_class_UnityEngineInternal_Input_NativeInputSystem(),"NotifyDeviceDiscovered",2,(Il2CppMethodPointer) UnityEngineInternal_Input_NativeInputSystem_NotifyDeviceDiscovered);
	init_event_method(&methods[41],mono_get_class_UnityEngineInternal_Input_NativeInputSystem(),il2cpp_get_class_UnityEngineInternal_Input_NativeInputSystem(),"ShouldRunUpdate",2,(Il2CppMethodPointer) UnityEngineInternal_Input_NativeInputSystem_ShouldRunUpdate);
	init_event_method(&methods[42],mono_get_class_UnityEngine_SubsystemManager(),il2cpp_get_class_UnityEngine_SubsystemManager(),"Internal_ReloadSubsystemsStarted",0,(Il2CppMethodPointer) UnityEngine_SubsystemManager_Internal_ReloadSubsystemsStarted);
	init_event_method(&methods[43],mono_get_class_UnityEngine_SubsystemManager(),il2cpp_get_class_UnityEngine_SubsystemManager(),"Internal_ReloadSubsystemsCompleted",0,(Il2CppMethodPointer) UnityEngine_SubsystemManager_Internal_ReloadSubsystemsCompleted);
	init_event_method(&methods[44],mono_get_class_UnityEngine_Experimental_TerrainAPI_TerrainCallbacks(),il2cpp_get_class_UnityEngine_Experimental_TerrainAPI_TerrainCallbacks(),"InvokeHeightmapChangedCallback",3,(Il2CppMethodPointer) UnityEngine_Experimental_TerrainAPI_TerrainCallbacks_InvokeHeightmapChangedCallback);
	init_event_method(&methods[45],mono_get_class_UnityEngine_Experimental_TerrainAPI_TerrainCallbacks(),il2cpp_get_class_UnityEngine_Experimental_TerrainAPI_TerrainCallbacks(),"InvokeTextureChangedCallback",4,(Il2CppMethodPointer) UnityEngine_Experimental_TerrainAPI_TerrainCallbacks_InvokeTextureChangedCallback);
	init_event_method(&methods[46],mono_get_class_UnityEngine_Font(),il2cpp_get_class_UnityEngine_Font(),"InvokeTextureRebuilt_Internal",1,(Il2CppMethodPointer) UnityEngine_Font_InvokeTextureRebuilt_Internal);
	init_event_method(&methods[47],mono_get_class_UnityEngine_UIElements_UIR_Utility(),il2cpp_get_class_UnityEngine_UIElements_UIR_Utility(),"RaiseGraphicsResourcesRecreate",1,(Il2CppMethodPointer) UnityEngine_UIElements_UIR_Utility_RaiseGraphicsResourcesRecreate);
	init_event_method(&methods[48],mono_get_class_UnityEngine_UIElements_UIR_Utility(),il2cpp_get_class_UnityEngine_UIElements_UIR_Utility(),"RaiseEngineUpdate",0,(Il2CppMethodPointer) UnityEngine_UIElements_UIR_Utility_RaiseEngineUpdate);
	init_event_method(&methods[49],mono_get_class_UnityEngine_UIElements_UIR_Utility(),il2cpp_get_class_UnityEngine_UIElements_UIR_Utility(),"RaiseFlushPendingResources",0,(Il2CppMethodPointer) UnityEngine_UIElements_UIR_Utility_RaiseFlushPendingResources);
	init_event_method(&methods[50],mono_get_class_UnityEngine_Canvas(),il2cpp_get_class_UnityEngine_Canvas(),"SendWillRenderCanvases",0,(Il2CppMethodPointer) UnityEngine_Canvas_SendWillRenderCanvases);
	init_event_method(&methods[51],mono_get_class_UnityEngine_XR_XRDevice(),il2cpp_get_class_UnityEngine_XR_XRDevice(),"InvokeDeviceLoaded",1,(Il2CppMethodPointer) UnityEngine_XR_XRDevice_InvokeDeviceLoaded);
	init_event_method(&methods[52],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokePrepareCompletedCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokePrepareCompletedCallback_Internal);
	init_event_method(&methods[53],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeFrameReadyCallback_Internal",2,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeFrameReadyCallback_Internal);
	init_event_method(&methods[54],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeLoopPointReachedCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeLoopPointReachedCallback_Internal);
	init_event_method(&methods[55],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeStartedCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeStartedCallback_Internal);
	init_event_method(&methods[56],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeFrameDroppedCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeFrameDroppedCallback_Internal);
	init_event_method(&methods[57],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeErrorReceivedCallback_Internal",2,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeErrorReceivedCallback_Internal);
	init_event_method(&methods[58],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeSeekCompletedCallback_Internal",1,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeSeekCompletedCallback_Internal);
	init_event_method(&methods[59],mono_get_class_UnityEngine_Video_VideoPlayer(),il2cpp_get_class_UnityEngine_Video_VideoPlayer(),"InvokeClockResyncOccurredCallback_Internal",2,(Il2CppMethodPointer) UnityEngine_Video_VideoPlayer_InvokeClockResyncOccurredCallback_Internal);
	init_event_method(&methods[60],mono_get_class_UnityEngine_XR_InputTracking(),il2cpp_get_class_UnityEngine_XR_InputTracking(),"InvokeTrackingEvent",4,(Il2CppMethodPointer) UnityEngine_XR_InputTracking_InvokeTrackingEvent);
	init_event_method(&methods[61],mono_get_class_UnityEngine_XR_InputDevices(),il2cpp_get_class_UnityEngine_XR_InputDevices(),"InvokeConnectionEvent",2,(Il2CppMethodPointer) UnityEngine_XR_InputDevices_InvokeConnectionEvent);
	init_event_method(&methods[62],mono_get_class_UnityEngine_XR_XRDisplaySubsystem(),il2cpp_get_class_UnityEngine_XR_XRDisplaySubsystem(),"InvokeDisplayFocusChanged",1,(Il2CppMethodPointer) UnityEngine_XR_XRDisplaySubsystem_InvokeDisplayFocusChanged);
	init_event_method(&methods[63],mono_get_class_UnityEngine_XR_XRInputSubsystem(),il2cpp_get_class_UnityEngine_XR_XRInputSubsystem(),"InvokeTrackingOriginUpdatedEvent",1,(Il2CppMethodPointer) UnityEngine_XR_XRInputSubsystem_InvokeTrackingOriginUpdatedEvent);
	init_event_method(&methods[64],mono_get_class_UnityEngine_XR_XRInputSubsystem(),il2cpp_get_class_UnityEngine_XR_XRInputSubsystem(),"InvokeBoundaryChangedEvent",1,(Il2CppMethodPointer) UnityEngine_XR_XRInputSubsystem_InvokeBoundaryChangedEvent);
}

