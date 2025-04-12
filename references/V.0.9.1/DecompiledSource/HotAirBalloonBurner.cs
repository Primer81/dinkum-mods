using UnityEngine;

public class HotAirBalloonBurner : MonoBehaviour
{
	public ParticleSystem burnerPart;

	public AudioSource burnerSound;

	public Light burnerLight;

	private Vector3 lastPos;

	private bool burning;

	private void Start()
	{
		lastPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) < (float)NetworkNavMesh.nav.animalDistance)
		{
			if (lastPos.y < base.transform.position.y && Mathf.Abs(lastPos.y - base.transform.position.y) > 0.01f)
			{
				BurnNow();
			}
			else
			{
				StopBurn();
			}
		}
		lastPos = base.transform.position;
	}

	private void BurnNow()
	{
		burnerPart.Emit(2);
		burnerSound.volume = Mathf.Lerp(burnerSound.volume, 0.1f * SoundManager.Instance.getSoundVolumeForChange(), Time.deltaTime * 2f);
		burnerSound.pitch = Mathf.Lerp(burnerSound.pitch, 1.5f, Time.deltaTime * 2f);
		burnerLight.intensity = Mathf.Lerp(burnerLight.intensity, 10f, Time.deltaTime * 2f);
	}

	private void StopBurn()
	{
		burnerSound.volume = Mathf.Lerp(burnerSound.volume, 0f * SoundManager.Instance.getSoundVolumeForChange(), Time.deltaTime * 3f);
		burnerSound.pitch = Mathf.Lerp(burnerSound.pitch, 1f, Time.deltaTime * 3f);
		burnerLight.intensity = Mathf.Lerp(burnerLight.intensity, 0f, Time.deltaTime * 3f);
	}
}
