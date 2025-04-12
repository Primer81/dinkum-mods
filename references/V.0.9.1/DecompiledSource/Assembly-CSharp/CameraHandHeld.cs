using UnityEngine;

public class CameraHandHeld : MonoBehaviour
{
	private CharMovement myChar;

	private Quaternion startRot;

	private bool cameraOpen;

	public void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void openCameraMenu()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			cameraOpen = true;
			PhotoManager.manage.openCameraView();
			Inventory.Instance.quickBarLocked(isLocked: true);
			startRot = CameraController.control.transform.rotation;
			CameraController.control.transform.rotation = NetworkMapSharer.Instance.localChar.transform.rotation;
		}
	}

	public void closeCameraView()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			cameraOpen = false;
			PhotoManager.manage.closeCameraView();
			Inventory.Instance.quickBarLocked(isLocked: false);
			CameraController.control.transform.rotation = startRot;
		}
	}

	private void OnDisable()
	{
		if (cameraOpen)
		{
			closeCameraView();
		}
	}
}
