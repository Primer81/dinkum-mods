using System;
using UnityEngine;

[Serializable]
public class BuildingCustomisation
{
	public Material shopColour;

	public Texture2D[] allColours;

	public void ChangeColourId(int newId)
	{
		shopColour.mainTexture = allColours[newId];
	}
}
