using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay
{
	public class HorizontalAxisTitle : ContentControl, IPlotterElement
	{
		public HorizontalAxisTitle()
		{
			FontSize = 16;
			HorizontalAlignment = HorizontalAlignment.Center;
		}

		private Plotter plotter;
		public Plotter Plotter
		{
			get { return plotter; }
		}

		public void OnPlotterAttached(Plotter plotter)
		{
			this.plotter = plotter;
			plotter.BottomPanel.Children.Add(this);
		}

		public void OnPlotterDetaching(Plotter plotter)
		{
			this.plotter = null;
			plotter.BottomPanel.Children.Remove(this);
		}
	}
}