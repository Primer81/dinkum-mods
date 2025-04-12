using UnityEngine;

public class AnimalAttackParticlesAndNoise : MonoBehaviour
{
	public ASound attackSound;

	public ASound altAttackSound;

	public ParticleSystem altAttackSwoosh;

	public Transform particlePosition;

	public ParticleSystem localParticleSystem;

	public void playAttackEffects()
	{
		if ((bool)attackSound)
		{
			if ((bool)particlePosition)
			{
				SoundManager.Instance.playASoundAtPoint(attackSound, particlePosition.position);
			}
			else
			{
				SoundManager.Instance.playASoundAtPoint(attackSound, base.transform.position);
			}
		}
		if ((bool)particlePosition)
		{
			ParticleManager.manage.emitAttackParticle(particlePosition.position);
		}
	}

	public void playLocalAttackParticles()
	{
		localParticleSystem.Emit(10);
	}

	public void playAltAttackSound()
	{
		if ((bool)altAttackSound)
		{
			SoundManager.Instance.playASoundAtPoint(altAttackSound, base.transform.position);
		}
		if ((bool)altAttackSwoosh)
		{
			altAttackSwoosh.Emit(5);
		}
	}
}
