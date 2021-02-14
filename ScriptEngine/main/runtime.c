#if IOS
#import <Foundation/Foundation.h>
#import <os/log.h>
#include <sys/mman.h>
#else
#include <windows.h>
#include <assert.h>
#endif

#include <mono/metadata/environment.h>
#include <mono/utils/mono-publib.h>
#include <mono/utils/mono-logger.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/exception.h>





#include <sys/stat.h>
#include <stdio.h>
#include <direct.h>

#include "runtime.h"

typedef char bool;
#define false 0
#define true 1

char* bundle_path = NULL;
char* doc_path = NULL;

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
get_bundle_path(void)
{
	if (bundle_path)
		return bundle_path;

#if IOS
	NSBundle *main_bundle = [NSBundle mainBundle];
	NSString *path;

	path = [main_bundle bundlePath];
	bundle_path = strdup([path UTF8String]);
#else

/*
	char path[1024];
	bundle_path = (char *)malloc(MAX_PATH);
	memset(bundle_path, 0, MAX_PATH);
	GetModuleFileName(NULL, bundle_path, MAX_PATH); // 得到当前执行文件的文件名（包含路径）
	*(strrchr(bundle_path, '\\')) = '\0';   // 删除文件名，只留下目录
*/

	if ((bundle_path = _getcwd(NULL, 0)) == NULL)
	{
		perror("getcwd error");
	}
	else
	{
		printf("bundle_path=%s\n", bundle_path);
	}
#endif

	return bundle_path;
}

const char *
get_documents_path(void)
{
	if (doc_path)
		return doc_path;

#if IOS
	NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
	NSString *path = [paths objectAtIndex : 0];
	doc_path = strdup([path UTF8String]);
#else
	if ((doc_path = _getcwd(NULL, 0)) == NULL)
	{
		perror("getcwd error");
	}
	else
	{
		printf("doc_path=%s\n", doc_path);
	}
#endif
	return doc_path;
}

MonoAssembly*
load_assembly(const char *name, const char *culture)
{
	const char *bundle = get_bundle_path();
	char path[1024];
	int res;

	printf("assembly_preload_hook: %s %s %s\n", name, culture, bundle);
	if (culture && strcmp(culture, ""))
		res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s/%s", bundle, culture, name);
	else
		res = snprintf(path, sizeof(path) - 1, "%s\\Managed\\%s", bundle, name);
	assert(res > 0);

	if (!file_exists(path))
	{
		const char *documents = get_documents_path();
		res = snprintf(path, sizeof(path) - 1, "%s/Managed/%s", documents, name);
		assert(res > 0);
	}


	printf("assembly_preload_hook load path: %s \n", path);
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

static MonoObject *
fetch_exception_property(MonoObject *obj, const char *name, bool is_virtual)
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
	MonoString *str = (MonoString *)fetch_exception_property(obj, name, is_virtual);
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
	//printf("%s (%s)\n%s\n", message, type_name, trace ? trace : "");

	//free(trace);
	//free(message);
	free(type_name);

	//os_log_info (OS_LOG_DEFAULT, "%@", msg);
	printf("Exit code: %d.", 1);
	exit(1);
}


MonoObject* mono_runtime_invoke_try(MonoMethod *method, void *obj, void **params) 
{
	MonoObject *exc = NULL;
	MonoObject* res = mono_runtime_invoke(method, obj, params, &exc);

	if (exc != NULL)
	{
		MonoClass *type = mono_object_get_class(exc);
		const char* type_name = mono_class_get_name(type);
		//char *type_name = strdup_printf("%s.%s", mono_class_get_namespace(type), mono_class_get_name(type));
		char *trace = fetch_exception_property_string(exc, "get_StackTrace", true);
		char *message = fetch_exception_property_string(exc, "get_Message", true);

		//printf("%s (%s)\n%s\n", message, type_name, trace ? trace : "");
		//TODO: exception..

		free(trace);
		free(message);
		free(type_name);
		return NULL;
	}
	return res;
}

/*
static int malloc_count = 0;

static void* custom_malloc(size_t bytes)
{
	++malloc_count;
	return malloc(bytes);
}*/

/* Implemented by generated code */
void mono_ios_register_icall(void);
//void mono_ios_register_modules(void);
//void mono_ios_setup_execution_mode(void);

void mono_debug() {

	mono_debug_init(MONO_DEBUG_FORMAT_MONO);

	static const char* options[] = {
		  "--soft-breakpoints",
		  "--debugger-agent=transport=dt_socket,address=127.0.0.1:10001,embedding=1,server=y,suspend=n"
	};
	mono_jit_parse_options(sizeof(options) / sizeof(char*), (char**)options);


	/*int da_port = GLOBAL_DEF("mono/debugger_agent/port", 23685);
	bool da_suspend = GLOBAL_DEF("mono/debugger_agent/wait_for_debugger", false);
	int da_timeout = GLOBAL_DEF("mono/debugger_agent/wait_timeout", 3000);

	CharString da_args = String("--debugger-agent=transport=dt_socket,address=127.0.0.1:" + itos(da_port) +
		",embedding=1,server=y,suspend=" + (da_suspend ? "y,timeout=" + itos(da_timeout) : "n"))
		.utf8();
	// --debugger-agent=help
	const char *options[] = {
		"--soft-breakpoints",
		da_args.get_data()
	};
	mono_jit_parse_options(2, (char **)options);*/
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
	mono_set_signal_chaining(TRUE);
	mono_set_crash_chaining(TRUE);

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
	g_domain = mono_jit_init (file);
	/*
	 * We add our special internal call, so that C# code
	 * can call us back.
	 */
	mono_ios_register_icall();

	char *managed_argv[2];
	managed_argv[0] = file;
	managed_argv[1] = file;
	main_function (g_domain, file, 2, managed_argv);
	
	//fprintf (stdout, "custom malloc calls = %d\n", malloc_count);

	return retval;
}

int mono_exit()
{
	int retval = mono_environment_exitcode_get ();

	mono_jit_cleanup (mono_domain_get());
	return retval;
}