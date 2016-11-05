using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class DateTimeLabelProvider : DateTimeLabelProviderBase
	{
		public override UIElement[] CreateLabels(ITicksInfo<DateTime> ticksInfo)
		{
			object info = ticksInfo.Info;
			var ticks = ticksInfo.Ticks;

			if (info is DifferenceIn)
			{
				DifferenceIn diff = (DifferenceIn)info;
				DateFormat = GetDateFormat(diff);
			}

			LabelTickInfo<DateTime> tickInfo = new LabelTickInfo<DateTime> { Info = info };

			UIElement[] res = new UIElement[ticks.Length];
			for (int i = 0; i < ticks.Length; i++)
			{
				tickInfo.Tick = ticks[i];

				string tickText = GetString(tickInfo);
				UIElement label = new TextBlock { Text = tickText, ToolTip = ticks[i] };
				ApplyCustomView(tickInfo, label);
				res[i] = label;
			}

			return res;
		}
	}
}
