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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

public class CustomOutputVisitor : CSharpOutputVisitor
{
    static HashSet<string> IgnoreUsing = Config.Instance.StripUsing;

    protected bool isNested;
    public List<string> nestedUsing = new List<string>();
    public HashSet<string> IgnoreNestType = new HashSet<string>();
    public HashSet<string> InternalTypeRef = new HashSet<string>();
    public HashSet<string> stripInterfaceSet = new HashSet<string>();

    public bool AddWObject = false;
    public bool isFullRetain = false;
    string curTypeName = null;

    public CheckTypeRefVisitor checkTypeRefVisitor;
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
                if(t != null)
                {
                    var td = t.Type as ITypeDefinition;
                    if (td != null )
                    {
                        if(td.IsBuiltinAttribute() == KnownAttribute.None)
                            att.Remove();
                    }

                    if (t.Type.Name == "StructLayoutAttribute") // fix StructLayoutAttribute bug
                    {
                        var firstArg = att.Arguments.First();
                        var primitExp = firstArg as PrimitiveExpression;
                        if(primitExp != null)
                        {
                            if(primitExp.Value.GetType() == typeof(int))
                            {
                                var layout = (LayoutKind)primitExp.Value;
                                firstArg.ReplaceWith(new IdentifierExpression("LayoutKind."+ layout));
                            }
                        }
                    }

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
            var res = t.Annotation<ResolveResult>();
            var td = res.Type.GetDefinition();
            var fullName = td == null ? res.Type.FullName : td.FullTypeName.ReflectionName;
            if ((res.Type.Kind == TypeKind.Interface && !res.Type.Namespace.StartsWith("System") && !Binder.retainTypes.Contains(fullName)) || stripInterfaceSet.Contains(res.Type.Name))
                dList.Add(t);
        }
        foreach (var t in dList)
            typeDeclaration.BaseTypes.Remove(t);

        if (AddWObject)
            typeDeclaration.BaseTypes.InsertBefore(typeDeclaration.BaseTypes.FirstOrDefault(),new SimpleType("WObject"));

        /*var wrapFlag = new AttributeSection();
        var wrapAttr = new Attribute();
        wrapAttr.Type = new SimpleType("WrapperClass");
        wrapAttr.Arguments.Add(new PrimitiveExpression(Binder.curModule.Name));
        wrapFlag.Attributes.Add(wrapAttr);
        typeDeclaration.Attributes.Add(wrapFlag);*/
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

    public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        if(checkTypeRefVisitor != null)
        {
            var collectRes = methodDeclaration.AcceptVisitor(checkTypeRefVisitor);
            if (collectRes != CheckTypeRefVisitor.Result.Ok)
                return;
        }
       

        base.VisitMethodDeclaration(methodDeclaration);
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

        if (fieldDeclaration.HasModifier(Modifiers.Static) && fieldDeclaration.HasModifier(Modifiers.Readonly) && !fieldDeclaration.HasModifier(Modifiers.Public))
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

public class CheckTypeRefVisitor:DepthFirstAstVisitor<CheckTypeRefVisitor.Result>
{
    public enum Result
    {
        Ok = 0,
        Error = 1,
    }
    public Func<string, bool> CheckRefFunc;
    public HashSet<string> TypeRefSet = new HashSet<string>();
    public HashSet<string> IgnorRefSet = new HashSet<string>();

    public CheckTypeRefVisitor(Func<string, bool> func)
    {
        CheckRefFunc = func;
    }

    protected override Result VisitChildren(AstNode node)
    {
        Result res = Result.Ok;
        AstNode next;
        for (var child = node.FirstChild; child != null; child = next)
        {
            next = child.NextSibling;
            res |= child.AcceptVisitor(this);
            if (res == Result.Error && !(node is TypeDeclaration))
                return Result.Error;
        }
        return res;
    }

    public override Result VisitSimpleType(SimpleType simpleType)
    {
        var res = simpleType.Resolve() as TypeResolveResult;
        if (res != null)
        {
            var td = res.Type.GetDefinition();
            if (td != null && !td.Namespace.StartsWith("System"))
            {
                var name = td.FullTypeName.ReflectionName;
                if (TypeRefSet.Contains(name))
                    return Result.Ok;
                else if (IgnorRefSet.Contains(name))
                    return Result.Error;

                if (CheckRefFunc(name))
                {
                    TypeRefSet.Add(name);
                    return Result.Ok;
                }
                else
                {
                    IgnorRefSet.Add(name);
                    return Result.Error;
                }
            }
        }
        return base.VisitSimpleType(simpleType);
    }

    public override Result VisitAttributeSection(AttributeSection attributeSection)
    {
        return Result.Ok;
    }
}