using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Common.Palettes
{
	public static class LinearPalettes
	{
		static LinearPalettes()
		{
			blackAndWhitePalette.IncreaseBrightness = false;
			rgbPalette.IncreaseBrightness = false;
			blueOrangePalette.IncreaseBrightness = false;
		}

		private static readonly LinearPalette blackAndWhitePalette =
			new LinearPalette(Colors.Black, Colors.White);

		public static LinearPalette BlackAndWhitePalette
		{
			get { return blackAndWhitePalette; }
		}

		private static readonly LinearPalette rgbPalette =
			new LinearPalette(Colors.Blue, Color.FromRgb(0, 255, 0), Colors.Red);

		public static LinearPalette RedGreenBluePalette
		{
			get { return rgbPalette; }
		}

		private static readonly LinearPalette blueOrangePalette = new LinearPalette(
			Colors.Blue,
			Colors.Cyan,
			Colors.Yellow,
			Colors.Orange);

		public static LinearPalette BlueOrangePalette
		{
			get { return blueOrangePalette; }
		}
	}
}
