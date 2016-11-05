using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class TimeSpanAxis : AxisBase<TimeSpan>
	{
		public TimeSpanAxis()
			: base(new TimeSpanAxisControl(),
				DoubleToTimeSpan, TimeSpanToDouble)
		{ }

		private static readonly long minTicks = TimeSpan.MinValue.Ticks;
		private static readonly long maxTicks = TimeSpan.MaxValue.Ticks;
		private static TimeSpan DoubleToTimeSpan(double value)
		{
			long ticks = (long)(value * 10000000000L);

			// todo should we throw an exception if number of ticks is too big or small?
			if (ticks < minTicks)
				ticks = minTicks;
			else if (ticks > maxTicks)
				ticks = maxTicks;

			return new TimeSpan(ticks);
		}

		private static double TimeSpanToDouble(TimeSpan time)
		{
			return time.Ticks / 10000000000.0;
		}

		public void SetConversion(double min, TimeSpan minSpan, double max, TimeSpan maxSpan)
		{
			var conversion = new TimeSpanToDoubleConversion(min, minSpan, max, maxSpan);

			ConvertToDouble = conversion.ToDouble;
			ConvertFromDouble = conversion.FromDouble;
		}

		
	}
}
