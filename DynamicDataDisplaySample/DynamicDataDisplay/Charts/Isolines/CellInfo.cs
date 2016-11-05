using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Isolines
{
	/// <summary>
	/// Isoline's grid cell
	/// </summary>
	internal interface ICell
	{
		Vector LeftTop { get; }
		Vector LeftBottom { get; }
		Vector RightTop { get; }
		Vector RightBottom { get; }
	}

	internal sealed class IrregularCell : ICell
	{
		public IrregularCell(Vector leftBottom, Vector rightBottom, Vector rightTop, Vector leftTop)
		{
			this.leftBottom = leftBottom;
			this.rightBottom = rightBottom;
			this.rightTop = rightTop;
			this.leftTop = leftTop;
		}

		public IrregularCell(Point lb, Point rb, Point rt, Point lt)
		{
			leftTop = lt.ToVector();
			leftBottom = lb.ToVector();
			rightTop = rt.ToVector();
			rightBottom = rb.ToVector();
		}

		#region ICell Members

		private readonly Vector leftTop;
		public Vector LeftTop
		{
			get { return leftTop; }
		}

		private readonly Vector leftBottom;
		public Vector LeftBottom
		{
			get { return leftBottom; }
		}

		private readonly Vector rightTop;
		public Vector RightTop
		{
			get { return rightTop; }
		}

		private readonly Vector rightBottom;
		public Vector RightBottom
		{
			get { return rightBottom; }
		}

		#endregion

		#region Sides
		public Vector LeftSide
		{
			get { return (leftBottom + leftTop) / 2; }
		}

		public Vector RightSide
		{
			get { return (rightBottom + rightTop) / 2; }
		}
		public Vector TopSide
		{
			get { return (leftTop + rightTop) / 2; }
		}
		public Vector BottomSide
		{
			get { return (leftBottom + rightBottom) / 2; }
		}
		#endregion

		public Point Center
		{
			get { return ((LeftSide + RightSide) / 2).ToPoint(); }
		}

		public IrregularCell GetSubRect(SubCell sub)
		{
			switch (sub)
			{
				case SubCell.LeftBottom:
					return new IrregularCell(LeftBottom, BottomSide, Center.ToVector(), LeftSide);
				case SubCell.LeftTop:
					return new IrregularCell(LeftSide, Center.ToVector(), TopSide, LeftTop);
				case SubCell.RightBottom:
					return new IrregularCell(BottomSide, RightBottom, RightSide, Center.ToVector());
				case SubCell.RightTop:
				default:
					return new IrregularCell(Center.ToVector(), RightSide, RightTop, TopSide);
			}
		}
	}

	internal enum SubCell
	{
		LeftBottom = 0,
		LeftTop = 1,
		RightBottom = 2,
		RightTop = 3
	}

	internal class ValuesInCell
	{
		public ValuesInCell(double leftBottom, double rightBottom, double rightTop, double leftTop)
		{
			DebugVerify.IsNotNaN(leftBottom);
			DebugVerify.IsNotNaN(rightBottom);
			DebugVerify.IsNotNaN(rightTop);
			DebugVerify.IsNotNaN(leftTop);

			this.leftTop = leftTop;
			this.leftBottom = leftBottom;
			this.rightTop = rightTop;
			this.rightBottom = rightBottom;

			left = (leftTop + leftBottom) / 2;
			right = (rightTop + rightBottom) / 2;
			top = (leftTop + rightTop) / 2;
			bottom = (leftBottom + rightBottom) / 2;
		}

		internal bool ValueBelongTo(double value)
		{
			IEnumerable<double> values = new double[] { leftTop, leftBottom, rightTop, rightBottom };

			return !(values.All(v => v > value) || values.All(v => v < value));
		}

		#region Edges
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double leftTop;
		public double LeftTop { get { return leftTop; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double leftBottom;
		public double LeftBottom { get { return leftBottom; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double rightTop;
		public double RightTop
		{
			get { return rightTop; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double rightBottom;
		public double RightBottom
		{
			get { return rightBottom; }
		}
		#endregion

		#region Sides & center
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double left;
		public double Left
		{
			get { return left; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double right;
		public double Right
		{
			get { return right; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double top;
		public double Top
		{
			get { return top; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly double bottom;
		public double Bottom
		{
			get { return bottom; }
		}

		public double Center
		{
			get { return (Left + Right) * 0.5; }
		}
		#endregion

		#region SubCells
		public ValuesInCell LeftTopCell
		{
			get { return new ValuesInCell(Left, Center, Top, LeftTop); }
		}

		public ValuesInCell RightTopCell
		{
			get { return new ValuesInCell(Center, Right, RightTop, Top); }
		}

		public ValuesInCell RightBottomCell
		{
			get { return new ValuesInCell(Bottom, RightBottom, Right, Center); }
		}

		public ValuesInCell LeftBottomCell
		{
			get { return new ValuesInCell(LeftBottom, Bottom, Center, Left); }
		}

		public ValuesInCell GetSubCell(SubCell subCell)
		{
			switch (subCell)
			{
				case SubCell.LeftBottom:
					return LeftBottomCell;
				case SubCell.LeftTop:
					return LeftTopCell;
				case SubCell.RightBottom:
					return RightBottomCell;
				case SubCell.RightTop:
				default:
					return RightTopCell;
			}
		}

		#endregion

		/// <summary>
		/// Returns bitmask of comparison of values at cell corners with reference value.
		/// Corresponding bit is set to one if value at cell corner is greater than reference value. 
		/// a------b
		/// | Cell |
		/// d------c
		/// </summary>
		/// <param name="a">Value at corner (see figure)</param>
		/// <param name="b">Value at corner (see figure)</param>
		/// <param name="c">Value at corner (see figure)</param>
		/// <param name="d">Value at corner (see figure)</param>
		/// <param name="value">Reference value</param>
		/// <returns>Bitmask</returns>
		public CellBitmask GetCellValue(double value)
		{
			CellBitmask n = CellBitmask.None;
			if (leftTop > value)
				n |= CellBitmask.LeftTop;
			if (leftBottom > value)
				n |= CellBitmask.LeftBottom;
			if (rightBottom > value)
				n |= CellBitmask.RightBottom;
			if (rightTop > value)
				n |= CellBitmask.RightTop;

			return n;
		}
	}
}
