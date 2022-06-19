using System;
using System.Collections.Generic;

namespace MahjongLib.Utils
{
	public class FSM<T> where T : IComparable
	{
		public struct Transition
		{
			public T state;
			public ulong time;
		}
		
		public event Action<T> OnStateChanged;
		
		public T Current { get; private set; }
		public List<Transition> History { get; private set; }

		private readonly ITimestamp _time;
		
		public FSM(T initialState, ITimestamp time)
		{
			_time = time;
			Current = initialState;
			History = new List<Transition>();
			History.Add(new Transition() { state = initialState, time = _time.Now });
		}

		public void Set(T state)
		{
			if (Current.CompareTo(state) == 0)
				return;
			
			History.Add(new Transition { state = state, time = _time.Now });
			Current = state;
			
			OnStateChanged?.Invoke(Current);
		}
	}
}
