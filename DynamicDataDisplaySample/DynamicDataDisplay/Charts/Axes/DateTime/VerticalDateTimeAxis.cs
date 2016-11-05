using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class VerticalDateTimeAxis : DateTimeAxis
	{
		public VerticalDateTimeAxis()
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
