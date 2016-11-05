using System;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.DataSources
{
	/// <summary>Data source that returns sequence of 2D points.</summary>
	public interface IPointDataSource
	{

		/// <summary>Returns object to enumerate points in source.</summary>
		IPointEnumerator GetEnumerator(DependencyObject context);

		/// <summary>This event is raised when contents of source are changed.</summary>
		event EventHandler DataChanged;
	}

	public class Context : DependencyObject
	{
		private readonly static Context emptyContext = new Context();

		public static Context EmptyContext
		{
			get { return emptyContext; }
		}

		public void ClearValues()
		{
			var localEnumerator = GetLocalValueEnumerator();
			localEnumerator.Reset();
			while (localEnumerator.MoveNext())
			{
				ClearValue(localEnumerator.Current.Property);
			}
		}

		public Rect VisibleRect
		{
			get { return (Rect)GetValue(VisibleRectProperty); }
			set { SetValue(VisibleRectProperty, value); }
		}

		public static readonly DependencyProperty VisibleRectProperty =
			DependencyProperty.Register(
			  "VisibleRect",
			  typeof(Rect),
			  typeof(Context),
			  new PropertyMetadata(new Rect()));

		public Rect Output
		{
			get { return (Rect)GetValue(OutputProperty); }
			set { SetValue(OutputProperty, value); }
		}

		public static readonly DependencyProperty OutputProperty =
			DependencyProperty.Register(
			  "Output",
			  typeof(Rect),
			  typeof(Context),
			  new PropertyMetadata(new Rect()));
	}
}
