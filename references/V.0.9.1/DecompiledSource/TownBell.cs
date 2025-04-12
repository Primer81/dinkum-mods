using System.Collections;
using UnityEngine;

public class TownBell : MonoBehaviour
{
	public static TownBell Instance;

	public Animator BellAnim;

	public Transform bellTransform;

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		Instance = null;
	}

	public void RingBell()
	{
		BellAnim.SetTrigger("RingBell");
		StartCoroutine(PlayAttackParticlesOnBellPos());
	}

	private IEnumerator PlayAttackParticlesOnBellPos()
	{
		float timer8;
		for (timer8 = 0.8f; timer8 > 0f; timer8 -= Time.deltaTime)
		{
			ParticleManager.manage.emitAttackParticle(bellTransform.position, 1);
			timer8 -= Time.deltaTime;
			yield return null;
			timer8 -= Time.deltaTime;
			yield return null;
			timer8 -= Time.deltaTime;
			yield return null;
			timer8 -= Time.deltaTime;
			yield return null;
			timer8 -= Time.deltaTime;
			yield return null;
			timer8 -= Time.deltaTime;
			yield return null;
			timer8 -= Time.deltaTime;
			yield return null;
		}
	}
}
