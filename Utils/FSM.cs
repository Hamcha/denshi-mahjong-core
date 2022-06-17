using System;
using System.Collections.Generic;

namespace DenshiMahjong.Utils
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
		
		public FSM(T initialState)
		{
			Current = initialState;
			History = new List<Transition>();
			History.Add(new Transition() { state = initialState, time = Godot.Time.GetTicksUsec() });
		}

		public void Set(T state)
		{
			if (Current.CompareTo(state) == 0)
				return;
			
			var time = Godot.Time.GetTicksUsec();
			History.Add(new Transition { state = state, time = time });
			Current = state;
			
			OnStateChanged?.Invoke(Current);
		}
	}
}
