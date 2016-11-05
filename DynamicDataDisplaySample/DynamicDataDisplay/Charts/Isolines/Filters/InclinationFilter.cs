using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Filters
{
	public sealed class InclinationFilter : IPointsFilter
	{
		private double criticalAngle = 178;
		public double CriticalAngle
		{
			get { return criticalAngle; }
			set { criticalAngle = value; }
		}

		#region IPointFilter Members

		public void SetVisibleRect(Rect visible)
		{
			// Nothing to do here - inclination filtering doesn't
			// need visible rectangle
		}

		public List<Point> Filter(List<Point> points)
		{
			if (points.Count == 0)
				return points;

			List<Point> res = new List<Point> { points[0] };

			int i = 1;
			while (i < points.Count)
			{
				bool added = false;
				int j = i;
				while (!added && (j < points.Count - 1))
				{
					Point x1 = res[res.Count - 1];
					Point x2 = points[j];
					Point x3 = points[j + 1];

					double a = (x1 - x2).Length;
					double b = (x2 - x3).Length;
					double c = (x1 - x3).Length;

					double angle13 = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
					double degrees = 180 / Math.PI * angle13;
					if (degrees < criticalAngle)
					{
						res.Add(x2);
						added = true;
						i = j + 1;
					}
					else
					{
						j++;
					}
				}
				// reached the end of resultPoints
				if (!added)
				{
					res.Add(points.GetLast());
					break;
				}
			}
			return res;
		}

		#endregion
	}
}
