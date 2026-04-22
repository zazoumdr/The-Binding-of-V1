using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using UnityEngine;

namespace NewBlood.Rendering;

public static class RenderThread
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate void UnityRenderingEventDelegate(int eventId);

	private readonly struct PluginEvent
	{
		public object? State { get; }

		public SendOrPostCallback Callback { get; }

		public PluginEvent(SendOrPostCallback callback, object? state)
		{
			State = state;
			Callback = callback;
		}
	}

	private const int InitialCapacity = 16;

	private static int s_NextEventId;

	private static int s_MainThreadId;

	private static readonly Dictionary<int, PluginEvent> s_EventQueue = new Dictionary<int, PluginEvent>(16);

	private static readonly UnityRenderingEventDelegate s_SendHandler = OnUnityRenderingEventSend;

	private static readonly IntPtr s_SendHandlerPtr = Marshal.GetFunctionPointerForDelegate(s_SendHandler);

	private static readonly UnityRenderingEventDelegate s_PostHandler = OnUnityRenderingEventPost;

	private static readonly IntPtr s_PostHandlerPtr = Marshal.GetFunctionPointerForDelegate(s_PostHandler);

	private static readonly ManualResetEventSlim s_WaitHandle = new ManualResetEventSlim();

	public static void Send(SendOrPostCallback callback, object? state)
	{
		ThrowIfNotMainThread("Send");
		s_WaitHandle.Reset();
		QueuePluginEvent(callback, state, s_SendHandlerPtr);
		s_WaitHandle.Wait();
	}

	public static void Post(SendOrPostCallback callback, object? state)
	{
		ThrowIfNotMainThread("Post");
		QueuePluginEvent(callback, state, s_PostHandlerPtr);
	}

	private static void QueuePluginEvent(SendOrPostCallback callback, object? state, IntPtr handler)
	{
		int num = s_NextEventId++;
		lock (s_EventQueue)
		{
			s_EventQueue.Add(num, new PluginEvent(callback, state));
		}
		GL.IssuePluginEvent(handler, num);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnSubsystemRegistration()
	{
		s_MainThreadId = Environment.CurrentManagedThreadId;
	}

	private static void ThrowIfNotMainThread([CallerMemberName] string? caller = null)
	{
		if (Environment.CurrentManagedThreadId != s_MainThreadId)
		{
			throw new UnityException("RenderThread." + caller + " can only be called from the main thread.");
		}
	}

	[MonoPInvokeCallback(typeof(UnityRenderingEventDelegate))]
	private static void OnUnityRenderingEventSend(int eventId)
	{
		InvokePluginEvent(eventId);
		s_WaitHandle.Set();
	}

	[MonoPInvokeCallback(typeof(UnityRenderingEventDelegate))]
	private static void OnUnityRenderingEventPost(int eventId)
	{
		InvokePluginEvent(eventId);
	}

	private static void InvokePluginEvent(int eventId)
	{
		PluginEvent value;
		lock (s_EventQueue)
		{
			if (!s_EventQueue.Remove(eventId, out value))
			{
				return;
			}
		}
		try
		{
			value.Callback(value.State);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
