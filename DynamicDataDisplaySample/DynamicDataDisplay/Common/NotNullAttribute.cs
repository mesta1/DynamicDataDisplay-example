using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.DynamicDataDisplay
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	internal sealed class NotNullAttribute : Attribute
	{
	}
}
