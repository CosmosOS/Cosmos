using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Debug.Symbols
{
    [DataContract(IsReference = true)]
    public partial class AssemblyFile
    {

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long ID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string Pathname
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class Document
    {
        [Key]
        [DataMember()]
        public long ID
        {
            get;
            set;
        }


        [DataMember()]
        public string Pathname
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class FIELD_INFO
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string TYPE
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int OFFSET
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string NAME
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        [Key]
        public long ID
        {
            get;
            set;
        }

    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class FIELD_MAPPING
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string TYPE_NAME
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string FIELD_NAME
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMember()]
        public long ID
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class Label
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long ID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long Address
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class LOCAL_ARGUMENT_INFO
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string METHODLABELNAME
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public bool IsArgument
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int INDEXINMETHOD
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int OFFSET
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string NAME
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string TYPENAME
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMember()]
        public long ID
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class Method
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        [Key]
        public long ID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int TypeToken
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int MethodToken
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long AssemblyFileID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long LineColStart
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long LineColEnd
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long DocumentID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long? LabelStartID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long? LabelEndID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string LabelCall
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class MethodIlOp
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMember()]
        public long ID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string LabelName
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int StackDiff
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public int IlOffset
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long? MethodID
        {
            get;
            set;
        }
    }

    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DataContract(IsReference = true)]
    public partial class INT3Label
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public string LabelName
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public long MethodID
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMember()]
        public bool LeaveAsINT3
        {
            get;
            set;
        }
    }
}
