using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;

public static class WorkshopHelper
{
	public static async Task<Item?> GetWorkshopItemInfo(ulong itemId)
	{
		return await SteamUGC.QueryFileAsync(PublishedFileId.op_Implicit(itemId));
	}

	public static async Task<Item?> DownloadWorkshopMap(ulong itemId, [CanBeNull] Action promptForUpdate = null)
	{
		Item? result = await Item.GetAsync(PublishedFileId.op_Implicit(itemId), 1800);
		if (!result.HasValue)
		{
			Debug.LogError("Failed to get workshop item info for " + itemId);
			return null;
		}
		object obj;
		Item val;
		if (!result.HasValue)
		{
			obj = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			obj = ((Item)(ref val)).Title;
		}
		Debug.Log("Title: " + (string?)obj);
		bool? flag;
		if (!result.HasValue)
		{
			flag = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			flag = ((Item)(ref val)).IsInstalled;
		}
		Debug.Log($"IsInstalled: {flag}");
		bool? flag2;
		if (!result.HasValue)
		{
			flag2 = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			flag2 = ((Item)(ref val)).IsDownloading;
		}
		Debug.Log($"IsDownloading: {flag2}");
		bool? flag3;
		if (!result.HasValue)
		{
			flag3 = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			flag3 = ((Item)(ref val)).IsDownloadPending;
		}
		Debug.Log($"IsDownloadPending: {flag3}");
		bool? flag4;
		if (!result.HasValue)
		{
			flag4 = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			flag4 = ((Item)(ref val)).IsSubscribed;
		}
		Debug.Log($"IsSubscribed: {flag4}");
		bool? flag5;
		if (!result.HasValue)
		{
			flag5 = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			flag5 = ((Item)(ref val)).NeedsUpdate;
		}
		Debug.Log($"NeedsUpdate: {flag5}");
		object obj2;
		if (!result.HasValue)
		{
			obj2 = null;
		}
		else
		{
			val = result.GetValueOrDefault();
			obj2 = ((Item)(ref val)).Description;
		}
		Debug.Log("Description: " + (string?)obj2);
		if (promptForUpdate != null)
		{
			bool? flag6;
			if (!result.HasValue)
			{
				flag6 = null;
			}
			else
			{
				val = result.GetValueOrDefault();
				flag6 = ((Item)(ref val)).NeedsUpdate;
			}
			bool? flag7 = flag6;
			if (flag7.Value)
			{
				promptForUpdate();
				return null;
			}
		}
		val = result.Value;
		if (!((Item)(ref val)).IsInstalled)
		{
			Debug.Log($"Downloading workshop map {itemId}");
			val = result.Value;
			if (!(await ((Item)(ref val)).DownloadAsync((Action<float>)null, 60, default(CancellationToken))))
			{
				Debug.LogError($"Failed to download workshop map {itemId}");
				return null;
			}
			Debug.Log($"Workshop map {itemId} downloaded successfully");
			result = await Item.GetAsync(PublishedFileId.op_Implicit(itemId), 1800);
			if (!result.HasValue)
			{
				Debug.LogError("Failed to get workshop item info for " + itemId);
				return null;
			}
		}
		else
		{
			Debug.LogWarning($"Workshop map {itemId} was already downloaded");
		}
		return result;
	}
}
