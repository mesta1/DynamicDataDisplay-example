using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Microsoft.Research.DynamicDataDisplay.Common.Auxiliary
{
	internal static class DispatcherExtensions
	{
		internal static void BeginInvoke(this Dispatcher dispatcher, Action action)
		{
			dispatcher.BeginInvoke((Delegate)action);
		}
	}
}
