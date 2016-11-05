using System;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay
{
	public sealed class PenDescription : StandardDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="PenDescription"/> class.
		/// </summary>
		public PenDescription() { }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PenDescription"/> class.
		/// </summary>
		/// <param name="description">Custom description.</param>
		public PenDescription(string description) : base(description) { }

		protected override LegendItem CreateLegendItemCore() {
			return new LineLegendItem(this);
		}

		protected override void AttachCore(UIElement graph) {
			base.AttachCore(graph);
			LineGraph g = graph as LineGraph;
			if (g == null) {
				throw new ArgumentException("Pen description can only be attached to PointsGraph", "graph");
			}
			pen = g.LinePen;
		}

		private Pen pen;

		public Pen Pen {
			get { return pen; }
		}
	}
}
