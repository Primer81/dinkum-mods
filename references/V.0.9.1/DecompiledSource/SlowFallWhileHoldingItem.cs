using UnityEngine;

public class SlowFallWhileHoldingItem : MonoBehaviour
{
	private CharMovement myChar;

	private Rigidbody myRig;

	public float fallSpeedDivider = 2f;

	private Vector3 lastPos;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			myRig = myChar.GetComponent<Rigidbody>();
			lastPos = myChar.transform.position;
		}
	}

	private void FixedUpdate()
	{
		if ((bool)myChar && myChar.isLocalPlayer && (bool)myRig)
		{
			Vector3 velocity = myRig.velocity;
			if (lastPos.y > myChar.transform.position.y)
			{
				velocity.y = myRig.velocity.y / fallSpeedDivider;
				myRig.velocity = velocity;
			}
			lastPos = myChar.transform.position;
		}
	}
}
