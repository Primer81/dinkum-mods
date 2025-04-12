using System.Collections;
using UnityEngine;

public class RotateSlowly : MonoBehaviour
{
	public float rotationSpeed = 3f;

	private void OnEnable()
	{
		StartCoroutine(RotateObject());
	}

	private IEnumerator RotateObject()
	{
		while (true)
		{
			float yAngle = rotationSpeed * Time.deltaTime;
			base.transform.Rotate(0f, yAngle, 0f);
			yield return null;
		}
	}
}
