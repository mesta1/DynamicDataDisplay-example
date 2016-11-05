using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using System.Diagnostics;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common.UndoSystem;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>Viewport2D is a specialized version of canvas providing additional
	/// virtual coordinates</summary>
	public class Viewport2D : Canvas, IViewport2D, IPlotterElement
	{
		public Viewport2D()
		{
			ClipToBounds = true;
			UpdateTransform();

			Grid.SetColumn(this, 1);
			Grid.SetRow(this, 1);

			restrictions.CollectionChanged += restrictions_CollectionChanged;
		}

		private void restrictions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CoerceValue(VisibleProperty);
		}

		#region Viewport changed event

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			// todo добавить сюда еще свойств.
			if (e.Property == ActualHeightProperty ||
				e.Property == ActualWidthProperty)
			{
				SetValue(OutputPropertyKey, new Rect(new Size(ActualWidth, ActualHeight)));
				// todo uncomment or create more sophysticated logic
				CoerceValue(VisibleProperty);
			}
			else if (e.Property == OutputProperty || e.Property == VisibleProperty)
			{
				UpdateTransform();
				RaisePropertyChangedEvent(e);
			}
		}

		#endregion

		public void FitToView()
		{
			ClearValue(VisibleProperty);
			CoerceValue(VisibleProperty);
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsFittedToView
		{
			get { return AutoFitToView && ReadLocalValue(VisibleProperty) == DependencyProperty.UnsetValue; }
		}

		public void UpdateVisible()
		{
			if (IsFittedToView)
			{
				CoerceValue(VisibleProperty);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Plotter2D Plotter
		{
			get { return plotter; }
		}

		private readonly RestrictionCollection restrictions = new RestrictionCollection
		{
			new MinimalSizeRestriction()
		};

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RestrictionCollection Restrictions
		{
			get { return restrictions; }
		}

		#region Output property

		public Rect Output
		{
			get { return (Rect)GetValue(OutputProperty); }
		}

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly DependencyPropertyKey OutputPropertyKey = DependencyProperty.RegisterReadOnly(
			"Output",
			typeof(Rect),
			typeof(Viewport2D),
			new PropertyMetadata());

		public static readonly DependencyProperty OutputProperty = OutputPropertyKey.DependencyProperty;

		#endregion

		#region Visible property

		public Rect Visible
		{
			get { return (Rect)GetValue(VisibleProperty); }
			set { SetValue(VisibleProperty, value); }
		}

		public static readonly DependencyProperty VisibleProperty =
			DependencyProperty.Register("Visible", typeof(Rect), typeof(Viewport2D),
			new FrameworkPropertyMetadata(
				new Rect(0, 0, 1, 1),
				OnVisibleChanged,
				OnCoerceVisible));

		private Rect CoerceVisible(Rect newVisible)
		{
			if (Plotter == null)
			{
				return newVisible;
			}

			bool isDefaultValue = newVisible == (Rect)VisibleProperty.DefaultMetadata.DefaultValue;
			if (isDefaultValue)
			{
				newVisible = Rect.Empty;
			}

			if (isDefaultValue && IsFittedToView)
			{
				Rect bounds = Rect.Empty;
				foreach (var g in Plotter.Children)
				{
					var graph = g as DependencyObject;
					if (graph != null)
					{
						var uiElement = g as UIElement;
						if (uiElement == null || (uiElement != null && uiElement.Visibility == Visibility.Visible))
						{
							bounds.Union((Rect)graph.GetValue(ViewportElement2D.ContentBoundsProperty));
						}
					}
				}
				Rect viewportBounds = bounds;
				if (!bounds.IsEmpty)
				{
					bounds = bounds.DataToViewport(transform);
				}

				//Rect defaultRect = (Rect)VisibleProperty.DefaultMetadata.DefaultValue;
				//if (bounds.Width.IsInfinite())
				//    bounds.Width = defaultRect.Width;
				//if (bounds.Height.IsInfinite())
				//    bounds.Height = defaultRect.Height;
				//if (bounds.X.IsInfinite())
				//    bounds.X = defaultRect.X;
				//if (bounds.Y.IsInfinite())
				//    bounds.Y = defaultRect.Y;

				if (!bounds.IsEmpty)
				{
					bounds = CoordinateUtilities.RectZoom(bounds, bounds.GetCenter(), clipToBoundsFactor);
				}
				else
				{
					bounds = (Rect)VisibleProperty.DefaultMetadata.DefaultValue;
				}
				newVisible.Union(bounds);
			}

			if (newVisible.IsEmpty)
			{
				newVisible = (Rect)VisibleProperty.DefaultMetadata.DefaultValue;
			}
			else if (newVisible.Width == 0 || newVisible.Height == 0)
			{
				Rect defRect = (Rect)VisibleProperty.DefaultMetadata.DefaultValue;
				Size size = newVisible.Size;
				Point loc = newVisible.Location;

				if (newVisible.Width == 0)
				{
					size.Width = defRect.Width;
					loc.X -= size.Width / 2;
				}
				if (newVisible.Height == 0)
				{
					size.Height = defRect.Height;
					loc.Y -= size.Height / 2;
				}

				newVisible = new Rect(loc, size);
			}

			newVisible = ApplyRestrictions(Visible, newVisible);

			// applying transform's data domain restriction
			if (!transform.DataTransform.DataDomain.IsEmpty)
			{
				var newDataRect = newVisible.ViewportToData(transform);
				newDataRect = Rect.Intersect(newDataRect, transform.DataTransform.DataDomain);
				newVisible = newDataRect.DataToViewport(transform);
			}

			if (newVisible.IsEmpty) return new Rect(0, 0, 1, 1);

			return newVisible;
		}

		private Rect ApplyRestrictions(Rect oldVisible, Rect newVisible)
		{
			Rect res = newVisible;
			foreach (var restriction in restrictions)
			{
				res = restriction.Apply(oldVisible, res, this);
			}
			return res;
		}

		private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		protected virtual void OnVisibleChanged()
		{
		}

		private static object OnCoerceVisible(DependencyObject d, object newValue)
		{
			Viewport2D viewport = (Viewport2D)d;

			Rect newRect = viewport.CoerceVisible((Rect)newValue);

			if (newRect.Width == 0 || newRect.Height == 0)
			{
				// doesn't apply rects with zero square
				return DependencyProperty.UnsetValue;
			}
			else
			{
				return newRect;
			}
		}

		#endregion

		private double clipToBoundsFactor = 1.10;
		/// <summary>
		/// Gets or sets the clip to bounds factor.
		/// </summary>
		/// <value>The clip to bounds factor.</value>
		public double ClipToBoundsFactor
		{
			get { return clipToBoundsFactor; }
			set
			{
				if (clipToBoundsFactor != value)
				{
					clipToBoundsFactor = value;
					UpdateVisible();
				}
			}
		}

		private bool autoFitToView = true;

		/// <summary>Gets or sets a value indicating whether viewport automatically clips 
		/// in its initial visible rect to bounds of graphs.</summary>
		[DefaultValue(true)]
		public bool AutoFitToView
		{
			get { return autoFitToView; }
			set
			{
				if (autoFitToView != value)
				{
					autoFitToView = value;
					if (value)
					{
						UpdateVisible();
					}
				}
			}
		}

		private void UpdateTransform()
		{
			transform = transform.WithRects(Visible, Output);
		}

		private CoordinateTransform transform = CoordinateTransform.CreateDefault();
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[NotNull]
		public CoordinateTransform Transform
		{
			get { return transform; }
			set
			{
				value.VerifyNotNull();

				if (value != transform)
				{
					var oldTransform = transform;

					transform = value;

					RaisePropertyChangedEvent("Transform", oldTransform, transform);
				}
			}
		}

		public event EventHandler<ExtendedPropertyChangedEventArgs> PropertyChanged;

		private void RaisePropertyChangedEvent(string propertyName, object oldValue, object newValue)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new ExtendedPropertyChangedEventArgs { PropertyName = propertyName, OldValue = oldValue, NewValue = newValue });
			}
		}

		private void RaisePropertyChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new ExtendedPropertyChangedEventArgs { PropertyName = propertyName });
			}
		}

		private void RaisePropertyChangedEvent(DependencyPropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, ExtendedPropertyChangedEventArgs.FromDependencyPropertyChanged(e));
			}
		}

		#region IPlotterElement Members

		protected virtual Panel GetHostPanel(Plotter plotter)
		{
			return plotter.MainGrid;
		}

		private Plotter2D plotter;
		void IPlotterElement.OnPlotterAttached(Plotter plotter)
		{
			plotter.Children.CollectionChanged += OnPlotterChildrenChanged;
			GetHostPanel(plotter).Children.Add(this);

			this.plotter = (Plotter2D)plotter;

			UpdateVisible();
		}

		private void OnPlotterChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateVisible();
		}

		void IPlotterElement.OnPlotterDetaching(Plotter plotter)
		{
			plotter.Children.CollectionChanged -= OnPlotterChildrenChanged;
			GetHostPanel(plotter).Children.Remove(this);

			this.plotter = null;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		Plotter IPlotterElement.Plotter { get { return plotter; } }

		#endregion
	}
}
