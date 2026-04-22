using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CustomMusicPlayer : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup panelGroup;

	[SerializeField]
	private Text panelText;

	[SerializeField]
	private Image panelIcon;

	[SerializeField]
	private CustomMusicPlaylistEditor playlistEditor;

	[SerializeField]
	private Sprite defaultIcon;

	public AudioSource source;

	public float panelApproachTime;

	public float panelStayTime;

	private System.Random random = new System.Random();

	private bool stopped;

	public Dictionary<string, AudioClip> fileClipCache = new Dictionary<string, AudioClip>();

	public void OnEnable()
	{
		StartPlaylist();
	}

	public void StartPlaylist()
	{
		if (playlistEditor.playlist.Count < 1)
		{
			Debug.LogError("No songs in playlist, somehow. Not starting playlist routine...");
		}
		else
		{
			StartCoroutine(PlaylistRoutine());
		}
	}

	public void StopPlaylist()
	{
		stopped = true;
	}

	private IEnumerator ShowPanelRoutine(Playlist.SongMetadata song)
	{
		panelText.text = song.displayName.ToUpper();
		panelIcon.sprite = ((song.icon != null) ? song.icon : defaultIcon);
		float time = 0f;
		while (time < panelApproachTime)
		{
			time += Time.deltaTime;
			panelGroup.alpha = time / panelApproachTime;
			yield return null;
		}
		panelGroup.alpha = 1f;
		yield return new WaitForSecondsRealtime(panelStayTime);
		time = panelApproachTime;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			panelGroup.alpha = time / panelApproachTime;
			yield return null;
		}
		panelGroup.alpha = 0f;
	}

	private IEnumerator PlaylistRoutine()
	{
		WaitUntil songFinished = new WaitUntil(() => Application.isFocused && !source.isPlaying);
		Playlist.SongIdentifier lastSong = null;
		bool first = true;
		Playlist playlist = playlistEditor.playlist;
		IEnumerable<Playlist.SongIdentifier> currentOrder = (playlist.shuffled ? new DeckShuffled<Playlist.SongIdentifier>(playlist.ids).AsEnumerable() : playlist.ids.AsEnumerable());
		if (playlist.loopMode == Playlist.LoopMode.LoopOne)
		{
			currentOrder = currentOrder.Skip(playlist.selected).Take(1);
		}
		while (!stopped)
		{
			if (currentOrder is DeckShuffled<Playlist.SongIdentifier> deckShuffled)
			{
				deckShuffled.Reshuffle();
			}
			foreach (Playlist.SongIdentifier id in currentOrder)
			{
				Playlist.SongMetadata songMetadata = playlistEditor.GetSongMetadata(id);
				if (id != lastSong)
				{
					StartCoroutine(ShowPanelRoutine(songMetadata));
				}
				lastSong = id;
				if (id.type == Playlist.SongIdentifier.IdentifierType.File)
				{
					FileInfo fileInfo = new FileInfo(id.path);
					AudioType audioType = CustomMusicFileBrowser.extensionTypeDict[fileInfo.Extension.ToLower()];
					UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(id.path).AbsoluteUri, audioType);
					try
					{
						DownloadHandler downloadHandler = request.downloadHandler;
						DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)(object)((downloadHandler is DownloadHandlerAudioClip) ? downloadHandler : null);
						handler.streamAudio = true;
						request.SendWebRequest();
						yield return request;
						source.clip = handler.audioClip;
						source.Play(tracked: true);
						yield return songFinished;
						UnityEngine.Object.Destroy((UnityEngine.Object)(object)handler.audioClip);
					}
					finally
					{
						((IDisposable)request)?.Dispose();
					}
				}
				if (id.type == Playlist.SongIdentifier.IdentifierType.Addressable)
				{
					AsyncOperationHandle<SoundtrackSong> handle = Addressables.LoadAssetAsync<SoundtrackSong>((object)id.path);
					yield return handle;
					SoundtrackSong song = handle.Result;
					if (first)
					{
						source.clip = song.introClip;
						source.Play(tracked: true);
						yield return songFinished;
					}
					int clipsPlayed = 0;
					foreach (AudioClip clip in song.clips)
					{
						source.clip = clip;
						source.Play(tracked: true);
						yield return songFinished;
						clipsPlayed++;
						if (playlist.loopMode != Playlist.LoopMode.LoopOne && song.maxClipsIfNotRepeating > 0 && clipsPlayed >= song.maxClipsIfNotRepeating)
						{
							break;
						}
					}
					Addressables.Release<SoundtrackSong>(handle);
				}
				first = false;
			}
		}
	}
}
