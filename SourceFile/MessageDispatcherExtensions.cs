using UnityEngine;
using UnityEngine.Events;

public static class MessageDispatcherExtensions
{
	public static void RemoveAllListeners<TMessage>(this GameObject gameObject) where TMessage : MessageDispatcherBase
	{
		if (gameObject.TryGetComponent<TMessage>(out var component))
		{
			component.RemoveAllListeners();
		}
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction handler) where TMessage : MessageDispatcher
	{
		gameObject.GetOrAddComponent<TMessage>().AddListener(handler);
	}

	public static void AddListener<TMessage, T>(this GameObject gameObject, UnityAction<T> handler) where TMessage : MessageDispatcher<T>
	{
		gameObject.GetOrAddComponent<TMessage>().AddListener(handler);
	}

	public static void AddListener<TMessage, T1, T2>(this GameObject gameObject, UnityAction<T1, T2> handler) where TMessage : MessageDispatcher<T1, T2>
	{
		gameObject.GetOrAddComponent<TMessage>().AddListener(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<int> handler) where TMessage : MessageDispatcher<int>
	{
		gameObject.AddListener<TMessage, int>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<bool> handler) where TMessage : MessageDispatcher<bool>
	{
		gameObject.AddListener<TMessage, bool>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<AnimationEvent> handler) where TMessage : MessageDispatcher<AnimationEvent>
	{
		gameObject.AddListener<TMessage, AnimationEvent>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<float[], int> handler) where TMessage : MessageDispatcher<float[], int>
	{
		gameObject.AddListener<TMessage, float[], int>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<Collision> handler) where TMessage : MessageDispatcher<Collision>
	{
		gameObject.AddListener<TMessage, Collision>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<Collision2D> handler) where TMessage : MessageDispatcher<Collision2D>
	{
		gameObject.AddListener<TMessage, Collision2D>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<ControllerColliderHit> handler) where TMessage : MessageDispatcher<ControllerColliderHit>
	{
		gameObject.AddListener<TMessage, ControllerColliderHit>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<float> handler) where TMessage : MessageDispatcher<float>
	{
		gameObject.AddListener<TMessage, float>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<Joint2D> handler) where TMessage : MessageDispatcher<Joint2D>
	{
		gameObject.AddListener<TMessage, Joint2D>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<GameObject> handler) where TMessage : MessageDispatcher<GameObject>
	{
		gameObject.AddListener<TMessage, GameObject>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<RenderTexture, RenderTexture> handler) where TMessage : MessageDispatcher<RenderTexture, RenderTexture>
	{
		gameObject.AddListener<TMessage, RenderTexture, RenderTexture>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<Collider> handler) where TMessage : MessageDispatcher<Collider>
	{
		gameObject.AddListener<TMessage, Collider>(handler);
	}

	public static void AddListener<TMessage>(this GameObject gameObject, UnityAction<Collider2D> handler) where TMessage : MessageDispatcher<Collider2D>
	{
		gameObject.AddListener<TMessage, Collider2D>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction handler) where TMessage : MessageDispatcher
	{
		gameObject.GetOrAddComponent<TMessage>().RemoveListener(handler);
	}

	public static void RemoveListener<TMessage, T>(this GameObject gameObject, UnityAction<T> handler) where TMessage : MessageDispatcher<T>
	{
		gameObject.GetOrAddComponent<TMessage>().RemoveListener(handler);
	}

	public static void RemoveListener<TMessage, T1, T2>(this GameObject gameObject, UnityAction<T1, T2> handler) where TMessage : MessageDispatcher<T1, T2>
	{
		gameObject.GetOrAddComponent<TMessage>().RemoveListener(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<int> handler) where TMessage : MessageDispatcher<int>
	{
		gameObject.RemoveListener<TMessage, int>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<bool> handler) where TMessage : MessageDispatcher<bool>
	{
		gameObject.RemoveListener<TMessage, bool>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<AnimationEvent> handler) where TMessage : MessageDispatcher<AnimationEvent>
	{
		gameObject.RemoveListener<TMessage, AnimationEvent>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<float[], int> handler) where TMessage : MessageDispatcher<float[], int>
	{
		gameObject.RemoveListener<TMessage, float[], int>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<Collision> handler) where TMessage : MessageDispatcher<Collision>
	{
		gameObject.RemoveListener<TMessage, Collision>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<Collision2D> handler) where TMessage : MessageDispatcher<Collision2D>
	{
		gameObject.RemoveListener<TMessage, Collision2D>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<ControllerColliderHit> handler) where TMessage : MessageDispatcher<ControllerColliderHit>
	{
		gameObject.RemoveListener<TMessage, ControllerColliderHit>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<float> handler) where TMessage : MessageDispatcher<float>
	{
		gameObject.RemoveListener<TMessage, float>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<Joint2D> handler) where TMessage : MessageDispatcher<Joint2D>
	{
		gameObject.RemoveListener<TMessage, Joint2D>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<GameObject> handler) where TMessage : MessageDispatcher<GameObject>
	{
		gameObject.RemoveListener<TMessage, GameObject>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<RenderTexture, RenderTexture> handler) where TMessage : MessageDispatcher<RenderTexture, RenderTexture>
	{
		gameObject.RemoveListener<TMessage, RenderTexture, RenderTexture>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<Collider> handler) where TMessage : MessageDispatcher<Collider>
	{
		gameObject.RemoveListener<TMessage, Collider>(handler);
	}

	public static void RemoveListener<TMessage>(this GameObject gameObject, UnityAction<Collider2D> handler) where TMessage : MessageDispatcher<Collider2D>
	{
		gameObject.RemoveListener<TMessage, Collider2D>(handler);
	}

	public static void RemoveAllListeners<TMessage>(this Component component) where TMessage : MessageDispatcherBase
	{
		component.gameObject.RemoveAllListeners<TMessage>();
	}

	public static void AddListener<TMessage>(this Component component, UnityAction handler) where TMessage : MessageDispatcher
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage, T>(this Component component, UnityAction<T> handler) where TMessage : MessageDispatcher<T>
	{
		component.gameObject.AddListener<TMessage, T>(handler);
	}

	public static void AddListener<TMessage, T1, T2>(this Component component, UnityAction<T1, T2> handler) where TMessage : MessageDispatcher<T1, T2>
	{
		component.gameObject.AddListener<TMessage, T1, T2>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<int> handler) where TMessage : MessageDispatcher<int>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<bool> handler) where TMessage : MessageDispatcher<bool>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<AnimationEvent> handler) where TMessage : MessageDispatcher<AnimationEvent>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<float[], int> handler) where TMessage : MessageDispatcher<float[], int>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<Collision> handler) where TMessage : MessageDispatcher<Collision>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<Collision2D> handler) where TMessage : MessageDispatcher<Collision2D>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<ControllerColliderHit> handler) where TMessage : MessageDispatcher<ControllerColliderHit>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<float> handler) where TMessage : MessageDispatcher<float>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<Joint2D> handler) where TMessage : MessageDispatcher<Joint2D>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<GameObject> handler) where TMessage : MessageDispatcher<GameObject>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<RenderTexture, RenderTexture> handler) where TMessage : MessageDispatcher<RenderTexture, RenderTexture>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<Collider> handler) where TMessage : MessageDispatcher<Collider>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void AddListener<TMessage>(this Component component, UnityAction<Collider2D> handler) where TMessage : MessageDispatcher<Collider2D>
	{
		component.gameObject.AddListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction handler) where TMessage : MessageDispatcher
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<int> handler) where TMessage : MessageDispatcher<int>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<bool> handler) where TMessage : MessageDispatcher<bool>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<AnimationEvent> handler) where TMessage : MessageDispatcher<AnimationEvent>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<float[], int> handler) where TMessage : MessageDispatcher<float[], int>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<Collision> handler) where TMessage : MessageDispatcher<Collision>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<Collision2D> handler) where TMessage : MessageDispatcher<Collision2D>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<ControllerColliderHit> handler) where TMessage : MessageDispatcher<ControllerColliderHit>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<float> handler) where TMessage : MessageDispatcher<float>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<Joint2D> handler) where TMessage : MessageDispatcher<Joint2D>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<GameObject> handler) where TMessage : MessageDispatcher<GameObject>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<RenderTexture, RenderTexture> handler) where TMessage : MessageDispatcher<RenderTexture, RenderTexture>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<Collider> handler) where TMessage : MessageDispatcher<Collider>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}

	public static void RemoveListener<TMessage>(this Component component, UnityAction<Collider2D> handler) where TMessage : MessageDispatcher<Collider2D>
	{
		component.gameObject.RemoveListener<TMessage>(handler);
	}
}
