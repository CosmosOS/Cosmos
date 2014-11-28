//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;


namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// MDbgException class.  Represents errors that occur in mdbg.
    /// </summary>
    [Serializable()]
    public class MDbgException : ApplicationException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgException.
        /// </summary>
        public MDbgException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }


    /// <summary>
    /// MDbgSpecialSourcePositionException class.  Represents errors that occur because the current source position is special.
    /// </summary>
    [Serializable()]
    public class MDbgSpecialSourcePositionException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgSpecialSourcePositionException.
        /// </summary>
        public MDbgSpecialSourcePositionException()
            : base("Current source position is special")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgSpecialSourcePositionException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgSpecialSourcePositionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgSpecialSourcePositionException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgSpecialSourcePositionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgSpecialSourcePositionException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgSpecialSourcePositionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }

    /// <summary>
    /// MDbgNoCurrentFrameException class.  Represents errors that occur because there is no current frame.
    /// </summary>
    [Serializable()]
    public class MDbgNoCurrentFrameException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgNoCurrentFrameException.
        /// </summary>
        public MDbgNoCurrentFrameException()
            : base("No current frame")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoCurrentFrameException with the specified inner Exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgNoCurrentFrameException(Exception innerException)
            : base("No current frame", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoCurrentFrameException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgNoCurrentFrameException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoCurrentFrameException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgNoCurrentFrameException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoCurrentFrameException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgNoCurrentFrameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// MDbgNoActiveInstanceException class.  Represents errors that occur because there is no active instance.
    /// </summary>
    [Serializable()]
    public class MDbgNoActiveInstanceException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgNoActiveInstanceException.
        /// </summary>
        public MDbgNoActiveInstanceException()
            : base("No active instance")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoActiveInstanceException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgNoActiveInstanceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoActiveInstanceException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgNoActiveInstanceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgNoActiveInstanceException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgNoActiveInstanceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// MDbgValueException class.  Represents errors that occur because a value is bad.
    /// </summary>
    [Serializable()]
    public class MDbgValueException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgValueException.
        /// </summary>
        public MDbgValueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgValueException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgValueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgValueException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgValueException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// MDbgInvalidArgumentException class.  Represents errors that occur because an argument is invalid.
    /// </summary>
    [Serializable()]
    public class MDbgInvalidArgumentException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgInvalidArgumentException.
        /// </summary>
        public MDbgInvalidArgumentException()
            : base("Invalid argument")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgInvalidArgumentException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgInvalidArgumentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgInvalidArgumentException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgInvalidArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgInvalidArgumentException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgInvalidArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// MDbgAmbiguousModuleNameException class.  Represents errors that occur because a module name is ambiguous.  For example if you had mscorlib.exe and mscorlib.dll and asked for mscorlib.
    /// </summary>
    [Serializable()]
    public class MDbgAmbiguousModuleNameException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgAmbiguousModuleNameException.
        /// </summary>
        public MDbgAmbiguousModuleNameException()
            : base("Ambiguous module name")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgAmbiguousModuleNameException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgAmbiguousModuleNameException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgAmbiguousModuleNameException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgAmbiguousModuleNameException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgAmbiguousModuleNameException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgAmbiguousModuleNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }

    /// <summary>
    /// MDbgValueWrongTypeException class.  Represents errors that occur because a value is of the wrong type.
    /// </summary>
    [Serializable()]
    public class MDbgValueWrongTypeException : MDbgException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the MDbgValueWrongTypeException.
        /// </summary>
        public MDbgValueWrongTypeException()
            : base("value is of wrong type")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgValueWrongTypeException with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MDbgValueWrongTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgValueWrongTypeException with the specified error message and inner Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MDbgValueWrongTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MDbgValueWrongTypeException class with serialized data. 
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected MDbgValueWrongTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

}
