using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public sealed class VerticalLine : SimpleLine
	{
		protected override void UpdateUIRepresentationCore()
		{
			var transform = Plotter.Viewport.Transform;

			Point p1 = new Point(Value, Plotter.Viewport.Visible.Top).DataToScreen(transform);
			Point p2 = new Point(Value, Plotter.Viewport.Visible.Bottom).DataToScreen(transform);

			LineGeometry.StartPoint = p1;
			LineGeometry.EndPoint = p2;
		}
	}
}
