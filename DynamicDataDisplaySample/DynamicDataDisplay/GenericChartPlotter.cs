using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Research.DynamicDataDisplay
{
	public class GenericChartPlotter<THorizontal, TVertical> : ChartPlotter
	{
		public GenericChartPlotter()
		{
			if (this is GenericChartPlotter<double, TVertical>)
			{
				var plotter = this as GenericChartPlotter<double, TVertical>;
				plotter.HorizontalToDouble = x => x;
				plotter.DoubleToHorizontal = x => x;
				plotter.HorizontalAxis = new HorizontalAxis();
			}
			else if (this is GenericChartPlotter<TimeSpan, TVertical>)
			{
				var plotter = this as GenericChartPlotter<TimeSpan, TVertical>;

				var conversion = new TimeSpanToDoubleConversion(0, new TimeSpan(), 1, TimeSpan.FromHours(1));
				plotter.HorizontalToDouble = conversion.ToDouble;
				plotter.DoubleToHorizontal = conversion.FromDouble;

				plotter.HorizontalAxis = new HorizontalTimeSpanAxis();

				plotter.HorizontalAxis.ConvertToDouble = conversion.ToDouble;
				plotter.HorizontalAxis.ConvertFromDouble = conversion.FromDouble;
			}
			else if (this is GenericChartPlotter<DateTime, TVertical>)
			{
				var plotter = this as GenericChartPlotter<DateTime, TVertical>;

				var conversion = new DateTimeToDoubleConversion(0, DateTime.Now, 1, DateTime.Now.AddHours(1));
				plotter.HorizontalToDouble = conversion.ToDouble;
				plotter.DoubleToHorizontal = conversion.FromDouble;

				plotter.HorizontalAxis = new HorizontalDateTimeAxis();

				plotter.HorizontalAxis.ConvertToDouble = conversion.ToDouble;
				plotter.HorizontalAxis.ConvertFromDouble = conversion.FromDouble;
			}

			if (this is GenericChartPlotter<THorizontal, double>)
			{
				var plotter = this as GenericChartPlotter<THorizontal, double>;
				plotter.VerticalToDouble = x => x;
				plotter.DoubleToVertical = x => x;

				plotter.VerticalAxis = new VerticalAxis();
			}
			else if (this is GenericChartPlotter<THorizontal, TimeSpan>)
			{
				var plotter = this as GenericChartPlotter<THorizontal, TimeSpan>;

				var conversion = new TimeSpanToDoubleConversion(0, new TimeSpan(), 1, TimeSpan.FromHours(1));
				plotter.VerticalToDouble = conversion.ToDouble;
				plotter.DoubleToVertical = conversion.FromDouble;

				plotter.VerticalAxis = new VerticalTimeSpanAxis();
			}
			else if (this is GenericChartPlotter<THorizontal, DateTime>)
			{
				var plotter = this as GenericChartPlotter<THorizontal, DateTime>;

				var conversion = new DateTimeToDoubleConversion(0, DateTime.Now, 1, DateTime.Now.AddHours(1));
				plotter.VerticalToDouble = conversion.ToDouble;
				plotter.DoubleToVertical = conversion.FromDouble;

				plotter.VerticalAxis = new VerticalDateTimeAxis();
			}
		}

		#region Axes

		protected override void ValidateHorizontalAxis(IAxis axis)
		{
			if (!(axis is AxisBase<THorizontal>))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidGenericAxis, GetType().Name, typeof(THorizontal).Name));
			}
			base.ValidateHorizontalAxis(axis);
		}

		protected override void OnHorizontalAxisChanged()
		{
			if (HorizontalToDouble != null && DoubleToHorizontal != null)
			{
				HorizontalAxis.ConvertToDouble = HorizontalToDouble;
				HorizontalAxis.ConvertFromDouble = DoubleToHorizontal;
			}
			base.OnHorizontalAxisChanged();
		}

		protected override void ValidateVerticalAxis(IAxis axis)
		{
			if (!(axis is AxisBase<TVertical>))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidGenericAxis, GetType().Name, typeof(TVertical).Name));
			}
			base.ValidateVerticalAxis(axis);
		}

		protected override void OnVerticalAxisChanged()
		{
			if (VerticalToDouble != null && DoubleToVertical != null)
			{
				VerticalAxis.ConvertToDouble = VerticalToDouble;
				VerticalAxis.ConvertFromDouble = DoubleToVertical;
			}
			base.OnVerticalAxisChanged();
		}

		public new AxisBase<THorizontal> HorizontalAxis
		{
			get { return (AxisBase<THorizontal>)base.HorizontalAxis; }
			set { base.HorizontalAxis = value; }
		}

		public new AxisBase<TVertical> VerticalAxis
		{
			get { return (AxisBase<TVertical>)base.VerticalAxis; }
			set { base.VerticalAxis = value; }
		}

		#endregion

		#region Conversions

		public Func<THorizontal, double> HorizontalToDouble { get; set; }

		public Func<double, THorizontal> DoubleToHorizontal { get; set; }

		public Func<TVertical, double> VerticalToDouble { get; set; }

		public Func<double, TVertical> DoubleToVertical { get; set; }

		public IValueConversion<THorizontal> HorizontalConversion
		{
			get { return new HorizontalConversionWrapper<THorizontal, TVertical>(this); }
		}

		public IValueConversion<TVertical> VerticalConversion
		{
			get { return new VerticalConversionWrapper<THorizontal, TVertical>(this); }
		}

		public DataRect<THorizontal, TVertical> VisibleRect
		{
			get
			{
				Rect visible = Viewport.Visible;
				var rect = new DataRect<THorizontal, TVertical>(
					DoubleToHorizontal(visible.Left),
					DoubleToVertical(visible.Top),
					DoubleToHorizontal(visible.Right),
					DoubleToVertical(visible.Bottom));

				return rect;
			}
			set
			{
				Rect visible = new Rect(
					new Point(
						HorizontalToDouble(value.X1),
						VerticalToDouble(value.Y1)
						),
					new Point(
						HorizontalToDouble(value.X2),
						VerticalToDouble(value.Y2)));

				Viewport.Visible = visible;
			}
		}

		#endregion

		#region Children data source handling

		protected override void OnChildAdded(IPlotterElement child)
		{
			base.OnChildAdded(child);

			IOneDimensionalChart chart = child as IOneDimensionalChart;
			if (chart != null)
			{
				chart.DataChanged += OnOneDimensionalChart_DataChanged;
				IPointDataSource dataSource = chart.DataSource;
				Visit(dataSource);
			}
		}

		private void Visit(IPointDataSource dataSource)
		{
			if (dataSource == null)
				return;

			CompositeDataSource compositeDS = dataSource as CompositeDataSource;
			Visit(compositeDS);
			VisitEnumerable(dataSource);
		}

		private void Visit(CompositeDataSource compositeDS)
		{
			if (compositeDS == null)
				return;

			foreach (var dataPart in compositeDS.DataParts)
			{
				VisitEnumerable(dataPart);
			}
		}

		private void VisitEnumerable(IPointDataSource dataSource)
		{
			var xDataPart = dataSource as EnumerableXDataSource<THorizontal>;
			Visit(xDataPart);
			var yDataPart = dataSource as EnumerableYDataSource<TVertical>;
			Visit(yDataPart);
		}

		private void Visit(EnumerableXDataSource<THorizontal> dataSource)
		{
			if (dataSource == null)
				return;

			if (dataSource.XYMapping == null && dataSource.XMapping == null)
			{
				dataSource.XMapping = HorizontalToDouble;
			}
		}

		private void Visit(EnumerableYDataSource<TVertical> dataSource)
		{
			if (dataSource == null)
				return;

			if (dataSource.XYMapping == null && dataSource.YMapping == null)
			{
				dataSource.YMapping = VerticalToDouble;
			}
		}

		protected override void OnChildRemoving(IPlotterElement child)
		{
			IOneDimensionalChart chart = child as IOneDimensionalChart;
			if (chart != null)
			{
				chart.DataChanged -= OnOneDimensionalChart_DataChanged;
			}
			base.OnChildRemoving(child);
		}

		private void OnOneDimensionalChart_DataChanged(object sender, EventArgs e)
		{
			IOneDimensionalChart chart = (IOneDimensionalChart)sender;
			Visit(chart.DataSource);
		}

		#endregion

		private sealed class HorizontalConversionWrapper<TH, TV> : IValueConversion<TH>
		{
			[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
			private readonly GenericChartPlotter<TH, TV> plotter;

			public HorizontalConversionWrapper(GenericChartPlotter<TH, TV> plotter)
			{
				this.plotter = plotter;
			}

			#region IValueConversion<T> Members

			public Func<TH, double> ConvertToDouble
			{
				get { return plotter.HorizontalToDouble; }
				set { plotter.HorizontalToDouble = value; }
			}

			public Func<double, TH> ConvertFromDouble
			{
				get { return plotter.DoubleToHorizontal; }
				set { plotter.DoubleToHorizontal = value; }
			}

			#endregion
		}

		private sealed class VerticalConversionWrapper<TH, TV> : IValueConversion<TV>
		{
			private readonly GenericChartPlotter<TH, TV> plotter;

			public VerticalConversionWrapper(GenericChartPlotter<TH, TV> plotter)
			{
				this.plotter = plotter;
			}

			#region IValueConversion<T> Members

			public Func<TV, double> ConvertToDouble
			{
				get { return plotter.VerticalToDouble; }
				set { plotter.VerticalToDouble = value; }
			}

			public Func<double, TV> ConvertFromDouble
			{
				get { return plotter.DoubleToVertical; }
				set { plotter.DoubleToVertical = value; }
			}

			#endregion
		}
	}
}
