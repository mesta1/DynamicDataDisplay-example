using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using Microsoft.Research.DynamicDataDisplay;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class ViewportPanel : Panel, IPlotterElement
	{
		public ViewportPanel()
		{
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (plotter == null)
			{
				Plotter parentPlotter = (Plotter)VisualTreeHelperHelper.GetParent(this, 6);

				// todo somehow determine time when to call OnPlotterDetaching.
				if (parentPlotter != null)
				{
					insideViewportListView = true;
					((IPlotterElement)this).OnPlotterAttached(parentPlotter);
					InvalidateMeasure();
				}
			}
		}

		#region DataX & DataY DPs

		[AttachedPropertyBrowsableForChildren]
		public static double GetDataX(DependencyObject obj)
		{
			return (double)obj.GetValue(DataXProperty);
		}

		public static void SetDataX(DependencyObject obj, double value)
		{
			obj.SetValue(DataXProperty, value);
		}

		public static readonly DependencyProperty DataXProperty = DependencyProperty.RegisterAttached(
		  "DataX",
		  typeof(double),
		  typeof(ViewportPanel),
		  new FrameworkPropertyMetadata(Double.NaN, OnPositioningChanged));

		[AttachedPropertyBrowsableForChildren]
		public static double GetDataY(DependencyObject obj)
		{
			return (double)obj.GetValue(DataYProperty);
		}

		public static void SetDataY(DependencyObject obj, double value)
		{
			obj.SetValue(DataYProperty, value);
		}

		public static readonly DependencyProperty DataYProperty = DependencyProperty.RegisterAttached(
		  "DataY",
		  typeof(double),
		  typeof(ViewportPanel),
		  new FrameworkPropertyMetadata(Double.NaN, OnPositioningChanged));

		private static void OnPositioningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement uiElement = d as UIElement;
			if (uiElement != null)
			{
				var parent1 = VisualTreeHelper.GetParent(uiElement);
				if (parent1 != null)
				{
					var parent2 = VisualTreeHelper.GetParent(parent1);
					if (parent2 != null)
					{
						ListViewItem listViewItem = VisualTreeHelper.GetParent(parent2) as ListViewItem;
						if (listViewItem != null)
						{
							listViewItem.SetValue(e.Property, e.NewValue);
						}
					}
				}

				ViewportPanel parent = VisualTreeHelper.GetParent(uiElement) as ViewportPanel;
				if (parent != null)
				{
					parent.InvalidateArrange();
				}
			}
		}

		#endregion

		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
			foreach (UIElement element in base.InternalChildren)
			{
				if (element != null)
				{
					element.Measure(size);
				}
			}
			return new Size();
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			using (new DisposableTimer("ViewportPanel arrange"))
			{
				if (plotter == null)
					return finalSize;

				var transform = plotter.Viewport.Transform;

				foreach (UIElement element in base.InternalChildren)
				{
					if (element == null)
						continue;

					double x = GetDataX(element);
					if (Double.IsNaN(x))
						x = 0;
					double y = GetDataY(element);
					if (Double.IsNaN(y))
						y = 0;

					Point screenPos = new Point(x, y).DataToScreen(transform);
					Rect bounds = RectExtensions.FromCenterSize(screenPos, element.DesiredSize);

					element.Arrange(bounds);
				}

				return finalSize;
			}
		}

		#region IPlotterElement Members

		private bool insideViewportListView = false;
		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			this.plotter = (Plotter2D)plotter;
			this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;

			if (!insideViewportListView)
				plotter.CentralGrid.Children.Add(this);
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			if (!insideViewportListView)
				plotter.CentralGrid.Children.Remove(this);

			this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
			this.plotter = null;
		}

		private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
		{
			InvalidateArrange();
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
