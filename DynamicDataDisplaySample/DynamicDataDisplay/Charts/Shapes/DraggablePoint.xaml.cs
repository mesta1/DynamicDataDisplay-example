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
using System.Windows.Controls.Primitives;
using Microsoft.Research.DynamicDataDisplay;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Shapes
{
	/// <summary>
	/// Represents a simple draggable point with position bound to point in viewport coordinates, which allows to drag iself by mouse.
	/// </summary>
	public partial class DraggablePoint : ViewportUIContainer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DraggablePoint"/> class.
		/// </summary>
		public DraggablePoint()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DraggablePoint"/> class.
		/// </summary>
		/// <param name="position">The position of DraggablePoint.</param>
		public DraggablePoint(Point position) : this() { Position = position; }

		bool dragging = false;
		Point dragStart;
		Vector shift;
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			dragStart = e.GetPosition(Plotter.Viewport).ScreenToData(Plotter.Viewport.Transform);
			shift = Position - dragStart;
			dragging = true;

			CaptureMouse();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!dragging) return;

			Point mouseInData = e.GetPosition(Plotter.Viewport).ScreenToData(Plotter.Viewport.Transform);

			if (mouseInData != dragStart)
			{
				Position = mouseInData + shift;
			}
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();

			dragging = false;
		}
	}
}
