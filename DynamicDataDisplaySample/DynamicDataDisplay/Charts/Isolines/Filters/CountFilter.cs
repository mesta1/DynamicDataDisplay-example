using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Filters
{
	public sealed class CountFilter : IPointsFilter
	{
		private int maxCount = 300;
		public int MaxCount
		{
			get { return maxCount; }
			set { maxCount = value; }
		}

		#region IPointsFilter Members

		public List<Point> Filter(List<Point> points)
		{
			if (points.Count > maxCount)
			{
				int ith = points.Count / maxCount;
				points = points.Where((p, i) => i % ith == 0).ToList();
			}
			return points;
		}

		private Rect visible;
		public void SetVisibleRect(Rect visible)
		{
			this.visible = visible;
		}

		#endregion
	}
}
