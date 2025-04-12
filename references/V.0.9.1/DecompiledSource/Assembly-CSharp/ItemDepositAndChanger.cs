using System.Collections;
using UnityEngine;

public class ItemDepositAndChanger : MonoBehaviour
{
	public enum UseVerb
	{
		Insert,
		Cook,
		Smelt,
		Grind,
		Brew,
		Display,
		Saw
	}

	public Transform ejectPos;

	public Animator processAnimator;

	public Transform proccessingItemPrefabPosition;

	private GameObject displayingPrefab;

	public ASound processingSound;

	public AudioSource myAudioSource;

	public int currentXPos;

	public int currentYPos;

	public bool useWindMill;

	public bool useSolar;

	public UseVerb MyVerb;

	public static WaitForSeconds afterProcessWait = new WaitForSeconds(3f);

	private Coroutine animatorDisableRoutine;

	public string GetLocalisedVerb()
	{
		return ConversationGenerator.generate.GetToolTip("Tip_" + MyVerb);
	}

	private void Start()
	{
		SoundManager.Instance.onMasterChange.AddListener(onVolumeChange);
		base.gameObject.AddComponent<InteractableObject>().isItemChanger = this;
	}

	public void mapUpdatePos(int xPos, int yPos, HouseDetails inside = null)
	{
		if ((inside == null && WorldManager.Instance.onTileStatusMap[xPos, yPos] == -1) || (inside == null && WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0))
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = -2;
		}
		else if ((inside != null && inside.houseMapOnTileStatus[xPos, yPos] == -1) || (inside != null && inside.houseMapOnTileStatus[xPos, yPos] == 0))
		{
			inside.houseMapOnTileStatus[xPos, yPos] = -2;
		}
		if ((bool)processAnimator)
		{
			if ((inside == null && WorldManager.Instance.onTileStatusMap[xPos, yPos] != -2) || (inside != null && inside.houseMapOnTileStatus[xPos, yPos] != -2))
			{
				startProcessingSound();
				startProcessingAnim();
			}
			else
			{
				stopProcessingSound();
				stopProcessingAnim();
			}
		}
		if (((inside == null && WorldManager.Instance.onTileStatusMap[xPos, yPos] != -2) || (inside != null && inside.houseMapOnTileStatus[xPos, yPos] != -2)) && displayingPrefab == null)
		{
			if (inside == null)
			{
				if ((bool)Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].altDropPrefab)
				{
					displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
				}
				else
				{
					displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
				}
			}
			else if ((bool)Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab)
			{
				displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
			}
			else
			{
				displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
			}
			displayingPrefab.transform.localPosition = Vector3.zero;
			displayingPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Object.Destroy(displayingPrefab.GetComponent<Animator>());
		}
		currentXPos = xPos;
		currentYPos = yPos;
	}

	public bool getIfProcessing()
	{
		return processAnimator.GetBool("Processing");
	}

	public void ejectItemOnCycle(int xPos, int yPos, HouseDetails inside = null)
	{
		if (inside != null)
		{
			if (inside.houseMapOnTileStatus[xPos, yPos] < 0 || !Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange)
			{
				return;
			}
			int stackAmount = 1;
			int changerResultId = Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange.getChangerResultId(inside.houseMapOnTile[xPos, yPos]);
			if (changerResultId != -1)
			{
				if (Inventory.Instance.allItems[changerResultId].hasFuel)
				{
					stackAmount = Inventory.Instance.allItems[changerResultId].fuelMax;
				}
				NetworkMapSharer.Instance.spawnAServerDropToSave(changerResultId, stackAmount, ejectPos.position, inside);
			}
		}
		else
		{
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] < 0)
			{
				return;
			}
			int stackAmount2 = 1;
			if (!Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemChange)
			{
				return;
			}
			int changerResultId2 = Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemChange.getChangerResultId(WorldManager.Instance.onTileMap[xPos, yPos]);
			if (changerResultId2 != -1)
			{
				if (Inventory.Instance.allItems[changerResultId2].hasFuel)
				{
					stackAmount2 = Inventory.Instance.allItems[changerResultId2].fuelMax;
				}
				NetworkMapSharer.Instance.spawnAServerDropToSave(changerResultId2, stackAmount2, ejectPos.position, inside);
			}
		}
	}

	public void ejectItem(int xPos, int yPos, HouseDetails inside = null)
	{
		if (inside != null)
		{
			if (inside.houseMapOnTileStatus[xPos, yPos] >= 0 && (bool)Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange && Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange.getChangerResultId(inside.houseMapOnTile[xPos, yPos]) != -1)
			{
				NetworkMapSharer.Instance.spawnAServerDropToSave(Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange.getChangerResultId(inside.houseMapOnTile[xPos, yPos]), 1, ejectPos.position, inside);
			}
		}
		else if (WorldManager.Instance.onTileStatusMap[xPos, yPos] >= 0 && (bool)Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemChange && Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemChange.getChangerResultId(WorldManager.Instance.onTileMap[xPos, yPos]) != -1)
		{
			NetworkMapSharer.Instance.spawnAServerDropToSave(Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemChange.getChangerResultId(WorldManager.Instance.onTileMap[xPos, yPos]), 1, ejectPos.position, inside);
		}
		if ((bool)processAnimator)
		{
			stopProcessingAnim();
		}
		if (displayingPrefab != null)
		{
			Object.Destroy(displayingPrefab);
		}
		if (inside != null)
		{
			inside.houseMapOnTileStatus[xPos, yPos] = -2;
		}
		else
		{
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = -2;
		}
	}

	public void depositItem(InventoryItem placeIn, int xPos, int yPos, HouseDetails inside = null)
	{
	}

	public void playLocalDeposit(int xPos, int yPos, HouseDetails inside = null)
	{
		if ((bool)processAnimator)
		{
			startProcessingSound();
			startProcessingAnim(placeIntoTrigger: true);
		}
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItemInChanger, base.transform.position);
		if (((inside != null || WorldManager.Instance.onTileStatusMap[xPos, yPos] == -2) && (inside == null || inside.houseMapOnTileStatus[xPos, yPos] == -2)) || !(displayingPrefab == null))
		{
			return;
		}
		if (inside == null)
		{
			if ((bool)Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].altDropPrefab)
			{
				displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
			}
			else
			{
				displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[WorldManager.Instance.onTileStatusMap[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
			}
		}
		else if ((bool)Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab)
		{
			displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
		}
		else
		{
			displayingPrefab = Object.Instantiate(Inventory.Instance.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
		}
		displayingPrefab.transform.localPosition = Vector3.zero;
		displayingPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Object.Destroy(displayingPrefab.GetComponent<Animator>());
	}

	public void stopLocalProcessing()
	{
		if ((bool)processAnimator)
		{
			stopProcessingSound();
			stopProcessingAnim();
		}
		if (displayingPrefab != null)
		{
			Object.Destroy(displayingPrefab);
		}
	}

	public void OnDisable()
	{
		Object.Destroy(displayingPrefab);
	}

	public int returnAmountNeeded(InventoryItem itemToCheck)
	{
		if ((bool)itemToCheck.itemChange)
		{
			return itemToCheck.itemChange.getAmountNeeded(GetComponent<TileObject>().tileObjectId);
		}
		return 0;
	}

	public bool canDepositThisItem(InventoryItem canDeposit, HouseDetails inside = null, int xPos = -2, int yPos = -2)
	{
		if (xPos == -2 && yPos == -2)
		{
			xPos = currentXPos;
			yPos = currentYPos;
		}
		if (inside == null && WorldManager.Instance.onTileStatusMap[xPos, yPos] != -2)
		{
			return false;
		}
		if (inside != null && inside.houseMapOnTileStatus[xPos, yPos] != -2)
		{
			return false;
		}
		if ((bool)canDeposit.itemChange)
		{
			return canDeposit.itemChange.checkIfCanBeDeposited(GetComponent<TileObject>().tileObjectId);
		}
		return false;
	}

	public void startProcessingSound()
	{
		StopCoroutine("stopSound");
		if ((bool)myAudioSource)
		{
			myAudioSource.Stop();
			if (!myAudioSource.isPlaying)
			{
				myAudioSource.clip = processingSound.getSound();
				myAudioSource.pitch = processingSound.getPitch() * 4f;
				myAudioSource.volume = 0f;
				myAudioSource.Play();
				StartCoroutine("startSound");
			}
		}
	}

	public void stopProcessingSound()
	{
		StopCoroutine("startSound");
		if ((bool)myAudioSource && myAudioSource.isPlaying)
		{
			StartCoroutine("stopSound");
		}
	}

	private IEnumerator startSound()
	{
		while (myAudioSource.volume < processingSound.volume * SoundManager.Instance.GetGlobalSoundVolume())
		{
			yield return null;
			myAudioSource.volume += 0.01f;
			myAudioSource.pitch = Mathf.Lerp(myAudioSource.pitch, processingSound.getPitch(), myAudioSource.volume / (processingSound.volume * SoundManager.Instance.GetGlobalSoundVolume()));
		}
		myAudioSource.volume = processingSound.volume * SoundManager.Instance.GetGlobalSoundVolume();
		myAudioSource.pitch = processingSound.getPitch();
	}

	private IEnumerator stopSound()
	{
		while (myAudioSource.volume > 0f)
		{
			yield return null;
			myAudioSource.volume -= 0.01f;
			myAudioSource.pitch += 0.01f;
		}
		myAudioSource.volume = 0f;
		myAudioSource.Stop();
	}

	private void onVolumeChange()
	{
		if ((bool)myAudioSource)
		{
			myAudioSource.volume = processingSound.volume * SoundManager.Instance.GetGlobalSoundVolume();
		}
	}

	public void startProcessingAnim(bool placeIntoTrigger = false)
	{
		if (animatorDisableRoutine != null)
		{
			StopCoroutine(animatorDisableRoutine);
			animatorDisableRoutine = null;
		}
		if (!processAnimator.enabled)
		{
			processAnimator.enabled = true;
		}
		if (placeIntoTrigger)
		{
			processAnimator.SetTrigger("PlaceInTo");
		}
		processAnimator.SetBool("Processing", value: true);
	}

	public void stopProcessingAnim()
	{
		processAnimator.SetBool("Processing", value: false);
		if (animatorDisableRoutine != null)
		{
			StopCoroutine(animatorDisableRoutine);
			animatorDisableRoutine = null;
		}
		animatorDisableRoutine = StartCoroutine(stopProccessingRoutine());
	}

	private IEnumerator stopProccessingRoutine()
	{
		yield return afterProcessWait;
		processAnimator.enabled = false;
		animatorDisableRoutine = null;
	}
}
