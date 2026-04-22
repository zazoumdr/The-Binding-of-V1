using plog.Helpers;
using plog.Models;
using plog.unity.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole;

public class LogLine : MonoBehaviour
{
	[SerializeField]
	private TMP_Text timestamp;

	[SerializeField]
	private TMP_Text message;

	[SerializeField]
	private TMP_Text context;

	[SerializeField]
	private Image contextPanel;

	[SerializeField]
	private Image mainPanel;

	[Space]
	[SerializeField]
	private CanvasGroup attentionFlashGroup;

	[Space]
	[SerializeField]
	private Color normalLogColor;

	[SerializeField]
	private Color warningLogColor;

	[SerializeField]
	private Color errorLogColor;

	[SerializeField]
	private Color cliLogColor;

	[Space]
	[SerializeField]
	private float normalHeight = 35f;

	[SerializeField]
	private float expandedHeight = 120f;

	private RectTransform rectTransform;

	private Vector2? defaultTextOffsetMin;

	private Vector2? defaultTextOffsetMax;

	private Vector2? defaultTextSizeDelta;

	private ConsoleLog log;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void Wipe()
	{
		log = null;
		timestamp.text = "";
		message.text = "";
		((Graphic)mainPanel).color = normalLogColor;
		RefreshSize();
	}

	public void PopulateLine(ConsoleLog capture)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Invalid comparison between Unknown and I4
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Invalid comparison between Unknown and I4
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Invalid comparison between Unknown and I4
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Invalid comparison between Unknown and I4
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		log = capture;
		timestamp.text = $"{capture.log.Timestamp:HH:mm:ss.f}";
		RefreshSize();
		if (capture.expanded && !string.IsNullOrEmpty(capture.log.StackTrace))
		{
			string stackTrace = capture.log.StackTrace;
			stackTrace = stackTrace.Replace("\r\n", "\n").Replace("\n", "");
			message.text = $"<b><size={message.fontSizeMax}>{capture.log.Message}</size></b>\n{stackTrace}";
			message.enableAutoSizing = true;
		}
		else
		{
			message.text = capture.log.Message;
			message.fontSize = message.fontSizeMax;
			message.enableAutoSizing = false;
		}
		Image val = mainPanel;
		Level level = capture.log.Level;
		Color color;
		if ((int)level <= 300)
		{
			if ((int)level != 200)
			{
				if ((int)level != 300)
				{
					goto IL_0163;
				}
				color = warningLogColor;
			}
			else
			{
				color = normalLogColor;
			}
		}
		else if ((int)level != 400)
		{
			if ((int)level != 500)
			{
				if ((int)level != 600)
				{
					goto IL_0163;
				}
				color = cliLogColor;
			}
			else
			{
				color = errorLogColor;
			}
		}
		else
		{
			color = errorLogColor;
		}
		goto IL_016a;
		IL_0163:
		color = normalLogColor;
		goto IL_016a;
		IL_016a:
		((Graphic)val).color = color;
		if (capture.source?.Tag != null)
		{
			context.text = ((object)capture.source.Tag).ToString();
			var (val2, val3) = ColorHelper.GetColorPair(capture.source.Tag.Color);
			((Graphic)context).color = UniversalColorExtensions.ToUnityColor(val3);
			Color color2 = UniversalColorExtensions.ToUnityColor(val2);
			color2.a = ((Graphic)contextPanel).color.a;
			((Graphic)contextPanel).color = color2;
			if (!((Component)(object)contextPanel).gameObject.activeSelf)
			{
				((Component)(object)contextPanel).gameObject.SetActive(value: true);
				RectTransform rectTransform = message.rectTransform;
				if (defaultTextOffsetMin.HasValue)
				{
					rectTransform.offsetMin = defaultTextOffsetMin.Value;
				}
				if (defaultTextOffsetMax.HasValue)
				{
					rectTransform.offsetMax = defaultTextOffsetMax.Value;
				}
				if (defaultTextSizeDelta.HasValue)
				{
					rectTransform.sizeDelta = defaultTextSizeDelta.Value;
				}
			}
		}
		else if (((Component)(object)contextPanel).gameObject.activeSelf)
		{
			((Component)(object)contextPanel).gameObject.SetActive(value: false);
			float x = ((Graphic)contextPanel).rectTransform.sizeDelta.x;
			RectTransform rectTransform2 = message.rectTransform;
			if (!defaultTextOffsetMin.HasValue)
			{
				defaultTextOffsetMin = rectTransform2.offsetMin;
			}
			if (!defaultTextOffsetMax.HasValue)
			{
				defaultTextOffsetMax = rectTransform2.offsetMax;
			}
			if (!defaultTextSizeDelta.HasValue)
			{
				defaultTextSizeDelta = rectTransform2.sizeDelta;
			}
			rectTransform2.offsetMin = new Vector2(rectTransform2.offsetMin.x - x * 2f, defaultTextOffsetMin.Value.y);
			rectTransform2.offsetMax = new Vector2(rectTransform2.offsetMax.x + x, defaultTextOffsetMax.Value.y);
			rectTransform2.sizeDelta = new Vector2(rectTransform2.sizeDelta.x - x * 2f, defaultTextSizeDelta.Value.y);
		}
		if ((float)capture.timeSinceLogged < 0.5f && base.gameObject.activeInHierarchy)
		{
			attentionFlashGroup.alpha = TimeSinceToFlashAlpha(capture.timeSinceLogged);
		}
	}

	public void ToggleExpand()
	{
		log.expanded = !log.expanded;
		RefreshSize();
		PopulateLine(log);
	}

	private void RefreshSize()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		RectTransform obj = rectTransform;
		ConsoleLog consoleLog = log;
		obj.sizeDelta = ((consoleLog == null || !consoleLog.expanded) ? new Vector2(rectTransform.sizeDelta.x, normalHeight) : new Vector2(rectTransform.sizeDelta.x, expandedHeight));
	}

	private void Update()
	{
		if (log != null)
		{
			if ((float)log.timeSinceLogged > 0.5f)
			{
				attentionFlashGroup.alpha = 0f;
			}
			else
			{
				attentionFlashGroup.alpha = TimeSinceToFlashAlpha(log.timeSinceLogged);
			}
		}
	}

	private float TimeSinceToFlashAlpha(float timeSinceLogged)
	{
		if (timeSinceLogged < 0.2f)
		{
			return timeSinceLogged / 0.2f;
		}
		return 1f - (timeSinceLogged - 0.2f) / 0.3f;
	}
}
