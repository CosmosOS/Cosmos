//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Globalization;

namespace Microsoft.Samples.Tools.Mdbg
{
    //////////////////////////////////////////////////////////////////////////////////
    //
    // Argument Parser
    //
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This allows for easy and consistent parsing of command arguments
    /// </summary>
    public class ArgParser
    {

        /// <summary>
        /// Creates a new ArgParser using the given arguments.
        /// </summary>
        /// <param name="arguments">The arguments to parse.</param>
        public ArgParser(string arguments)
        {
            Init(arguments);
        }

        /// <summary>
        /// Creates a new ArgParser using the given arguments and Option Specification.
        /// Options need to be in the format: "optionName(:1)?;optionName(:1)"
        /// </summary>
        /// <param name="arguments">The arguments to parse.</param>
        /// <param name="optionSpec">The Option Specification to use.</param>
        public ArgParser(string arguments,string optionSpec)
        {
            Init(arguments);
            ParseOptions(optionSpec);
        }
        
        /// <summary>
        /// Gets how many arguments there are.
        /// </summary>
        /// <value>The number of arguments.</value>
        public int Count 
        {
            get
            {
                return m_args.Count;
            }
        }

        /// <summary>
        /// Determines if a given argument index exists without throwing an IndexOutOfRangeException.
        /// </summary>
        /// <param name="index">What index to check.</param>
        /// <returns>If there is an argument at that index.</returns>
        public bool Exists(int index)
        {
            if(index<0)
            {
                throw new ArgumentException();
            }
            
            return (index<m_args.Count);
        }

        /// <summary>
        /// Gets the argument from a given index.
        /// </summary>
        /// <param name="index">Which index to get the argument from.</param>
        /// <returns>The argument at he given index.</returns>
        public ArgToken GetArgument(int index)
        {
            if(!Exists(index))
            {
               throw new MDbgShellException("Missing argument "+index);
            }
            return new ArgToken((string)m_args[index]);
        }

        /// <summary>
        /// Determines if an argument was passed.
        /// </summary>
        /// <param name="name">Which argument you are looking for.</param>
        /// <returns>Was that argument passed.</returns>
        public bool OptionPassed(string name)
        {
            if(name==null)
            {
                throw new ArgumentException();
            }
            
            if(m_options==null)
            {
                throw new InvalidOperationException
                    ("Option specifier were not specified during construction of ArgParser");
            }

            return m_options.Contains(String.Intern(name));
        }
        
        /// <summary>
        /// Gets an Option's value if it was supplied.
        /// </summary>
        /// <param name="name">Which Option to get.</param>
        /// <returns>An ArgToken for that option.</returns>
        public ArgToken GetOption(string name)
        {
            if(name==null)
            {
                throw new ArgumentException();
            }
            
            if(m_options==null)
            {
                throw new InvalidOperationException("Option specifier was not specified during construction of ArgParser");
            }

            string optName = String.Intern(name);
            if(!m_options.Contains(optName))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Option {0} not specified",new Object[]{name}));
            }
            
            string optVal = (string)m_options[optName];
            if(optVal==null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Option {0} is flag only",new Object[]{name}));
            }

            return new ArgToken(optVal);
        }

        /// <summary>
        /// Returns argument at given index as a Boolean. (For backward compatibility.)
        /// </summary>
        /// <param name="index">Which index to use.</param>
        /// <returns>The argument at that index as a Boolean.</returns>
        public bool AsBool(int index)
        {
            return GetArgument(index).AsBool;
        }

        /// <summary>
        /// Returns argument at given index as an integer. (For backward compatibility.)
        /// </summary>
        /// <param name="index">Which index to use.</param>
        /// <returns>The argument at that index as an integer.</returns>
        public int AsInt(int index)
        {
            return GetArgument(index).AsInt;
        }

        /// <summary>
        /// Returns argument at given index as a string. (For backward compatibility.)
        /// </summary>
        /// <param name="index">Which index to use.</param>
        /// <returns>The argument at that index as a string.</returns>
        public string AsString(int index)
        {
            return GetArgument(index).AsString;
        }

        /// <summary>
        /// Returns argument at given index as a double. (For backward compatibility.)
        /// </summary>
        /// <param name="index">Which index to use.</param>
        /// <returns>The argument at that index as a double.</returns>
        public double AsDouble(int index)
        {
            return GetArgument(index).AsDouble;
        }

