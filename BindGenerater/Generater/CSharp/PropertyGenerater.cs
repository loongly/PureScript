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
        bool isAbstract;
        bool isOverride;
        bool isVirtual;

        List<MethodGenerater> methods = new List<MethodGenerater>();

        public PropertyGenerater(PropertyDefinition property)
        {
            genProperty = property;
            if (genProperty.GetMethod != null && genProperty.GetMethod.IsPublic )
            {
                isStatic = genProperty.GetMethod.IsStatic;
                isAbstract = genProperty.GetMethod.IsAbstract;
                isOverride = genProperty.GetMethod.IsOverride();
                isVirtual = genProperty.GetMethod.IsVirtual && !genProperty.DeclaringType.IsValueType;
                methods.Add(new MethodGenerater(genProperty.GetMethod));
            }
            if (genProperty.SetMethod != null && genProperty.SetMethod.IsPublic )
            {
                isStatic = genProperty.SetMethod.IsStatic;
                isAbstract = genProperty.SetMethod.IsAbstract;
                isOverride = genProperty.SetMethod.IsOverride();
                isVirtual = genProperty.SetMethod.IsVirtual && !genProperty.DeclaringType.IsValueType;
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
            if (isAbstract)
                flag += "abstract ";
            else if (isOverride)
                flag += "override ";
            else if(isVirtual)
                flag += "virtual ";

            CS.Writer.Start($"public {flag}{TypeResolver.Resolve(genProperty.PropertyType).RealTypeName()} {genProperty.Name}");

            foreach (var m in methods)
                m.Gen();

            CS.Writer.End();
        }

    }
}