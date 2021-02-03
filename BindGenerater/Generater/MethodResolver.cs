using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public class MethodResolver
    {
        public static BaseMethodResolver Resolve(MethodDefinition _method)
        {
            if (_method.IsConstructor)
                return new ConstructorMethodResolver(_method);
            if (_method.IsSetter)
                return new SetterMethodResolver(_method);
            if (_method.IsGetter)
                return new GetterMethodResolver(_method);

            if(_method.IsAddOn)
                return new AddOnMethodResolver(_method);
            if (_method.IsRemoveOn)
                return new RemoveOnMethodResolver(_method);

            return new BaseMethodResolver(_method);
        }
    }

    public class BaseMethodResolver
    {
        protected MethodDefinition method;
        public BaseMethodResolver(MethodDefinition _method)
        {
            method = _method;
        }

        public virtual string ReturnType()
        {
            return TypeResolver.Resolve(method.ReturnType).TypeName();
        }

        /// <summary>
        /// var res = MonoBind.UnityEngine_GameObject_GetComponent_1(this.Handle, type);
        /// var resObj = ObjectStore.Get<Component>(res);
		/// return resObj;
        /// </summary>
        /// <returns> resObj </returns>
        public virtual string Call(string name)
        {
            if (method.ReturnType.IsVoid())
            {
                CS.Writer.WriteLine(Utils.BindMethodName(method));
                return "";
            }

            var reName = TypeResolver.Resolve(method.ReturnType).Paramer(name);

            CS.Writer.WriteLine($"{reName} = {Utils.BindMethodName(method)}");
            return TypeResolver.Resolve(method.ReturnType).Unbox(name);
        }

        /// <summary>
        /// var thizObj = (GameObject)ObjectStore.Get(thiz_h);
		/// var value = thizObj.GetComponent(type);
        /// var value_h = ObjectStore.Store(value);
		/// return value_h;
        /// </summary>
        /// <returns> value_h </returns>
        public virtual string Implement(string name)
        {
            var thizObj = GetThizObj();

            if (method.ReturnType.IsVoid())
                CS.Writer.WriteLine("", false);
            else
                CS.Writer.WriteLine($"var {name} = ", false);

            CS.Writer.Write($"{thizObj}.{method.Name}(");
            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                CS.Writer.Write(TypeResolver.Resolve(p.ParameterType).Unbox(p.Name, true));
                if (lastP != p)
                    CS.Writer.Write(",");
            }
            CS.Writer.Write(");");

            return TypeResolver.Resolve(method.ReturnType).Box(name);
        }

        protected string GetThizObj()
        {
            if (method.IsStatic)
                return method.DeclaringType.Name;
            else
                return TypeResolver.Resolve(method.DeclaringType).Unbox("thiz", true);
        }
    }

    public class ConstructorMethodResolver : BaseMethodResolver
    {
        public ConstructorMethodResolver(MethodDefinition _method) : base(_method)
        {
        }

        public override string ReturnType()
        {
            return "int";
        }

        /// <summary>
        /// var value = new GameObject(name);
		/// var valueHandle = ObjectStore.Store(value);
		/// return valueHandle;
        /// </summary>
        /// <returns> valueHandle </returns>
        public override string Implement(string name)
        {
            CS.Writer.WriteLine($"var {name} = new {method.DeclaringType.Name}(", false);
            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                CS.Writer.Write(TypeResolver.Resolve(p.ParameterType).Unbox(p.Name, true));
                if (lastP != p)
                    CS.Writer.Write(",");
            }
            CS.Writer.Write(");");

            CS.Writer.WriteLine($"var {name}Handle = ObjectStore.Store({name})");
            return $"{name}Handle";
        }
    }

    public class SetterMethodResolver : BaseMethodResolver
    {
        public SetterMethodResolver(MethodDefinition _method) : base(_method)
        {
        }

        /// <summary>
        /// var thizObj = (GameObject)ObjectStore.Get(thiz);
		/// thizObj.layer = value;
        /// </summary>
        /// <returns> valueHandle </returns>
        public override string Implement(string name)
        {
            var thizObj = GetThizObj();
            var propertyName = method.Name.Substring("set_".Length);
            var valueName = TypeResolver.Resolve(method.Parameters.First().ParameterType).Unbox(name, true);
            CS.Writer.WriteLine($"{thizObj}.{propertyName} = {valueName}");
            return "";
        }
    }

    public class GetterMethodResolver : BaseMethodResolver
    {
        public GetterMethodResolver(MethodDefinition _method) : base(_method)
        {
        }

        /// <summary>
        /// var thizObj = (GameObject)ObjectStore.Get(thiz);
		/// var value = thizObj.layer;
		/// return value;
        /// </summary>
        /// <returns> value </returns>
        public override string Implement(string name)
        {
            var thizObj = GetThizObj();
            var propertyName = method.Name.Substring("get_".Length);
            CS.Writer.WriteLine($"var {name} = {thizObj}.{propertyName}");
            return TypeResolver.Resolve(method.ReturnType).Box(name);
        }
    }

    public class AddOnMethodResolver : BaseMethodResolver
    {
        public AddOnMethodResolver(MethodDefinition _method) : base(_method)
        {
        }

        public override string Implement(string name)
        {
            var thizObj = GetThizObj();
            var propertyName = method.Name.Substring("add_".Length);
            var value = TypeResolver.Resolve(method.Parameters.FirstOrDefault().ParameterType).Unbox(name);
            CS.Writer.WriteLine($"{thizObj}.{propertyName} += {value}");
            return "";
        }
    }

    public class RemoveOnMethodResolver : BaseMethodResolver
    {
        public RemoveOnMethodResolver(MethodDefinition _method) : base(_method)
        {
        }
        
        public override string Implement(string name)
        {
            var thizObj = GetThizObj();
            var propertyName = method.Name.Substring("remove_".Length);
            var value = TypeResolver.Resolve(method.Parameters.FirstOrDefault().ParameterType).Unbox(name);
            CS.Writer.WriteLine($"{thizObj}.{propertyName} -= {value}");
            return "";
        }
    }
}

