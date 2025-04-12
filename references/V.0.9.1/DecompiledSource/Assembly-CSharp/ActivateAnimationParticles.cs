using UnityEngine;

public class ActivateAnimationParticles : MonoBehaviour
{
	public ParticleSystem myPartSystem;

	public bool waterSplash;

	public ASound soundToPlayAnimation;

	public float animSpeed = 1f;

	private void Start()
	{
		if (animSpeed != 1f)
		{
			GetComponent<Animator>().SetFloat("Speed", animSpeed);
		}
	}

	public void emitParticles(int particleAmount)
	{
		if ((bool)myPartSystem)
		{
			myPartSystem.Emit(particleAmount);
		}
		if (waterSplash)
		{
			ParticleManager.manage.waterSplash(base.transform.position, 10);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.footStepWater, base.transform.position);
		}
	}

	public void waterWake()
	{
		ParticleManager.manage.waterWakePart(base.transform.position, 5);
	}

	public void playSound()
	{
		if ((bool)soundToPlayAnimation)
		{
			SoundManager.Instance.playASoundAtPoint(soundToPlayAnimation, base.transform.position);
		}
	}

	public void play2DSoundForAnimation()
	{
		if ((bool)soundToPlayAnimation)
		{
			SoundManager.Instance.play2DSound(soundToPlayAnimation);
		}
	}

	public void playAttackParticles()
	{
		ParticleManager.manage.emitAttackParticle(base.transform.position);
	}

	public void playParticleAndSound()
	{
		playSound();
		emitParticles(15);
	}
}
