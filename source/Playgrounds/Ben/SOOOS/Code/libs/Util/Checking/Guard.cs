
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Diagnostics;

namespace Util
{
	/// <summary>
	/// Common guard clauses
	/// </summary>
	public static class Guard
	{
		/// <summary>
		/// Checks a string argument to ensure it isn't null or empty
		/// </summary>
		/// <param name="argumentValue">The argument value to check.</param>
		/// <param name="argumentName">The name of the argument.</param>
        [DebuggerStepThrough]
		public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
		{
			ArgumentNotNull(argumentValue, argumentName);

			if (argumentValue.Length == 0)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "String cannot be empty", argumentName));


        }

		/// <summary>
		/// Checks an argument to ensure it isn't null
		/// </summary>
		/// <param name="argumentValue">The argument value to check.</param>
		/// <param name="argumentName">The name of the argument.</param>
        [DebuggerStepThrough]
        public static void ArgumentNotNull(object argumentValue, string argumentName)
		{
			if (argumentValue == null)
				throw new ArgumentNullException(argumentName + " Is null" );
		}

		/// <summary>
		/// Checks an Enum argument to ensure that its value is defined by the specified Enum type.
		/// </summary>
		/// <param name="enumType">The Enum type the value should correspond to.</param>
		/// <param name="value">The value to check for.</param>
		/// <param name="argumentName">The name of the argument holding the value.</param>
        [DebuggerStepThrough]
        public static void EnumValueIsDefined(Type enumType, object value, string argumentName)
		{
			if (Enum.IsDefined(enumType, value) == false)
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
					"Invalid enum value" ,
					argumentName, enumType.ToString()));
		}

		/// <summary>
		/// Verifies that an argument type is assignable from the provided type (meaning
		/// interfaces are implemented, or classes exist in the base class hierarchy).
		/// </summary>
		/// <param name="assignee">The argument type.</param>
		/// <param name="providedType">The type it must be assignable from.</param>
		/// <param name="argumentName">The argument name.</param>
        [DebuggerStepThrough]
        public static void TypeIsAssignableFromType(Type assignee, Type providedType, string argumentName)
		{
			if (!providedType.IsAssignableFrom(assignee))
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
					"Type not compatible", assignee, providedType), argumentName);
		}

        [DebuggerStepThrough]
        public static void CollectionNotNullOrEmpty(ICollection collection ,string argumentName)
        {
            ArgumentNotNull(collection, argumentName);

            if (collection.Count == 0)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Collection Cannot Be Null Or Empty", argumentName));

        }

        [DebuggerStepThrough]
        public static void ArrayNotNullOrEmpty(Array collection, string argumentName)
        {
            ArgumentNotNull(collection, argumentName);

            if (collection.Length == 0)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Collection Cannot Be Null Or Empty", argumentName));

        }

        /// <summary>
        /// If the 'truth' isn't true, throw an empty CommonException.
        /// </summary>
        /// <param name="truth">The 'truth' to evaluate.</param>
        [DebuggerStepThrough]
        public static void Require(bool truth)
        {
            if (!truth)
            {
                throw new Exception(string.Empty);
            }
        }

        /// <summary>
        /// If the 'truth' isn't true, throw a CommonException with the provided message.
        /// </summary>
        /// <param name="truth">The 'truth' to evaluate.</param>
        /// <param name="message">The contents of the CommonException thrown if the 'truth' is false.</param>
        [DebuggerStepThrough]
        public static void Require(bool truth, string message)
        {
            ArgumentNotNullOrEmptyString(message, "message");
            if (!truth)
            {
                throw new Exception(message);
            }
        }

        /// <summary>
        /// If the 'truth' isn't true, throw the provided exception.
        /// </summary>
        /// <param name="truth">The 'truth' to evaluate.</param>
        /// <param name="e">The Exception thrown if the 'truth' is false.</param>
        [DebuggerStepThrough]
        public static void Require(bool truth, Exception e)
        {
            ArgumentNotNull(e, "e");
            if (!truth)
            {
                throw e;
            }
        }

        ///// <summary>
        /////     Throws an <see cref="ArgumentNullException"/> if the
        /////     provided string is null.
        /////     Throws an <see cref="ArgumentOutOfRangeException"/> if the
        /////     provided string is empty.
        ///// </summary>
        ///// <param name="stringParameter">The object to test for null and empty.</param>
        ///// <param name="parameterName">The string for the ArgumentException parameter, if thrown.</param>
        //[DebuggerStepThrough]
        //public static void RequireNotNullOrEmpty(string stringParameter, string parameterName)
        //{
        //    if (stringParameter == null)
        //    {
        //        throw new ArgumentNullException(parameterName);
        //    }
        //    else if (stringParameter.Length == 0)
        //    {
        //        throw new ArgumentOutOfRangeException(parameterName);
        //    }
        //}

        ///// <summary>
        /////     Throws an <see cref="ArgumentNullException"/> if the
        /////     provided object is null.
        ///// </summary>
        ///// <param name="obj">The object to test for null.</param>
        ///// <param name="parameterName">The string for the ArgumentNullException parameter, if thrown.</param>
        //[DebuggerStepThrough]
        //public static void RequireNotNull(object obj, string parameterName)
        //{
        //    if (obj == null)
        //    {
        //        throw new ArgumentNullException(parameterName);
        //    }
        //}

        /// <summary>
        ///     Throws an <see cref="ArgumentException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="parameterName">The string for <see cref="ArgumentException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgument(bool truth, string parameterName)
        {
            Guard.ArgumentNotNullOrEmptyString(parameterName, "parameterName");

            if (!truth)
            {
                throw new ArgumentException(parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="paramName">The paramName for the <see cref="ArgumentException"/>, if thrown.</param>
        /// <param name="message">The message for <see cref="ArgumentException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgument(bool truth, string paramName, string message)
        {
            Guard.ArgumentNotNullOrEmptyString(paramName, "paramName");
            Guard.ArgumentNotNullOrEmptyString(message, "message");

            if (!truth)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="parameterName">The string for <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgumentRange(bool truth, string parameterName)
        {
            Guard.ArgumentNotNullOrEmptyString(parameterName, "parameterName");

            if (!truth)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="paramName">The paramName for the <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
        /// <param name="message">The message for <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgumentRange(bool truth, string paramName, string message)
        {
            Guard.ArgumentNotNullOrEmptyString(paramName, "paramName");
            Guard.ArgumentNotNullOrEmptyString(message, "message");

            if (!truth)
            {
                throw new ArgumentOutOfRangeException(message, paramName);
            }
        }

	} // Guard
}
