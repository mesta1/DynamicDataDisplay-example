using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.ViewportRestrictions
{
	public abstract class ViewportRestrictionBase : IViewportRestriction
	{
		#region IViewportRestriction Members

		private bool isActive = true;
		public bool IsActive
		{
			get { return isActive; }
			set
			{
				if (isActive != value)
				{
					isActive = value;
					RaiseChanged();
				}
			}
		}

		public abstract Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport);

		protected void RaiseChanged()
		{
			Changed.Raise(this);
		}
		public event EventHandler Changed;

		#endregion
	}
}
