#include "embeddinator.h"
#include "runtime.h"
#include <mono/metadata/environment.h>
#include <mono/utils/mono-publib.h>
#include <mono/utils/mono-logger.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/debug-helpers.h>

#pragma region class

#pragma endregion


#pragma region embedObject


void* mono_embeddinator_create_object(MonoObject* instance)
{
	/*MonoEmbedObject* object = g_new(MonoEmbedObject, 1);
	mono_embeddinator_init_object(object, instance);

	return object;*/
	return NULL;
}

void mono_embeddinator_init_object(MonoEmbedObject* object, MonoObject* instance)
{
	object->_class = mono_object_get_class(instance);
	object->_handle = mono_gchandle_new(instance, /*pinned=*/0);
}

void mono_embeddinator_destroy_object(MonoEmbedObject* object)
{
	/*if (object == 0) return;
	mono_gchandle_free(object->_handle);
	g_free(object);*/
}
#pragma endregion



#pragma region errorReport

void mono_embeddinator_throw_exception(MonoObject *exception)
{
#if defined(__OBJC__) && defined(NATIVEEXCEPTION)
	xamarin_process_managed_exception(exception);
#else
	mono_embeddinator_error_t error;
	error.type = MONO_EMBEDDINATOR_EXCEPTION_THROWN;
	error.exception = (MonoException*)exception;
	error.string = 0;

	mono_embeddinator_error(error);
#endif
}

char* mono_embeddinator_error_to_string(mono_embeddinator_error_t error)
{
	switch (error.type)
	{
	case MONO_EMBEDDINATOR_OK:
		return "No error";
	case MONO_EMBEDDINATOR_EXCEPTION_THROWN:
		return "Mono threw a managed exception";
	case MONO_EMBEDDINATOR_ASSEMBLY_OPEN_FAILED:
		return "Mono failed to load assembly";
	case MONO_EMBEDDINATOR_CLASS_LOOKUP_FAILED:
		return "Mono failed to lookup class";
	case MONO_EMBEDDINATOR_METHOD_LOOKUP_FAILED:
		return "Mono failed to lookup method";
	case MONO_EMBEDDINATOR_MONO_RUNTIME_MISSING_SYMBOLS:
		return "Failed to load Mono runtime shared libary symbols";
	}
	return "";
	//g_assert_not_reached();
}

static void mono_embeddinator_report_error_and_abort(mono_embeddinator_error_t error)
{
	fprintf(stderr, "Embeddinator error: %s.\n", mono_embeddinator_error_to_string(error));
	abort();
}

static mono_embeddinator_error_report_hook_t g_error_report_hook = mono_embeddinator_report_error_and_abort;

void* mono_embeddinator_install_error_report_hook(mono_embeddinator_error_report_hook_t hook)
{
	mono_embeddinator_error_report_hook_t prev = g_error_report_hook;
	g_error_report_hook = hook;

	return (void*)prev;
}

void mono_embeddinator_error(mono_embeddinator_error_t error)
{
	if (g_error_report_hook == 0)
		return;

	g_error_report_hook(error);
}
#pragma endregion


#pragma region tools

MonoClass* mono_embeddinator_get_decimal_class()
{
	static MonoClass* decimalclass = NULL;
	if (!decimalclass) {
		decimalclass = mono_class_from_name(mono_get_corlib(), "System", "Decimal");
	}
	return decimalclass;
}

MonoClass* mono_embeddinator_get_datetime_class()
{
	static MonoClass* datetimeclass = 0;
	if (!datetimeclass) {
		datetimeclass = mono_class_from_name(mono_get_corlib(), "System", "DateTime");
	}
	return datetimeclass;
}
#pragma endregion
