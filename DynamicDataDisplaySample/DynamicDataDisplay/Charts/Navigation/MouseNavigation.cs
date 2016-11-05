using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace Microsoft.Research.DynamicDataDisplay.Navigation
{
	/// <summary>Provides common methods of mouse navigation around viewport</summary>
	public class MouseNavigation : NavigationBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MouseNavigation"/> class.
		/// </summary>
		public MouseNavigation() { }

		private AdornerLayer adornerLayer;
		protected AdornerLayer AdornerLayer
		{
			get
			{
				if (adornerLayer == null)
				{
					adornerLayer = AdornerLayer.GetAdornerLayer(this);
					if (adornerLayer != null)
					{
						adornerLayer.IsHitTestVisible = false;
					}
				}

				return adornerLayer;
			}
		}

		public override void OnPlotterAttached(Plotter plotter)
		{
			base.OnPlotterAttached(plotter);

			Mouse.AddPreviewMouseDownHandler(Parent, OnMouseDown);
			Mouse.AddPreviewMouseMoveHandler(Parent, OnMouseMove);
			Mouse.AddPreviewMouseUpHandler(Parent, OnMouseUp);
			Mouse.AddPreviewMouseWheelHandler(Parent, OnMouseWheel);
		}

		public override void OnPlotterDetaching(Plotter plotter)
		{
			Mouse.RemovePreviewMouseDownHandler(Parent, OnMouseDown);
			Mouse.RemovePreviewMouseMoveHandler(Parent, OnMouseMove);
			Mouse.RemovePreviewMouseUpHandler(Parent, OnMouseUp);
			Mouse.RemovePreviewMouseWheelHandler(Parent, OnMouseWheel);

			base.OnPlotterDetaching(plotter);
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (!e.Handled)
			{
				Point mousePos = e.GetPosition(this);
				int delta = -e.Delta;
				MouseWheelZoom(mousePos, delta);

				e.Handled = true;
			}
		}

#if DEBUG
		public override string ToString()
		{
			if (!String.IsNullOrEmpty(Name))
			{
				return Name;
			}
			return base.ToString();
		}
#endif

		bool adornerAdded;
		RectangleSelectionAdorner selectionAdorner;
		private void AddSelectionAdorner()
		{
			if (!adornerAdded)
			{
				AdornerLayer layer = AdornerLayer;
				if (layer != null)
				{
					selectionAdorner = new RectangleSelectionAdorner(this) { Border = zoomRect };

					layer.Add(selectionAdorner);
					adornerAdded = true;
				}
			}
		}

		private void RemoveSelectionAdorner()
		{
			AdornerLayer layer = AdornerLayer;
			if (layer != null)
			{
				layer.Remove(selectionAdorner);
				adornerAdded = false;
			}
		}

		private void UpdateSelectionAdorner()
		{
			selectionAdorner.Border = zoomRect;
			selectionAdorner.InvalidateVisual();
		}

		Rect? zoomRect = null;
		private const double wheelZoomSpeed = 1.2;
		private bool shouldKeepRatioWhileZooming;

		private bool isZooming = false;
		protected bool IsZooming
		{
			get { return isZooming; }
		}

		private bool isPanning = false;
		protected bool IsPanning
		{
			get { return isPanning; }
		}

		private Point panningStartPointInViewport;
		protected Point PanningStartPointInViewport
		{
			get { return panningStartPointInViewport; }
		}

		private Point zoomStartPoint;

		private static bool IsShiftOrCtrl
		{
			get
			{
				ModifierKeys currKeys = Keyboard.Modifiers;
				return (currKeys | ModifierKeys.Shift) == currKeys ||
					(currKeys | ModifierKeys.Control) == currKeys;
			}
		}

		protected virtual bool ShouldStartPanning(MouseButtonEventArgs e)
		{
			return e.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.None;
		}

		protected virtual bool ShouldStartZoom(MouseButtonEventArgs e)
		{
			return e.ChangedButton == MouseButton.Left && IsShiftOrCtrl;
		}

		Point panningStartPointInScreen;
		protected virtual void StartPanning(MouseButtonEventArgs e)
		{
			panningStartPointInScreen = e.GetPosition(this);
			panningStartPointInViewport = panningStartPointInScreen.ScreenToViewport(Viewport.Transform);

			Plotter2D.UndoProvider.CaptureOldValue(Viewport, Viewport2D.VisibleProperty, Viewport.Visible);

			isPanning = true;
			CaptureMouse();
		}

		protected virtual void StartZoom(MouseButtonEventArgs e)
		{
			zoomStartPoint = e.GetPosition(this);
			if (Viewport.Output.Contains(zoomStartPoint))
			{
				isZooming = true;
				AddSelectionAdorner();
				CaptureMouse();
				shouldKeepRatioWhileZooming = Keyboard.Modifiers == ModifierKeys.Shift;
			}
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			// dragging
			bool shouldStartDrag = ShouldStartPanning(e);
			if (shouldStartDrag)
				StartPanning(e);

			// zooming
			bool shouldStartZoom = ShouldStartZoom(e);
			if (shouldStartZoom)
				StartZoom(e);

			((IInputElement)Parent).Focus();
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (!isPanning && !isZooming) return;

			// dragging
			if (isPanning && e.LeftButton == MouseButtonState.Pressed)
			{
				Point endPoint = e.GetPosition(this).ScreenToViewport(Viewport.Transform);

				Point loc = Viewport.Visible.Location;
				Vector shift = panningStartPointInViewport - endPoint;
				loc += shift;

				// preventing unnecessary changes, if actually visible hasn't change.
				if (shift.X != 0 || shift.Y != 0)
				{
					Cursor = Cursors.ScrollAll;

					Rect visible = Viewport.Visible;

					visible.Location = loc;
					Viewport.Visible = visible;
				}
			}
			// zooming
			else if (isZooming && e.LeftButton == MouseButtonState.Pressed)
			{
				Point zoomEndPoint = e.GetPosition(this);
				UpdateZoomRect(zoomEndPoint);
			}
		}

		private static bool IsShiftPressed()
		{
			return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
		}

		private void UpdateZoomRect(Point zoomEndPoint)
		{
			Rect output = Viewport.Output;
			Rect tmpZoomRect = new Rect(zoomStartPoint, zoomEndPoint);
			tmpZoomRect = Rect.Intersect(tmpZoomRect, output);

			shouldKeepRatioWhileZooming = IsShiftPressed();
			if (shouldKeepRatioWhileZooming)
			{
				double currZoomRatio = tmpZoomRect.Width / tmpZoomRect.Height;
				double zoomRatio = output.Width / output.Height;
				if (currZoomRatio < zoomRatio)
				{
					double oldHeight = tmpZoomRect.Height;
					double height = tmpZoomRect.Width / zoomRatio;
					tmpZoomRect.Height = height;
					if (!tmpZoomRect.Contains(zoomStartPoint))
					{
						tmpZoomRect.Offset(0, oldHeight - height);
					}
				}
				else
				{
					double oldWidth = tmpZoomRect.Width;
					double width = tmpZoomRect.Height * zoomRatio;
					tmpZoomRect.Width = width;
					if (!tmpZoomRect.Contains(zoomStartPoint))
					{
						tmpZoomRect.Offset(oldWidth - width, 0);
					}
				}
			}

			zoomRect = tmpZoomRect;
			UpdateSelectionAdorner();
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			OnParentMouseUp(e);
		}

		protected virtual void OnParentMouseUp(MouseButtonEventArgs e)
		{
			if (isPanning && e.ChangedButton == MouseButton.Left)
			{
				isPanning = false;
				StopPanning(e);
			}
			else if (isZooming && e.ChangedButton == MouseButton.Left)
			{
				isZooming = false;
				StopZooming();
			}
		}

		protected virtual void StopZooming()
		{
			if (zoomRect.HasValue)
			{
				Rect output = Viewport.Output;

				Point p1 = zoomRect.Value.TopLeft.ScreenToViewport(Viewport.Transform);
				Point p2 = zoomRect.Value.BottomRight.ScreenToViewport(Viewport.Transform);
				Rect newVisible = new Rect(p1, p2);
				Viewport.Visible = newVisible;

				zoomRect = null;
				ReleaseMouseCapture();
				RemoveSelectionAdorner();
			}
		}

		protected virtual void StopPanning(MouseButtonEventArgs e)
		{
			Plotter2D.UndoProvider.CaptureNewValue(Plotter2D.Viewport, Viewport2D.VisibleProperty, Viewport.Visible);

			Plotter2D.Focus();

			ReleaseMouseCapture();
			ClearValue(CursorProperty);
		}

		//protected override void OnRenderCore(DrawingContext dc, RenderState state)
		//{
		//    // do nothing here
		//}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			if (isZooming)
			{
				RemoveSelectionAdorner();
			}
			ReleaseMouseCapture();
			base.OnLostFocus(e);
		}

		private void MouseWheelZoom(Point mousePos, int wheelRotationDelta)
		{
			Point zoomTo = mousePos.ScreenToViewport(Viewport.Transform);

			double zoomSpeed = Math.Abs(wheelRotationDelta / Mouse.MouseWheelDeltaForOneLine);
			zoomSpeed *= wheelZoomSpeed;
			if (wheelRotationDelta < 0)
			{
				zoomSpeed = 1 / zoomSpeed;
			}
			Viewport.Visible = Viewport.Visible.Zoom(zoomTo, zoomSpeed);
		}
	}
}
