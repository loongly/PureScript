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
        public static BaseTypeResolver Resolve(TypeReference _type, IMemberDefinition context = null)
        {
            var type = _type.Resolve();

            if (Utils.IsDelegate(_type))
                return new DelegateResolver(_type, context);

            if (_type.Name.Equals("Void"))
                return new VoidResolver(_type);

           // if (_type.Name.StartsWith("List`"))
           //     return new ListResolver(_type);

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
        public object data;
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
                    if (!tName.StartsWith("System") && !type.IsGeneric())
                        tName = "global::" + tName;

                    break;
            }

            if (et != null)
                tName = type.FullName.Replace(et.FullName, tName);

            var genericIndex = tName.IndexOf('`');
            if(genericIndex > 0)
            {
                tName = tName.Remove(genericIndex, 2);
            }

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

            if (type.IsByReference && (et.IsValueType || et.IsGeneric()))
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
            return "IntPtr";
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
        bool isStaticMember;
        TypeReference declarType;
        MethodDefinition contextMember;
        string uniqueName;
        int paramCount;
        bool returnValue;

        static HashSet<string> BoxedMemberSet = new HashSet<string>();
        static HashSet<string> UnBoxedMemberSet = new HashSet<string>();

        private static string _Member(string name)
        {
            return $"_{name}";
        }
        private static string _Action(string name)
        {
            return $"{name}Action";
        }

        public static string LocalMamberName(string name, MethodDefinition context)
        {
            var uniq = FullMemberName(context);
            return _Member($"{uniq}_{name}");
        }

        public DelegateResolver(TypeReference type, IMemberDefinition context) : base(type)
        {
            if(context != null)
            {
                var method = context as MethodDefinition;
                contextMember = method;
                isStaticMember = method.IsStatic;
                declarType = method.DeclaringType;
                paramCount = method.Parameters.Count;
                returnValue = !method.ReturnType.IsVoid();
                uniqueName = FullMemberName(method);
            }
        }

        static string FullMemberName(MethodDefinition method)
        {
            var methodName = method.Name;
            if (method.IsAddOn || method.IsSetter || method.IsGetter)
                methodName = methodName.Substring("add_".Length);//trim "add_" or "set_"
            else if (method.IsRemoveOn)
                methodName = methodName.Substring("remove_".Length);//trim "remove_"

            // Special to AddListener / RemoveListener
            else if (method.Parameters.Count == 1 && methodName.StartsWith("Add"))
                methodName = methodName.Substring("Add".Length);// trim "Add"  
            else if (method.Parameters.Count == 1 && methodName.StartsWith("Remove"))
                methodName = methodName.Substring("Remove".Length);// trim "Remove"  

            return method.DeclaringType.Name.Replace("/", "_") + "_" + methodName.Replace(".", "_");
        }

        public override string Paramer(string name)
        {
            return $"{TypeName()} {name}_p";
        }

        public override string TypeName()
        {
            return "IntPtr";
        }

        /*
        static event global::UnityEngine.Application.LogCallback _logMessageReceived;
        static Action<int, int, int> logMessageReceivedAction = OnlogMessageReceived;
        static void OnlogMessageReceived(int arg0,int arg1,int arg2)
        {
            _logMessageReceived(unbox(arg0), unbox(arg1), unbox(arg2));
        }
         */
        void WriteBoxedMember(string name)
        {
            if (contextMember == null)
                return;

            var varName = uniqueName + name;
            if (BoxedMemberSet.Contains(varName))
                return;
            BoxedMemberSet.Add(varName);

            string _member = _Member(name);// _logMessageReceived
            string _action = _Action(name);// logMessageReceivedAction

            var flag = isStaticMember ? "static" : "";
            var eventTypeName = TypeResolver.Resolve(type).RealTypeName();
            if (type.IsGenericInstance)
                eventTypeName = Utils.GetGenericTypeName(type);

            var eventDeclear = Utils.GetDelegateWrapTypeName(type, isStaticMember ? null : declarType); //Action <int,int,int>
            var paramTpes = Utils.GetDelegateParams(type, isStaticMember ? null : declarType, out var returnType); // string , string , LogType ,returnType
            var returnTypeName = returnType != null ? TypeResolver.Resolve(returnType).TypeName() : "void";

            //static event global::UnityEngine.Application.LogCallback _logMessageReceived;
            CS.Writer.WriteLine($"public {flag} {eventTypeName} {_member}");

            if(!isStaticMember)
                CS.Writer.WriteLine($"public GCHandle {_member}_ref"); // resist gc

            //static Action<int, int, int> logMessageReceivedAction = OnlogMessageReceived;
            CS.Writer.WriteLine($"static {eventDeclear} {_action} = On{name}");

            //static void OnlogMessageReceived(int arg0,int arg1,int arg2)
            var eventFuncDeclear = $"static {returnTypeName} On{name}(";

            for (int i = 0; i < paramTpes.Count; i++)
            {
                var p = paramTpes[i];
                eventFuncDeclear += TypeResolver.Resolve(p).LocalVariable($"arg{i}");
                if (i != paramTpes.Count - 1)
                {
                    eventFuncDeclear += ",";
                }
            }
            eventFuncDeclear += ")";

            CS.Writer.Start(eventFuncDeclear);
            CS.Writer.WriteLine("Exception __e = null");
            CS.Writer.Start("try");
            //_logMessageReceived(unbox(arg0), unbox(arg1), unbox(arg2));
            var callCmd = $"{_member}(";
            var targetObj = "";

            for (int i = 0; i < paramTpes.Count; i++)
            {
                var p = paramTpes[i];
                var param = TypeResolver.Resolve(p).Unbox($"arg{i}");

                if (i == 0 && !isStaticMember)
                {
                    targetObj = param + ".";
                    continue;
                }

                callCmd += param;
                if (i != paramTpes.Count - 1)
                    callCmd += ",";
            }
            callCmd += ")";

            if (!string.IsNullOrEmpty(targetObj))
                callCmd = targetObj + callCmd;
            if (returnType != null)
                callCmd = $"var res = " + callCmd;

            CS.Writer.WriteLine(callCmd);
            if (returnType != null)
            {
                var res = TypeResolver.Resolve(returnType).Box("res");
                CS.Writer.WriteLine($"return {res}");
            }
            CS.Writer.End();//try
            CS.Writer.Start("catch(Exception e)");
            CS.Writer.WriteLine("__e = e");
            CS.Writer.End();//catch
            CS.Writer.WriteLine("if(__e != null)", false);
            CS.Writer.WriteLine("ScriptEngine.OnException(__e.ToString())");
            if (returnType != null)
                CS.Writer.WriteLine($"return default({returnTypeName})");

            CS.Writer.End();//method

        }

        /*
        static Action <int,int,int> logMessageReceived;
        static Action <int,int,int> logMessageReceivedAction;
        static void OnlogMessageReceived(string arg0, string arg1, LogType arg2)
        {
            logMessageReceived(box(arg0), box(arg1), box(arg2));
        }

         */
        void WriteUnboxedMember(string name)
        {
            if (contextMember == null)
                return;

            var varName = uniqueName + name;

            if (UnBoxedMemberSet.Contains(varName))
                return;
            UnBoxedMemberSet.Add(varName);

            string _member = _Member(name);// _logMessageReceived

            var paramTpes = Utils.GetDelegateParams(type, isStaticMember ? null : declarType, out var returnType); // string , string , LogType ,returnType
            var returnTypeName = returnType != null ? TypeResolver.Resolve(returnType).RealTypeName() : "void";
            var eventDeclear = Utils.GetDelegateWrapTypeName(type, isStaticMember ? null : declarType); //Action <int,int,int>

            //static void OnlogMessageReceived(string arg0, string arg1, LogType arg2)
            var eventFuncDeclear = $"static {returnTypeName} On{name}(";
            for (int i = 0; i < paramTpes.Count; i++)
            {
                var p = paramTpes[i];
                if (!isStaticMember && i == 0)
                    eventFuncDeclear += "this ";
                eventFuncDeclear += $"{TypeResolver.Resolve(p).RealTypeName()} arg{i}";
                if (i != paramTpes.Count - 1)
                {
                    eventFuncDeclear += ",";
                }
            }
            eventFuncDeclear += ")";


            CS.Writer.WriteLine($"static {eventDeclear} {_member}");

            CS.Writer.Start(eventFuncDeclear);

            var callCmd = $"{_member}(";
            if (returnType != null)
            {
                var localVar = TypeResolver.Resolve(returnType).Paramer("res");
                if(localVar.StartsWith("ref "))
                    localVar = localVar.Replace("ref ", "");
                callCmd = localVar + " = " + callCmd;
            }

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
                var res = TypeResolver.Resolve(returnType).Unbox("res");
                CS.Writer.WriteLine($"return {res}");
            }

            CS.Writer.End();

        }

        public override string Box(string name)
        {
            var memberUniqueName = $"{uniqueName}_{name}";
            using (new LP(CS.Writer.GetLinePoint("//member")))
            {
                WriteBoxedMember(memberUniqueName);
            }
            
            var _action = _Action(memberUniqueName);
            var _member = _Member(memberUniqueName);

            CS.Writer.WriteLine($"var {memberUniqueName}_p = Marshal.GetFunctionPointerForDelegate({_action})");
            return $"{memberUniqueName}_p";
        }

        
        public override string Unbox(string name, bool previous)
        {
            var memberUniqueName = $"{uniqueName}_{name}";

            if(!contextMember.IsRemoveOn)
            {
                using (new LP(CS.Writer.GetLinePoint("//Method")))
                {
                    WriteUnboxedMember(memberUniqueName);
                }

                string _member = _Member(memberUniqueName);// _logMessageReceived

                string ptrName = $"{name}_p";

                var eventDeclear = Utils.GetDelegateWrapTypeName(type, isStaticMember ? null : declarType);

                var unboxCmd = $"{_member} = {ptrName} == IntPtr.Zero ? null: Marshal.GetDelegateForFunctionPointer<{eventDeclear}>({ptrName})";
                if (previous)
                    CS.Writer.WritePreviousLine(unboxCmd);
                else
                    CS.Writer.WriteLine(unboxCmd);
            }

            var resCmd = $"On{memberUniqueName}";
            if (!isStaticMember)
                resCmd = "thizObj." + resCmd;

            return resCmd;
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
