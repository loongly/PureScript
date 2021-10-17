using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

static internal class TypeDefinitionExtensions
{
   /// <summary>
   /// Is childTypeDef a subclass of parentTypeDef. Does not test interface inheritance
   /// </summary>
   /// <param name="childTypeDef"></param>
   /// <param name="parentTypeDef"></param>
   /// <returns></returns>
   public static bool IsSubclassOf(this TypeDefinition childTypeDef, TypeDefinition parentTypeDef) => 
      childTypeDef.MetadataToken 
          != parentTypeDef.MetadataToken 
          && childTypeDef
         .EnumerateBaseClasses()
         .Any(b => b.MetadataToken == parentTypeDef.MetadataToken);

   /// <summary>
   /// Does childType inherit from parentInterface
   /// </summary>
   /// <param name="childType"></param>
   /// <param name="parentInterfaceDef"></param>
   /// <returns></returns>
   public static bool DoesAnySubTypeImplementInterface(this TypeDefinition childType, TypeDefinition parentInterfaceDef)
   {
      Debug.Assert(parentInterfaceDef.IsInterface);
      return childType
     .EnumerateBaseClasses()
     .Any(typeDefinition => typeDefinition.DoesSpecificTypeImplementInterface(parentInterfaceDef));
   }

   /// <summary>
   /// Does the childType directly inherit from parentInterface. Base
   /// classes of childType are not tested
   /// </summary>
   /// <param name="childTypeDef"></param>
   /// <param name="parentInterfaceDef"></param>
   /// <returns></returns>
   public static bool DoesSpecificTypeImplementInterface(this TypeDefinition childTypeDef, TypeDefinition parentInterfaceDef)
   {
      Debug.Assert(parentInterfaceDef.IsInterface);
      return childTypeDef
     .Interfaces
     .Any(ifaceDef => DoesSpecificInterfaceImplementInterface(ifaceDef.InterfaceType.Resolve(), parentInterfaceDef));
   }

    public static bool DoesSpecificTypeImplementInterface(this TypeDefinition childTypeDef, string interfaceName)
    {
        return childTypeDef
       .Interfaces
       .Any(ifaceDef => ifaceDef.InterfaceType.Name == interfaceName);
    }

    /// <summary>
    /// Does interface iface0 equal or implement interface iface1
    /// </summary>
    /// <param name="iface0"></param>
    /// <param name="iface1"></param>
    /// <returns></returns>
    public static bool DoesSpecificInterfaceImplementInterface(TypeDefinition iface0, TypeDefinition iface1)
   {
     Debug.Assert(iface1.IsInterface);
     Debug.Assert(iface0.IsInterface);
     return iface0.MetadataToken == iface1.MetadataToken || iface0.DoesAnySubTypeImplementInterface(iface1);
   }

   /// <summary>
   /// Is source type assignable to target type
   /// </summary>
   /// <param name="target"></param>
   /// <param name="source"></param>
   /// <returns></returns>
   public static bool IsAssignableFrom(this TypeDefinition target, TypeDefinition source) 
  => target == source 
     || target.MetadataToken == source.MetadataToken 
     || source.IsSubclassOf(target)
     || target.IsInterface && source.DoesAnySubTypeImplementInterface(target);

   /// <summary>
   /// Enumerate the current type, it's parent and all the way to the top type
   /// </summary>
   /// <param name="klassType"></param>
   /// <returns></returns>
   public static IEnumerable<TypeDefinition> EnumerateBaseClasses(this TypeDefinition klassType)
   {
      for (var typeDefinition = klassType; typeDefinition != null; typeDefinition = typeDefinition.BaseType?.Resolve())
      {
         yield return typeDefinition;
      }
   }

    public static bool IsDelegate(this TypeDefinition typeDefinition)
    {
        if (typeDefinition.BaseType == null)
        {
            return false;
        }
        return typeDefinition.BaseType.FullName == "System.MulticastDelegate";
    }

