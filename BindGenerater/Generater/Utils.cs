using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace Generater
{
   public static class Utils
    {

        public static void Log(string str)
        {
            Console.WriteLine(str);
        }

        public static HashSet<string> IgnoreTypeSet = new HashSet<string>();

        //declear : static void UnityEngine_GameObject_SetActive (int thiz_h, System.Boolean value)
        //or: MonoBind.UnityEngine_GameObject_SetActive(this.Handle, value)
        public static string BindMethodName(MethodDefinition method, bool declear = false,bool withParam = true)
        {
            var name = method.DeclaringType.FullName + "_" + GetSignName(method);

            var res = ReName(name);
            if (!declear)
                res = "MonoBind." + res;

            if (withParam)
                res += BindMethodParamDefine(method, declear);

            return res;
        }

        //declear : (int thiz_h, System.Boolean value)
        //or:(this.Handle, value)
        public static string BindMethodParamDefine(MethodDefinition method, bool declear = false)
        {
            var param = "(";

            if (!method.IsStatic)
            {
                if(method.DeclaringType.IsValueType)
                {
                    if (declear)
                        param += TypeResolver.Resolve(method.DeclaringType).Paramer("thiz");
                    else
                        param += "ref this";
						
					if (method.HasParameters)
                        param += ", ";
                }
                else if (!method.IsConstructor)
                {
                    if (declear)
                        param += TypeResolver.Resolve(method.DeclaringType).Paramer("thiz");
                    else
                        param += "this.Handle";
						
					if (method.HasParameters)
                        param += ", ";
                }
                

                
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

        public static string ReName(string name)
        {
            return name.Replace("::", "_").Replace(".", "_").Replace("/","_");
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
            if(property.HasThis && property.Name == "Item")
            {//TODO:indexer
                return false; 
            }

            if (!Filter(property.PropertyType))
                return false;

            foreach (var attr in property.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("ObsoleteAttribute"))
                    return false;
            }
            return true;
        }

        public static bool Filter(MethodDefinition method)
        {
            if (!Filter(method.ReturnType) || !Filter(method.DeclaringType) || method.DeclaringType.IsNotPublic)
                return false;

            if (IsObsolete(method))
                return false;

            if (method.GenericParameters != null && method.GenericParameters.Count > 0)
                return false;

            foreach (var p in method.Parameters)
            {
                if (p.ParameterType.IsByReference && !p.ParameterType.GetElementType().IsValueType)
                    return false;
                if (p.ParameterType.IsPointer && method.IsConstructor && method.DeclaringType.IsStruct())
                    return false;

                if (p.IsOut)
                    return false;
               
                if (!Filter(p.ParameterType))
                    return false;
            }

            return true;
        }

        public static bool Filter(EventDefinition genEvent)
        {
            if (!Filter(genEvent.EventType))
                return false;
            var gType = genEvent.EventType as GenericInstanceType;

            if (genEvent.AddMethod != null && !genEvent.AddMethod.IsPublic)
                return false;

            if (IsObsolete(genEvent))
                return false;

            if(gType != null)
            {
                foreach (var p in gType.GenericArguments)
                {
                    if (p.IsGenericInstance || !Filter(p))
                        return false;
                }
            }
            return true;
        }

        public static bool IsObsolete(ICustomAttributeProvider method)
        {
            foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType.Name.Equals("ObsoleteAttribute"))
                {
                    if(attr.ConstructorArguments.Count == 2)
                    {
                        bool error = (bool)attr.ConstructorArguments[1].Value ;
                        return error;
                    }
                }
            }

            return false;
        }

        private static HashSet<TypeReference> DropTypes = new HashSet<TypeReference>();
        public static bool Filter(TypeReference type)
        {

            if (DropTypes.Contains(type))
                return false;

            foreach (var t in IgnoreTypeSet)
            {
                if (type.FullName.Contains(t))
                {
                    DropTypes.Add(type);
                    return false;
                }
            }

            if(IsDelegate(type))
            {
                var tList = GetDelegateParams(type, null, out var rType);
                tList.Add(rType);
                foreach(var t in tList)
                {
                    if(t != null && !Utils.Filter(t))
                    {
                        DropTypes.Add(type);
                        return false;
                    }
                }
            }

            if (type.IsGeneric() && !(IsDelegate(type) || IsFullValueType(type))) // 
            {
                Log("ignorType: " + type.FullName);
                DropTypes.Add(type);
                return false;
            }

            if(IsException(type))
            {
                Log("ignorType: " + type.FullName);
                DropTypes.Add(type);
                return false;
            }

            if (type.IsArray)
            {
                DropTypes.Add(type);
                return false;
            }

            if (IsAttribute(type))
            {
                DropTypes.Add(type);
                return false;
            }

            

            var td = type.Resolve();
            if (td != null && (IsObsolete(td) || td.IsInterface))
            {
                DropTypes.Add(type);
                return false;
            }

            if (td != null && td.IsStruct())
            {
                foreach (var f in td.Fields)
                {
                    if (!f.IsStatic && !Utils.Filter(f.FieldType))
                    {
                        DropTypes.Add(type);
                        return false;
                    }
                }
            }


            var ct = type.BaseType();
            while(ct != null)
            {
                if (!Filter(ct))
                {
                    DropTypes.Add(type);
                    return false;
                }

                ct = ct.BaseType();
            }

            return true;
        }

        public static bool IsAttribute(TypeReference type)
        {
            var td = type.Resolve();
            if (td == null)
                return false;
            if (type.Name == "Attribute")
                return true;

            if (td.BaseType != null && IsAttribute(td.BaseType))
                return true;

            return false;
            
        }

        public static bool IsException(TypeReference type)
        {
            var td = type.Resolve();
            if (td == null)
                return false;
            if (type.Name == "Exception")
                return true;

            if (td.BaseType != null && IsException(td.BaseType))
                return true;

            return false;

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

        

        public static bool IsDelegate(TypeReference type)
        {

            TypeReference curType = type;
            while(curType != null)
            {
                if (curType.FullName.Equals("System.Delegate") || curType.FullName.Equals("System.MulticastDelegate"))
                    return true;

                var tDef = curType.Resolve();
                if (tDef != null)
                    curType = tDef.BaseType;
                else
                    return false;
            }

            return false;
        }

        public static string GetGenericTypeName(TypeReference type, bool boxedType = false)
        {
            var gType = type as GenericInstanceType;
            if (!type.Name.Contains("`") || gType == null)
                return type.Name;

            var typeName = TypeResolver.Resolve(type).RealTypeName();
            var baseType = typeName.Substring(0, typeName.IndexOf('<'));

            var param = "<";

            for(int i = 0;i< gType.GenericArguments.Count;i++)
            {
                var p = gType.GenericArguments[i];

                var pName = boxedType ? TypeResolver.Resolve(p).TypeName() : TypeResolver.Resolve(p).RealTypeName();
                if (p.IsGenericInstance)
                    pName = Utils.GetGenericTypeName(p, boxedType);

                param += pName + (i == gType.GenericArguments.Count -1 ? "" : ", ");
            }
            param += ">";
            return baseType + param;
        }

        public static TypeReference GetGenericParamTyep(GenericParameter paramType, GenericInstanceType instanceType)
        {
            if (paramType == null || instanceType == null)
                return null;

            var postion = paramType.Position;
            if (postion >= 0 && postion < instanceType.GenericArguments.Count)
                return instanceType.GenericArguments[postion];
            return null;
        }

        /// <summary>
        /// GetDelegateParams
        /// </summary>
        /// <param name="type">delegate type</param>
        /// <param name="delegateTarget">static:null    else:delegate.Target</param>
        /// <param name="returnType">void:null    else:return type</param>
        /// <returns></returns>
        public static List<TypeReference> GetDelegateParams(TypeReference type, TypeReference delegateTarget, out TypeReference returnType)
        {
            List<TypeReference> types = new List<TypeReference>();

            if (delegateTarget != null)
                types.Add(delegateTarget);

            

            var invokMethod = type.Resolve().Methods.Where(m => m.Name == "Invoke").FirstOrDefault();
            if(invokMethod != null)
            {
                foreach (var p in invokMethod.Parameters)
                {
                    if (p.ParameterType.IsGenericParameter)
                        types.Add(GetGenericParamTyep(p.ParameterType as GenericParameter, type as GenericInstanceType));
                    else
                        types.Add(p.ParameterType);
                }

                if (invokMethod.ReturnType.IsVoid())
                    returnType = null;
                else
                    returnType = invokMethod.ReturnType;
            }
            else if (type.IsGenericInstance)
            {
                var gType = type as GenericInstanceType;
                types.AddRange(gType.GenericArguments);
                returnType = null;

                if (type.Name.StartsWith("Func"))
                {
                    returnType = types.Last();
                    types.Remove(returnType);
                }
                return types;
            }
            else
            {
                returnType = null;
            }
            
            return types;
        }

        static Dictionary<string, string> delegateSignDic = new Dictionary<string, string>();

        public static string GetDelegateWrapTypeName(TypeReference type, TypeReference delegateTarget)
        {
            var paramTpes = Utils.GetDelegateParams(type, delegateTarget, out var returnType);

            var paramDeclear = "(";
            for (int i = 0; i < paramTpes.Count; i++)
            {
                var p = paramTpes[i];
                paramDeclear += $"{TypeResolver.Resolve(p).TypeName()} arg{i} ";
                if (i != paramTpes.Count - 1)
                    paramDeclear += ",";
            }
            paramDeclear += ")";

            var returnName = returnType == null ? "void" : TypeResolver.Resolve(returnType).TypeName();
            var sign = paramDeclear + returnName;
            var delegateName = "";
            if(!delegateSignDic.TryGetValue(sign,out delegateName))
            {
                delegateName = "Delegate" + sign.GetHashCode().ToString("x");
                delegateSignDic[sign] = delegateName;
            }
            
            var define = $"public delegate {returnName} {delegateName} {paramDeclear}";

            GenerateBindings.AddDelegateDefine(define);

            return delegateName;
        }

        
        public static HashSet<string> GetNameSpaceList(HashSet<MethodDefinition> methods)
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

        public static bool IsManagedValueType(TypeReference _type)
        {
            return _type.IsValueType && !IsFullValueType(_type);
        }

        public static bool IsFullValueType(IType _type)
        {
            var tn = _type.GetDefinition().FullTypeName.ReflectionName.Replace("+", "/");

            var td = Binder.curModule.GetType(tn);
            if (td != null)
                return IsFullValueType(td);
            else if (Binder.curModule.TryGetTypeReference(tn, out var tref))
                return IsFullValueType(tref);

            return false;
        }

        public static bool IsFullValueType(TypeReference _type)
        {
            if (_type.IsPointer)
                _type = _type.GetElementType();
            var type = _type.Resolve();
            
            if (type == null)
                return false;

            if (_type.IsPrimitive || type.IsEnum || _type.IsVoid())
            {
                return true;
            }

            if (!_type.IsValueType)
            {
                return false;
            }

            foreach (var field in type.Fields)
            {
                if (!field.IsStatic && !IsFullValueType(field.FieldType))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsUnsafeMethod(MethodDefinition method)
        {
            if (method.ReturnType.IsPointer)
                return true;
            foreach (var p in method.Parameters)
            {
                if (p.ParameterType.IsPointer)
                    return true;
            }

            return false;
        }

        private static FieldDefinition GetFirstParam(TypeDefinition type)
        {
            foreach (var p in type.Fields)
            {
                if (!p.IsStatic)
                    return p;
            }
            return null;
        }

        
        public static bool HaveHeadPtr(TypeReference _type)
        {
            if (_type.IsArray || _type.IsByReference || _type.IsPointer)
                return HaveHeadPtr(_type.GetElementType());

            if (_type.Name.StartsWith("List`") || _type.Name.StartsWith("Action`"))
            {
                var gType = _type as GenericInstanceType;
                foreach (var p in gType.GenericArguments)
                {
                    if (!HaveHeadPtr(p))
                        return false;
                }
                return true;
            }

            if (_type.IsValueType || _type.Name == "String" || _type.Name == "Void")
                return true;


            var type = _type.Resolve();

            FieldDefinition firstParam = GetFirstParam(type);

            while (type.BaseType != null)
            {
                var p = GetFirstParam(type.BaseType.Resolve());
                if (p != null)
                    firstParam = p;

                type = type.BaseType.Resolve();
            }

            return firstParam != null && firstParam.FieldType.Name == "IntPtr";
        }

        public static HashSet<string> IcallSupportClass = new HashSet<string>();
        private static bool SupportIcallBind(TypeReference _type)
        {
            if (_type.Namespace.StartsWith("UnityEngine"))
            {
                if (IcallSupportClass.Contains(_type.Name))
                    return true;
                var ct = _type.BaseType();
                while (ct != null)
                {
                    if (SupportIcallBind(ct))
                        return true;

                    ct = ct.BaseType();
                }
            }
            return false;

        }
        public static bool IsUnityICallBind(TypeReference _type)
        {
            if(_type.Namespace.StartsWith("UnityEngine"))
            {
                if (SupportIcallBind(_type))
                    return true;

                //if (HaveHeadPtr(_type))
                //    return true;
            }

            return false;
        }


        

        public static int RunCMD(string cmd, string[] args, string workdir = null)
        {
            var argument = string.Join("\" \"", args);
            Console.WriteLine($"RunCMD: {cmd} {argument}");
            Process p = new Process();
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = argument;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            if (workdir != null)
                p.StartInfo.WorkingDirectory = workdir;
            else
                p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            p.Start();

            while (!p.StandardOutput.EndOfStream)
            {
                string line = p.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
            p.WaitForExit();

            return p.ExitCode;
        }

        public static bool IsWin32()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform.ToString().StartsWith("Win");
        }

        public static void CollectMonoAssembly(string entry, string dir, HashSet<string> adapterSet,HashSet<string> outSet)
        {
            if (outSet.Contains(entry))
                return;

            var file = Path.Combine(dir, entry);
            if (!File.Exists(file))
                return;

            outSet.Add(entry);
            var assembly = AssemblyDefinition.ReadAssembly(file);
            var refs = assembly.MainModule.AssemblyReferences;
            foreach(var aref in refs)
            {
                var refName = aref.Name;
                if (!refName.EndsWith(".dll"))
                    refName += ".dll";

                if (adapterSet.Contains(refName))
                    continue;

                CollectMonoAssembly(refName, dir, adapterSet, outSet);
            }
        }


        public static Dictionary<int, AstNode> TokenMap;
        public static string GetMethodDelcear(MethodDefinition method)
        {
            if(method.IsConstructor && method.Parameters.Count == 0)
            {
                return $"public {method.DeclaringType.Name}()";
            }

            var token = method.MetadataToken.ToInt32();
            StringWriter writer = new StringWriter();
            var output = new MethodDeclearVisitor(false, writer, Binder.DecompilerSetting.CSharpFormattingOptions);
            TokenMap[token].AcceptVisitor(output);
            return writer.ToString();
        }
    }

    public class NameCounter
    {
        Dictionary<string, int> nameDic = new Dictionary<string, int>();
        public string UniqueName(string name)
        {
            if (nameDic.TryGetValue(name, out var count))
            {
                count++;
                nameDic[name] = count;
                name += "_" + count;
            }
            else
            {
                nameDic[name] = 0;
            }

            return name;
        }
    }
}
