/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Defines a type converter.
	/// </summary>
	/// <remarks>This is needed to get rid of the type TypeConverter type that could not give back the Type we were passing to him.
	/// We do not want to use reflection to get the type back from the  ConverterTypeName. Also the GetType methos does not spwan converters from other assemblies.</remarks>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class PropertyPageTypeConverterAttribute : Attribute
	{
		#region fields
		Type converterType;
		#endregion

		#region ctors
		public PropertyPageTypeConverterAttribute(Type type)
		{
			this.converterType = type;
		}
		#endregion

		#region properties
		public Type ConverterType
		{
			get
			{
				return this.converterType;
			}
		}
		#endregion
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class LocDisplayNameAttribute : DisplayNameAttribute
	{
		#region fields
		string name;
		#endregion

		#region ctors
		public LocDisplayNameAttribute(string name)
		{
			this.name = name;
		}
		#endregion

		#region properties
		public override string DisplayName
		{
			get
			{
				string result = SR.GetString(this.name, CultureInfo.CurrentUICulture);
				if(result == null)
				{
					Debug.Assert(false, "String resource '" + this.name + "' is missing");
					result = this.name;
				}
				return result;
			}
		}
		#endregion
	}
}
