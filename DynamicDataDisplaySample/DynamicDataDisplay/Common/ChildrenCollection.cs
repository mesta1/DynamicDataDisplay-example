using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Microsoft.Research.DynamicDataDisplay.Common
{
	/// <summary>
	/// Contains all charts added to ChartPlotter.
	/// </summary>
	public sealed class ChildrenCollection : D3Collection<IPlotterElement>
	{
		protected override void OnItemAdding(IPlotterElement item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
		}

		/// <summary>
		/// This override enables notifying about removing each element, instead of
		/// notifying about collection reset.
		/// </summary>
		protected override void ClearItems()
		{
			var items = new List<IPlotterElement>(base.Items);
			foreach (var item in items)
			{
				Remove(item);
			}
		}
	}
}
