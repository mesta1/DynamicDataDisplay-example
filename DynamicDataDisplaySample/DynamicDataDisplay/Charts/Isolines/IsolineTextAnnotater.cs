using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Isolines
{
	/// <summary>
	/// IsolineTextAnnotater defines methods to annotate isolines - create a list of labels with its position.
	/// </summary>
	public sealed class IsolineTextAnnotater
	{
		private double wayBeforeText = 10;
		/// <summary>
		/// Gets or sets the distance between text labels.
		/// </summary>
		public double WayBeforeText
		{
			get { return wayBeforeText; }
			set { wayBeforeText = value; }
		}

		/// <summary>
		/// Annotates the specified isoline collection.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="visible">The visible rectangle.</param>
		/// <returns></returns>
		public Collection<IsolineTextLabel> Annotate(IsolineCollection collection, Rect visible)
		{
			Collection<IsolineTextLabel> res = new Collection<IsolineTextLabel>();

			foreach (var line in collection.Lines)
			{
				double way = 0;
				foreach (var segment in line.GetSegments())
				{
					double length = segment.GetLength();
					way += length;
					if (way > wayBeforeText)
					{
						way = 0;
						res.Add(new IsolineTextLabel
						{
							Text = line.RealValue.ToString("G2"),
							Position = segment.Max,
							Rotation = (segment.Max - segment.Min).ToAngle()
						});
					}
				}
			}

			return res;
		}
	}
}
