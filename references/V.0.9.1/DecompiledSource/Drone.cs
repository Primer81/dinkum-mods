using Mirror;
using UnityEngine;

public class Drone : NetworkBehaviour
{
	public float HorizontalVerticalSensitivity = 0.1f;

	public float UpDownSensitivity = 0.015f;

	public bool InvertedY;

	public float SpeedMultiplyerOnShiftPressed = 4f;

	private float slowSpeedMax = 1f;

	private float moveUpDown;

	private float speedMultiplyer = 1f;

	private float inversion = -1f;

	public bool charFollows = true;

	private Rigidbody myRig;

	private void Start()
	{
		myRig = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		PhotoManager.manage.openCameraView(base.transform, isDrone: true);
		NetworkMapSharer.Instance.localChar.moveLockRotateSlowOn(isOn: true);
	}

	private void FixedUpdate()
	{
		float x = InputMaster.input.getRightStick().x;
		float y = InputMaster.input.getRightStick().y;
		float x2 = InputMaster.input.getMousePosOld().x;
		float y2 = InputMaster.input.getMousePosOld().y;
		speedMultiplyer = Mathf.Lerp(speedMultiplyer, slowSpeedMax, 0.1f);
		if (!Inventory.Instance.usingMouse)
		{
			x = InputMaster.input.getLeftStick().x;
			y = InputMaster.input.getLeftStick().y;
			x2 = InputMaster.input.getRightStick().x;
			y2 = InputMaster.input.getRightStick().y;
			if (InputMaster.input.VehicleAccelerate() > 0.5f)
			{
				speedMultiplyer += 0.2f;
				speedMultiplyer = Mathf.Clamp(speedMultiplyer, 1f, SpeedMultiplyerOnShiftPressed);
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				if (slowSpeedMax.Equals(1f))
				{
					slowSpeedMax = 0.45f;
				}
				else
				{
					slowSpeedMax = 1f;
				}
			}
			if (Input.GetKey(KeyCode.RightShift))
			{
				speedMultiplyer += 0.2f;
				speedMultiplyer = Mathf.Clamp(speedMultiplyer, 1f, SpeedMultiplyerOnShiftPressed);
			}
		}
		moveUpDown = Mathf.Lerp(moveUpDown, 0f, 0.1f);
		if (Input.GetKey(KeyCode.N))
		{
			moveUpDown += UpDownSensitivity;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			moveUpDown -= UpDownSensitivity;
		}
		base.transform.Rotate(Vector3.up * x2, Space.World);
		base.transform.Rotate(base.transform.right * -1f * y2, Space.World);
		base.transform.position += base.transform.right * x * HorizontalVerticalSensitivity * speedMultiplyer;
		base.transform.position += base.transform.forward * y * HorizontalVerticalSensitivity * speedMultiplyer;
		base.transform.position += base.transform.up * moveUpDown * speedMultiplyer;
	}

	private void MirrorProcessed()
	{
	}
}
