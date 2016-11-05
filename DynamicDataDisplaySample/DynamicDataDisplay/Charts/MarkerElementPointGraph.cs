using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System.Windows.Controls;

namespace Microsoft.Research.DynamicDataDisplay
{
	public class ElementMarkerPointsGraph : PointsGraphBase
	{
		/// <summary>List with markers for all points</summary>
		private readonly List<UIElement> markers = new List<UIElement>();

		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
		/// </summary>
		public ElementMarkerPointsGraph()
		{
			ManualTranslate = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
		/// </summary>
		/// <param name="dataSource">The data source.</param>
		public ElementMarkerPointsGraph(IPointDataSource dataSource)
		{
			DataSource = dataSource;
		}

		Grid grid;
		Canvas canvas;
		public override void OnPlotterAttached(Plotter plotter)
		{
			base.OnPlotterAttached(plotter);

			grid = new Grid();
			canvas = new Canvas { ClipToBounds = true };
			grid.Children.Add(canvas);

			Plotter2D.CentralGrid.Children.Add(grid);
		}

		public override void OnPlotterDetaching(Plotter plotter)
		{
			Plotter2D.CentralGrid.Children.Remove(grid);
			grid = null;
			canvas = null;

			base.OnPlotterDetaching(plotter);
		}

		protected override void OnDataChanged()
		{
			if (canvas != null)
			{
				canvas.Children.Clear();
			}
			// todo почему так?
			markers.Clear(); // Remove all elements
			base.OnDataChanged();
		}

		public ElementPointMarker Marker
		{
			get { return (ElementPointMarker)GetValue(MarkerProperty); }
			set { SetValue(MarkerProperty, value); }
		}

		public static readonly DependencyProperty MarkerProperty =
			DependencyProperty.Register(
			  "Marker",
			  typeof(ElementPointMarker),
			  typeof(ElementMarkerPointsGraph),
			  new FrameworkPropertyMetadata { DefaultValue = null, AffectsRender = true }
				  );

		protected override void OnRenderCore(DrawingContext dc, RenderState state)
		{
			if (DataSource == null)
			{
				markers.Clear();
				canvas.Children.Clear();
				return;
			}

			if (Marker == null) return;

			var transform = Viewport.Transform;

			if (markers.Count == 0)
			{
				Rect bounds = Rect.Empty;
				using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
				{
					Point point = new Point();
					while (enumerator.MoveNext())
					{
						enumerator.GetCurrent(ref point);
						enumerator.ApplyMappings(Marker);

						Point screenPoint = point.DataToScreen(transform);
						if (!screenPoint.IsFinite())
							continue;

						bounds = Rect.Union(bounds, point);
						UIElement marker = Marker.CreateMarker();
						Marker.SetPosition(marker, screenPoint);

						// todo было раскомментировано. Сделать хранение маркеров и добавление их на плоттер.
						canvas.Children.Add(marker);
						markers.Add(marker);
					}
				}
				ContentBounds = bounds;
			}
			else
			{
				int index = 0;
				using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
				{
					Point point = new Point();
					while (enumerator.MoveNext())
					{
						enumerator.GetCurrent(ref point);
						Point screenPoint = point.DataToScreen(transform);
						Marker.SetPosition(markers[index++], screenPoint);
					}
				}
			}
		}
	}
}
