using System.Collections.Generic;
using UnityEngine;

public class LightStackManager : MonoBehaviour
{
	public static LightStackManager manage;

	private List<StopLightStack> activeLights = new List<StopLightStack>();

	public LayerMask lightCheckLayer;

	private int lightsCount;

	private void Awake()
	{
		manage = this;
	}

	public void addLight(StopLightStack lightToAdd)
	{
		if (!activeLights.Contains(lightToAdd))
		{
			activeLights.Add(lightToAdd);
			lightsCount++;
		}
	}

	public void removeLight(StopLightStack lightToRemove)
	{
		if (activeLights.Contains(lightToRemove))
		{
			activeLights.Remove(lightToRemove);
			lightsCount--;
		}
	}

	public void doLightCheck(Vector3 position)
	{
		for (int i = 0; i < lightsCount; i++)
		{
			activeLights[i].checkForCloseLights(position);
		}
	}

	public int getAmountOfLightsClose(Vector3 lightPos, float checkDistance)
	{
		int num = 0;
		for (int i = 0; i < lightsCount; i++)
		{
			if ((activeLights[i].GetLightPosition() - lightPos).sqrMagnitude <= checkDistance * checkDistance)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return 1;
		}
		return num;
	}
}
