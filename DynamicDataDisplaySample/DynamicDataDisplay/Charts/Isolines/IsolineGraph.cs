using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay.Charts.Isolines;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	/// <summary>
	/// Draws isolines on given two-dimensional scalar data.
	/// </summary>
	public sealed class IsolineGraph : IsolineGraphBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IsolineGraph"/> class.
		/// </summary>
		public IsolineGraph()
		{
			Content = content;
		}

		private readonly Canvas content = new Canvas();

		protected override void UpdateDataSource()
		{
			base.UpdateDataSource();

			CreateUIRepresentation();
		}

		protected override void UpdateLineThickness()
		{
			foreach (var path in linePaths)
			{
				path.StrokeThickness = StrokeThickness;
			}
		}

		private List<TextBlock> textBlocks = new List<TextBlock>();
		private List<Path> linePaths = new List<Path>();
		protected override void CreateUIRepresentation()
		{
			if (Plotter2D == null)
				return;

			content.Children.Clear();
			linePaths.Clear();

			if (Collection != null)
			{
				foreach (var line in Collection.Lines)
				{
					Path path = new Path
					{
						Stroke = new SolidColorBrush(Palette.GetColor(line.Value01)),
						StrokeThickness = StrokeThickness,
						Data = CreateGeometry(line),
						Tag = line
					};
					content.Children.Add(path);
					linePaths.Add(path);
				}

				if (DrawLabels)
				{
					var transform = Plotter2D.Viewport.Transform;
					double wayBeforeText = new Rect(new Size(2000, 2000)).ScreenToData(transform).Width;
					Annotater.WayBeforeText = wayBeforeText;
					var textLabels = Annotater.Annotate(Collection, Plotter2D.Viewport.Visible);

					foreach (var textLabel in textLabels)
					{
						TextBlock text = CreateTextLabel(textLabel);
						content.Children.Add(text);
						textBlocks.Add(text);
					}
				}
			}
		}

		private TextBlock CreateTextLabel(IsolineTextLabel textLabel)
		{
			var transform = Plotter2D.Viewport.Transform;
			Point screenPos = textLabel.Position.DataToScreen(transform);

			double angle = textLabel.Rotation;
			Debug.WriteLine(angle);
			if (angle < 0)
				angle += 360;
			if (135 < angle && angle < 225)
				angle -= 180;

			TextBlock res = new TextBlock
			{
				Text = textLabel.Text,
				RenderTransform = new RotateTransform(angle),
				Tag = textLabel,
				RenderTransformOrigin = new Point(0.5, 0.5)
			};

			res.Measure(SizeHelper.CreateInfiniteSize());

			Size textSize = res.DesiredSize;
			Point position = new Point(screenPos.X - textSize.Width / 2, screenPos.Y - textSize.Height / 2);

			Canvas.SetLeft(res, position.X);
			Canvas.SetTop(res, position.Y);
			return res;
		}

		private Geometry CreateGeometry(LevelLine lineData)
		{
			var transform = Plotter2D.Viewport.Transform;

			StreamGeometry geometry = new StreamGeometry();
			using (var context = geometry.Open())
			{
				context.BeginFigure(lineData.StartPoint.DataToScreen(transform), false, false);
				context.PolyLineTo(lineData.OtherPoints.DataToScreen(transform), true, true);
			}
			geometry.Freeze();
			return geometry;
		}

		private bool rebuildText = false;
		protected override void OnViewportPropertyChanged(ExtendedPropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Visible" || e.PropertyName == "Output")
			{
				Rect prevRect = (Rect)e.OldValue;
				Rect currRect = (Rect)e.NewValue;

				// completely rebuild text only if width and height have changed many
				const double smallChangePercent = 0.05;
				bool widthChangedLittle = Math.Abs(currRect.Width - prevRect.Width) / currRect.Width < smallChangePercent;
				bool heightChangedLittle = Math.Abs(currRect.Height - prevRect.Height) / currRect.Height < smallChangePercent;

				rebuildText = !(widthChangedLittle && heightChangedLittle);
			}
			UpdateUIRepresentation();
		}

		private void UpdateUIRepresentation()
		{
			foreach (var path in linePaths)
			{
				LevelLine line = (LevelLine)path.Tag;
				path.Data = CreateGeometry(line);
			}

			var transform = Plotter2D.Viewport.Transform;
			Rect output = Plotter2D.Viewport.Output;
			Rect visible = Plotter2D.Viewport.Visible;

			if (rebuildText)
			{
				foreach (var text in textBlocks)
				{
					if (text.Visibility == Visibility.Visible)
						content.Children.Remove(text);
				}
				textBlocks.Clear();

				double wayBeforeText = new Rect(new Size(100, 100)).ScreenToData(transform).Width;
				Annotater.WayBeforeText = wayBeforeText;
				var textLabels = Annotater.Annotate(Collection, Plotter2D.Viewport.Visible);
				foreach (var textLabel in textLabels)
				{
					TextBlock text = CreateTextLabel(textLabel);
					textBlocks.Add(text);
					if (visible.Contains(textLabel.Position))
					{
						content.Children.Add(text);
					}
					else
					{
						text.Visibility = Visibility.Hidden;
					}
				}
			}
			else
			{
				foreach (var text in textBlocks)
				{
					IsolineTextLabel label = (IsolineTextLabel)text.Tag;
					Point screenPos = label.Position.DataToScreen(transform);
					Size textSize = text.DesiredSize;

					Point position = new Point(screenPos.X - textSize.Width / 2, screenPos.Y - textSize.Height / 2);

					if (output.Contains(position))
					{
						Canvas.SetLeft(text, position.X);
						Canvas.SetTop(text, position.Y);
						if (text.Visibility == Visibility.Hidden)
						{
							text.Visibility = Visibility.Visible;
							content.Children.Add(text);
						}
					}
					else if (text.Visibility == Visibility.Visible)
					{
						text.Visibility = Visibility.Hidden;
						content.Children.Remove(text);
					}
				}
			}
		}
	}
}
