using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class ViewportRectPanel : Panel, IPlotterElement
	{
		#region Properties

		public static Point GetPoint1(DependencyObject obj)
		{
			return (Point)obj.GetValue(Point1Property);
		}

		public static void SetPoint1(DependencyObject obj, Point value)
		{
			obj.SetValue(Point1Property, value);
		}

		public static readonly DependencyProperty Point1Property = DependencyProperty.RegisterAttached(
		  "Point1",
		  typeof(Point),
		  typeof(ViewportRectPanel),
		  new FrameworkPropertyMetadata(new Point(), OnPointChanged));

		public static Point GetPoint2(DependencyObject obj)
		{
			return (Point)obj.GetValue(Point2Property);
		}

		public static void SetPoint2(DependencyObject obj, Point value)
		{
			obj.SetValue(Point2Property, value);
		}

		public static readonly DependencyProperty Point2Property = DependencyProperty.RegisterAttached(
		  "Point2",
		  typeof(Point),
		  typeof(ViewportRectPanel),
		  new FrameworkPropertyMetadata(new Point(), OnPointChanged));

		private static void OnPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement uiElement = d as UIElement;
			if (uiElement != null)
			{
				ViewportRectPanel panel = VisualTreeHelper.GetParent(uiElement) as ViewportRectPanel;
				if (panel != null)
				{
					// invalidating not self arrange, but call Arrange method of only uiElement which has changed position
					panel.InvalidateArrange(uiElement);
				}
			}
		}

		private void InvalidateArrange(UIElement uiElement) {
			if (plotter == null)
				return;

			var transform = plotter.Transform;
			var childBounds = GetElementBounds(transform, uiElement);

			uiElement.Arrange(childBounds);
		}

		#endregion

		#region Panel methods override

		protected override Size MeasureOverride(Size availableSize)
		{
			if (plotter == null)
				return availableSize;

			var transform = plotter.Viewport.Transform;

			foreach (UIElement child in InternalChildren)
			{
				if (child != null)
				{
					Rect bounds = GetElementBounds(transform, child);

					child.Measure(bounds.Size);
				}
			}

			return availableSize;
		}

		private static Rect GetElementBounds(CoordinateTransform transform, UIElement child)
		{
			Point p1 = GetPoint1(child);
			Point p2 = GetPoint2(child);

			Point p1Screen = p1.DataToScreen(transform);
			Point p2Screen = p2.DataToScreen(transform);

			Rect bounds = new Rect(p1Screen, p2Screen);
			return bounds;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (plotter == null)
				return finalSize;

			var transform = plotter.Viewport.Transform;

			foreach (UIElement child in InternalChildren)
			{
				if (child != null)
				{
					Rect bounds = GetElementBounds(transform, child);
					child.Arrange(bounds);
				}
			}

			return finalSize;
		}

		#endregion

		#region IPlotterElement Members

		private Plotter2D plotter;
		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			this.plotter = (Plotter2D)plotter;
			plotter.CentralGrid.Children.Add(this);
			this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;
		}

		private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Visible")
			{
				Rect oldVisible = (Rect)e.OldValue;
				Rect visible = (Rect)e.NewValue;

				if (oldVisible.Size == visible.Size)
				{
					Point prevLocation = oldVisible.Location.ViewportToScreen(plotter.Transform);
					Point location = visible.Location.ViewportToScreen(plotter.Transform);

					VisualOffset += prevLocation - location;
				}
				else
				{
					VisualOffset = new Vector();
					InvalidateArrange();
				}
			}
			else if (e.PropertyName == "Output")
			{
				VisualOffset = new Vector();
				InvalidateArrange();
			}
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
			plotter.CentralGrid.Children.Remove(this);
			this.plotter = null;
		}

		Plotter IPlotterElement.Plotter
		{
			get { return plotter; }
		}

		#endregion
	}
}
