using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace NewBlood;

internal static class ResourceLoader
{
	public static IEnumerator LoadAudioClip(string path, AudioClipLoadType loadType, Action<AudioClip> onCompleted)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return LoadAudioClip(path, loadType, AudioType.UNKNOWN, onCompleted, delegate(Action<AudioClip> _, AudioClip clip)
		{
			onCompleted(clip);
		});
	}

	public static IEnumerator LoadAudioClip(string path, AudioClipLoadType loadType, AudioType audioType, Action<AudioClip> onCompleted)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return LoadAudioClip(path, loadType, audioType, onCompleted, delegate(Action<AudioClip> _, AudioClip clip)
		{
			onCompleted(clip);
		});
	}

	public static IEnumerator LoadAudioClip<TState>(string path, AudioClipLoadType loadType, TState state, Action<TState, AudioClip> onCompleted)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LoadAudioClip(path, loadType, AudioType.UNKNOWN, state, onCompleted);
	}

	public static IEnumerator LoadAudioClip<TState>(string path, AudioClipLoadType loadType, AudioType audioType, TState state, Action<TState, AudioClip> onCompleted)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		Uri fileUri = GetFileUri(path);
		UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUri, audioType);
		DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)request.downloadHandler;
		if ((int)loadType != 1)
		{
			if ((int)loadType == 2)
			{
				handler.streamAudio = true;
			}
		}
		else
		{
			handler.compressed = true;
		}
		UnityWebRequestAsyncOperation val = request.SendWebRequest();
		DisposeAndThrowIfRequestFailed(request);
		if ((int)loadType == 2)
		{
			try
			{
				onCompleted(state, handler.audioClip);
			}
			catch
			{
				request.Dispose();
				throw;
			}
		}
		yield return val;
		DisposeAndThrowIfRequestFailed(request);
		if ((int)loadType == 2)
		{
			request.Dispose();
			yield break;
		}
		UnityWebRequest val2 = request;
		try
		{
			onCompleted(state, handler.audioClip);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private static void DisposeAndThrowIfRequestFailed(UnityWebRequest request)
	{
		if (!request.isHttpError || !request.isNetworkError || !request.isDone)
		{
			return;
		}
		Exception exceptionForWebRequest = GetExceptionForWebRequest(request);
		request.Dispose();
		throw exceptionForWebRequest;
	}

	private static Exception GetExceptionForWebRequest(UnityWebRequest request)
	{
		if (request.responseCode == 404)
		{
			return new FileNotFoundException(null, request.uri.LocalPath);
		}
		return new Exception(request.error);
	}

	private static Uri GetFileUri(string path)
	{
		path = Path.GetFullPath(path);
		path = path.Replace('\\', '/');
		path = Uri.EscapeUriString(path);
		return new UriBuilder(Uri.UriSchemeFile, string.Empty, 0, path).Uri;
	}
}
