using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater.C
{
    public static class ClassCacheGenerater
    {
        private class ClassDesc
        {
            public string Assembly;
            public string Namespace;
            public string Name;

            public override string ToString()
            {
                return $"{Assembly}-{Namespace}-{Name}";
            }

            public override int GetHashCode()
            {
                return $"{Assembly}-{Namespace}-{Name}".GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return GetHashCode() == obj.GetHashCode();
            }
        }
        private static HashSet<string> MonoImageSet = new HashSet<string>();
        private static HashSet<ClassDesc> MonoClassSet = new HashSet<ClassDesc>();

        private static HashSet<string> I2ImageSet = new HashSet<string>();
        private static HashSet<ClassDesc> I2ClassSet = new HashSet<ClassDesc>();
        static CodeWriter HeadWriter;
        static CodeWriter SourceWriter;

        static ClassCacheGenerater()
        {
            HeadWriter = new CodeWriter(File.CreateText(Path.Combine(CBinder.OutDir, "class_cache_gen.h")));
            SourceWriter = new CodeWriter(File.CreateText(Path.Combine(CBinder.OutDir, "class_cache_gen.c")));
        }

        public static string GetAssembly(string name, bool il2cpp)
        {
            if (il2cpp)
                I2ImageSet.Add(name);
            else
                MonoImageSet.Add(name);
            return GetImageDefine(name, il2cpp);
        }

        public static string GetClass(TypeDefinition type,bool il2cpp = false)
        {
            var filePath = type.Module.Assembly.MainModule.FileName;

            return GetClass(Path.GetFileNameWithoutExtension(filePath), type.Namespace, type.Name, il2cpp);
        }

        public static string GetClass(string _assembly ,string _namespace,string name, bool il2cpp)
        {
            GetAssembly(_assembly, il2cpp);
            var desc = new ClassDesc() { Assembly = _assembly, Namespace = _namespace, Name = name };
            if (il2cpp)
                I2ClassSet.Add(desc);
            else
                MonoClassSet.Add(desc);
            return GetClassDefine(desc, il2cpp);
        }

        private static string GetImageDefine(string name, bool il2cpp)
        {
            var perfix = il2cpp ? "il2cpp" : "mono";
            return $"{perfix}_get_image_{name}()".Replace(".", "_");
        }

        private static string GetClassDefine(ClassDesc klass, bool il2cpp)
        {
            var perfix = il2cpp ? "il2cpp" : "mono";
            return $"{perfix}_get_class_{klass.Namespace}_{klass.Name}()".Replace(".", "_");
        }

        public static void Gen()
        {
            using (new CS(HeadWriter))
            {
                CS.Writer.WriteLine("#pragma once", false);
                CS.Writer.WriteLine("#include \"../main/runtime.h\"", false);
                CS.Writer.WriteLine("#include \"../main/il2cpp_support.h\"", false);
                CS.Writer.WriteLine("#include \"../main/Mediator.h\"", false);

                CS.Writer.WriteLine("#if defined(__cplusplus)", false);
                CS.Writer.WriteLine("extern \"C\" {", false);
                CS.Writer.WriteLine("#endif", false);

                foreach (var img in MonoImageSet)
                {
                    CS.Writer.WriteLine($"MonoImage* {GetImageDefine(img,false)}");
                }
                foreach (var img in I2ImageSet)
                {
                    CS.Writer.WriteLine($"Il2CppImage* {GetImageDefine(img, true)}");
                }

                foreach (var desc in MonoClassSet)
                {
                    CS.Writer.WriteLine($"MonoClass* {GetClassDefine(desc,false)}");
                }
                foreach (var desc in I2ClassSet)
                {
                    CS.Writer.WriteLine($"Il2CppClass* {GetClassDefine(desc,true)}");
                }

                CS.Writer.WriteLine("#if defined(__cplusplus)", false);
                CS.Writer.WriteLine("}",false);
                CS.Writer.WriteLine("#endif", false);
            }
            HeadWriter.EndAll();

            using (new CS(SourceWriter))
            {
                CS.Writer.WriteLine("#include \"class_cache_gen.h\"", false);

                foreach (var img in MonoImageSet)
                {
                    CS.Writer.Start($"MonoImage* {GetImageDefine(img,false)}");
                    CS.Writer.WriteLine("static MonoImage* img");
                    CS.Writer.WriteLine("if(!img)",false);
                    CS.Writer.WriteLine("\t"+ $"img = mono_get_image(\"{img}.dll\")");
                    CS.Writer.WriteLine("return img");
                    CS.Writer.End();
                }
                foreach (var img in I2ImageSet)
                {
                    CS.Writer.Start($"Il2CppImage* {GetImageDefine(img, true)}");
                    CS.Writer.WriteLine("static Il2CppImage* img");
                    CS.Writer.WriteLine("if(!img)",false);
                    CS.Writer.WriteLine("\t" + $"img = il2cpp_get_image(\"{img}.dll\")");
                    CS.Writer.WriteLine("return img");
                    CS.Writer.End();
                }

                foreach (var desc in MonoClassSet)
                {
                    CS.Writer.Start($"MonoClass* {GetClassDefine(desc,false)}");
                    CS.Writer.WriteLine("static MonoClass* klass");
                    CS.Writer.WriteLine("if(!klass)", false);
                    CS.Writer.WriteLine("\t" + $"klass = mono_get_class({GetImageDefine(desc.Assembly,false)},\"{desc.Namespace}\",\"{desc.Name}\")");
                    CS.Writer.WriteLine("return klass");
                    CS.Writer.End();
                }

                foreach (var desc in I2ClassSet)
                {
                    CS.Writer.Start($"Il2CppClass* {GetClassDefine(desc, true)}");
                    CS.Writer.WriteLine("static Il2CppClass* klass");
                    CS.Writer.WriteLine("if(!klass)", false);
                    CS.Writer.WriteLine("\t" + $"klass = il2cpp_get_class({GetImageDefine(desc.Assembly, true)},\"{desc.Namespace}\",\"{desc.Name}\")");
                    CS.Writer.WriteLine("return klass");
                    CS.Writer.End();
                }
            }
            SourceWriter.EndAll();
        }

    }
}
