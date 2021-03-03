#include "runtime.h"

#if !RUNTIME_IOS
#include <windows.h>
#include <direct.h>
#endif

#include "mono/jit/jit.h"
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



MonoDomain *g_domain;

char* mono_runtime_bundle_path;
char* mono_runtime_reload_path;

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

extern const char *ios_bundle_path(void);

const char * runtime_bundle_path(void)
{
    if(mono_runtime_bundle_path != NULL)
        return mono_runtime_bundle_path;
 
#if RUNTIME_IOS
	mono_runtime_bundle_path = ios_bundle_path();
#else
	if ((mono_runtime_bundle_path = _getcwd(NULL, 0)) == NULL)
		perror("getcwd error");
	else
		printf("doc_path=%s\n", mono_runtime_bundle_path);
#endif

	return mono_runtime_bundle_path;
}


MonoAssembly*
load_assembly(const char *name, const char *culture)
{
	char *load_dir = mono_runtime_reload_path;
	char path[1024];
	int res;
    const char *post = name + strlen(name) -4;

	if (culture && strcmp(culture, ""))
		res = snprintf(path, sizeof(path) - 1, "%s/%s/%s", load_dir, culture, name);
    else if(strcmp(post, ".dll") == 0 || strcmp(post, ".exe") == 0)
        res = snprintf(path, sizeof(path) - 1, "%s/%s", load_dir, name);
	else
		res = snprintf(path, sizeof(path) - 1, "%s/%s.dll", load_dir, name);
	assert(res > 0);

	if (!file_exists(path))
	{
		load_dir = runtime_bundle_path();
		if (culture && strcmp(culture, ""))
			res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s/%s", load_dir, culture, name);
		else if (strcmp(post, ".dll") == 0 || strcmp(post, ".exe") == 0)
			res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s", load_dir, name);
		else
			res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s.dll", load_dir, name);
		assert(res > 0);
	}

	printf("load_assembly load path: %s \n", path);
	if (file_exists(path)) {
		MonoAssembly *assembly = mono_assembly_open(path, NULL);
		assert(assembly);
		return assembly;
	}
	return NULL;
}

void check_mono_exception(MonoException* mono);
static void main_function (MonoDomain *domain, const char *file, int argc, char** argv)
{

	MonoAssembly *assembly = load_assembly("Adapter.wrapper.dll", NULL);

	if (!assembly)
		return;

	MonoImage* img = mono_assembly_get_image(assembly);
	MonoClass* klass = mono_class_from_name(img, "PureScript.Mono", "ScriptEngine");
	MonoMethod* main = mono_class_get_method_from_name(klass, "Main", 1);

	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	 //mono_jit_exec(domain, assembly, argc, argv);

	MonoObject *exc = NULL;
	int res = mono_runtime_run_main(main, argc, argv, &exc);
	check_mono_exception(exc);
	
	
	/*if (exc != NULL)
		return;

	assembly = load_assembly(file, NULL);
	if (!assembly)
		return;

	img = mono_assembly_get_image(assembly);
	klass = mono_class_from_name(img, "", "MonoEntry");
	main = mono_class_get_method_from_name(klass, "Main", 1);

	int res = mono_runtime_run_main(main, argc, argv, &exc);
	//mono_runtime_invoke(main, 0, NULL, &exc);

	check_mono_exception(exc);*/
}

const char* resolve_assembly(const char* request);
static MonoAssembly*
assembly_preload_hook(MonoAssemblyName *aname, char **assemblies_path, void* user_data)
{
	const char *name = resolve_assembly(mono_assembly_name_get_name(aname));
	const char *culture = mono_assembly_name_get_culture(aname);

	return load_assembly(name, culture);
}

void
log_callback_default(const char *log_domain, const char *log_level, const char *message, mono_bool fatal, void *user_data)
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
void register_assembly_map();

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
mono_setup(char* reloadDir, const char* file) {

	mono_runtime_reload_path = _strdup(reloadDir);

	int retval = 0;

	//MonoAllocatorVTable mem_vtable = {custom_malloc};
	//mono_set_allocator_vtable (&mem_vtable);
	
	mono_install_assembly_preload_hook(assembly_preload_hook, NULL);

	mono_install_unhandled_exception_hook(unhandled_exception_handler, NULL);
	mono_trace_set_log_handler(log_callback_default, NULL);
	mono_set_signal_chaining(true);
	mono_set_crash_chaining(true);

	mono_debug();

	
#if RUNTIME_IOS
	const char* rootdir = runtime_bundle_path();
#else
	const char* rootdir = mono_runtime_reload_path;
#endif

	mono_set_dirs(rootdir, rootdir);

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
	mono_domain_set(g_domain, false);

	register_assembly_map();
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
