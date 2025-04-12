using UnityEngine;

public class CamerTripod : MonoBehaviour
{
	public Transform cameraPos;

	public void useTripod()
	{
		if (!PhotoManager.manage.cameraViewOpen)
		{
			PhotoManager.manage.openCameraView(cameraPos);
			Inventory.Instance.quickBarLocked(isLocked: true);
		}
		CameraController.control.transform.rotation = NetworkMapSharer.Instance.localChar.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
	}
}
