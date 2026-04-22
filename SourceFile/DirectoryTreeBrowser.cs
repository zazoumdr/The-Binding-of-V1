using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class DirectoryTreeBrowser<T> : MonoBehaviour
{
	[SerializeField]
	protected GameObject itemButtonTemplate;

	[SerializeField]
	protected GameObject folderButtonTemplate;

	[SerializeField]
	protected Transform itemParent;

	[SerializeField]
	protected GameObject backButton;

	[SerializeField]
	protected GameObject plusButton;

	[SerializeField]
	private TMP_Text pageText;

	private List<Action> cleanupActions = new List<Action>();

	protected IDirectoryTree<T> currentDirectory;

	protected int maxPages;

	protected int currentPage;

	protected abstract int maxPageLength { get; }

	protected abstract IDirectoryTree<T> baseDirectory { get; }

	public static FakeDirectoryTree<T> Folder(string name, List<T> files = null, List<IDirectoryTree<T>> children = null, IDirectoryTree<T> parent = null)
	{
		FakeDirectoryTree<T> fakeDirectoryTree = new FakeDirectoryTree<T>(name, files, children);
		if (children != null)
		{
			foreach (IDirectoryTree<T> child in children)
			{
				child.parent = fakeDirectoryTree;
			}
		}
		return fakeDirectoryTree;
	}

	private void Awake()
	{
		currentDirectory = baseDirectory;
		Rebuild();
	}

	public int PageOf(int index)
	{
		return Mathf.CeilToInt(index / maxPageLength);
	}

	public void SetPage(int target)
	{
		currentPage = Mathf.Clamp(target, 0, maxPages - 1);
		Rebuild(setToPageZero: false);
	}

	public void NextPage()
	{
		SetPage(currentPage + 1);
	}

	public void PreviousPage()
	{
		SetPage(currentPage - 1);
	}

	public void StepUp()
	{
		currentDirectory = currentDirectory.parent ?? currentDirectory;
		Rebuild();
	}

	public void StepDown(IDirectoryTree<T> dir)
	{
		currentDirectory = dir;
		Rebuild();
	}

	public void GoToBase()
	{
		if (currentDirectory != baseDirectory)
		{
			currentDirectory = baseDirectory;
			Rebuild();
		}
	}

	public virtual void Rebuild(bool setToPageZero = true)
	{
		if (setToPageZero)
		{
			currentPage = 0;
		}
		currentDirectory.Refresh();
		int num = maxPageLength;
		if ((bool)backButton)
		{
			bool flag = currentDirectory.parent != null;
			backButton.SetActive(flag);
			if (flag && backButton.transform.IsChildOf(itemParent))
			{
				num--;
			}
		}
		foreach (Action cleanupAction in cleanupActions)
		{
			cleanupAction?.Invoke();
		}
		cleanupActions.Clear();
		List<IDirectoryTree<T>> list = currentDirectory.children.Skip(currentPage * num).Take(num).ToList();
		int num2 = 0;
		foreach (IDirectoryTree<T> item in list)
		{
			Action action = BuildDirectory(item, num2++);
			if (action != null)
			{
				cleanupActions.Add(action);
			}
		}
		List<T> list2 = currentDirectory.files.Skip(currentPage * num - currentDirectory.children.Count()).Take(num - list.Count).ToList();
		num2 = 0;
		foreach (T item2 in list2)
		{
			Action action2 = BuildLeaf(item2, num2++);
			if (action2 != null)
			{
				cleanupActions.Add(action2);
			}
		}
		int num3 = currentDirectory.children.Count() + currentDirectory.files.Count();
		if ((bool)plusButton)
		{
			num3++;
			plusButton.transform.SetAsLastSibling();
			if (list.Count + list2.Count < maxPageLength)
			{
				plusButton.SetActive(value: true);
			}
			else
			{
				plusButton.SetActive(value: false);
			}
		}
		maxPages = Mathf.CeilToInt((float)num3 / (float)num);
		pageText.text = $"{currentPage + 1}/{maxPages}";
	}

	protected abstract Action BuildLeaf(T item, int indexInPage);

	protected virtual Action BuildDirectory(IDirectoryTree<T> folder, int indexInPage)
	{
		GameObject btn = UnityEngine.Object.Instantiate(folderButtonTemplate, itemParent, worldPositionStays: false);
		((UnityEventBase)(object)btn.GetComponent<Button>().onClick).RemoveAllListeners();
		((UnityEvent)(object)btn.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
		{
			StepDown(folder);
		});
		btn.GetComponentInChildren<TMP_Text>().text = folder.name;
		btn.SetActive(value: true);
		return delegate
		{
			UnityEngine.Object.Destroy(btn);
		};
	}
}
