using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Research.DynamicDataDisplay
{
	public static class CoordinateTransformExtensions
	{
		#region Points

		/// <summary>
		/// Transforms point in data coordinates to screen coordinates.
		/// </summary>
		/// <param name="dataPoint">Point in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in screen coordinates</returns>
		public static Point DataToScreen(this Point dataPoint, CoordinateTransform transform)
		{
			return transform.DataToScreen(dataPoint);
		}

		/// <summary>
		/// Transforms point in screen coordinates to data coordinates.
		/// </summary>
		/// <param name="screenPoint">Point in screen coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in data coordinates</returns>
		public static Point ScreenToData(this Point screenPoint, CoordinateTransform transform)
		{
			return transform.ScreenToData(screenPoint);
		}

		/// <summary>
		/// Transforms point in screen coordinates to viewport coordinates.
		/// </summary>
		/// <param name="screenPoint">Point in screen coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in viewport coordinates</returns>
		public static Point ScreenToViewport(this Point screenPoint, CoordinateTransform transform)
		{
			return transform.ScreenToViewport(screenPoint);
		}

		/// <summary>
		/// Transforms point in viewport coordinates to screen coordinates.
		/// </summary>
		/// <param name="viewportPoint">Point in viewport coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in screen coordinates</returns>
		public static Point ViewportToScreen(this Point viewportPoint, CoordinateTransform transform)
		{
			return transform.ViewportToScreen(viewportPoint);
		}

		/// <summary>
		/// Transforms point in viewport coordinates to data coordinates.
		/// </summary>
		/// <param name="viewportPoint">Point in viewport coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in data coordinates</returns>
		public static Point ViewportToData(this Point viewportPoint, CoordinateTransform transform)
		{
			return transform.DataTransform.ViewportToData(viewportPoint);
		}

		/// <summary>
		/// Transforms point in data coordinates to viewport coordinates.
		/// </summary>
		/// <param name="dataPoint">Point in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in viewport coordinates</returns>
		public static Point DataToViewport(this Point dataPoint, CoordinateTransform transform)
		{
			return transform.DataTransform.DataToViewport(dataPoint);
		}

		/// <summary>
		/// Transforms point in data coordinates to viewport coordinates.
		/// </summary>
		/// <param name="dataPoint">Point in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Point in viewport coordinates</returns>
		public static Point DataToViewport(this Point dataPoint, DataTransform transform)
		{
			return transform.DataToViewport(dataPoint);
		}

		#endregion

		#region Rects

		/// <summary>
		/// Transforms rectangle from screen coordinates to data coordinates.
		/// </summary>
		/// <param name="screenRect">Rectangle in screen coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in data coordinates</returns>
		public static Rect ScreenToData(this Rect screenRect, CoordinateTransform transform)
		{
			Point p1 = screenRect.BottomLeft.ScreenToData(transform);
			Point p2 = screenRect.TopRight.ScreenToData(transform);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from data coordinates to screen coordinates.
		/// </summary>
		/// <param name="dataRect">Rectangle in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in screen coordinates</returns>
		public static Rect DataToScreen(this Rect dataRect, CoordinateTransform transform)
		{
			Point p1 = dataRect.BottomLeft.DataToScreen(transform);
			Point p2 = dataRect.TopRight.DataToScreen(transform);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from screen coordinates to viewport coordinates.
		/// </summary>
		/// <param name="screenRect">Rectangle in screen coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in viewport coordinates</returns>
		public static Rect ScreenToViewport(this Rect screenRect, CoordinateTransform transform)
		{
			Point p1 = screenRect.BottomLeft.ScreenToViewport(transform);
			Point p2 = screenRect.TopRight.ScreenToViewport(transform);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from viewport coordinates to screen coordinates.
		/// </summary>
		/// <param name="viewportRect">Rectangle in viewport coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in screen coordinates</returns>
		public static Rect ViewportToScreen(this Rect viewportRect, CoordinateTransform transform)
		{
			Point p1 = viewportRect.BottomLeft.ViewportToScreen(transform);
			Point p2 = viewportRect.TopRight.ViewportToScreen(transform);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from viewport coordinates to data coordinates.
		/// </summary>
		/// <param name="viewportRect">Rectangle in viewport coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in data coordinates</returns>
		public static Rect ViewportToData(this Rect viewportRect, CoordinateTransform transform)
		{
			Point p1 = viewportRect.BottomLeft.ViewportToData(transform);
			Point p2 = viewportRect.TopRight.ViewportToData(transform);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from data coordinates to viewport coordinates.
		/// </summary>
		/// <param name="dataRect">Rectangle in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in viewport coordinates</returns>
		public static Rect DataToViewport(this Rect dataRect, CoordinateTransform transform)
		{
			Point p1 = dataRect.BottomLeft.DataToViewport(transform);
			Point p2 = dataRect.TopRight.DataToViewport(transform);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from viewport coordinates to data coordinates.
		/// </summary>
		/// <param name="viewportRect">Rectangle in viewport coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in data coordinates</returns>
		public static Rect ViewportToData(this Rect viewportRect, DataTransform transform)
		{
			Point p1 = transform.ViewportToData(viewportRect.BottomLeft);
			Point p2 = transform.ViewportToData(viewportRect.TopRight);

			return new Rect(p1, p2);
		}

		/// <summary>
		/// Transforms rectangle from data coordinates to viewport coordinates.
		/// </summary>
		/// <param name="dataRect">Rectangle in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Rectangle in viewport coordinates</returns>
		public static Rect DataToViewport(this Rect dataRect, DataTransform transform)
		{
			Point p1 = transform.DataToViewport(dataRect.BottomLeft);
			Point p2 = transform.DataToViewport(dataRect.TopRight);

			return new Rect(p1, p2);
		}

		#endregion

		#region Collections

		/// <summary>
		/// Transforms list of points from data coordinates to screen coordinates.
		/// </summary>
		/// <param name="dataPoints">Points in data coordinates</param>
		/// <param name="transform">CoordinateTransform used to perform transformation</param>
		/// <returns>Points in screen coordinates</returns>
		public static List<Point> DataToScreen(this IEnumerable<Point> dataPoints, CoordinateTransform transform)
		{
			ICollection<Point> iCollection = dataPoints as ICollection<Point>;
			List<Point> res;

			if (iCollection != null)
			{
				res = new List<Point>(iCollection.Count);
			}
			else
			{
				res = new List<Point>();
			}

			foreach (var point in dataPoints)
			{
				res.Add(transform.DataToScreen(point));
			}

			return res;
		}

		/// <summary>
		/// Transforms list of points from data coordinates to screen coordinates.
		/// </summary>
		/// <param name="transform">Coordinate transform used to perform transformation</param>
		/// <param name="dataPoints">Points in data coordinates</param>
		/// <returns>List of points in screen coordinates</returns>
		[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public static List<Point> DataToScreen(this CoordinateTransform transform, IEnumerable<Point> dataPoints)
		{
			return dataPoints.DataToScreen(transform);
		}

		/// <summary>
		/// Transforms list of points from data coordinates to viewport coordinates.
		/// </summary>
		/// <param name="dataPoints">Points in data coordinates</param>
		/// <param name="transform">Data transform used to perform transformation</param>
		/// <returns>List of points in viewport coordinates</returns>
		public static IEnumerable<Point> DataToViewport(this IEnumerable<Point> dataPoints, DataTransform transform)
		{
			foreach (Point pt in dataPoints)
			{
				yield return pt.DataToViewport(transform);
			}
		}

		#endregion
	}
}
