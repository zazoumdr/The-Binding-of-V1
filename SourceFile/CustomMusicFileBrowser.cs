using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class CustomMusicFileBrowser : DirectoryTreeBrowser<FileInfo>
{
	[SerializeField]
	private CyberGrindSettingsNavigator navigator;

	[SerializeField]
	private CustomMusicPlaylistEditor playlistEditorLogic;

	[SerializeField]
	private GameObject playlistEditor;

	[SerializeField]
	private GameObject loadingPrefab;

	[SerializeField]
	private Sprite defaultIcon;

	private AudioClip selectedClip;

	public static Dictionary<string, AudioType> extensionTypeDict = new Dictionary<string, AudioType>
	{
		{
			".wav",
			AudioType.WAV
		},
		{
			".mp3",
			AudioType.MPEG
		},
		{
			".ogg",
			AudioType.OGGVORBIS
		},
		{
			".flac",
			(AudioType)7
		}
	};

	private AudioClip currentSong;

	protected override int maxPageLength => 4;

	protected override IDirectoryTree<FileInfo> baseDirectory => new FileDirectoryTree(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CyberGrind", "Music"));

	protected override Action BuildLeaf(FileInfo file, int indexInPage)
	{
		if (extensionTypeDict.ContainsKey(file.Extension.ToLower()))
		{
			GameObject go = UnityEngine.Object.Instantiate(itemButtonTemplate, itemParent, worldPositionStays: false);
			CustomContentButton component = go.GetComponent<CustomContentButton>();
			((UnityEvent)(object)component.button.onClick).AddListener((UnityAction)delegate
			{
				int count = playlistEditorLogic.playlist.Count;
				int page = playlistEditorLogic.PageOf(count);
				playlistEditorLogic.playlist.Add(new Playlist.SongIdentifier(file.FullName, Playlist.SongIdentifier.IdentifierType.File));
				playlistEditorLogic.SetPage(page);
				playlistEditorLogic.Select(count);
				navigator.GoToNoMenu(playlistEditor);
			});
			component.text.text = file.Name;
			component.icon.sprite = defaultIcon;
			if ((bool)(UnityEngine.Object)(object)component.costText)
			{
				component.costText.text = "";
			}
			go.SetActive(value: true);
			return delegate
			{
				UnityEngine.Object.Destroy(go);
			};
		}
		return null;
	}
}
