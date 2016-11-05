using System.Windows;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Charts;
using System.Windows.Controls;
using System;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>This element draws grid over viewport. Number of 
	/// grid lines depends on horizontal and vertical axes</summary>
	public class AxisGrid : ContentControl, IPlotterElement
	{
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		internal void BeginTicksUpdate()
		{
		}
		internal void EndTicksUpdate()
		{
			UpdateUIRepresentation();
		}

		protected internal MinorTickInfo<double>[] MinorHorizontalTicks { get; set; }

		protected internal MinorTickInfo<double>[] MinorVerticalTicks { get; set; }

		protected internal double[] HorizontalTicks { get; set; }

		protected internal double[] VerticalTicks { get; set; }


		private bool drawVerticalTicks = true;
		public bool DrawVerticalTicks
		{
			get { return drawVerticalTicks; }
			set
			{
				if (drawVerticalTicks != value)
				{
					drawVerticalTicks = value;
					UpdateUIRepresentation();
				}
			}
		}

		private bool drawHorizontalTicks = true;
		public bool DrawHorizontalTicks
		{
			get { return drawHorizontalTicks; }
			set
			{
				if (drawHorizontalTicks != value)
				{
					drawHorizontalTicks = value;
					UpdateUIRepresentation();
				}
			}
		}

		private bool drawHorizontalMinorTicks = false;
		public bool DrawHorizontalMinorTicks
		{
			get { return drawHorizontalMinorTicks; }
			set
			{
				if (drawHorizontalMinorTicks != value)
				{
					drawHorizontalMinorTicks = value;
					UpdateUIRepresentation();
				}
			}
		}

		private bool drawVerticalMinorTicks = false;
		public bool DrawVerticalMinorTicks
		{
			get { return drawVerticalMinorTicks; }
			set
			{
				if (drawVerticalMinorTicks != value)
				{
					drawVerticalMinorTicks = value;
					UpdateUIRepresentation();
				}
			}
		}

		private double gridBrushThickness = 1;

		private Path path = new Path();
		private Canvas canvas = new Canvas();
		public AxisGrid()
		{
			IsHitTestVisible = false;

			canvas.ClipToBounds = true;

			path.Stroke = Brushes.LightGray;
			path.StrokeThickness = gridBrushThickness;

			Content = canvas;
		}

		private void UpdateUIRepresentation()
		{
			canvas.Children.Clear();
			Size size = RenderSize;

			DrawMinorHorizontalTicks();
			DrawMinorVerticalTicks();

			GeometryGroup group = new GeometryGroup();
			if (HorizontalTicks != null && drawHorizontalTicks)
			{
				double minY = 0;
				double maxY = size.Height;

				for (int i = 0; i < HorizontalTicks.Length; i++)
				{
					double screenX = HorizontalTicks[i];
					LineGeometry line = new LineGeometry(new Point(screenX, minY), new Point(screenX, maxY));
					group.Children.Add(line);
				}
			}

			if (VerticalTicks != null && drawVerticalTicks)
			{
				double minX = 0;
				double maxX = size.Width;

				for (int i = 0; i < VerticalTicks.Length; i++)
				{
					double screenY = VerticalTicks[i];
					LineGeometry line = new LineGeometry(new Point(minX, screenY), new Point(maxX, screenY));
					group.Children.Add(line);
				}
			}

			canvas.Children.Add(path);
			path.Data = group;
		}

		private void DrawMinorVerticalTicks()
		{
			Size size = RenderSize;
			if (MinorVerticalTicks != null && drawVerticalMinorTicks)
			{
				double minX = 0;
				double maxX = size.Width;

				for (int i = 0; i < MinorVerticalTicks.Length; i++)
				{
					double screenY = MinorVerticalTicks[i].Tick;
					if (screenY < 0)
						continue;
					if (screenY > size.Height)
						continue;

					Line line = new Line
					{
						Y1 = screenY,
						Y2 = screenY,
						X1 = minX,
						X2 = maxX,
						Stroke = Brushes.LightGray,
						StrokeThickness = MinorVerticalTicks[i].Value * gridBrushThickness
					};
					canvas.Children.Add(line);
				}
			}
		}

		private void DrawMinorHorizontalTicks()
		{
			Size size = RenderSize;
			if (MinorHorizontalTicks != null && drawHorizontalMinorTicks)
			{
				double minY = 0;
				double maxY = size.Height;

				for (int i = 0; i < MinorHorizontalTicks.Length; i++)
				{
					double screenX = MinorHorizontalTicks[i].Tick;
					if (screenX < 0)
						continue;
					if (screenX > size.Width)
						continue;

					Line line = new Line
					{
						X1 = screenX,
						X2 = screenX,
						Y1 = minY,
						Y2 = maxY,
						Stroke = Brushes.LightGray,
						StrokeThickness = MinorHorizontalTicks[i].Value * gridBrushThickness
					};
					canvas.Children.Add(line);
				}
			}
		}

		#region IPlotterElement Members

		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			this.plotter = plotter;
			plotter.CentralGrid.Children.Add(this);
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			plotter.CentralGrid.Children.Remove(this);
			this.plotter = null;
		}

		private Plotter plotter;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Plotter Plotter
		{
			get { return plotter; }
		}

		#endregion
	}
}