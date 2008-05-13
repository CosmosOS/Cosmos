using System;
using System.Collections.Generic;
using System.Text;

[AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
public sealed class ManifestResourceStreamAttribute: Attribute
{
    public string ResourceName;
}
