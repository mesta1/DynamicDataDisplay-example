using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Microsoft.Research.DynamicDataDisplay.Common.UndoSystem;
using Microsoft.Research.DynamicDataDisplay.Navigation;
using System.Windows.Data;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>Plotter is a base control for displaying various graphs. It provides
	/// means to draw chart itself and side space for axes, annotations, etc</summary>
	[ContentProperty("Children")]
	[TemplatePart(Name = "PART_HeaderPanel", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_FooterPanel", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_BottomPanel", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_LeftPanel", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_RightPanel", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_TopPanel", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_MainCanvas", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_CentralGrid", Type = typeof(Grid))]
	[TemplatePart(Name = "PART_MainGrid", Type = typeof(Grid))]
	[TemplatePart(Name = "PART_ContentsGrid", Type = typeof(Grid))]
	[TemplatePart(Name = "PART_ParallelCanvas", Type = typeof(Canvas))]
	public abstract class Plotter : ContentControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Plotter"/> class.
		/// </summary>
		protected Plotter()
		{
			UpdateUIParts();

			children.CollectionChanged += OnChildrenCollectionChanged;
			Loaded += Plotter_Loaded;

			ContextMenu = null;
		}

		public override bool ShouldSerializeContent()
		{
			return false;
		}

		protected override bool ShouldSerializeProperty(DependencyProperty dp)
		{
			// do not serialize context menu if it was created by DefaultContextMenu, because that context menu items contains references of plotter
			if (dp == ContextMenuProperty && children.Any(el => el is DefaultContextMenu)) return false;
			if (dp == TemplateProperty) return false;
			if (dp == ContentProperty) return false;

			return base.ShouldSerializeProperty(dp);
		}

		private const string templateKey = "defaultPlotterTemplate";
		private const string styleKey = "defaultPlotterStyle";
		private void UpdateUIParts()
		{
			ResourceDictionary dict = new ResourceDictionary
			{
				Source = new Uri("/DynamicDataDisplay;component/Common/PlotterStyle.xaml", UriKind.Relative)
			};

			Style = (Style)dict[styleKey];

			ControlTemplate template = (ControlTemplate)dict[templateKey];
			Template = template;
			ApplyTemplate();
		}

		private void Plotter_Loaded(object sender, RoutedEventArgs e)
		{
			// this is done to enable keyboard shortcuts
			Focus();

			OnLoaded();
		}

		protected virtual void OnLoaded() { }


		private NotifyingGrid contentsGrid;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			headerPanel = GetPart<NotifyingStackPanel>("PART_HeaderPanel");
			footerPanel = GetPart<NotifyingStackPanel>("PART_FooterPanel");

			leftPanel = GetPart<NotifyingStackPanel>("PART_LeftPanel");
			bottomPanel = GetPart<NotifyingStackPanel>("PART_BottomPanel");
			rightPanel = GetPart<NotifyingStackPanel>("PART_RightPanel");
			topPanel = GetPart<NotifyingStackPanel>("PART_TopPanel");

			mainCanvas = GetPart<NotifyingCanvas>("PART_MainCanvas");
			centralGrid = GetPart<NotifyingGrid>("PART_CentralGrid");
			mainGrid = GetPart<NotifyingGrid>("PART_MainGrid");
			parallelCanvas = GetPart<NotifyingCanvas>("PART_ParallelCanvas");

			contentsGrid = GetPart<NotifyingGrid>("PART_ContentsGrid");
			Content = contentsGrid;
			AddLogicalChild(contentsGrid);

			foreach (var notifyingItem in GetAllPanels())
			{
				if (notifyingItem.NotifyingChildren == null)
				{
					notifyingItem.ChildrenCreated += notifyingItem_ChildrenCreated;
				}
				else
				{
					notifyingItem.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
				}
			}
		}

		private void notifyingItem_ChildrenCreated(object sender, EventArgs e)
		{
			INotifyingPanel panel = (INotifyingPanel)sender;

			SubscribePanelEvents(panel);
		}

		private void SubscribePanelEvents(INotifyingPanel panel)
		{
			panel.ChildrenCreated -= notifyingItem_ChildrenCreated;

			panel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
			panel.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
		}

		private void OnVisualCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					INotifyingPanel notifyingPanel = item as INotifyingPanel;
					if (notifyingPanel != null)
					{
						if (notifyingPanel.NotifyingChildren != null)
						{
							notifyingPanel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
							notifyingPanel.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
						}
						else
						{
							notifyingPanel.ChildrenCreated += notifyingItem_ChildrenCreated;
						}
					}

					OnVisualChildAdded((UIElement)item, (UIElementCollection)sender);
				}
			}
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					INotifyingPanel notifyingPanel = item as INotifyingPanel;
					if (notifyingPanel != null)
					{
						notifyingPanel.ChildrenCreated -= notifyingItem_ChildrenCreated;
						if (notifyingPanel.NotifyingChildren != null)
						{
							notifyingPanel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
						}
					}

					OnVisualChildRemoved((UIElement)item, (UIElementCollection)sender);
				}
			}
		}

		private readonly VisualBindingCollection visualBindingCollection = new VisualBindingCollection();
		public VisualBindingCollection VisualBindings
		{
			get { return visualBindingCollection; }
		}

		protected virtual void OnVisualChildAdded(UIElement target, UIElementCollection uIElementCollection)
		{
			IPlotterElement element = null;
			if (addingElements.Count > 0)
			{
				element = addingElements.Peek();

				var dict = visualBindingCollection.Cache;
				var proxy = dict[element];

				List<UIElement> visualElements;
				if (!addedVisualElements.ContainsKey(element))
				{
					visualElements = new List<UIElement>();
					addedVisualElements.Add(element, visualElements);
				}
				else
				{
					visualElements = addedVisualElements[element];
				}

				visualElements.Add(target);

				SetBindings(proxy, target);
			}
		}

		private void SetBindings(UIElement proxy, UIElement target)
		{
			if (proxy != target)
			{
				foreach (var property in GetPropertiesToSetBindingOn())
				{
					BindingOperations.SetBinding(target, property, new Binding { Path = new PropertyPath(property.Name), Source = proxy, Mode = BindingMode.TwoWay });
				}
			}
		}

		private void RemoveBindings(UIElement proxy, UIElement target)
		{
			if (proxy != target)
			{
				foreach (var property in GetPropertiesToSetBindingOn())
				{
					BindingOperations.ClearBinding(target, property);
				}
			}
		}

		private IEnumerable<DependencyProperty> GetPropertiesToSetBindingOn()
		{
			yield return UIElement.OpacityProperty;
			yield return UIElement.VisibilityProperty;
			yield return UIElement.IsHitTestVisibleProperty;
		}

		protected virtual void OnVisualChildRemoved(UIElement target, UIElementCollection uiElementCollection)
		{
			IPlotterElement element = null;
			if (removingElements.Count > 0)
			{
				element = removingElements.Peek();

				var dict = visualBindingCollection.Cache;
				var proxy = dict[element];

				if (addedVisualElements.ContainsKey(element))
				{
					var list = addedVisualElements[element];
					list.Remove(target);

					if (list.Count == 0)
					{
						dict.Remove(element);
					}
				}

				RemoveBindings(proxy, target);
			}
		}

		internal virtual IEnumerable<INotifyingPanel> GetAllPanels()
		{
			yield return headerPanel;
			yield return footerPanel;

			yield return leftPanel;
			yield return bottomPanel;
			yield return rightPanel;
			yield return topPanel;

			yield return mainCanvas;
			yield return centralGrid;
			yield return mainGrid;
			yield return parallelCanvas;
			yield return contentsGrid;
		}

		private T GetPart<T>(string name)
		{
			return (T)Template.FindName(name, this);
		}

		#region Children and add/removed events handling

		private readonly ChildrenCollection children = new ChildrenCollection();

		/// <summary>
		/// Provides access to Plotter's children charts.
		/// </summary>
		/// <value>The children.</value>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ChildrenCollection Children
		{
			get { return children; }
		}

		private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (IPlotterElement item in e.NewItems)
				{
					OnChildAdded(item);
				}
			}
			if (e.OldItems != null)
			{
				foreach (IPlotterElement item in e.OldItems)
				{
					OnChildRemoving(item);
				}
			}
		}

		private readonly Stack<IPlotterElement> addingElements = new Stack<IPlotterElement>();
		protected virtual void OnChildAdded(IPlotterElement child)
		{
			if (child != null)
			{
				addingElements.Push(child);
				try
				{
					UIElement visualProxy = CreateVisualProxy(child);
					visualBindingCollection.Cache.Add(child, visualProxy);

					if (child.Plotter != null)
					{
						throw new InvalidOperationException(Properties.Resources.PlotterElementAddedToAnotherPlotter);
					}

					child.OnPlotterAttached(this);

					if (child.Plotter != this)
					{
						throw new InvalidOperationException(Properties.Resources.InvalidParentPlotterValue);
					}
				}
				finally
				{
					addingElements.Pop();
				}
			}
		}

		private UIElement CreateVisualProxy(IPlotterElement child)
		{
			if (visualBindingCollection.Cache.ContainsKey(child))
				throw new InvalidOperationException(Properties.Resources.VisualBindingsWrongState);

			UIElement result = child as UIElement;

			if (result == null)
			{
				result = new UIElement();
			}

			return result;
		}

		private readonly Stack<IPlotterElement> removingElements = new Stack<IPlotterElement>();
		protected virtual void OnChildRemoving(IPlotterElement child)
		{
			if (child != null)
			{
				removingElements.Push(child);
				try
				{
					if (child.Plotter != this)
					{
						throw new InvalidOperationException(Properties.Resources.InvalidParentPlotterValueRemoving);
					}

					child.OnPlotterDetaching(this);

					if (child.Plotter != null)
					{
						throw new InvalidOperationException(Properties.Resources.ParentPlotterNotNull);
					}

					visualBindingCollection.Cache.Remove(child);

					if (addedVisualElements.ContainsKey(child) && addedVisualElements[child].Count > 0)
					{
						throw new InvalidOperationException(String.Format(Properties.Resources.PlotterElementDidnotCleanedAfterItself, child.ToString()));
					}
				}
				finally
				{
					removingElements.Pop();
				}
			}
		}

		private readonly Dictionary<IPlotterElement, List<UIElement>> addedVisualElements = new Dictionary<IPlotterElement, List<UIElement>>();

		#endregion

		#region Layout zones

		private NotifyingCanvas parallelCanvas;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Canvas ParallelCanvas
		{
			get { return parallelCanvas; }
		}

		private NotifyingStackPanel headerPanel;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StackPanel HeaderPanel
		{
			get { return headerPanel; }
		}

		private NotifyingStackPanel footerPanel;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StackPanel FooterPanel
		{
			get { return footerPanel; }
		}

		private NotifyingStackPanel leftPanel;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StackPanel LeftPanel
		{
			get { return leftPanel; }
		}

		private NotifyingStackPanel rightPanel;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StackPanel RightPanel
		{
			get { return rightPanel; }
		}

		private NotifyingStackPanel topPanel;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StackPanel TopPanel
		{
			get { return topPanel; }
		}

		private NotifyingStackPanel bottomPanel;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StackPanel BottomPanel
		{
			get { return bottomPanel; }
		}

		private NotifyingCanvas mainCanvas;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Canvas MainCanvas
		{
			get { return mainCanvas; }
		}

		private NotifyingGrid centralGrid;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Grid CentralGrid
		{
			get { return centralGrid; }
		}

		private NotifyingGrid mainGrid;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Grid MainGrid
		{
			get { return mainGrid; }
		}

		#endregion

		#region Screenshots & copy to clipboard

		public BitmapSource CreateScreenshot()
		{

			UIElement parent = (UIElement)Parent;

			Rect renderBounds = new Rect(RenderSize);

			Point p1 = renderBounds.TopLeft;
			Point p2 = renderBounds.BottomRight;

			if (parent != null)
			{
				p1 = TranslatePoint(p1, parent);
				p2 = TranslatePoint(p2, parent);
			}

			Int32Rect rect = new Rect(p1, p2).ToInt32Rect();

			return ScreenshotHelper.CreateScreenshot(this, rect);
		}


		/// <summary>Saves screenshot to file.</summary>
		/// <param name="filePath">File path.</param>
		public void SaveScreenshot(string filePath)
		{
			ScreenshotHelper.SaveBitmapToFile(CreateScreenshot(), filePath);
		}

		/// <summary>
		/// Saves screenshot to stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="fileExtension">The file type extension.</param>
		public void SaveScreenshotToStream(Stream stream, string fileExtension)
		{
			ScreenshotHelper.SaveBitmapToStream(CreateScreenshot(), stream, fileExtension);
		}

		/// <summary>Copies the screenshot to clipboard.</summary>
		public void CopyScreenshotToClipboard()
		{
			Clipboard.Clear();
			Clipboard.SetImage(CreateScreenshot());
		}

		#endregion

		#region IsDefaultElement attached property

		protected void SetAllChildrenAsDefault()
		{
			foreach (var child in Children.OfType<DependencyObject>())
			{
				child.SetValue(IsDefaultElementProperty, true);
			}
		}

		/// <summary>Gets a value whether specified graphics object is default to this plotter or not</summary>
		/// <param name="obj">Graphics object to check</param>
		/// <returns>True if it is default or false otherwise</returns>
		public static bool GetIsDefaultElement(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDefaultElementProperty);
		}

		public static void SetIsDefaultElement(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDefaultElementProperty, value);
		}

		public static readonly DependencyProperty IsDefaultElementProperty = DependencyProperty.RegisterAttached(
			"IsDefaultElement",
			typeof(bool),
			typeof(Plotter),
			new UIPropertyMetadata(false));

		/// <summary>Removes all user graphs from given UIElementCollection, 
		/// leaving only default graphs</summary>
		protected static void RemoveUserElements(IList<IPlotterElement> elements)
		{
			int index = 0;

			while (index < elements.Count)
			{
				DependencyObject d = elements[index] as DependencyObject;
				if (d != null && !GetIsDefaultElement(d))
				{
					elements.RemoveAt(index);
				}
				else
				{
					index++;
				}
			}
		}

		public void RemoveUserElements()
		{
			RemoveUserElements(Children);
		}

		#endregion

		#region IsDefaultAxis

		public static bool GetIsDefaultAxis(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDefaultAxisProperty);
		}

		public static void SetIsDefaultAxis(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDefaultAxisProperty, value);
		}

		public static readonly DependencyProperty IsDefaultAxisProperty = DependencyProperty.RegisterAttached(
			"IsDefaultAxis",
			typeof(bool),
			typeof(Plotter),
			new UIPropertyMetadata(false, OnIsDefaultAxisChanged));

		private static void OnIsDefaultAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Plotter parentPlotter = null;
			IPlotterElement plotterElement = d as IPlotterElement;
			if (plotterElement != null)
			{
				parentPlotter = plotterElement.Plotter;

				if (parentPlotter != null)
				{
					parentPlotter.OnIsDefaultAxisChangedCore(d, e);
				}
			}
		}

		protected virtual void OnIsDefaultAxisChangedCore(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

		#endregion

		#region Undo

		private readonly UndoProvider undoProvider = new UndoProvider();
		public UndoProvider UndoProvider
		{
			get { return undoProvider; }
		}

		#endregion
	}
}
