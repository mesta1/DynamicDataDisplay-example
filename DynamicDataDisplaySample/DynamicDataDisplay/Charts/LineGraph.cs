using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.Filters;
using Microsoft.Research.DynamicDataDisplay.Charts;
using System.Collections.Specialized;


namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>Series of points connected by one polyline</summary>
	public class LineGraph : PointsGraphBase
	{
		/// <summary>Filters applied to points before rendering</summary>
		private readonly FilterCollection filters = new FilterCollection();

		/// <summary>
		/// Initializes a new instance of the <see cref="LineGraph"/> class.
		/// </summary>
		public LineGraph()
		{
			Legend.SetVisibleInLegend(this, true);
			ManualTranslate = true;

			filters.CollectionChanged += filters_CollectionChanged;
		}

		void filters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			filteredPoints = null;
			Update();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LineGraph"/> class.
		/// </summary>
		/// <param name="pointSource">The point source.</param>
		public LineGraph(IPointDataSource pointSource)
			: this()
		{
			DataSource = pointSource;
		}

		protected override Description CreateDefaultDescription()
		{
			return new PenDescription();
		}

		/// <summary>Provides access to filters collection</summary>
		public FilterCollection Filters
		{
			get { return filters; }
		}

		#region Pen

		/// <summary>
		/// Gets or sets the brush, using which polyline is plotted.
		/// </summary>
		/// <value>The line brush.</value>
		public Brush Stroke
		{
			get { return LinePen.Brush; }
			set
			{
				if (LinePen.Brush != value)
				{
					if (!LinePen.IsSealed)
					{
						LinePen.Brush = value;
						InvalidateVisual();
					}
					else
					{
						Pen pen = LinePen.Clone();
						pen.Brush = value;
						LinePen = pen;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the line thickness.
		/// </summary>
		/// <value>The line thickness.</value>
		public double StrokeThickness
		{
			get { return LinePen.Thickness; }
			set
			{
				if (LinePen.Thickness != value)
				{
					if (!LinePen.IsSealed)
					{
						LinePen.Thickness = value; InvalidateVisual();
					}
					else
					{
						Pen pen = LinePen.Clone();
						pen.Thickness = value;
						LinePen = pen;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the line pen.
		/// </summary>
		/// <value>The line pen.</value>
		[NotNull]
		public Pen LinePen
		{
			get { return (Pen)GetValue(LinePenProperty); }
			set { SetValue(LinePenProperty, value); }
		}

		public static readonly DependencyProperty LinePenProperty =
			DependencyProperty.Register(
			"LinePen",
			typeof(Pen),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				new Pen(Brushes.Blue, 1),
				FrameworkPropertyMetadataOptions.AffectsRender
				),
			OnValidatePen);

		private static bool OnValidatePen(object value)
		{
			return value != null;
		}

		#endregion

		protected override void OnOutputChanged(Rect newRect, Rect oldRect)
		{
			filteredPoints = null;

			base.OnOutputChanged(newRect, oldRect);
		}

		protected override void OnDataChanged()
		{
			filteredPoints = null;

			base.OnDataChanged();
		}

		protected override void OnVisibleChanged(Rect newRect, Rect oldRect)
		{
			if (newRect.Size != oldRect.Size)
			{
				filteredPoints = null;
			}

			base.OnVisibleChanged(newRect, oldRect);
		}

		private FakePointList filteredPoints;
		protected override void UpdateCore()
		{
			if (DataSource == null) return;

			Rect output = Viewport.Output;
			var transform = GetTransform();

			if (filteredPoints == null || !(transform.DataTransform is IdentityTransform))
			{
				IEnumerable<Point> points = GetPoints();

				ContentBounds = BoundsHelper.GetViewportBounds(points, transform.DataTransform);

				transform = GetTransform();
				List<Point> transformedPoints = transform.DataToScreen(points);

				// Analysis and filtering of unnecessary points
				filteredPoints = new FakePointList(FilterPoints(transformedPoints),
					output.Left, output.Right);

				Offset = new Vector();
			}
			else
			{
				double left = output.Left;
				double right = output.Right;
				double shift = Offset.X;
				left -= shift;
				right -= shift;

				filteredPoints.SetXBorders(left, right);
			}
		}


		protected override void OnRenderCore(DrawingContext dc, RenderState state)
		{
			if (DataSource == null) return;

			if (filteredPoints.HasPoints)
			{
				StreamGeometry geometry = new StreamGeometry();
				using (StreamGeometryContext context = geometry.Open())
				{
					context.BeginFigure(filteredPoints.StartPoint, false, false);
					context.PolyLineTo(filteredPoints, true, true);
				}
				geometry.Freeze();

				const Brush brush = null;
				Pen pen = LinePen;


				bool isTranslated = IsTranslated;
				if (isTranslated)
				{
					dc.PushTransform(new TranslateTransform(Offset.X, Offset.Y));
				}
				dc.DrawGeometry(brush, pen, geometry);
				if (isTranslated)
				{
					dc.Pop();
				}

#if __DEBUG
				FormattedText text = new FormattedText(filteredPoints.Count.ToString(),
					CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
					new Typeface("Arial"), 12, Brushes.Black);
				dc.DrawText(text, Viewport.Output.GetCenter());
#endif
			}
		}

		private bool filteringEnabled = true;
		public bool FilteringEnabled
		{
			get { return filteringEnabled; }
			set
			{
				if (filteringEnabled != value)
				{
					filteringEnabled = value;
					filteredPoints = null;
					Update();
				}
			}
		}

		private List<Point> FilterPoints(List<Point> points)
		{
			if (!filteringEnabled)
				return points;

			var filteredPoints = filters.Filter(points, Viewport.Output);

			return filteredPoints;
		}
	}
}
