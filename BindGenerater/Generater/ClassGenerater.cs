using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Generater
{
    public class ClassGenerater : CodeGenerater
    {
        private TypeDefinition genType;

        List<PropertyGenerater> properties = new List<PropertyGenerater>();
        List<MethodGenerater> methods = new List<MethodGenerater>();

        HashSet<string> refNameSpace = new HashSet<string>();
        

        public ClassGenerater(TypeDefinition type)
        {
            genType = type;

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
                if(method.IsPublic && !method.IsGetter && !method.IsSetter && Utils.Filter(method))
                {
                    methods.Add(new MethodGenerater(method));
                    refNameSpace.UnionWith(Utils.GetNameSpaceRef(method));
                }
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

                
                foreach (var ns in refNameSpace)
                {
                    if (!string.IsNullOrEmpty(ns))
                    {
                        CS.Writer.WriteLine($"using {ns}");
                        if(!ns.StartsWith("System"))
                            CS.Writer.WriteLine($"using PS_{ns}");
                    }
                }
                CS.Writer.WriteLine("using System.Runtime.InteropServices");
                CS.Writer.WriteLine("using Object = UnityEngine.Object");

                if (!string.IsNullOrEmpty(genType.Namespace))
                {
                    CS.Writer.Start($"namespace PS_{genType.Namespace}");
                }

                var classDefine = $"public class {genType.Name}";

                if (genType.IsInterface)
                {
                    classDefine += $" : WObject";
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


                CS.Writer.Start($"internal {genType.Name}(int handle,IntPtr ptr): base(handle, ptr)");
                CS.Writer.End();

                foreach (var p in properties)
                {
                    p.Gen();
                }

                foreach (var m in methods)
                {
                    m.Gen();
                }

                CS.Writer.EndAll();
            }
        }
    }
}