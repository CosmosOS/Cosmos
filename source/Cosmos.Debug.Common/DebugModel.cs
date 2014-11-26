using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SQLinq;

namespace Cosmos.Debug.Common
{
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("AssemblyFiles")]
    public partial class AssemblyFile
    {
        
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;set;
        }
        
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("PATHNAME")]
        public global::System.String Pathname
        {
          get;set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("DOCUMENTS")]
    public partial class Document 
    {
        [Key]
        [DataMemberAttribute()]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        
        [DataMemberAttribute()]
        [SQLinqColumn("PATHNAME")]
        public global::System.String Pathname
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("FIELD_INFOS")]
    public partial class FIELD_INFO
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("TYPE")]
        public global::System.String TYPE
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("OFFSET")]
        public global::System.Int32 OFFSET
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [SQLinqColumn("NAME")]
        [DataMemberAttribute()]
        public global::System.String NAME
        {
           get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [Key]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
            
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("FIELD_MAPPINGS")]
    public partial class FIELD_MAPPING
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("TYPE_NAME")]
        public global::System.String TYPE_NAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("FIELD_NAME")]
        public global::System.String FIELD_NAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMemberAttribute()]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("LABELS")]
    public partial class Label 
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("NAME")]
        public global::System.String Name
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("ADDRESS")]
        public global::System.Int64 Address
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("LOCAL_ARGUMENT_INFOS")]
    public partial class LOCAL_ARGUMENT_INFO
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("METHODLABELNAME")]
        public global::System.String METHODLABELNAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("ISARGUMENT")]
        public global::System.Boolean IsArgument
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("INDEXINMETHOD")]
        public global::System.Int32 INDEXINMETHOD
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("OFFSET")]
        public global::System.Int32 OFFSET
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("NAME")]
        public global::System.String NAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("TYPENAME")]
        public global::System.String TYPENAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMemberAttribute()]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("METHODS")]
    public partial class Method
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [Key]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("TYPETOKEN")]
        public global::System.Int32 TypeToken
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("METHODTOKEN")]
        public global::System.Int32 MethodToken
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("ASSEMBLYFILEID")]
        public global::System.Guid AssemblyFileID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LINECOLSTART")]
        public global::System.Int64 LineColStart
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LINECOLEND")]
        public global::System.Int64 LineColEnd
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("DOCUMENTID")]
        public global::System.Guid DocumentID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LABELSTARTID")]
        public Nullable<global::System.Guid> LabelStartID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LABELENDID")]
        public Nullable<global::System.Guid> LabelEndID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LABELCALL")]
        public global::System.String LabelCall
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [SQLinqTable("METHODILOPs")]
    public partial class MethodIlOp
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMemberAttribute()]
        [SQLinqColumn("ID")]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LABELNAME")]
        public global::System.String LabelName
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("STACKDIFF")]
        public global::System.Int32 StackDiff
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("ILOFFSET")]
        public global::System.Int32 IlOffset
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("METHODID")]
        public Nullable<global::System.Guid> MethodID
        {
            get;
            set;
        }
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    [SQLinqTable("INT3LABELS")]
    public partial class INT3Label
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LABELNAME")]
        public global::System.String LabelName
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("METHODID")]
        public global::System.Guid MethodID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [SQLinqColumn("LEAVEASINT3")]
        public global::System.Boolean LeaveAsINT3
        {
            get;
            set;
        }
    }
}