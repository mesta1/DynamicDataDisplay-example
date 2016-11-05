using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.Common.Auxiliary;

namespace Microsoft.Research.DynamicDataDisplay.Charts.Axes
{
	public class IntegerTicksProvider : ITicksProvider<int>
	{
		private int minStep = 0;
		public int MinStep
		{
			get { return minStep; }
			set
			{
				Verify.IsTrue(value >= 0, "value");
				if (minStep != value)
				{
					minStep = value;
					RaiseChangedEvent();
				}
			}
		}

		private int maxStep = Int32.MaxValue;
		public int MaxStep
		{
			get { return maxStep; }
			set
			{
				if (maxStep != value)
				{
					if (value < 0)
						throw new ArgumentOutOfRangeException("value", Properties.Resources.ParameterShouldBePositive);

					maxStep = value;
					RaiseChangedEvent();
				}
			}
		}

		#region ITicksProvider<int> Members

		public ITicksInfo<int> GetTicks(Range<int> range, int ticksCount)
		{
			double start = range.Min;
			double finish = range.Max;

			double delta = finish - start;

			int log = (int)Math.Round(Math.Log10(delta));

			double newStart = RoundHelper.Round(start, log);
			double newFinish = RoundHelper.Round(finish, log);
			if (newStart == newFinish)
			{
				log--;
				newStart = RoundHelper.Round(start, log);
				newFinish = RoundHelper.Round(finish, log);
			}

			// calculating step between ticks
			double unroundedStep = (newFinish - newStart) / ticksCount;
			int stepLog = log;
			// trying to round step
			int step = (int)RoundHelper.Round(unroundedStep, stepLog);
			if (step == 0)
			{
				stepLog--;
				step = (int)RoundHelper.Round(unroundedStep, stepLog);
				if (step == 0)
				{
					// step will not be rounded if attempts to be rounded to zero.
					step = (int)unroundedStep;
				}
			}

			if (step < minStep)
				step = minStep;
			if (step > maxStep)
				step = maxStep;

			if (step <= 0)
				step = 1;

			int[] ticks = CreateTicks(start, finish, step);

			TicksInfo<int> res = new TicksInfo<int> { Info = log, Ticks = ticks };

			return res;
		}

		private static int[] CreateTicks(double start, double finish, int step)
		{
			DebugVerify.Is(step != 0);

			int x = (int)(step * Math.Floor(start / (double)step));
			List<int> res = new List<int>();

			checked
			{
				double increasedFinish = finish + step * 1.05;
				while (x <= increasedFinish)
				{
					res.Add(x);
					x += step;
				}
			}
			return res.ToArray();
		}

		private static int[] tickCounts = new int[] { 20, 10, 5, 4, 2, 1 };

		public int DecreaseTickCount(int ticksCount)
		{
			return tickCounts.FirstOrDefault(tick => tick < ticksCount);
		}

		public int IncreaseTickCount(int ticksCount)
		{
			int newTickCount = tickCounts.Reverse().FirstOrDefault(tick => tick > ticksCount);
			if (newTickCount == 0)
				newTickCount = tickCounts[0];

			return newTickCount;
		}

		public ITicksProvider<int> MinorProvider
		{
			get { return null; }
		}

		public ITicksProvider<int> MayorProvider
		{
			get { return null; }
		}

		protected void RaiseChangedEvent()
		{
			Changed.Raise(this);
		}
		public event EventHandler Changed;

		#endregion
	}
}
