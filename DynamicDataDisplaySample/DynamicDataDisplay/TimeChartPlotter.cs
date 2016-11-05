using System;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay.Navigation;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.Windows.Controls;
using Microsoft.Research.DynamicDataDisplay.Charts;


namespace Microsoft.Research.DynamicDataDisplay
{
	public class TimeChartPlotter : ChartPlotter
	{
		public TimeChartPlotter()
		{
			HorizontalAxis = new HorizontalDateTimeAxis();
		}

		public void SetHorizontalAxisMapping(Func<double, DateTime> fromDouble, Func<DateTime, double> toDouble)
		{
			if (fromDouble == null)
				throw new ArgumentNullException("fromDouble");
			if (toDouble == null)
				throw new ArgumentNullException("toDouble");
	

			HorizontalDateTimeAxis axis = (HorizontalDateTimeAxis)HorizontalAxis;
			axis.ConvertFromDouble = fromDouble;
			axis.ConvertToDouble = toDouble;
		}

		public void SetHorizontalAxisMapping(double min, DateTime minDate, double max, DateTime maxDate) {
			HorizontalDateTimeAxis axis = (HorizontalDateTimeAxis)HorizontalAxis;
			
			axis.SetConversion(min, minDate, max, maxDate);
		}
	}
}
