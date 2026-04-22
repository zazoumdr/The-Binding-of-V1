using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CustomMusicPlaylistEditor : DirectoryTreeBrowser<Playlist.SongIdentifier>
{
	[SerializeField]
	private CustomMusicSoundtrackBrowser browser;

	[SerializeField]
	private Sprite defaultIcon;

	[SerializeField]
	private Sprite loopSprite;

	[SerializeField]
	private Sprite loopOnceSprite;

	[Header("UI Elements")]
	[SerializeField]
	private Image loopModeImage;

	[SerializeField]
	private Image shuffleImage;

	[SerializeField]
	private RectTransform selectedControls;

	[SerializeField]
	private List<Transform> anchors;

	public Playlist playlist = new Playlist();

	private Coroutine moveControlsRoutine;

	private Dictionary<Transform, Coroutine> changeAnchorRoutines = new Dictionary<Transform, Coroutine>();

	private List<Transform> buttons = new List<Transform>();

	private Dictionary<Playlist.SongIdentifier, Playlist.SongMetadata> metadataDict = new Dictionary<Playlist.SongIdentifier, Playlist.SongMetadata>();

	protected override int maxPageLength => anchors.Count;

	protected override IDirectoryTree<Playlist.SongIdentifier> baseDirectory => new FakeDirectoryTree<Playlist.SongIdentifier>("Songs", playlist.ids);

	private Playlist.SongIdentifier selectedSongId => playlist.ids[playlist.selected];

	private CustomContentButton currentButton => buttons.ElementAtOrDefault(playlist.selected % maxPageLength)?.GetComponent<CustomContentButton>();

	public Playlist.SongMetadata GetSongMetadata(Playlist.SongIdentifier id)
	{
		if (metadataDict.TryGetValue(id, out var value))
		{
			return value;
		}
		Playlist.SongMetadata songMetadata = null;
		songMetadata = id.type switch
		{
			Playlist.SongIdentifier.IdentifierType.Addressable => GetSongMetadataFromAddressable(id), 
			Playlist.SongIdentifier.IdentifierType.File => GetSongMetadataFromFilepath(id), 
			_ => throw new ArgumentException($"Could not fetch matadata: SongIdentifier '{id.path}' has invalid type '{id.type}'."), 
		};
		metadataDict.Add(id, songMetadata);
		return songMetadata;
	}

	private Playlist.SongMetadata GetSongMetadataFromAddressable(Playlist.SongIdentifier id)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		AsyncOperationHandle<SoundtrackSong> val = ((AssetReferenceT<SoundtrackSong>)new AssetReferenceSoundtrackSong(id.path)).LoadAssetAsync();
		val.WaitForCompletion();
		return new Playlist.SongMetadata(val.Result.songName, val.Result.icon, val.Result.maxClipsIfNotRepeating);
	}

	private Playlist.SongMetadata GetSongMetadataFromFilepath(Playlist.SongIdentifier id)
	{
		return new Playlist.SongMetadata(new FileInfo(id.path).Name, defaultIcon);
	}

	public void SavePlaylist()
	{
		File.WriteAllText(Playlist.currentPath, JsonConvert.SerializeObject((object)playlist));
	}

	public void LoadPlaylist()
	{
		Debug.Log("Loading Playlist");
		Playlist playlist = null;
		using (StreamReader streamReader = new StreamReader(File.Open(Playlist.currentPath, FileMode.OpenOrCreate)))
		{
			playlist = JsonConvert.DeserializeObject<Playlist>(streamReader.ReadToEnd());
		}
		if (playlist == null)
		{
			Debug.Log("No saved playlist found at " + Playlist.currentPath + ". Creating default...");
			foreach (AssetReferenceSoundtrackSong item in browser.rootFolder)
			{
				this.playlist.Add(new Playlist.SongIdentifier(((AssetReference)item).AssetGUID, Playlist.SongIdentifier.IdentifierType.Addressable));
			}
		}
		else
		{
			this.playlist = playlist;
			currentDirectory = baseDirectory;
			Rebuild();
		}
		Rebuild();
	}

	public void Remove()
	{
		playlist.Remove(playlist.selected);
		if (playlist.selected >= playlist.ids.Count)
		{
			Select(playlist.Count - 1);
		}
		Rebuild(setToPageZero: false);
	}

	public void MoveUp()
	{
		Move(-1);
	}

	public void MoveDown()
	{
		Move(1);
	}

	public void Move(int amount)
	{
		int num = playlist.selected % maxPageLength;
		int index = num + amount;
		bool flag = PageOf(playlist.selected) == PageOf(playlist.selected + amount);
		if (playlist.selected + amount >= 0 && playlist.selected + amount < playlist.ids.Count)
		{
			playlist.Swap(playlist.selected, playlist.selected + amount);
			if (flag)
			{
				ChangeAnchorOf(buttons[num], anchors[index]);
				ChangeAnchorOf(selectedControls, anchors[index]);
				ChangeAnchorOf(buttons[index], anchors[num]);
				CustomContentButton customContentButton = currentButton;
				buttons.RemoveAt(num);
				buttons.Insert(index, customContentButton.transform);
				Select(playlist.selected + amount, rebuild: false);
			}
			else
			{
				selectedControls.gameObject.SetActive(value: false);
				Select(playlist.selected + amount);
			}
		}
	}

	public void ChangeAnchorOf(Transform obj, Transform anchor, float time = 0.15f)
	{
		if (changeAnchorRoutines.ContainsKey(obj))
		{
			if (changeAnchorRoutines[obj] != null)
			{
				StopCoroutine(changeAnchorRoutines[obj]);
			}
			changeAnchorRoutines.Remove(obj);
		}
		changeAnchorRoutines.Add(obj, StartCoroutine(ChangeAnchorOverTime()));
		IEnumerator ChangeAnchorOverTime()
		{
			float t = 0f;
			_ = obj.position;
			while (t < time && time > 0f)
			{
				obj.position = Vector3.MoveTowards(obj.position, anchor.position, Time.deltaTime * 2f);
				if (Vector3.Distance(obj.position, anchor.position) <= Mathf.Epsilon)
				{
					break;
				}
				yield return null;
			}
			obj.position = anchor.position;
		}
	}

	public void ToggleLoopMode()
	{
		SetLoopMode((playlist.loopMode == Playlist.LoopMode.Loop) ? Playlist.LoopMode.LoopOne : Playlist.LoopMode.Loop);
	}

	private void SetLoopMode(Playlist.LoopMode mode)
	{
		playlist.loopMode = mode;
		loopModeImage.sprite = ((playlist.loopMode == Playlist.LoopMode.Loop) ? loopSprite : loopOnceSprite);
	}

	public void ToggleShuffle()
	{
		SetShuffle(!playlist.shuffled);
	}

	private void SetShuffle(bool shuffle)
	{
		playlist.shuffled = shuffle;
		((Graphic)shuffleImage).color = (shuffle ? Color.white : Color.gray);
	}

	public void Select(int newIndex, bool rebuild = true)
	{
		if (newIndex < 0 || newIndex >= playlist.Count)
		{
			Debug.LogWarning("Attempted to set current index outside bounds of playlist");
			return;
		}
		bool num = PageOf(newIndex) == currentPage;
		if ((bool)currentButton)
		{
			((Graphic)currentButton.border).color = Color.white;
			if ((UnityEngine.Object)(object)currentButton.iconInset != null)
			{
				((Graphic)currentButton.iconInset).color = Color.white;
			}
		}
		int selected = playlist.selected;
		playlist.selected = newIndex;
		if (PageOf(selected) < PageOf(newIndex))
		{
			ChangeAnchorOf(selectedControls, anchors.First(), 0f);
		}
		else if (PageOf(selected) > PageOf(newIndex))
		{
			ChangeAnchorOf(selectedControls, anchors.Last(), 0f);
		}
		if ((bool)currentButton)
		{
			((Graphic)currentButton.border).color = Color.red;
			if ((UnityEngine.Object)(object)currentButton.iconInset != null)
			{
				((Graphic)currentButton.iconInset).color = Color.red;
			}
		}
		Transform transform = anchors[playlist.selected % maxPageLength];
		if (num)
		{
			selectedControls.gameObject.SetActive(value: true);
			ChangeAnchorOf(selectedControls, transform);
		}
		else
		{
			selectedControls.gameObject.SetActive(value: false);
			selectedControls.transform.position = transform.position;
		}
		if (rebuild)
		{
			Rebuild(setToPageZero: false);
		}
	}

	public override void Rebuild(bool setToPageZero = true)
	{
		foreach (KeyValuePair<Transform, Coroutine> changeAnchorRoutine in changeAnchorRoutines)
		{
			if (changeAnchorRoutine.Value != null)
			{
				StopCoroutine(changeAnchorRoutine.Value);
			}
		}
		changeAnchorRoutines.Clear();
		buttons.Clear();
		base.Rebuild(setToPageZero);
		if (buttons.Count < maxPageLength)
		{
			ChangeAnchorOf(plusButton.transform, anchors[buttons.Count], 0f);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
	}

	protected override Action BuildLeaf(Playlist.SongIdentifier id, int currentIndex)
	{
		Playlist.SongMetadata songMetadata = GetSongMetadata(id);
		GameObject go = UnityEngine.Object.Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
		CustomContentButton contentButton = go.GetComponent<CustomContentButton>();
		contentButton.text.text = songMetadata.displayName;
		contentButton.icon.sprite = ((songMetadata.icon != null) ? songMetadata.icon : defaultIcon);
		go.SetActive(value: true);
		ChangeAnchorOf(go.transform, anchors[currentIndex], 0f);
		buttons.Add(go.transform);
		if (PageOf(playlist.selected) == currentPage && contentButton == currentButton)
		{
			((Graphic)contentButton.border).color = Color.red;
			if ((UnityEngine.Object)(object)currentButton.iconInset != null)
			{
				((Graphic)currentButton.iconInset).color = Color.red;
			}
			selectedControls.gameObject.SetActive(value: true);
			ChangeAnchorOf(selectedControls, anchors[currentIndex]);
			return delegate
			{
				selectedControls.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(go);
			};
		}
		((UnityEvent)(object)contentButton.button.onClick).AddListener((UnityAction)delegate
		{
			Select(buttons.IndexOf(contentButton.transform) + currentPage * maxPageLength);
		});
		return delegate
		{
			UnityEngine.Object.Destroy(go);
		};
	}

	private void Start()
	{
		//IL_0009: Expected O, but got Unknown
		try
		{
			LoadPlaylist();
		}
		catch (JsonReaderException ex)
		{
			JsonReaderException ex2 = ex;
			Debug.LogError("Error loading Playlist.json: '" + ((Exception)(object)ex2).Message + "'. Recreating file.");
			File.Delete(Playlist.currentPath);
			LoadPlaylist();
		}
		Select(playlist.selected);
		SetLoopMode(playlist.loopMode);
		SetShuffle(playlist.shuffled);
		playlist.OnChanged += SavePlaylist;
	}

	private void OnDestroy()
	{
		playlist.OnChanged -= SavePlaylist;
	}
}
