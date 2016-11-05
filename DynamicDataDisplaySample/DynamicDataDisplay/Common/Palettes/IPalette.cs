using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Common.Palettes
{
	public interface IPalette
	{
		/// <summary>
		/// Gets the color by interpolation coefficient.
		/// </summary>
		/// <param name="t">Interpolation coefficient, should belong to [0..1].</param>
		/// <returns>Color.</returns>
		Color GetColor(double t);

		event EventHandler Changed;
	}
}
