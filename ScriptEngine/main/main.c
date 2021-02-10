#include "runtime.h"

#include <mono/jit/jit.h>
#include <stdlib.h>

#if IOS
typedef enum {
	MONO_AOT_MODE_NONE,
	MONO_AOT_MODE_NORMAL,
	MONO_AOT_MODE_HYBRID,
	MONO_AOT_MODE_FULL,
	MONO_AOT_MODE_LLVMONLY,
	MONO_AOT_MODE_INTERP,
	MONO_AOT_MODE_INTERP_LLVMONLY
} MonoAotMode;

void mono_jit_set_aot_mode (MonoAotMode mode);

extern void mono_aot_register_module (char *name);

extern void mono_ee_interp_init (const char *);
extern void mono_icall_table_init (void);
extern void mono_marshal_ilgen_init (void);
extern void mono_method_builder_ilgen_init (void);
extern void mono_sgen_mono_ilgen_init (void);

extern void *mono_aot_module_mscorlib_info;

void mono_ios_register_modules (void)
{
	mono_aot_register_module (mono_aot_module_mscorlib_info);
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



#endif


/*
static MonoString* gimme() {
	return mono_string_new(mono_domain_get(), "All your monos are belong to us!");
}*/

static void* GetManageFuncPtr() 
{
	return g_manageFuncPtr;
}

//extern void mono_ios_register_icall(void);