    public static bool IsGeneric(this TypeReference type)
    {
        var dt = type.Resolve();
        if (type.HasGenericParameters || type.IsGenericParameter) //|| (dt != null && dt.IsGeneric())
        {
            return true;
        }
        if (type.IsByReference)
        {
            return ((ByReferenceType)type).ElementType.IsGeneric();
        }
        if (type.IsArray)
        {
            return ((ArrayType)type).ElementType.IsGeneric();
        }
        if (type.IsGenericInstance)
        {
            var gt = (GenericInstanceType)type;
            if (gt.HasGenericArguments)
                return true;
            foreach (var typeArg in gt.GenericArguments)
            {
                if (typeArg.IsGeneric())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool HasGenericArgumentFromMethod(this TypeReference type)
    {
        if (type.IsGenericParameter)
        {
            return (type as GenericParameter).Type == GenericParameterType.Method;
        }

        if (type.IsByReference)
        {
            return ((ByReferenceType)type).ElementType.HasGenericArgumentFromMethod();
        }
        if (type.IsArray)
        {
            return ((ArrayType)type).ElementType.HasGenericArgumentFromMethod();
        }
        if (type.IsGenericInstance)
        {
            foreach (var typeArg in ((GenericInstanceType)type).GenericArguments)
            {
                if (typeArg.HasGenericArgumentFromMethod())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsGeneric(this MethodReference method)
    {
        if (method.HasGenericParameters) return true;
        //if (method.ReturnType.IsGeneric()) return true;
        //foreach (var paramInfo in method.Parameters)
        //{
        //    if (paramInfo.ParameterType.IsGeneric()) return true;
        //}
        if (method.IsGenericInstance)
        {
            foreach (var typeArg in ((GenericInstanceMethod)method).GenericArguments)
            {
                if (typeArg.IsGeneric())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsGeneric(this FieldReference field)
    {
        return field.FieldType.IsGeneric();
    }
    
    public static bool IsVoid(this TypeReference type)
    {
        return type.Name.Equals("Void");
    }

    public static bool IsOverride(this MethodReference self)
    {
        if (self == null)
            return false;

        MethodDefinition method = self.Resolve();
        if ((method == null) || !method.IsVirtual)
            return false;

        TypeDefinition declaring = method.DeclaringType;
        TypeDefinition parent = declaring.BaseType != null ? declaring.BaseType.Resolve() : null;
        while (parent != null)
        {
            string name = method.Name;
            foreach (MethodDefinition md in parent.Methods)
            {
                if (name != md.Name)
                    continue;
                if (!method.CompareSignature(md))
                    continue;

                return md.IsVirtual;
            }
            parent = parent.BaseType != null ? parent.BaseType.Resolve() : null;
        }
        return false;
    }
    /// <summary>
    /// Compare the IMethodSignature members with the one being specified.
    /// </summary>
    /// <param name="self">>The IMethodSignature on which the extension method can be called.</param>
    /// <param name="signature">The IMethodSignature which is being compared.</param>
    /// <returns>True if the IMethodSignature members are identical, false otherwise</returns>
    public static bool CompareSignature(this IMethodSignature self, IMethodSignature signature)
    {
        if (self == null)
            return (signature == null);

        if (self.HasThis != signature.HasThis)
            return false;
        if (self.ExplicitThis != signature.ExplicitThis)
            return false;
        if (self.CallingConvention != signature.CallingConvention)
            return false;

        if (!AreSameElementTypes(self.ReturnType, signature.ReturnType))
            return false;

        bool h1 = self.HasParameters;
        bool h2 = signature.HasParameters;
        if (h1 != h2)
            return false;
        if (!h1 && !h2)
            return true;

        IList<ParameterDefinition> pdc1 = self.Parameters;
        IList<ParameterDefinition> pdc2 = signature.Parameters;
        int count = pdc1.Count;
        if (count != pdc2.Count)
            return false;

        for (int i = 0; i < count; ++i)
        {
            if (!AreSameElementTypes(pdc1[i].ParameterType, pdc2[i].ParameterType))
                return false;
        }
        return true;
    }

    private static bool AreSameElementTypes(TypeReference a, TypeReference b)
    {
        return a.IsGenericParameter || b.IsGenericParameter || b.IsNamed(a.Namespace, a.Name);
    }

    /// <summary>
    /// Check if the type and its namespace are named like the provided parameters.
    /// This is preferred to checking the FullName property since the later can allocate (string) memory.
    /// </summary>
    /// <param name="self">The TypeReference on which the extension method can be called.</param>
    /// <param name="nameSpace">The namespace to be matched</param>
    /// <param name="name">The type name to be matched</param>
    /// <returns>True if the type is namespace and name match the arguments, False otherwise</returns>
    public static bool IsNamed(this TypeReference self, string nameSpace, string name)
    {
        if (nameSpace == null)
            throw new ArgumentNullException("nameSpace");
        if (name == null)
            throw new ArgumentNullException("name");
        if (self == null)
            return false;

        if (self.IsNested)
        {
            int spos = name.LastIndexOf('/');
            if (spos == -1)
                return false;
            // GetFullName could be optimized away but it's a fairly uncommon case
            return (nameSpace + "." + name == self.FullName);
        }

        return ((self.Namespace == nameSpace) && (self.Name == name));
    }

    /// <summary>
    /// Check if the type full name match the provided parameter.
    /// Note: prefer the overload where the namespace and type name can be supplied individually
    /// </summary>
    /// <param name="self">The TypeReference on which the extension method can be called.</param>
    /// <param name="fullName">The full name to be matched</param>
    /// <returns>True if the type is namespace and name match the arguments, False otherwise</returns>
    public static bool IsNamed(this TypeReference self, string fullName)
    {
        if (fullName == null)
            throw new ArgumentNullException("fullName");
        if (self == null)
            return false;

        if (self.IsNested)
        {
            int spos = fullName.LastIndexOf('/');
            if (spos == -1)
                return false;
            // FIXME: GetFullName could be optimized away but it's a fairly uncommon case
            return (fullName == self.FullName);
        }

        int dpos = fullName.LastIndexOf('.');
        string nspace = self.Namespace;
        if (dpos != nspace.Length)
            return false;

        if (String.CompareOrdinal(nspace, 0, fullName, 0, dpos) != 0)
            return false;

        string name = self.Name;
        if (fullName.Length - dpos - 1 != name.Length)
            return false;
        return (String.CompareOrdinal(name, 0, fullName, dpos + 1, fullName.Length - dpos - 1) == 0);
    }

    public static TypeReference BaseType(this TypeReference self)
    {
        var td = self.Resolve();
        if (td == null)
            return null;

        return td.BaseType;
    }

    public static bool IsStruct(this TypeDefinition type)
    {
        return type.IsValueType && !type.IsEnum && !type.IsPrimitive;

    }
}