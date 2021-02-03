using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
   public static class Utils
    {

        public static void Log(string str)
        {

        }

        public static HashSet<string> IgnoreTypeSet = new HashSet<string>();

        public static string BindMethodName(MethodDefinition method, bool declear = false,bool withParam = true)
        {
            var name = method.DeclaringType.FullName + "_" + GetSignName(method);

            var res = ReName(name);
            if (!declear)
                res = "MonoBind." + res;

            if (withParam)
                res += GetParamDefine(method, declear);

            return res;
        }

        public static string GetParamDefine(MethodDefinition method, bool declear = false)
        {
            var param = "(";

            if (!method.IsStatic && !method.IsConstructor)
            {
                if (declear)
                    param += "int thiz_h";
                else
                    param += "this.Handle";

                if (method.HasParameters)
                    param += ", ";
            }

            var lastP = method.Parameters.LastOrDefault();
            foreach (var p in method.Parameters)
            {
                if (declear)
                {
                    param +=  TypeResolver.Resolve(p.ParameterType).Paramer(p.Name) + (p == lastP ? "" : ", ");
                }
                else
                {
                    param += TypeResolver.Resolve(p.ParameterType).Box(p.Name) + (p == lastP ? "" : ", ");
                }

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

        public static bool Filter(PropertyDefinition property)
        {
            foreach (var attr in property.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("ObsoleteAttribute"))
                    return false;
            }
            return true;
        }

        public static bool Filter(MethodDefinition method)
        {
            if (IgnoreTypeSet.Contains(method.ReturnType.FullName))
                return false;

            if (method.GenericParameters != null && method.GenericParameters.Count > 0)
                return false;

            foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("ObsoleteAttribute"))
                    return false;
            }

            if (method.IsAbstract)
                return false;

            if (method.ReturnType.IsArray)
                return false;

            foreach (var p in method.Parameters)
            {
                if (p.IsOut)
                    return false;
                if (p.ParameterType.IsArray)
                    return false;
                if (p.ParameterType.Name.StartsWith("List`"))
                    return false;

                if (IgnoreTypeSet.Contains(p.ParameterType.FullName))
                    return false;
            }

            return true;
        }

       // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
       // public delegate int UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType(int typeint);
        static void AppendDelegateType(MethodDefinition method, CodeWriter writer)
        {
            writer.Write("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]\n");
            writer.Write($"delegate ");

            // Return type
           /* if (IsFullValueType(method.ReturnType.Resolve()))
            {
                AppendCsharpTypeFullName(
                    returnType,
                    output);
            }
            else
            {
                output.Append("int");
            }*/
        }

        enum TypeKind
        {
            // No type (e.g. a global function)
            None,

            // An instance of any class
            Class,

            // A struct that must be managed. This includes types like
            // RaycastHit which have class fields (Transform) and types with no
            // C++ equivalent like decimal.
            ManagedStruct,

            // A struct that can be copied between C#/C++. These are types like
            // Vector3 with only non-class fields and a C++ equivalent can be
            // generated.
            FullStruct,

            // Any enum
            Enum,

            // Any primitive (e.g. int) except pointers
            Primitive,

            // A pointer to any type, either X*, IntPtr, or UIntPtr
            Pointer
        }

        static TypeKind GetTypeKind(TypeDefinition type)
        {
            /*if (type == typeof(void))
            {
                return TypeKind.None;
            }*/

            if (type.IsPointer)
            {
                return TypeKind.Pointer;
            }

            if (type.IsEnum)
            {
                return TypeKind.Enum;
            }

            if (type.IsPrimitive)
            {
                return TypeKind.Primitive;
            }

            if (!type.IsValueType)
            {
                return TypeKind.Class;
            }

            // Decimal (currently) can't be represented on the C++ side, so
            // don't count it as a full struct
            if ( IsFullValueType(type))
            {
                return TypeKind.FullStruct;
            }

            return TypeKind.ManagedStruct;
        }

        public static bool IsFullValueType(TypeDefinition type)
        {
            if (!type.IsValueType)
            {
                return false;
            }
            if (type.IsPrimitive || type.IsEnum )
            {
                return true;
            }

            foreach (FieldDefinition field in type.Fields)
            {
                if (!field.IsPublic
                    || (!field.IsStatic
                        && !IsFullValueType(field.FieldType.Resolve())))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDelegate(TypeReference type)
        {
            if (type.Name.Contains("Action"))
                Console.WriteLine(type.Name);

            TypeReference curType = type;
            while(curType != null)
            {
                if (curType.FullName.Equals("System.Delegate"))
                    return true;

                var tDef = curType.Resolve();
                if (tDef != null)
                    curType = tDef.BaseType;
                else
                    return false;
            }

            return false;
        }

        public static string GetGenericTypeName(TypeReference type)
        {
            var gType = type as GenericInstanceType;
            if (!type.Name.Contains("`") || gType == null)
                return type.Name;

            var typeName = TypeResolver.Resolve(type).RealTypeName();
            var baseType = typeName.Substring(0, typeName.IndexOf('`'));

            var param = "<";

            var lastP = gType.GenericArguments.LastOrDefault();
            foreach (var p in gType.GenericArguments)
            {
                param += p.Name + (p == lastP ? "" : ", ");
            }
            param += ">";
            return baseType + param;
        }

        
        public static HashSet<string> GetNameSpaceList(List<MethodDefinition> methods)
        {
            HashSet<string> nsSet = new HashSet<string>();
            foreach(var method in methods)
            {
                nsSet.UnionWith(GetNameSpaceRef(method));
            }
            return nsSet;
        }

        public static HashSet<string> GetNameSpaceRef(MethodDefinition method)
        {
            HashSet<string> set = new HashSet<string>();
            if(method.DeclaringType != null)
                set.Add(method.DeclaringType.Namespace);

            if (method.ReturnType != null)
                set.Add(method.ReturnType.Namespace);

            foreach (var p in method.Parameters)
            {
                set.Add(p.ParameterType.Namespace);
            }
            set.Remove("");
            return set;
        }
    }
}
