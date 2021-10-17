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
    public HashSet<string> stripInterfaceSet = new HashSet<string>();

    public bool AddWObject = false;
    public bool isFullRetain = false;
    string curTypeName = null;
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

    protected void ResolveTypeDeclear(TypeDeclaration typeDeclaration)
    {
        if (typeDeclaration.ClassType == ClassType.Struct || typeDeclaration.ClassType == ClassType.Class)
        {
            typeDeclaration.Modifiers |= Modifiers.Partial;
            if (typeDeclaration.HasModifier(Modifiers.Readonly))
                typeDeclaration.Modifiers ^= Modifiers.Readonly;
        }
        /*else if(typeDeclaration.ClassType == ClassType.Interface)
        {
            typeDeclaration.BaseTypes.Add(new SimpleType("IWObject"));
        }*/

        List<AstType> dList = new List<AstType>();
        foreach (var t in typeDeclaration.BaseTypes)
        {
            var at = t.Annotation<ResolveResult>();
            if (at.Type.Kind == TypeKind.Interface && (!at.Type.Namespace.StartsWith("System") || stripInterfaceSet.Contains(at.Type.Name)))
                dList.Add(t);
        }
        foreach (var t in dList)
            typeDeclaration.BaseTypes.Remove(t);

        if (AddWObject)
            typeDeclaration.BaseTypes.InsertBefore(typeDeclaration.BaseTypes.FirstOrDefault(),new SimpleType("WObject"));
    }

    public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        if (!string.IsNullOrEmpty(curTypeName) && !isFullRetain)
            return;

        curTypeName = typeDeclaration.Name;
        var type = typeDeclaration.Annotation<ResolveResult>().Type;
        if (IgnoreNestType.Contains(type.Name))
            return;

        ResolveTypeDeclear(typeDeclaration);

        base.VisitTypeDeclaration(typeDeclaration);
    }

    public override void VisitSimpleType(SimpleType simpleType)
    {
        var res = simpleType.Resolve() as TypeResolveResult;
        if(res != null )
        {
            var td = res.Type.GetDefinition();
            if(td != null && !td.Namespace.StartsWith("System")) 
                InternalTypeRef.Add(td.FullTypeName.ReflectionName);
        }
        base.VisitSimpleType(simpleType);
    }
}

public class BlittablePartOutputVisitor : CustomOutputVisitor
{
    public BlittablePartOutputVisitor(bool _isNested, TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(_isNested, textWriter, formattingPolicy)
    {
    }

    public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
    {
        if(fieldDeclaration.HasModifier(Modifiers.Static) && !fieldDeclaration.HasModifier(Modifiers.Readonly))
            return;

        base.VisitFieldDeclaration(fieldDeclaration);
    }

    public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
    {
        return;
    }

    public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
    {
        return;
    }

    public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
    {
        return;
    }

    public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        return;
    }
    public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        return;
    }
}


public class MemberDeclearVisitor: CustomOutputVisitor
{
    bool hasBodyBlock;
    
    public MemberDeclearVisitor(bool outputBodyBlock,TextWriter textWriter, CSharpFormattingOptions formattingPolicy) : base(false, textWriter, formattingPolicy)
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

    public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        if (!hasBodyBlock)
        {
            StartNode(propertyDeclaration);
            WriteAttributes(propertyDeclaration.Attributes);
            WriteModifiers(propertyDeclaration.ModifierTokens);
            propertyDeclaration.ReturnType.AcceptVisitor(this);
            Space();
            WritePrivateImplementationType(propertyDeclaration.PrivateImplementationType);
            WriteIdentifier(propertyDeclaration.NameToken);
            EndNode(propertyDeclaration);
            return;
        }
        base.VisitPropertyDeclaration(propertyDeclaration);
    }

    public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        ResolveTypeDeclear(typeDeclaration);

        if (!hasBodyBlock)
        {
            StartNode(typeDeclaration);
            WriteModifiers(typeDeclaration.ModifierTokens);
            BraceStyle braceStyle;
            switch (typeDeclaration.ClassType)
            {
                case ClassType.Enum:
                    WriteKeyword(Roles.EnumKeyword);
                    braceStyle = policy.EnumBraceStyle;
                    break;
                case ClassType.Interface:
                    WriteKeyword(Roles.InterfaceKeyword);
                    braceStyle = policy.InterfaceBraceStyle;
                    break;
                case ClassType.Struct:
                    WriteKeyword(Roles.StructKeyword);
                    braceStyle = policy.StructBraceStyle;
                    break;
                default:
                    WriteKeyword(Roles.ClassKeyword);
                    braceStyle = policy.ClassBraceStyle;
                    break;
            }
            WriteIdentifier(typeDeclaration.NameToken);
            WriteTypeParameters(typeDeclaration.TypeParameters);
            if (typeDeclaration.BaseTypes.Any())
            {
                Space();
                WriteToken(Roles.Colon);
                Space();
                WriteCommaSeparatedList(typeDeclaration.BaseTypes);
            }

            return;
        }

        base.VisitTypeDeclaration(typeDeclaration);
    }

}