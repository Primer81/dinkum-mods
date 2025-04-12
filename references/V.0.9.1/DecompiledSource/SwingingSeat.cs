using System.Collections;
using UnityEngine;

public class SwingingSeat : MonoBehaviour
{
	public GameObject someoneInSeat;

	public Transform swingRotation;

	public Animator swingAnim;

	private float swingSpeed;

	private float backSpeed;

	private float forwardSpeed;

	private void OnEnable()
	{
		StartCoroutine(swingTheSwing());
	}

	private IEnumerator swingTheSwing()
	{
		while (true)
		{
			yield return null;
			if (!someoneInSeat.activeInHierarchy)
			{
				if (RealWorldTimeLight.time.currentMinute % 2 == 0)
				{
					backSpeed = 0f;
					forwardSpeed = Mathf.Clamp(forwardSpeed + Time.deltaTime * 2.5f, 0f, 1f);
					swingSpeed = Mathf.Clamp(swingSpeed + Time.deltaTime * forwardSpeed, -1f, 1f);
					swingAnim.SetFloat("Time", swingSpeed);
				}
				else
				{
					forwardSpeed = 0f;
					backSpeed = Mathf.Clamp(backSpeed + Time.deltaTime * 2.5f, 0f, 1f);
					swingSpeed = Mathf.Clamp(swingSpeed - Time.deltaTime * backSpeed, -1f, 1f);
					swingAnim.SetFloat("Time", swingSpeed);
				}
			}
			else
			{
				backSpeed = 0f;
				forwardSpeed = 0f;
				swingSpeed = Mathf.Lerp(swingSpeed, 0f, Time.deltaTime * 2f);
				swingAnim.SetFloat("Time", swingSpeed);
			}
		}
	}
}
