using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
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
    static CustomOutputVisitor()
    {
        ignoreUsing.Add("UnityEngine.Internal");
        ignoreUsing.Add("UnityEngine.Scripting.APIUpdating");
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
        if(typeDeclaration.ClassType != ClassType.Enum)
        {
            typeDeclaration.BaseTypes.Clear();
        }
        
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
        base.VisitOperatorDeclaration(operatorDeclaration);
    }

    public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        return;
        base.VisitMethodDeclaration(methodDeclaration);
    }
    public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        return;
        base.VisitPropertyDeclaration(propertyDeclaration);
    }
}
