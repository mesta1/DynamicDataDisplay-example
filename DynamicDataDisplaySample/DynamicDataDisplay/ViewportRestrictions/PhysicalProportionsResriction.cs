using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.ViewportRestrictions
{
	/// <summary>
	/// Represents a restriction where actual visible rect's proportions depends on 
	/// actual output rect's proportions.
	/// </summary>
	public sealed class PhysicalProportionsRestriction : ViewportRestrictionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicalProportionsRestriction"/> class.
		/// </summary>
		public PhysicalProportionsRestriction() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicalProportionsRestriction"/> class with given proportion ratio.
		/// </summary>
		/// <param name="proportionRatio">The proportion ratio.</param>
		public PhysicalProportionsRestriction(double proportionRatio)
		{
			ProportionRatio = proportionRatio;
		}

		private double proportionRatio = 1.0;
		public double ProportionRatio
		{
			get { return proportionRatio; }
			set
			{
				if (proportionRatio != value)
				{
					proportionRatio = value;
					RaiseChanged();
				}
			}
		}

		public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
		{
			Rect output = viewport.Output;
			if (output.Width == 0 || output.Height == 0)
				return newDataRect;

			double screenRatio = output.Width / output.Height;
			double viewportRatio = newDataRect.Width / newDataRect.Height;
			double ratio = screenRatio / viewportRatio;
			double width = proportionRatio * newDataRect.Width * ratio;
			double height = newDataRect.Height;

			if (width < newDataRect.Width)
			{
				height = newDataRect.Height / proportionRatio / ratio;
				width = newDataRect.Width;
			}

			Point center = newDataRect.GetCenter();
			Rect res = RectExtensions.FromCenterSize(center, width, height);

			return res;
		}
	}
}
