using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay
{
	public static class BrushHelper
	{
		/// <summary>
		/// Creates a SolidColorBrush with random hue of its color.
		/// </summary>
		/// <returns>A SolicColorBrush with random hue of its color.</returns>
		public static SolidColorBrush CreateBrushWithRandomHue()
		{
			return new SolidColorBrush { Color = ColorHelper.CreateColorWithRandomHue() };
		}

		/// <summary>
		/// Makes SolidColorBrush transparent.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="alpha">The alpha, [0..255]</param>
		/// <returns></returns>
		public static SolidColorBrush MakeTransparent(this SolidColorBrush brush, int alpha)
		{
			Color color = brush.Color;
			color.A = (byte)alpha;

			return new SolidColorBrush(color);
		}

		/// <summary>
		/// Makes SolidColorBrush transparent.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="alpha">The alpha, [0.0 .. 1.0].</param>
		/// <returns></returns>
		public static SolidColorBrush MakeTransparent(this SolidColorBrush brush, double alpha)
		{
			return MakeTransparent(brush, (int)(alpha * 255));
		}
	}
}
