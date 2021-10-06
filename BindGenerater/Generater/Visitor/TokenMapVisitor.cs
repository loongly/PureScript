using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TokenMapVisitor: DepthFirstAstVisitor
{
    public Dictionary<int, AstNode> TokenMap = new Dictionary<int, AstNode>();

    public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
    {
        InsertMap(propertyDeclaration);
        base.VisitPropertyDeclaration(propertyDeclaration);
    }
    public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        InsertMap(methodDeclaration);
        base.VisitMethodDeclaration(methodDeclaration);
    }

    void InsertMap(AstNode node)
    {
        TokenMap[GetToken(node)] = node;
    }

    protected int GetToken(AstNode node)
    {
        var res = node.Annotation<ResolveResult>() as MemberResolveResult;
        if (res != null)
            return res.Member.MemberDefinition.MetadataToken.GetHashCode();

        return 0;
    }
}
