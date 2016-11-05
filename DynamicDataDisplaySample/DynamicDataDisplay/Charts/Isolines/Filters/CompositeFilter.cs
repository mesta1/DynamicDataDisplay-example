using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Filters
{
	public sealed class CompositeFilter : IPointsFilter
	{
		public CompositeFilter() { }

		public CompositeFilter(params IPointsFilter[] filters)
		{
			if (filters == null)
				throw new ArgumentNullException("filters");

			this.filters = new List<IPointsFilter>(filters);
		}

		private readonly List<IPointsFilter> filters = new List<IPointsFilter>();

		#region IPointFilter Members

		public void SetVisibleRect(Rect visible)
		{
			foreach (IPointsFilter filter in filters)
				filter.SetVisibleRect(visible);
		}

		public List<Point> Filter(List<Point> points)
		{
			foreach (var filter in filters)
			{
				points = filter.Filter(points);
			}
			return points;
		}

		#endregion
	}
}
