using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using Microsoft.Research.DynamicDataDisplay;

namespace Microsoft.Research.DynamicDataDisplay
{
	public class ExtendedPropertyChangedEventArgs : EventArgs
	{
		public string PropertyName { get; set; }
		public object OldValue { get; set; }
		public object NewValue { get; set; }

		public static ExtendedPropertyChangedEventArgs FromDependencyPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			return new ExtendedPropertyChangedEventArgs { PropertyName = e.Property.Name, NewValue = e.NewValue, OldValue = e.OldValue };
		}
	}

	public interface IViewport2D
	{
		Rect Visible { get; set; }
		Rect Output { get; }
		CoordinateTransform Transform { get; set; }
        void FitToView();

		event EventHandler<ExtendedPropertyChangedEventArgs> PropertyChanged;
	}
}
