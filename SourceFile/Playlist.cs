using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class Playlist
{
	public enum LoopMode
	{
		Loop,
		LoopOne
	}

	public class SongMetadata
	{
		public string displayName;

		public Sprite icon;

		public int maxClips;

		public SongMetadata(string displayName, Sprite icon, int maxClips = 1)
		{
			this.displayName = displayName;
			this.icon = icon;
			this.maxClips = maxClips;
		}
	}

	public class SongIdentifier
	{
		public enum IdentifierType
		{
			Addressable,
			File
		}

		public string path;

		public IdentifierType type;

		public SongIdentifier(string id, IdentifierType type)
		{
			path = id;
			this.type = type;
		}

		public static implicit operator SongIdentifier(string id)
		{
			return new SongIdentifier(id, IdentifierType.Addressable);
		}

		public override bool Equals(object obj)
		{
			SongIdentifier songIdentifier = obj as SongIdentifier;
			if (songIdentifier?.path == path)
			{
				return songIdentifier?.type == type;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (-1056084179 * -1521134295 + EqualityComparer<string>.Default.GetHashCode(path)) * -1521134295 + type.GetHashCode();
		}
	}

	[JsonProperty]
	private List<SongIdentifier> _ids = new List<SongIdentifier>();

	[JsonProperty]
	private LoopMode _loopMode = LoopMode.LoopOne;

	[JsonProperty]
	private int _selected;

	[JsonProperty]
	private bool _shuffled = true;

	public static DirectoryInfo directory => Directory.CreateDirectory(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Preferences", "Playlists"));

	public static string currentPath
	{
		get
		{
			string text = Path.Combine(directory.Parent.FullName, "Playlist.json");
			string text2 = Path.Combine(directory.FullName, $"slot{GameProgressSaver.currentSlot + 1}.json");
			if (File.Exists(text) && !File.Exists(text2))
			{
				File.Move(text, text2);
			}
			return text2;
		}
	}

	public List<SongIdentifier> ids => _ids;

	public LoopMode loopMode
	{
		get
		{
			return _loopMode;
		}
		set
		{
			_loopMode = value;
			this.OnChanged?.Invoke();
		}
	}

	public int selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			this.OnChanged?.Invoke();
		}
	}

	public bool shuffled
	{
		get
		{
			return _shuffled;
		}
		set
		{
			_shuffled = value;
			this.OnChanged?.Invoke();
		}
	}

	public int Count => _ids.Count;

	public event Action OnChanged;

	public Playlist()
	{
	}

	public Playlist(IEnumerable<SongIdentifier> passedIds)
	{
		_ids.AddRange(passedIds);
	}

	public void Add(SongIdentifier id)
	{
		_ids.Add(id);
		this.OnChanged?.Invoke();
	}

	public void Remove(int index)
	{
		if (_ids.Count <= 1)
		{
			Debug.LogWarning("Attempted to remove last song from playlist!");
			return;
		}
		if (index < 0 && index > _ids.Count - 1)
		{
			Debug.LogError($"Attempted to remove index '{index}' from playlist, which is out of bounds. (0..{_ids.Count - 1})");
			return;
		}
		_ids.RemoveAt(index);
		this.OnChanged?.Invoke();
	}

	public void Swap(int index1, int index2)
	{
		SongIdentifier value = _ids[index1];
		_ids[index1] = _ids[index2];
		_ids[index2] = value;
		this.OnChanged?.Invoke();
	}
}
