using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public class BindResolver
    {
        public static BaseTypeResolver Resolve(TypeReference _type)
        {
            var type = _type.Resolve();

            if (_type.IsGenericParameter || _type.IsGenericInstance || type == null)
                return new GenericResolver(_type);

            if (_type.Name.Equals("Void"))
                return new VoidResolver(_type);

            if (_type.Name.Equals("String"))
                return new StringResolver(_type);
            if (type != null && type.IsEnum)
                return new EnumResolver(_type);
            if (_type.IsPrimitive)
                return new BaseTypeResolver(_type);

            if (_type.IsValueType)
                return new StructResolver(_type);

            return new ClassResolver(type);
        }
    }

    public class BaseTypeResolver
    {
        protected TypeReference type;
        public BaseTypeResolver(TypeReference _type)
        {
            type = _type;
        }
        public virtual void Paramer(string name)
        {
            CS.Writer.Write($"{type.DeclaringType.Name} {name}");
        }

        public virtual void Set(string name)
        {
           // return name;
        }

        public virtual string Get(MethodDefinition method, string name)
        {
            CS.Writer.WriteLine($"var {name} = {Utils.BindMethodName(method)}");
            return name;
        }

        public virtual string Return()
        {
            return $"{type.Name}";
        }
    }

    public class GenericResolver : BaseTypeResolver
    {
        public GenericResolver(TypeReference type) : base(type)
        {
        }
    }
    public class VoidResolver : BaseTypeResolver
    {
        public VoidResolver(TypeReference type) : base(type)
        {
        }
        public override void Paramer(string name)
        {
        }

        public override void Set(string name)
        {
        }

        public override string Get(MethodDefinition method, string name)
        {
            CS.Writer.WriteLine(Utils.BindMethodName(method));
            return "";
        }
    }

    public class EnumResolver : BaseTypeResolver
    {
        public EnumResolver(TypeReference type) : base(type)
        {
        }

        /*public override string Paramer(string name)
        {
            return $"int r_{name}";
        }

        public override string Set(string name)
        {
            return $"(int){name}";
        }
        public override string Get(string name)
        {
            return $"var {name} = ({type.DeclaringType.Name}) r_{name}";
        }*/
    }


    public class ClassResolver : BaseTypeResolver
    {
        public ClassResolver(TypeReference type) : base(type)
        {
        }

        public override void Paramer(string name)
        {
            CS.Writer.Write( $"int {name}_h");
        }

        public override string Get(MethodDefinition method, string name)
        {
            CS.Writer.WriteLine($"var {name}_h = {Utils.BindMethodName(method)}");
            CS.Writer.WriteLine($"var {name} = ObjectStore.Get<{type.Name}>({name}_h)");
            return name;
        }

        /*public override string Set(string name)
        {
            return $"(int){name}";
        }*/

    }

    public class StructResolver : BaseTypeResolver
    {
        public StructResolver(TypeReference type) : base(type)
        {
        }
    }

    public class StringResolver : BaseTypeResolver
    {
        public StringResolver(TypeReference type) : base(type)
        {
        }
    }
}
