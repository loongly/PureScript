#ifndef il2cpp_support_h
#define il2cpp_support_h

#include <stdint.h>
#include "glib.h"


#ifdef __cplusplus
extern "C" {
#endif


void init_il2cpp();

#if !defined(_WIN32)
	#include "il2cpp-api.h"
#else
	#include "il2cpp-api-types.h"
	

#define DO_API(r, n, p)		typedef r (*_##n) p;
#include "il2cpp-api-functions.h"
#undef DO_API

#define DO_API(r, n, p)		extern _##n n
#include "il2cpp-api-functions.h"
#undef DO_API

#endif

Il2CppImage* il2cpp_get_image(const char* assembly);
Il2CppClass* il2cpp_get_class(Il2CppImage* image, const char* _namespace, const char* name);
Il2CppClass* il2cpp_search_class(const char* assembly, const char* _namespace, const char* name);
Il2CppMethodPointer hook_method(const char* assembly, const char* _namespace, const char* name, const char* method, int32_t param_count, Il2CppMethodPointer hook);
Il2CppMethodPointer hook_method2(Il2CppClass* klass, const char* method, int32_t param_count, Il2CppMethodPointer hook);

Il2CppClass* il2cpp_get_exception_class();
Il2CppObject * il2cpp_exception_property(Il2CppObject *obj, const char *name, char is_virtual);

char* il2cpp_array_addr_with_size(Il2CppArray *array, int32_t size, uintptr_t idx);

#define il2cpp_array_addr(array, type, index) ((type*)(void*) il2cpp_array_addr_with_size (array, sizeof (type), index))
#define il2cpp_array_get(array, type, index) ( *(type*)il2cpp_array_addr ((array), type, (index)) )
#define il2cpp_array_set(array, type, index, value)    \
    do {    \
        type *__p = (type *) il2cpp_array_addr ((array), type, (index));    \
        *__p = (value); \
    } while (0)
#define il2cpp_array_setref(array, index, value)  \
    do {    \
        void* *__p = (void* *) il2cpp_array_addr ((array), void*, (index)); \
        /* il2cpp_gc_wbarrier_set_arrayref ((array), __p, (MonoObject*)(value));    */\
         *__p = (value);    \
    } while (0)

#ifdef __cplusplus
} /* extern "C" */
#endif
#endif