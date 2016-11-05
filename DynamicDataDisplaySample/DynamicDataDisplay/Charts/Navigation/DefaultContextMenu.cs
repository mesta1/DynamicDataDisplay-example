using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;

namespace Microsoft.Research.DynamicDataDisplay.Navigation
{
	/// <summary>This class is responsible for displaying and populating default context menu of ChartPlotter</summary>
	public class DefaultContextMenu : IPlotterElement
	{
		private static readonly BitmapImage helpIcon;
		private static readonly BitmapImage copyScreenshotIcon;
		private static readonly BitmapImage saveScreenshotIcon;
		private static readonly BitmapImage fitToViewIcon;
		static DefaultContextMenu()
		{
			helpIcon = LoadIcon("HelpIcon");
			saveScreenshotIcon = LoadIcon("SaveIcon");
			copyScreenshotIcon = LoadIcon("CopyScreenshotIcon");
			fitToViewIcon = LoadIcon("FitToViewIcon");
		}

		private static BitmapImage LoadIcon(string name)
		{
			Assembly currentAssembly = typeof(DefaultContextMenu).Assembly;

			BitmapImage icon = new BitmapImage();
			icon.BeginInit();
			icon.StreamSource = currentAssembly.GetManifestResourceStream("Microsoft.Research.DynamicDataDisplay.Resources." + name + ".png");
			icon.EndInit();
			icon.Freeze();

			return icon;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultContextMenu"/> class.
		/// </summary>
		public DefaultContextMenu() { }

		protected ContextMenu PopulateContextMenu(IInputElement target)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem fitToViewMenuItem = new MenuItem
			{
				Header = Properties.Resources.ContextMenuFitToView,
				ToolTip = Properties.Resources.ContextMenuFitToViewTooltip,
				Icon = new Image { Source = fitToViewIcon },
				Command = ChartCommands.FitToView,
				CommandTarget = target
			};

			MenuItem savePictureMenuItem = new MenuItem
			{
				Header = Properties.Resources.ContextMenuSaveScreenshot,
				ToolTip = Properties.Resources.ContextMenuSaveScreenshotTooltip,
				Icon = new Image { Source = saveScreenshotIcon },
				Command = ChartCommands.SaveScreenshot,
				CommandTarget = target
			};

			MenuItem copyPictureMenuItem = new MenuItem
			{
				Header = Properties.Resources.ContextMenuCopyScreenshot,
				ToolTip = Properties.Resources.ContextMenuCopyScreenshotTooltip,
				Icon = new Image { Source = copyScreenshotIcon },
				Command = ChartCommands.CopyScreenshot,
				CommandTarget = target
			};

			MenuItem quickHelpMenuItem = new MenuItem
			{
				Header = Properties.Resources.ContextMenuQuickHelp,
				ToolTip = Properties.Resources.ContextMenuQuickHelpTooltip,
				Command = ChartCommands.ShowHelp,
				Icon = new Image { Source = helpIcon },
				CommandTarget = target
			};

			staticMenuItems.Add(fitToViewMenuItem);
			staticMenuItems.Add(copyPictureMenuItem);
			staticMenuItems.Add(savePictureMenuItem);
			staticMenuItems.Add(quickHelpMenuItem);

			menu.ItemsSource = staticMenuItems;

			return menu;
		}

		private readonly ObservableCollection<object> staticMenuItems = new ObservableCollection<object>();

		// hidden because default menu items' command target is plotter, and serializing this will
		// cause circular reference

		/// <summary>
		/// Gets the static menu items.
		/// </summary>
		/// <value>The static menu items.</value>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ObservableCollection<object> StaticMenuItems
		{
			get { return null; }
		}

		#region IPlotterElement Members

		private Plotter2D plotter;
		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			this.plotter = (Plotter2D)plotter;

			plotter.ContextMenu = PopulateContextMenu(plotter);
			plotter.PreviewMouseRightButtonDown += plotter_PreviewMouseRightButtonDown;
			plotter.PreviewMouseRightButtonUp += plotter_PreviewMouseRightButtonUp;

			plotter.ContextMenu.Closed += ContextMenu_Closed;
		}

		private void ContextMenu_Closed(object sender, RoutedEventArgs e)
		{
			foreach (var item in dynamicMenuItems)
			{
				staticMenuItems.Remove(item);
			}
		}

		private Point mousePos;
		private void plotter_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			mousePos = e.GetPosition(plotter);
		}

		private readonly ObservableCollection<object> dynamicMenuItems = new ObservableCollection<object>();
		private void plotter_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			Point position = e.GetPosition(plotter);
			if (mousePos == position)
			{
				hitResults.Clear();
				VisualTreeHelper.HitTest(plotter, null, CollectAllVisuals_Callback, new PointHitTestParameters(position));

				dynamicMenuItems.Clear();
				var dynamicItems = hitResults.Where(r =>
				{
					var items = GetPlotterContextMenu(r);
					return items != null && items.Count > 0;
				}).SelectMany(r => GetPlotterContextMenu(r));

				foreach (var item in dynamicItems)
				{
					dynamicMenuItems.Add(item);
				}

				staticMenuItems.AddMany(dynamicMenuItems);

				plotter.Focus();

				plotter.ContextMenu.IsOpen = true;
				e.Handled = true;
			}
		}

		#region PlotterContextMenu property

		/// <summary>
		/// Gets the plotter context menu.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public static ObjectCollection GetPlotterContextMenu(DependencyObject obj)
		{
			return (ObjectCollection)obj.GetValue(PlotterContextMenuProperty);
		}

		/// <summary>
		/// Sets the plotter context menu.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		public static void SetPlotterContextMenu(DependencyObject obj, ObjectCollection value)
		{
			obj.SetValue(PlotterContextMenuProperty, value);
		}

		/// <summary>
		/// Identifies the PlotterContextMenu attached property.
		/// </summary>
		public static readonly DependencyProperty PlotterContextMenuProperty = DependencyProperty.RegisterAttached(
		  "PlotterContextMenu",
		  typeof(ObjectCollection),
		  typeof(DefaultContextMenu),
		  new FrameworkPropertyMetadata(null));

		#endregion

		private List<DependencyObject> hitResults = new List<DependencyObject>();
		private HitTestResultBehavior CollectAllVisuals_Callback(HitTestResult result)
		{
			if (result == null || result.VisualHit == null)
				return HitTestResultBehavior.Stop;

			hitResults.Add(result.VisualHit);
			return HitTestResultBehavior.Continue;
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			plotter.ContextMenu.Closed -= ContextMenu_Closed;

			plotter.ContextMenu = null;
			plotter.PreviewMouseRightButtonDown -= plotter_PreviewMouseRightButtonDown;
			plotter.PreviewMouseRightButtonUp -= plotter_PreviewMouseRightButtonUp;

			this.plotter = null;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		Plotter IPlotterElement.Plotter
		{
			get { return plotter; }
		}

		#endregion
	}

	/// <summary>
	/// This is a collection to hold additional items ChartPlotter's context menu.
	/// </summary>
	public sealed class ObjectCollection : ObservableCollection<Object> { }
}
