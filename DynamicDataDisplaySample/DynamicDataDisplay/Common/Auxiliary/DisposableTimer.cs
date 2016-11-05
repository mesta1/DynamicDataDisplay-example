using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Research.DynamicDataDisplay.Common.Auxiliary
{
	public sealed class DisposableTimer : IDisposable
	{
		private readonly string name;
		Stopwatch timer;
		public DisposableTimer(string name)
		{
			this.name = name;
			timer = Stopwatch.StartNew();
		}

		#region IDisposable Members

		public void Dispose()
		{
			var duration = timer.ElapsedMilliseconds;
			Debug.WriteLine(name + ": elapsed " + duration + " ms.");
			timer.Stop();
		}

		#endregion
	}
}
