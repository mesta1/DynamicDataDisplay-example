using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Common.Auxiliary
{
	public static class VectorExtensions
	{
		public static Vector ToData(this Vector vector, IViewport2D viewport)
		{
			Vector result = new Vector(
				vector.X * viewport.Visible.Width / viewport.Output.Width,
				-vector.Y * viewport.Visible.Height / viewport.Output.Height);

			return result;
		}

		public static Vector ToScreen(this Vector vector, IViewport2D viewport)
		{
			Vector result = new Vector(
				vector.X * viewport.Output.Width / viewport.Visible.Width,
				-vector.Y * viewport.Output.Height / viewport.Visible.Height);

			return result;
		}

		public static Point ToPoint(this Vector vector)
		{
			return new Point(vector.X, vector.Y);
		}
	}
}
