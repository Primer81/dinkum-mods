using System.Collections;
using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
	private EquipItemToChar attachedChar;

	private CharMovement connectedChar;

	public InventoryItem useAsAmmo;

	public Transform lookable;

	public Transform faceCam;

	public int projectileId;

	public float projectileDelay = 0.5f;

	public float holdToFireAgainDelay = 0.5f;

	public float strengthMax = 5f;

	public AudioSource stretchSound;

	private float stretchSoundMax = 0.1f;

	public ASound fireSound;

	private int ammoId;

	private bool lookingDownSights;

	public LayerMask canHitLayer;

	public float chargeSpeed = 3f;

	[Header("Spawns Network Object on fire")]
	public GameObject spawnAndFire;

	private void OnEnable()
	{
		Crossheir.cross.crossheirIcon.enabled = true;
		ammoId = Inventory.Instance.getInvItemId(useAsAmmo);
		attachedChar = base.transform.GetComponentInParent<EquipItemToChar>();
		connectedChar = base.transform.GetComponentInParent<CharMovement>();
		if ((bool)attachedChar)
		{
			StartCoroutine(aimDownSights());
			StartCoroutine(attachLeftHand());
		}
	}

	private IEnumerator aimDownSights()
	{
		updateAmmoCounter();
		if (attachedChar.isLocalPlayer)
		{
			StartCoroutine(updateLookableLocation());
		}
		if (attachedChar.isLocalPlayer)
		{
			StartCoroutine(lookDownSights());
		}
		while (true)
		{
			if (attachedChar.usingItem)
			{
				float strength = 1f;
				stretchSound.volume = 0f;
				stretchSound.pitch = 1f + Random.Range(-0.1f, 0.1f);
				stretchSound.Play();
				while (attachedChar.usingItem)
				{
					strength = Mathf.Clamp(strength + Time.deltaTime * chargeSpeed, 1f, strengthMax);
					if (attachedChar.isLocalPlayer)
					{
						Crossheir.cross.setPower(strength, strengthMax);
					}
					if (strength < strengthMax)
					{
						stretchSound.volume = Mathf.Clamp(stretchSound.volume + Time.deltaTime, 0f, stretchSoundMax * SoundManager.Instance.GetGlobalSoundVolume());
						stretchSound.pitch = Mathf.Clamp(stretchSound.pitch + Time.deltaTime, 0f, strengthMax / 2f);
					}
					else
					{
						stretchSound.volume = Mathf.Clamp(stretchSound.volume - Time.deltaTime * 5f, 0f, stretchSoundMax * SoundManager.Instance.GetGlobalSoundVolume());
					}
					yield return null;
				}
				if (attachedChar.isLocalPlayer)
				{
					Crossheir.cross.fadeOut();
				}
				stretchSound.Pause();
				yield return new WaitForSeconds(projectileDelay);
				if ((bool)fireSound)
				{
					SoundManager.Instance.playASoundAtPoint(fireSound, base.transform.position);
				}
				if (attachedChar.isLocalPlayer)
				{
					if (canFire())
					{
						Vector3 direction = CameraController.control.cameraTrans.position + CameraController.control.cameraTrans.forward * (strength * 5f) - base.transform.position;
						if (Physics.Raycast(CameraController.control.cameraTrans.position, CameraController.control.cameraTrans.forward, out var hitInfo, 25f, canHitLayer))
						{
							direction = hitInfo.point - base.transform.position;
						}
						direction.Normalize();
						if (spawnAndFire != null)
						{
							attachedChar.CmdSpawnProjectileObject(useAsAmmo.getItemId(), attachedChar.holdPos.position + attachedChar.holdPos.forward, direction, strength, useAsAmmo.getItemId());
						}
						else
						{
							attachedChar.CmdFireProjectileAtDir(attachedChar.holdPos.position + attachedChar.holdPos.forward, direction, strength, projectileId);
						}
						consumeAmmo();
						Inventory.Instance.useItemWithFuel();
					}
					updateAmmoCounter();
				}
				yield return new WaitForSeconds(holdToFireAgainDelay);
			}
			if (attachedChar.isLocalPlayer && !attachedChar.usingItem)
			{
				while (!attachedChar.usingItem)
				{
					yield return null;
				}
			}
			yield return null;
		}
	}

	private IEnumerator updateLookableLocation()
	{
		while (true)
		{
			if (attachedChar.usingItem && lookingDownSights)
			{
				faceCam.LookAt(CameraController.control.Camera_Y.position + CameraController.control.Camera_Y.forward * 5f);
				lookable.transform.position = faceCam.position + faceCam.forward * 2f;
			}
			else
			{
				lookable.transform.localPosition = Vector3.zero + Vector3.up;
			}
			yield return null;
		}
	}

	public void exitSights()
	{
		CameraController.control.exitAimCamera();
		connectedChar.lockRotation(isLocked: false);
		lookingDownSights = false;
		Cursor.lockState = CursorLockMode.Confined;
		Crossheir.cross.turnOffCrossheir();
	}

	private IEnumerator lookDownSights()
	{
		while (true)
		{
			if (Inventory.Instance.CanMoveCharacter() && InputMaster.input.Use())
			{
				bool lookingDownSights = true;
				CameraController.control.enterAimCamera();
				connectedChar.lockRotation(isLocked: true);
				Crossheir.cross.turnOnCrossheir();
				while (lookingDownSights)
				{
					Cursor.lockState = CursorLockMode.Locked;
					if (!Inventory.Instance.CanMoveCharacter())
					{
						lookingDownSights = false;
						exitSights();
					}
					if (!InputMaster.input.Use() && (InputMaster.input.UICancel() || (Inventory.Instance.usingMouse && InputMaster.input.Interact())))
					{
						lookingDownSights = false;
						exitSights();
					}
					yield return null;
				}
			}
			yield return null;
		}
	}

	private void OnDisable()
	{
		if ((bool)attachedChar && attachedChar.isLocalPlayer)
		{
			exitSights();
			CameraController.control.exitAimCamera();
			Crossheir.cross.turnOffCrossheir();
			connectedChar.lockRotation(isLocked: false);
			lookingDownSights = false;
			Cursor.lockState = CursorLockMode.Confined;
		}
	}

	public void updateAmmoCounter()
	{
		if (connectedChar.isLocalPlayer)
		{
			Crossheir.cross.setAmmo(useAsAmmo.itemSprite, Inventory.Instance.getAmountOfItemInAllSlots(ammoId));
		}
	}

	public void consumeAmmo()
	{
		Inventory.Instance.removeAmountOfItem(ammoId, 1);
		Inventory.Instance.equipNewSelectedSlot();
	}

	public bool canFire()
	{
		return Inventory.Instance.getAmountOfItemInAllSlots(ammoId) > 0;
	}

	private IEnumerator attachLeftHand()
	{
		while (true)
		{
			if ((bool)attachedChar)
			{
				if (attachedChar.usingItem)
				{
					attachedChar.attachLeftHand();
				}
				else
				{
					attachedChar.removeLeftHand();
				}
			}
			yield return null;
		}
	}
}
