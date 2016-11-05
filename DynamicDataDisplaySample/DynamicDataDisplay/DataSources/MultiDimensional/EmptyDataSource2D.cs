using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.DataSources.MultiDimensional
{
	/// <summary>
	/// Defines empty two-dimensional data source.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class EmptyDataSource2D<T> : IDataSource2D<T>
	{
		#region IDataSource2D<T> Members

		private T[,] data = new T[0, 0];
		public T[,] Data
		{
			get { return data; }
		}

		private Point[,] grid = new Point[0, 0];
		public Point[,] Grid
		{
			get { return grid; }
		}

		public int Width
		{
			get { return 0; }
		}

		public int Height
		{
			get { return 0; }
		}

		IDataSource2D<T> IDataSource2D<T>.GetSubset(int x0, int y0, int countX, int countY, int stepX, int stepY)
		{
			throw new NotImplementedException();
		}

		void IDataSource2D<T>.ApplyMappings(DependencyObject marker, int x, int y)
		{
			throw new NotImplementedException();
		}

		private void RaiseChanged()
		{
			if (Changed != null)
			{
				Changed(this, EventArgs.Empty);
			}
		}
		public event EventHandler Changed;

		#endregion
	}
}
