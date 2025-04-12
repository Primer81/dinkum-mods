using UnityEngine;

public class RotateTowardsRootTransform : MonoBehaviour
{
	private Transform target;

	public float rotationSpeed = 1f;

	private void Start()
	{
		target = base.transform.root;
	}

	private void Update()
	{
		if (target != null)
		{
			Quaternion rotation = base.transform.rotation;
			Quaternion b = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
			base.transform.rotation = Quaternion.Slerp(rotation, b, Time.deltaTime * rotationSpeed);
		}
	}
}
