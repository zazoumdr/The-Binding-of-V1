using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CustomMusicSoundtrackBrowser : DirectoryTreeBrowser<AssetReferenceSoundtrackSong>
{
	[Header("References")]
	[SerializeField]
	private CustomMusicPlaylistEditor playlistEditorLogic;

	[SerializeField]
	private GameObject playlistEditorPanel;

	[SerializeField]
	private CyberGrindSettingsNavigator navigator;

	[SerializeField]
	private TMP_Text songName;

	[SerializeField]
	private Image songIcon;

	[Header("Assets")]
	[SerializeField]
	private GameObject loadingPrefab;

	[SerializeField]
	private Sprite lockedLevelSprite;

	[SerializeField]
	private Sprite defaultIcon;

	[SerializeField]
	private GameObject buySound;

	public List<AssetReferenceSoundtrackSong> rootFolder = new List<AssetReferenceSoundtrackSong>();

	public List<SoundtrackFolder> levelFolders = new List<SoundtrackFolder>();

	public List<AssetReferenceSoundtrackSong> secretLevelFolder = new List<AssetReferenceSoundtrackSong>();

	public List<AssetReferenceSoundtrackSong> primeFolder = new List<AssetReferenceSoundtrackSong>();

	public List<AssetReferenceSoundtrackSong> encoreFolder = new List<AssetReferenceSoundtrackSong>();

	public List<AssetReferenceSoundtrackSong> miscFolder = new List<AssetReferenceSoundtrackSong>();

	private FakeDirectoryTree<AssetReferenceSoundtrackSong> _baseDirectory;

	private Dictionary<AssetReferenceSoundtrackSong, SoundtrackSong> referenceCache = new Dictionary<AssetReferenceSoundtrackSong, SoundtrackSong>();

	private SoundtrackSong songBeingBought;

	protected override int maxPageLength => 4;

	protected override IDirectoryTree<AssetReferenceSoundtrackSong> baseDirectory
	{
		get
		{
			if (_baseDirectory == null)
			{
				levelFolders.Add(new SoundtrackFolder("Secret Levels", secretLevelFolder));
				for (int i = 1; i <= 3; i++)
				{
					if (GameProgressSaver.GetPrime(0, i) > 0)
					{
						levelFolders.Add(new SoundtrackFolder("Prime Sanctums", primeFolder));
						break;
					}
				}
				if (GameProgressSaver.GetEncoreProgress(0) > 0)
				{
					levelFolders.Add(new SoundtrackFolder("Encores", encoreFolder));
				}
				levelFolders.Add(new SoundtrackFolder("Miscellaneous Tracks", miscFolder));
				levelFolders.Insert(0, new SoundtrackFolder("The Cyber Grind", rootFolder));
				IEnumerable<FakeDirectoryTree<AssetReferenceSoundtrackSong>> source = levelFolders.Select((SoundtrackFolder f) => new FakeDirectoryTree<AssetReferenceSoundtrackSong>(f.name, f.songs));
				_baseDirectory = DirectoryTreeBrowser<AssetReferenceSoundtrackSong>.Folder("OST", null, source.Cast<IDirectoryTree<AssetReferenceSoundtrackSong>>().ToList());
			}
			return _baseDirectory;
		}
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		Rebuild();
	}

	private void SelectSong(string id, SoundtrackSong song)
	{
		if (song.clips.Count > 0)
		{
			int count = playlistEditorLogic.playlist.Count;
			int page = playlistEditorLogic.PageOf(count);
			playlistEditorLogic.playlist.Add(new Playlist.SongIdentifier(id, Playlist.SongIdentifier.IdentifierType.Addressable));
			playlistEditorLogic.SetPage(page);
			playlistEditorLogic.Select(count);
			navigator.GoToNoMenu(playlistEditorPanel);
		}
		else
		{
			Debug.LogWarning("Attempted to add song with no clips to playlist.");
		}
	}

	private void OnDestroy()
	{
	}

	public IEnumerator LoadSongButton(AssetReferenceSoundtrackSong reference, GameObject btn)
	{
		GameObject placeholder = UnityEngine.Object.Instantiate(loadingPrefab, itemParent, worldPositionStays: false);
		placeholder.SetActive(value: true);
		SoundtrackSong song;
		if (referenceCache.ContainsKey(reference))
		{
			yield return new WaitUntil(() => referenceCache[reference] != null || btn == null);
			if (btn == null)
			{
				UnityEngine.Object.Destroy(placeholder);
				yield break;
			}
			song = referenceCache[reference];
		}
		else
		{
			AsyncOperationHandle<SoundtrackSong> handle = ((AssetReferenceT<SoundtrackSong>)reference).LoadAssetAsync();
			referenceCache.Add(reference, null);
			yield return new WaitUntil(() => handle.IsDone || btn == null);
			if (btn == null)
			{
				UnityEngine.Object.Destroy(placeholder);
				yield return handle;
			}
			song = handle.Result;
			referenceCache[reference] = song;
			Addressables.Release<SoundtrackSong>(handle);
			if (btn == null)
			{
				yield break;
			}
		}
		UnityEngine.Object.Destroy(placeholder);
		CustomContentButton componentInChildren = btn.GetComponentInChildren<CustomContentButton>();
		((UnityEventBase)(object)componentInChildren.button.onClick).RemoveAllListeners();
		if (song.conditions.AllMet())
		{
			componentInChildren.icon.sprite = ((song.icon != null) ? song.icon : defaultIcon);
			componentInChildren.text.text = song.songName + " <color=grey>" + song.extraLevelBit + "</color>";
			componentInChildren.costText.text = "Unlocked";
			((UnityEvent)(object)componentInChildren.button.onClick).AddListener((UnityAction)delegate
			{
				SelectSong(((AssetReference)reference).AssetGUID, song);
			});
			SetActiveAll(componentInChildren.objectsToActivateIfAvailable, active: true);
			btn.SetActive(value: true);
			yield break;
		}
		SetActiveAll(componentInChildren.objectsToActivateIfAvailable, active: false);
		componentInChildren.text.text = "????????? " + song.extraLevelBit;
		componentInChildren.costText.text = song.conditions.DescribeAll();
		componentInChildren.icon.sprite = lockedLevelSprite;
		((Graphic)componentInChildren.border).color = Color.grey;
		if ((UnityEngine.Object)(object)componentInChildren.iconInset != null)
		{
			((Graphic)componentInChildren.iconInset).color = Color.grey;
		}
		((Graphic)componentInChildren.text).color = Color.grey;
		((Graphic)componentInChildren.costText).color = Color.grey;
		btn.SetActive(value: true);
	}

	protected override Action BuildLeaf(AssetReferenceSoundtrackSong reference, int indexInPage)
	{
		GameObject btn = UnityEngine.Object.Instantiate(itemButtonTemplate, itemParent, worldPositionStays: false);
		StartCoroutine(LoadSongButton(reference, btn));
		return delegate
		{
			UnityEngine.Object.Destroy(btn);
		};
	}

	private void SetActiveAll(List<GameObject> objects, bool active)
	{
		foreach (GameObject @object in objects)
		{
			@object.SetActive(active);
		}
	}
}
