using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generater.C
{
   public static class CUtils
    {
        public static void Log(string str)
        {
            Utils.Log("C>:"+str);
        }

        public static HashSet<string> IgnoreTypeSet = new HashSet<string>();

        public static string GetICallDescName(MethodDefinition method)
        {
            return method.DeclaringType.FullName.Replace("::", ".") + "::" + method.Name;
        }

        public static string ImplementMethodName(MethodDefinition method,bool withParam = true)
        {
            var name = method.DeclaringType.FullName + "_" + GetSignName(method);

            var res = ReName(name);

            if (withParam)
                res += GetParamDefine(method,false);

            return res;
        }

        public static string GetParamDefine(MethodDefinition method,bool il2cpp, string addParam = null )
        {
            var param = "(";

            if (!method.IsStatic)
            {
                if(il2cpp)
                    param += "Il2CppObject* thiz";
                else
                    param += "MonoObject* thiz";

                if (method.HasParameters)
                    param += ", ";
            }

            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                param += CTypeResolver.Resolve(p.ParameterType, il2cpp).Paramer(p.Name) + (p == lastP ? "" : ", ");
            }

            if(addParam != null)
            {
                if (!param.EndsWith("(") && !param.EndsWith(", "))
                    param += ", ";
                param += addParam;
            }

            param += ")";

            return param;
        }

        static string ReName(string name)
        {
            return name.Replace("::", "_").Replace(".", "_");
        }

        static Dictionary<string, List<string>> NameDic = new Dictionary<string, List<string>>();
        static string GetSignName(MethodDefinition method)
        {
            string name = method.Name;
            foreach (var generic in method.GenericParameters)
                name += "_" + generic.FullName;

            List<string> nameList;
            if(!NameDic.TryGetValue(name,out nameList))
            {
                nameList = new List<string>();
                NameDic[name] = nameList;
            }

            string fullName = method.FullName;
            int signIndex = -1;
            for(int i = 0;i< nameList.Count;i++)
            {
                if(nameList[i].Equals(fullName))
                {
                    signIndex = i;
                    break;
                }
            }
            if (signIndex < 0)
            {
                signIndex = nameList.Count;
                nameList.Add(fullName);
            }

            if (signIndex > 0)
                name += "_" + signIndex;

            return name;
        }

        /*public static bool Filter(PropertyDefinition property)
        {
            foreach (var attr in property.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("ObsoleteAttribute"))
                    return false;
            }

            return true;
        }*/

        public static bool Filter(MethodDefinition method)
        {
            if (!Filter(method.ReturnType))
                return false;
            foreach (var p in method.Parameters)
            {
                if (p.ParameterType.IsByReference && !Utils.IsFullValueType(p.ParameterType.GetElementType()))
                    return false;
                if (!Filter(p.ParameterType)) 
                    return false;
            }

            if (method.GenericParameters != null && method.GenericParameters.Count > 0)
                return false;

            /*foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("ObsoleteAttribute"))
                    return false;
            }*/

            if (method.IsAbstract)
                return false;

            return true;
        }

        public static bool Filter(TypeReference type)
        {
            foreach (var t in IgnoreTypeSet)
            {
                if (type.FullName.Contains(t))
                    return false;
            }
            var td = type.Resolve();
            if (td != null && td.IsStruct() && !Utils.IsFullValueType(td))
                return false;

            return true;
        }

        static HashSet<string> customICallSet;
        public static bool IsCustomICall(string icallName)
        {
            if(customICallSet == null)
            {
                var mark = "mono_add_internal_call(\"";
                customICallSet = new HashSet<string>();
                var customFile = Path.Combine(CBinder.OutDir, "..", "custom", "icall_binding.c");
                if(File.Exists(customFile))
                {
                    var lines = File.ReadAllLines(customFile);
                    foreach(var line in lines)
                    {
                        var start = line.IndexOf(mark);
                        if(start > 0)
                        {
                            start += mark.Length;
                            var end = line.IndexOf("\"", start + 1);
                            customICallSet.Add(line.Substring(start, end - start));
                        }
                    }
                }
            }

            return customICallSet.Contains(icallName);
        }

        
        public static bool IsUnityObject(TypeDefinition type )
        {
            while (type.BaseType != null)
            {
                if (type.BaseType.FullName == "UnityEngine.Object")
                    return true;

                type = type.BaseType.Resolve();
            }
            return false;
        }

        public static bool IsIcall(MethodDefinition method)
        {
            if (method.ImplAttributes == MethodImplAttributes.InternalCall)
                return true;

            return false;
        }

        public static bool IsNativeCallback(MethodDefinition method)
        {
            if (!method.HasBody)
                return false;
            bool res = false;
            foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType.Name == "RequiredByNativeCodeAttribute")
                {
                    res = true;
                    break;
                }
            }

            //if (!IsUnityObject(method.DeclaringType))
            //    res = false;

        return res;
        }

        public static bool IsEventCallback(MethodDefinition method)
        {
            foreach(var p in method.Parameters)
            {
                if (Utils.IsDelegate(p.ParameterType))
                    return false;
            }

            bool res = IsNativeCallback(method);
            if (!res)
                return false;

            if (!method.HasBody)
                return false;
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                var instructions = method.Body.Instructions[i];
                if(instructions.OpCode.Code == Mono.Cecil.Cil.Code.Callvirt)
                {
                    var op = instructions.Operand;
                    var invockMethod = op as MethodReference;
                    if(invockMethod != null)
                    {
                        if (Utils.IsDelegate(invockMethod.DeclaringType) && invockMethod.Name == "Invoke")
                            return true;
                    }
                }
            }

            return false;
        } 
    }
}
