using System.Collections;
using UnityEngine;

public class EventFireworks : MonoBehaviour
{
	public ParticleSystem fireworks;

	public ASound fireworksSound;

	public Light lightFlash;

	public Color[] lightFlashColours;

	private WaitForSeconds fireworksWaitBeforeExplode = new WaitForSeconds(5f);

	private IEnumerator startFireworks()
	{
		float fireworksDelay = 0f;
		while (true)
		{
			if (!RealWorldTimeLight.time.underGround)
			{
				if (fireworksDelay <= 0f)
				{
					StartCoroutine(fireworksSoundDelay());
					base.transform.localPosition = new Vector3(0f, 0f, Random.Range(120, 220));
					fireworks.Emit(1);
					fireworksDelay = Random.Range(1f, 5f);
				}
				else
				{
					fireworksDelay -= Time.deltaTime;
				}
			}
			yield return null;
		}
	}

	private IEnumerator fireworksSoundDelay()
	{
		yield return fireworksWaitBeforeExplode;
		SoundManager.Instance.playASoundAtPoint(fireworksSound, CameraController.control.transform.position);
		float lightFlashTime2 = 0f;
		lightFlash.intensity = 0f;
		lightFlash.color = lightFlashColours[Random.Range(0, lightFlashColours.Length)];
		for (; lightFlashTime2 < 0.5f; lightFlashTime2 += Time.deltaTime)
		{
			lightFlash.intensity = Mathf.Lerp(0f, 0.25f, lightFlashTime2 * 2f);
			yield return null;
		}
		for (lightFlashTime2 = 0f; lightFlashTime2 < 1f; lightFlashTime2 += Time.deltaTime)
		{
			lightFlash.intensity = Mathf.Lerp(0.25f, 0f, lightFlashTime2);
			yield return null;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(startFireworks());
	}
}
