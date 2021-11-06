using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace Generater
{
    public class DelegateGenerater : CodeGenerater
    {
        string genName;
        TypeReference genType;
        TypeReference declarType;
        bool isStatic;
        bool isEvent;
        MethodDefinition addMethod;
        MethodDefinition removeMethod;

        MethodDefinition setMethod;
        MethodDefinition getMethod;

        List<MethodGenerater> methods = new List<MethodGenerater>();
        public DelegateGenerater(EventDefinition e)
        {
            genName = e.Name;
            genType = e.EventType;
            declarType = e.DeclaringType;

            if (e.AddMethod != null)
            {
                addMethod = e.AddMethod;
                methods.Add(new MethodGenerater(e.AddMethod));
                isStatic = e.AddMethod.IsStatic;
            }

            if (e.RemoveMethod != null)
            {
                removeMethod = e.RemoveMethod;
                methods.Add(new MethodGenerater(e.RemoveMethod));
                isStatic = e.RemoveMethod.IsStatic;
            }
            isEvent = true;

        }
        public DelegateGenerater(PropertyDefinition prop)
        {
            genName = prop.Name;
            genType = prop.PropertyType;
            declarType = prop.DeclaringType;

            if (prop.SetMethod != null)
            {
                setMethod = prop.SetMethod;
                methods.Add(new MethodGenerater(prop.SetMethod));
                isStatic = prop.SetMethod.IsStatic;
            }

            if (prop.GetMethod != null)
            {
                getMethod = prop.GetMethod;
                //methods.Add(new MethodGenerater(prop.GetMethod));
                isStatic = prop.GetMethod.IsStatic;
            }
            isEvent = false;
        }
        

        /*
        public static event global::UnityEngine.Application.LogCallback logMessageReceived
		{
			add
			{
                bool add = _logMessageReceived == null;
                _logMessageReceived += value;
                if(add)
                {
                    var value_p = Marshal.GetFunctionPointerForDelegate(logMessageReceivedAction);
                    MonoBind.UnityEngine_Application_add_logMessageReceived(value_p);
                }
			}
			remove
			{
                _logMessageReceived -= value;
                if(_logMessageReceived == null)
                {
                    var value_p = Marshal.GetFunctionPointerForDelegate(logMessageReceivedAction);
                    MonoBind.UnityEngine_Application_remove_logMessageReceived(value_p);
                }
			}
		}
             */
        public override void Gen()
        {
            var name = genName;

            var flag = isStatic ? "static" : "";
            flag += isEvent ? " event" : "";
            var type = genType; // LogCallback(string condition, string stackTrace, LogType type);

            var eventTypeName = TypeResolver.Resolve(type).RealTypeName();
            if (type.IsGenericInstance)
                eventTypeName = Utils.GetGenericTypeName(type);

            
            IMemberDefinition context = null ;

            //public static event LogCallback logMessageReceived
            CS.Writer.Start($"public {flag} {eventTypeName} {name}");

            var targetHandle = isStatic ? "" : "this.Handle, ";
            if (addMethod != null || setMethod != null)
            {
                var method = isEvent ? addMethod : setMethod;
                context = method;
                string _member = DelegateResolver.LocalMamberName(name, method); // _logMessageReceived

                var op = isEvent ? "+=" : "=";
                CS.Writer.Start(isEvent ? "add" : "set");
                if(addMethod != null)
                    CS.Writer.WriteLine($"bool attach = ({_member} == null)");
                else
                    CS.Writer.WriteLine($"bool attach = ({_member} != value)");

                CS.Writer.WriteLine($"{_member} {op} value");

                CS.Writer.Start("if(attach)");

                if (!isStatic)
                    CS.Writer.WriteLine($"ObjectStore.RefMember(this,ref {_member}_ref,{_member})"); // resist gc

                var res = TypeResolver.Resolve(type, context).Box(name);
                
                CS.Writer.WriteLine(Utils.BindMethodName(method, false, false) + $"({targetHandle}{res})");
                //var value_p = Marshal.GetFunctionPointerForDelegate(logMessageReceivedAction);
                //MonoBind.UnityEngine_Application_add_logMessageReceived(value_p);
                CS.Writer.WriteLine("ScriptEngine.CheckException()");
                CS.Writer.End(); //if(attach)
                CS.Writer.End(); // add
            }
            if(removeMethod != null)
            {
                if(context == null)
                    context = removeMethod;

                string _member = DelegateResolver.LocalMamberName(name, removeMethod); // _logMessageReceived

                CS.Writer.Start("remove");
                CS.Writer.WriteLine($"{_member} -= value");

                CS.Writer.Start($"if({_member} == null)");

                if (!isStatic)
                    CS.Writer.WriteLine($"ObjectStore.RefMember(this,ref {_member}_ref,{_member})"); // resist gc

                var res = TypeResolver.Resolve(type, context).Box(name);
                CS.Writer.WriteLine(Utils.BindMethodName(removeMethod, false, false) + $"({targetHandle}{res})");
                CS.Writer.WriteLine("ScriptEngine.CheckException()");
                CS.Writer.End(); //if(attach)
                CS.Writer.End(); // remove
            }
            else if (getMethod != null)
            {
                string _member = DelegateResolver.LocalMamberName(name, getMethod); // _logMessageReceived

                CS.Writer.Start("get");
                CS.Writer.WriteLine($"return {_member}");
                CS.Writer.End(); //get
            }

            CS.Writer.End();
        }

        static string GetEventFieldName(MethodDefinition method)
        {
            var name = method.Name;
            if (name.StartsWith("add_"))
                name = name.Substring("add_".Length);
            else if (name.StartsWith("remove_"))
                name = name.Substring("remove_".Length);
            return name;
        }



    }
}