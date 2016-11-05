using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Collections.Generic;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Isolines
{
	/// <summary>
	/// Generates geometric object for isolines of the input 2d scalar field.
	/// </summary>
	public sealed class IsolineBuilder
	{
		/// <summary>
		/// The density of isolines means the number of levels to draw.
		/// </summary>
		private int density = 12;

		static IsolineBuilder()
		{
			SetCellDictionaries();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IsolineBuilder"/> class.
		/// </summary>
		public IsolineBuilder() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="IsolineBuilder"/> class for specified 2d scalar data source.
		/// </summary>
		/// <param name="dataSource">The data source with 2d scalar data.</param>
		public IsolineBuilder(IDataSource2D<double> dataSource)
		{
			DataSource = dataSource;
		}

		#region Private methods

		private static Dictionary<int, Dictionary<int, Edge>> dictChooser = new Dictionary<int, Dictionary<int, Edge>>();
		private static void SetCellDictionaries()
		{
			var bottomDict = new Dictionary<int, Edge>();
			bottomDict.Add((int)CellBitmask.RightBottom, Edge.Right);
			bottomDict.Add(Edge.Left,
				CellBitmask.LeftTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop,
				CellBitmask.LeftTop | CellBitmask.RightBottom | CellBitmask.RightTop,
				CellBitmask.LeftBottom);
			bottomDict.Add(Edge.Right,
				CellBitmask.RightTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.LeftTop,
				CellBitmask.LeftBottom | CellBitmask.LeftTop | CellBitmask.RightTop);
			bottomDict.Add(Edge.Top,
				CellBitmask.RightBottom | CellBitmask.RightTop,
				CellBitmask.LeftBottom | CellBitmask.LeftTop);

			var leftDict = new Dictionary<int, Edge>();
			leftDict.Add(Edge.Top,
				CellBitmask.LeftTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop);
			leftDict.Add(Edge.Right,
				CellBitmask.LeftTop | CellBitmask.RightTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom);
			leftDict.Add(Edge.Bottom,
				CellBitmask.RightBottom | CellBitmask.RightTop | CellBitmask.LeftTop,
				CellBitmask.LeftBottom);

			var topDict = new Dictionary<int, Edge>();
			topDict.Add(Edge.Right,
				CellBitmask.RightTop,
				CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightBottom);
			topDict.Add(Edge.Right,
				CellBitmask.RightBottom,
				CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightTop);
			topDict.Add(Edge.Left,
				CellBitmask.RightBottom | CellBitmask.RightTop | CellBitmask.LeftTop,
				CellBitmask.LeftBottom,
				CellBitmask.LeftTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop);
			topDict.Add(Edge.Bottom,
				CellBitmask.RightBottom | CellBitmask.RightTop,
				CellBitmask.LeftTop | CellBitmask.LeftBottom);

			var rightDict = new Dictionary<int, Edge>();
			rightDict.Add(Edge.Top,
				CellBitmask.RightTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.LeftTop);
			rightDict.Add(Edge.Left,
				CellBitmask.LeftTop | CellBitmask.RightTop,
				CellBitmask.LeftBottom | CellBitmask.RightBottom);
			rightDict.Add(Edge.Bottom,
				CellBitmask.RightBottom,
				CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightTop);

			dictChooser.Add((int)Edge.Left, leftDict);
			dictChooser.Add((int)Edge.Right, rightDict);
			dictChooser.Add((int)Edge.Bottom, bottomDict);
			dictChooser.Add((int)Edge.Top, topDict);
		}

		private Edge GetOutEdge(Edge inEdge, ValuesInCell cv, IrregularCell rect, double value)
		{
			// value smaller than all values in corners or 
			// value greater than all values in corners
			if (!cv.ValueBelongTo(value))
			{
				throw new IsolineGenerationException(Properties.Resources.IsolinesValueIsOutOfCell);
			}

			CellBitmask cellVal = cv.GetCellValue(value);
			var dict = dictChooser[(int)inEdge];
			if (dict.ContainsKey((int)cellVal))
			{
				return dict[(int)cellVal];
			}
			else if (cellVal.IsDiagonal())
			{
				return GetOutForOpposite(inEdge, cellVal, value, cv, rect);
			}

			const double near_zero = 0.0001;
			const double near_one = 1 - near_zero;

			double lt = cv.LeftTop;
			double rt = cv.RightTop;
			double rb = cv.RightBottom;
			double lb = cv.LeftBottom;

			switch (inEdge)
			{
				case Edge.Left:
					if (value == lt)
						value = near_one * lt + near_zero * rt;
					else if (value == rt)
						value = near_one * rt + near_zero * lt;
					else
						throw new IsolineGenerationException(Properties.Resources.IsolinesUnsupportedCase);
					break;
				case Edge.Top:
					if (value == rt)
						value = near_one * rt + near_zero * lt;
					else if (value == lt)
						value = near_one * lt + near_zero * rt;
					else
						throw new IsolineGenerationException(Properties.Resources.IsolinesUnsupportedCase);
					break;
				case Edge.Right:
					if (value == rb)
						value = near_one * rb + near_zero * rt;
					else if (value == rt)
						value = near_one * rt + near_zero * rb;
					else
						throw new IsolineGenerationException(Properties.Resources.IsolinesUnsupportedCase);
					break;
				case Edge.Bottom:
					if (value == rb)
						value = near_one * rb + near_zero * lb;
					else if (value == lb)
						value = near_one * lb + near_zero * rb;
					else
						throw new IsolineGenerationException(Properties.Resources.IsolinesUnsupportedCase);
					break;
			}

			return GetOutEdge(inEdge, cv, rect, value);
		}

		private Edge GetOutForOpposite(Edge inEdge, CellBitmask cellVal, double value, ValuesInCell cellValues, IrregularCell rect)
		{
			Edge outEdge;

			SubCell subCell = GetSubCell(inEdge, value, cellValues);

			int iters = 1000; // max number of iterations
			do
			{
				ValuesInCell subValues = cellValues.GetSubCell(subCell);
				IrregularCell subRect = rect.GetSubRect(subCell);
				outEdge = GetOutEdge(inEdge, subValues, subRect, value);
				bool isAppropriate = subCell.IsAppropriate(outEdge);
				if (isAppropriate)
				{
					ValuesInCell sValues = subValues.GetSubCell(subCell);

					Point point = GetPointXY(outEdge, value, subValues, subRect);
					segments.AddPoint(point);
					return outEdge;
				}
				else
				{
					subCell = GetAdjacentEdge(subCell, outEdge);
				}

				byte e = (byte)outEdge;
				inEdge = (Edge)((e > 2) ? (e >> 2) : (e << 2));
				iters--;
			} while (iters >= 0);

			throw new IsolineGenerationException(Properties.Resources.IsolinesDataIsUndetailized);
		}

		private static SubCell GetAdjacentEdge(SubCell sub, Edge edge)
		{
			SubCell res = SubCell.LeftBottom;

			switch (sub)
			{
				case SubCell.LeftBottom:
					res = edge == Edge.Top ? SubCell.LeftTop : SubCell.RightBottom;
					break;
				case SubCell.LeftTop:
					res = edge == Edge.Bottom ? SubCell.LeftBottom : SubCell.RightTop;
					break;
				case SubCell.RightBottom:
					res = edge == Edge.Top ? SubCell.RightTop : SubCell.LeftBottom;
					break;
				case SubCell.RightTop:
				default:
					res = edge == Edge.Bottom ? SubCell.RightBottom : SubCell.LeftTop;
					break;
			}

			return res;
		}

		private static SubCell GetSubCell(Edge inEdge, double value, ValuesInCell vc)
		{
			double lb = vc.LeftBottom;
			double rb = vc.RightBottom;
			double rt = vc.RightTop;
			double lt = vc.LeftTop;

			SubCell res = SubCell.LeftBottom;
			switch (inEdge)
			{
				case Edge.Left:
					res = (Math.Abs(value - lb) < Math.Abs(value - lt)) ? SubCell.LeftBottom : SubCell.LeftTop;
					break;
				case Edge.Top:
					res = (Math.Abs(value - lt) < Math.Abs(value - rt)) ? SubCell.LeftTop : SubCell.RightTop;
					break;
				case Edge.Right:
					res = (Math.Abs(value - rb) < Math.Abs(value - rt)) ? SubCell.RightBottom : SubCell.RightTop;
					break;
				case Edge.Bottom:
				default:
					res = (Math.Abs(value - lb) < Math.Abs(value - rb)) ? SubCell.LeftBottom : SubCell.RightBottom;
					break;
			}

			ValuesInCell subValues = vc.GetSubCell(res);
			bool valueInside = subValues.ValueBelongTo(value);
			if (!valueInside)
			{
				throw new IsolineGenerationException(Properties.Resources.IsolinesDataIsUndetailized);
			}

			return res;
		}

		private static Point GetPoint(double value, double a1, double a2, Vector v1, Vector v2)
		{
			double ratio = (value - a1) / (a2 - a1);

			Verify.IsTrue(0 <= ratio && ratio <= 1);

			Vector r = (1 - ratio) * v1 + ratio * v2;
			return new Point(r.X, r.Y);
		}

		private Point GetPointXY(Edge edge, double value, ValuesInCell vc, IrregularCell rect)
		{
			double lt = vc.LeftTop;
			double lb = vc.LeftBottom;
			double rb = vc.RightBottom;
			double rt = vc.RightTop;

			switch (edge)
			{
				case Edge.Left:
					return GetPoint(value, lb, lt, rect.LeftBottom, rect.LeftTop);
				case Edge.Top:
					return GetPoint(value, lt, rt, rect.LeftTop, rect.RightTop);
				case Edge.Right:
					return GetPoint(value, rb, rt, rect.RightBottom, rect.RightTop);
				case Edge.Bottom:
					return GetPoint(value, lb, rb, rect.LeftBottom, rect.RightBottom);
				default:
					throw new InvalidOperationException();
			}
		}

		private static bool BelongsToEdge(double value, double edgeValue1, double edgeValue2, bool onBoundary)
		{
			if (onBoundary)
			{
				return (edgeValue1 <= value && value < edgeValue2) ||
				(edgeValue2 <= value && value < edgeValue1);
			}

			return (edgeValue1 < value && value < edgeValue2) ||
				(edgeValue2 < value && value < edgeValue1);
		}

		private bool IsPassed(Edge edge, int i, int j, byte[,] edges)
		{
			switch (edge)
			{
				case Edge.Left:
					return (i == 0) || (edges[i, j] & (byte)edge) != 0;
				case Edge.Bottom:
					return (j == 0) || (edges[i, j] & (byte)edge) != 0;
				case Edge.Top:
					return (j == edges.GetLength(1) - 2) || (edges[i, j + 1] & (byte)Edge.Bottom) != 0;
				case Edge.Right:
					return (i == edges.GetLength(0) - 2) || (edges[i + 1, j] & (byte)Edge.Left) != 0;
				default:
					throw new InvalidOperationException();
			}
		}

		private void MakeEdgePassed(Edge edge, int i, int j)
		{
			switch (edge)
			{
				case Edge.Left:
				case Edge.Bottom:
					edges[i, j] |= (byte)edge;
					break;
				case Edge.Top:
					edges[i, j + 1] |= (byte)Edge.Bottom;
					break;
				case Edge.Right:
					edges[i + 1, j] |= (byte)Edge.Left;
					break;
				default:
					throw new InvalidOperationException();
			}
		}

		private Edge TrackLine(Edge inEdge, double value, ref int x, ref int y, out double newX, out double newY)
		{
			// Getting output edge
			ValuesInCell vc = new ValuesInCell(
				values[x, y],
				values[x + 1, y],
				values[x + 1, y + 1],
				values[x, y + 1]);

			IrregularCell rect = new IrregularCell(
				grid[x, y],
				grid[x + 1, y],
				grid[x + 1, y + 1],
				grid[x, y + 1]);

			Edge outEdge = GetOutEdge(inEdge, vc, rect, value);

			// Drawing new segment
			Point point = GetPointXY(outEdge, value, vc, rect);
			newX = point.X;
			newY = point.Y;
			segments.AddPoint(point);

			// Whether out-edge already was passed?
			if (IsPassed(outEdge, x, y, edges)) // line is closed
			{
				MakeEdgePassed(outEdge, x, y); // boundaries should be marked as passed too
				x = y = -1;
				return Edge.Bottom;
			}

			// Make this edge passed
			MakeEdgePassed(outEdge, x, y);

			// Getting next cell's indices
			switch (outEdge)
			{
				case Edge.Left:
					x--;
					return Edge.Right;
				case Edge.Top:
					y++;
					return Edge.Bottom;
				case Edge.Right:
					x++;
					return Edge.Left;
				case Edge.Bottom:
					y--;
					return Edge.Top;
				default:
					throw new InvalidOperationException();
			}
		}

		private void TrackLineNonRecursive(Edge inEdge, double value, int x, int y)
		{
			int s = x, t = y;

			ValuesInCell vc = new ValuesInCell(
				values[x, y],
				values[x + 1, y],
				values[x + 1, y + 1],
				values[x, y + 1]);

			IrregularCell rect = new IrregularCell(
				grid[x, y],
				grid[x + 1, y],
				grid[x + 1, y + 1],
				grid[x, y + 1]);

			Point point = GetPointXY(inEdge, value, vc, rect);

			segments.StartLine(point, currentRatio, value);

			MakeEdgePassed(inEdge, x, y);

			double x2, y2;
			do
			{
				inEdge = TrackLine(inEdge, value, ref s, ref t, out x2, out y2);
			} while (s != -1);
		}

		#endregion

		private double currentRatio;

		/// <summary>Finds isoline for specified reference value</summary>
		/// <param name="value">Reference value</param>
		private void PrepareCells(double value)
		{
			currentRatio = (value - minMax.Min) / (minMax.Max - minMax.Min);

			Verify.IsTrue(0 <= currentRatio && currentRatio <= 1);

			int xSize = dataSource.Width;
			int ySize = dataSource.Height;
			int x, y;
			for (x = 0; x < xSize; x++)
				for (y = 0; y < ySize; y++)
					edges[x, y] = 0;

			// Looking in boundaries.
			// left
			for (y = 1; y < ySize; y++)
			{
				if (BelongsToEdge(value, values[0, y - 1], values[0, y], true) &&
					(edges[0, y - 1] & (byte)Edge.Left) == 0)
				{
					TrackLineNonRecursive(Edge.Left, value, 0, y - 1);
				}
			}

			// bottom
			for (x = 0; x < xSize - 1; x++)
			{
				if (BelongsToEdge(value, values[x, 0], values[x + 1, 0], true)
					&& (edges[x, 0] & (byte)Edge.Bottom) == 0)
				{
					TrackLineNonRecursive(Edge.Bottom, value, x, 0);
				};
			}

			// right
			x = xSize - 1;
			for (y = 1; y < ySize; y++)
			{
				if (BelongsToEdge(value, values[0, y - 1], values[0, y], true) &&
					(edges[0, y - 1] & (byte)Edge.Left) == 0)
				{
					TrackLineNonRecursive(Edge.Left, value, 0, y - 1);
				};

				if (BelongsToEdge(value, values[x, y - 1], values[x, y], true) &&
					(edges[x, y - 1] & (byte)Edge.Left) == 0)
				{
					TrackLineNonRecursive(Edge.Right, value, x - 1, y - 1);
				};
			}

			// horizontals
			for (y = ySize - 1; y >= 1; y--)
			{
				for (x = 0; x < xSize - 1; x++)
				{
					if ((edges[x, y] & (byte)Edge.Bottom) == 0 &&
						BelongsToEdge(value, values[x, y], values[x + 1, y], false))
					{
						TrackLineNonRecursive(Edge.Top, value, x, y - 1);
					}
				}
			}
		}

		/// <summary>
		/// Builds isoline data by 2d scalar field containing in data source.
		/// </summary>
		/// <returns>Collection of data describing built isolines</returns>
		public IsolineCollection Build()
		{
			VerifyDataSource();


			minMax = dataSource.GetMinMax();
			values = dataSource.Data;
			double[] levels = GetLevelsForIsolines();

			segments = new IsolineCollection();

			foreach (double level in levels)
			{
				PrepareCells(level);
			}

			return segments;
		}

		/// <summary>
		/// Builds isoline data for the specified level in 2d scalar field.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <returns></returns>
		public IsolineCollection Build(double level)
		{
			VerifyDataSource();

			minMax = dataSource.GetMinMax();
			values = dataSource.Data;

			segments = new IsolineCollection();

			PrepareCells(level);

			return segments;
		}

		private void VerifyDataSource()
		{
			if (dataSource == null)
				throw new InvalidOperationException(Properties.Resources.IsolinesDataSourceShouldBeSet);
		}

		IsolineCollection segments;

		private double[,] values;
		private byte[,] edges;
		private Point[,] grid;

		private Range<double> minMax;
		private IDataSource2D<double> dataSource;
		/// <summary>
		/// Gets or sets the data source - 2d scalar field.
		/// </summary>
		/// <value>The data source.</value>
		public IDataSource2D<double> DataSource
		{
			get { return dataSource; }
			set
			{
				if (dataSource != value)
				{
					value.VerifyNotNull("value");

					dataSource = value;

					values = dataSource.Data;
					grid = dataSource.Grid;
					minMax = dataSource.GetMinMax();
					edges = new byte[dataSource.Width, dataSource.Height];
				}
			}
		}

		private const double shiftPercent = 0.05;
		private double[] GetLevelsForIsolines()
		{
			double[] levels;
			double min = minMax.Min;
			double max = minMax.Max;

			double step = (max - min) / (density - 1);
			double delta = (max - min);

			levels = new double[density];
			levels[0] = min + delta * shiftPercent;
			levels[levels.Length - 1] = max - delta * shiftPercent;

			for (int i = 1; i < levels.Length - 1; i++)
				levels[i] = min + i * step;

			return levels;
		}
	}
}
