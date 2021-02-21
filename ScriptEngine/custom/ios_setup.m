
#include "engine_include.h"
#if RUNTIME_IOS
#import <Foundation/Foundation.h>
#import <os/log.h>
#include <sys/stat.h>
#include <sys/mman.h>

#include "../lib/include/mono/metadata/assembly.h"
#include "../lib/include/mono/utils/mono-logger.h"


#define PRINT(...) do { printf (__VA_ARGS__); } while (0);

static print_log print_log_callback;

/* These are not in public headers */
typedef unsigned char* (*MonoLoadAotDataFunc)          (MonoAssembly *assembly, int size, void *user_data, void **out_handle);
typedef void  (*MonoFreeAotDataFunc)          (MonoAssembly *assembly, int size, void *user_data, void *handle);
void mono_install_load_aot_data_hook (MonoLoadAotDataFunc load_func, MonoFreeAotDataFunc free_func, void *user_data);


void mono_jit_set_aot_mode (MonoAotMode mode);

extern void mono_ee_interp_init (const char *);
extern void mono_icall_table_init (void);
extern void mono_marshal_ilgen_init (void);
extern void mono_method_builder_ilgen_init (void);
extern void mono_sgen_mono_ilgen_init (void);

const char * runtime_bundle_path(void);

void mono_ios_setup_execution_mode (void)
{
	mono_icall_table_init ();
	mono_marshal_ilgen_init ();
	mono_method_builder_ilgen_init ();
	mono_sgen_mono_ilgen_init ();
	mono_ee_interp_init (0);
	mono_jit_set_aot_mode (MONO_AOT_MODE_INTERP);
}

void set_log_callback(print_log callback)
{
    print_log_callback = callback;
}

static unsigned char *
load_aot_data (MonoAssembly *assembly, int size, void *user_data, void **out_handle)
{
	*out_handle = NULL;

	char path [1024];
	int res;

	MonoAssemblyName *assembly_name = mono_assembly_get_name (assembly);
	const char *aname = mono_assembly_name_get_name (assembly_name);
	const char *bundle = runtime_bundle_path ();

	// LOG (PRODUCT ": Looking for aot data for assembly '%s'.", name);
	res = snprintf (path, sizeof (path) - 1, "%s/%s.aotdata", bundle, aname);
	assert (res > 0);

	int fd = open (path, O_RDONLY);
	if (fd < 0) {
		//LOG (PRODUCT ": Could not load the aot data for %s from %s: %s\n", aname, path, strerror (errno));
		return NULL;
	}

	void *ptr = mmap (NULL, size, PROT_READ, MAP_FILE | MAP_PRIVATE, fd, 0);
	if (ptr == MAP_FAILED) {
		//LOG (PRODUCT ": Could not map the aot file for %s: %s\n", aname, strerror (errno));
		close (fd);
		return NULL;
	}

	close (fd);

	//LOG (PRODUCT ": Loaded aot data for %s.\n", name);

	*out_handle = ptr;

	return (unsigned char *) ptr;
}

static void
free_aot_data (MonoAssembly *assembly, int size, void *user_data, void *handle)
{
	munmap (handle, size);
}

void
log_callback (const char *log_domain, const char *log_level, const char *message, mono_bool fatal, void *user_data)
{
	NSLog (@"(%s %s) %s", log_domain, log_level, message);
	if (fatal) {
		exit (1);
	}
}

static void
register_dllmap (void)
{
	mono_dllmap_insert (NULL, "System.Native", NULL, "__Internal", NULL);
	mono_dllmap_insert (NULL, "System.Security.Cryptography.Native.Apple", NULL, "__Internal", NULL);
}

/* Implemented by generated code */
void mono_ios_register_modules (void);

void
mono_ios_runtime_init (void)
{
	register_dllmap ();

#if TARGET_IPHONE_SIMULATOR//模拟器
    
#elif TARGET_OS_IPHONE//真机
    mono_ios_register_modules ();
    mono_ios_setup_execution_mode ();
    
#endif

	mono_install_load_aot_data_hook (load_aot_data, free_aot_data, NULL);
	mono_trace_set_log_handler (log_callback, NULL);

	//setenv ("MONO_LOG_LEVEL", "debug", TRUE);

	//mono_jit_init_version ("Mono.ios", "mobile");
}

char* doc_path = NULL;
const char *
get_documents_path(void)
{
	if (doc_path)
		return doc_path;

#if RUNTIME_IOS
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

#endif
