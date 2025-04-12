using System.Collections;
using UnityEngine;

public class RandomScaleOverTime : MonoBehaviour
{
	public float maxScale = 1.01f;

	public float minScale = 0.98f;

	private float xrotation = 4f;

	private void OnEnable()
	{
		StartCoroutine(randomiseScaleOverTime());
	}

	public IEnumerator randomiseScaleOverTime()
	{
		float num = Random.Range(minScale, maxScale);
		Vector3 targetScale = new Vector3(num, num, num);
		num = Random.Range(minScale, maxScale);
		Vector3 startingScale = new Vector3(num, num, num);
		float lerpAmount = 0f;
		float xRotation = Random.Range(0f - xrotation, xrotation);
		Quaternion startingRotation = base.transform.localRotation;
		Quaternion currentRotation = base.transform.localRotation;
		while (true)
		{
			yield return null;
			if (lerpAmount < 1f)
			{
				lerpAmount += Time.deltaTime * Random.Range(4.5f, 5f);
				base.transform.localScale = Vector3.Lerp(startingScale, targetScale, lerpAmount);
				base.transform.localRotation = Quaternion.Lerp(currentRotation, startingRotation * Quaternion.Euler(xRotation, (0f - xRotation) / 3.3f, xRotation / 2f), lerpAmount);
				continue;
			}
			lerpAmount = 0f;
			startingScale = new Vector3(base.transform.localScale.x, base.transform.localScale.x, base.transform.localScale.x);
			num = Random.Range(minScale, maxScale);
			xRotation = Random.Range(0f - xrotation, xrotation);
			currentRotation = base.transform.localRotation;
			targetScale = new Vector3(num, num, num);
		}
	}
}
