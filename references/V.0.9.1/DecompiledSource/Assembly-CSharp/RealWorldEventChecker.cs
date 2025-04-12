using System;
using System.Collections;
using UnityEngine;

public class RealWorldEventChecker : MonoBehaviour
{
	public enum TimedEvent
	{
		None,
		Chrissy
	}

	public static RealWorldEventChecker check;

	public TimedEvent currentEvent;

	public Material chrissyLightsMat;

	public Material movingLightsMat;

	private Coroutine movingLightRoutine;

	private void Awake()
	{
		check = this;
	}

	private void Start()
	{
		setCurrentEvent();
	}

	public TimedEvent getCurrentEvent()
	{
		return currentEvent;
	}

	public void setCurrentEvent()
	{
		if (DateTime.Now.Month == 12)
		{
			currentEvent = TimedEvent.Chrissy;
			StartCoroutine(chrissyLights());
		}
	}

	private IEnumerator chrissyLights()
	{
		float christmasLightTimer = 0f;
		float yDif = 0f;
		while (true)
		{
			yield return null;
			christmasLightTimer += Time.deltaTime;
			if (christmasLightTimer > 1f)
			{
				christmasLightTimer = 0f;
				yDif += 0.25f;
				if (yDif >= 1f)
				{
					yDif = 0f;
				}
				chrissyLightsMat.SetTextureOffset("_MainTex", new Vector2(0f, yDif));
			}
		}
	}

	public void startMovingLights()
	{
		if (movingLightRoutine == null)
		{
			movingLightRoutine = StartCoroutine(movingLights());
		}
	}

	private IEnumerator movingLights()
	{
		float christmasLightTimer = 0f;
		float yDif = 0f;
		while (true)
		{
			yield return null;
			christmasLightTimer += Time.deltaTime;
			if (christmasLightTimer > 0.75f)
			{
				christmasLightTimer = 0f;
				yDif += 0.125f;
				if (yDif >= 1f)
				{
					yDif = 0f;
				}
				movingLightsMat.SetTextureOffset("_MainTex", new Vector2(yDif, 0f));
			}
		}
	}
}
