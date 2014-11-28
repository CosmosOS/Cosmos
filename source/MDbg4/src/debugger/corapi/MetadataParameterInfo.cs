//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug; 
using Microsoft.Samples.Debugging.CorMetadata.NativeApi; 

namespace Microsoft.Samples.Debugging.CorMetadata
{
    public sealed class MetadataParameterInfo : ParameterInfo
    {
        internal MetadataParameterInfo(IMetadataImport importer,int paramToken,
                                       MemberInfo memberImpl,Type typeImpl)
        {
            int parentToken;
            uint pulSequence,pdwAttr,pdwCPlusTypeFlag,pcchValue,size;
            
            IntPtr ppValue;
            importer.GetParamProps(paramToken,
                                   out parentToken,
                                   out pulSequence,
                                   null,
                                   0,
                                   out size,
                                   out pdwAttr,
                                   out pdwCPlusTypeFlag,
                                   out ppValue,
                                   out pcchValue
                                   );
            StringBuilder szName = new StringBuilder((int)size);        
            importer.GetParamProps(paramToken,
                                   out parentToken,
                                   out pulSequence,
                                   szName,
                                   (uint)szName.Capacity,
                                   out size,
                                   out pdwAttr,
                                   out pdwCPlusTypeFlag,
                                   out ppValue,
                                   out pcchValue
                                   );
            NameImpl = szName.ToString();
            ClassImpl = typeImpl;
            PositionImpl = (int)pulSequence;
            AttrsImpl = (ParameterAttributes)pdwAttr;
            
            MemberImpl=memberImpl;
        }

        private MetadataParameterInfo(SerializationInfo info, StreamingContext context)
        {
            
        }

        public override String Name
        {
            get 
            {
                return NameImpl;
            }
        }

        public override int Position
        { 
            get 
            { 
                return PositionImpl; 
            }
        }
    }
}
