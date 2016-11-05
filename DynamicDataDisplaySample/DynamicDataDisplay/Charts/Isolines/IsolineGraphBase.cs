using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using DataSource = Microsoft.Research.DynamicDataDisplay.DataSources.IDataSource2D<double>;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Isolines
{
	public abstract class IsolineGraphBase : ContentGraph
	{
		protected IsolineGraphBase() { }

		private IsolineCollection collection = new IsolineCollection();
		protected IsolineCollection Collection
		{
			get { return collection; }
		}

		private readonly IsolineBuilder isolineBuilder = new IsolineBuilder();
		protected IsolineBuilder IsolineBuilder
		{
			get { return isolineBuilder; }
		}

		private readonly IsolineTextAnnotater annotater = new IsolineTextAnnotater();
		protected IsolineTextAnnotater Annotater
		{
			get { return annotater; }
		}

		private IPalette palette = new HSBPalette();
		/// <summary>
		/// Gets or sets the palette for coloring isoline lines.
		/// </summary>
		/// <value>The palette.</value>
		public IPalette Palette
		{
			get { return palette; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (palette != value)
				{
					palette = value;
					CreateUIRepresentation();
				}
			}
		}

		private bool drawLabels = true;
		/// <summary>
		/// Gets or sets a value indicating whether to draw text labels.
		/// </summary>
		/// <value><c>true</c> if isolines draws labels; otherwise, <c>false</c>.</value>
		public bool DrawLabels
		{
			get { return drawLabels; }
			set
			{
				if (drawLabels != value)
				{
					drawLabels = value;
					CreateUIRepresentation();
				}
			}
		}

		#region DataSource

		private DataSource dataSource = null;
		/// <summary>
		/// Gets or sets the data source.
		/// </summary>
		/// <value>The data source.</value>
		public DataSource DataSource
		{
			get { return dataSource; }
			set
			{
				if (dataSource != value)
				{
					DetachDataSource(dataSource);
					dataSource = value;
					AttachDataSource(dataSource);

					UpdateDataSource();
				}
			}
		}

		/// <summary>
		/// This method is called when data source changes.
		/// </summary>
		protected virtual void UpdateDataSource()
		{
			if (dataSource != null)
			{
				IsolineBuilder.DataSource = dataSource;
				collection = IsolineBuilder.Build();
			}
			else
			{
				collection = null;
			}
		}

		protected virtual void CreateUIRepresentation() { }

		protected virtual void AttachDataSource(IDataSource2D<double> dataSource)
		{
			if (dataSource != null)
			{
				dataSource.Changed += OnDataSourceChanged;
			}
		}

		protected virtual void DetachDataSource(IDataSource2D<double> dataSource)
		{
			if (dataSource != null)
			{
				dataSource.Changed -= OnDataSourceChanged;
			}
		}

		protected virtual void OnDataSourceChanged(object sender, EventArgs e)
		{
			UpdateDataSource();
		}

		#endregion

		#region StrokeThickness

		/// <summary>
		/// Gets or sets thickness of isoline lines.
		/// </summary>
		/// <value>The stroke thickness.</value>
		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}

		/// <summary>
		/// Identifies the StrokeThickness dependency property.
		/// </summary>
		public static readonly DependencyProperty StrokeThicknessProperty =
			DependencyProperty.Register(
			  "StrokeThickness",
			  typeof(double),
			  typeof(IsolineGraphBase),
			  new FrameworkPropertyMetadata(
				  2.0,
				  OnLineThicknessChanged)
				  );

		private static void OnLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			IsolineGraphBase graph = (IsolineGraphBase)d;
			graph.UpdateLineThickness();
		}

		protected virtual void UpdateLineThickness() { }

		#endregion
	}
}
