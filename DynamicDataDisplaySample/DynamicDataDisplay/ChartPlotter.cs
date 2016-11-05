using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.Navigation;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>Chart plotter is a plotter that renders axis and grid</summary>
	public class ChartPlotter : Plotter2D
	{
		private IAxis horizontalAxis = new HorizontalAxis();
		private IAxis verticalAxis = new VerticalAxis();
		private AxisGrid axisGrid = new AxisGrid();

		private readonly Legend legend = new Legend();

		public ChartPlotter()
		{
			horizontalAxis.TicksChanged += OnHorizontalAxisTicksChanged;
			verticalAxis.TicksChanged += OnVerticalAxisTicksChanged;

			SetIsDefaultAxis(horizontalAxis as DependencyObject, true);
			SetIsDefaultAxis(verticalAxis as DependencyObject, true);

			mouseNavigation = new MouseNavigation();
			keyboardNavigation = new KeyboardNavigation();
			defaultContextMenu = new DefaultContextMenu();
			horizontalAxisNavigation = new AxisNavigation { Orientation = Orientation.Horizontal };
			verticalAxisNavigation = new AxisNavigation { Orientation = Orientation.Vertical };

			Children.AddMany(
				horizontalAxis,
				verticalAxis,
				axisGrid,
				mouseNavigation,
				keyboardNavigation,
				defaultContextMenu,
				horizontalAxisNavigation,
				verticalAxisNavigation,
				legend
				);

			SetAllChildrenAsDefault();
		}

		private ChartPlotter(bool empty) { }

		internal static ChartPlotter CreateEmpty()
		{
			return new ChartPlotter(true);
		}

		#region Default charts

		private MouseNavigation mouseNavigation;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MouseNavigation MouseNavigation
		{
			get { return mouseNavigation; }
		}

		private KeyboardNavigation keyboardNavigation;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public KeyboardNavigation KeyboardNavigation
		{
			get { return keyboardNavigation; }
		}

		private DefaultContextMenu defaultContextMenu;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DefaultContextMenu DefaultContextMenu
		{
			get { return defaultContextMenu; }
		}

		private AxisNavigation horizontalAxisNavigation;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AxisNavigation HorizontalAxisNavigation
		{
			get { return horizontalAxisNavigation; }
		}

		private AxisNavigation verticalAxisNavigation;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AxisNavigation VerticalAxisNavigation
		{
			get { return verticalAxisNavigation; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AxisGrid AxisGrid
		{
			get { return axisGrid; }
		}

		#endregion

		private void OnHorizontalAxisTicksChanged(object sender, EventArgs e)
		{
			IAxis axis = (IAxis)sender;
			UpdateHorizontalTicks(axis);
		}

		private void UpdateHorizontalTicks(IAxis axis)
		{
			axisGrid.BeginTicksUpdate();

			if (axis != null)
			{
				axisGrid.HorizontalTicks = axis.ScreenTicks;
				axisGrid.MinorHorizontalTicks = axis.MinorScreenTicks;
			}
			else
			{
				axisGrid.HorizontalTicks = null;
				axisGrid.MinorHorizontalTicks = null;
			}

			axisGrid.EndTicksUpdate();
		}

		private void OnVerticalAxisTicksChanged(object sender, EventArgs e)
		{
			IAxis axis = (IAxis)sender;
			UpdateVerticalTicks(axis);
		}

		private void UpdateVerticalTicks(IAxis axis)
		{
			axisGrid.BeginTicksUpdate();

			if (axis != null)
			{
				axisGrid.VerticalTicks = axis.ScreenTicks;
				axisGrid.MinorVerticalTicks = axis.MinorScreenTicks;
			}
			else
			{
				axisGrid.VerticalTicks = null;
				axisGrid.MinorVerticalTicks = null;
			}

			axisGrid.EndTicksUpdate();
		}

		bool keepOldAxis = false;
		bool updatingAxis = false;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IAxis VerticalAxis
		{
			get { return verticalAxis; }
			set
			{
				if (updatingAxis)
					return;

				if (value == null && verticalAxis != null)
				{
					if (!keepOldAxis)
					{
						Children.Remove(verticalAxis);
					}
					verticalAxis.TicksChanged -= OnVerticalAxisTicksChanged;
					verticalAxis = null;

					UpdateVerticalTicks(verticalAxis);

					return;
				}

				VerifyAxisType(value.Placement, AxisType.Vertical);

				if (value != verticalAxis)
				{
					ValidateVerticalAxis(value);

					updatingAxis = true;

					if (verticalAxis != null)
					{
						verticalAxis.TicksChanged -= OnVerticalAxisTicksChanged;
						SetIsDefaultAxis(verticalAxis as DependencyObject, false);
						if (!keepOldAxis)
						{
							Children.Remove(verticalAxis);
						}
					}
					SetIsDefaultAxis(value as DependencyObject, true);

					verticalAxis = value;
					verticalAxis.TicksChanged += OnVerticalAxisTicksChanged;

					if (!Children.Contains(value))
					{
						Children.Add(value);
					}

					UpdateVerticalTicks(value);
					OnVerticalAxisChanged();

					updatingAxis = false;
				}
			}
		}

		protected virtual void OnVerticalAxisChanged() { }
		protected virtual void ValidateVerticalAxis(IAxis axis) { }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IAxis HorizontalAxis
		{
			get { return horizontalAxis; }
			set
			{
				if (updatingAxis)
					return;

				if (value == null && horizontalAxis != null)
				{
					Children.Remove(horizontalAxis);
					horizontalAxis.TicksChanged -= OnHorizontalAxisTicksChanged;
					horizontalAxis = null;

					UpdateHorizontalTicks(horizontalAxis);

					return;
				}

				VerifyAxisType(value.Placement, AxisType.Horizontal);

				if (value != horizontalAxis)
				{
					ValidateHorizontalAxis(value);

					updatingAxis = true;

					if (horizontalAxis != null)
					{
						horizontalAxis.TicksChanged -= OnHorizontalAxisTicksChanged;
						SetIsDefaultAxis(horizontalAxis as DependencyObject, false);
						if (!keepOldAxis)
						{
							Children.Remove(horizontalAxis);
						}
					}
					SetIsDefaultAxis(value as DependencyObject, true);

					horizontalAxis = value;
					horizontalAxis.TicksChanged += OnHorizontalAxisTicksChanged;

					if (!Children.Contains(value))
					{
						Children.Add(value);
					}

					UpdateHorizontalTicks(value);
					OnHorizontalAxisChanged();

					updatingAxis = false;
				}
			}
		}

		protected virtual void OnHorizontalAxisChanged() { }
		protected virtual void ValidateHorizontalAxis(IAxis axis) { }

		private static void VerifyAxisType(AxisPlacement axisPlacement, AxisType axisType)
		{
			bool result = false;
			switch (axisPlacement)
			{
				case AxisPlacement.Left:
				case AxisPlacement.Right:
					result = axisType == AxisType.Vertical;
					break;
				case AxisPlacement.Top:
				case AxisPlacement.Bottom:
					result = axisType == AxisType.Horizontal;
					break;
				default:
					break;
			}

			if (!result)
				throw new ArgumentException(Properties.Resources.InvalidAxisPlacement);
		}

		protected override void OnIsDefaultAxisChangedCore(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			IAxis axis = d as IAxis;
			if (axis != null)
			{
				bool value = (bool)e.NewValue;
				bool oldKeepOldAxis = keepOldAxis;

				bool horizontal = axis.Placement == AxisPlacement.Bottom || axis.Placement == AxisPlacement.Top;
				keepOldAxis = true;

				if (value && horizontal)
				{
					HorizontalAxis = axis;
				}
				else if (value && !horizontal)
				{
					VerticalAxis = axis;
				}
				else if (!value && horizontal)
				{
					HorizontalAxis = null;
				}
				else if (!value && !horizontal)
				{
					VerticalAxis = null;
				}

				keepOldAxis = oldKeepOldAxis;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Legend Legend
		{
			get { return legend; }
		}

		public bool LegendVisible
		{
			get { return legend.Visibility == Visibility.Visible; }
			set { legend.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
		}

		private enum AxisType
		{
			Horizontal,
			Vertical
		}
	}
}