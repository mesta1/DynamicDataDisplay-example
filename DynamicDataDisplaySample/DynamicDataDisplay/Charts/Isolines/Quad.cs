using System;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Isolines
{
	/// <summary>
	/// Represents quadrangle; its points are arranged by round in one direction.
	/// </summary>
	internal sealed class Quad
	{
		private readonly Point v00;
		public Point V00
		{
			get { return v00; }
		}

		private readonly Point v01;
		public Point V01
		{
			get { return v01; }
		}

		private readonly Point v10;
		public Point V10
		{
			get { return v10; }
		}

		private readonly Point v11;
		public Point V11
		{
			get { return v11; }
		}

		public Quad(Point v00, Point v01, Point v11, Point v10)
		{
			DebugVerify.IsNotNaN(v00);
			DebugVerify.IsNotNaN(v01);
			DebugVerify.IsNotNaN(v11);
			DebugVerify.IsNotNaN(v10);

			this.v00 = v00;
			this.v01 = v01;
			this.v10 = v10;
			this.v11 = v11;
		}

		/// <summary>
		/// Determines whether this quad contains the specified point.
		/// </summary>
		/// <param name="v">The point</param>
		/// <returns>
		/// 	<c>true</c> if quad contains the specified point; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(Point pt)
		{
			// breaking quad into 2 triangles, 
			// points contains in quad, if it contains in at least one half-triangle of it.
			return TriangleContains(v00, v01, v11, pt) || TriangleContains(v00, v10, v11, pt);
		}

		private const double eps = 0.00001;
		private static bool AreClose(double x, double y)
		{
			return Math.Abs(x - y) < eps;
		}

		private static bool TriangleContains(Point a, Point b, Point c, Point v)
		{
			double a0 = a.X - c.X;
			double a1 = b.X - c.X;
			double a2 = a.Y - c.Y;
			double a3 = b.Y - c.Y;

			if (AreClose(a0 * a3, a1 * a2))
			{
				// determinant is too close to zero => apexes are on one line
				Vector ab = a - b;
				Vector ac = a - c;
				Vector bc = b - c;
				Vector ax = a - v;
				Vector bx = b - v;
				bool res = AreClose(ab.X * ax.Y, ab.Y * ax.X) && !AreClose(ab.LengthSquared, 0) ||
					AreClose(ac.X * ax.Y, ac.Y * ax.X) && !AreClose(ac.LengthSquared, 0) ||
					AreClose(bc.X * bx.Y, bc.Y * bx.X) && !AreClose(bc.LengthSquared, 0);
				return res;
			}
			else
			{
				double b1 = v.X - c.X;
				double b2 = v.Y - c.Y;

				// alpha, beta and gamma - are baricentric coordinates of v 
				// in triangle with apexes a, b and c
				double beta = (b2 / a2 * a0 - b1) / (a3 / a2 * a0 - a1);
				double alpha = (b1 - a1 * beta) / a0;
				double gamma = 1 - beta - alpha;
				return alpha >= 0 && beta >= 0 && gamma >= 0;
			}
		}
	}
}
