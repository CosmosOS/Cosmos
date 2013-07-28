using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.EntityClient;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Config;

namespace Cosmos.Debug.Common
{
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [DbConfigurationType(typeof(DebugModelConfiguration))]
    public partial class Entities : DbContext
    {
        /// <summary>
        /// Initializes a new Entities object using the connection string found in the 'Entities' section of the application configuration file.
        /// </summary>
        public Entities() : base("Entities")
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Configuration.LazyLoadingEnabled = true;
            
        }

        private ObjectContext ObjectContext
        {
            get
            {
                return ((IObjectContextAdapter)this).ObjectContext;
            }
        }
    
        /// <summary>
        /// Initialize a new Entities object.
        /// </summary>
        public Entities(string connectionString) : base(connectionString)
        {
            Initialize();
        }
    
        /// <summary>
        /// Initialize a new Entities object.
        /// </summary>
        public Entities(DbConnection connection, bool ownsConnection) : base(connection, ownsConnection)
        {
            Initialize();
        }
                      
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<FIELD_INFO> FIELD_INFO
        {
            get;
            set;
        }
            
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<FIELD_MAPPING> FIELD_MAPPING
        {
           get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<Label> Labels
        {
           get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<LOCAL_ARGUMENT_INFO> LOCAL_ARGUMENT_INFO
        {
           get;
            set;
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<MethodIlOp> MethodIlOps
        {
           get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<AssemblyFile> AssemblyFiles
        {
           get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<Method> Methods
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public DbSet<Document> Documents
        {
            get;
            set;
        }
    }

    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class AssemblyFile
    {
        
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Guid ID
        {
            get;set;
        }
        
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String Pathname
        {
          get;set;
        }

        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual IList<Method> Methods
        {
            get;set;
        }

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Document 
    {
        [Key]
                [DataMemberAttribute()]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        
        [DataMemberAttribute()]
        public global::System.String Pathname
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual IList<Method> Methods
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
    public partial class FIELD_INFO
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String TYPE
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 OFFSET
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
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
    public partial class FIELD_MAPPING
    {
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String TYPE_NAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
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
        public global::System.Guid ID
        {
            get;
            set;
        }

        #endregion

    
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Label 
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int64 Address
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual IList<Method> MethodStart
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual IList<Method> MethodEnd
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
    public partial class LOCAL_ARGUMENT_INFO
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String METHODLABELNAME
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Boolean IsArgument
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 INDEXINMETHOD
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 OFFSET
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
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
    public partial class Method
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        [Key]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 TypeToken
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 MethodToken
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Guid AssemblyFileID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int64 LineColStart
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int64 LineColEnd
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Guid DocumentID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public Nullable<global::System.Guid> LabelStartID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public Nullable<global::System.Guid> LabelEndID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String LabelCall
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual AssemblyFile AssemblyFile
        {
            get;
            set;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual Document Document
        {
            get;
            set;
        }
            
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual Label LabelStart
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual Label LabelEnd
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual IList<MethodIlOp> MethodIlOps
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
    public partial class MethodIlOp
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [Key]
        [DataMemberAttribute()]
        public global::System.Guid ID
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.String LabelName
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 StackDiff
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public global::System.Int32 IlOffset
        {
            get;
            set;
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [DataMemberAttribute()]
        public Nullable<global::System.Guid> MethodID
        {
            get;
            set;
        }
            
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        public virtual Method Method
        {
            get;
            set;
        }
    }
}