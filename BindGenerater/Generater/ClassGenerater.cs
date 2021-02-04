using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Generater
{
    public class ClassGenerater : CodeGenerater
    {
        private TypeDefinition genType;

        List<PropertyGenerater> properties = new List<PropertyGenerater>();
        List<MethodGenerater> methods = new List<MethodGenerater>();
        List<TypeDefinition> nestType = new List<TypeDefinition>();
        HashSet<string> refNameSpace = new HashSet<string>();
        bool hasDefaultConstructor = false;

        public ClassGenerater(TypeDefinition type)
        {
            genType = type;

            foreach (var t in type.NestedTypes)
            {
                if (CopyOrign(t))
                    nestType.Add(t);
            }
            if (CopyOrign(genType))
                return;

            foreach (FieldDefinition field in genType.Fields)
            {
                properties.Add(new PropertyGenerater(field));
                refNameSpace.Add(field.FieldType.Namespace);
            }

            foreach (PropertyDefinition prop in genType.Properties)
            {
                if(Utils.Filter(prop))
                {
                    properties.Add(new PropertyGenerater(prop));
                    refNameSpace.Add(prop.PropertyType.Namespace);
                }
            }

            foreach (MethodDefinition method in genType.Methods)
            {
                if((method.IsPublic || genType.IsInterface) && !method.IsGetter && !method.IsSetter && Utils.Filter(method))
                {
                    methods.Add(new MethodGenerater(method));
                    refNameSpace.UnionWith(Utils.GetNameSpaceRef(method));
                }
                if (method.IsConstructor && method.Parameters.Count == 0 && method.IsPublic)
                    hasDefaultConstructor = true;
            }
        }

        public override string TypeFullName()
        {
            return genType.FullName;
        }

        public override void Gen()
        {
            

            var filePath = Path.Combine(Binder.OutDir, $"Binder.{TypeFullName()}.cs");
            using (new CS(new CodeWriter(File.CreateText(filePath))))
            {
                base.Gen();

                if (CopyOrign(genType))
                {
                    CS.Writer.WriteLine(CopyGen(genType,false), false);
                    CS.Writer.EndAll();
                    return;
                }

                foreach (var ns in refNameSpace)
                {
                    if (!string.IsNullOrEmpty(ns))
                    {
                        CS.Writer.WriteLine($"using {ns}");
                       // if(!ns.StartsWith("System"))
                       //     CS.Writer.WriteLine($"using PS_{ns}");
                    }
                }
                CS.Writer.WriteLine("using System.Runtime.InteropServices");
                CS.Writer.WriteLine("using Object = UnityEngine.Object");

                if (!string.IsNullOrEmpty(genType.Namespace))
                {
                    CS.Writer.Start($"namespace {genType.Namespace}");
                }

                var classDefine = $"public class {genType.Name}";

                if (genType.IsInterface)
                {
                   // classDefine += $" : WObject";
                }
                else if (genType.BaseType != null)
                {
                    string baseName = genType.BaseType.Name;
                    if (genType.BaseType.FullName == "System.Object")
                        baseName = "WObject";
                    else
                        Binder.AddType(genType.BaseType.Resolve());

                    classDefine += $" : {baseName}";
                }

                CS.Writer.Start(classDefine);

                foreach(var t in nestType)
                {
                    CS.Writer.WriteLine(CopyGen(t,true),false);
                }

                /*CS.Writer.Start($"internal {genType.Name}(int handle,IntPtr ptr): base(handle, ptr)");
                CS.Writer.End();*/

                foreach (var p in properties)
                {
                    p.Gen();
                }


                if(!hasDefaultConstructor && !genType.IsSealed)
                {
                    CS.Writer.WriteLine($"internal {genType.Name}()" + " { }", false);
                }
                foreach (var m in methods)
                {
                    m.Gen();
                }

                CS.Writer.EndAll();
            }
        }

        bool CopyOrign(TypeDefinition type)
        {
            if (!type.IsPublic && !type.IsNestedPublic)
                return false;
            return type.IsValueType || type.IsEnum || type.IsDelegate() || type.IsInterface;
        }

        string CopyGen(TypeDefinition type ,bool isNested)
        {
            var tName = type.FullName.Replace("/", "+");
            var name = new FullTypeName(tName);
            SyntaxTree syntaxTree;

            if (isNested)
            {
                ITypeDefinition typeInfo = Binder.Decompiler.TypeSystem.MainModule.Compilation.FindType(name).GetDefinition();
                var tokenOfFirstMethod = typeInfo.MetadataToken;
                syntaxTree = Binder.Decompiler.Decompile(tokenOfFirstMethod);
            }
            else
            {
                syntaxTree = Binder.Decompiler.DecompileType(name);
            }

            StringWriter w = new StringWriter();
            var outVisitor = new CustomOutputVisitor(isNested, w, Binder.DecompilerSetting.CSharpFormattingOptions);
            syntaxTree.AcceptVisitor(outVisitor);

            foreach(var ns in outVisitor.nestedUsing)
            {
                CS.Writer.WriteHead($"using {ns}");
            }

            var txt = w.ToString();
            return txt;

        }
    }
}