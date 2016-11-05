using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>
	/// Represents a text situated in ChartPlotter's footer.
	/// </summary>
	public class Footer : ContentControl, IPlotterElement
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Footer"/> class.
		/// </summary>
		public Footer()
		{
			HorizontalAlignment = HorizontalAlignment.Center;
			Margin = new Thickness(0, 0, 0, 3);
		}

		public void OnPlotterAttached(Plotter plotter)
		{
			this.plotter = plotter;
			plotter.FooterPanel.Children.Add(this);
		}

		public void OnPlotterDetaching(Plotter plotter)
		{
			plotter.FooterPanel.Children.Remove(this);
			this.plotter = null;
		}

		private Plotter plotter;
		public Plotter Plotter
		{
			get { return plotter;; }
		}
	}
}
