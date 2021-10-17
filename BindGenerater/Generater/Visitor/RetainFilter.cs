using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Generater;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using AstAttribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;


public class RetainFilter :DepthFirstAstVisitor<bool>
{
    public Dictionary<int, AstNode> RetainDic = new Dictionary<int, AstNode>();
    public HashSet<string> NamespaceRef = new HashSet<string>();
    private int targetTypeToken;
    private MetadataModule module;
    public Dictionary<int, AstNode> TokenMap = new Dictionary<int, AstNode>();
    private HashSet<AstNode> pendingSet = new HashSet<AstNode>();
    private string curTypeName;
    public bool InUnsafeNS;
    public bool isFullValueType;


    public RetainFilter(int tarTypeToken, CSharpDecompiler decompiler)
    {
        module = decompiler.TypeSystem.MainModule;
        targetTypeToken = tarTypeToken;
    }

    // return true if need Wrap
    protected override bool VisitChildren(AstNode node)
    {
        bool res = false;
        AstNode next;
        for (var child = node.FirstChild; child != null; child = next)
        {
            next = child.NextSibling;
            StartNode(child);
            res |= child.AcceptVisitor(this);
            EndNode(child);
            if (res && !(node is TypeDeclaration))
                return true;
        }
        return res;
    }

    void StartNode(AstNode node)
    {
        pendingSet.Add(node);
    }
    void EndNode(AstNode node)
    {
        pendingSet.Remove(node);
    }

    #region Declaration
   
    public override bool VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        bool wrap = base.VisitPropertyDeclaration(propertyDeclaration);

        if(!wrap)
        {
            var res = propertyDeclaration.Resolve() as MemberResolveResult;
            RetainDic[res.Member.MetadataToken.GetHashCode()] = propertyDeclaration;
            RequiredNamespaceCollector.CollectNamespaces(res.Member,module, NamespaceRef);
        }

