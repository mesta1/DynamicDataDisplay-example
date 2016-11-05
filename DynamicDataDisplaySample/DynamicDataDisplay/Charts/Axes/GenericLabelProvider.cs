using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Axes
{
	public class GenericLabelProvider<T> : LabelProviderBase<T>
	{
		#region ILabelProvider<T> Members

		public override UIElement[] CreateLabels(ITicksInfo<T> ticksInfo)
		{
			var ticks = ticksInfo.Ticks;
			var info = ticksInfo.Info;

			LabelTickInfo<T> tickInfo = new LabelTickInfo<T>();
			UIElement[] res = new UIElement[ticks.Length];
			for (int i = 0; i < res.Length; i++)
			{
				tickInfo.Tick = ticks[i];
				tickInfo.Info = info;

				string text = GetString(tickInfo);

				res[i] = new TextBlock
				{
					Text = text,
					ToolTip = ticks[i].ToString()
				};
			}
			return res;
		}

		#endregion
	}
}
