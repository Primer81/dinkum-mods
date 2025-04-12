using System.Collections;
using UnityEngine;

public class DroppedItemBounce : MonoBehaviour
{
	public void bounceNow()
	{
		StopAllCoroutines();
		StartCoroutine(dropBouncedropBounce());
	}

	private IEnumerator dropBouncedropBounce()
	{
		float journey = 0f;
		float duration = 0.6f * Random.Range(0.8f, 1.2f);
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.droppedItemCurve.Evaluate(time);
			float y = Mathf.LerpUnclamped(0f, 0.45f, t);
			float num = Mathf.LerpUnclamped(1f, 1.4f, t);
			float num2 = (num - 1f) / 1.5f;
			base.transform.transform.localScale = new Vector3(1f - num2, num, 1f - num2);
			base.transform.transform.localPosition = new Vector3(0f, y, 0f);
			yield return null;
		}
	}
}
