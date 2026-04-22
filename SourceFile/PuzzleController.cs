using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour
{
	private bool backOnBlack = true;

	private Image img;

	private PuzzlePanel[] panels;

	public List<PuzzlePanel> activatedPanels = new List<PuzzlePanel>();

	public List<TileColor> activatedColors = new List<TileColor>();

	public bool puzzleInProgress;

	public bool solved;

	public GameObject[] toActivate;

	private TileColor currentColor;

	private int starts;

	private int ends;

	public GameObject puzzleCorrect;

	public GameObject puzzleWrong;

	public GameObject puzzleClick;

	private float checkForHold;

	private Punch punch;

	private void Start()
	{
		panels = GetComponentsInChildren<PuzzlePanel>();
		img = GetComponent<Image>();
	}

	private void OnDisable()
	{
		if (!solved)
		{
			ResetPuzzle();
		}
	}

	private void Update()
	{
		if (!backOnBlack)
		{
			((Graphic)img).color = Color.Lerp(((Graphic)img).color, Color.white, Time.deltaTime);
			if (((Graphic)img).color == Color.white)
			{
				backOnBlack = true;
			}
		}
		if (checkForHold > 0f)
		{
			checkForHold = Mathf.MoveTowards(checkForHold, 0f, Time.deltaTime);
		}
	}

	public void Clicked(PuzzlePanel other)
	{
		if (other.tileType == TileType.Start)
		{
			if (!puzzleInProgress && !activatedPanels.Contains(other))
			{
				if (solved)
				{
					solved = false;
					backOnBlack = false;
					ResetPuzzle();
				}
				checkForHold = 0.3f;
				Object.Instantiate(puzzleClick, other.transform.position, Quaternion.identity);
				starts++;
				puzzleInProgress = true;
				currentColor = other.tileColor;
				other.Activate(currentColor);
				activatedPanels.Add(other);
				activatedColors.Add(currentColor);
			}
			else
			{
				ResetPuzzle();
			}
		}
		else if (other.tileType == TileType.End && puzzleInProgress && activatedPanels[activatedPanels.Count - 1] == other)
		{
			if (currentColor == other.tileColor || other.tileColor == TileColor.None)
			{
				CheckSolution();
			}
			else
			{
				Failure();
			}
		}
		else if (puzzleInProgress)
		{
			ResetPuzzle();
			AudioSource component = Object.Instantiate(puzzleClick, other.transform.position, Quaternion.identity).GetComponent<AudioSource>();
			component.SetPitch(component.GetPitch() - 0.5f);
		}
	}

	public void Unclicked()
	{
		if (puzzleInProgress && checkForHold == 0f)
		{
			Clicked(activatedPanels[activatedPanels.Count - 1]);
			if (punch == null)
			{
				punch = MonoSingleton<FistControl>.Instance.currentPunch;
			}
			punch.anim.SetTrigger("ShopTap");
		}
	}

	public void Hovered(PuzzlePanel other)
	{
		if (!puzzleInProgress)
		{
			return;
		}
		if (!activatedPanels.Contains(other))
		{
			if (Vector3.Distance(other.transform.localPosition, activatedPanels[activatedPanels.Count - 1].transform.localPosition) < (float)(other.pl.length - 3))
			{
				other.pl.DrawLine(other.transform.localPosition, activatedPanels[activatedPanels.Count - 1].transform.localPosition, currentColor);
				other.Activate(currentColor);
				activatedPanels.Add(other);
				activatedColors.Add(currentColor);
			}
		}
		else if (activatedPanels.IndexOf(other) == activatedPanels.Count - 2)
		{
			activatedPanels[activatedPanels.Count - 1].DeActivate();
			activatedPanels[activatedPanels.Count - 1].pl.Hide();
			activatedPanels.Remove(activatedPanels[activatedPanels.Count - 1]);
			activatedColors.Remove(activatedColors[activatedColors.Count - 1]);
		}
	}

	public void Success()
	{
		((Graphic)img).color = Color.green;
		puzzleInProgress = false;
		solved = true;
		backOnBlack = true;
		if (toActivate.Length != 0)
		{
			Invoke("ActivateNow", 0.5f);
		}
		Object.Instantiate(puzzleCorrect, base.transform.position, Quaternion.identity);
	}

	public void Failure()
	{
		((Graphic)img).color = Color.red;
		backOnBlack = false;
		ResetPuzzle();
		Object.Instantiate(puzzleWrong, base.transform.position, Quaternion.identity);
	}

	public void ResetPuzzle()
	{
		starts = 0;
		ends = 0;
		puzzleInProgress = false;
		activatedPanels.Clear();
		activatedColors.Clear();
		PuzzlePanel[] array = panels;
		foreach (PuzzlePanel obj in array)
		{
			obj.DeActivate();
			obj.pl.Hide();
		}
	}

	private void CheckSolution()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		ends = 0;
		for (int i = 0; i < panels.Length; i++)
		{
			if (panels[i].tileType == TileType.End)
			{
				if (!activatedPanels.Contains(panels[i]))
				{
					flag2 = true;
				}
				else
				{
					ends++;
					if (panels[i].tileColor != activatedColors[activatedPanels.IndexOf(panels[i])] && panels[i].tileColor != TileColor.None)
					{
						flag = true;
					}
				}
			}
			else if (panels[i].tileType == TileType.Fill)
			{
				if (!activatedPanels.Contains(panels[i]) && panels[i].tileColor == currentColor)
				{
					flag = true;
				}
				else if (!activatedPanels.Contains(panels[i]) && panels[i].tileColor == TileColor.None)
				{
					flag3 = true;
				}
				else if (activatedPanels.Contains(panels[i]) && panels[i].tileColor != activatedColors[activatedPanels.IndexOf(panels[i])] && panels[i].tileColor != TileColor.None)
				{
					flag = true;
				}
			}
			else if (panels[i].tileType == TileType.Pit && puzzleInProgress && activatedPanels.Contains(panels[i]) && (panels[i].tileColor == activatedColors[activatedPanels.IndexOf(panels[i])] || panels[i].tileColor == TileColor.None))
			{
				flag = true;
			}
			if (flag)
			{
				break;
			}
		}
		if (starts != ends)
		{
			Failure();
		}
		else if (!flag && !flag3 && !flag2)
		{
			Success();
		}
		else if (flag || (flag3 && !flag2))
		{
			Failure();
		}
		else if (flag2)
		{
			WhiteFlash();
		}
	}

	private void WhiteFlash()
	{
		puzzleInProgress = false;
		backOnBlack = false;
		((Graphic)img).color = Color.white;
		Object.Instantiate(puzzleClick, base.transform.position, Quaternion.identity);
	}

	private void ActivateNow()
	{
		GameObject[] array = toActivate;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}
}
