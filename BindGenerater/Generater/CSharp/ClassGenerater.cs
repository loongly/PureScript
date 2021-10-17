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
        private Dictionary<TypeDefinition, ClassGenerater> nestType = new Dictionary<TypeDefinition, ClassGenerater>();
        private bool hasDefaultConstructor = false;
        private bool isCopyOrignType;
        private bool isFullRetainType;
        private StreamWriter FileStream;

        public HashSet<string> RefNameSpace = new HashSet<string>();
        

        Dictionary<int, AstNode> retainDic = new Dictionary<int, AstNode>();
        public TokenMapVisitor nodesCollector;

        public ClassGenerater(TypeDefinition type, StreamWriter writer = null)
        {
            genType = type;
            RefNameSpace.Add("PureScript.Mono");

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
            CheckCopyOrignNodes();
            if (isFullRetainType)
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
                    nestType[t] = nestGen;
                    RefNameSpace.UnionWith(nestGen.RefNameSpace);
                }
            }

            if (!isCopyOrignType)
            {
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
                    CheckInterface(method);
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
            foreach (var t in nestType.Values)
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
                    if (!genType.IsStruct() || isFullRetainType)
                        return;
                }

                if(!genType.IsNested)
                {
                    if(!isCopyOrignType)
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
                    }

                    if (!string.IsNullOrEmpty(genType.Namespace))
                    {
                        CS.Writer.Start($"namespace {genType.Namespace}");
                    }
                }

                

                Utils.TokenMap = nodesCollector.TokenMap;
                string classDefine = Utils.GetMemberDelcear(genType,stripInterfaceSet);

                bool isStatic = genType.IsAbstract && genType.IsSealed;
                if (genType.BaseType != null && !isStatic && !genType.IsStruct())
                {
                    if (genType.BaseType.FullName == "System.Object")
                    {
                        var index = classDefine.IndexOf(":");
                        if (index > 0)
                            classDefine = classDefine.Replace(":", ": WObject,");
                        else
                            classDefine += ": WObject";
                    }
                    else
                        Binder.AddType(genType.BaseType.Resolve());
                }

                CS.Writer.Start(classDefine);

                

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

                /*if(!hasDefaultConstructor && !genType.IsSealed)
                {
                    CS.Writer.WriteLine($"internal {genType.Name}()" + " { }", false);
                }*/
                foreach (var m in methods)
                {
                    m.Gen();
                }
                
                GenCopyOrignNodes();

                GenNested();

                CS.Writer.EndAll();
            }
        }

        bool IsCopyOrignType(TypeDefinition type)
        {


            isFullRetainType = Binder.retainTypes.Contains(type.FullName);

            isFullRetainType |= type.DoesSpecificTypeImplementInterface("IEnumerator");

            if (type.IsEnum || type.IsDelegate() || type.IsInterface)
                return true;

            if (Utils.IsFullValueType(type) || isFullRetainType)
                return true;

            return false;
        }

        void CopyType(TypeDefinition type )
        {
            bool isNested = type.IsNested;
            
            HashSet<string> IgnoreNestType = new HashSet<string>();

            //if (!(isNested && IsCopyOrignType(genType.DeclaringType)))
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

                if(isFullRetainType)
                    outVisitor = new CustomOutputVisitor(isNested, w, Binder.DecompilerSetting.CSharpFormattingOptions);
                else
                    outVisitor = new BlittablePartOutputVisitor(isNested, w, Binder.DecompilerSetting.CSharpFormattingOptions);

                outVisitor.isFullRetain = isFullRetainType;
                bool isStatic = genType.IsAbstract && genType.IsSealed;
                if (genType.BaseType != null && !isStatic && genType.IsClass)
                {
                    if (genType.BaseType.FullName == "System.Object")
                        outVisitor.AddWObject = true;
                }

                syntaxTree.AcceptVisitor(outVisitor);
                AddRefType(outVisitor.InternalTypeRef);
               

                if (!isNested)
                {
                    RefNameSpace.UnionWith(outVisitor.nestedUsing);
                    RefNameSpace.ExceptWith(Binder.IgnoreUsing);
                    foreach (var ns in RefNameSpace)
                    {
                        if(!string.IsNullOrEmpty(ns))
                            CS.Writer.WriteHead($"using {ns}");
                    }
                }
                
                var txt = w.ToString();
                CS.Writer.WriteLine(txt, false);
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

            bool inUnsafeNS = genType.Namespace.Contains("LowLevel.Unsafe");

            var retainFilter = new RetainFilter(genType.MetadataToken.ToInt32(), decompiler);
            retainFilter.TokenMap = nodesCollector.TokenMap;
            retainFilter.InUnsafeNS = inUnsafeNS;
            retainFilter.isFullValueType = isCopyOrignType;

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
            {
                node.AcceptVisitor(outputVisitor);
            }

            AddRefType(outputVisitor.InternalTypeRef);
            
        }

        void AddRefType(HashSet<string> refSet)
        {
            foreach (var rtype in refSet)
            {
                var tName = rtype.Replace("+", "/");
                var td = genType.Module.GetType(tName);
                if (td != null)
                {
                    var tdDeclear = td.DeclaringType;
                    if (tdDeclear != null && tdDeclear.MetadataToken == genType.MetadataToken)
                        nestType[td] = new ClassGenerater(td, FileStream);
                    else
                        Binder.AddType(td);
                }
                else if (genType.Module.TryGetTypeReference(tName, out var tref))
                    Binder.AddTypeRef(tref);
            }
        }

        private HashSet<string> stripInterfaceSet = new HashSet<string>();
        void CheckInterface(MethodDefinition method)
        {
            if(method.Name == "System.IDisposable.Dispose" && method.Parameters.Count == 0 && !method.IsPublic)
                stripInterfaceSet.Add("IDisposable");

            if(method.Name == "GetSurrogate" && method.Parameters.Count == 3 && !Utils.Filter(method))
                stripInterfaceSet.Add("ISurrogateSelector");
        }
    }
}