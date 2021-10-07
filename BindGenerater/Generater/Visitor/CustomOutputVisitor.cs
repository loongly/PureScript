using Generater;
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
    static HashSet<string> IgnoreUsing = Binder.IgnoreUsing;

    protected bool isNested;
    public List<string> nestedUsing = new List<string>();
    public HashSet<string> IgnoreNestType = new HashSet<string>();
    public HashSet<string> InternalTypeRef = new HashSet<string>();

    public CustomOutputVisitor(bool _isNested, TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(textWriter, formattingPolicy)
    {
        isNested = _isNested;
    }

    protected override void WriteAttributes(IEnumerable<AttributeSection> attributes)
    {
        foreach (var attSec in attributes)
        {
            foreach (var att in attSec.Attributes)
            {
                var t = att.Type.Annotation<ResolveResult>(); // .Annotations.First() as ResolveResult
                var td = t.Type as ITypeDefinition;
                if (td.IsBuiltinAttribute() == KnownAttribute.None)
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

        if (!IgnoreUsing.Contains(usingDeclaration.Namespace))
            base.VisitUsingDeclaration(usingDeclaration);
    }

    public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        var type = typeDeclaration.Annotation<ResolveResult>().Type;
        if (IgnoreNestType.Contains(type.Name))
            return;

        if (typeDeclaration.ClassType == ClassType.Struct || typeDeclaration.ClassType == ClassType.Class)
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

    public override void VisitSimpleType(SimpleType simpleType)
    {
        var res = simpleType.Resolve() as TypeResolveResult;
        if(res != null )
        {
            var td = res.Type.GetDefinition();
            if(td != null && td.Accessibility != Accessibility.Public && td.DeclaringType == null)
                InternalTypeRef.Add(res.Type.FullName);
        }
        base.VisitSimpleType(simpleType);
    }
}

public class BlittableOutputVisitor : CustomOutputVisitor
{
    public BlittableOutputVisitor(bool _isNested, TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(_isNested,textWriter, formattingPolicy)
    {
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
}

public class MethodDeclearVisitor: CustomOutputVisitor
{
    bool hasBodyBlock;
    public MethodDeclearVisitor(bool outputBodyBlock,TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(false, textWriter, formattingPolicy)
    {
        hasBodyBlock = outputBodyBlock;
    }

    protected override void WriteMethodBody(BlockStatement body, BraceStyle style)
    {
        if (!hasBodyBlock)
            return;
        base.WriteMethodBody(body, style);
    }

    public override void VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
    {
        if (!hasBodyBlock)
            return;
        base.VisitConstructorInitializer(constructorInitializer);
    }

}