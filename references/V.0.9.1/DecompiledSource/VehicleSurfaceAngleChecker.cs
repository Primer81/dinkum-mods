using UnityEngine;

public class VehicleSurfaceAngleChecker : MonoBehaviour
{
	public float minimumX = -45f;

	public float maximumX = 45f;

	public float returnSpeed = 1f;

	public LayerMask checkLayers;

	private Rigidbody myRig;

	private void Start()
	{
		myRig = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (Physics.Raycast(new Ray(base.transform.position + Vector3.up / 2f, Vector3.down), out var hitInfo, 2f, checkLayers))
		{
			Vector3 normal = hitInfo.normal;
			Quaternion b = Quaternion.FromToRotation(base.transform.right, normal);
			myRig.MoveRotation(Quaternion.Slerp(myRig.rotation, b, Time.deltaTime));
			Vector3 eulerAngles = base.transform.eulerAngles;
			if (eulerAngles.x > 180f)
			{
				eulerAngles.x -= 360f;
			}
			eulerAngles.x = Mathf.Clamp(eulerAngles.x, minimumX, maximumX);
			myRig.MoveRotation(Quaternion.Euler(eulerAngles));
		}
	}
}
