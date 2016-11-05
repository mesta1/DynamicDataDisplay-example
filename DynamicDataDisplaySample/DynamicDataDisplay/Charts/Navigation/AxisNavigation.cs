using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Navigation
{
	/// <summary>
	/// Represents a navigation methods upon one axis - mouse panning and zooming.
	/// </summary>
	public sealed class AxisNavigation : ContentGraph
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AxisNavigation"/> class.
		/// </summary>
		public AxisNavigation()
		{
			SetHorizontalOrientation();
			Content = content;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AxisNavigation"/> class.
		/// </summary>
		/// <param name="orientation">The orientation.</param>
		public AxisNavigation(Orientation orientation)
		{
			Orientation = orientation;
			Content = content;
		}

		private void SetHorizontalOrientation()
		{
			Grid.SetColumn(this, 1);
			Grid.SetRow(this, 2);
		}

		private void SetVerticalOrientation()
		{
			// todo should automatically search for location of axes as they can be 
			// not only from the left or bottom.
			Grid.SetColumn(this, 0);
			Grid.SetRow(this, 1);
		}

		private Orientation orientation = Orientation.Horizontal;
		/// <summary>
		/// Gets or sets the orientation of AxisNavigation.
		/// </summary>
		/// <value>The orientation.</value>
		public Orientation Orientation
		{
			get { return orientation; }
			set
			{
				if (orientation != value)
				{
					orientation = value;
					OnOrientationChanged();
				}
			}
		}

		private void OnOrientationChanged()
		{
			switch (orientation)
			{
				case Orientation.Horizontal:
					SetHorizontalOrientation();
					break;
				case Orientation.Vertical:
					SetVerticalOrientation();
					break;
				default:
					break;
			}
		}

		private bool lmbPressed = false;
		private Point dragStart;

		private CoordinateTransform Transform
		{
			get { return Plotter2D.Viewport.Transform; }
		}

		protected override Panel HostPanel
		{
			get { return Plotter2D.MainGrid; }
		}

		private readonly Panel content = new Canvas { Background = Brushes.Transparent };
		private readonly SolidColorBrush fillBrush = new SolidColorBrush(Color.FromRgb(255, 228, 209)).MakeTransparent(0.2);
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			dragStart = e.GetPosition(this).ScreenToViewport(Transform);
			lmbPressed = true;

			content.Background = fillBrush;
			Cursor = orientation == Orientation.Horizontal ? Cursors.ScrollWE : Cursors.ScrollNS;

			CaptureMouse();
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);
			lmbPressed = false;

			ClearValue(CursorProperty);
			content.Background = Brushes.Transparent;

			ReleaseMouseCapture();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (lmbPressed)
			{
				Point mousePos = e.GetPosition(this).ScreenToViewport(Transform);

				Rect visible = Plotter2D.Viewport.Visible;
				double delta;
				if (orientation == Orientation.Horizontal)
				{
					delta = (mousePos - dragStart).X;
					visible.X -= delta;
				}
				else
				{
					delta = (mousePos - dragStart).Y;
					visible.Y -= delta;
				}
				Plotter2D.Viewport.Visible = visible;
			}
		}

		private const double wheelZoomSpeed = 1.2;
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			Point mousePos = e.GetPosition(this);
			int delta = -e.Delta;

			Point zoomTo = mousePos.ScreenToViewport(Transform);

			double zoomSpeed = Math.Abs(delta / Mouse.MouseWheelDeltaForOneLine);
			zoomSpeed *= wheelZoomSpeed;
			if (delta < 0)
			{
				zoomSpeed = 1 / zoomSpeed;
			}

			Rect visible = Plotter2D.Viewport.Visible.Zoom(zoomTo, zoomSpeed);
			Rect oldVisible = Plotter2D.Viewport.Visible;
			if (orientation == Orientation.Horizontal)
			{
				visible.Y = oldVisible.Y;
				visible.Height = oldVisible.Height;
			}
			else
			{
				visible.X = oldVisible.X;
				visible.Width = oldVisible.Width;
			}
			Plotter2D.Viewport.Visible = visible;

			e.Handled = true;
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);

			ClearValue(CursorProperty);
			content.Background = Brushes.Transparent;

			ReleaseMouseCapture();
		}
	}
}
