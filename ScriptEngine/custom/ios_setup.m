
#if !RUNTIME_IOS
#import <Foundation/Foundation.h>
#import <os/log.h>
#include <sys/stat.h>
#include <sys/mman.h>

#include "../lib/include/mono/metadata/assembly.h"
#include "../lib/include/mono/utils/mono-logger.h"
#include "../lib/include/runtime.h"

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


char *bundle_path;
char *doc_path;

const char *ios_bundle_path(void)
{
	if (bundle_path != NULL)
		return bundle_path;

    NSBundle *main_bundle = [NSBundle mainBundle];
    NSString *path;
    
    path = [main_bundle bundlePath];
	bundle_path = strdup ([path UTF8String]);
    return bundle_path;
}

const char *
get_documents_path(void)
{
    if (doc_path)
        return doc_path;
    
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *path = [paths objectAtIndex : 0];
    doc_path = strdup([path UTF8String]);

    return doc_path;
}


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
	res = snprintf (path, sizeof (path) - 1, "%s/Managed/%s.dll.aotdata", bundle, aname);
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
	mono_dllmap_insert (NULL, "ScriptEngine", NULL, "__Internal", NULL);
	
}

/* Implemented by generated code */
void mono_ios_register_modules (void);

void
mono_ios_runtime_init(void)
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



//
// ICALLS used by the mobile profile of mscorlib
//
// NOTE: The timezone functions are duplicated in XI, so if you're going to modify here, you have to
// modify there.
//
// See in XI runtime/xamarin-support.m

void*
xamarin_timezone_get_data(const char *name, uint32_t *size)
{
    NSTimeZone *tz = nil;
    if (name) {
        NSString *n = [[NSString alloc] initWithUTF8String: name];
        tz = [[NSTimeZone alloc] initWithName:n];
    } else {
        tz = [NSTimeZone localTimeZone];
    }
    NSData *data = [tz data];
    *size = [data length];
    void* result = malloc (*size);
    memcpy (result, data.bytes, *size);
    return result;
}

//
// Returns the geopolitical region ID of the local timezone.

const char *
xamarin_timezone_get_local_name ()
{
    NSTimeZone *tz = nil;
    tz = [NSTimeZone localTimeZone];
    NSString *name = [tz name];
    return (name != nil) ? strdup ([name UTF8String]) : strdup ("Local");
}

char**
xamarin_timezone_get_names (uint32_t *count)
{
    // COOP: no managed memory access: any mode.
    NSArray *array = [NSTimeZone knownTimeZoneNames];
    *count = array.count;
    char** result = (char**) malloc (sizeof (char*) * (*count));
    for (uint32_t i = 0; i < *count; i++) {
        NSString *s = [array objectAtIndex: i];
        result [i] = strdup (s.UTF8String);
    }
    return result;
}

// called from mono-extensions/mcs/class/corlib/System/Environment.iOS.cs
const char *
xamarin_GetFolderPath (int folder)
{
    // COOP: no managed memory access: any mode.
    // NSUInteger-based enum (and we do not want corlib exposed to 32/64 bits differences)
    NSSearchPathDirectory dd = (NSSearchPathDirectory) folder;
    NSURL *url = [[[NSFileManager defaultManager] URLsForDirectory:dd inDomains:NSUserDomainMask] lastObject];
    NSString *path = [url path];
    return strdup ([path UTF8String]);
}


// mcs/class/corlib/System/Console.iOS.cs
void
xamarin_log (const unsigned short *unicodeMessage)
{
    // COOP: no managed memory access: any mode.
    int length = 0;
    const unsigned short *ptr = unicodeMessage;
    while (*ptr++)
        length += sizeof (unsigned short);
    NSString *msg = [[NSString alloc] initWithBytes: unicodeMessage length: length encoding: NSUTF16LittleEndianStringEncoding];
    
   // if(print_log_callback != NULL)
    //    print_log_callback(msg);
#if TARGET_OS_WATCH && defined (__arm__) // maybe make this configurable somehow?
    const char *utf8 = [msg UTF8String];
    int len = strlen (utf8);
    fwrite (utf8, 1, len, stdout);
    if (len == 0 || utf8 [len - 1] != '\n')
        fwrite ("\n", 1, 1, stdout);
    fflush (stdout);
#else
 //   os_log (stdout_log, "%{public}@", msg);
    NSLog (@"%@", msg);
#endif
}

#endif
