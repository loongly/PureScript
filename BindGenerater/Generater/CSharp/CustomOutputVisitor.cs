using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CustomOutputVisitor : CSharpOutputVisitor
{
    static HashSet<string> ignoreUsing = new HashSet<string>();
    static HashSet<string> includeMethod = new HashSet<string>();
    static CustomOutputVisitor()
    {
        ignoreUsing.Add("UnityEngine.Internal");
        ignoreUsing.Add("UnityEngine.Scripting.APIUpdating");
       // includeMethod.Add("Equals");
/*
        includeMethod.Add("Dispose");
        includeMethod.Add("MoveNext");
        includeMethod.Add("Reset");
        includeMethod.Add("Current");*/
    }

    bool isNested;
    public List<string> nestedUsing = new List<string>();
    public CustomOutputVisitor(bool _isNested, TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(textWriter, formattingPolicy)
    {
        isNested = _isNested;
    }

    protected override void WriteAttributes(IEnumerable<AttributeSection> attributes)
    {
        foreach (var attSec in attributes)
        {
            foreach(var att in attSec.Attributes)
            {
                var t = att.Type.Annotation<ResolveResult>(); // .Annotations.First() as ResolveResult
                var td = t.Type as ITypeDefinition;
                if(td.IsBuiltinAttribute() == KnownAttribute.None)
                {
                    att.Remove();
                }
            }

            if (attSec.Attributes.Count() <= 0)
                attSec.Remove();
        }
        if (attributes.Count() > 0)
        {
            base.WriteAttributes(attributes);
        }
    }

    public override void VisitUsingDeclaration(UsingDeclaration usingDeclaration)
    {
        if (isNested)
        {
            nestedUsing.Add(usingDeclaration.Namespace);
            return;
        }
        if(!ignoreUsing.Contains(usingDeclaration.Namespace))
            base.VisitUsingDeclaration(usingDeclaration);
    }


    public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
    {
        
        foreach(var token in fieldDeclaration.ModifierTokens)
        {
            if (token.Modifier == Modifiers.Static)
                return;
        }

        base.VisitFieldDeclaration(fieldDeclaration);
    }

    public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        /*if(typeDeclaration.ClassType != ClassType.Enum)
        {
            typeDeclaration.BaseTypes.Clear();
        }*/

        if(typeDeclaration.ClassType == ClassType.Struct)
        {
            typeDeclaration.Modifiers |= Modifiers.Partial;
        }

        List<AstType> dList = new List<AstType>();
        foreach (var t in typeDeclaration.BaseTypes)
        {
            var at = t.Annotation<ResolveResult>();
            if (at.Type.Kind == TypeKind.Interface && !at.Type.Namespace.StartsWith("System"))
                dList.Add(t);
        }
        foreach (var t in dList)
            typeDeclaration.BaseTypes.Remove(t);

        base.VisitTypeDeclaration(typeDeclaration);
    }

    public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
    {
        return;
        base.VisitIndexerDeclaration(indexerDeclaration);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
    {
        return ;
        base.VisitConstructorDeclaration(constructorDeclaration);
    }

    public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
    {
        return;

        //if (operatorDeclaration.Name == "op_Implicit" || operatorDeclaration.Name == "op_Explicit")
           
        base.VisitOperatorDeclaration(operatorDeclaration);
    }

    public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        bool forceWrite = false;
        if (!methodDeclaration.HasModifier(Modifiers.Public) && methodDeclaration.Name == "Dispose")
            forceWrite = true;

        if (includeMethod.Contains(methodDeclaration.Name) || forceWrite)
            base.VisitMethodDeclaration(methodDeclaration);
    }
    public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        if (!propertyDeclaration.HasModifier(Modifiers.Public) && propertyDeclaration.Name == "Current")
            base.VisitPropertyDeclaration(propertyDeclaration);
    }
}


/*
public class CheckMethodBodyVisible: CSharpOutputVisitor
{
    public bool IsVisible = true;

    public CheckMethodBodyVisible(TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(textWriter, formattingPolicy)
    {
    }

    public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        if (!IsVisible)
            return;

        IsVisible = methodDeclaration.HasModifier(Modifiers.Public);
        base.VisitMethodDeclaration(methodDeclaration);
    }
    public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        if (!IsVisible)
            return;

        IsVisible = propertyDeclaration.HasModifier(Modifiers.Public);

        base.VisitPropertyDeclaration(propertyDeclaration);
    }
}*/