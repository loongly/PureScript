using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Generater
{
    public class PropertyGenerater : CodeGenerater
    {
        PropertyDefinition genProperty;
        FieldDefinition genField;
 
        bool isStatic;

        List<MethodGenerater> methods = new List<MethodGenerater>();

        public PropertyGenerater(PropertyDefinition property)
        {
            genProperty = property;
            if (genProperty.GetMethod != null && genProperty.GetMethod.IsPublic && Utils.Filter(genProperty.GetMethod))
            {
                isStatic = genProperty.GetMethod.IsStatic;
                methods.Add(new MethodGenerater(genProperty.GetMethod));
            }
            if (genProperty.SetMethod != null && genProperty.SetMethod.IsPublic && Utils.Filter(genProperty.SetMethod))
            {
                isStatic = genProperty.SetMethod.IsStatic;
                methods.Add(new MethodGenerater(genProperty.SetMethod));
            }

        }

        public PropertyGenerater(FieldDefinition field)
        {//TODO: PropertyGenerater(FieldDefinition field)
            genField = field;
        }

        

        public override void Gen()
        {
            base.Gen();
            if (methods.Count < 1)
                return;

            if (genProperty != null)
                GenProperty();

        }

        void GenProperty()
        {
            var flag = isStatic ? "static " : "";
            CS.Writer.Start($"public {flag}{TypeResolver.Resolve(genProperty.PropertyType).RealTypeName()} {genProperty.Name}");

            foreach (var m in methods)
                m.Gen();

            CS.Writer.End();
        }

    }
}