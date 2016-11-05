using System.Windows;
using System;
using System.ComponentModel;

namespace Microsoft.Research.DynamicDataDisplay
{
	public sealed class PlotterConnectionEventArgs : EventArgs
	{
		public PlotterConnectionEventArgs(Plotter plotter)
		{
			this.plotter = plotter;
		}

		private readonly Plotter plotter;
		public Plotter Plotter
		{
			get { return plotter; }
		}
	}

	/// <summary>
	/// Main interface of DynamicDataDisplay - each item that is going to be added to Plotter should implement it.
	/// Contains methods that are called by parent plotter when item is added to it or removed from it.
	/// </summary>
	public interface IPlotterElement
	{
		/// <summary>
		/// Called when parent plotter is attached.
		/// Allows to, for example, add custom UI parts to ChartPlotter's visual tree or subscribe to ChartPlotter's events.
		/// </summary>
		/// <param name="plotter">The parent plotter.</param>
		void OnPlotterAttached(Plotter plotter);
		/// <summary>
		/// Called when item is being detached from parent plotter.
		/// Allows to remove added in OnPlotterAttached method UI parts or unsubscribe from events.
		/// This should be done as each chart can be added only one Plotter at one moment of time.
		/// </summary>
		/// <param name="plotter">The plotter.</param>
		void OnPlotterDetaching(Plotter plotter);
		/// <summary>
		/// Gets the parent plotter of chart.
		/// Should be equal to null if item is not connected to any plotter.
		/// </summary>
		/// <value>The plotter.</value>
		Plotter Plotter { get; }
	}

	public abstract class PlotterElement : FrameworkElement, IPlotterElement
	{
		private Plotter plotter;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Plotter Plotter
		{
			get { return plotter; }
		}

		/// <summary>This method is invoked when element is attached to plotter. It is the place
		/// to put additional controls to Plotter</summary>
		/// <param name="plotter">Plotter for this element</param>
		public virtual void OnPlotterAttached(Plotter plotter)
		{
			this.plotter = plotter;
			RaisePlotterAttached(plotter);
		}

		public event EventHandler<PlotterConnectionEventArgs> PlotterAttached;
		protected void RaisePlotterAttached(Plotter plotter)
		{
			if (PlotterAttached != null)
			{
				PlotterAttached(this, new PlotterConnectionEventArgs(plotter));
			}
		}

		/// <summary>This method is invoked when element is being detached from plotter. If additional
		/// controls were put on plotter in OnPlotterAttached method, they should be removed here</summary>
		/// <remarks>This method is always called in pair with OnPlotterAttached</remarks>
		public virtual void OnPlotterDetaching(Plotter plotter)
		{
			RaisePlotterDetaching(plotter);
			this.plotter = null;
		}

		public event EventHandler<PlotterConnectionEventArgs> PlotterDetaching;
		protected void RaisePlotterDetaching(Plotter plotter)
		{
			if (PlotterDetaching != null)
			{
				PlotterDetaching(this, new PlotterConnectionEventArgs(plotter));
			}
		}
	}
}