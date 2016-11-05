using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.ViewportRestrictions
{
	public class BorderRestriction : ViewportRestrictionBase
	{
		Rect border = new Rect(-1, -1, 200, 100);
		public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
		{
			Rect res = oldDataRect;
			if (newDataRect.IntersectsWith(border))
			{
				res = newDataRect;
				if (newDataRect.Size == oldDataRect.Size)
				{
					if (res.X < border.X) res.X = border.X;
					if (res.Y < border.Y) res.Y = border.Y;
					if (res.Right > border.Right) res.X += border.Right - res.Right;
					if (res.Bottom > border.Bottom) res.Y += border.Bottom - res.Bottom;
				}
				else
				{
					res = Rect.Intersect(newDataRect, border);
				}
			}
			return res;
		}
	}
}
