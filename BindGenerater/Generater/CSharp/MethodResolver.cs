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
            {
                var firstParam = _method.Parameters.FirstOrDefault()?.ParameterType?.Resolve();
                if (firstParam != null && firstParam.IsDelegate())
                    return new AddOnMethodResolver(_method,false);
                else
                    return new SetterMethodResolver(_method);
            }
            if (_method.IsGetter)
                return new GetterMethodResolver(_method);

            if(_method.IsAddOn)
                return new AddOnMethodResolver(_method, true);
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

            var reName = TypeResolver.Resolve(method.ReturnType).LocalVariable(name);

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

            string paramSeparation = ",";

            if (method.Name == "op_Implicit")
                CS.Writer.Write($"");
            else if (method.Name == "op_Equality")
                paramSeparation = "==";
            else if (method.Name == "op_Inequality")
                paramSeparation = "!=";

            else if (method.Name == "op_Addition")
                paramSeparation = "+";
            else if (method.Name == "op_Subtraction")
                paramSeparation = "-";
            else if (method.Name == "op_Multiply")
                paramSeparation = "*";
            else if (method.Name == "op_Division")
                paramSeparation = "/";
            else if (method.Name == "op_LessThan")
                paramSeparation = "<";
            else if (method.Name == "op_GreaterThan")
                paramSeparation = ">";
            else if (method.Name == "op_Explicit")
                CS.Writer.Write($"({TypeResolver.Resolve(method.ReturnType).RealTypeName()})");
            else if (method.Name == "op_UnaryNegation")
                CS.Writer.Write($"-");

            

            else
                CS.Writer.Write($"{thizObj}.{method.Name}");

            CS.Writer.Write($"(");
            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                var value = TypeResolver.Resolve(p.ParameterType).Unbox(p.Name, true);
                //if (p.ParameterType.IsByReference)
                //    value = "ref " + value;

                CS.Writer.Write(value);
                if (lastP != p)
                    CS.Writer.Write(paramSeparation);
            }
            CS.Writer.Write(");");

            return TypeResolver.Resolve(method.ReturnType).Box(name);
        }

        protected string GetThizObj()
        {
            if (method.IsStatic)
                return method.DeclaringType.FullName.Replace("/",".");
            else
                return TypeResolver.Resolve(method.DeclaringType).Unbox("thiz", true);
        }
    }

    public class ConstructorMethodResolver : BaseMethodResolver
    {
        bool IsValueTypeConstructor;
        public ConstructorMethodResolver(MethodDefinition _method) : base(_method)
        {
            IsValueTypeConstructor = Utils.IsFullValueType(method.DeclaringType);
        }

        public override string ReturnType()
        {
            if(IsValueTypeConstructor)
                return "void";

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

            if(!IsValueTypeConstructor)
            {
                CS.Writer.WriteLine($"var {name} = new {TypeResolver.Resolve(method.DeclaringType).RealTypeName()}(", false);
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
            else
            {
                //if (UnityEngine_Vector3__ctor_94_info == null)
                //    UnityEngine_Vector3__ctor_94_info = typeof(UnityEngine.Vector3).GetConstructor(new Type[] { typeof(System.Single), typeof(System.Single), typeof(System.Single) });
                //UnityEngine_Vector3__ctor_94_info.Invoke(thiz, System.Reflection.BindingFlags.Default, Type.DefaultBinder, new object[] { x, y, z }, null);

                /*var infoName = Utils.BindMethodName(method, true, false) + "_info";
                CS.Writer.WriteLine($"if({infoName} == null)", false);
                CS.Writer.Write($"{infoName} = typeof({TypeResolver.Resolve(method.DeclaringType).RealTypeName()}).GetConstructor(new Type[] {{ ");
                var lastP = method.Parameters.LastOrDefault();
                foreach (var p in method.Parameters)
                {
                    CS.Writer.Write($"typeof({TypeResolver.Resolve(p.ParameterType).RealTypeName()})");
                    if (lastP != p)
                        CS.Writer.Write(",");
                }
                CS.Writer.Write(" });");

                CS.Writer.WriteLine($"{infoName}.Invoke(thiz, System.Reflection.BindingFlags.Default, Type.DefaultBinder, new object[] {{", false);
                lastP = method.Parameters.LastOrDefault();
                foreach (var p in method.Parameters)
                {
                    CS.Writer.Write(TypeResolver.Resolve(p.ParameterType).Unbox(p.Name, true));
                    if (lastP != p)
                        CS.Writer.Write(",");
                }
                CS.Writer.Write("}, null);");*/

                CS.Writer.WriteLine($"var n = new {TypeResolver.Resolve(method.DeclaringType).RealTypeName()}(",false);
                var lastP = method.Parameters.LastOrDefault();
                foreach (var p in method.Parameters)
                {
                    CS.Writer.Write(TypeResolver.Resolve(p.ParameterType).Unbox(p.Name, true));
                    if (lastP != p)
                        CS.Writer.Write(",");
                }
                CS.Writer.Write(");");
                CS.Writer.WriteLine("thiz = n");
                return "";
            }
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
            name = "value";
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
        bool isEvent;
        string propertyName;
        string uniqueName;
        public AddOnMethodResolver(MethodDefinition _method,bool _event) : base(_method)
        {
            isEvent = _event;

            propertyName = method.Name.Substring(4);//trim "add_" or "set_"
            uniqueName = method.DeclaringType.Name.Replace("/", "_") + "_" + propertyName;
        }

        /*
        static Action <int,int,int> logMessageReceived;
    static void OnlogMessageReceived(string arg0, string arg1, LogType arg2)
    {
        logMessageReceived(box(arg0), box(arg1), box(arg2));
    }
    [MonoPInvokeCallback(typeof(UnityEngine_Application_logMessageReceived_Type))]
	static void UnityEngine_Application_logMessageReceived (IntPtr value_p)
	{
        logMessageReceived = Marshal.GetDelegateForFunctionPointer<Action<int, int, int>>(value_p);
        UnityEngine.Application.logMessageReceived += OnlogMessageReceived;
	}
             */
        public override string Implement(string name)
        {
            
            var isStatic = method.IsStatic;

            var type = method.Parameters.FirstOrDefault().ParameterType; // LogCallback(string condition, string stackTrace, LogType type);
            var paramTpes = Utils.GetDelegateParams(type,isStatic? null: method.DeclaringType, out var returnType); // string , string , LogType ,returnType
            var eventDeclear = Utils.GetDelegateWrapTypeName(type, isStatic ? null : method.DeclaringType); //Action <int,int,int>

            var returnTypeName = returnType != null ? TypeResolver.Resolve(returnType).RealTypeName() : "void";

            //static void OnlogMessageReceived(string arg0, string arg1, LogType arg2)
            var eventFuncDeclear = $"static {returnTypeName} On{uniqueName}("; 
            for (int i = 0;i< paramTpes.Count;i++)
            {
                var p = paramTpes[i];
                if (!isStatic && i == 0)
                    eventFuncDeclear += "this ";
                eventFuncDeclear += $"{TypeResolver.Resolve(p).RealTypeName()} arg{i}";
                if (i != paramTpes.Count -1)
                {
                    eventFuncDeclear += ",";
                }
            }
            eventFuncDeclear += ")";

            using (new LP(CS.Writer.GetLinePoint("//Method")))
            {
                CS.Writer.WriteLine($"static {eventDeclear} {uniqueName}");

                CS.Writer.Start(eventFuncDeclear);

                var callCmd = $"{uniqueName}(";
                if (returnType != null)
                    callCmd = "var res = " + callCmd;

                for (int i = 0; i < paramTpes.Count; i++)
                {
                    var p = paramTpes[i];
                    callCmd += TypeResolver.Resolve(p).Box($"arg{i}");
                        
                    if (i != paramTpes.Count - 1)
                            callCmd += ",";
                }
                
                callCmd += ")";
                CS.Writer.WriteLine(callCmd);
                CS.Writer.WriteLine("ScriptEngine.CheckException()");
                if (returnType != null)
                {
                    var res = TypeResolver.Resolve(returnType).Box("res");
                    CS.Writer.WriteLine($"return {res}");
                }

                CS.Writer.End();
            }

            name = "value";
            var thizObj = GetThizObj();
            CS.Writer.WriteLine($"{uniqueName} = Marshal.GetDelegateForFunctionPointer<{eventDeclear}>({name}_p)");

            var actionTarget = isStatic ? $"On{uniqueName}" : $"{thizObj}.On{uniqueName}";
            var op = isEvent ? "+=" : "=";
            CS.Writer.WriteLine($"{thizObj}.{propertyName} {op} {actionTarget}");
            
            return "";
        }
    }

    public class RemoveOnMethodResolver : BaseMethodResolver
    {
        string propertyName;
        string uniqueName;
        public RemoveOnMethodResolver(MethodDefinition _method) : base(_method)
        {
            propertyName = method.Name.Substring("remove_".Length);
            uniqueName = method.DeclaringType.Name.Replace("/", "_") + "_" + propertyName;
        }
        
        public override string Implement(string name)
        {
            name = "value";
            var thizObj = GetThizObj();
            var isStatic = method.IsStatic;

            
            var actionTarget = isStatic ? $"On{uniqueName}" : $"{thizObj}.On{uniqueName}";
            CS.Writer.WriteLine($"{thizObj}.{propertyName} -= {actionTarget}");
            return "";
        }
    }
}

