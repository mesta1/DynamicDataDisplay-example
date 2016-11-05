using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	/// <summary>
	/// Interaction logic for ViewportListView.xaml
	/// </summary>
	public partial class ViewportListView : ListView, IPlotterElement
	{
		public ViewportListView()
		{
			InitializeComponent();
		}

		#region IPlotterElement Members

		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			this.plotter = (Plotter2D)plotter;
			this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;

			plotter.CentralGrid.Children.Add(this);
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			plotter.CentralGrid.Children.Remove(this);

			this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
			this.plotter = null;
		}

		private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
		{
		}

		Plotter IPlotterElement.Plotter
		{
			get { return plotter; }
		}

		private Plotter2D plotter;
		public Plotter2D Plotter
		{
			get { return plotter; }
		}

		#endregion
	}
}
