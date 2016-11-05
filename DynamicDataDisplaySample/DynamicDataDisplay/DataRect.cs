using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.DynamicDataDisplay
{
	public struct DataRect<TH, TV>
	{
		private TH x1;
		private TV y1;

		private TH x2;
		private TV y2;

		public DataRect(TH x1, TV y1, TH x2, TV y2)
		{
			this.x1 = x1;
			this.x2 = x2;
			this.y1 = y1;
			this.y2 = y2;
		}

		public TH X1 { get { return x1; } }
		public TV Y1 { get { return y1; } }
		public TH X2 { get { return x2; } }
		public TV Y2 { get { return y2; } }
	}
}
