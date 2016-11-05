using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Filters
{
    /// <summary>Provides algorithm for filtering point lists in screen coordinates</summary>
	public interface IPointsFilter {

        /// <summary>Performs filtering</summary>
        /// <param name="points">List of source points</param>
        /// <returns>List of filtered points</returns>
		List<Point> Filter(List<Point> points);

        /// <summary>Sets visible rectangle in screen coordinates</summary>
        /// <param name="rect">Visible rectangle</param>
        /// <remarks>Should be invoked before first call to <see cref="Filter"/></remarks>
        void SetVisibleRect(Rect visible);
	}
}
