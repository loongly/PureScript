#pragma once
#include <mono/jit/jit.h>
#include "il2cpp_support.h"

#pragma region Class

#pragma endregion

#pragma region embedObject


struct MonoEmbedObject
{
	MonoClass* _class;
	uint32_t _handle;
};
typedef struct MonoEmbedObject MonoEmbedObject;


/**
 * Creates a MonoEmbedObject support object from a Mono object instance.
 */
void* mono_embeddinator_create_object(MonoObject* instance);

/**
 * Initializes a MonoEmbedObject object from a Mono object instance.
 */
void mono_embeddinator_init_object(MonoEmbedObject* object, MonoObject* instance);

/**
 * Destroys a MonoEmbedObject object for a Mono object instance.
 */
void mono_embeddinator_destroy_object(MonoEmbedObject *object);

#pragma endregion


#pragma region errorReport

void mono_embeddinator_throw_exception(MonoObject* exception);
/**
 * Represents the different types of errors to be reported.
 */
typedef enum
{
	MONO_EMBEDDINATOR_OK = 0,
	// Mono managed exception
	MONO_EMBEDDINATOR_EXCEPTION_THROWN,
	// Mono failed to load assembly
	MONO_EMBEDDINATOR_ASSEMBLY_OPEN_FAILED,
	// Mono failed to lookup class
	MONO_EMBEDDINATOR_CLASS_LOOKUP_FAILED,
	// Mono failed to lookup method
	MONO_EMBEDDINATOR_METHOD_LOOKUP_FAILED,
	// Failed to load Mono runtime shared library symbols
	MONO_EMBEDDINATOR_MONO_RUNTIME_MISSING_SYMBOLS
} mono_embeddinator_error_type_t;

/**
 * Represents the error type and associated data.
 */
typedef struct
{
	mono_embeddinator_error_type_t type;
	// Contains exception object if type is MONO_EMBEDDINATOR_EXCEPTION_THROWN
	MonoException* exception;
	const char* string;
} mono_embeddinator_error_t;

/**
 * Converts an error object to its string representation.
 */
char* mono_embeddinator_error_to_string(mono_embeddinator_error_t error);



/**
 * Fires an error and calls the installed error hook for handling.
 */
void mono_embeddinator_error(mono_embeddinator_error_t error);

/** Represents the error report hook function type. */
typedef void(*mono_embeddinator_error_report_hook_t)(mono_embeddinator_error_t);

/**
 * Installs an hook that is called for each error reported.
 */
void* mono_embeddinator_install_error_report_hook(mono_embeddinator_error_report_hook_t hook);

#pragma endregion

#pragma region tools
/**
 * Gets decimal MonoClass.
 */
MonoClass* mono_embeddinator_get_decimal_class();

/**
 * Gets DateTime MonoClass.
 */
MonoClass* mono_embeddinator_get_datetime_class();

#pragma endregion




