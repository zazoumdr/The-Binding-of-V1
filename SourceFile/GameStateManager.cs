using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class GameStateManager : MonoBehaviour
{
	public bool introCheckComplete;

	public CustomGameDetails currentCustomGame;

	private readonly Dictionary<string, GameState> activeStates = new Dictionary<string, GameState>();

	private readonly List<string> stateOrder = new List<string>();

	public static GameStateManager Instance { get; private set; }

	public Vector3 defaultGravity { get; private set; }

	public bool CameraLocked { get; private set; }

	public bool PlayerInputLocked { get; private set; }

	public bool CursorLocked { get; private set; }

	public float TimerModifier { get; private set; } = 1f;

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
		defaultGravity = Physics.gravity;
		IntroCheck();
	}

	private void IntroCheck()
	{
		if (introCheckComplete || GameProgressSaver.GetIntro())
		{
			introCheckComplete = true;
			AudioListener.volume = MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume");
		}
	}

	public bool IsStateActive(string stateKey)
	{
		return activeStates.ContainsKey(stateKey);
	}

	public void RegisterState(GameState newState)
	{
		if (activeStates.ContainsKey(newState.key))
		{
			Debug.LogWarning("State " + newState.key + " is already registered");
			return;
		}
		activeStates.Add(newState.key, newState);
		int num = stateOrder.Count;
		for (int num2 = stateOrder.Count - 1; num2 >= 0; num2--)
		{
			string key = stateOrder[num2];
			GameState gameState = activeStates[key];
			num = num2;
			if (gameState.priority > newState.priority)
			{
				num++;
				break;
			}
		}
		stateOrder.Insert(num, newState.key);
		EvaluateState();
	}

	public void PopState(string stateKey)
	{
		if (!activeStates.ContainsKey(stateKey))
		{
			Debug.Log("Tried to pop state " + stateKey + ", but it was not registered");
			return;
		}
		activeStates.Remove(stateKey);
		stateOrder.Remove(stateKey);
		EvaluateState();
	}

	public void SceneReset()
	{
		ResetGravity();
		IntroCheck();
	}

	public void ResetGravity()
	{
		Physics.gravity = defaultGravity;
	}

	private void EvaluateState()
	{
		float num = 1f;
		for (int num2 = stateOrder.Count - 1; num2 >= 0; num2--)
		{
			string key = stateOrder[num2];
			GameState gameState = activeStates[key];
			if (gameState.cursorLock != LockMode.None)
			{
				CursorLocked = gameState.cursorLock == LockMode.Lock;
			}
			if (gameState.playerInputLock != LockMode.None)
			{
				PlayerInputLocked = gameState.playerInputLock == LockMode.Lock;
			}
			if (gameState.cameraInputLock != LockMode.None)
			{
				CameraLocked = gameState.cameraInputLock == LockMode.Lock;
			}
			if (gameState.timerModifier.HasValue)
			{
				num *= gameState.timerModifier.Value;
			}
		}
		Cursor.lockState = (CursorLocked ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = !CursorLocked;
		TimerModifier = num;
	}

	private void Update()
	{
		for (int num = stateOrder.Count - 1; num >= 0; num--)
		{
			string text = stateOrder[num];
			if (!activeStates[text].IsValid())
			{
				activeStates.Remove(text);
				stateOrder.Remove(text);
				EvaluateState();
				break;
			}
		}
	}
}
