using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    public static class AOTGenerater
    {
        static CodeWriter NinjaWriter;
        static CodeWriter ModuleRegisterWriter;
        static Dictionary<string, string> AOTDic = new Dictionary<string, string>();

        static string WorkDir;
        static string ManagedDir;
        static string AotDir;
        static bool needAOT;
        public static void Init(string workDir)
        {
            WorkDir = workDir;
            
            ManagedDir = Path.Combine(workDir, "Managed");
            NinjaWriter = new CodeWriter(File.CreateText(Path.Combine(ManagedDir, "build.ninja")));
            NinjaWriter._eol = "";
            AotDir = Path.Combine(workDir, "aot");
            ModuleRegisterWriter = new CodeWriter(File.CreateText(Path.Combine(workDir, "generated", "aot_module_register.c")));

            needAOT = !Utils.IsWin32();
        }
        public static void AddAOTAssembly(string file)
        {
            var assembly = AssemblyDefinition.ReadAssembly(file);
            var tmp = file + ".tmp";
            if (File.Exists(tmp))
                File.Delete(tmp);

            AOTDic[Path.GetFileName(file)] = assembly.Name.Name.Replace(".","_").Replace("-","_");
            assembly.Dispose();
        }

        public static void End()
        {
            using (new CS(NinjaWriter))
            {
                CS.Writer.WriteLine($"include ../tools/config.ninja");
                CS.Writer.WriteLine("src_dir = .");
                CS.Writer.WriteLine($"out_dir = {AotDir}");

                foreach(var file in AOTDic.Keys)
                {
                    CS.Writer.WriteLine($"build $out_dir/lib{file}.a : aot_build $src_dir/{file}");
                }
                CS.Writer.EndAll();
            }

            using (new CS(ModuleRegisterWriter))
            {
                CS.Writer.WriteLine("#if RUNTIME_IOS",false);
                CS.Writer.WriteLine("#include \"runtime.h\"",false);
                foreach (var module in AOTDic.Values)
                {
                    CS.Writer.WriteLine($" extern void *mono_aot_module_{module}_info");
                }

                CS.Writer.Start("void mono_ios_register_modules(void)");

                foreach (var module in AOTDic.Values)
                {
                    CS.Writer.WriteLine($"mono_aot_register_module(mono_aot_module_{module}_info)");
                }

                CS.Writer.End();
                CS.Writer.WriteLine("#endif",false);
                CS.Writer.EndAll();
            }

            if(needAOT)
            {
                var res = Utils.RunCMD("ninja", new string[] { }, ManagedDir);
                if (res != 0)
                    throw new Exception($"Run ninja with dir {ManagedDir} error. ");
            }
        }

    }
}
