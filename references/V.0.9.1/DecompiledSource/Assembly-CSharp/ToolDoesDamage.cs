using System.Collections;
using UnityEngine;

public class ToolDoesDamage : MonoBehaviour
{
	public CharInteract myCharInteract;

	public ASound soundOnDamage;

	public ASound refillSound;

	public bool refillInWater;

	public TileObject[] refillOnTile;

	public bool changeToFull;

	public bool spawnPlaceableObjectOnUse;

	public float speedDif;

	private bool canMiss = true;

	public ASound incorrectHitSound;

	private Animator myAnim;

	private InventoryItem holding;

	private NPCDoesTasks npcDoes;

	private bool canClang;

	private bool canDamage = true;

	private bool canSpawnPlaceable = true;

	public void Start()
	{
		myCharInteract = GetComponentInParent<CharInteract>();
		myAnim = GetComponent<Animator>();
		if (!myAnim)
		{
			return;
		}
		if (speedDif != 0f)
		{
			myAnim.SetFloat("Speed", speedDif);
		}
		AnimatorControllerParameter[] parameters = myAnim.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == "Clang")
			{
				canClang = true;
				break;
			}
		}
		if ((bool)myCharInteract && myCharInteract.myEquip.currentlyHoldingItemId >= 0)
		{
			holding = Inventory.Instance.allItems[myCharInteract.myEquip.currentlyHoldingItemId];
		}
	}

	public IEnumerator damageThisFrameTimer()
	{
		canDamage = false;
		yield return null;
		yield return null;
		canDamage = true;
	}

	public void doDamageNow()
	{
		if (!canDamage)
		{
			return;
		}
		StartCoroutine(damageThisFrameTimer());
		if ((bool)myCharInteract)
		{
			if (spawnPlaceableObjectOnUse && canSpawnPlaceable)
			{
				myCharInteract.SpawnPlaceableObject();
			}
			else
			{
				myCharInteract.doDamage();
			}
		}
		else if ((bool)npcDoes && npcDoes.isServer)
		{
			npcDoes.onToolDoesDamage();
		}
	}

	public void refill()
	{
		if (myCharInteract.isLocalPlayer)
		{
			if (changeToFull)
			{
				Inventory.Instance.changeToFullItem();
			}
			else
			{
				Inventory.Instance.fillFuelInItem();
			}
		}
	}

	public void checkRefill()
	{
		if ((bool)npcDoes)
		{
			return;
		}
		int num = (int)myCharInteract.currentlyAttackingPos.x;
		int num2 = (int)myCharInteract.currentlyAttackingPos.y;
		if ((bool)myCharInteract && myCharInteract.isLocalPlayer)
		{
			num = (int)myCharInteract.selectedTile.x;
			num2 = (int)myCharInteract.selectedTile.y;
		}
		if (refillInWater && WorldManager.Instance.heightMap[num, num2] <= 0 && WorldManager.Instance.waterMap[num, num2])
		{
			base.transform.root.GetComponent<Animator>().SetTrigger("Refill");
			SoundManager.Instance.playASoundAtPoint(refillSound, base.transform.position);
			ParticleManager.manage.waterWakePart(base.transform.position + base.transform.root.forward * 1.5f, 15);
			refill();
		}
		else
		{
			if (refillOnTile.Length == 0)
			{
				return;
			}
			for (int i = 0; i < refillOnTile.Length; i++)
			{
				Vector2 vector = new Vector2(num, num2);
				if (WorldManager.Instance.onTileMap[(int)vector.x, (int)vector.y] < -1)
				{
					vector = WorldManager.Instance.findMultiTileObjectPos((int)vector.x, (int)vector.y);
				}
				if (WorldManager.Instance.onTileMap[(int)vector.x, (int)vector.y] == refillOnTile[i].tileObjectId)
				{
					base.transform.root.GetComponent<Animator>().SetTrigger("Refill");
					SoundManager.Instance.playASoundAtPoint(refillSound, base.transform.position);
					refill();
					break;
				}
			}
		}
	}

	public void playClangSound()
	{
		SoundManager.Instance.playASoundAtPoint(incorrectHitSound, base.transform.position);
	}

	public bool checkIfNeedClang()
	{
		int num = 0;
		int num2 = 0;
		if ((bool)npcDoes)
		{
			return false;
		}
		if (myCharInteract.isLocalPlayer)
		{
			num = (int)myCharInteract.selectedTile.x;
			num2 = (int)myCharInteract.selectedTile.y;
		}
		else
		{
			num = (int)myCharInteract.currentlyAttackingPos.x;
			num2 = (int)myCharInteract.currentlyAttackingPos.y;
		}
		if (myCharInteract.CheckIfCanDamage(new Vector2(num, num2)))
		{
			return false;
		}
		if ((WorldManager.Instance.onTileMap[num, num2] != 30 && WorldManager.Instance.onTileMap[num, num2] != -1) || base.transform.root.position.y + 2f < (float)WorldManager.Instance.heightMap[num, num2])
		{
			ParticleManager.manage.emitAttackParticle(new Vector3(num * 2, WorldManager.Instance.heightMap[num, num2], num2 * 2), 5);
			return true;
		}
		return false;
	}

	public void earlyMissCheck()
	{
		int num = 0;
		int num2 = 0;
		if (myCharInteract.isLocalPlayer)
		{
			num = (int)myCharInteract.selectedTile.x;
			num2 = (int)myCharInteract.selectedTile.y;
		}
		else
		{
			num = (int)myCharInteract.currentlyAttackingPos.x;
			num2 = (int)myCharInteract.currentlyAttackingPos.y;
		}
		if (!myCharInteract.CheckIfCanDamage(new Vector2(num, num2)) && (((bool)holding.placeable && !WorldManager.Instance.allObjectSettings[holding.placeable.tileObjectId].canBePlacedOnTopOfFurniture) || WorldManager.Instance.onTileMap[num, num2] != -1 || base.transform.root.position.y + 2f < (float)WorldManager.Instance.heightMap[num, num2]) && (bool)myAnim && canClang)
		{
			myAnim.SetTrigger("Clang");
		}
	}

	public void attachNPC(NPCDoesTasks NPC)
	{
		npcDoes = NPC;
	}
}
