// auto gengerated !
#include "../custom/event_binding.h"
#include "../main/Mediator.h"

void init_event_method(EventMethodDesc* desc, MonoClass *monoklass, Il2CppClass* ilklass, const char* method_name, int param_count, Il2CppMethodPointer hook2)
{
	desc->hooked = mono_lookup_method(method_name,monoklass,param_count);
	desc->orign = hook_method2(ilklass, method_name, param_count, hook2);
}

void init_event_gen();
void init_event()
{
	init_event_gen();
}