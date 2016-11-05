using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Research.DynamicDataDisplay.Common.Auxiliary
{
	public static class IDataSource2DExtensions
	{
		public static Range<double> GetMinMax(this double[,] data)
		{
			data.VerifyNotNull("data");

			int width = data.GetLength(0);
			int height = data.GetLength(1);
			Verify.IsTrueWithMessage(width > 0, Properties.Resources.ArrayWidthShouldBePositive);
			Verify.IsTrueWithMessage(height > 0, Properties.Resources.ArrayHeightShouldBePositive);

			double min = data[0, 0];
			double max = data[0, 0];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (data[x, y] < min)
						min = data[x, y];
					if (data[x, y] > max)
						max = data[x, y];
				}
			}

			Range<double> res = new Range<double>(min, max);
			return res;
		}

		public static Range<double> GetMinMax(this IDataSource2D<double> dataSource)
		{
			dataSource.VerifyNotNull("data");

			return GetMinMax(dataSource.Data);
		}

		public static Rect GetGridBounds(this Point[,] grid)
		{
			double minX = grid[0, 0].X;
			double maxX = minX;
			double minY = grid[0, 0].Y;
			double maxY = minY;

			int width = grid.GetLength(0);
			int height = grid.GetLength(1);
			for (int ix = 0; ix < width; ix++)
			{
				for (int iy = 0; iy < height; iy++)
				{
					Point pt = grid[ix, iy];

					double x = pt.X;
					double y = pt.Y;
					if (x < minX) minX = x;
					if (x > maxX) maxX = x;

					if (y < minY) minY = y;
					if (y > maxY) maxY = y;
				}
			}
			return new Rect(new Point(minX, minY), new Point(maxX, maxY));
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static Rect GetGridBounds<T>(this IDataSource2D<T> dataSource)
		{
			return dataSource.Grid.GetGridBounds();
		}
	}
}
