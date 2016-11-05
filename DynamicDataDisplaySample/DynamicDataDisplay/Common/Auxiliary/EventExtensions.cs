using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Microsoft.Research.DynamicDataDisplay
{
	public static class EventExtensions
	{
		public static void Raise<T>(this EventHandler<T> @event, object sender, T args) where T : EventArgs
		{
			if (@event != null)
			{
				@event(sender, args);
			}
		}

		public static void Raise(this EventHandler @event, object sender)
		{
			if (@event != null)
			{
				@event(sender, EventArgs.Empty);
			}
		}

		public static void Raise(this EventHandler @event, object sender, EventArgs args)
		{
			if (@event != null)
			{
				@event(sender, args);
			}
		}

		public static void Raise(this PropertyChangedEventHandler @event, object sender, string propertyName)
		{
			if (@event != null)
			{
				@event(sender, new PropertyChangedEventArgs(propertyName));
			}
		}

		public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender)
		{
			if (@event != null)
			{
				@event(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender, NotifyCollectionChangedAction action)
		{
			if (@event != null)
			{
				@event(sender, new NotifyCollectionChangedEventArgs(action));
			}
		}

		public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			if (@event != null)
			{
				@event(sender, e);
			}
		}
	}
}
