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

        

        public ClassGenerater(TypeDefinition type)
        {
            genType = type;

            foreach (FieldDefinition field in genType.Fields)
            {
                properties.Add(new PropertyGenerater(field));
            }

            foreach (PropertyDefinition prop in genType.Properties)
            {
                properties.Add(new PropertyGenerater(prop));
            }

            foreach (MethodDefinition method in genType.Methods)
            {
                if(method.IsPublic && !method.IsGetter && !method.IsSetter)
                    methods.Add(new MethodGenerater(method));
            }
        }


        public override void Gen()
        {
            base.Gen();

            if (!string.IsNullOrEmpty(genType.Namespace))
            {
                CS.Writer.Start($"namespace {genType.Namespace}");
            }

            var classDefine = $"public class {genType.Name}";

            if(genType.BaseType != null)
            {
                classDefine += $" : {genType.BaseType.FullName}";
            }

            CS.Writer.Start(classDefine);

            foreach(var p in properties)
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