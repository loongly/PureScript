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
        public MethodGenerater(MethodDefinition method)
        {
            genMethod = method;
        }

        public override void Gen()
        {
            writer = CS.Writer;
            base.Gen();

            if (!genMethod.IsPublic)
                return;

            GenerateBindings.AddMethod(genMethod);
            if (genMethod.IsGetter)
                GenGeter();
            else if (genMethod.IsSetter)
                GenSeter();
            else
                GenMethod();
        }

        void GenGeter()
        {
            writer.Start("get");
            BindResolver.Resolve(genMethod.ReturnType).Call(genMethod, "res");
            writer.WriteLine($"return res");
            writer.End();
        }

        void GenSeter()
        {
            writer.Start("set");
            BindResolver.Resolve(genMethod.ReturnType).Call(genMethod, "");
           // writer.WriteLine(Utils.BindMethodName(genMethod) );
            writer.End();
        }

        void GenMethod()
        {
            writer.Start(GetMethodDelcear());
            if(genMethod.IsConstructor)
            {
                CS.Writer.WriteLine($"var h = {Utils.BindMethodName(genMethod)}"); ;
                writer.WriteLine($"SetHandle(h)");
            }
            else
            {
                var res = BindResolver.Resolve(genMethod.ReturnType).Call(genMethod, "res");
                writer.WriteLine($"return {res}");
            }
            
            writer.End();
        }


        string GetMethodDelcear()
        {
            string declear = "public ";
            if (genMethod.IsStatic)
                declear += "static ";
            if (!genMethod.IsConstructor)
            {
                declear += genMethod.ReturnType.FullName + " ";
                declear += genMethod.Name;
            }
            else
            {
                declear += genMethod.DeclaringType.Name;
            }

            var param = "(";
            var lastP = genMethod.Parameters.LastOrDefault();
            foreach (var p in genMethod.Parameters)
            {
                var type = p.ParameterType;
                var typeName = type.Name;
                if (type.IsGenericInstance)
                    typeName = Utils.GetGenericTypeName(type);

                param += $"{typeName} {p.Name}" + (p == lastP ? "" : ", ");
            }
            param += ")";

            declear += param;

            return declear;
        }
    }
}