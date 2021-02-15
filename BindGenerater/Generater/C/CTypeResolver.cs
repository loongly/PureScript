using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Generater.C
{
    class CTypeResolver
    {
        public static BaseTypeResolver Resolve(TypeReference _type,bool il2cppType = false)
        {
            var type = _type.Resolve();

            if(type != null && type.IsEnum)
                return new EnumResolver(_type, il2cppType);
            
            if(_type.IsByReference)
            {
                var et = _type.GetElementType();
                if (et.IsValueType)
                    return new ValueTypeResolver(_type, il2cppType);
                else
                    return new ValueTypeResolver(_type, il2cppType); //TODO ref String
            }

            if (_type.IsPointer || _type.Name.Equals("IntPtr"))
                return new ValueTypeResolver(_type, il2cppType);

            if (_type.IsPrimitive)
                return new BaseTypeResolver(_type, il2cppType);

            if ( _type.IsValueType )
                return new ValueTypeResolver(_type, il2cppType);

            if (_type.Name.Equals("Void"))
                return new VoidResolver(_type, il2cppType);

            if (_type.Name.Equals("String") )
                return new StringResolver(_type, il2cppType);
            if (_type.FullName.Equals("System.Type"))
                return new ReflectionTypeResolver(_type, il2cppType);

            if (_type.IsArray)
                return new ArrayResolver(_type, il2cppType);

            return new RefTypeResolver(_type,il2cppType);

            /*if (CUtils.IsDelegate(_type))
                return new DelegateResolver(_type);

            if (_type.Name.StartsWith("List`"))
                return new ListResolver(_type);

            if (_type.IsGenericParameter || _type.IsGenericInstance || type == null)
                return new GenericResolver(_type);

            if (_type.FullName.StartsWith("System."))
                return new SystemResolver(_type);

            return new ClassResolver(_type);*/

        }
    }

    public class BaseTypeResolver
    {
        protected TypeReference type;
        protected bool il2cppType;
        public BaseTypeResolver(TypeReference _type,bool _il2cppType)
        {
            type = _type;
            il2cppType = _il2cppType;
        }

        public virtual string TypeName()
        {
            switch(type.Name)
            {
                case "Int32":
                    return "int32_t";
                case "UInt32":
                    return "uint32_t";
                case "Int16":
                    return "int16_t";
                case "UInt16":
                    return "uint16_t";

                case "Boolean":
                    return "bool";
                case "Double":
                    return "double";

                case "Single":
                    return "float";
                case "Int64":
                    return "int64_t";
                case "UInt64":
                    return "uint64_t";

                case "Char":
                    return "char";
                case "Byte":
                    return "int8_t";
            }

            return type.Name;
        }

        public virtual string Paramer(string name)
        {
            return $"{TypeName()} {name}";
        }

        public virtual string Unbox(string name, bool previous = false)
        {
            return name;
        }
        public virtual string Box(string name, bool previous = false)
        {
            return name;
        }

        public string RealTypeName()
        {
            if (type.Name.Equals("Void"))
                return "void";

            if (type.FullName.Equals("System.Object"))
                return "object";

            var tName = type.Name;
            if (type.IsNested)
                tName = $"{type.DeclaringType.FullName}.{tName}";
            return tName;
        }
    }

    public class VoidResolver : BaseTypeResolver
    {
        public VoidResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
        }

        public override string Box(string name, bool previous = false)
        {
            return "";
        }

        public override string TypeName()
        {
            return $"void";
        }
    }

    public class EnumResolver : BaseTypeResolver
    {
        public EnumResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
        }
        public override string TypeName()
        {
            return "int32_t";
        }
    }

    public class ValueTypeResolver : BaseTypeResolver
    {
        public ValueTypeResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
        }
        public override string TypeName()
        {
            return "void *";
        }
    }


    public class StringResolver : BaseTypeResolver
    {
        public StringResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
        }

        public override string Box(string name, bool previous = false)
        {
            var reName = $"mono{name}";
            var cmd = $"MonoString* {reName} = get_mono_string({name})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);

            return reName;
        }
        public override string Unbox(string name, bool previous = false)
        {
            var reName = $"i2{name}";
            var cmd = $"Il2CppString* {reName} = get_il2cpp_string({name})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);
            return reName;
        }
        public override string TypeName()
        {
            if (il2cppType)
                return "Il2CppString*";
            else
                return "MonoString*";
        }
        
    }

    public class RefTypeResolver : BaseTypeResolver
    {
        public RefTypeResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
        }

        public override string Box(string name, bool previous = false)
        {
            var reName = $"mono{name}";
            var classCache = ClassCacheGenerater.GetClass(type.Resolve());
            if(type.FullName == "UnityEngine.Object" || type.FullName == "System.Object")
                classCache = $"get_mono_class(il2cpp_object_get_class({name}))";

            var cmd = $"MonoObject* {reName} = get_mono_object({name},{classCache})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);

            return reName;
        }
        public override string Unbox(string name, bool previous = false)
        {
            var reName = $"i2{name}";
            string classCache = "NULL";
            if (type.Namespace.StartsWith("UnityEngine"))
            {
                classCache = ClassCacheGenerater.GetClass(type.Resolve(), true);
               // if (type.FullName == "UnityEngine.Object" || type.FullName == "System.Object")
               //     classCache = $"get_il2cpp_class(mono_object_get_class({name}))";
            }


            var cmd = $"Il2CppObject* {reName} = get_il2cpp_object({name},{classCache})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);
            return reName;
        }
        public override string TypeName()
        {
            if (il2cppType)
                return "Il2CppObject*";
            else
                return "MonoObject*";
        }
    }

    public class ReflectionTypeResolver : BaseTypeResolver
    {
        public ReflectionTypeResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
        }

        public override string Box(string name, bool previous = false)
        {
            var reName = $"mono{name}";
            var cmd = $"MonoReflectionType* {reName} = get_mono_reflection_type({name})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);
            return reName;
        }
        public override string Unbox(string name, bool previous = false)
        {
            var reName = $"i2{name}";

            var cmd = $"Il2CppReflectionType* {reName} = get_il2cpp_reflection_type({name})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);
            return reName;
        }
        public override string TypeName()
        {
            if (il2cppType)
                return "Il2CppReflectionType*";
            else
                return "MonoReflectionType*";
        }
    }


    public class ArrayResolver : BaseTypeResolver
    {
        bool isValueElement;
        public ArrayResolver(TypeReference _type, bool _il2cppType) : base(_type, _il2cppType)
        {
            isValueElement = type.GetElementType().IsValueType;
        }

        public override string Box(string name, bool previous = false)
        {
            if (isValueElement)
            {
                return name;
            }

            var eType = type.GetElementType();
            if(eType.Name != "String")
                ClassCacheGenerater.GetClass(type.GetElementType().Resolve());

            var reName = $"mono{name}";
            var cmd = $"MonoArray* {reName} = get_mono_array({name})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);
            return reName;
        }
        public override string Unbox(string name, bool previous = false)
        {
            if (isValueElement)
            {
                return name;
            }

            var reName = $"i2{name}";
            var cmd = $"Il2CppArray* {reName} = get_il2cpp_array({name})";
            if (previous)
                CS.Writer.WritePreviousLine(cmd);
            else
                CS.Writer.WriteLine(cmd);
            return reName;
        }
        public override string TypeName()
        {
            if (isValueElement)
                return "void*";

            if (il2cppType)
                return "Il2CppArray*";
            else
                return "MonoArray*";
        }

    }
}
