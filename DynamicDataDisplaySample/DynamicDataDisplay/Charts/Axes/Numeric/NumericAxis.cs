using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class NumericAxis : AxisBase<double>
	{
		public NumericAxis()
			: base(new NumericAxisControl(),
				d => d,
				d => d)
		{
		}
	}
}
