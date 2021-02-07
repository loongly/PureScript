using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public class TypeResolver
    {
        public static bool WrapperSide;
        public static BaseTypeResolver Resolve(TypeReference _type)
        {
            var type = _type.Resolve();

           // if (Utils.IsDelegate(_type))
           //     Console.WriteLine(type.Name);
            
            if (Utils.IsDelegate(_type))
                return new DelegateResolver(_type);

            if (_type.Name.Equals("Void"))
                return new VoidResolver(_type);

            if (_type.Name.StartsWith("List`"))
                return new ListResolver(_type);

            if (_type.Name.Equals("String") || _type.FullName.Equals("System.Object"))
                return new StringResolver(_type);
            if (type != null && type.IsEnum)
                return new EnumResolver(_type);

            if (_type.IsGenericParameter || _type.IsGenericInstance || type == null)
                return new GenericResolver(_type);

            if (_type.IsPrimitive || _type.IsPointer)
                return new BaseTypeResolver(_type);

            if (_type.FullName.StartsWith("System."))
                return new SystemResolver(_type);

            if (_type.IsValueType || (_type.IsByReference && _type.GetElementType().IsValueType))
                return new StructResolver(_type);

            return new ClassResolver(_type);

        }
    }

    public class BaseTypeResolver
    {
        protected TypeReference type;
        public BaseTypeResolver(TypeReference _type)
        {
            type = _type;
        }
        public virtual string Paramer(string name)
        {
            return $"{TypeName()} {name}";
        }

        public virtual string LocalVariable(string name)
        {
            return Paramer(name);
        }

        public virtual string TypeName()
        {
            return RealTypeName();
        }

        protected string Alias()
        {
            var tName = type.FullName;
            var et = type.GetElementType();
            if (et != null)
                tName = et.FullName;
            switch (tName)
            {
                case "System.Void":
                    tName = "void";
                    break;
                case "System.Int32":
                    tName = "int";
                    break;
                case "System.Object":
                    tName = "object";
                    break;

                default:
                    if (!tName.StartsWith("System"))
                        tName = "global::" + tName;

                    break;
            }

            if (et != null)
                tName = type.FullName.Replace(et.FullName, tName);

            return tName.Replace("/",".");
        }

        public virtual string Unbox(string name,bool previous = false)
        {
            if (type.IsByReference)
                return "ref " + name;
            else
                return name;
        }
        public virtual string Box(string name)
        {
            if (type.IsByReference)
                return "ref " + name;
            else
                return name;
        }

        public string RealTypeName()
        {
            var et = type.GetElementType();

            var tName = Alias();

            if (type.IsByReference && et.IsValueType)
                tName = "ref " + tName.Replace("&", "");

            return tName;
        }
    }

    public class VoidResolver : BaseTypeResolver
    {
        public VoidResolver(TypeReference type) : base(type)
        {
        }

        public override string Box(string name)
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
        public EnumResolver(TypeReference type) : base(type)
        {
        }
        
        public override string TypeName()
        {
            return "int";
        }

        /// <summary>
        /// var type_e = (PrimitiveType)type;
        /// </summary>
        /// <returns> type_e </returns>
        public override string Box(string name)
        {
            CS.Writer.WriteLine($"var {name}_e = (int) {name}");
            return $"{name}_e";
        }

        /// <summary>
        /// var type_e = (int) type;
        /// </summary>
        /// <returns> type_e </returns>
        public override string Unbox(string name,bool previous)
        {
            var unboxCmd = $"var {name}_e = ({RealTypeName()}){name}";
            if (previous)
                CS.Writer.WritePreviousLine(unboxCmd);
            else
                CS.Writer.WriteLine(unboxCmd);
            return $"{name}_e";
        }
    }


    public class ClassResolver : BaseTypeResolver
    {
        public ClassResolver(TypeReference type) : base(type)
        {
        }

        public override string Paramer(string name)
        {
            return $"{TypeName()} {name}_h";
        }

        public override string TypeName()
        {
            return "int";
        }

        /// <summary>
        /// var value_h = ObjectStore.Store(value);
        /// </summary>
        /// <returns> value_h </returns>
        public override string Box(string name)
        {
            if(TypeResolver.WrapperSide)
                CS.Writer.WriteLine($"var {name}_h = {name}.__GetHandle()");
            else
                CS.Writer.WriteLine($"var {name}_h = ObjectStore.Store({name})");
            return $"{name}_h";
        }

        /// <summary>
        /// var resObj = ObjectStore.Get<GameObject>(res);
        /// </summary>
        /// <returns> resObj </returns>
        public override string Unbox(string name, bool previous)
        {
            var unboxCmd = $"var {name}Obj = ObjectStore.Get<{RealTypeName()}>({name}_h)";
            if (previous)
                CS.Writer.WritePreviousLine(unboxCmd);
            else
                CS.Writer.WriteLine(unboxCmd);
            return $"{name}Obj";
        }
    }

    public class ListResolver : BaseTypeResolver
    {
        BaseTypeResolver resolver;
        TypeReference genericType;
        public ListResolver(TypeReference type) : base(type)
        {
            var genericInstace = type as GenericInstanceType;
            genericType = genericInstace.GenericArguments.First();
            resolver = TypeResolver.Resolve(genericType);
        }

        public override string TypeName()
        {
            return $"List<{resolver.TypeName()}>";
        }

        public override string Box(string name)
        {
            CS.Writer.WriteLine($"{TypeName()} {name}_h = new {TypeName()}()");
            CS.Writer.Start($"foreach (var item in { name})");
            var res = resolver.Box("item");
            CS.Writer.WriteLine($"{name}_h.add({res})");
            CS.Writer.End();
            return $"{name}_h";
        }
        public override string Unbox(string name, bool previous)
        {
            using (new LP(CS.Writer.CreateLinePoint("//list unbox", previous)))
            {
                var relTypeName = $"List<{TypeResolver.Resolve(genericType).RealTypeName()}>";
                CS.Writer.WriteLine($"{relTypeName} {name}_r = new {relTypeName}()");
                CS.Writer.Start($"foreach (var item in { name})");
                var res = resolver.Unbox("item");
                CS.Writer.WriteLine($"{name}_r.add({res})");
                CS.Writer.End();
            }

            return $"{name}_r";
        }
    }

    public class DelegateResolver : BaseTypeResolver
    {
        public DelegateResolver(TypeReference type) : base(type)
        {
        }

        public override string Paramer(string name)
        {
            return $"{TypeName()} {name}_p";
        }

        public override string TypeName()
        {
            return "IntPtr";
        }

        public override string Box(string name)
        {
            CS.Writer.WriteLine($"var {name}_p = Marshal.GetFunctionPointerForDelegate({name})");
            return $"{name}_p";
        }

        public override string Unbox(string name, bool previous)
        {
            var typeName = RealTypeName();
            if (type.IsGenericInstance)
                typeName = Utils.GetGenericTypeName(type);
            var unboxCmd = $"var {name}_r = Marshal.GetDelegateForFunctionPointer<{typeName}>({name}_p)";
            if (previous)
                CS.Writer.WritePreviousLine(unboxCmd);
            else
                CS.Writer.WriteLine(unboxCmd);
            return $"{name}_r";
        }
    }

    public class StructResolver : BaseTypeResolver
    {
        public StructResolver(TypeReference type) : base(type)
        {
        }

        public override string Paramer(string name)
        {
            if(type.IsByReference)
                return base.Paramer(name);
            else 
                return "ref " + base.Paramer(name);
        }

        public override string LocalVariable(string name)
        {
            return base.Paramer(name);
        }


        public override string Box(string name)
        {
            name = base.Box(name);
            if (TypeResolver.WrapperSide && !name.StartsWith("ref "))
                name = "ref " + name;

            return name;
        }
    }

    public class StringResolver : BaseTypeResolver
    {
        public StringResolver(TypeReference type) : base(type)
        {
        }

        /*public override string TypeName()
        {
            if (type.Name.Equals("Object"))
                return "object";

            return base.TypeName();
        }*/
    }

    public class SystemResolver : BaseTypeResolver
    {
        public SystemResolver(TypeReference type) : base(type)
        {
        }

       /* public override string TypeName()
        {
            if (type.Name.Equals("Object"))
                return "object";

            return base.TypeName();
        }*/
    }

    public class GenericResolver : BaseTypeResolver
    {
        public GenericResolver(TypeReference type) : base(type)
        {
        }

    }
}
