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
            res |= child.AcceptVisitor(this);
            if (res && !(node is TypeDeclaration))
                return true;
        }
        return res;
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
        
        if(IsInternCallNode(methodDeclaration) || methodDeclaration.HasModifier(Modifiers.Public))
            retain = !base.VisitMethodDeclaration(methodDeclaration);

        /*if (imethod.IsExtensionMethod)
            retain = true;*/

        if (retain)
        {
            RetainDic[res.Member.MetadataToken.GetHashCode()] = methodDeclaration;
            RequiredNamespaceCollector.CollectNamespaces(res.Member, module, NamespaceRef);
        }

        return !retain;
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
            bool res = NeedWrap(target.Member);
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
        if(simpleType.Identifier == "Internal_DrawTextureArguments")
        {
            int c = 0;
        }
        var res = simpleType.Resolve() as TypeResolveResult;
        if(res != null)
        {
            var td = res.Type.GetDefinition();
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
        var member = memberReferenceExpression.Target.Resolve();
        var mres = member as MemberResolveResult;
        if (mres != null)
        {
            bool res = NeedWrap(mres.Member);
            if (res)
                return true;
        }

        return base.VisitMemberReferenceExpression(memberReferenceExpression);
    }

    #endregion

    protected bool NeedWrap(IMember member)
    {
        bool res = false;
        var kind = member.SymbolKind;
        var accable = member.Accessibility;

        if (kind == SymbolKind.Property)
        {
            var property = member as IProperty;
            
            if (property.Getter != null)
                res |= NeedWrap(property.Getter);
            if (property.Setter != null)
                res |= NeedWrap(property.Setter);
        }
        else if(kind == SymbolKind.Method)
        {
            var method = member as IMethod;
            if (method != null)
                res |= NeedWrap(method);
        }
        else
        {
            res = true;
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
        return attr != null;

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


public class RetainNode
{
    public string token;
    public AstNode node;
    public RetainNode(string _token ,AstNode _node)
    {
        token = _token;
        node = _node;
    }
}