using System.Collections;
using UnityEngine;

public class DisableAnimatorAtDistance : MonoBehaviour
{
	public Animator disableAnim;

	private ChangeAnimatorOffsetOnStart setOffset;

	private void Awake()
	{
		setOffset = GetComponent<ChangeAnimatorOffsetOnStart>();
	}

	private void OnEnable()
	{
		disableAnim.enabled = false;
		StartCoroutine(distanceDisabled());
	}

	public IEnumerator distanceDisabled()
	{
		WaitForSeconds distanceCheckTimer = new WaitForSeconds(Random.Range(1.8f, 2.3f));
		while (true)
		{
			if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) <= (float)NetworkNavMesh.nav.animalDistance)
			{
				if (!disableAnim.enabled)
				{
					disableAnim.enabled = true;
					if ((bool)setOffset)
					{
						setOffset.ChangeOffset();
					}
				}
			}
			else if (disableAnim.enabled)
			{
				disableAnim.enabled = false;
			}
			yield return distanceCheckTimer;
		}
	}
}
