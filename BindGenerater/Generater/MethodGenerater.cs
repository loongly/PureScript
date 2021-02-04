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

            if (!genMethod.IsPublic && !genMethod.DeclaringType.IsInterface)
                return;

            foreach (var p in genMethod.Parameters)
            {
                var type = p.ParameterType;
                Binder.AddType(type.Resolve());
            }
            Binder.AddType(genMethod.ReturnType.Resolve());

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
            var res =  MethodResolver.Resolve(genMethod).Call("res");
            writer.WriteLine($"return {res}");
            writer.End();
        }

        void GenSeter()
        {
            writer.Start("set");
            MethodResolver.Resolve(genMethod).Call("");
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
                var res = MethodResolver.Resolve(genMethod).Call("res");
                writer.WriteLine($"return {res}");
            }
            
            writer.End();
        }

        string GetMethodDelcear()
        {
           

            string declear = "public ";
            if (genMethod.IsStatic)
                declear += "static ";

            if (Utils.IsUnsafeMethod(genMethod))
                declear += "unsafe ";

            if (genMethod.IsOverride())
                declear += "override ";
            else if (genMethod.IsVirtual)
                declear += "virtual ";

            if (!genMethod.IsConstructor)
            {
                declear += TypeResolver.Resolve(genMethod.ReturnType).RealTypeName() + " ";
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
                var typeName = TypeResolver.Resolve(type).RealTypeName();
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