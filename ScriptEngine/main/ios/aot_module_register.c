#include "../runtime.h"
#if RUNTIME_IOS
extern void mono_aot_register_module(char *name);

extern void *mono_aot_module_mscorlib_info;

void mono_ios_register_modules(void)
{
	mono_aot_register_module(mono_aot_module_mscorlib_info);
}
#endif