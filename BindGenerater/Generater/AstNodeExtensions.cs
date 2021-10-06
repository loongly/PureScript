using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class AstNodeExtensions
{

    public static string SimpleTypeName(this AstType tNode)
    {
        var simple = tNode as SimpleType;
        if (simple != null)
            return simple.Identifier;

        return null;
    }

    public static List<T> GetChildsOf<T>(this AstNode node) where T : AstNode
    {
        List<T> result = new List<T>();
        AstNode m = node as AstNode;
        foreach (AstNode n in m.Children)
        {
            if (n.GetType() == typeof(T))
            {
                result.Add(n as T);
            }
            result.AddRange(GetChildsOf<T>(n));
        }
        return result;

    }
    public static T GetParentOf<T>(this AstNode node) where T : AstNode
    {
        AstNode m = node as AstNode;
        while (m.Parent != null)
        {
            if (m.Parent.GetType() == typeof(T))
            {
                return m.Parent as T;
            }
            m = m.Parent;
        }
        return default(T);
    }
}
