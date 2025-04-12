using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private void Update()
	{
		base.transform.LookAt(CameraController.control.cameraTrans);
	}
}
