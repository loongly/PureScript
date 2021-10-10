using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Generater
{
    public class MethodGenerater : CodeGenerater
    {
        MethodDefinition genMethod;
        CodeWriter writer;

        bool isNotImplement = false;
        public MethodGenerater(MethodDefinition method)
        {
            genMethod = method;
            isNotImplement = !Utils.Filter(method);

            if (isNotImplement)//maybe generate a empty method?
                return;

            if (!genMethod.IsPublic && !genMethod.DeclaringType.IsInterface)
                return;
            if (genMethod.IsConstructor && genMethod.DeclaringType.IsAbstract)
                return;

            foreach (var p in genMethod.Parameters)
            {
                var type = p.ParameterType;
                Binder.AddType(type.Resolve());
            }
            Binder.AddType(genMethod.ReturnType.Resolve());

            if (!method.IsAbstract && !isNotImplement)
                GenerateBindings.AddMethod(genMethod);
        }

        public override void Gen()
        {
            if (isNotImplement)
                return;

            writer = CS.Writer;
            base.Gen();

            if (!genMethod.IsPublic && !genMethod.DeclaringType.IsInterface)
                return;
            if (genMethod.IsConstructor && genMethod.DeclaringType.IsAbstract)
                return;

            if (genMethod.IsGetter)
                GenGeter();
            else if (genMethod.IsSetter)
                GenSeter();
            else if (genMethod.IsAddOn)
                GenAddOn();
            else if (genMethod.IsRemoveOn)
                GenRemoveOn();
            else if (genMethod.IsAbstract)
                GenAbstract();
            else
                GenMethod();
        }

        void GenGeter()
        {
            if(genMethod.IsAbstract)
            {
                writer.WriteLine("get");
                return;
            }
            writer.Start("get");
            if(isNotImplement)
            {
                WriteNotImplemented();
            }
            else
            {
                var res = MethodResolver.Resolve(genMethod).Call("res");
                writer.WriteLine("ScriptEngine.CheckException()");
                writer.WriteLine($"return {res}");
            }
            
            writer.End();
        }

        void GenSeter()
        {
            if (genMethod.IsAbstract)
            {
                writer.WriteLine("set");
                return;
            }

            writer.Start("set");
            if (isNotImplement)
            {
                WriteNotImplemented();
            }
            else
            {
                MethodResolver.Resolve(genMethod).Call("");
                writer.WriteLine("ScriptEngine.CheckException()");
            }
                
            writer.End();
        }

        void GenAddOn()
        {
            writer.Start("add");
            if (isNotImplement)
            {
                WriteNotImplemented();
            }
            else
            {
                MethodResolver.Resolve(genMethod).Call("");
                writer.WriteLine("ScriptEngine.CheckException()");
            }
                
            writer.End();
        }

        void GenRemoveOn()
        {
            writer.Start("remove");
            if (isNotImplement)
            {
                WriteNotImplemented();
            }
            else
            {
                MethodResolver.Resolve(genMethod).Call("");
                writer.WriteLine("ScriptEngine.CheckException()");
            }
            writer.End();
        }

        void GenMethod()
        {
            writer.Start(GetMethodDelcear());

            if (isNotImplement)
            {
                WriteNotImplemented();
                writer.End();
                return;
            }

            if (genMethod.IsConstructor)
            {
                if(genMethod.DeclaringType.IsValueType)
                {
                    CS.Writer.WriteLine(Utils.BindMethodName(genMethod));
                    writer.WriteLine("ScriptEngine.CheckException()");
                }
                else
                {
                    CS.Writer.WriteLine($"var h = {Utils.BindMethodName(genMethod)}");
                    writer.WriteLine("ScriptEngine.CheckException()");
                    CS.Writer.WriteLine($"SetHandle(h)");
                    CS.Writer.WriteLine("ObjectStore.Store(this, h)");
                }
            }
            else
            {
                var res = MethodResolver.Resolve(genMethod).Call("res");
                writer.WriteLine("ScriptEngine.CheckException()");
                writer.WriteLine($"return {res}");
            }
            
            writer.End();
        }

        void GenAbstract()
        {
            writer.WriteLine(GetMethodDelcear());
        }


        string GetMethodDelcear()
        {
            string declear = Utils.GetMethodDelcear(genMethod);

            if (genMethod.IsConstructor && genMethod.DeclaringType.IsValueType)
                declear += ":this()";

            return declear;
        }

        void WriteNotImplemented()
        {
            CS.Writer.WriteLine("throw new System.NotImplementedException()");
        }

    }
}