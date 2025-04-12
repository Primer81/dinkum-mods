using System;
using UnityEngine;

[Serializable]
public class SeasonalMaterials
{
	public string mainTextName = "_MainTex";

	public Material material;

	public Texture summerMat;

	public Texture autumnMat;

	public Texture springMat;

	public Texture winterMat;

	public void setSeasonMat()
	{
		if (WorldManager.Instance.month == 1)
		{
			material.SetTexture(mainTextName, summerMat);
		}
		else if (WorldManager.Instance.month == 2)
		{
			material.SetTexture(mainTextName, autumnMat);
		}
		else if (WorldManager.Instance.month == 3)
		{
			if ((bool)WeatherManager.Instance && WeatherManager.Instance.IsSnowDay)
			{
				material.SetTexture(mainTextName, winterMat);
			}
			else
			{
				material.SetTexture(mainTextName, autumnMat);
			}
		}
		else if (WorldManager.Instance.month == 4)
		{
			material.SetTexture(mainTextName, springMat);
		}
	}

	public void resetToDefaultOnDestroy()
	{
		material.SetTexture("_MainTex", summerMat);
	}
}
