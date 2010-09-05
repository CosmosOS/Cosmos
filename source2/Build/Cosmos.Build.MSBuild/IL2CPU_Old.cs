//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Build.Utilities;
//using Microsoft.Build.Framework;
//using System.Reflection;
//using Cosmos.Compiler.Assembler;
//using Cosmos.Compiler.Assembler.X86;
//using System.IO;
//using Cosmos.Build.Common;
//using Microsoft.Win32;
//using Cosmos.IL2CPU.X86;
//using Cosmos.IL2CPU;
//using Microsoft.CSharp;

//namespace Cosmos.Build.MSBuild
//{
//    public class IL2CPU_Old : BaseToolTask
//    {
//        #region properties
//        [Required]
//        public string DebugMode{
//            get;
//            set;
//        }

//        public string TraceAssemblies
//        {
//            get;
//            set;
//        }

//        public byte DebugCom
//        {
//            get;
//            set;
//        }

//        [Required]
//        public bool UseNAsm
//        {
//            get;
//            set;
//        }

//        [Required]
//        public ITaskItem[] References
//        {
//            get;
//            set;
//        }

//        [Required]
//        public string OutputFilename
//        {
//            get;
//            set;
//        }

//        public bool EnableLogging
//        {
//            get;
//            set;
//        }

//        public bool EmitDebugSymbols
//        {
//            get;
//            set;
//        }

//        #endregion

//        public static string base64Encode(string data)
//{
//    try
//    {
//        byte[] encData_byte = new byte[data.Length];
//        encData_byte = System.Text.Encoding.UTF8.GetBytes(data);    
//        string encodedData = Convert.ToBase64String(encData_byte);
//        return encodedData;
//    }
//    catch(Exception e)
//    {
//        throw new Exception("Error in base64Encode" + e.Message, e);
//    }
//}

//        public static string base64Decode(string data)
//{
//    try
//    {
//        System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();  
//        System.Text.Decoder utf8Decode = encoder.GetDecoder();
    
//        byte[] todecode_byte = Convert.FromBase64String(data);
//        int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);    
//        char[] decoded_char = new char[charCount];
//        utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);                   
//        string result = new String(decoded_char);
//        return result;
//    }
//    catch(Exception e)
//    {
//        throw new Exception("Error in base64Decode" + e.Message, e);
//    }
//}

//        private string CreateIL2CPUParameter()
//        {
//            IL2CPUData xData = new IL2CPUData();
//            xData.DebugMode = DebugMode;
//            xData.TraceAssemblies = TraceAssemblies;
//            xData.DebugCom = DebugCom;
//            xData.UseNAsm = UseNAsm;
//            xData.OutputFilename = OutputFilename;
//            xData.EnableLogging = EnableLogging;
//            xData.EmitDebugSymbols = EmitDebugSymbols;

//            if (References != null)
//            {
//                var xReferences = new List<string>(References.Length);
//                foreach (var xRef in References)
//                {
//                    if (xRef.MetadataNames.OfType<string>().Contains("FullPath"))
//                        xReferences.Insert(0, xRef.GetMetadata("FullPath"));
//                }
//                xData.References = xReferences.ToArray();
//            }
//            string ret = string.Empty;
//            using (var xReader = new StreamReader(new System.IO.MemoryStream()))
//            {
//                var xSer = new System.Xml.Serialization.XmlSerializer(typeof(IL2CPUData));
//                xSer.Serialize(xReader.BaseStream, xData);

//                xReader.BaseStream.Position = 0;
//                ret = base64Encode(xReader.ReadToEnd());
//            }
//            return ret;
//        }

//        public override bool Execute()
//        {
//            var xParameter = CreateIL2CPUParameter();
//            return base.ExecuteTool(
//                string.Empty,
//                Cosmos.Build.Common.CosmosPaths.IL2CPUTask,
//                xParameter,
//                "IL2CPU");
//        }
//    }
//}