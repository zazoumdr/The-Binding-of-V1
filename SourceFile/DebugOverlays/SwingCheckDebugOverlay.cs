using UnityEngine;

namespace DebugOverlays;

public class SwingCheckDebugOverlay : MonoBehaviour
{
	private bool damaging;

	private EnemyIdentifier eid;

	public void ConsumeData(bool damaging, EnemyIdentifier eid)
	{
		this.damaging = damaging;
		this.eid = eid;
	}

	private void OnGUI()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		if (!damaging)
		{
			return;
		}
		Rect? onScreenRect = OnGUIHelper.GetOnScreenRect(base.transform.position);
		if (onScreenRect.HasValue)
		{
			Rect value = onScreenRect.Value;
			Rect rect = value;
			GUIStyle val = new GUIStyle
			{
				fontSize = 20,
				fontStyle = (FontStyle)1
			};
			val.normal.textColor = Color.red;
			GUI.Label(rect, "SWING!", val);
			value.y += 20f;
			if (eid == null)
			{
				Rect rect2 = value;
				GUIStyle val2 = new GUIStyle
				{
					fontSize = 20,
					fontStyle = (FontStyle)1
				};
				val2.normal.textColor = Color.magenta;
				GUI.Label(rect2, "No EID", val2);
			}
			else if (eid.target == null)
			{
				Rect rect3 = value;
				GUIStyle val3 = new GUIStyle
				{
					fontSize = 20,
					fontStyle = (FontStyle)1
				};
				val3.normal.textColor = Color.yellow;
				GUI.Label(rect3, "No target", val3);
			}
			else if (eid.target.isPlayer)
			{
				Rect rect4 = value;
				GUIStyle val4 = new GUIStyle
				{
					fontSize = 20,
					fontStyle = (FontStyle)1
				};
				val4.normal.textColor = Color.green;
				GUI.Label(rect4, "Player target", val4);
			}
			else
			{
				Rect rect5 = value;
				string text = eid.target.ToString();
				GUIStyle val5 = new GUIStyle
				{
					fontSize = 20,
					fontStyle = (FontStyle)1
				};
				val5.normal.textColor = Color.blue;
				GUI.Label(rect5, text, val5);
			}
		}
	}
}
