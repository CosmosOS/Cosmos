﻿using System;
using System.Globalization;
using System.Text;
using System.IO;

using Cosmos.Assembler;

namespace ASharp.Compiler
{
    // This class performs the translation from A# source code into a target
    // assembly language. At current time the only supported assembler syntax is arm-none-eabi.

#warning Completely not tested!!!

    public class AsmGenerator
    {
        protected TokenPatterns mPatterns = new TokenPatterns();

        /// <summary>Should we keep the user comments in the generated target assembly program ?</summary>
        public bool EmitUserComments = false;
        protected int mLineNo = 0;
        protected string mPathname = "";

        /// <summary>Invoke this method when end of source code file is reached to make sure the last
        /// function or interrupt handler has well balanced opening/closing curly braces.</summary>
        private void AssertLastFunctionComplete()
        {
            if (!mPatterns.InFunctionBody)
            {
                return;
            }
            throw new Exception("The last function or interrupt handler from source code file is missing a curly brace.");
        }

        /// <summary>Parse the input A# source code file and generate the matching target assembly
        /// language.</summary>
        /// <param name="aReader">A# source code reader.</param>
        /// <returns>The resulting target assembler content. The returned object contains
        /// a code and a data block.</returns>
        public Assembler Generate(TextReader aReader)
        {
            if (aReader == null)
            {
                throw new ArgumentNullException(nameof(aReader));
            }
            mPatterns.EmitUserComments = EmitUserComments;
            mLineNo = 0;

            var xResult = new Assembler(CompilerStyles.GNU);

            try
            {
                // Read one A# source code line at a time and process it.
                while (true)
                {
                    mLineNo++;
                    string xLine = aReader.ReadLine();
                    if (xLine == null)
                    {
                        break;
                    }

                    ProcessLine(xLine, mLineNo);
                }
                AssertLastFunctionComplete();
                return xResult;
            }
            finally
            {
                Assembler.ClearCurrentInstance();
            }
        }

        /// <summary>Parse the input A# source code file and generate the matching target assembly
        /// language.</summary>
        /// <param name="aSrcPathname">A# source code file.</param>
        /// <returns>The resulting target assembler content. The returned object contains
        /// a code and a data block.</returns>
        public Assembler Generate(string aSrcPathname)
        {
            try
            {
                using (var xInput = new StreamReader(aSrcPathname))
                {
                    return Generate(xInput);
                }
            }
            catch (Exception E)
            {
                throw new Exception("Error while generating output for file " + Path.GetFileName(aSrcPathname), E);
            }
        }

        /// <summary>Parse the input A# source code file and generate two new files with target
        /// assembly language. The two generated files contain target assembler source and target
        /// assembler data respectively.</summary>
        /// <param name="aSrcPathname">A# source code file.</param>
        public void GenerateToFiles(string aSrcPathname)
        {
            mPathname = Path.GetFileName(aSrcPathname);

            new Assembler(CompilerStyles.GNU, false);

            try
            {
                using (var xInput = new StreamReader(aSrcPathname))
                {
                    using (var xOutput = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".S")))
                    {
                        xOutput.WriteLine("@ Generated at {0}", DateTime.Now.ToString(new CultureInfo("en-US")));

                        Generate(xInput, xOutput);
                    }
                }
            }
            finally
            {
                Assembler.ClearCurrentInstance();
            }
        }

        /// <summary>Parse the input A# source code from the given reader and write both target
        /// assembler code and target assembler data in their respective writers.</summary>
        /// <param name="aInput">A reader to acquire A# source code from.</param>
        /// <param name="aOutputData">A writer that will receive target assembler data.</param>
        /// <param name="aOutputCode">A writer that will receive target assembler code.</param>
        public void Generate(TextReader aInput, TextWriter aOutput)
        {
            mPatterns.EmitUserComments = EmitUserComments;
            mLineNo = 0;
            // Read one A# source code line at a time and process it.
            while (true)
            {
                mLineNo++;
                string xLine = aInput.ReadLine();
                if (xLine == null)
                {
                    break;
                }

                ProcessLine(xLine, mLineNo);

            }
            Assembler.CurrentInstance.FlushText(aOutput);
            AssertLastFunctionComplete();
        }

        /// <summary>Process a single A# source code line and translate it into the target
        /// assembler syntax.</summary>
        /// <param name="aLine">The processed A# source code line.</param>
        /// <param name="lineNumber">Line number for debugging and diagnostic messages.</param>
        /// <returns>The resulting target assembler content. The returned object contains
        /// a code and a data block.</returns>
        protected void ProcessLine(string aLine, int lineNumber)
        {
            aLine = aLine.Trim();

            if (String.IsNullOrEmpty(aLine) || aLine == "//")
            {
                return;
            }

            // Currently we use a new assembler for every line.
            // If we dont it could create a really large in memory object.
            if (!mPatterns.GetCode(aLine, lineNumber))
            {
                var xMsg = new StringBuilder();
                if (mPathname != "")
                {
                    xMsg.Append("File " + mPathname + ", ");
                }
                xMsg.Append("Line " + mLineNo + ", ");
                xMsg.Append("Parsing error: " + aLine);
                throw new Exception(xMsg.ToString());
            }
        }
    }
}
