using UnityEngine;

public class camerFlys : MonoBehaviour
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

	public float x;

	public float y;

	public float mouseX;

	public float mouseY;

	private int smoothMode = 1;

	private void Start()
	{
		if (InvertedY)
		{
			inversion = 1f;
		}
		else
		{
			inversion = -1f;
		}
	}

	private void Update()
	{
		if (Inventory.Instance.usingMouse && !Input.GetKey(KeyCode.Mouse1))
		{
			return;
		}
		CheckForModeChange();
		float num = Time.deltaTime;
		if (smoothMode == 2)
		{
			num *= 2f;
		}
		if (smoothMode == 3)
		{
			num = 1f;
		}
		if (Inventory.Instance.usingMouse)
		{
			x = Mathf.Lerp(x, InputMaster.input.getLeftStick().x, num);
			y = Mathf.Lerp(y, InputMaster.input.getLeftStick().y, num);
			mouseX = Mathf.Lerp(mouseX, InputMaster.input.getMousePosOld().x, num);
			mouseY = Mathf.Lerp(mouseY, InputMaster.input.getMousePosOld().y, num);
		}
		speedMultiplyer = Mathf.Lerp(speedMultiplyer, slowSpeedMax, 0.1f);
		if (!Inventory.Instance.usingMouse)
		{
			x = Mathf.Lerp(x, InputMaster.input.getLeftStick().x, num);
			y = Mathf.Lerp(y, InputMaster.input.getLeftStick().y, num);
			mouseX = Mathf.Lerp(mouseX, InputMaster.input.getRightStick().x, num);
			mouseY = Mathf.Lerp(mouseY, InputMaster.input.getRightStick().y, num);
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
		if (charFollows)
		{
			NetworkMapSharer.Instance.localChar.transform.position = base.transform.position - base.transform.forward * 2f;
			NetworkMapSharer.Instance.localChar.transform.rotation = base.transform.rotation;
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
		base.transform.Rotate(Vector3.up * mouseX, Space.World);
		base.transform.Rotate(base.transform.right * -1f * mouseY, Space.World);
		base.transform.position += base.transform.right * x * HorizontalVerticalSensitivity * speedMultiplyer;
		base.transform.position += base.transform.forward * y * HorizontalVerticalSensitivity * speedMultiplyer;
		base.transform.position += base.transform.up * moveUpDown * speedMultiplyer;
	}

	private void CheckForModeChange()
	{
		if (Input.GetKey(KeyCode.Alpha1))
		{
			smoothMode = 1;
		}
		else if (Input.GetKey(KeyCode.Alpha2))
		{
			smoothMode = 2;
		}
		else if (Input.GetKey(KeyCode.Alpha3))
		{
			smoothMode = 3;
		}
	}
}