        return wrap;
    }

    public override bool VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        var res = methodDeclaration.Resolve() as MemberResolveResult;
        var imethod = res.Member as IMethod;

        bool retain = false;
        
        var isICall = IsInternCallNode(methodDeclaration);
        if (!isICall && InUnsafeNS)
            return true;

        if (isICall || methodDeclaration.HasModifier(Modifiers.Public))
            retain = !base.VisitMethodDeclaration(methodDeclaration);

        if (!retain)
        {
            if (methodDeclaration.Name == "ToString" && methodDeclaration.Parameters.Count == 2) // IFormattable
                retain = true;
        }

        if (retain)
        {
            RetainDic[res.Member.MetadataToken.GetHashCode()] = methodDeclaration;
            RequiredNamespaceCollector.CollectNamespaces(res.Member, module, NamespaceRef);
        }

        return !retain;
    }

    public override bool VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
    {
        
        var retain = !base.VisitConstructorDeclaration(constructorDeclaration);
        if (retain)
        {
            var res = constructorDeclaration.Resolve() as MemberResolveResult;
            RetainDic[res.Member.MetadataToken.GetHashCode()] = constructorDeclaration;
            RequiredNamespaceCollector.CollectNamespaces(res.Member, module, NamespaceRef);
        }

        return !retain;
    }

    public override bool VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
    {
        var retain = !base.VisitIndexerDeclaration(indexerDeclaration);
        if (retain)
        {
            var res = indexerDeclaration.Resolve() as MemberResolveResult;
            RetainDic[res.Member.MetadataToken.GetHashCode()] = indexerDeclaration;
            RequiredNamespaceCollector.CollectNamespaces(res.Member, module, NamespaceRef);
        }

        return !retain;
    }

    public override bool VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
    {
        if (isFullValueType)
        {
            if (fieldDeclaration.HasModifier(Modifiers.Static) && !fieldDeclaration.HasModifier(Modifiers.Readonly))
                return true;

            return false;
        }

        //return fieldDeclaration.HasModifier(Modifiers.Public);
        return true;
    }

    public override bool VisitAccessor(Accessor accessor)
    {
        if (accessor.Body.IsNull)
        {
            return !IsInternCallNode(accessor);
        }

        return base.VisitAccessor(accessor);
    }

    #endregion

    #region Filter

    public override bool VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        if (typeDeclaration.GetToken() != targetTypeToken)
            return false;

        curTypeName = typeDeclaration.Name;
        return base.VisitTypeDeclaration(typeDeclaration);
    }

    public override bool VisitAttribute(ICSharpCode.Decompiler.CSharp.Syntax.Attribute attribute)
    {
        return false;
    }

    // aa(bb);
    public override bool VisitInvocationExpression(InvocationExpression invocationExpression)
    {
        var target = invocationExpression.Resolve() as CSharpInvocationResolveResult;

        if (target != null)
        {
            bool res = NeedWrap(target.Member, invocationExpression);
            if (res)
                return true;
        }
        return base.VisitInvocationExpression(invocationExpression);
    }

    public override bool VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
    {
        var target = objectCreateExpression.Resolve() as CSharpInvocationResolveResult;
        if (target != null)
        {
            bool res = NeedWrap(target.Member);
            if (res)
                return true;
        }

        return base.VisitObjectCreateExpression(objectCreateExpression);
    }

    // T0.xx  /  T1 func(T2 aa,T3<T4> bb)  /  (T5)xx
    public override bool VisitSimpleType(SimpleType simpleType)
    {
        var res = simpleType.Resolve() as TypeResolveResult;
        if(res != null && res.Type.Kind != TypeKind.TypeParameter)
        {
            var td = res.Type.GetDefinition();
            if (td!= null)
            {
                if(Binder.retainTypes.Contains(td.FullTypeName.ReflectionName))
                    return false;
                if (!td.Namespace.StartsWith("System") && !Utils.Filter(td))
                    return true;
            }

            if (td == null || (td.Accessibility != Accessibility.Public && !Utils.IsFullValueType(td)))
                return true;
        }

        return base.VisitSimpleType(simpleType);
    }

    public override bool VisitIdentifierExpression(IdentifierExpression identifierExpression)
    {
        var member = identifierExpression.Resolve();
        if(member != null)
        {
            var mres = member as MemberResolveResult;
            if (mres != null)
            {
                bool res = NeedWrap(mres.Member);
                if (res)
                    return true;
            }
        }

        return base.VisitIdentifierExpression(identifierExpression);
    }

    //aa.bb
    public override bool VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
    {
        IMember imember = null;
        var member = memberReferenceExpression.Target.Resolve();
        var typeMember = member as TypeResolveResult;
        if (typeMember != null )
        {
            if (typeMember.Type.Kind == TypeKind.Enum || Binder.retainTypes.Contains(typeMember.Type.ReflectionName) || typeMember.Type.Namespace.StartsWith("System"))
                return false;

            return true;
        }

        member = memberReferenceExpression.MemberNameToken.Resolve();
        if(member == null)
            member = memberReferenceExpression.Resolve();
        var mres = member as MemberResolveResult;
        if (mres != null)
        {
            bool res = NeedWrap(mres.Member);
            if (res)
                return true;
        }

        return base.VisitMemberReferenceExpression(memberReferenceExpression);
    }

    public override bool VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
    {
        var target = constructorInitializer.Resolve() as CSharpInvocationResolveResult;

        if (target != null)
        {
            bool res = NeedWrap(target.Member);
            if (res)
                return true;
        }

        return base.VisitConstructorInitializer(constructorInitializer);
    }

    #endregion

    protected bool NeedWrap(IMember member, AstNode pNode = null)
    {
        if (member.DeclaringType.Namespace.StartsWith("System"))
            return false;
        if (Binder.retainTypes.Contains(member.DeclaringTypeDefinition.ReflectionName))
            return false;

        bool res = true;
        var kind = member.SymbolKind;
        var accable = member.Accessibility;

        if(kind == SymbolKind.Property || kind == SymbolKind.Method || kind == SymbolKind.Field || kind == SymbolKind.Constructor)
        {
            var token = member.MetadataToken.GetHashCode();
            if(TokenMap.TryGetValue(token,out var node))
            {
                if (pendingSet.Contains(node))
                    return false;
                StartNode(node);
                res = node.AcceptVisitor(this);
                EndNode(node);

                if(!res && pNode != null)
                {
                    var pEntity = pNode.GetParentOf<EntityDeclaration>();
                    var cEntity = node as EntityDeclaration;
                    if (cEntity.HasModifier(Modifiers.Unsafe) && !pEntity.HasModifier(Modifiers.Unsafe))
                        pEntity.Modifiers |= Modifiers.Unsafe;
                }
            }
        }

        return res;
    }

    protected bool NeedWrap(IMethod method)
    {
        var accable = method.AccessorOwner != null ? method.AccessorOwner.Accessibility : method.Accessibility;
        if (accable == Accessibility.Public)
            return false;

        var attr = method.GetAttribute(KnownAttribute.MethodImpl);
        if (attr != null)
        {
            var arg = attr.FixedArguments.FirstOrDefault();
            if ((int)arg.Value == (int)MethodImplOptions.InternalCall)
                return false;
        }

        return true;
    }

    protected bool IsInternCallNode(AstNode node)
    {
        var attrs = node.GetChildsOf<AstAttribute>() ;
        var attr = attrs.Find(at => at.Type.SimpleTypeName() == "MethodImpl");
        if(attr != null)
        {
            var arg = attr.Arguments.FirstOrDefault() as MemberReferenceExpression;
            if(arg != null && arg.MemberName == "InternalCall")
            {
                return true;
            }
        }
        return false;

        /*var res = Resolve(node) as MemberResolveResult;
        if(res != null)
        {
            var attr = res.Member.GetAttribute(KnownAttribute.MethodImpl);
            if (attr != null)
            {
                var arg = attr.FixedArguments.FirstOrDefault();
                if ((int)arg.Value == (int)MethodImplOptions.InternalCall)
                    return true;
            }
        }
        return false;*/
    }



}
