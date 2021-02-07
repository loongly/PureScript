using Generater;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindGenerater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world.");

            string fileDir = @"..\..\EngineLib\full\";
            Binder.Init(@"..\..\glue\");
            foreach (var filePath in Directory.GetFiles(fileDir))
            {
                var file = Path.GetFileName(filePath);
                if (file.StartsWith("UnityEngine.") && file.EndsWith(".dll"))
                //if(file == "UnityEngine.CoreModule.dll")
                {
                        Binder.Bind(filePath);
                    }
            }
            Binder.End();

        }

        static void MainTest(string[] args)
        {
            /* ClassGenerater c = new ClassGenerater(1);
             c.Test("abcd"+ c.TestC);
             var ctr = c.GetType().GetConstructor(new Type[] { typeof(int) });
             // c.GetType().InvokeMember()
             //var d = ctr.Invoke(new object[] { 2 });

             var flag = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
               var n = ctr.Invoke(c, System.Reflection.BindingFlags.Default, Type.DefaultBinder, new object[] { 200 }, null);
 */


         /*   [MonoPInvokeCallback(typeof(UnityEngine_Application_add_onBeforeRender_Type))]
        static void UnityEngine_Application_add_onBeforeRender(IntPtr value_p)
        {
            UnityAction onBeforeRender = null;
            onBeforeRender = Marshal.GetDelegateForFunctionPointer<UnityEngine.Events.UnityAction>(value_p);
            void OnFocusChangeAction(bool focus)
            {
                if (onBeforeRender != null)
                    onBeforeRender.Invoke();
            }

            Application.focusChanged += OnFocusChangeAction;
            //UnityEngine.Application.onBeforeRender += OnBeforeRenderAction;
        }*/

        }

        static void TestWriter()
        {
            using (new CS(new CodeWriter(File.CreateText("test.txt"))))
            {
                CS.Writer.WriteLine("1");
                CS.Writer.WriteLine("2");
                CS.Writer.WriteLine("3");
                CS.Writer.CreateLinePoint("//aa");

                CS.Writer.WriteLine("4");
                CS.Writer.WriteLine("5");

                using (new LP(CS.Writer.GetLinePoint("//aa")))
                {
                    CS.Writer.WriteLine("aa 1");
                    CS.Writer.WriteLine("aa 2");
                }

                CS.Writer.WriteLine("6");
            }
        }
    }
}
