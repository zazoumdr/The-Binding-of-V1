using System;
using Discord;

[Serializable]
public struct SerializedActivityAssets
{
	public string LargeImage;

	public string LargeText;

	public ActivityAssets Deserialize()
	{
		return new ActivityAssets
		{
			LargeImage = LargeImage,
			LargeText = LargeText
		};
	}
}
