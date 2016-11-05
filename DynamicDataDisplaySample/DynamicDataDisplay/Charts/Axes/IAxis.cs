using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	/// <summary>
	/// Represents interface that is implemented by each axis and provides uniform access to some non-generic properties.
	/// </summary>
	public interface IAxis : IPlotterElement
	{
		/// <summary>
		/// Gets or sets the axis placement.
		/// </summary>
		/// <value>The placement.</value>
		AxisPlacement Placement { get; set; }
		/// <summary>
		/// Occurs when ticks are changed.
		/// Used by AxisGrid.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		event EventHandler TicksChanged;

		/// <summary>
		/// Gets the screen coordinates of ticks.
		/// Used by AxisGrid.
		/// </summary>
		/// <value>The screen ticks.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		double[] ScreenTicks { get; }
		/// <summary>
		/// Gets the screen coordinates of minor ticks.
		/// used by AxisGrid.
		/// </summary>
		/// <value>The minor screen ticks.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		MinorTickInfo<double>[] MinorScreenTicks { get; }
	}
}
