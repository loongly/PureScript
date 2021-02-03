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

        List<MethodGenerater> methods = new List<MethodGenerater>();

        public PropertyGenerater(PropertyDefinition property)
        {
            genProperty = property;
            if (genProperty.GetMethod != null && genProperty.GetMethod.IsPublic && Utils.Filter(genProperty.GetMethod))
                methods.Add(new MethodGenerater(genProperty.GetMethod));
            if (genProperty.SetMethod != null && genProperty.SetMethod.IsPublic && Utils.Filter(genProperty.SetMethod))
                methods.Add(new MethodGenerater(genProperty.SetMethod));

        }

        public PropertyGenerater(FieldDefinition field)
        {//TODO: PropertyGenerater(FieldDefinition field)
            genField = field;
        }

        public override void Gen()
        {
            base.Gen();

            if (genProperty == null || methods.Count < 1)
                return;

            CS.Writer.Start($"public {genProperty.PropertyType.Name} {genProperty.Name}");

            foreach (var m in methods)
                m.Gen();

            CS.Writer.End();


        }

    }
}