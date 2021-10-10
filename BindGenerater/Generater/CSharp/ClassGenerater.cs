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

        private List<PropertyGenerater> properties = new List<PropertyGenerater>();
        private List<DelegateGenerater> events = new List<DelegateGenerater>();
        private List<MethodGenerater> methods = new List<MethodGenerater>();
        private List<ClassGenerater> nestType = new List<ClassGenerater>();
        private bool hasDefaultConstructor = false;
        private bool isCopyOrignType;
        private StreamWriter FileStream;

        public HashSet<string> RefNameSpace = new HashSet<string>();

        Dictionary<int, AstNode> retainDic = new Dictionary<int, AstNode>();
        public TokenMapVisitor nodesCollector;

        public ClassGenerater(TypeDefinition type, StreamWriter writer = null)
        {
            genType = type;
            RefNameSpace.Add("PureScript.Mono");

            CheckCopyOrignNodes();
            if (writer == null)
            {
                var filePath = Path.Combine(Binder.OutDir, $"Binder.{TypeFullName()}.cs");
                CSCGenerater.AdapterWrapperCompiler.AddSource(filePath);
                FileStream = File.CreateText(filePath);
            }
            else
            {
                FileStream = writer;
            }
            
            isCopyOrignType = IsCopyOrignType(genType);
            if (isCopyOrignType)
                return;

            if (type.BaseType != null)
                RefNameSpace.Add(type.BaseType.Namespace);

            foreach (var t in type.NestedTypes)
            {
                if (t.Name.StartsWith("<") || !Utils.Filter(t))
                    continue;
                if ((t.IsPublic || t.IsNestedPublic) && !Utils.IsObsolete(t))
                {
                    var nestGen = new ClassGenerater(t, FileStream);
                    nestType.Add(nestGen);
                    RefNameSpace.UnionWith(nestGen.RefNameSpace);
                }
            }

            foreach (FieldDefinition field in genType.Fields)
            {
               // if (isFullValueType && !field.IsStatic)
                //    continue;
                if (field.IsPublic)
                {
                    properties.Add(new PropertyGenerater(field));
                    RefNameSpace.Add(field.FieldType.Namespace);
                }

            }

            foreach(var e in genType.Events)
            {
                if(Utils.Filter(e))
                {
                    events.Add(new DelegateGenerater(e));
                    RefNameSpace.Add(e.EventType.Namespace);
                }
            }

            foreach (PropertyDefinition prop in genType.Properties)
            {
                if (Utils.Filter(prop) && !IsCopyOrignNode(prop))
                {
                    var pt = prop.PropertyType.Resolve();
                    if (pt.IsDelegate())
                    {
                        events.Add(new DelegateGenerater(prop));
                    }
                    else
                    {
                        properties.Add(new PropertyGenerater(prop));
                        RefNameSpace.Add(prop.PropertyType.Namespace);
                    }
                }
            }

            if(!genType.IsDelegate())
            {
                foreach (MethodDefinition method in genType.Methods)
                {
                    if (IsCopyOrignNode(method))
                        continue;

                    if ((method.IsPublic || genType.IsInterface) && !method.IsGetter && !method.IsSetter && !method.IsAddOn && !method.IsRemoveOn )
                    {
                        var methodGener = new MethodGenerater(method);
                        methods.Add(methodGener);
                        RefNameSpace.UnionWith(Utils.GetNameSpaceRef(method));
                    }
                    if (method.IsConstructor && method.Parameters.Count == 0 && method.IsPublic)
                        hasDefaultConstructor = true;
                }
            }
        }

        public override string TypeFullName()
        {
            return genType.FullName.Replace("`","_");
        }

        private void GenNested()
        {
            if (nestType.Count <= 0)
                return;

            CS.Writer.Flush();
            foreach (var t in nestType)
            {
                t.Gen();
            }
        }

        public override void Gen()
        {
            using (new CS(new CodeWriter(FileStream)))
            {
                base.Gen();

                if (isCopyOrignType)
                {
                    CopyType(genType);
                    CS.Writer.EndAll();
                    return;
                }

                if(!genType.IsNested)
                {
                    RefNameSpace.ExceptWith(Binder.IgnoreUsing);
                    foreach (var ns in RefNameSpace)
                    {
                        if (!string.IsNullOrEmpty(ns))
                        {
                            CS.Writer.WriteLine($"using {ns}");
                        }
                    }
                    CS.Writer.WriteLine("using System.Runtime.InteropServices");
                    CS.Writer.WriteLine("using Object = UnityEngine.Object");

                    if (!string.IsNullOrEmpty(genType.Namespace))
                    {
                        CS.Writer.Start($"namespace {genType.Namespace}");
                    }
                }

                bool isStatic = genType.IsAbstract && genType.IsSealed;
                var classDefine = "public";
                if (isStatic)
                    classDefine += " static";
                else if(genType.IsAbstract)
                    classDefine += " abstract";

                if (genType.IsInterface)
                    classDefine += " interface ";
                else
                    classDefine += " class ";

                classDefine += genType.Name;

                if (genType.BaseType != null && !isStatic)
                {
                    string baseName = genType.BaseType.IsNested ? genType.BaseType.FullName.Replace("/", ".") : genType.BaseType.Name;
                    if (genType.BaseType.FullName == "System.Object" )
                        baseName = "WObject";
                    else
                        Binder.AddType(genType.BaseType.Resolve());

                    classDefine += $" : {baseName}";
                }

                CS.Writer.Start(classDefine);

                GenNested();

                Utils.TokenMap = nodesCollector.TokenMap;

                /*CS.Writer.Start($"internal {genType.Name}(int handle,IntPtr ptr): base(handle, ptr)");
                CS.Writer.End();*/

                foreach (var p in properties)
                {
                    p.Gen();
                }

                foreach(var e in events)
                {
                    e.Gen();
                }

                if(!hasDefaultConstructor && !genType.IsSealed)
                {
                    CS.Writer.WriteLine($"internal {genType.Name}()" + " { }", false);
                }
                foreach (var m in methods)
                {
                    m.Gen();
                }
                
                GenCopyOrignNodes();

                CS.Writer.EndAll();
            }
        }

        bool IsCopyOrignType(TypeDefinition type)
        {

            if (Utils.IsFullValueType(type))
                return true;

            if (type.IsGeneric() && !type.IsDelegate())
                return false;
            return type.IsEnum || type.IsDelegate() || type.IsInterface;
        }

        void CopyType(TypeDefinition type )
        {
            bool isNested = type.IsNested;
            
            HashSet<string> IgnoreNestType = new HashSet<string>();

            if (!(isNested && IsCopyOrignType(genType.DeclaringType)))
            {
                var tName = type.FullName.Replace("/", "+");
                var name = new FullTypeName(tName);
                var decompiler = Binder.GetDecompiler(type.Module.Name);
                AstNode syntaxTree;

                if (isNested)
                {
                    ITypeDefinition typeInfo = decompiler.TypeSystem.MainModule.Compilation.FindType(name).GetDefinition();
                    var tokenOfFirstMethod = typeInfo.MetadataToken;
                    syntaxTree = decompiler.Decompile(tokenOfFirstMethod);
                }
                else
                {
                    syntaxTree = decompiler.DecompileType(name);
                }

                StringWriter w = new StringWriter();
                CustomOutputVisitor outVisitor;
                outVisitor = new CustomOutputVisitor(isNested, w, Binder.DecompilerSetting.CSharpFormattingOptions);

                syntaxTree.AcceptVisitor(outVisitor);

                foreach(var rtype in outVisitor.InternalTypeRef)
                {
                    var td = genType.Module.GetType(rtype);
                    if(td != null)
                        Binder.AddType(td);
                    else if(genType.Module.TryGetTypeReference(rtype, out var tref))
                        Binder.AddTypeRef(tref);
                }

                /*if (!isNested)
                {
                    RefNameSpace.UnionWith(outVisitor.nestedUsing);
                    foreach (var ns in RefNameSpace)
                    {
                        if(!string.IsNullOrEmpty(ns))
                            CS.Writer.WriteHead($"using {ns}");
                    }
                }*/
                
                var txt = w.ToString();
                CS.Writer.WriteLine(txt, false);
            }

            if(genType.IsStruct())
            {
                foreach(var f in genType.Fields)
                {
                    var fType = f.FieldType.Resolve();
                    if (fType != null && !fType.IsPublic && !fType.IsNested)
                        Binder.AddType(fType);
                }

                if(properties.Count + methods.Count + nestType.Count > 0)
                {
                    if (!string.IsNullOrEmpty(genType.Namespace))
                    {
                        CS.Writer.Start($"namespace {genType.Namespace}");
                    }

                    var classDefine = $"public partial struct {genType.Name}";

                    CS.Writer.Start(classDefine);

                    GenNested();

                    foreach (var p in properties)
                    {
                        p.Gen();
                    }

                    foreach (var m in methods)
                    {
                        m.Gen();
                    }
                }
            }
        }


        void CheckCopyOrignNodes()
        {
            
            var decompiler = Binder.GetDecompiler(genType.Module.Name);

            
            var tName = genType.FullName.Replace("/", "+");
            var name = new FullTypeName(tName);
            ITypeDefinition typeInfo = decompiler.TypeSystem.MainModule.Compilation.FindType(name).GetDefinition();
            var tokenOfType = typeInfo.MetadataToken;
            var st = decompiler.Decompile(tokenOfType);

            nodesCollector = new TokenMapVisitor();
            st.AcceptVisitor(nodesCollector);

            if (!Binder.UnityCoreModuleSet.Contains(genType.Module.Name))
                return;

            var retainFilter = new RetainFilter(genType.MetadataToken.ToInt32(), decompiler);
            retainFilter.TokenMap = nodesCollector.TokenMap;
            st.AcceptVisitor(retainFilter);
            retainDic = retainFilter.RetainDic;

            if (retainDic.Count > 0)
                RefNameSpace.UnionWith(retainFilter.NamespaceRef);
        }
        bool IsCopyOrignNode(MemberReference member)
        {
            if (retainDic.Count < 1)
                return false;

            var token = member.MetadataToken.ToInt32();
            return token != 0 && retainDic.ContainsKey(token);
        }
        void GenCopyOrignNodes()
        {
            if (retainDic.Count < 1)
                return ;

            CS.Writer.WriteLine("// -- copy orign code nodes --");
            CS.Writer.Flush();
            var outputVisitor = new CustomOutputVisitor(genType.IsNested,CS.Writer.GetWriter(), Binder.DecompilerSetting.CSharpFormattingOptions);
            foreach (var node in retainDic.Values)
                node.AcceptVisitor(outputVisitor);

            foreach (var rtype in outputVisitor.InternalTypeRef)
            {
                var td = genType.Module.GetType(rtype);
                if (td != null)
                    Binder.AddType(td);
                else if (genType.Module.TryGetTypeReference(rtype, out var tref))
                    Binder.AddTypeRef(tref);
            }
        }
    }
}