using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.Compiler.XSharp;
using EnvDTE;

namespace XSharp.XSC {
  class Program {
    public static bool Project;
    static void Main(string[] aArgs) {
      var xArgs = new string[aArgs.Length];
      for (int i = 0; i < xArgs.Length; i++) {
          xArgs[i] = aArgs[i].ToUpper();
      }
      Project=xArgs.Contains("-PROJECT");        
      try {
        string xSrc = aArgs[0];
        string xNamespace = aArgs[1];
        var xGenerator = new CSharpGenerator();
        try
        {
            if (Directory.Exists(xSrc))//If a driectory exists, its a directory, else is either a file or non existant.
            {
                foreach (string file in Directory.GetFiles(xSrc,"*.xs")) //Loop through all files
                    xGenerator.Execute(xNamespace, file);
                if (Project)
                    GenerateCSProj(xNamespace,xSrc);
            }
            else
                xGenerator.Execute(xNamespace, xSrc);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(1);
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Environment.Exit(1);
      }
    }

    private static void GenerateCSProj(string xNamespace, string xSrc)
    {
        StreamWriter proj = new StreamWriter(Path.Combine(xSrc, xNamespace + ".csproj"));
        /*
        <Reference Include="Tools">
            <HintPath>$(ToolsDllPath)</HintPath>
        </Reference>
        */
        //C:\Users\ADAM\AppData\Roaming\Cosmos User Kit\Build\VSIP\Cosmos.Assembler.dll
        //C:\Users\ADAM\AppData\Roaming\Cosmos User Kit\Build\VSIP\Cosmos.Assembler.x86.dll
        string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.None);
        proj.WriteLine("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
        proj.WriteLine("    <PropertyGroup>");
        proj.WriteLine("        <AssemblyName>"+xNamespace+"</AssemblyName>");
        proj.WriteLine("        <OutputPath>"+xSrc+"\\bin\\</OutputPath>");
        proj.WriteLine("    </PropertyGroup>");
        proj.WriteLine("    <ItemGroup>");
        proj.WriteLine("        <Reference Include=\"System\" />");
        proj.WriteLine("        <Reference Include=\"Cosmos.Assembler\">");
        proj.WriteLine("            <HintPath>"+appdata+@"\Cosmos User Kit\Build\VSIP</HintPath>");
        proj.WriteLine("        </Reference>");
        proj.WriteLine("        <Reference Include=\"Cosmos.Assembler.x86\">");
        proj.WriteLine("            <HintPath>"+appdata+@"\Cosmos User Kit\Build\VSIP</HintPath>");
        proj.WriteLine("        </Reference>");
        proj.WriteLine("        <Reference Include=\"System.Data\" />");
        proj.WriteLine("    </ItemGroup>");
        proj.WriteLine("    <ItemGroup>");
        proj.WriteLine("        <Compile Include=\"*.cs\" />");
        proj.WriteLine("    </ItemGroup>");
        proj.WriteLine("    <Target Name=\"Build\">");
        proj.WriteLine("        <MakeDir Directories=\"$(OutputPath)\" Condition=\"!Exists('$(OutputPath)')\" />");
        proj.WriteLine("        <Csc Sources=\"@(Compile)\" OutputAssembly=\"$(OutputPath)$(AssemblyName).exe\" />");
        proj.WriteLine("    </Target>");
        proj.WriteLine("</Project>");
        proj.Flush();
        proj.Close();
    }
  }
}