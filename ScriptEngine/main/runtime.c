#include "runtime.h"

#if !RUNTIME_IOS
#include <windows.h>
#include <direct.h>
#endif

#include "mono/metadata/environment.h"
#include "mono/utils/mono-publib.h"
#include "mono/utils/mono-logger.h"
#include "mono/metadata/assembly.h"
#include "mono/metadata/mono-debug.h"
#include "mono/metadata/exception.h"

#include <sys/stat.h>
#include <stdio.h>
#include <string.h>
#include <assert.h>
#include <stdarg.h>

typedef char bool;
#define false 0
#define true 1

char* bundle_path = NULL;

void* g_manageFuncPtr;
MonoDomain *g_domain;

bool
file_exists(const char *path)
{
	struct stat buffer;
	return stat(path, &buffer) == 0;
}

char *
strdup_printf(const char *msg, ...)
{
	va_list args;
	char *formatted = NULL;

	va_start(args, msg);
	printf(formatted, msg, args);
	va_end(args);

	return formatted;
}

const char *
runtime_bundle_path(void)
{
	return bundle_path;
}


MonoAssembly*
load_assembly(const char *name, const char *culture)
{
	const char *bundle = runtime_bundle_path();
	char path[1024];
	int res;

	printf("load_assembly: %s %s %s\n", name, culture, bundle);
	if (culture && strcmp(culture, ""))
		res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s/%s", bundle, culture, name);
	else
		res = snprintf(path, sizeof(path) - 1, "%s\\Managed\\%s", bundle, name);
	assert(res > 0);

	/*if (!file_exists(path))
	{
		const char *documents = get_documents_path();
		res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s", documents, name);
		assert(res > 0);
	}*/


	printf("load_assembly load path: %s \n", path);
	if (file_exists(path)) {
		MonoAssembly *assembly = mono_assembly_open(path, NULL);
		assert(assembly);
		return assembly;
	}
	return NULL;
}


static void main_function (MonoDomain *domain, const char *file, int argc, char** argv)
{

	//assembly = mono_domain_assembly_open (domain, file);

	MonoAssembly *assembly = load_assembly(file, NULL);

	if (!assembly)
		exit (2);
	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	mono_jit_exec (domain, assembly, argc, argv);
}


static MonoAssembly*
assembly_preload_hook(MonoAssemblyName *aname, char **assemblies_path, void* user_data)
{
	const char *name = mono_assembly_name_get_name(aname);
	const char *culture = mono_assembly_name_get_culture(aname);

	return load_assembly(name, culture);
}

void
log_callback(const char *log_domain, const char *log_level, const char *message, mono_bool fatal, void *user_data)
{
	printf("(%s %s) %s", log_domain, log_level, message);
	if (fatal) {
		printf("Exit code: %d.", 1);
		exit(1);
	}
}

MonoObject *
mono_exception_property(MonoObject *obj, const char *name, char is_virtual)
{
	MonoMethod *get = NULL;
	MonoMethod *get_virt = NULL;
	MonoObject *exc = NULL;

	get = mono_class_get_method_from_name(mono_get_exception_class(), name, 0);
	if (get) {
		if (is_virtual) {
			get_virt = mono_object_get_virtual_method(obj, get);
			if (get_virt)
				get = get_virt;
		}

		return (MonoObject *)mono_runtime_invoke(get, obj, NULL, &exc);
	}
	else {
		printf("Could not find the property System.Exception.%s", name);
	}

	return NULL;
}

static char *
fetch_exception_property_string(MonoObject *obj, const char *name, bool is_virtual)
{
	MonoString *str = (MonoString *)mono_exception_property(obj, name, is_virtual);
	return str ? mono_string_to_utf8(str) : NULL;
}

void
unhandled_exception_handler(MonoObject *exc, void *user_data)
{
	//NSMutableString *msg = [[NSMutableString alloc] init];
	MonoClass *type = mono_object_get_class(exc);
	const char* type_name = mono_class_get_name(type);
	//char *type_name = strdup_printf("%s.%s", mono_class_get_namespace(type), mono_class_get_name(type));
	char *trace = fetch_exception_property_string(exc, "get_StackTrace", true);
	char *message = fetch_exception_property_string(exc, "get_Message", true);

	printf("Unhandled managed exception:\n");
	printf("%s (%s)\n%s\n", message, type_name, trace ? trace : "");

	if (trace != NULL)
		mono_free(trace);
	if (message != NULL)
		mono_free(message);

	//os_log_info (OS_LOG_DEFAULT, "%@", msg);
	printf("Exit code: %d.", 1);
	exit(1);
}

/*
static int malloc_count = 0;

static void* custom_malloc(size_t bytes)
{
	++malloc_count;
	return malloc(bytes);
}*/

/* Implemented by generated code */
void mono_register_icall(void);

#if RUNTIME_IOS
void mono_ios_runtime_init(void);
#endif

void mono_debug() {

	mono_debug_init(MONO_DEBUG_FORMAT_MONO);

	static const char* options[] = {
		  "--soft-breakpoints",
		  "--debugger-agent=transport=dt_socket,address=127.0.0.1:10001,embedding=1,server=y,suspend=n"
	};
	mono_jit_parse_options(sizeof(options) / sizeof(char*), (char**)options);
}

int 
mono_setup(char* bundleDir, const char* file) {

	bundle_path = bundleDir;


	int retval = 0;

	//MonoAllocatorVTable mem_vtable = {custom_malloc};
	//mono_set_allocator_vtable (&mem_vtable);
	
	mono_install_assembly_preload_hook(assembly_preload_hook, NULL);

	mono_install_unhandled_exception_hook(unhandled_exception_handler, NULL);
	mono_trace_set_log_handler(log_callback, NULL);
	mono_set_signal_chaining(true);
	mono_set_crash_chaining(true);

	mono_debug();
	/*
	 * Load the default Mono configuration file, this is needed
	 * if you are planning on using the dllmaps defined on the
	 * system configuration
	 */
	//mono_config_parse (NULL);
	/*
	 * mono_jit_init() creates a domain: each assembly is
	 * loaded and run in a MonoDomain.
	 */

#if RUNTIME_IOS
	mono_ios_runtime_init();
#endif
	g_domain = mono_jit_init (file);
	/*
	 * We add our special internal call, so that C# code
	 * can call us back.
	 */
	mono_register_icall();

	char *managed_argv[2];
	managed_argv[0] = file;
	managed_argv[1] = file;
	main_function (g_domain, file, 2, managed_argv);
	
	return retval;
}

int mono_exit()
{
	int retval = mono_environment_exitcode_get ();

	mono_jit_cleanup (mono_domain_get());
	return retval;
}
