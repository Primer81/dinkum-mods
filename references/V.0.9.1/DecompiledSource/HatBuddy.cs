using UnityEngine;

public class HatBuddy : MonoBehaviour
{
	public Transform buddyTransform;

	public Transform perchPosition;

	public Transform spinner;

	private Quaternion spinnerDefaultPos;

	public Animator buddyAnim;

	public float followSpeed = 2f;

	private float currentSpeed;

	public float stopRunningDistance = 0.5f;

	public float spinnerSpeed = 5f;

	private void OnEnable()
	{
		buddyTransform.parent = null;
		buddyTransform.rotation = Quaternion.identity;
		if ((bool)spinner)
		{
			spinnerDefaultPos = spinner.rotation;
		}
		buddyTransform.gameObject.SetActive(value: true);
	}

	private void OnDestroy()
	{
		Object.Destroy(buddyTransform.gameObject);
	}

	private void OnDisable()
	{
		buddyTransform.gameObject.SetActive(value: false);
	}

	private void FixedUpdate()
	{
		buddyTransform.position = Vector3.Lerp(buddyTransform.position, perchPosition.position, Time.fixedDeltaTime * followSpeed);
		buddyTransform.rotation = Quaternion.Lerp(buddyTransform.rotation, perchPosition.rotation, Time.fixedDeltaTime * followSpeed);
		float num = Vector3.Distance(buddyTransform.position, perchPosition.position);
		if (num > 15f)
		{
			buddyTransform.position = perchPosition.position;
		}
		if (num < stopRunningDistance)
		{
			currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.fixedDeltaTime * 2f);
			if ((bool)spinner)
			{
				spinner.Rotate(Vector3.up, Time.deltaTime * spinnerSpeed);
			}
		}
		else
		{
			currentSpeed = Mathf.Lerp(currentSpeed, 2f, Time.fixedDeltaTime * 2f);
			if ((bool)spinner)
			{
				spinner.rotation = spinnerDefaultPos;
			}
		}
		if ((bool)buddyAnim)
		{
			buddyAnim.SetFloat("WalkingSpeed", currentSpeed);
		}
	}
}
