using Generater;
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// enum struct struct{class}(Scene) delegate
/// </summary>
public static class CopyGenerater
{
    static string outFilePath;
    static CodeWriter writer;
    static AssemblyDefinition tarAsm ;
    static ModuleDefinition tarModule => tarAsm.MainModule;
    static HashSet<TypeReference> types = new HashSet<TypeReference>();

    static CopyGenerater()
    {
        outFilePath = Path.Combine(Binder.OutDir, $"Binder.copy.dll");
    }

    public static void AddTpe(TypeReference type)
    {
        if (tarAsm == null)
            tarAsm = type.Resolve().Module.Assembly;

        types.Add(type);
    }

   
    public static void GenAsm()
    {
        List<TypeDefinition> delList = new List<TypeDefinition>();

        foreach (var t in tarModule.Types)
        {
            if (!types.Contains(t))
                delList.Add(t);
        }

        foreach (var t in delList)
            tarModule.Types.Remove(t);


        tarAsm.Write(outFilePath);
    }


}