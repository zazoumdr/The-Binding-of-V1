using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionState
{
	public InputAction Action { get; }

	public float PerformedTime { get; private set; }

	public int PerformedFrame { get; private set; }

	public int CanceledFrame { get; private set; }

	public bool IsPressed { get; private set; }

	public string LastUsedBinding { get; private set; }

	public float HoldTime
	{
		get
		{
			if (!IsPressed && !WasCanceledThisFrame)
			{
				return 0f;
			}
			return Time.time - PerformedTime;
		}
	}

	public bool WasPerformedThisFrame => PerformedFrame == Time.frameCount;

	public bool WasCanceledThisFrame => CanceledFrame == Time.frameCount;

	public InputActionState(InputAction action)
	{
		Action = action;
		action.started += OnTriggered;
		action.canceled += OnTriggered;
	}

	private void OnTriggered(CallbackContext context)
	{
		if (((CallbackContext)(ref context)).started)
		{
			IsPressed = true;
			PerformedFrame = Time.frameCount;
			PerformedTime = Time.time;
			LastUsedBinding = ((CallbackContext)(ref context)).control.path;
		}
		else if (((CallbackContext)(ref context)).canceled)
		{
			IsPressed = false;
			CanceledFrame = Time.frameCount;
		}
	}

	public TValue ReadValue<TValue>() where TValue : struct
	{
		return Action.ReadValue<TValue>();
	}
}
