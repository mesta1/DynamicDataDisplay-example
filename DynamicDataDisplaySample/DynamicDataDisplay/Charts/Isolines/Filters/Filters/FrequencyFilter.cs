using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Filters
{
	internal sealed class FrequencyFilter : IPointFilter {

		/// <summary>Visible region in screen coordinates</summary>
		private Rect visible;

		#region IPointFilter Members

		public void SetVisibleRect(Rect rect) {
			visible = rect;
		}

		public List<Point> Filter(List<Point> points) {
			if(points.Count == 0) return points;
			
			List<Point> resultPoints = points;

			if (points.Count > 2 * visible.Width) {
				resultPoints = new List<Point>();

				List<Point> currentChain = new List<Point>();
				double currentX = Math.Floor(points[0].X);
				foreach (Point p in points) {
					if (Math.Floor(p.X) == currentX) {
						currentChain.Add(p);
					}
					else {
						// Analyse current chain
						if (currentChain.Count <= 2) {
							resultPoints.AddRange(currentChain);
						}
						else {
							Point first = MinByX(currentChain);
							Point last = MaxByX(currentChain);
							Point min = MinByY(currentChain);
							Point max = MaxByY(currentChain);
							resultPoints.Add(first);

							Point smaller = min.X < max.X ? min : max;
							Point greater = min.X > max.X ? min : max;
							if (smaller != resultPoints.GetLast()) {
								resultPoints.Add(smaller);
							}
                            if (greater != resultPoints.GetLast())
                            {
								resultPoints.Add(greater);
							}
                            if (last != resultPoints.GetLast())
                            {
								resultPoints.Add(last);
							}
						}
						currentChain.Clear();
						currentChain.Add(p);
						currentX = Math.Floor(p.X);
					}
				}
			}

			return resultPoints;
		}

		#endregion

		private static Point MinByX(IList<Point> points) {
			Point minPoint = points[0];
			foreach (Point p in points) {
				if (p.X < minPoint.X) {
					minPoint = p;
				}
			}
			return minPoint;
		}

		private static Point MaxByX(IList<Point> points) {
			Point maxPoint = points[0];
			foreach (Point p in points) {
				if (p.X > maxPoint.X) {
					maxPoint = p;
				}
			}
			return maxPoint;
		}

		private static Point MinByY(IList<Point> points) {
			Point minPoint = points[0];
			foreach (Point p in points) {
				if (p.Y < minPoint.Y) {
					minPoint = p;
				}
			}
			return minPoint;
		}

		private static Point MaxByY(IList<Point> points) {
			Point maxPoint = points[0];
			foreach (Point p in points) {
				if (p.Y > maxPoint.Y) {
					maxPoint = p;
				}
			}
			return maxPoint;
		}
	}
}
