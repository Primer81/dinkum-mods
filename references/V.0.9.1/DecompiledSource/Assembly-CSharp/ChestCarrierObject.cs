using System.Collections;
using UnityEngine;

public class ChestCarrierObject : MonoBehaviour
{
	private CharChestCarrier myChar;

	public Transform objectSpawnPoint;

	public Transform trolleyTransform;

	private Transform followTransform;

	public Transform leftHandle;

	public Transform rightHandle;

	public Transform moveLeftHandTo;

	public Transform moveRightHandTo;

	public Transform wheelsRotate;

	private bool slotDisabled;

	private Vector3 lastPos;

	private float rotationSpeed;

	private void OnEnable()
	{
		myChar = GetComponentInParent<CharChestCarrier>();
		if ((bool)myChar)
		{
			myChar.SetCarrierInPlayerHands(this);
			StartCoroutine(ControllTrolley());
		}
	}

	private IEnumerator ControllTrolley()
	{
		followTransform = myChar.transform;
		trolleyTransform.transform.SetParent(null);
		trolleyTransform.localScale = Vector3.one;
		while (true)
		{
			yield return null;
			if (myChar.isLocalPlayer)
			{
				HandleControls();
				LockIconIfInvOpen();
			}
			HandleTrolleyHight();
		}
	}

	private void LockIconIfInvOpen()
	{
		if (Inventory.Instance.IsQuickBarLocked() && !slotDisabled && !Inventory.Instance.CanMoveCharacter())
		{
			Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].disableForGive();
			slotDisabled = true;
		}
		else if (Inventory.Instance.IsQuickBarLocked() && slotDisabled && Inventory.Instance.CanMoveCharacter())
		{
			Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].clearDisable();
			slotDisabled = false;
		}
		else if (!Inventory.Instance.IsQuickBarLocked() && slotDisabled)
		{
			Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].clearDisable();
			slotDisabled = false;
		}
	}

	private void HandleControls()
	{
		if (!InputMaster.input.Use() || !Inventory.Instance.CanMoveCharacter())
		{
			return;
		}
		if (!myChar.IsHoldingChest())
		{
			if (myChar.CheckIfChestIsOnSelectedTileForButtonPress())
			{
				myChar.PickUpChestAtCurrentPos();
				Inventory.Instance.quickBarLocked(isLocked: true);
			}
		}
		else if (myChar.NeedsToEnterPlacementMode())
		{
			myChar.EnterPlacementMode();
		}
		else if (myChar.PutDownChestAtCurrentPos())
		{
			Inventory.Instance.quickBarLocked(isLocked: false);
		}
	}

	private void HandleTrolleyHight()
	{
		trolleyTransform.position = followTransform.position + followTransform.forward * 1.5f;
		Vector3 eulerAngles = followTransform.eulerAngles;
		trolleyTransform.transform.rotation = Quaternion.Euler(-35f, eulerAngles.y, 0f);
		leftHandle.transform.position = moveLeftHandTo.transform.position;
		rightHandle.transform.position = moveRightHandTo.transform.position;
		if (Vector3.Distance(followTransform.position, lastPos) > 0.001f)
		{
			rotationSpeed = -220f;
		}
		else
		{
			rotationSpeed = Mathf.Lerp(rotationSpeed, 0f, Time.deltaTime * 4f);
		}
		wheelsRotate.Rotate(Vector3.right * rotationSpeed * Time.deltaTime, Space.Self);
		lastPos = followTransform.position;
	}

	private void OnDestroy()
	{
		Object.Destroy(trolleyTransform.gameObject);
	}
}
