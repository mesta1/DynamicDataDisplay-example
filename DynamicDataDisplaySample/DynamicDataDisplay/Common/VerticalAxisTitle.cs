using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay
{
	public class VerticalAxisTitle : ContentControl, IPlotterElement
	{
		public VerticalAxisTitle()
		{
			LayoutTransform = new RotateTransform(-90);
			VerticalAlignment = VerticalAlignment.Center;
			FontSize = 16;
		}

		private Plotter plotter;
		public Plotter Plotter
		{
			get { return plotter; }
		}

		public void OnPlotterAttached(Plotter plotter)
		{
			this.plotter = plotter;
			plotter.LeftPanel.Children.Insert(0, this);
		}

		public void OnPlotterDetaching(Plotter plotter)
		{
			this.plotter = null;
			plotter.LeftPanel.Children.Remove(this);
		}
	}
}