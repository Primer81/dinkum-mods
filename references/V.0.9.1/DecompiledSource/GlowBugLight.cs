using System.Collections;
using UnityEngine;

public class GlowBugLight : MonoBehaviour
{
	public Light myLight;

	public float minLight;

	public float maxLight = 4f;

	private void OnEnable()
	{
		StartCoroutine(lightGlows());
	}

	private IEnumerator lightGlows()
	{
		bool glowingUp = true;
		myLight.intensity = 0f;
		while (true)
		{
			if (glowingUp)
			{
				myLight.intensity = Mathf.Lerp(myLight.intensity, 4.5f, Time.deltaTime * 2f);
				if (myLight.intensity >= maxLight)
				{
					yield return null;
					yield return null;
					yield return null;
					yield return null;
					yield return null;
					glowingUp = false;
				}
			}
			else
			{
				myLight.intensity = Mathf.Lerp(myLight.intensity, -0.5f, Time.deltaTime * 2f);
				if (myLight.intensity <= minLight)
				{
					yield return null;
					yield return null;
					yield return null;
					yield return null;
					yield return null;
					glowingUp = true;
				}
			}
			yield return null;
		}
	}
}
