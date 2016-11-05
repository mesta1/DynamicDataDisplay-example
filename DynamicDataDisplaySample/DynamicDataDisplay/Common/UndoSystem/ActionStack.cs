using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.DynamicDataDisplay.Common.UndoSystem
{
	public class ActionStack
	{
		private readonly Stack<UndoableAction> stack = new Stack<UndoableAction>();

		public void Push(UndoableAction action)
		{
			stack.Push(action);

			if (stack.Count == 1)
				RaiseIsEmptyChanged();
		}

		public UndoableAction Pop()
		{
			var action = stack.Pop();

			if (stack.Count == 0)
				RaiseIsEmptyChanged();

			return action;
		}

		public int Count { get { return stack.Count; } }

		public void Clear()
		{
			stack.Clear();
			RaiseIsEmptyChanged();
		}

		public bool IsEmpty
		{
			get { return stack.Count == 0; }
		}

		private void RaiseIsEmptyChanged()
		{
			IsEmptyChanged.Raise(this);
		}
		public event EventHandler IsEmptyChanged;
	}
}
