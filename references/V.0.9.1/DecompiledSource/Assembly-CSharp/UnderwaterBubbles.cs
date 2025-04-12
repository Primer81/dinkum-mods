using System.Collections;
using UnityEngine;

public class UnderwaterBubbles : MonoBehaviour
{
	public ParticleSystem bubbleParts;

	private float yThreshold;

	private ParticleSystem.Particle[] particles;

	private bool isCloseAndActive;

	private void OnEnable()
	{
		isCloseAndActive = false;
		if (LicenceManager.manage.allLicences[3].getCurrentLevel() >= 2)
		{
			StartCoroutine(CheckDistance());
		}
	}

	public IEnumerator CheckDistance()
	{
		WaitForSeconds distanceCheckTimer = new WaitForSeconds(Random.Range(0.45f, 0.75f));
		while (true)
		{
			if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) <= 45f)
			{
				if (!isCloseAndActive)
				{
					isCloseAndActive = true;
					bubbleParts.gameObject.SetActive(value: true);
				}
			}
			else if (isCloseAndActive)
			{
				isCloseAndActive = false;
				bubbleParts.gameObject.SetActive(value: false);
			}
			yield return distanceCheckTimer;
		}
	}

	private void Update()
	{
		if (isCloseAndActive)
		{
			RemoveParticlesAboveY();
		}
	}

	private void RemoveParticlesAboveY()
	{
		int particleCount = bubbleParts.particleCount;
		if (particleCount == 0)
		{
			return;
		}
		particles = new ParticleSystem.Particle[particleCount];
		bubbleParts.GetParticles(particles);
		for (int i = 0; i < particleCount; i++)
		{
			if (particles[i].position.y > yThreshold)
			{
				Vector3 position = particles[i].position;
				position.y = yThreshold;
				particles[i].position = position;
			}
		}
		bubbleParts.SetParticles(particles, particleCount);
	}
}
