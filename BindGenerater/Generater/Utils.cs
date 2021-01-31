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
        public static string BindMethodName(MethodDefinition method, bool declear = false,bool withParam = true)
        {
            var name = method.DeclaringType.FullName + "_" + method.Name;

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
                    param += "int thisHandle";
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
                    param += $"{p.ParameterType.FullName} {p.Name}" + (p == lastP ? "" : ", ");
                }
                else
                {
                    param += p.Name + (p == lastP ? "" : ", ");
                }

            }
            param += ")";

            return param;
        }

        static string ReName(string name)
        {
            return name.Replace("::", "_").Replace(".", "_");
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
    }
}
