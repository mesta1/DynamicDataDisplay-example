using System;
using System.Windows.Media;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>
	/// Represents color in Hue Saturation Brightness color space.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hsb")]
	[DebuggerDisplay("HSBColor A={Alpha} H={Hue} S={Saturation} B={Brightness}")]
	public struct HsbColor
	{
		private double hue;
		private double saturation;
		private double brightness;
		private double alpha;

		/// <summary>Hue; [0, 360]</summary>
		public double Hue
		{
			get { return hue; }
			set
			{
				if (value < 0)
					value = 360 - value;

				hue = value % 360;
			}
		}

		/// <summary>Saturation; [0, 1]</summary>
		public double Saturation
		{
			get { return saturation; }
			set { saturation = value; }
		}

		/// <summary>Brightness; [0, 1]</summary>
		public double Brightness
		{
			get { return brightness; }
			set { brightness = value; }
		}

		/// <summary>Alpha; [0, 1]</summary>
		public double Alpha
		{
			get { return alpha; }
			set { alpha = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HSBColor"/> struct.
		/// </summary>
		/// <param name="hue">The hue; [0; 360]</param>
		/// <param name="saturation">The saturation; [0, 1]</param>
		/// <param name="brightness">The brightness; [0, 1]</param>
		public HsbColor(double hue, double saturation, double brightness)
		{
			this.hue = hue;
			this.saturation = saturation;
			this.brightness = brightness;
			alpha = 1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HSBColor"/> struct.
		/// </summary>
		/// <param name="hue">The hue; [0, 360]</param>
		/// <param name="saturation">The saturation; [0, 1]</param>
		/// <param name="brightness">The brightness; [0, 1]</param>
		/// <param name="alpha">The alpha; [0, 1]</param>
		public HsbColor(double hue, double saturation, double brightness, double alpha)
		{
			this.hue = hue;
			this.saturation = saturation;
			this.brightness = brightness;
			this.alpha = alpha;
		}

		/// <summary>
		/// Creates HSBColor from the ARGB color.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Argb")]
		public static HsbColor FromArgb(Color color)
		{
			double limit = 255;

			double r = color.R / limit;
			double g = color.G / limit;
			double b = color.B / limit;

			double max = Math.Max(Math.Max(r, g), b);
			double min = Math.Min(Math.Min(r, g), b);

			double len = max - min;

			double brightness = max; // 0.5 * (max + min);
			double sat;
			double hue;

			if (max == 0 || len == 0)
			{
				sat = hue = 0;
			}
			else
			{
				sat = len / max;
				if (r == max)
				{
					hue = (g - b) / len;
				}
				else if (g == max)
				{
					hue = 2 + (b - r) / len;
				}
				else
				{
					hue = 4 + (r - g) / len;
				}
			}

			hue *= 60;
			if (hue < 0)
				hue += 360;


			//hue = max == min ? 0 :
			//    (max == r && g >= b) ? 60 * (g - b) / len :
			//    (max == r && g < b) ? 60 * (g - b) / len + 360 :
			//    max == g ? 60 * (b - r) / len + 120 : 60 * (r - g) / len + 240;

			//sat = len /  brightness;
			//sat = (sat == 0 || max == min) ? 0 :
			//    (0 <= sat && sat <= 0.5) ? len / (max + min) :
			//    (0.5 < sat && sat < 1) ? len / (2 - 2 * sat) : 1;

			HsbColor res = new HsbColor();
			res.hue = hue;
			res.saturation = sat;
			res.brightness = brightness;
			res.alpha = color.A / limit;
			return res;
		}

		/// <summary>
		/// Converts HSBColor to ARGB color space.
		/// </summary>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Argb")]
		public Color ToArgb()
		{
			double r = 0.0;
			double g = 0.0;
			double b = 0.0;
			double hue = this.hue % 360.0;
			if (saturation == 0.0)
			{
				r = g = b = brightness;
			}
			else
			{
				double smallHue = hue / 60.0;
				int smallHueInt = (int)Math.Floor(smallHue);
				double smallHueFrac = smallHue - smallHueInt;
				double val1 = brightness * (1.0 - saturation);
				double val2 = brightness * (1.0 - (saturation * smallHueFrac));
				double val3 = brightness * (1.0 - (saturation * (1.0 - smallHueFrac)));
				switch (smallHueInt)
				{
					case 0:
						r = brightness;
						g = val3;
						b = val1;
						break;

					case 1:
						r = val2;
						g = brightness;
						b = val1;
						break;

					case 2:
						r = val1;
						g = brightness;
						b = val3;
						break;

					case 3:
						r = val1;
						g = val2;
						b = brightness;
						break;

					case 4:
						r = val3;
						g = val1;
						b = brightness;
						break;

					case 5:
						r = brightness;
						g = val1;
						b = val2;
						break;
				}
			}


			return Color.FromArgb(
				(byte)(Math.Round(alpha * 255)),
				(byte)(Math.Round(r * 255)),
				(byte)(Math.Round(g * 255)),
				(byte)(Math.Round(b * 255)));

			/*
			double clampedHue = this.hue / 360;
			double r = 0;
			double g = 0;
			double b = 0;
			if (saturation == 0.0)
			{
				r = g = b = (int)Math.Floor(brightness * 255.0 + 0.5);
			}
			else
			{
				double h = (clampedHue - Math.Floor(clampedHue)) * 6.0;
				double f = h - Math.Floor(h);
				double p = brightness * (1.0 - saturation);
				double q = brightness * (1.0 - saturation * f);
				double t = brightness * (1.0 - (saturation * (1.0 - f)));

				switch ((int)Math.Floor(h))
				{
					case 0:
						ToArgb(brightness, t, p, out r, out g, out b);
						break;

					case 1:
						ToArgb(q, brightness, p, out r, out g, out b);
						break;

					case 2:
						ToArgb(p, brightness, t, out r, out g, out b);
						break;

					case 3:
						ToArgb(p, q, brightness, out r, out g, out b);
						break;

					case 4:
						ToArgb(t, p, brightness, out r, out g, out b);
						break;

					case 5:
						ToArgb(brightness, p, q, out r, out g, out b);
						break;
				}
			}
			return new Color { A = (byte)(alpha * 255), R = (byte)r, G = (byte)g, B = (byte)b };
			 */
		}

		public override bool Equals(object obj)
		{
			if (obj is HsbColor)
			{
				HsbColor c = (HsbColor)obj;
				return (c.alpha == alpha &&
					c.brightness == brightness &&
					c.hue == hue &&
					c.saturation == saturation);
			}
			else
				return false;
		}

		public override int GetHashCode()
		{
			return alpha.GetHashCode() ^
				brightness.GetHashCode() ^
				hue.GetHashCode() ^
				saturation.GetHashCode();
		}

		public static bool operator ==(HsbColor first, HsbColor second)
		{
			return (first.alpha == second.alpha &&
				first.brightness == second.brightness &&
				first.hue == second.hue &&
				first.saturation == second.saturation);
		}

		public static bool operator !=(HsbColor first, HsbColor second)
		{
			return (first.alpha != second.alpha ||
				first.brightness != second.brightness ||
				first.hue != second.hue ||
				first.saturation != second.saturation);
		}
	}

	public static class ColorExtensions
	{
		/// <summary>
		/// Converts the ARGB color to the HSB color.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hsb")]
		public static HsbColor ToHsbColor(this Color color)
		{
			return HsbColor.FromArgb(color);
		}
	}
}
