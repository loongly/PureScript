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

    public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
    {
        InsertMap(constructorDeclaration);
        base.VisitConstructorDeclaration(constructorDeclaration);
    }

    public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
    {
        InsertMap(operatorDeclaration);
        base.VisitOperatorDeclaration(operatorDeclaration);
    }

    void InsertMap(AstNode node)
    {
        TokenMap[node.GetToken()] = node;
    }

}
