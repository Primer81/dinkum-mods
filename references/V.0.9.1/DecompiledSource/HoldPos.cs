using System.Collections;
using UnityEngine;

public class HoldPos : MonoBehaviour
{
	public CharMovement parentChar;

	private Vector3 lastPosition;

	private Quaternion lastRotation;

	private bool tracking;

	private float magnitude;

	private float angle;

	private float timer;

	private float fullThrowTimer = 0.4f;

	private IEnumerator TrackLastPosition()
	{
		lastPosition = base.transform.position;
		lastRotation = base.transform.rotation;
		WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();
		while (tracking)
		{
			yield return fixedWait;
			magnitude = Mathf.Clamp01(Vector3.Distance(base.transform.position, lastPosition) / Time.fixedDeltaTime / 9f);
			angle = Quaternion.Angle(base.transform.rotation, lastRotation);
			AddToFullThrowTimer();
			lastPosition = base.transform.position;
			lastRotation = base.transform.rotation;
		}
	}

	public void StartTrackingDistanceForThrow()
	{
		if ((bool)parentChar && parentChar.isLocalPlayer)
		{
			magnitude = 0f;
			angle = 0f;
			tracking = true;
			StartCoroutine(TrackLastPosition());
		}
	}

	public void StopTrackingDistanceForThrow()
	{
		tracking = false;
	}

	public float GetMagitudeForCharacterMovement()
	{
		if (parentChar.InJump)
		{
			return 1f;
		}
		if (angle <= 7f)
		{
			if (timer < fullThrowTimer)
			{
				return magnitude / 4f;
			}
			return magnitude;
		}
		return 0f;
	}

	private void AddToFullThrowTimer()
	{
		if (magnitude >= 0.85f && angle <= 7f)
		{
			if (timer < fullThrowTimer)
			{
				timer += Time.fixedDeltaTime;
			}
		}
		else
		{
			timer = 0f;
		}
	}
}
