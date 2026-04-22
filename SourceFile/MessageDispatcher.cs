using UnityEngine;
using UnityEngine.Events;

public abstract class MessageDispatcher : MessageDispatcherBase
{
	[SerializeField]
	private UnityEvent _handler = new UnityEvent();

	public new UnityEvent Handler => _handler;

	public void AddListener(UnityAction action)
	{
		Handler.AddListener(action);
	}

	public void RemoveListener(UnityAction action)
	{
		Handler.RemoveListener(action);
	}

	public new void RemoveAllListeners()
	{
		Handler.RemoveAllListeners();
	}

	protected sealed override UnityEventBase GetHandler()
	{
		return Handler;
	}
}
public abstract class MessageDispatcher<T> : MessageDispatcherBase
{
	public abstract class Callback<TEvent> : MessageDispatcher<T> where TEvent : UnityEvent<T>, new()
	{
		[SerializeField]
		private TEvent _handler;

		public sealed override UnityEvent<T> Handler => _handler;

		public Callback()
		{
			_handler = new TEvent();
		}
	}

	public new abstract UnityEvent<T> Handler { get; }

	private MessageDispatcher()
	{
	}

	public void AddListener(UnityAction<T> action)
	{
		Handler.AddListener(action);
	}

	public void RemoveListener(UnityAction<T> action)
	{
		Handler.RemoveListener(action);
	}

	public new void RemoveAllListeners()
	{
		Handler.RemoveAllListeners();
	}

	protected sealed override UnityEventBase GetHandler()
	{
		return Handler;
	}
}
public abstract class MessageDispatcher<T1, T2> : MessageDispatcherBase
{
	public abstract class Callback<TEvent> : MessageDispatcher<T1, T2> where TEvent : UnityEvent<T1, T2>, new()
	{
		[SerializeField]
		private TEvent _handler;

		public sealed override UnityEvent<T1, T2> Handler => _handler;

		public Callback()
		{
			_handler = new TEvent();
		}
	}

	public new abstract UnityEvent<T1, T2> Handler { get; }

	private MessageDispatcher()
	{
	}

	public void AddListener(UnityAction<T1, T2> action)
	{
		Handler.AddListener(action);
	}

	public void RemoveListener(UnityAction<T1, T2> action)
	{
		Handler.RemoveListener(action);
	}

	public new void RemoveAllListeners()
	{
		Handler.RemoveAllListeners();
	}

	protected sealed override UnityEventBase GetHandler()
	{
		return Handler;
	}
}
