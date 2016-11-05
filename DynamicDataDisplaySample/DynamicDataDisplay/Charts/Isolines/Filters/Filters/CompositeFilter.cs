using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Filters
{
	public sealed class CompositeFilter : IPointFilter {
		public CompositeFilter() { }

        public CompositeFilter(params IPointFilter[] filters)
        {
			if (filters == null)
				throw new ArgumentNullException("filters");

            this.filters = new List<IPointFilter>(filters);
		}

        private readonly List<IPointFilter> filters = new List<IPointFilter>();

        #region IPointFilter Members

        public void SetVisibleRect(Rect rect)
        {
            foreach (IPointFilter filter in filters)
                filter.SetVisibleRect(rect);
        }

		public List<Point> Filter(List<Point> points) {
			foreach (var filter in filters) {
				points = filter.Filter(points);
			}
			return points;
		}

		#endregion
	}
}
