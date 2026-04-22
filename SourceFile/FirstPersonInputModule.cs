using UnityEngine;
using UnityEngine.EventSystems;

public class FirstPersonInputModule : StandaloneInputModule
{
	protected override MouseState GetMousePointerEventData(int id)
	{
		CursorLockMode lockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.None;
		MouseState mousePointerEventData = ((PointerInputModule)this).GetMousePointerEventData(id);
		Cursor.lockState = lockState;
		return mousePointerEventData;
	}

	protected override void ProcessMove(PointerEventData pointerEvent)
	{
		CursorLockMode lockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.None;
		((PointerInputModule)this).ProcessMove(pointerEvent);
		Cursor.lockState = lockState;
	}

	protected override void ProcessDrag(PointerEventData pointerEvent)
	{
		CursorLockMode lockState = Cursor.lockState;
		Cursor.lockState = CursorLockMode.None;
		((PointerInputModule)this).ProcessDrag(pointerEvent);
		Cursor.lockState = lockState;
	}
}
