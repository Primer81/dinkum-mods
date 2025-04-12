using System.Collections;
using UnityEngine;

public class StopLightStack : MonoBehaviour
{
	public Light connectedLight;

	public float origIntensity = 1f;

	public float desiredIntensity = 1f;

	private float lightRange = 5f;

	private Vector3 lightPos;

	private void Start()
	{
		connectedLight.renderMode = LightRenderMode.ForceVertex;
		lightRange = connectedLight.range;
	}

	private void OnEnable()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			LightStackManager.manage.addLight(this);
			StartCoroutine(checkDistanceAfterEnabled());
		}
		connectedLight.intensity = 0f;
		lightPos = connectedLight.transform.position;
	}

	public Vector3 GetLightPosition()
	{
		return lightPos;
	}

	private IEnumerator checkDistanceAfterEnabled()
	{
		yield return null;
		LightStackManager.manage.doLightCheck(lightPos);
	}

	private void OnDisable()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			LightStackManager.manage.removeLight(this);
			LightStackManager.manage.doLightCheck(lightPos);
		}
	}

	private void OnDestroy()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			LightStackManager.manage.removeLight(this);
		}
	}

	public void checkForCloseLights(Vector3 checkingThisPos)
	{
		float sqrMagnitude = (checkingThisPos - lightPos).sqrMagnitude;
		float num = lightRange * lightRange;
		if (!(sqrMagnitude > num))
		{
			StartCoroutine(checkForLights());
		}
	}

	private IEnumerator checkForLights()
	{
		yield return null;
		int amountOfLightsClose = LightStackManager.manage.getAmountOfLightsClose(lightPos, lightRange / 4f);
		if (amountOfLightsClose > 1)
		{
			desiredIntensity = Mathf.Clamp(origIntensity / ((float)amountOfLightsClose / 1.5f), 0f, 1f);
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.currentHour != RealWorldTimeLight.time.getSunSetTime())
			{
				connectedLight.intensity = desiredIntensity;
			}
		}
		else
		{
			desiredIntensity = origIntensity;
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.currentHour != RealWorldTimeLight.time.getSunSetTime())
			{
				connectedLight.intensity = origIntensity;
			}
		}
	}
}
