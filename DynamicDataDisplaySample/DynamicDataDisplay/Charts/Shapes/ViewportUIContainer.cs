using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Threading;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public sealed class PositionChangedEventArgs : EventArgs
	{
		public Point Position { get; internal set; }
		public Point PreviousPosition { get; internal set; }
	}

	public class ViewportUIContainer : ContentControl, IPlotterElement
	{
		static ViewportUIContainer()
		{
			Type type = typeof(ViewportUIContainer);

			// todo subscribe for properties changes
			HorizontalContentAlignmentProperty.AddOwner(
				type, new FrameworkPropertyMetadata(HorizontalAlignment.Center));
			VerticalContentAlignmentProperty.AddOwner(
				type, new FrameworkPropertyMetadata(VerticalAlignment.Center));
		}

		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			UpdateUIRepresentation();
		}

		public Point Position
		{
			get { return (Point)GetValue(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}

		public static readonly DependencyProperty PositionProperty =
			DependencyProperty.Register(
			  "Position",
			  typeof(Point),
			  typeof(ViewportUIContainer),
			  new FrameworkPropertyMetadata(new Point(0, 0), OnPositionChanged));

		private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ViewportUIContainer container = (ViewportUIContainer)d;
			container.OnPositionChanged(e);
		}

		public event EventHandler<PositionChangedEventArgs> PositionChanged;

		private void OnPositionChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == PositionProperty)
			{
				PositionChanged.Raise(this, new PositionChangedEventArgs { Position = (Point)e.NewValue, PreviousPosition = (Point)e.OldValue });
			}
			UpdateUIRepresentation();
		}

		public Vector Shift
		{
			get { return (Vector)GetValue(ShiftProperty); }
			set { SetValue(ShiftProperty, value); }
		}

		public static readonly DependencyProperty ShiftProperty =
			DependencyProperty.Register(
			  "Shift",
			  typeof(Vector),
			  typeof(ViewportUIContainer),
			  new FrameworkPropertyMetadata(new Vector(), OnPositionChanged));

		#region IPlotterElement Members

		private Plotter2D plotter;
		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			plotter.MainCanvas.Children.Add(this);

			Plotter2D plotter2d = (Plotter2D)plotter;
			this.plotter = plotter2d;
			plotter2d.Viewport.PropertyChanged += Viewport_PropertyChanged;

			UpdateUIRepresentation();
		}

		void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
		{
			UpdateUIRepresentation();
		}

		private void UpdateUIRepresentation()
		{
			if (plotter == null)
				return;

			var transform = Plotter.Viewport.Transform;

			Point position = Position.DataToScreen(transform);
			position += Shift;

			double x = position.X;
			double y = position.Y;

			UIElement content = Content as UIElement;
			if (content != null)
			{
				if (!content.IsMeasureValid)
				{
					content.InvalidateMeasure();

					Dispatcher.BeginInvoke(
						((Action)(() => { UpdateUIRepresentation(); })), DispatcherPriority.Background);
					return;
				}

				Size contentSize = content.DesiredSize;

				switch (HorizontalContentAlignment)
				{
					case HorizontalAlignment.Center:
						x -= contentSize.Width / 2;
						break;
					case HorizontalAlignment.Left:
						break;
					case HorizontalAlignment.Right:
						x -= contentSize.Width;
						break;
					case HorizontalAlignment.Stretch:
						break;
					default:
						break;
				}

				switch (VerticalContentAlignment)
				{
					case VerticalAlignment.Bottom:
						y -= contentSize.Height;
						break;
					case VerticalAlignment.Center:
						y -= contentSize.Height / 2;
						break;
					case VerticalAlignment.Stretch:
						break;
					case VerticalAlignment.Top:
						break;
					default:
						break;
				}
			}

			Canvas.SetLeft(this, x);
			Canvas.SetTop(this, y);
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			Plotter2D plotter2d = (Plotter2D)plotter;
			plotter2d.Viewport.PropertyChanged -= Viewport_PropertyChanged;
			plotter.MainCanvas.Children.Remove(this);

			this.plotter = null;
		}

		public Plotter2D Plotter
		{
			get { return plotter; }
		}

		Plotter IPlotterElement.Plotter
		{
			get { return plotter; }
		}

		#endregion
	}
}
