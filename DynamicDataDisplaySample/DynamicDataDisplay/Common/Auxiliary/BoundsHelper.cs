using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay
{
	public static class BoundsHelper
	{
		/// <summary>Computes bounding rectangle for sequence of points</summary>
		/// <param name="points">Points sequence</param>
		/// <returns>Minimal axis-aligned bounding rectangle</returns>
		public static Rect GetDataBounds(IEnumerable<Point> points)
		{
			Rect bounds = Rect.Empty;

			double xMin = Double.PositiveInfinity;
			double xMax = Double.NegativeInfinity;

			double yMin = Double.PositiveInfinity;
			double yMax = Double.NegativeInfinity;

			foreach (Point p in points)
			{
				xMin = Math.Min(xMin, p.X);
				xMax = Math.Max(xMax, p.X);

				yMin = Math.Min(yMin, p.Y);
				yMax = Math.Max(yMax, p.Y);
			}

			// were some points in collection
			if (!Double.IsInfinity(xMin))
			{
				bounds = MathHelper.CreateRectByPoints(xMin, yMin, xMax, yMax);
			}

			return bounds;
		}

		public static Rect GetViewportBounds(IEnumerable<Point> dataPoints, DataTransform transform)
		{
			return GetDataBounds(dataPoints.DataToViewport(transform));
		}
	}
}