        /// <summary>
        /// Returns argument at given index with a given Command Argument as a command. (For backward compatibility.)
        /// </summary>
        /// <param name="index">Which index to use.</param>
        /// <param name="ca">A Command Argument to use.</param>
        /// <returns>A command where the argument at the given index is the command and the given ca make the Arguments.</returns>
        public string AsCommand(int index, CommandArgument ca)
        {
            return GetArgument(index).AsCommand(ca);
        }

        private void Init(string arguments)
        {
            m_args = new ArrayList();
            StringBuilder sb = new StringBuilder();
            bool isInString=false;
            char stringStart=' ';
            foreach(char c in arguments)
            {
                if(isInString) 
                {
                    if(c==stringStart)
                    {
                        m_args.Add(sb.ToString());
                        sb.Length=0;
                        isInString=false;
                        continue;
                    }
                } 
                else
                {
                    switch(c)
                    {
                    case ' ':
                    case '\t':
                        if(sb.Length>0)
                        {
                            m_args.Add(sb.ToString());
                            sb.Length=0;
                        }
                        continue;
                    case '"':
                    case '\'':
                        isInString = true;
                        stringStart=c;
                        continue;
                    }
                }
                sb.Append(c);
            }
            if(isInString)
            {
                throw new MDbgShellException("Arguments contain an unterminated string");
            }

            if(sb.Length>0)
            {
                m_args.Add(sb.ToString());
            }
        }
        
        //private class OptionArg
        private void ParseOptions(string optionSpec)
        {
            //optionName:?;optionName:?...
            Hashtable availOptions = new Hashtable();
            foreach(string opt in optionSpec.Split(';'))
            {
                if(opt.EndsWith(":1"))
                {
                    availOptions.Add(opt.Substring(0,opt.Length-2),true);
                }
                else
                {
                    availOptions.Add(opt,false);
                }
            }

            Debug.Assert(m_args!=null);
            m_options = new Hashtable();

            // do the parsing
            int i;
            for(i=0; i<m_args.Count;i++)
            {
                if((m_args[i] as String)=="--")
                {
                    break;
                }

                if((m_args[i] as String).StartsWith("-"))
                {
                    string optName = String.Intern((m_args[i] as String).Substring(1));
                    if(availOptions.Contains(optName))
                    {
                        if((bool)availOptions[optName])
                        {
                            // we have an argument
                            if(i==m_args.Count-1)
                            {
                                throw new Exception(String.Format(CultureInfo.InvariantCulture, "option {0} needs argument",new Object[]{optName}));
                            }
                            m_options.Add(optName,m_args[++i]);
                        }
                        else
                        {
                            m_options.Add(optName,null);
                        }
                    }
                    else
                        throw new MDbgShellException(string.Format("Invalid option '{0}' specified.",m_args[i]));
                 }
                else
                {
                    break;
                }
            }
            // now when we are done with parsing i will contain # of args that were discarded; let's delete them
            m_args.RemoveRange(0,i);
        }

