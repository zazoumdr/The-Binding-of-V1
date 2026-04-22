using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveCustomSetter : MonoBehaviour
{
	public enum ButtonState
	{
		Selected,
		Unselected
	}

	private int _wave;

	public int waveChangeAmount;

	private WaveMenu wm;

	private ButtonState _state;

	private ShopButton shopButton;

	private bool prepared;

	private Button button;

	[SerializeField]
	private Image buttonGraphic;

	[SerializeField]
	private TMP_Text buttonText;

	[SerializeField]
	private Button increaseButton;

	[SerializeField]
	private Button decreaseButton;

	[SerializeField]
	private Image increaseArrow;

	[SerializeField]
	private Image decreaseArrow;

	[Space]
	[SerializeField]
	private ShopButton increaseShopButton;

	[SerializeField]
	private ShopButton decreaseShopButton;

	public int wave
	{
		get
		{
			return _wave;
		}
		set
		{
			_wave = value;
			buttonText.SetText(wave.ToString(), true);
			UpdateChangeButtons();
		}
	}

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
			shopButton.deactivated = state == ButtonState.Selected;
			UpdateChangeButtons();
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
		increaseShopButton.PointerClickSuccess += delegate
		{
			IncreaseWave();
		};
		decreaseShopButton.PointerClickSuccess += delegate
		{
			DecreaseWave();
		};
	}

	public void IncreaseWave()
	{
		if (wm.highestWave >= 60)
		{
			if (wave + waveChangeAmount <= wm.highestWave / 2)
			{
				wave += waveChangeAmount;
			}
			Select();
		}
	}

	public void DecreaseWave()
	{
		if (wm.highestWave >= 60)
		{
			if (wave - waveChangeAmount >= 30)
			{
				wave -= waveChangeAmount;
			}
			Select();
		}
	}

	private void UpdateChangeButtons()
	{
		bool flag = wave + waveChangeAmount <= wm.highestWave / 2;
		((Selectable)increaseButton).interactable = flag;
		increaseShopButton.deactivated = !flag;
		increaseShopButton.failure = !flag;
		((Graphic)increaseArrow).color = (flag ? Color.white : Color.gray);
		bool flag2 = wave - waveChangeAmount >= 30;
		((Selectable)decreaseButton).interactable = flag2;
		decreaseShopButton.deactivated = !flag2;
		decreaseShopButton.failure = !flag2;
		((Graphic)decreaseArrow).color = (flag2 ? Color.white : Color.gray);
	}

	private void Select()
	{
		wm.SetCurrentWave(wave);
	}
}
