using System.Collections;
using UnityEngine;

public class CharBreath : MonoBehaviour
{
	public bool autoBreath;

	public Transform breathingPos;

	public static WaitForSeconds breathWait = new WaitForSeconds(4f);

	private Coroutine breathRoutine;

	private void OnEnable()
	{
		if (autoBreath)
		{
			WorldManager.Instance.changeDayEvent.AddListener(startBreathOnColdDay);
			Invoke("startBreathOnColdDay", 4f);
		}
	}

	private void OnDisable()
	{
		if (autoBreath)
		{
			WorldManager.Instance.changeDayEvent.RemoveListener(startBreathOnColdDay);
		}
	}

	public void startBreathOnColdDay()
	{
		if (breathRoutine != null)
		{
			StopCoroutine(breathRoutine);
			breathRoutine = null;
		}
		if (WorldManager.Instance.month == 3)
		{
			breathRoutine = StartCoroutine(breathAuto());
		}
	}

	private IEnumerator breathAuto()
	{
		yield return new WaitForSeconds(Random.Range(0f, 1f));
		while (true)
		{
			yield return breathWait;
			if (CameraController.control.IsCloseToCamera50(base.transform.position) && base.transform.position.y >= -10f)
			{
				ParticleManager.manage.breathParticleAtPos(breathingPos);
			}
		}
	}

	public void takeBreathAnim()
	{
		ParticleManager.manage.breathParticleAtPos(breathingPos);
	}
}