        private ArrayList m_args;
        private Hashtable m_options;
    }

    /// <summary>
    /// Command Argument class
    /// </summary>
    public class CommandArgument
    {
        /// <summary>
        /// Creates a new instance of the CommandArgument class with the given array of command names.
        /// </summary>
        /// <param name="commandNames">Which command names to use for the new CommandArgument</param>
        public CommandArgument( params string[] commandNames)
        {
            AddCommand(commandNames);
        }
        
        /// <summary>
        /// Adds the given command name.
        /// </summary>
        /// <param name="commandName">Which command name to add.</param>
        public void AddCommand(string commandName)
        {
            m_commands.Add(commandName);
        }

        /// <summary>
        /// Adds each command name from the given array.
        /// </summary>
        /// <param name="commandNames">Which command names to add.</param>
        public void AddCommand(string[] commandNames)
        {
            foreach(string c in commandNames)
            {
                AddCommand(c);
            }
        }

        /// <summary>
        /// Looks up a command.
        /// </summary>
        /// <param name="input">Which command to look up.</param>
        /// <returns></returns>
        public string Lookup(string input)
        {
            ArrayList al = new ArrayList();
            foreach(string cmdName in m_commands)
            {
                if(String.Compare(input,0,cmdName,0,input.Length,false,System.Globalization.CultureInfo.InvariantCulture)==0)
                {
                    if(input.Length==cmdName.Length)
                    {
                        return input;                   // fully qualified command
                    }
                    al.Add(cmdName);
                }
            }

            if(al.Count==0)
            {
                throw new MDbgShellException("Invalid command '"+input+"'.");
            }
            else if(al.Count==1)
            {
                return (string)al[0];
            }
            else
            {
                StringBuilder s=new StringBuilder("Command prefix too short. \nPossible completitions:");
                foreach(string c in al)
                {
                    s.Append("\n").Append(c);
                }
                throw new MDbgShellException(s.ToString());
            }
        }

        private ArrayList m_commands=new ArrayList();
    }

    /// <summary>
    /// This struct provides implementation for turning argument tokens into usable types.
    /// </summary>
    public struct ArgToken
    {
        internal ArgToken(string valueToken)
        {
            Debug.Assert(valueToken!=null);
            m_token = valueToken;
        }

        /// <summary>
        /// Parses the token as a Boolean.
        /// </summary>
        /// <value>The Boolean that the token represents.</value>
        public bool AsBool
        {
            get
            {
                return Boolean.Parse(m_token);
            }
        }

        /// <summary>
        /// Parses the token as an int.
        /// </summary>
        /// <value>The int that the token represents.</value>
        public int AsInt
        {
            get
            {
                return Int32.Parse(m_token,System.Globalization.CultureInfo.CurrentUICulture);
            }
        }

        /// <summary>
        /// Parses the token as an int, treating it as hex if prefixed with 0x
        /// </summary>
        /// <value>The int that the token represents.</value>
        public int AsHexOrDecInt
        {
            get
            {
                if( m_token.StartsWith( "0x", true, System.Globalization.CultureInfo.CurrentUICulture ) )
                {
                    // parse as hex - strip off leading 0x
                    UInt32 valHex = UInt32.Parse( 
                        m_token.Substring( 2 ), 
                        NumberStyles.HexNumber, 
                        System.Globalization.CultureInfo.CurrentUICulture );

                    // we use signed integer types everywhere (for CLS compliance), 
                    // but we sometimes expect >31 bit hex values (eg. Win9x PIDs) as 
                    // negative numbers, so cast from UInt32 to Int32
                    return (Int32)valHex;
                }
                else
                {
                    // treat as decimal
                    return this.AsInt;
                }
            }
        }
       

        /// <summary>
        /// Parses the token as an address entered as hex or decimal number.
        /// </summary>
        /// <value>The int that the token represents.</value>
        public long AsAddress
        {
            get
            {
                if( m_token.StartsWith("0x") )
                    return Int64.Parse(m_token.Substring(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentUICulture);
                else
                    return Int64.Parse(m_token, System.Globalization.CultureInfo.CurrentUICulture);
            }
        }


        /// <summary>
        /// Returns the string value of the token.
        /// </summary>
        /// <value>The string that the token represents.</value>
        public string AsString
        {
            get
            {
                return m_token;
            }
        }

        /// <summary>
        /// Parses the token as a double.
        /// </summary>
        /// <value>The double that the token represents.</value>
        public double AsDouble
        {
            get
            {
                return Int32.Parse(m_token,System.Globalization.CultureInfo.CurrentUICulture);
            }
        }

        /// <summary>
        /// Looks up the token and creates a command for it using the given command argument.
        /// </summary>
        /// <param name="ca">What command argument to use.</param>
        /// <returns>The command.</returns>
        public string AsCommand(CommandArgument ca)
        {
            return ca.Lookup(m_token);
        }

        /// <summary>
        /// Inequality testing.  Allows for things like "if(thing1 != thing2)" to work properly.
        /// </summary>
        /// <param name="operand">First Operand.</param>
        /// <param name="operand2">Second Operand.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=( ArgToken operand, ArgToken operand2)
        {
            return operand.m_token != operand2.m_token;
        }
        
        /// <summary>
        /// Equality testing.  Allows for things like "if(thing1 == thing2)" to work properly.
        /// </summary>
        /// <param name="operand">First Operand.</param>
        /// <param name="operand2">Second Operand.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==( ArgToken operand, ArgToken operand2)
        {
            return operand.m_token == operand2.m_token;
        }

        /// <summary>
        /// Required to override Equals.
        /// </summary>
        /// <returns>Hash Code.</returns>
        public override int GetHashCode() 
        {
            return m_token.GetHashCode();
        }
        
        /// <summary>
        /// Determines if the value is equal to another.
        /// </summary>
        /// <param name="value">The object to compare to.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(Object value)
        {
            return (ArgToken)value == this;
        }

        private string m_token;
    }
}
