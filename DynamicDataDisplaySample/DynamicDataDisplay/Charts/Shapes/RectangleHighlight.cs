using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	/// <summary>
	/// Represents a rectangle with corners bound to viewport coordinates.
	/// </summary>
	public sealed class RectangleHighlight : ViewportShape
	{
		public RectangleHighlight() { }

		private Rect rect = Rect.Empty;
		public Rect Bounds
		{
			get { return rect; }
			set
			{
				if (rect != value)
				{
					rect = value;
					UpdateUIRepresentation();
				}
			}
		}

		protected override void UpdateUIRepresentationCore()
		{
			var transform = Plotter.Viewport.Transform;

			Point p1 = rect.Location.DataToScreen(transform);
			Point p2 = rect.BottomRight.DataToScreen(transform);
			rectGeometry.Rect = new Rect(p1, p2);
		}

		private RectangleGeometry rectGeometry = new RectangleGeometry();
		protected override Geometry DefiningGeometry
		{
			get { return rectGeometry; }
		}
	}
}
