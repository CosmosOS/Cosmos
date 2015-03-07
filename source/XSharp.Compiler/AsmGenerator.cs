using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using XSharp.Nasm;

namespace XSharp.Compiler {
  // This class performs the translation from X# source code into a target
  // assembly language. At current time the only supported assembler syntax is NASM.

  public class AsmGenerator {
    protected TokenPatterns mPatterns = new TokenPatterns();

    /// <summary>Should we keep the user comments in the generated target assembly program ?</summary>
    public bool EmitUserComments = false;
    protected int mLineNo = 0;
    protected string mPathname = "";

    /// <summary>Invoke this method when end of source code file is reached to make sure the last
    /// function or interrupt handler has well balanced opening/closing curly braces.</summary>
    private void AssertLastFunctionComplete() {
      if (!mPatterns.InFunctionBody) {
        return;
      }
      throw new Exception("The last function or interrupt handler from source code file is missing a curly brace.");
    }

    /// <summary>Parse the input X# source code file and generate the matching target assembly
    /// language.</summary>
    /// <param name="aSrcPathname">X# source code file.</param>
    /// <returns>The resulting target assembler content. The returned object contains
    /// a code and a data block.</returns>
    public Assembler Generate(string aSrcPathname) {
      try
      {
        mPatterns.EmitUserComments = EmitUserComments;
        mLineNo = 0;
        var xResult = new Assembler();
        using (var xInput = new StreamReader(aSrcPathname))
        {
          // Read one X# source code line at a time and process it.
          while (true)
          {
            mLineNo++;
            string xLine = xInput.ReadLine();
            if (xLine == null)
            {
              break;
            }

            var xAsm = ProcessLine(xLine, mLineNo);
            xResult.Data.AddRange(xAsm.Data);
            xResult.Code.AddRange(xAsm.Code);
          }
        }
        AssertLastFunctionComplete();
        return xResult;
      }
      catch (Exception E)
      {
        throw new Exception("Error while generating output for file " + Path.GetFileName(aSrcPathname), E);
      }
    }

    /// <summary>Parse the input X# source code file and generate two new files with target
    /// assembly language. The two generated files contain target assembler source and target
    /// assembler data respectively.</summary>
    /// <param name="aSrcPathname">X# source code file.</param>
    public void GenerateToFiles(string aSrcPathname) {
      mPathname = Path.GetFileName(aSrcPathname);
      using (var xInput = new StreamReader(aSrcPathname)) {
        using (var xOutputCode = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".asm"))) {
          using (var xOutputData = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".asmdata"))) {
            Generate(xInput, xOutputData, xOutputCode);
          }
        }
      }
    }

    /// <summary>Parse the input X# source code from the given reader and write both target
    /// assembler code and target assembler data in their respective writers.</summary>
    /// <param name="aInput">A reader to acquire X# source code from.</param>
    /// <param name="aOutputData">A writer that will receive target assembler data.</param>
    /// <param name="aOutputCode">A writer that will receive target assembler code.</param>
    public void Generate(TextReader aInput, TextWriter aOutputData, TextWriter aOutputCode) {
      mPatterns.EmitUserComments = EmitUserComments;
      mLineNo = 0;
      // Read one X# source code line at a time and process it.
      while (true) {
        mLineNo++;
        string xLine = aInput.ReadLine();
        if (xLine == null) {
          break;
        }

        var xAsm = ProcessLine(xLine, mLineNo);
        foreach (var x in xAsm.Data) {
          aOutputData.WriteLine(x);
        }
        foreach (var x in xAsm.Code) {
          aOutputCode.WriteLine(x);
        }
      }
      AssertLastFunctionComplete();
    }

    /// <summary>Process a single X# source code line and translate it into the target
    /// assembler syntax.</summary>
    /// <param name="aLine">The processed X# source code line.</param>
    /// <param name="lineNumber">Line number for debugging and diagnostic messages.</param>
    /// <returns>The resulting target assembler content. The returned object contains
    /// a code and a data block.</returns>
    protected Assembler ProcessLine(string aLine, int lineNumber) {
      Assembler xAsm;

      aLine = aLine.Trim();
      if (String.IsNullOrEmpty(aLine) || aLine == "//") {
        xAsm = new Assembler();
        xAsm += "";
        return xAsm;
      }

      // Currently we use a new assembler for every line.
      // If we dont it could create a really large in memory object.
      xAsm = mPatterns.GetCode(aLine, lineNumber);
      if (xAsm == null) {
        var xMsg = new StringBuilder();
        if (mPathname != "") {
          xMsg.Append("File " + mPathname + ", ");
        }
        xMsg.Append("Line " + mLineNo + ", ");
        xMsg.Append("Parsing error: " + aLine);
        throw new Exception(xMsg.ToString());
      }
      return xAsm;
    }
  }
}
