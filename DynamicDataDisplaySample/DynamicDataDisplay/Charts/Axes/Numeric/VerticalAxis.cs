using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class VerticalAxis : NumericAxis
	{
		public VerticalAxis()
		{
			Placement = AxisPlacement.Left;
		}

		protected override void ValidatePlacement(AxisPlacement newPlacement)
		{
			if (newPlacement == AxisPlacement.Bottom || newPlacement == AxisPlacement.Top)
				throw new ArgumentException(Properties.Resources.VerticalAxisCannotBeHorizontal);
		}
	}
}
