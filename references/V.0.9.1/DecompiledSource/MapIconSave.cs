using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal class MapIconSave
{
	public float[] iconXPos;

	public float[] iconYPos;

	public int[] iconId;

	public void saveIcons()
	{
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		List<int> list3 = new List<int>();
		for (int i = 0; i < RenderMap.Instance.mapIcons.Count; i++)
		{
			if (!SkipSaveMapIcon(RenderMap.Instance.mapIcons[i]))
			{
				list.Add(RenderMap.Instance.mapIcons[i].PointingAtPosition.x);
				list2.Add(RenderMap.Instance.mapIcons[i].PointingAtPosition.z);
				list3.Add(RenderMap.Instance.mapIcons[i].IconId);
			}
		}
		iconXPos = list.ToArray();
		iconYPos = list2.ToArray();
		iconId = list3.ToArray();
	}

	public void LoadPlayerPlacedIcons()
	{
		for (int i = 0; i < iconId.Length; i++)
		{
			if (iconId[i] <= 7)
			{
				NetworkMapSharer.Instance.ServerPlaceMarkerOnMap(new Vector2(iconXPos[i] / 8f, iconYPos[i] / 8f), iconId[i]);
			}
		}
	}

	private bool SkipSaveMapIcon(mapIcon iconToCheck)
	{
		if (iconToCheck.mapIconLevelIndex != 0)
		{
			return true;
		}
		if (iconToCheck.CurrentIconType != 0)
		{
			return true;
		}
		return false;
	}
}
