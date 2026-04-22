using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveSetter : MonoBehaviour
{
	public int wave;

	private WaveMenu wm;

	private ButtonState _state;

	private ShopButton shopButton;

	private bool prepared;

	private Button button;

	[SerializeField]
	private Image buttonGraphic;

	[SerializeField]
	private TMP_Text buttonText;

	public ButtonState state
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			((Selectable)button).interactable = state == ButtonState.Unselected;
			shopButton.deactivated = state != ButtonState.Unselected;
			shopButton.failure = state == ButtonState.Locked;
			((Graphic)buttonGraphic).color = ((state == ButtonState.Locked) ? Color.red : Color.white);
			((Graphic)buttonText).color = ((state == ButtonState.Locked) ? Color.red : Color.white);
		}
	}

	private void Awake()
	{
		button = GetComponent<Button>();
		wm = GetComponentInParent<WaveMenu>();
		if (TryGetComponent<ShopButton>(out shopButton))
		{
			shopButton.PointerClickSuccess += delegate
			{
				Select();
			};
		}
	}

	private void Select()
	{
		if (_state != ButtonState.Locked)
		{
			wm.SetCurrentWave(wave);
		}
	}
}
