using System.Collections;
using System.Runtime.InteropServices;
using I2.Loc;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharInteract : NetworkBehaviour
{
	public enum TilePromptType
	{
		None,
		Harvest,
		ItemName,
		Open,
		Close
	}

	[SyncVar(hook = "OnChangeAttackingPos")]
	public Vector2 currentlyAttackingPos = new Vector2(0f, 0f);

	public Transform tileHighlighter;

	public LayerMask tileHighlighterCollisionMask;

	[HideInInspector]
	public EquipItemToChar myEquip;

	[SerializeField]
	private TileObject _objectAttacking;

	public Material previewYes;

	public Material previewNo;

	[SerializeField]
	[Tooltip("If true - player is placing an object independantly to the character, eg. placing a building.")]
	private bool _isPlacingDeed;

	public DisplayPlayerHouseTiles insideHouseDisplay;

	public Vector2 selectedTile = new Vector2(0f, 0f);

	[SerializeField]
	private bool _isInsidePlayerHouse;

	[SerializeField]
	private GameObject tileHighlighterRotArrow;

	[SerializeField]
	private TileHighlighter highLighterChange;

	private InventoryItem lastEquip;

	private CharMovement myMove;

	private TileObject previewObject;

	private Transform playerHouseTransform;

	private Vector3 selectPositionOffset = Vector3.zero;

	private Vector3 desiredHighlighterPos = Vector3.zero;

	private Vector2 selectedTileBeforeToolUse;

	private int currentlyAttackingId = -1;

	private int placingObjectRotation;

	private bool canAttackSelectedTile;

	private bool canWalkThroughDoor = true;

	private bool wasPreviouslyInside;

	private Vector3 desiredPreviewPos;

	private int previewShowingRot;

	private GameObject vehiclePreview;

	private VehiclePreview canPlaceVehiclePreview;

	private Renderer[] previewRens = new Renderer[0];

	private Coroutine placingRoutine;

	private TileObject ObjectAttacking
	{
		get
		{
			return _objectAttacking;
		}
		set
		{
			_objectAttacking = value;
		}
	}

	public bool IsPlacingDeed
	{
		get
		{
			return _isPlacingDeed;
		}
		set
		{
			_isPlacingDeed = value;
		}
	}

	public HouseDetails InsideHouseDetails { get; set; }

	public bool IsInsidePlayerHouse
	{
		get
		{
			return _isInsidePlayerHouse;
		}
		set
		{
			_isInsidePlayerHouse = value;
		}
	}

	public bool ScheduleForRefreshSelection { get; set; }

	public Vector2 NetworkcurrentlyAttackingPos
	{
		get
		{
			return currentlyAttackingPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyAttackingPos))
			{
				Vector2 oldPos = currentlyAttackingPos;
				SetSyncVar(value, ref currentlyAttackingPos, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnChangeAttackingPos(oldPos, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public void Start()
	{
		myMove = GetComponent<CharMovement>();
		myEquip = GetComponent<EquipItemToChar>();
	}

	public override void OnStartClient()
	{
		if (base.isLocalPlayer)
		{
			InitializeTileHighlighter();
		}
		else
		{
			Object.Destroy(tileHighlighter.gameObject);
		}
	}

	private void InitializeTileHighlighter()
	{
		tileHighlighter.parent = null;
		tileHighlighter.transform.rotation = Quaternion.identity;
		highLighterChange = tileHighlighter.GetComponent<TileHighlighter>();
		highLighterChange.setAtHighliter();
		StartCoroutine(HidePreviewInMenuAndConvo());
	}

	public void Update()
	{
		if (base.isLocalPlayer)
		{
			if (!IsStillAttemptingToPlaceDeed())
			{
				SetNotPlacingDeed();
			}
			UpdateTilePositionOfTileHighlighter();
			LerpHighlighterPosition();
			LerpObjectPlacedPreview();
		}
	}

	private void UpdateTilePositionOfTileHighlighter()
	{
		Vector3 highlighterPrecisePosition = GetHighlighterPrecisePosition();
		int num = (int)(Mathf.Round(highlighterPrecisePosition.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(highlighterPrecisePosition.z + 0.5f) / 2f);
		if (!IsInsidePlayerHouse)
		{
			num = Mathf.Clamp(num, 0, WorldManager.Instance.GetMapSize() - 1);
			num2 = Mathf.Clamp(num2, 0, WorldManager.Instance.GetMapSize() - 1);
		}
		if (IsSelectionScheduledForRefresh(num, num2))
		{
			RefreshTileSelection(num, num2);
		}
	}

	private Vector3 GetHighlighterPrecisePosition()
	{
		Vector3 position = base.transform.position;
		if (IsPlacingDeed)
		{
			position += selectPositionOffset;
			position += GetHighlighterPosDifFromHoldingDeed();
		}
		else
		{
			position += base.transform.forward * 2f;
		}
		if (IsInsidePlayerHouse)
		{
			if ((object)playerHouseTransform != null)
			{
				position -= playerHouseTransform.position;
			}
			if ((object)insideHouseDisplay != null)
			{
				return ReturnPrecisePositionClampedInsideHouse(position);
			}
		}
		return position;
	}

	private Vector3 ReturnPrecisePositionClampedInsideHouse(Vector3 posPrecise)
	{
		if (myEquip.CurrentlyHoldingMultiTiledPlaceableItem() && IsPlacingDeed)
		{
			int num = myEquip.itemCurrentlyHolding.placeable.GetXSize();
			int num2 = myEquip.itemCurrentlyHolding.placeable.GetYSize();
			if (placingObjectRotation == 2 || placingObjectRotation == 4)
			{
				num = myEquip.itemCurrentlyHolding.placeable.GetYSize();
				num2 = myEquip.itemCurrentlyHolding.placeable.GetXSize();
			}
			return new Vector3(Mathf.Clamp(posPrecise.x, 0f, insideHouseDisplay.xLength * 2 - 2 - num), posPrecise.y, Mathf.Clamp(posPrecise.z, 0f, insideHouseDisplay.yLength * 2 - 2 - num2));
		}
		return new Vector3(Mathf.Clamp(posPrecise.x, 0f, insideHouseDisplay.xLength * 2 - 2), posPrecise.y, Mathf.Clamp(posPrecise.z, 0f, insideHouseDisplay.yLength * 2 - 2));
	}

	private bool IsSelectionScheduledForRefresh(int tileX, int tileY)
	{
		if (!ScheduleForRefreshSelection && (int)selectedTile.x == tileX && (int)selectedTile.y == tileY)
		{
			return lastEquip != myEquip.itemCurrentlyHolding;
		}
		return true;
	}

	private bool IsStillAttemptingToPlaceDeed()
	{
		if (ConversationManager.manage.IsConversationActive)
		{
			return true;
		}
		if (InputMaster.input.UICancel() || (Inventory.Instance.usingMouse && InputMaster.input.Interact()) || !myEquip.itemCurrentlyHolding || !myEquip.itemCurrentlyHolding.placeable || (!IsInsidePlayerHouse && myEquip.IsCurrentlyHoldingSinglePlaceableItem() && myEquip.itemCurrentlyHolding.placeable.GetRotationFromMap()))
		{
			return false;
		}
		return true;
	}

	public bool RotateObjectBeingPlacedPreview()
	{
		if (myEquip.IsDriving() || ConversationManager.manage.IsConversationActive)
		{
			return false;
		}
		if (myEquip.IsCurrentlyHoldingItem && ((myEquip.IsCurrentlyHoldingSinglePlaceableItem() && myEquip.itemCurrentlyHolding.placeable.GetRotationFromMap()) || (myEquip.CurrentlyHoldingMultiTiledPlaceableItem() && IsPlacingDeed) || (bool)myEquip.itemCurrentlyHolding.spawnPlaceable))
		{
			RotateObjectBeingPlaced();
			RefreshPreview((int)selectedTile.x, (int)selectedTile.y);
			SoundManager.Instance.play2DSound(SoundManager.Instance.rotationSound);
			return true;
		}
		return false;
	}

	private void RotateObjectBeingPlaced()
	{
		placingObjectRotation++;
		if (placingObjectRotation > 4)
		{
			placingObjectRotation = 1;
		}
	}

	private IEnumerator HidePreviewInMenuAndConvo()
	{
		while (true)
		{
			if ((object)previewObject != null && !Inventory.Instance.CanMoveCharacter())
			{
				RefreshPreview((int)selectedTile.x, (int)selectedTile.y);
				while (!Inventory.Instance.CanMoveCharacter())
				{
					yield return null;
				}
				RefreshPreview((int)selectedTile.x, (int)selectedTile.y);
			}
			yield return null;
		}
	}

	public bool GetSelectedTileNeedsServerRefresh()
	{
		if (selectedTile != currentlyAttackingPos)
		{
			return true;
		}
		return false;
	}

	public bool CanCurrentTileBePickedUp()
	{
		if (myEquip.IsInsideNonPlayerHouse())
		{
			return false;
		}
		if ((bool)ObjectAttacking && ObjectAttacking.canBePickedUp())
		{
			if ((bool)WorldManager.Instance.allObjects[ObjectAttacking.tileObjectId].tileObjectItemChanger && WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] >= 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public TilePromptType GetTileCloseToMyPosNeedsPrompt(out int xPos, out int yPos)
	{
		if (myEquip.IsInsideNonPlayerHouse())
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y;
			return TilePromptType.None;
		}
		TilePromptType tilePopUp = GetTilePopUp((int)selectedTile.x, (int)selectedTile.y);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y;
			return tilePopUp;
		}
		tilePopUp = GetTilePopUp((int)selectedTile.x - 1, (int)selectedTile.y);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x - 1;
			yPos = (int)selectedTile.y;
			return tilePopUp;
		}
		tilePopUp = GetTilePopUp((int)selectedTile.x + 1, (int)selectedTile.y);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x + 1;
			yPos = (int)selectedTile.y;
			return tilePopUp;
		}
		tilePopUp = GetTilePopUp((int)selectedTile.x, (int)selectedTile.y - 1);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y - 1;
			return tilePopUp;
		}
		tilePopUp = GetTilePopUp((int)selectedTile.x, (int)selectedTile.y + 1);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y + 1;
			return tilePopUp;
		}
		xPos = (int)selectedTile.x;
		yPos = (int)selectedTile.x;
		return TilePromptType.None;
	}

	public string GetTranslatedStringForTilePromptType(TilePromptType type, int xPos, int yPos)
	{
		return type switch
		{
			TilePromptType.Harvest => (LocalizedString)"ToolTips/Tip_Harvest", 
			TilePromptType.ItemName => WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestDrop.getInvItemName(), 
			TilePromptType.Open => (LocalizedString)"ToolTips/Tip_Open", 
			TilePromptType.Close => (LocalizedString)"ToolTips/Tip_Close", 
			TilePromptType.None => string.Empty, 
			_ => string.Empty, 
		};
	}

	private TilePromptType GetTilePopUp(int interactX, int interactY)
	{
		if (!WorldManager.Instance.isPositionOnMap(interactX, interactY))
		{
			return TilePromptType.None;
		}
		int num = WorldManager.Instance.heightMap[interactX, interactY];
		bool flag = false;
		if (WorldManager.Instance.onTileMap[interactX, interactY] < -1)
		{
			Vector2 vector = WorldManager.Instance.findMultiTileObjectPos(interactX, interactY);
			interactX = (int)vector.x;
			interactY = (int)vector.y;
			flag = true;
		}
		if (WorldManager.Instance.onTileMap[interactX, interactY] > -1)
		{
			TileObject tileObject = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[interactX, interactY]];
			if (Vector3.Distance(new Vector3(interactX * 2, base.transform.position.y, interactY * 2), base.transform.position) >= 5f)
			{
				return TilePromptType.None;
			}
			if (Vector3.Dot(base.transform.forward, (new Vector3(interactX * 2, base.transform.position.y, interactY * 2) - base.transform.position).normalized) < 0.7f && !flag)
			{
				return TilePromptType.None;
			}
			if (base.transform.position.y < (float)num + 1.5f && base.transform.position.y > (float)num - 1.5f)
			{
				if ((bool)tileObject && (bool)tileObject.tileObjectGrowthStages && tileObject.tileObjectGrowthStages.canBeHarvested(WorldManager.Instance.onTileStatusMap[interactX, interactY]))
				{
					if (tileObject.tileObjectGrowthStages.normalPickUp)
					{
						return TilePromptType.ItemName;
					}
					return TilePromptType.Harvest;
				}
				if ((bool)tileObject && (bool)tileObject.tileOnOff && interactX == (int)selectedTile.x && interactY == (int)selectedTile.y && tileObject.tileOnOff.isGate && !tileObject.tileOnOff.requiredToOpen)
				{
					if (tileObject.tileOnOff.isOpen)
					{
						return TilePromptType.Close;
					}
					return TilePromptType.Open;
				}
			}
			return TilePromptType.None;
		}
		return TilePromptType.None;
	}

	public void doDamageToolPos(Vector3 newToolPos)
	{
		int num = Mathf.RoundToInt(newToolPos.x / 2f);
		int num2 = Mathf.RoundToInt(newToolPos.z / 2f);
		selectedTileBeforeToolUse = selectedTile;
		selectedTile = new Vector2(num, num2);
		RefreshTileSelection(num, num2);
		doDamage(useStamina: false);
		selectedTile = selectedTileBeforeToolUse;
		RefreshTileSelection((int)selectedTile.x, (int)selectedTile.y);
		ScheduleForRefreshSelection = true;
	}

	public void DoTileInteraction()
	{
		if (!DoTileInteractionOnTilePos((int)selectedTile.x, (int)selectedTile.y) && !DoTileInteractionOnTilePos((int)selectedTile.x - 1, (int)selectedTile.y) && !DoTileInteractionOnTilePos((int)selectedTile.x + 1, (int)selectedTile.y) && !DoTileInteractionOnTilePos((int)selectedTile.x, (int)selectedTile.y - 1))
		{
			DoTileInteractionOnTilePos((int)selectedTile.x, (int)selectedTile.y + 1);
		}
	}

	public bool DoTileInteractionOnTilePos(int tileX, int tileY)
	{
		if (InsideHouseDetails != null)
		{
			return false;
		}
		int num = WorldManager.Instance.heightMap[tileX, tileY];
		bool flag = false;
		if (WorldManager.Instance.onTileMap[tileX, tileY] < -1)
		{
			Vector2 vector = WorldManager.Instance.findMultiTileObjectPos(tileX, tileY);
			tileX = (int)vector.x;
			tileY = (int)vector.y;
			flag = true;
		}
		if (WorldManager.Instance.onTileMap[tileX, tileY] > -1)
		{
			TileObject tileObject = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[tileX, tileY]];
			if (Vector3.Distance(new Vector3(tileX * 2, base.transform.position.y, tileY * 2), base.transform.position) >= 5f)
			{
				return false;
			}
			if (Vector3.Dot(base.transform.forward, (new Vector3(tileX * 2, base.transform.position.y, tileY * 2) - base.transform.position).normalized) < 0.7f && !flag)
			{
				return false;
			}
			if (base.transform.position.y < (float)num + 1.5f && base.transform.position.y > (float)num - 1.5f)
			{
				if (!IsPlacingDeed && (bool)tileObject && (bool)tileObject.tileOnOff && CheckClientLocked() && tileObject.tileOnOff.isGate && !tileObject.tileOnOff.requiredToOpen)
				{
					WorldManager.Instance.lockTileClient(tileX, tileY);
					CmdOpenClose(tileX, tileY);
					return true;
				}
				if (!IsPlacingDeed && (bool)tileObject && (bool)tileObject.tileObjectGrowthStages && CheckClientLocked() && tileObject.tileObjectGrowthStages.canBeHarvested(WorldManager.Instance.onTileStatusMap[tileX, tileY]))
				{
					WorldManager.Instance.lockTileClient(tileX, tileY);
					if (tileObject.tileObjectGrowthStages.needsTilledSoil || tileObject.tileObjectGrowthStages.isAPlantSproutFromAFarmPlant(tileObject.tileObjectId))
					{
						if (tileObject.tileObjectGrowthStages.diesOnHarvest)
						{
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(tileObject.tileObjectGrowthStages.objectStages.Length / 3, 1, 12));
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
						}
						else
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(tileObject.tileObjectGrowthStages.objectStages.Length / 8, 1, 12));
						}
					}
					else if (tileObject.tileObjectGrowthStages.mustBeInWater)
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Fishing, 3);
					}
					else if ((bool)tileObject.tileObjectGrowthStages.harvestDrop && (!tileObject.tileObjectGrowthStages.harvestDrop.placeable || tileObject.tileObjectGrowthStages.harvestDrop.placeable.tileObjectId != tileObject.tileObjectId))
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Foraging, 1);
					}
					if (!tileObject.tileObjectGrowthStages.normalPickUp && !tileObject.tileObjectGrowthStages.autoPickUpOnHarvest)
					{
						InputMaster.input.harvestRumble();
						CmdHarvestOnTile(tileX, tileY, pickedUpAuto: false);
						DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, tileObject.tileObjectId, tileObject.tileObjectGrowthStages.harvestSpots.Length);
						return true;
					}
					int crabTrapDrop;
					if (tileObject.tileObjectGrowthStages.isCrabPot)
					{
						crabTrapDrop = tileObject.tileObjectGrowthStages.getCrabTrapDrop(tileX, tileY);
						if (Inventory.Instance.addItemToInventory(crabTrapDrop, 1))
						{
							if ((bool)Inventory.Instance.allItems[crabTrapDrop].underwaterCreature)
							{
								CharLevelManager.manage.addToDayTally(crabTrapDrop, 1, 3);
							}
							SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
							InputMaster.input.harvestRumble();
							CmdHarvestOnTile(tileX, tileY, pickedUpAuto: true);
							return true;
						}
						NotificationManager.manage.turnOnPocketsFullNotification();
						InputMaster.input.harvestRumble();
						CmdHarvestCrabPot(tileX, tileY, crabTrapDrop);
						return true;
					}
					crabTrapDrop = ((!tileObject.tileObjectGrowthStages.dropsFromLootTable) ? Inventory.Instance.getInvItemId(tileObject.tileObjectGrowthStages.harvestDrop) : Inventory.Instance.getInvItemId(tileObject.tileObjectGrowthStages.dropsFromLootTable.getRandomDropFromTable()));
					if (crabTrapDrop != -1)
					{
						if (Inventory.Instance.addItemToInventory(crabTrapDrop, 1))
						{
							CharLevelManager.manage.addToDayTally(crabTrapDrop, 1, 1);
							SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
							InputMaster.input.harvestRumble();
							CmdHarvestOnTile(tileX, tileY, pickedUpAuto: true);
							DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, tileObject.tileObjectId);
							return true;
						}
						NotificationManager.manage.turnOnPocketsFullNotification();
						InputMaster.input.harvestRumble();
						CmdHarvestOnTile(tileX, tileY, pickedUpAuto: false);
						DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, tileObject.tileObjectId);
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public void pickUpTileObject()
	{
		if (myEquip.IsInsideNonPlayerHouse() || ((IsPlacingDeed || !ObjectAttacking || !ObjectAttacking.canBePickedUp()) && (IsPlacingDeed || !ObjectAttacking || !ObjectAttacking.tileObjectBridge || !WorldManager.Instance.waterMap[(int)selectedTile.x, (int)selectedTile.y])))
		{
			return;
		}
		if (IsInsidePlayerHouse)
		{
			Vector2 vector = WorldManager.Instance.findMultiTileObjectPos((int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails);
			int num = InsideHouseDetails.houseMapOnTile[(int)vector.x, (int)vector.y];
			if (ObjectAttacking == null)
			{
				ItemOnTopManager.manage.hasItemsOnTop(ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails);
			}
			if (ObjectAttacking.canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails))
			{
				int num2 = ObjectAttacking.returnClosestPositionWithItemOnTop(tileHighlighter.position, ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails);
				if (ItemOnTopManager.manage.getItemOnTopInPosition(num2, ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails) != null)
				{
					ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(num2, ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails);
					if (!WorldManager.Instance.checkTileClientLockHouse(ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos))
					{
						if (!WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].pickUpRequiresEmptyPocket)
						{
							WorldManager.Instance.lockTileHouseClient(ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
							CmdPickUpObjectOnTopOfInside(num2, ObjectAttacking.xPos, ObjectAttacking.yPos);
						}
						else if (Inventory.Instance.checkIfItemCanFit(itemOnTopInPosition.getStatus(), 1))
						{
							WorldManager.Instance.lockTileHouseClient(ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
							CmdPickUpObjectOnTopOfInside(num2, ObjectAttacking.xPos, ObjectAttacking.yPos);
						}
						else
						{
							NotificationManager.manage.turnOnPocketsFullNotification();
						}
					}
				}
				else
				{
					NotificationManager.manage.pocketsFull.showMustBeEmpty();
				}
			}
			else if (num > -1 && (bool)WorldManager.Instance.allObjects[num].tileObjectChest && !WorldManager.Instance.allObjects[num].tileObjectChest.checkIfEmpty((int)vector.x, (int)vector.y, InsideHouseDetails))
			{
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
			else if (num > -1 && (bool)WorldManager.Instance.allObjects[num].tileObjectItemChanger && InsideHouseDetails.houseMapOnTileStatus[(int)vector.x, (int)vector.y] > 0)
			{
				if (base.isServer)
				{
					WorldManager.Instance.checkIfTileHasChanger((int)vector.x, (int)vector.y, InsideHouseDetails);
				}
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
			else if (num > -1 && (bool)WorldManager.Instance.allObjects[num].tileObjectFurniture && InsideHouseDetails.houseMapOnTileStatus[(int)selectedTile.x, (int)selectedTile.y] >= 1)
			{
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
			else
			{
				if (num <= -1 || WorldManager.Instance.checkTileClientLockHouse((int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails.xPos, InsideHouseDetails.yPos))
				{
					return;
				}
				if (!WorldManager.Instance.allObjectSettings[num].pickUpRequiresEmptyPocket || (WorldManager.Instance.allObjectSettings[num].pickUpRequiresEmptyPocket && Inventory.Instance.checkIfItemCanFit(InsideHouseDetails.houseMapOnTileStatus[(int)vector.x, (int)vector.y], 1)))
				{
					if (!WorldManager.Instance.checkTileClientLockHouse((int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails.xPos, InsideHouseDetails.yPos))
					{
						WorldManager.Instance.lockTileHouseClient((int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
						CmdPickUpOnTileInside((int)selectedTile.x, (int)selectedTile.y, playerHouseTransform.position.y);
					}
				}
				else
				{
					NotificationManager.manage.pocketsFull.showPocketsFull(isHolding: false);
				}
			}
		}
		else if (ObjectAttacking.canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(ObjectAttacking.xPos, ObjectAttacking.yPos))
		{
			int num3 = ObjectAttacking.returnClosestPositionWithItemOnTop(tileHighlighter.position, ObjectAttacking.xPos, ObjectAttacking.yPos, null);
			if (ItemOnTopManager.manage.getItemOnTopInPosition(num3, ObjectAttacking.xPos, ObjectAttacking.yPos, null) != null)
			{
				if (myEquip.myPermissions.CheckIfCanDamgeTiles())
				{
					ItemOnTop itemOnTopInPosition2 = ItemOnTopManager.manage.getItemOnTopInPosition(num3, ObjectAttacking.xPos, ObjectAttacking.yPos, null);
					if (!WorldManager.Instance.CheckTileClientLock(ObjectAttacking.xPos, ObjectAttacking.yPos))
					{
						if (!WorldManager.Instance.allObjectSettings[itemOnTopInPosition2.getTileObjectId()].pickUpRequiresEmptyPocket)
						{
							WorldManager.Instance.lockTileClient(ObjectAttacking.xPos, ObjectAttacking.yPos);
							CmdPickUpObjectOnTopOf(num3, ObjectAttacking.xPos, ObjectAttacking.yPos);
						}
						else if (Inventory.Instance.checkIfItemCanFit(itemOnTopInPosition2.getStatus(), 1))
						{
							WorldManager.Instance.lockTileClient(ObjectAttacking.xPos, ObjectAttacking.yPos);
							CmdPickUpObjectOnTopOf(num3, ObjectAttacking.xPos, ObjectAttacking.yPos);
						}
						else
						{
							NotificationManager.manage.turnOnPocketsFullNotification();
						}
					}
				}
				else
				{
					NotificationManager.manage.pocketsFull.showMustBeEmpty();
				}
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
		}
		else if ((bool)ObjectAttacking && (bool)WorldManager.Instance.allObjects[ObjectAttacking.tileObjectId].tileObjectChest && !WorldManager.Instance.allObjects[ObjectAttacking.tileObjectId].tileObjectChest.checkIfEmpty((int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails))
		{
			NotificationManager.manage.pocketsFull.showMustBeEmpty();
		}
		else if ((bool)WorldManager.Instance.allObjects[ObjectAttacking.tileObjectId].tileObjectFurniture && WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] >= 1)
		{
			NotificationManager.manage.pocketsFull.showMustBeEmpty();
		}
		else if ((bool)WorldManager.Instance.allObjects[ObjectAttacking.tileObjectId].tileObjectItemChanger && WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] > 0)
		{
			if (base.isServer)
			{
				WorldManager.Instance.checkIfTileHasChanger((int)selectedTile.x, (int)selectedTile.y);
			}
			NotificationManager.manage.pocketsFull.showMustBeEmpty();
		}
		else
		{
			if (!CheckClientLocked())
			{
				return;
			}
			if (myEquip.myPermissions.CheckIfCanDamgeTiles())
			{
				if ((bool)ObjectAttacking && WorldManager.Instance.allObjectSettings[ObjectAttacking.tileObjectId].pickUpRequiresEmptyPocket)
				{
					if (WorldManager.Instance.allObjectSettings[ObjectAttacking.tileObjectId].dropsStatusNumberOnDeath)
					{
						if (Inventory.Instance.checkIfItemCanFit(WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y], 1))
						{
							WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
							CmdPickUpOnTile((int)selectedTile.x, (int)selectedTile.y);
						}
						else
						{
							NotificationManager.manage.turnOnPocketsFullNotification();
						}
					}
				}
				else
				{
					WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
					CmdPickUpOnTile((int)selectedTile.x, (int)selectedTile.y);
				}
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
		}
	}

	public void SpawnPlaceableObject()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (!canPlaceVehiclePreview)
		{
			if (tileHighlighter.position.y > base.transform.position.y + 2f)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
				return;
			}
			CmdSpawnPlaceable(new Vector3(tileHighlighter.transform.position.x, base.transform.position.y + 1.4f, tileHighlighter.transform.position.z), Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding));
			Inventory.Instance.consumeItemInHand();
		}
		else if ((bool)canPlaceVehiclePreview && canPlaceVehiclePreview.canBePlaced)
		{
			CmdSpawnVehicle(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), placingObjectRotation, Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].stack - 1);
			Inventory.Instance.consumeItemInHand();
		}
		else
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
		}
	}

	public void doDamage(bool useStamina = true)
	{
		if (base.isLocalPlayer && useStamina)
		{
			RefreshTileSelection((int)selectedTile.x, (int)selectedTile.y);
		}
		bool flag = true;
		if (!base.isLocalPlayer)
		{
			if (WorldManager.Instance.onTileMap[(int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y] != currentlyAttackingId)
			{
				OnChangeAttackingPos(currentlyAttackingPos, currentlyAttackingPos);
			}
			if ((bool)ObjectAttacking && CheckIfCanDamage(currentlyAttackingPos))
			{
				if (myMove.stamina <= 10 && useStamina)
				{
					StatusManager.manage.sweatParticlesNotLocal(base.transform.position);
				}
				ObjectAttacking.damage();
				ObjectAttacking.currentHealth -= myEquip.itemCurrentlyHolding.damagePerAttack;
				Vector3 position = ObjectAttacking.transform.position;
				ParticleManager.manage.emitAttackParticle(position);
			}
			return;
		}
		if (IsInsidePlayerHouse)
		{
			if (((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.equipable && myEquip.itemCurrentlyHolding.equipable.wallpaper) || ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.equipable && myEquip.itemCurrentlyHolding.equipable.flooring))
			{
				if ((myEquip.itemCurrentlyHolding.equipable.wallpaper && Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].stack == 1) || (myEquip.itemCurrentlyHolding.equipable.wallpaper && Inventory.Instance.checkIfItemCanFit(Inventory.Instance.wallSlot.itemNo, 1)))
				{
					if (myEquip.itemCurrentlyHolding.getItemId() != Inventory.Instance.wallSlot.itemNo)
					{
						int itemNo = Inventory.Instance.wallSlot.itemNo;
						CmdUpdateHouseWall(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), InsideHouseDetails.xPos, InsideHouseDetails.yPos);
						Inventory.Instance.consumeItemInHand();
						Inventory.Instance.addItemToInventory(itemNo, 1);
					}
				}
				else if ((myEquip.itemCurrentlyHolding.equipable.flooring && Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].stack == 1) || (myEquip.itemCurrentlyHolding.equipable.flooring && Inventory.Instance.checkIfItemCanFit(Inventory.Instance.floorSlot.itemNo, 1)))
				{
					if (myEquip.itemCurrentlyHolding.getItemId() != Inventory.Instance.floorSlot.itemNo)
					{
						int itemNo2 = Inventory.Instance.floorSlot.itemNo;
						CmdUpdateHouseFloor(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), InsideHouseDetails.xPos, InsideHouseDetails.yPos);
						Inventory.Instance.consumeItemInHand();
						Inventory.Instance.addItemToInventory(itemNo2, 1);
					}
				}
				else
				{
					NotificationManager.manage.turnOnPocketsFullNotification();
				}
			}
			else if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.placeable && myEquip.itemCurrentlyHolding.placeable.canBePlacedOntoFurniture() && (bool)ObjectAttacking && ObjectAttacking.canBePlaceOn())
			{
				if (!IsPlacingDeed)
				{
					SetPlacingDeed();
					return;
				}
				int num = ObjectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
				if (ItemOnTopManager.manage.getItemOnTopInPosition(num, ObjectAttacking.xPos, ObjectAttacking.yPos, InsideHouseDetails) == null)
				{
					int status = 0;
					if (myEquip.usesHandPlaceable())
					{
						status = myEquip.itemCurrentlyHolding.getItemId();
					}
					CmdPlaceItemOnTopOfInside(myEquip.itemCurrentlyHolding.placeable.tileObjectId, num, status, previewShowingRot, ObjectAttacking.xPos, ObjectAttacking.yPos);
					Inventory.Instance.consumeItemInHand();
				}
			}
			else if (myEquip.CurrentlyHoldingMultiTiledPlaceableItem())
			{
				if (IsHighlighterBlockedByVehicleOrCarryItem())
				{
					return;
				}
				if (!IsPlacingDeed)
				{
					SetPlacingDeed();
				}
				else if (myEquip.itemCurrentlyHolding.placeable.CheckIfMultiTileObjectCanBePlacedInside(InsideHouseDetails, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation))
				{
					CmdPlayPlaceableAnimation();
					int tileObjectId = myEquip.itemCurrentlyHolding.placeable.tileObjectId;
					CmdChangeOnTileInside(tileObjectId, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation);
					if (tileObjectId > -1 && WorldManager.Instance.allObjectSettings[tileObjectId].dropsStatusNumberOnDeath)
					{
						CmdGiveStatusInside(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), (int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
					}
					if (!myEquip.itemCurrentlyHolding.isATool)
					{
						Inventory.Instance.consumeItemInHand();
					}
					ScheduleForRefreshSelection = true;
				}
			}
			else
			{
				if (!myEquip.IsCurrentlyHoldingSinglePlaceableItem() || IsHighlighterBlockedByVehicleOrCarryItem())
				{
					return;
				}
				if (!IsPlacingDeed)
				{
					SetPlacingDeed();
				}
				else if (CheckIfCanDamageInside(selectedTile))
				{
					CmdPlayPlaceableAnimation();
					int tileObjectId2 = myEquip.itemCurrentlyHolding.placeable.tileObjectId;
					CmdChangeOnTileInside(tileObjectId2, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation);
					if (tileObjectId2 > -1 && WorldManager.Instance.allObjectSettings[tileObjectId2].dropsStatusNumberOnDeath)
					{
						CmdGiveStatusInside(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), (int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
					}
					if (!myEquip.itemCurrentlyHolding.isATool)
					{
						Inventory.Instance.consumeItemInHand();
					}
					ScheduleForRefreshSelection = true;
				}
			}
			return;
		}
		if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.placeable && myEquip.itemCurrentlyHolding.placeable.canBePlacedOntoFurniture() && (bool)ObjectAttacking && ObjectAttacking.canBePlaceOn())
		{
			int num2 = ObjectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
			if (ItemOnTopManager.manage.getItemOnTopInPosition(num2, ObjectAttacking.xPos, ObjectAttacking.yPos, null) == null)
			{
				WorldManager.Instance.lockTileClient(ObjectAttacking.xPos, ObjectAttacking.yPos);
				int status2 = 0;
				if (WorldManager.Instance.allObjectSettings[myEquip.itemCurrentlyHolding.placeable.tileObjectId].dropsStatusNumberOnDeath)
				{
					status2 = myEquip.itemCurrentlyHolding.getItemId();
				}
				CmdPlaceItemOnTopOf(myEquip.itemCurrentlyHolding.placeable.tileObjectId, num2, status2, previewShowingRot, ObjectAttacking.xPos, ObjectAttacking.yPos);
				Inventory.Instance.consumeItemInHand();
			}
		}
		else if (canAttackSelectedTile && myEquip.itemCurrentlyHolding.placeableTileType != -1 && CheckClientLocked())
		{
			if (myEquip.myPermissions.CheckIfCanTerraform())
			{
				if (!myEquip.itemCurrentlyHolding.hasFuel || Inventory.Instance.hasFuelAndCanBeUsed())
				{
					if ((WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.getResultingPlaceableTileType(WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y])].isWetFertilizedDirt || WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.getResultingPlaceableTileType(WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y])].isWetTilledDirt) && WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y] > -1 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].tileObjectGrowthStages && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].tileObjectGrowthStages.needsTilledSoil)
					{
						DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.WaterCrops);
						if (Random.Range(0, 5) == 2)
						{
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, 1);
						}
					}
					if (WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].isGrass && !myEquip.itemCurrentlyHolding.ignoreOnTileObject)
					{
						ChangeTile();
					}
					myEquip.itemCurrentlyHolding.checkForTask();
					ChangeTileType(myEquip.itemCurrentlyHolding.getResultingPlaceableTileType(WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]));
					CmdPlayPlaceableAnimation();
					WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
				}
				if (!myEquip.itemCurrentlyHolding.isATool)
				{
					Inventory.Instance.consumeItemInHand();
				}
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
		}
		else if (myEquip.IsCurrentlyHoldingSinglePlaceableItem() && !myEquip.itemCurrentlyHolding.burriedPlaceable && CheckClientLocked() && canAttackSelectedTile)
		{
			if (IsHighlighterBlockedByVehicleOrCarryItem())
			{
				return;
			}
			if (myEquip.myPermissions.CheckIfCanDamgeTiles())
			{
				WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
				if ((bool)myEquip.itemCurrentlyHolding.placeable.tileObjectGrowthStages && myEquip.itemCurrentlyHolding.placeable.tileObjectGrowthStages.needsTilledSoil)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PlantSeeds);
				}
				CmdPlayPlaceableAnimation();
				ChangeTile(myEquip.itemCurrentlyHolding.placeable.tileObjectId, placingObjectRotation);
				Inventory.Instance.consumeItemInHand();
				ScheduleForRefreshSelection = true;
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
		}
		else if (CheckClientLocked() && myEquip.CurrentlyHoldingMultiTiledPlaceableItem())
		{
			if (IsHighlighterBlockedByVehicleOrCarryItem())
			{
				return;
			}
			if (!IsPlacingDeed)
			{
				SetPlacingDeed();
			}
			else if (myEquip.CurrentlyHoldingDeed() && !ConversationManager.manage.IsConversationActive && myEquip.itemCurrentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation))
			{
				if (base.isServer || ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.placeable && myEquip.itemCurrentlyHolding.placeable.tileObjectId == DeedManager.manage.deedsUnlockedOnStart[0].placeable.tileObjectId))
				{
					if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
					{
						MonoBehaviour.print("Not On Island deed");
						ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, myEquip.confirmDeedNotOnIsland);
					}
					else
					{
						ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, myEquip.confirmDeedConvoSO);
					}
				}
				else
				{
					ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, myEquip.confirmDeedNotServerSO);
				}
			}
			else if ((myEquip.CurrentlyHoldingDeed() && myEquip.itemCurrentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation)) || (!myEquip.CurrentlyHoldingDeed() && !myEquip.itemCurrentlyHolding.placeable.tileObjectBridge && myEquip.itemCurrentlyHolding.placeable.checkIfMultiTileObjectCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation) && myEquip.itemCurrentlyHolding.placeable.checkIfMultiTileObjectIsUnderWater(myEquip.itemCurrentlyHolding, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation)) || (!myEquip.CurrentlyHoldingDeed() && (bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge && myEquip.itemCurrentlyHolding.placeable.checkIfBridgeCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation)))
			{
				if (myEquip.myPermissions.CheckIfCanDamgeTiles())
				{
					WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
					CmdPlayPlaceableAnimation();
					if ((bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge)
					{
						int length = 2;
						if (placingObjectRotation == 1)
						{
							length = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, -1);
						}
						else if (placingObjectRotation == 2)
						{
							length = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, -1);
						}
						else if (placingObjectRotation == 3)
						{
							length = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, 1);
						}
						else if (placingObjectRotation == 4)
						{
							length = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 1);
						}
						CmdPlaceBridgeTileObject(myEquip.itemCurrentlyHolding.placeable.tileObjectId, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation, length);
					}
					else
					{
						int tileObjectId3 = myEquip.itemCurrentlyHolding.placeable.tileObjectId;
						CmdPlaceMultiTiledObject(tileObjectId3, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation);
						if (tileObjectId3 > -1 && WorldManager.Instance.allObjectSettings[tileObjectId3].dropsStatusNumberOnDeath)
						{
							CmdGiveStatus(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), (int)selectedTile.x, (int)selectedTile.y);
						}
					}
					Inventory.Instance.consumeItemInHand();
				}
				else
				{
					NotificationManager.manage.pocketsFull.ShowRequirePermission();
				}
			}
		}
		else if (checkIfPlaceableOnSelectedTileObject() && CheckClientLocked())
		{
			CmdPlaceItemInToTileObject(myEquip.itemCurrentlyHolding.statusToChangeToWhenPlacedOnTop, (int)selectedTile.x, (int)selectedTile.y);
			WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
			Inventory.Instance.consumeItemInHand();
		}
		else if ((bool)ObjectAttacking && canAttackSelectedTile)
		{
			if (myEquip.myPermissions.CheckIfCanDamgeTiles())
			{
				ObjectAttacking.currentHealth -= myEquip.itemCurrentlyHolding.damagePerAttack + (float)ObjectAttacking.getEffectedBuffLevel() * (myEquip.itemCurrentlyHolding.damagePerAttack / 3f);
				InputMaster.input.doRumble(0.2f);
				CameraController.control.shakeScreenMax(0.05f, 0.05f);
				if (!WorldManager.Instance.CheckTileClientLock((int)selectedTile.x, (int)selectedTile.y) && (bool)ObjectAttacking && (bool)ObjectAttacking.tileObjectGrowthStages && ObjectAttacking.tileObjectGrowthStages.canBeHarvested(WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y], deathCheck: true))
				{
					WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
					if (ObjectAttacking.tileObjectGrowthStages.needsTilledSoil || ObjectAttacking.tileObjectGrowthStages.isAPlantSproutFromAFarmPlant(ObjectAttacking.tileObjectId))
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, 1);
					}
					else if ((bool)ObjectAttacking)
					{
						DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, ObjectAttacking.tileObjectId);
					}
					CmdHarvestOnTileDeath((int)selectedTile.x, (int)selectedTile.y);
				}
				Vector3 position2 = ObjectAttacking.transform.position;
				ParticleManager.manage.emitAttackParticle(position2);
				ObjectAttacking.damage();
				if (ObjectAttacking.currentHealth <= 0f)
				{
					float rumbleMax = Mathf.Clamp(WorldManager.Instance.allObjectSettings[ObjectAttacking.tileObjectId].fullHealth / 15f, 0.1f, 0.5f);
					InputMaster.input.doRumble(rumbleMax, 1.5f);
					if (((bool)ObjectAttacking.tileObjectGrowthStages && ObjectAttacking.tileObjectGrowthStages.needsTilledSoil) || ((bool)ObjectAttacking.tileObjectGrowthStages && ObjectAttacking.tileObjectGrowthStages.isAPlantSproutFromAFarmPlant(ObjectAttacking.tileObjectId)))
					{
						if (ObjectAttacking.tileObjectGrowthStages.diesOnHarvest)
						{
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(ObjectAttacking.tileObjectGrowthStages.objectStages.Length / 3, 1, 12));
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
						}
						else
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(ObjectAttacking.tileObjectGrowthStages.objectStages.Length / 8, 1, 12));
						}
					}
					ChangeTile(ObjectAttacking.getTileObjectChangeToOnDeath());
					ScheduleForRefreshSelection = true;
				}
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
			if (myEquip.itemCurrentlyHolding.changeToHeightTiles != 0 && !myEquip.itemCurrentlyHolding.onlyChangeHeightPaths)
			{
				if ((bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange)
				{
					if (myEquip.myPermissions.CheckIfCanTerraform())
					{
						ChangeTileType(0);
					}
					else
					{
						NotificationManager.manage.pocketsFull.ShowRequirePermission();
					}
				}
				else if ((myEquip.itemCurrentlyHolding.changeToHeightTiles < 0 && (bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange) || (myEquip.itemCurrentlyHolding.changeToHeightTiles < 0 && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].saveUnderTile) || (myEquip.itemCurrentlyHolding.changeToHeightTiles < 0 && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].changeTileKeepUnderTile))
				{
					if (myEquip.myPermissions.CheckIfCanTerraform())
					{
						ChangeTileType(0);
					}
					else
					{
						NotificationManager.manage.pocketsFull.ShowRequirePermission();
					}
				}
				else if (myEquip.myPermissions.CheckIfCanTerraform())
				{
					ChangeTileHeigh(myEquip.itemCurrentlyHolding.changeToHeightTiles);
					if ((bool)myEquip.itemCurrentlyHolding.changeToWhenUsed && myEquip.itemCurrentlyHolding.changeToAndStillUseFuel)
					{
						Inventory.Instance.changeItemInHand();
					}
				}
				else
				{
					NotificationManager.manage.pocketsFull.ShowRequirePermission();
				}
			}
		}
		else if (canAttackSelectedTile && myEquip.itemCurrentlyHolding.changeToHeightTiles != 0)
		{
			if ((bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange)
			{
				if (myEquip.myPermissions.CheckIfCanTerraform())
				{
					ChangeTileType(0);
				}
				else
				{
					NotificationManager.manage.pocketsFull.ShowRequirePermission();
				}
			}
			else if ((myEquip.itemCurrentlyHolding.changeToHeightTiles < 0 && (bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange) || (myEquip.itemCurrentlyHolding.changeToHeightTiles < 0 && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].saveUnderTile) || (myEquip.itemCurrentlyHolding.changeToHeightTiles < 0 && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].changeTileKeepUnderTile))
			{
				if (myEquip.myPermissions.CheckIfCanTerraform())
				{
					ChangeTileType(0);
				}
				else
				{
					NotificationManager.manage.pocketsFull.ShowRequirePermission();
				}
			}
			else if (myEquip.myPermissions.CheckIfCanTerraform())
			{
				ChangeTileHeigh(myEquip.itemCurrentlyHolding.changeToHeightTiles);
				if ((bool)myEquip.itemCurrentlyHolding.changeToWhenUsed && myEquip.itemCurrentlyHolding.changeToAndStillUseFuel)
				{
					Inventory.Instance.changeItemInHand();
				}
			}
			else
			{
				NotificationManager.manage.pocketsFull.ShowRequirePermission();
			}
		}
		else if ((bool)ObjectAttacking && (bool)ObjectAttacking.tileOnOff && CheckClientLocked())
		{
			if (WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] == 0 && (bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding == ObjectAttacking.tileOnOff.requiredToOpen)
			{
				if (ObjectAttacking.tileOnOff.taskWhenUnlocked != 0)
				{
					DailyTaskGenerator.generate.doATask(ObjectAttacking.tileOnOff.taskWhenUnlocked);
				}
				Inventory.Instance.consumeItemInHand();
				CmdPlayPlaceableAnimation();
				WorldManager.Instance.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
				CmdFillFood((int)selectedTile.x, (int)selectedTile.y);
			}
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			UseStaminaForTool(useStamina);
		}
		if ((bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.hasFuel && CheckIfCanDamage(selectedTile))
		{
			Inventory.Instance.useItemWithFuel();
		}
	}

	public void UseStaminaForTool(bool useStamina)
	{
		if (!myEquip.itemCurrentlyHolding || !myEquip.itemCurrentlyHolding.isATool || ((bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.tag == "IgnoreStamina") || !useStamina)
		{
			return;
		}
		if ((bool)ObjectAttacking && WorldManager.Instance.allObjectSettings[ObjectAttacking.tileObjectId].isGrass)
		{
			if (!myEquip.itemCurrentlyHolding.isPowerTool)
			{
				StatusManager.manage.changeStamina(0f - CharLevelManager.manage.getStaminaCost(-1));
			}
			else
			{
				StatusManager.manage.changeStamina((0f - CharLevelManager.manage.getStaminaCost(-1)) / 3f);
			}
		}
		else if (!myEquip.itemCurrentlyHolding.isPowerTool)
		{
			StatusManager.manage.changeStamina(0f - myEquip.itemCurrentlyHolding.getStaminaCost());
		}
		else
		{
			StatusManager.manage.changeStamina((0f - myEquip.itemCurrentlyHolding.getStaminaCost()) / 3f);
		}
	}

	public void InsertItemInTo(ItemDepositAndChanger changer, int itemToInsert)
	{
		if (changer.canDepositThisItem(Inventory.Instance.allItems[itemToInsert], InsideHouseDetails) && ((InsideHouseDetails == null && !WorldManager.Instance.CheckTileClientLock(changer.currentXPos, changer.currentYPos)) || (InsideHouseDetails != null && !WorldManager.Instance.checkTileClientLockHouse(changer.currentXPos, changer.currentYPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos))))
		{
			Inventory.Instance.allItems[itemToInsert].itemChange.checkTask(changer.GetComponent<TileObject>().tileObjectId);
			if (IsInsidePlayerHouse)
			{
				CmdDepositItemInside(itemToInsert, changer.currentXPos, changer.currentYPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
			}
			else
			{
				CmdDepositItem(itemToInsert, changer.currentXPos, changer.currentYPos);
			}
			if (InsideHouseDetails == null)
			{
				WorldManager.Instance.lockTileClient(changer.currentXPos, changer.currentYPos);
			}
			else
			{
				WorldManager.Instance.lockTileHouseClient(changer.currentXPos, changer.currentYPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
			}
			Inventory.Instance.placeItemIntoSomething(Inventory.Instance.allItems[itemToInsert], changer);
		}
	}

	public bool checkIfPlaceableOnSelectedTileObject()
	{
		if ((bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.canBePlacedOnToTileObject.Length != 0 && WorldManager.Instance.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] <= 0)
		{
			for (int i = 0; i < myEquip.itemCurrentlyHolding.canBePlacedOnToTileObject.Length; i++)
			{
				if (myEquip.itemCurrentlyHolding.canBePlacedOnToTileObject[i].tileObjectId == WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckIfCanDamage(Vector2 selectedTile)
	{
		int num = (int)selectedTile.x;
		int num2 = (int)selectedTile.y;
		if ((bool)myEquip.itemCurrentlyHolding && !myEquip.itemCurrentlyHolding.anyHeight && (!(myEquip.transform.position.y <= (float)WorldManager.Instance.heightMap[num, num2] + 1.5f) || !(myEquip.transform.position.y >= (float)WorldManager.Instance.heightMap[num, num2] - 1.5f)))
		{
			return false;
		}
		if (!myEquip.itemCurrentlyHolding)
		{
			return false;
		}
		if (myEquip.itemCurrentlyHolding.placeOnWaterOnly && !WorldManager.Instance.waterMap[num, num2])
		{
			return false;
		}
		if (((bool)myEquip.itemCurrentlyHolding.placeable && WorldManager.Instance.onTileMap[num, num2] == -1) || ((bool)myEquip.itemCurrentlyHolding.placeable && WorldManager.Instance.onTileMap[num, num2] == 30) || ((bool)myEquip.itemCurrentlyHolding.placeable && WorldManager.Instance.onTileMap[num, num2] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass))
		{
			if (myEquip.itemCurrentlyHolding.canBePlacedOntoTileType.Length == 0)
			{
				return true;
			}
			for (int i = 0; i < myEquip.itemCurrentlyHolding.canBePlacedOntoTileType.Length; i++)
			{
				if (myEquip.itemCurrentlyHolding.canBePlacedOntoTileType[i] == WorldManager.Instance.tileTypeMap[num, num2])
				{
					return true;
				}
			}
			return false;
		}
		if (!myEquip.itemCurrentlyHolding.ignoreOnTileObject && WorldManager.Instance.onTileMap[num, num2] > -1 && WorldManager.Instance.onTileMap[num, num2] != 30)
		{
			TileObjectSettings tileObjectSettings = WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]];
			if (myEquip.itemCurrentlyHolding.placeableTileType > -1 && WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.placeableTileType].isPath && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.placeableTileType > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass && (WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.placeableTileType].isTilledDirt || WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.placeableTileType].isFertilizedDirt || WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.placeableTileType].isWetFertilizedDirt || WorldManager.Instance.tileTypes[myEquip.itemCurrentlyHolding.placeableTileType].isWetTilledDirt))
			{
				return true;
			}
			if ((tileObjectSettings.isStone && myEquip.itemCurrentlyHolding.damageStone) || (tileObjectSettings.isHardStone && myEquip.itemCurrentlyHolding.damageHardStone) || (tileObjectSettings.isWood && myEquip.itemCurrentlyHolding.damageWood) || (tileObjectSettings.isHardWood && myEquip.itemCurrentlyHolding.damageHardWood) || (tileObjectSettings.isMetal && myEquip.itemCurrentlyHolding.damageMetal) || (tileObjectSettings.isSmallPlant && myEquip.itemCurrentlyHolding.damageSmallPlants))
			{
				return true;
			}
			return false;
		}
		if (myEquip.itemCurrentlyHolding.ignoreOnTileObject || WorldManager.Instance.onTileMap[num, num2] == -1 || WorldManager.Instance.onTileMap[num, num2] == 30 || (WorldManager.Instance.onTileMap[num, num2] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[num, num2]].isGrass))
		{
			if (myEquip.itemCurrentlyHolding.canDamagePath && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isPath)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.grassGrowable && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isGrassGrowable)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.canDamageDirt && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isDirt)
			{
				if (myEquip.itemCurrentlyHolding.changeToHeightTiles != 0 && (myEquip.itemCurrentlyHolding.changeToHeightTiles != -1 || WorldManager.Instance.heightMap[num, num2] <= -5))
				{
					if (myEquip.itemCurrentlyHolding.changeToHeightTiles != 1)
					{
						return true;
					}
					_ = WorldManager.Instance.heightMap[num, num2];
					_ = 15;
				}
				return true;
			}
			if (myEquip.itemCurrentlyHolding.canDamageStone && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isStone)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.canDamageTilledDirt && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isTilledDirt)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.canDamageWetTilledDirt && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isWetTilledDirt)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.canDamageFertilizedSoil && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isFertilizedDirt)
			{
				return true;
			}
			if (myEquip.itemCurrentlyHolding.canDamageWetFertilizedSoil && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[num, num2]].isWetFertilizedDirt)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckIfCanDamageInside(Vector2 selectedTile)
	{
		if (!myEquip.itemCurrentlyHolding)
		{
			return false;
		}
		if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.placeable && !WorldManager.Instance.allObjectSettings[myEquip.itemCurrentlyHolding.placeable.tileObjectId].canBePickedUp)
		{
			return false;
		}
		if ((bool)myEquip.itemCurrentlyHolding.placeable && InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] == -1)
		{
			return true;
		}
		if ((bool)myEquip.itemCurrentlyHolding.placeable && InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] >= 0 && !WorldManager.Instance.allObjectSettings[InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y]].canBePickedUp)
		{
			return true;
		}
		return false;
	}

	private void LerpHighlighterPosition()
	{
		if (Vector3.Distance(desiredHighlighterPos, tileHighlighter.position) > 6f)
		{
			tileHighlighter.position = desiredHighlighterPos;
		}
		else
		{
			tileHighlighter.position = Vector3.Lerp(tileHighlighter.position, desiredHighlighterPos, Time.deltaTime * 15f);
		}
	}

	private void LerpObjectPlacedPreview()
	{
		if ((object)previewObject != null)
		{
			if (Vector3.Distance(desiredPreviewPos, previewObject.transform.position) > 6f)
			{
				previewObject.transform.position = desiredPreviewPos;
			}
			else
			{
				previewObject.transform.position = Vector3.Lerp(previewObject.transform.position, desiredPreviewPos, Time.deltaTime * 15f);
			}
		}
		if (vehiclePreview != null)
		{
			if (Vector3.Distance(desiredPreviewPos, vehiclePreview.transform.position) > 6f)
			{
				vehiclePreview.transform.position = desiredPreviewPos;
			}
			else
			{
				vehiclePreview.transform.position = Vector3.Lerp(vehiclePreview.transform.position, desiredPreviewPos, Time.deltaTime * 15f);
			}
		}
	}

	private void UpdateHighlighterPreview(int xPos, int yPos)
	{
		if (!myEquip.itemCurrentlyHolding)
		{
			UpdateHighlighterPreviewHoldingZeroObjects(xPos, yPos);
			return;
		}
		if (IsPlacingDeed && (bool)myEquip.itemCurrentlyHolding.placeable && myEquip.itemCurrentlyHolding.placeable.IsMultiTileObject())
		{
			UpdateHighlighterPreviewHoldingMultiTileObject(xPos, yPos);
			return;
		}
		highLighterChange.ShowNormal();
		if (IsInsidePlayerHouse)
		{
			desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2, -0.5f, yPos * 2);
		}
		else
		{
			desiredHighlighterPos = new Vector3(xPos * 2, (float)WorldManager.Instance.heightMap[xPos, yPos] - 0.5f, yPos * 2);
		}
	}

	private void UpdateHighlighterPreviewHoldingZeroObjects(int xPos, int yPos)
	{
		highLighterChange.ShowNormal();
		if (IsInsidePlayerHouse)
		{
			desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2, -0.5f, yPos * 2);
		}
		else
		{
			desiredHighlighterPos = new Vector3(xPos * 2, (float)WorldManager.Instance.heightMap[xPos, yPos] - 0.5f, yPos * 2);
		}
	}

	private void UpdateHighlighterPreviewHoldingMultiTileObject(int xPos, int yPos)
	{
		int num = myEquip.itemCurrentlyHolding.placeable.GetXSize();
		int num2 = myEquip.itemCurrentlyHolding.placeable.GetYSize();
		if (placingObjectRotation == 2 || placingObjectRotation == 4)
		{
			num = myEquip.itemCurrentlyHolding.placeable.GetYSize();
			num2 = myEquip.itemCurrentlyHolding.placeable.GetXSize();
		}
		if ((bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge)
		{
			switch (placingObjectRotation)
			{
			case 1:
			{
				int num3 = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, -1);
				if (num3 <= 15)
				{
					num2 = num3;
					yPos -= num3 - 1;
				}
				break;
			}
			case 2:
			{
				int num3 = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, -1);
				if (num3 <= 15)
				{
					num = num3;
					xPos -= num3 - 1;
				}
				break;
			}
			case 3:
			{
				int num3 = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, 1);
				if (num3 <= 15)
				{
					num2 = num3;
				}
				break;
			}
			case 4:
			{
				int num3 = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 1);
				if (num3 <= 15)
				{
					num = num3;
				}
				break;
			}
			}
		}
		highLighterChange.ShowMultiTiled(num, num2);
		if (IsInsidePlayerHouse)
		{
			desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2 + (num - 1), -0.5f, yPos * 2 + (num2 - 1));
		}
		else
		{
			desiredHighlighterPos = new Vector3(xPos * 2 + (num - 1), (float)WorldManager.Instance.heightMap[xPos, yPos] - 0.5f, yPos * 2 + (num2 - 1));
		}
	}

	public void FollowingNPCKillsItem(int xPos, int yPos, TileObject npcAttackingObject)
	{
		if ((bool)npcAttackingObject)
		{
			npcAttackingObject.onDeath();
			npcAttackingObject.addXp();
			DailyTaskGenerator.generate.doATask(WorldManager.Instance.allObjectSettings[npcAttackingObject.tileObjectId].TaskType);
		}
		CmdChangeOnTile(npcAttackingObject.getTileObjectChangeToOnDeath(), xPos, yPos);
	}

	public void ChangeTile(int newOnTile = -1, int rotation = -1)
	{
		if ((bool)ObjectAttacking)
		{
			ObjectAttacking.onDeath();
			ObjectAttacking.addXp();
			DailyTaskGenerator.generate.doATask(WorldManager.Instance.allObjectSettings[ObjectAttacking.tileObjectId].TaskType);
		}
		if (rotation != -1)
		{
			CmdSetRotation((int)selectedTile.x, (int)selectedTile.y, rotation);
		}
		CmdChangeOnTile(newOnTile, (int)selectedTile.x, (int)selectedTile.y);
		if (newOnTile > -1 && WorldManager.Instance.allObjectSettings[newOnTile].dropsStatusNumberOnDeath)
		{
			CmdGiveStatus(Inventory.Instance.getInvItemId(myEquip.itemCurrentlyHolding), (int)selectedTile.x, (int)selectedTile.y);
		}
		ScheduleForRefreshSelection = true;
	}

	public void ChangeTileHeigh(int heightDif)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SoilMover);
		InputMaster.input.doRumble(0.15f, 1.5f);
		if (heightDif > 0 && (bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.useRightHandAnim && myEquip.itemCurrentlyHolding.myAnimType == InventoryItem.typeOfAnimation.ShovelAnimation)
		{
			CmdChangeTileHeight(heightDif, myEquip.itemCurrentlyHolding.resultingTileType[0], (int)selectedTile.x, (int)selectedTile.y);
		}
		else
		{
			CmdChangeTileHeight(heightDif, WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y], (int)selectedTile.x, (int)selectedTile.y);
		}
		ScheduleForRefreshSelection = true;
	}

	public void ChangeTileType(int newTileType)
	{
		CmdChangeTileType(newTileType, (int)selectedTile.x, (int)selectedTile.y);
		ScheduleForRefreshSelection = true;
	}

	private Vector3 GetHighlighterPosDifFromHoldingDeed()
	{
		if (myEquip.CurrentlyHoldingMultiTiledPlaceableItem())
		{
			int num = myEquip.itemCurrentlyHolding.placeable.GetXSize();
			int num2 = myEquip.itemCurrentlyHolding.placeable.GetYSize();
			if (placingObjectRotation == 2 || placingObjectRotation == 4)
			{
				num = myEquip.itemCurrentlyHolding.placeable.GetYSize();
				num2 = myEquip.itemCurrentlyHolding.placeable.GetXSize();
			}
			float num3 = 0f;
			float num4 = 0f;
			if ((base.transform.position + base.transform.forward).x < base.transform.position.x - 0.8f)
			{
				num3 = -num * 2;
			}
			else if ((base.transform.position + base.transform.forward).x > base.transform.position.x + 0.8f)
			{
				num3 = 2f;
			}
			if ((base.transform.position + base.transform.forward).z < base.transform.position.z - 0.8f)
			{
				num4 = -num2 * 2 + 2;
			}
			else if ((base.transform.position + base.transform.forward).z > base.transform.position.z + 0.8f)
			{
				num4 = 1f;
			}
			if (num3 != 0f || num4 != 0f)
			{
				if (num3 == 0f)
				{
					num3 = -num;
				}
				else if (num4 == 0f)
				{
					num4 = -num2 + 1;
				}
			}
			return new Vector3(num3, 0f, num4);
		}
		return Vector3.zero;
	}

	public void MoveSelectionToMainTileForMultiTiledObject(int xPos, int yPos)
	{
		selectedTile = WorldManager.Instance.findMultiTileObjectPos(xPos, yPos, InsideHouseDetails);
	}

	private bool CheckIfOnMap(int intToCheck)
	{
		if (IsInsidePlayerHouse)
		{
			if (intToCheck >= 0 && intToCheck < 25)
			{
				return true;
			}
			return false;
		}
		if (intToCheck >= 0 && intToCheck < WorldManager.Instance.GetMapSize())
		{
			return true;
		}
		return false;
	}

	public void RefreshTileSelection(int xPos, int yPos)
	{
		lastEquip = myEquip.itemCurrentlyHolding;
		ScheduleForRefreshSelection = false;
		if ((!IsInsidePlayerHouse && WorldManager.Instance.onTileMap[xPos, yPos] <= -2) || (IsInsidePlayerHouse && CheckIfOnMap((int)selectedTile.x) && CheckIfOnMap((int)selectedTile.y) && InsideHouseDetails.houseMapOnTile[xPos, yPos] <= -2))
		{
			MoveSelectionToMainTileForMultiTiledObject(xPos, yPos);
		}
		else
		{
			selectedTile = new Vector2(xPos, yPos);
		}
		if (IsInsidePlayerHouse && (bool)insideHouseDisplay)
		{
			if (CheckIfOnMap((int)selectedTile.x) && CheckIfOnMap((int)selectedTile.y))
			{
				if (InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] > -1)
				{
					ObjectAttacking = insideHouseDisplay.tileObjectsInHouse[(int)selectedTile.x, (int)selectedTile.y];
				}
				else
				{
					ObjectAttacking = null;
				}
			}
		}
		else
		{
			ObjectAttacking = WorldManager.Instance.findTileObjectInUse((int)selectedTile.x, (int)selectedTile.y);
			TileObjectHealthBar.tile.setCurrentlyAttacking(ObjectAttacking);
		}
		if (myEquip.IsHoldingShovel())
		{
			myEquip.itemCurrentlyHolding.changeToWhenUsed = WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].uniqueShovel;
		}
		canAttackSelectedTile = CheckIfCanDamage(selectedTile);
		UpdateHighlighterPreview(xPos, yPos);
		RefreshPreview(xPos, yPos);
	}

	private void RefreshPreview(int xPos, int yPos)
	{
		if ((bool)previewObject && (!myEquip.itemCurrentlyHolding || !myEquip.itemCurrentlyHolding.placeable || (myEquip.CurrentlyHoldingDeed() && !IsPlacingDeed) || (myEquip.CurrentlyHoldingMultiTiledPlaceableItem() && !IsPlacingDeed) || (myEquip.IsCurrentlyHoldingSinglePlaceableItem() && !myEquip.itemCurrentlyHolding.placeable.GetRotationFromMap() && !IsInsidePlayerHouse) || (myEquip.IsCurrentlyHoldingSinglePlaceableItem() && IsInsidePlayerHouse && !IsPlacingDeed) || previewObject.tileObjectId != myEquip.itemCurrentlyHolding.placeable.tileObjectId || !Inventory.Instance.CanMoveCharacter()))
		{
			Object.Destroy(previewObject.gameObject);
			previewObject = null;
			tileHighlighterRotArrow.SetActive(value: false);
		}
		if (!Inventory.Instance.CanMoveCharacter())
		{
			return;
		}
		if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.spawnPlaceable && (bool)myEquip.itemCurrentlyHolding.spawnPlaceable.GetComponent<Vehicle>())
		{
			RefreshPreviewOfVehicle();
			return;
		}
		if ((bool)vehiclePreview)
		{
			Object.Destroy(vehiclePreview);
		}
		if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.placeable && !myEquip.itemCurrentlyHolding.burriedPlaceable && !previewObject)
		{
			if ((!myEquip.itemCurrentlyHolding.placeable.IsMultiTileObject() || !IsPlacingDeed) && (!myEquip.IsCurrentlyHoldingSinglePlaceableItem() || !myEquip.itemCurrentlyHolding.placeable.GetRotationFromMap() || IsInsidePlayerHouse) && (!myEquip.IsCurrentlyHoldingSinglePlaceableItem() || !IsInsidePlayerHouse || !IsPlacingDeed))
			{
				return;
			}
			GetInitialPlaceableRotation();
			previewObject = Object.Instantiate(WorldManager.Instance.allObjects[myEquip.itemCurrentlyHolding.placeable.tileObjectId]).GetComponent<TileObject>();
			previewObject._transform = previewObject.transform;
			Animator[] componentsInChildren = previewObject.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Light[] componentsInChildren2 = previewObject.gameObject.GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].gameObject.SetActive(value: false);
			}
			LineRenderer[] componentsInChildren3 = previewObject.gameObject.GetComponentsInChildren<LineRenderer>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].enabled = false;
			}
			Collider[] componentsInChildren4 = previewObject.gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren4.Length; i++)
			{
				Object.Destroy(componentsInChildren4[i]);
			}
			previewRens = previewObject.gameObject.GetComponentsInChildren<Renderer>();
		}
		if (!previewObject)
		{
			return;
		}
		if (!IsInsidePlayerHouse)
		{
			desiredPreviewPos = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		}
		else
		{
			desiredPreviewPos = playerHouseTransform.position + new Vector3(xPos * 2, 0f, yPos * 2);
		}
		if (myEquip.itemCurrentlyHolding.placeable.GetRotationFromMap() || myEquip.CurrentlyHoldingMultiTiledPlaceableItem())
		{
			if ((bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge)
			{
				int value = 4;
				if (placingObjectRotation == 1)
				{
					value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, -1);
				}
				else if (placingObjectRotation == 2)
				{
					value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, -1);
				}
				else if (placingObjectRotation == 3)
				{
					value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, 1);
				}
				else if (placingObjectRotation == 4)
				{
					value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 1);
				}
				value = Mathf.Clamp(value, 4, 15);
				desiredPreviewPos += previewObject.SetRotationNumberForPreviewObject(placingObjectRotation, value);
				if (previewShowingRot != placingObjectRotation)
				{
					if ((bool)previewObject)
					{
						previewObject.transform.position = desiredPreviewPos;
						tileHighlighter.transform.position += previewObject.SetRotationNumberForPreviewObject(placingObjectRotation, value);
					}
					previewShowingRot = placingObjectRotation;
				}
			}
			else
			{
				desiredPreviewPos += previewObject.SetRotationNumberForPreviewObject(placingObjectRotation);
				if (previewShowingRot != placingObjectRotation)
				{
					if ((bool)previewObject)
					{
						previewObject.transform.position = desiredPreviewPos;
						previewObject.SetRotationNumberForPreviewObject(placingObjectRotation);
					}
					previewShowingRot = placingObjectRotation;
				}
			}
			float num = Mathf.Clamp((float)previewObject.GetYSize() * 2f - 1f, 2f, 1000f);
			tileHighlighterRotArrow.transform.rotation = GetRotationInEurler(placingObjectRotation);
			tileHighlighterRotArrow.transform.position = tileHighlighter.transform.position + new Vector3(0f, 0.5f, 0f) + tileHighlighterRotArrow.transform.forward / 2f * num;
			tileHighlighterRotArrow.SetActive(value: true);
			if (myEquip.CurrentlyHoldingMultiTiledPlaceableItem())
			{
				UpdateHighlighterPreviewHoldingMultiTileObject(xPos, yPos);
			}
		}
		if (myEquip.IsCurrentlyHoldingSinglePlaceableItem())
		{
			if (IsInsidePlayerHouse)
			{
				if (InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] == -1)
				{
					ChangePreviewColor(previewYes);
				}
				else if (myEquip.itemCurrentlyHolding.placeable.canBePlacedOntoFurniture() && InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] > -1 && WorldManager.Instance.allObjectSettings[InsideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y]].canBePlacedOn())
				{
					int num2 = ObjectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
					if ((bool)previewObject)
					{
						desiredPreviewPos = ObjectAttacking.placedPositions[num2].position;
					}
					else if (ItemOnTopManager.manage.getItemOnTopInPosition(num2, (int)selectedTile.x, (int)selectedTile.y, InsideHouseDetails) == null)
					{
						ChangePreviewColor(previewYes);
					}
					else
					{
						ChangePreviewColor(previewNo);
					}
				}
				else
				{
					ChangePreviewColor(previewNo);
				}
			}
			else if (CheckIfCanDamage(selectedTile) || myEquip.itemCurrentlyHolding.placeable.canBePlacedOntoFurniture())
			{
				if (myEquip.itemCurrentlyHolding.placeable.canBePlacedOntoFurniture() && WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y] > -1 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].canBePlacedOn())
				{
					int num3 = ObjectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
					if ((bool)previewObject)
					{
						desiredPreviewPos = ObjectAttacking.placedPositions[num3].position;
					}
					if (ItemOnTopManager.manage.getItemOnTopInPosition(num3, (int)selectedTile.x, (int)selectedTile.y, null) == null)
					{
						ChangePreviewColor(previewYes);
					}
					else
					{
						ChangePreviewColor(previewNo);
					}
				}
				else if (!myEquip.itemCurrentlyHolding.placeable.canBePlacedOntoFurniture())
				{
					ChangePreviewColor(previewYes);
				}
				else if (CheckIfCanDamage(selectedTile))
				{
					ChangePreviewColor(previewYes);
				}
				else
				{
					ChangePreviewColor(previewNo);
				}
			}
			else if ((bool)ObjectAttacking && myEquip.itemCurrentlyHolding.canBePlacedOnToTileObject.Length != 0)
			{
				bool flag = false;
				for (int j = 0; j < myEquip.itemCurrentlyHolding.canBePlacedOnToTileObject.Length; j++)
				{
					if (myEquip.itemCurrentlyHolding.canBePlacedOnToTileObject[j].tileObjectId == ObjectAttacking.tileObjectId)
					{
						desiredPreviewPos = ObjectAttacking.loadInsidePos.position;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (WorldManager.Instance.onTileStatusMap[xPos, yPos] <= 0)
					{
						ChangePreviewColor(previewYes);
					}
					else
					{
						ChangePreviewColor(previewNo);
					}
				}
				else
				{
					ChangePreviewColor(previewNo);
				}
			}
			else
			{
				ChangePreviewColor(previewNo);
			}
			if (IsHighlighterBlockedByVehicleOrCarryItem())
			{
				ChangePreviewColor(previewNo);
			}
			return;
		}
		if (!IsInsidePlayerHouse)
		{
			if ((!myEquip.CurrentlyHoldingDeed() && !myEquip.itemCurrentlyHolding.placeable.tileObjectBridge && myEquip.itemCurrentlyHolding.placeable.checkIfMultiTileObjectCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation)) || (!myEquip.CurrentlyHoldingDeed() && (bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge && myEquip.itemCurrentlyHolding.placeable.checkIfBridgeCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation)) || (myEquip.CurrentlyHoldingDeed() && myEquip.itemCurrentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation)))
			{
				ChangePreviewColor(previewYes);
			}
			else
			{
				ChangePreviewColor(previewNo);
			}
			if (myEquip.CurrentlyHoldingDeed() && !myEquip.itemCurrentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation))
			{
				NotificationManager.manage.pocketsFull.showCanPlaceText(myEquip.itemCurrentlyHolding.placeable.getWhyCantPlaceDeedText((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation));
			}
			else if (myEquip.CurrentlyHoldingDeed())
			{
				NotificationManager.manage.pocketsFull.hidePopUp();
			}
		}
		else if (myEquip.itemCurrentlyHolding.placeable.CheckIfMultiTileObjectCanBePlacedInside(InsideHouseDetails, (int)selectedTile.x, (int)selectedTile.y, placingObjectRotation))
		{
			ChangePreviewColor(previewYes);
		}
		else
		{
			ChangePreviewColor(previewNo);
		}
		if (IsHighlighterBlockedByVehicleOrCarryItem())
		{
			ChangePreviewColor(previewNo);
		}
	}

	public bool IsHighlighterBlockedByVehicleOrCarryItem()
	{
		if (myEquip.CurrentlyHoldingBridge())
		{
			return false;
		}
		Collider[] array = ((!myEquip.IsCurrentlyHoldingSinglePlaceableItem()) ? Physics.OverlapBox(desiredHighlighterPos + Vector3.up * 1f, new Vector3(highLighterChange.centreFill.transform.localScale.x, 1f, highLighterChange.centreFill.transform.localScale.y), Quaternion.identity, tileHighlighterCollisionMask) : Physics.OverlapBox(desiredHighlighterPos + Vector3.up * 1f, Vector3.one / 2f, Quaternion.identity, tileHighlighterCollisionMask));
		if (array.Length != 0)
		{
			return true;
		}
		return false;
	}

	private Quaternion GetRotationInEurler(int rot)
	{
		return rot switch
		{
			1 => Quaternion.Euler(0f, 180f, 0f), 
			2 => Quaternion.Euler(0f, 270f, 0f), 
			3 => Quaternion.Euler(0f, 0f, 0f), 
			4 => Quaternion.Euler(0f, 90f, 0f), 
			_ => Quaternion.Euler(0f, 180f, 0f), 
		};
	}

	private void RefreshPreviewOfVehicle()
	{
		desiredPreviewPos = base.transform.position + base.transform.forward * 2f;
		int num = (int)(Mathf.Round(desiredPreviewPos.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(desiredPreviewPos.z + 0.5f) / 2f);
		if (WorldManager.Instance.heightMap[num, num2] > 0)
		{
			desiredPreviewPos = new Vector3(num * 2, WorldManager.Instance.heightMap[num, num2], num2 * 2);
		}
		else
		{
			desiredPreviewPos = new Vector3(num * 2, 1f, num2 * 2);
		}
		if (vehiclePreview == null)
		{
			vehiclePreview = Object.Instantiate(NetworkMapSharer.Instance.vehicleBoxPreview, desiredPreviewPos, Quaternion.identity);
			canPlaceVehiclePreview = vehiclePreview.GetComponent<VehiclePreview>();
		}
		if (placingObjectRotation == 1)
		{
			vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		}
		else if (placingObjectRotation == 2)
		{
			vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
		}
		else if (placingObjectRotation == 3)
		{
			vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else if (placingObjectRotation == 4)
		{
			vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
		}
	}

	private void CheckIfIsFarmAnimalHouse(int xPos, int yPos, int newId, int rotation = -1)
	{
		if (newId >= 0 && (bool)WorldManager.Instance.allObjects[newId].tileObjectAnimalHouse && !WorldManager.Instance.allObjectSettings[newId].canPlaceItemsUnderIt)
		{
			StartCoroutine(PlaceFarmAnimalWithDelay(xPos, yPos, newId, rotation));
		}
		if (newId == -1)
		{
			FarmAnimalManager.manage.removeAnimalHouse(xPos, yPos);
		}
	}

	private IEnumerator PlaceFarmAnimalWithDelay(int xPos, int yPos, int newId, int rotation = -1)
	{
		yield return null;
		FarmAnimalManager.manage.createNewAnimalHouseWithHouseId(xPos, yPos, newId, rotation);
	}

	public void ForceRequestHouse()
	{
		wasPreviouslyInside = false;
		ChangeInsideOut(IsInsidePlayerHouse, InsideHouseDetails);
	}

	public bool IsInside()
	{
		return IsInsidePlayerHouse;
	}

	public void ChangeInsideOut(bool isEntry, HouseDetails details = null)
	{
		IsInsidePlayerHouse = isEntry;
		InsideHouseDetails = details;
		if (details != null)
		{
			Inventory.Instance.wallSlot.updateSlotContentsAndRefresh(InsideHouseDetails.wall, 1);
			Inventory.Instance.floorSlot.updateSlotContentsAndRefresh(InsideHouseDetails.floor, 1);
		}
		if (InsideHouseDetails != null)
		{
			insideHouseDisplay = HouseManager.manage.findHousesOnDisplay(InsideHouseDetails.xPos, InsideHouseDetails.yPos);
			playerHouseTransform = insideHouseDisplay.getStartingPosTransform();
		}
		myEquip.setInsideOrOutside(isEntry, isEntry);
		if (!base.isServer)
		{
			if (IsInsidePlayerHouse)
			{
				CmdisInsidePlayerHouse(InsideHouseDetails.xPos, InsideHouseDetails.yPos);
			}
			else
			{
				CmdGoOutside();
			}
		}
		if (isEntry && !wasPreviouslyInside)
		{
			wasPreviouslyInside = true;
		}
		if (IsInsidePlayerHouse)
		{
			if ((bool)insideHouseDisplay)
			{
				playerHouseTransform = insideHouseDisplay.getStartingPosTransform();
				insideHouseDisplay.refreshHouseTiles();
			}
		}
		else
		{
			playerHouseTransform = null;
		}
	}

	[Command]
	public void CmdPickUpObjectOnTop(int newStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpObjectOnTop", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPickUpObjectOnTopOf(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpObjectOnTopOf", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPickUpObjectOnTopOfInside(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpObjectOnTopOfInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceItemOnTopOf(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceItemOnTopOf", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceItemOnTopOfInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(CharInteract), "RpcPlaceItemOnTopOfInside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUnlockClient(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(CharInteract), "RpcUnlockClient", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceItemOnTop(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(CharInteract), "RpcPlaceItemOnTop", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRemoveItemOnTop(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(CharInteract), "RpcRemoveItemOnTop", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRemoveItemOnTopInside(int posId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(CharInteract), "RpcRemoveItemOnTopInside", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdisInsidePlayerHouse(int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdisInsidePlayerHouse", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGoOutside()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharInteract), "CmdGoOutside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceItemInToTileObject(int newStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceItemInToTileObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnPlaceable(Vector3 spawnPos, int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(spawnPos);
		writer.WriteInt(id);
		SendCommandInternal(typeof(CharInteract), "CmdSpawnPlaceable", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnVehicle(int id, int rot, int variation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		writer.WriteInt(rot);
		writer.WriteInt(variation);
		SendCommandInternal(typeof(CharInteract), "CmdSpawnVehicle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdFixTeleport(string dir)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(dir);
		SendCommandInternal(typeof(CharInteract), "CmdFixTeleport", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeOnTile(int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdChangeOnTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdChangeOnTileInside(int newTileType, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdChangeOnTileInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpOnTile(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpOnTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpOnTileInside(int xPos, int yPos, float playerHouseTransformY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteFloat(playerHouseTransformY);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpOnTileInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestCrabPot(int xPos, int yPos, int drop)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(drop);
		SendCommandInternal(typeof(CharInteract), "CmdHarvestCrabPot", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestOnTile(int xPos, int yPos, bool pickedUpAuto)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteBool(pickedUpAuto);
		SendCommandInternal(typeof(CharInteract), "CmdHarvestOnTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdHarvestOnTileDeath(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdHarvestOnTileDeath", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdFillFood(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdFillFood", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenClose(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdOpenClose", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdChangeTileHeight(int newTileHeight, int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileHeight);
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdChangeTileHeight", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeTileType(int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdChangeTileType", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceMultiTiledObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceBridgeTileObject(int multiTiledObjectId, int xPos, int yPos, int rotation, int length)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		writer.WriteInt(length);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceBridgeTileObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdUpdateHouseWall(int itemId, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdUpdateHouseWall", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdUpdateHouseFloor(int itemId, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdUpdateHouseFloor", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSetRotation(int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdSetRotation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDepositItem(int depositItemId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(depositItemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdDepositItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDepositItemInside(int depositItemId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(depositItemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdDepositItemInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCurrentlyAttackingPos(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdCurrentlyAttackingPos", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void ChangePreviewColor(Material newColor)
	{
		if ((bool)previewObject.GetComponent<TileObjectGrowthStages>())
		{
			previewObject.GetComponent<TileObjectGrowthStages>().showOnlyFirstStageForPreview();
			previewRens = previewObject.gameObject.GetComponentsInChildren<Renderer>();
		}
		if ((bool)previewObject.GetComponent<TileObjectConnect>())
		{
			previewObject.GetComponent<TileObjectConnect>().connectToTiles((int)selectedTile.x, (int)selectedTile.y, placingObjectRotation);
			previewRens = previewObject.gameObject.GetComponentsInChildren<Renderer>();
		}
		if ((bool)previewObject.GetComponent<TileObjectBridge>())
		{
			int value = 4;
			if (placingObjectRotation == 1)
			{
				value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, -1);
			}
			else if (placingObjectRotation == 2)
			{
				value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, -1);
			}
			else if (placingObjectRotation == 3)
			{
				value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 0, 1);
			}
			else if (placingObjectRotation == 4)
			{
				value = WorldManager.Instance.allObjectSettings[0].CheckBridgeLength((int)selectedTile.x, (int)selectedTile.y, 1);
			}
			value = Mathf.Clamp(value, 3, 15);
			previewObject.GetComponent<TileObjectBridge>().setUpBridge(value);
			previewRens = previewObject.gameObject.GetComponentsInChildren<Renderer>();
		}
		Renderer[] array = previewRens;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j] = newColor;
			}
			renderer.materials = materials;
		}
	}

	private void OnChangeAttackingPos(Vector2 oldPos, Vector2 newPos)
	{
		NetworkcurrentlyAttackingPos = newPos;
		if (!base.isLocalPlayer)
		{
			if (IsInsidePlayerHouse)
			{
				ObjectAttacking = insideHouseDisplay.tileObjectsInHouse[(int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y];
			}
			else
			{
				ObjectAttacking = WorldManager.Instance.findTileObjectInUse((int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y);
			}
			currentlyAttackingId = WorldManager.Instance.onTileMap[(int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y];
		}
	}

	private void SetPlacingDeed()
	{
		IsPlacingDeed = true;
		selectPositionOffset = base.transform.forward * 2f;
		GetInitialPlaceableRotation();
		CameraController.control.SetFollowTransform(tileHighlighter);
		if (placingRoutine == null)
		{
			placingRoutine = StartCoroutine(MovePlacingObjectPreview());
		}
		RefreshPreview((int)selectedTile.x, (int)selectedTile.y);
	}

	private void SetNotPlacingDeed()
	{
		if (IsPlacingDeed)
		{
			IsPlacingDeed = false;
			selectPositionOffset = Vector3.zero;
			CameraController.control.SetFollowTransform(base.transform);
		}
	}

	private IEnumerator MovePlacingObjectPreview()
	{
		float moveTimer = 0.2f;
		bool resetToFreeCam = CameraController.control.isFreeCamOn() && myEquip.itemCurrentlyHolding.isDeed;
		if (!resetToFreeCam)
		{
			resetToFreeCam = CameraController.control.isFreeCamOn() && (bool)myEquip.itemCurrentlyHolding.placeable && (bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge;
		}
		if (resetToFreeCam)
		{
			CameraController.control.setCamDistanceForDeedPlacement();
			CameraController.control.swapFreeCam();
		}
		while (IsPlacingDeed)
		{
			yield return null;
			if (ConversationManager.manage.IsConversationActive)
			{
				continue;
			}
			float num = InputMaster.input.getLeftStick().x;
			float num2 = InputMaster.input.getLeftStick().y;
			if (num == 0f && num2 == 0f)
			{
				moveTimer = 0.25f;
			}
			if (moveTimer > 0.2f)
			{
				if (1f - Mathf.Abs(num) <= 0.25f)
				{
					if (num > 0f)
					{
						num = 1f;
					}
					if (num < 0f)
					{
						num = -1f;
					}
				}
				if (1f - Mathf.Abs(num2) <= 0.25f)
				{
					if (num2 > 0f)
					{
						num2 = 1f;
					}
					if (num2 < 0f)
					{
						num2 = -1f;
					}
				}
				if (num != 0f || num2 != 0f)
				{
					Vector3 vector = CameraController.control.transform.forward * num2;
					Vector3 vector2 = CameraController.control.transform.right * num;
					selectPositionOffset += (vector + vector2).normalized * 2f;
					if ((bool)myEquip.itemCurrentlyHolding && myEquip.itemCurrentlyHolding.isDeed)
					{
						selectPositionOffset = new Vector3(Mathf.Clamp(selectPositionOffset.x, -20f, 20f), 0f, Mathf.Clamp(selectPositionOffset.z, -20f, 20f));
					}
					else
					{
						selectPositionOffset = new Vector3(Mathf.Clamp(selectPositionOffset.x, -5f, 5f), 0f, Mathf.Clamp(selectPositionOffset.z, -4f, 4f));
					}
					moveTimer = 0f;
				}
			}
			else
			{
				moveTimer += Time.deltaTime;
			}
		}
		NotificationManager.manage.pocketsFull.hidePopUp();
		placingRoutine = null;
		if (resetToFreeCam && !CameraController.control.isFreeCamOn())
		{
			CameraController.control.swapFreeCam();
		}
	}

	private bool CheckClientLocked()
	{
		if (!WorldManager.Instance.CheckTileClientLock((int)selectedTile.x, (int)selectedTile.y))
		{
			return true;
		}
		return false;
	}

	public bool CanWalkThroughDoor()
	{
		return canWalkThroughDoor;
	}

	public void StartWalkThroughDoorTimer()
	{
		StartCoroutine(WalkThroughDoorTimer());
	}

	private IEnumerator WalkThroughDoorTimer()
	{
		canWalkThroughDoor = false;
		yield return new WaitForSeconds(0.1f);
		canWalkThroughDoor = true;
	}

	public void GetInitialPlaceableRotation()
	{
		int num = (int)(Mathf.Round(base.transform.eulerAngles.y / 90f) * 90f);
		if (num == 0 || base.transform.eulerAngles.y > 350f)
		{
			placingObjectRotation = 1;
		}
		else
		{
			switch (num)
			{
			case 90:
				placingObjectRotation = 2;
				break;
			case 180:
				placingObjectRotation = 3;
				break;
			case 270:
				placingObjectRotation = 4;
				break;
			}
		}
		if ((bool)myEquip.itemCurrentlyHolding && (bool)myEquip.itemCurrentlyHolding.placeable && (bool)myEquip.itemCurrentlyHolding.placeable.tileObjectBridge)
		{
			placingObjectRotation += 2;
			if (placingObjectRotation > 4)
			{
				placingObjectRotation -= 4;
			}
		}
	}

	public int ReturnPlacingObjectRotation()
	{
		return placingObjectRotation;
	}

	[Command]
	public void CmdPlayPlaceableAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharInteract), "CmdPlayPlaceableAnimation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlayPlaceableAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharInteract), "RpcPlayPlaceableAnimation", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGiveStatus(int newStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdGiveStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGiveStatusInside(int newStatus, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdGiveStatusInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetSendMustBeEmptyPrompt(NetworkConnection con)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendTargetRPCInternal(con, typeof(CharInteract), "TargetSendMustBeEmptyPrompt", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetGiveInvItemOnPickUpRequiresEmptyPockets(NetworkConnection con, int itemId, int stack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(stack);
		SendTargetRPCInternal(con, typeof(CharInteract), "TargetGiveInvItemOnPickUpRequiresEmptyPockets", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceSingleChestOutside(int newTileType, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceSingleChestOutside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdPickUpObjectOnTop(int newStatus, int xPos, int yPos)
	{
		if (myEquip.myPermissions.CheckIfCanDamgeTiles())
		{
			if ((bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.Instance.onTileStatusMap[xPos, yPos]].placeable)
			{
				WorldManager.Instance.allObjectSettings[WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.Instance.onTileStatusMap[xPos, yPos]].placeable.tileObjectId].removeBeauty();
			}
			int invItemId = Inventory.Instance.getInvItemId(WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.Instance.onTileStatusMap[xPos, yPos]]);
			NetworkMapSharer.Instance.spawnAServerDrop(invItemId, 1, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdPickUpObjectOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObjectOnTop called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpObjectOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpObjectOnTopOf(int posId, int xPos, int yPos)
	{
		if (!myEquip.myPermissions.CheckIfCanDamgeTiles())
		{
			return;
		}
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null) == null)
		{
			RpcUnlockClient(xPos, yPos);
			return;
		}
		ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null);
		TileObject tileObjectForServerDrop = WorldManager.Instance.getTileObjectForServerDrop(WorldManager.Instance.onTileMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		if ((bool)WorldManager.Instance.allObjects[itemOnTopInPosition.getTileObjectId()].showObjectOnStatusChange && (bool)WorldManager.Instance.allObjects[itemOnTopInPosition.getTileObjectId()].showObjectOnStatusChange.isBoomBox && itemOnTopInPosition.getStatus() > 0)
		{
			NetworkMapSharer.Instance.spawnAServerDrop(itemOnTopInPosition.getStatus(), 1, tileObjectForServerDrop.placedPositions[posId].position);
		}
		if (!WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].pickUpRequiresEmptyPocket)
		{
			if (WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsStatusNumberOnDeath)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(itemOnTopInPosition.getStatus(), 1, tileObjectForServerDrop.placedPositions[posId].position);
			}
			else
			{
				NetworkMapSharer.Instance.spawnAServerDrop(WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsItemOnDeath.getItemId(), 1, tileObjectForServerDrop.placedPositions[posId].position);
			}
		}
		else
		{
			TargetGiveInvItemOnPickUpRequiresEmptyPockets(base.connectionToClient, itemOnTopInPosition.getStatus(), 1);
		}
		WorldManager.Instance.returnTileObject(tileObjectForServerDrop);
		RpcRemoveItemOnTop(posId, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPickUpObjectOnTopOf(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObjectOnTopOf called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpObjectOnTopOf(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpObjectOnTopOfInside(int posId, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, InsideHouseDetails) == null)
		{
			return;
		}
		ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, InsideHouseDetails);
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(InsideHouseDetails.xPos, InsideHouseDetails.yPos);
		TileObject tileObjectForHouse = WorldManager.Instance.getTileObjectForHouse(InsideHouseDetails.houseMapOnTile[xPos, yPos], displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(xPos * 2, 0f, yPos * 2), xPos, yPos, InsideHouseDetails);
		if ((bool)WorldManager.Instance.allObjects[itemOnTopInPosition.getTileObjectId()].showObjectOnStatusChange && (bool)WorldManager.Instance.allObjects[itemOnTopInPosition.getTileObjectId()].showObjectOnStatusChange.isBoomBox && itemOnTopInPosition.getStatus() > 0)
		{
			NetworkMapSharer.Instance.spawnAServerDrop(itemOnTopInPosition.getStatus(), 1, tileObjectForHouse.placedPositions[posId].position, InsideHouseDetails);
		}
		if (!WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].pickUpRequiresEmptyPocket)
		{
			if (WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsStatusNumberOnDeath)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(itemOnTopInPosition.getStatus(), 1, tileObjectForHouse.placedPositions[posId].position, InsideHouseDetails);
			}
			else
			{
				NetworkMapSharer.Instance.spawnAServerDrop(WorldManager.Instance.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsItemOnDeath.getItemId(), 1, tileObjectForHouse.placedPositions[posId].position, InsideHouseDetails);
			}
		}
		else
		{
			TargetGiveInvItemOnPickUpRequiresEmptyPockets(base.connectionToClient, itemOnTopInPosition.getStatus(), 1);
		}
		RpcRemoveItemOnTopInside(posId, xPos, yPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
		WorldManager.Instance.returnTileObject(tileObjectForHouse);
	}

	protected static void InvokeUserCode_CmdPickUpObjectOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObjectOnTopOfInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpObjectOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceItemOnTopOf(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null) != null)
		{
			RpcUnlockClient(xPos, yPos);
		}
		else
		{
			RpcPlaceItemOnTop(objectId, posId, status, rotation, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdPlaceItemOnTopOf(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceItemOnTopOf called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceItemOnTopOf(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, InsideHouseDetails) == null)
		{
			RpcPlaceItemOnTopOfInside(objectId, posId, status, rotation, xPos, yPos, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
		}
	}

	protected static void InvokeUserCode_CmdPlaceItemOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceItemOnTopOfInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceItemOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos, int houseX, int houseY)
	{
		ItemOnTopManager.manage.placeItemOnTop(objectId, posId, status, rotation, xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles && (bool)displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos])
		{
			displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].checkOnTopInside(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
			if (displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].placedPositions.Length != 0)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].placedPositions[posId].transform.position, 3);
				SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].placedPositions[posId].transform.position);
			}
		}
	}

	protected static void InvokeUserCode_RpcPlaceItemOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceItemOnTopOfInside called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcPlaceItemOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUnlockClient(int xPos, int yPos)
	{
		WorldManager.Instance.unlockClientTile(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcUnlockClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUnlockClient called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcUnlockClient(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceItemOnTop(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		ItemOnTopManager.manage.placeItemOnTop(objectId, posId, status, rotation, xPos, yPos, null);
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject && tileObject.placedPositions.Length != 0)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], tileObject.placedPositions[posId].transform.position, 3);
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.placeItem, tileObject.placedPositions[posId].transform.position);
		}
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		WorldManager.Instance.refreshAllChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcPlaceItemOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceItemOnTop called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcPlaceItemOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRemoveItemOnTop(int posId, int xPos, int yPos)
	{
		TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject && tileObject.placedPositions.Length != 0)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], tileObject.placedPositions[posId].transform.position, 3);
		}
		ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null));
		WorldManager.Instance.unlockClientTile(xPos, yPos);
		WorldManager.Instance.refreshAllChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcRemoveItemOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveItemOnTop called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcRemoveItemOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRemoveItemOnTopInside(int posId, int xPos, int yPos, int houseX, int houseY)
	{
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles && (bool)displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos])
		{
			displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].checkOnTopInside(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
			if (displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].placedPositions.Length != 0)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].placedPositions[posId].transform.position, 3);
			}
		}
		ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY)));
		if ((bool)displayPlayerHouseTiles && (bool)displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos])
		{
			displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].checkOnTopInside(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
		}
		WorldManager.Instance.unlockClientTileHouse(xPos, yPos, houseX, houseY);
	}

	protected static void InvokeUserCode_RpcRemoveItemOnTopInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveItemOnTopInside called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcRemoveItemOnTopInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdisInsidePlayerHouse(int houseX, int houseY)
	{
		IsInsidePlayerHouse = true;
		InsideHouseDetails = HouseManager.manage.getHouseInfo(houseX, houseY);
		NetworkMapSharer.Instance.TargetRequestHouse(base.connectionToClient, houseX, houseY, WorldManager.Instance.getHouseDetailsArray(InsideHouseDetails.houseMapOnTile), WorldManager.Instance.getHouseDetailsArray(InsideHouseDetails.houseMapOnTileStatus), WorldManager.Instance.getHouseDetailsArray(InsideHouseDetails.houseMapRotation), InsideHouseDetails.wall, InsideHouseDetails.floor, ItemOnTopManager.manage.getAllItemsOnTopInHouse(InsideHouseDetails));
		if (SignManager.manage.areThereSignsInThisHouse(houseX, houseY))
		{
			NetworkMapSharer.Instance.TargetGiveSignDetailsForHouse(base.connectionToClient, SignManager.manage.collectSignsInHouse(houseX, houseY), houseX, houseY);
		}
		if (InsideHouseDetails != null)
		{
			insideHouseDisplay = HouseManager.manage.findHousesOnDisplay(InsideHouseDetails.xPos, InsideHouseDetails.yPos);
			playerHouseTransform = insideHouseDisplay.getStartingPosTransform();
		}
	}

	protected static void InvokeUserCode_CmdisInsidePlayerHouse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdisInsidePlayerHouse called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdisInsidePlayerHouse(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGoOutside()
	{
		IsInsidePlayerHouse = false;
		InsideHouseDetails = null;
		insideHouseDisplay = null;
		playerHouseTransform = null;
	}

	protected static void InvokeUserCode_CmdGoOutside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoOutside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdGoOutside();
		}
	}

	protected void UserCode_CmdPlaceItemInToTileObject(int newStatus, int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcPlaceItemOnToTileObject(newStatus, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlaceItemInToTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceItemInToTileObject called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceItemInToTileObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSpawnPlaceable(Vector3 spawnPos, int id)
	{
		GameObject gameObject = Object.Instantiate(Inventory.Instance.allItems[id].spawnPlaceable, spawnPos, Quaternion.identity);
		if ((bool)gameObject.GetComponent<NetworkTransform>() && gameObject.GetComponent<NetworkTransform>().clientAuthority)
		{
			NetworkServer.Spawn(gameObject, base.connectionToClient);
		}
		else
		{
			NetworkServer.Spawn(gameObject);
		}
	}

	protected static void InvokeUserCode_CmdSpawnPlaceable(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnPlaceable called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdSpawnPlaceable(reader.ReadVector3(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSpawnVehicle(int id, int rot, int variation)
	{
		Vector3 position = base.transform.position + base.transform.forward * 2f;
		int num = (int)(Mathf.Round(position.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(position.z + 0.5f) / 2f);
		if (WorldManager.Instance.isPositionOnMap(num, num2))
		{
			position = ((WorldManager.Instance.heightMap[num, num2] <= 0) ? new Vector3(num * 2, 1f, num2 * 2) : new Vector3(num * 2, WorldManager.Instance.heightMap[num, num2], num2 * 2));
		}
		Quaternion rotation = rot switch
		{
			1 => Quaternion.Euler(0f, 180f, 0f), 
			2 => Quaternion.Euler(0f, 270f, 0f), 
			3 => Quaternion.Euler(0f, 0f, 0f), 
			4 => Quaternion.Euler(0f, 90f, 0f), 
			_ => Quaternion.Euler(0f, 180f, 0f), 
		};
		GameObject obj = Object.Instantiate(NetworkMapSharer.Instance.vehicleBox, position, rotation);
		obj.GetComponent<SpawnVehicleOnOpen>().fillDetails(Inventory.Instance.allItems[id].spawnPlaceable, variation, base.connectionToClient);
		NetworkServer.Spawn(obj);
	}

	protected static void InvokeUserCode_CmdSpawnVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnVehicle called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdSpawnVehicle(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdFixTeleport(string dir)
	{
		switch (dir)
		{
		case "north":
			WorldManager.Instance.onTileChunkHasChanged(TownManager.manage.northTowerPos[0], TownManager.manage.northTowerPos[1]);
			NetworkMapSharer.Instance.RpcUpdateOnTileObject(292, TownManager.manage.northTowerPos[0], TownManager.manage.northTowerPos[1]);
			NetworkMapSharer.Instance.NetworknorthOn = true;
			break;
		case "east":
			WorldManager.Instance.onTileChunkHasChanged(TownManager.manage.eastTowerPos[0], TownManager.manage.eastTowerPos[1]);
			NetworkMapSharer.Instance.RpcUpdateOnTileObject(292, TownManager.manage.eastTowerPos[0], TownManager.manage.eastTowerPos[1]);
			NetworkMapSharer.Instance.NetworkeastOn = true;
			break;
		case "south":
			WorldManager.Instance.onTileChunkHasChanged(TownManager.manage.southTowerPos[0], TownManager.manage.southTowerPos[1]);
			NetworkMapSharer.Instance.RpcUpdateOnTileObject(292, TownManager.manage.southTowerPos[0], TownManager.manage.southTowerPos[1]);
			NetworkMapSharer.Instance.NetworksouthOn = true;
			break;
		case "west":
			WorldManager.Instance.onTileChunkHasChanged(TownManager.manage.westTowerPos[0], TownManager.manage.westTowerPos[1]);
			NetworkMapSharer.Instance.RpcUpdateOnTileObject(292, TownManager.manage.westTowerPos[0], TownManager.manage.westTowerPos[1]);
			NetworkMapSharer.Instance.NetworkwestOn = true;
			break;
		}
	}

	protected static void InvokeUserCode_CmdFixTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFixTeleport called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdFixTeleport(reader.ReadString());
		}
	}

	protected void UserCode_CmdChangeOnTile(int newTileType, int xPos, int yPos)
	{
		if (CheckIfCanDamage(new Vector2(xPos, yPos)) && myEquip.myPermissions.CheckIfCanDamgeTiles())
		{
			if (myEquip.CurrentlyHoldingDeed())
			{
				DeedManager.manage.placeDeed(myEquip.itemCurrentlyHolding);
			}
			WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
			if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].IsMultiTileObject())
			{
				NetworkMapSharer.Instance.RpcRemoveMultiTiledObject(WorldManager.Instance.onTileMap[xPos, yPos], xPos, yPos, WorldManager.Instance.rotationMap[xPos, yPos]);
			}
			else if (newTileType == -1 && BuriedManager.manage.checkIfBuriedItem(xPos, yPos) != null)
			{
				NetworkMapSharer.Instance.RpcUpdateOnTileObject(30, xPos, yPos);
			}
			else if (newTileType == -1 && BuriedManager.manage.checkIfShouldTurnIntoBuriedItem(xPos, yPos))
			{
				NetworkMapSharer.Instance.RpcUpdateOnTileObject(30, xPos, yPos);
			}
			else
			{
				NetworkMapSharer.Instance.RpcUpdateOnTileObject(newTileType, xPos, yPos);
			}
			CheckIfIsFarmAnimalHouse(xPos, yPos, newTileType, placingObjectRotation);
		}
	}

	protected static void InvokeUserCode_CmdChangeOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOnTile called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeOnTile(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeOnTileInside(int newTileType, int xPos, int yPos, int rotation)
	{
		NetworkMapSharer.Instance.RpcChangeHouseOnTile(newTileType, xPos, yPos, rotation, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
	}

	protected static void InvokeUserCode_CmdChangeOnTileInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOnTileInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeOnTileInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpOnTile(int xPos, int yPos)
	{
		if (!myEquip.myPermissions.CheckIfCanDamgeTiles())
		{
			return;
		}
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		if (WorldManager.Instance.onTileMap[xPos, yPos] < 0)
		{
			return;
		}
		if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest && !WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectChest.checkIfEmpty(xPos, yPos, InsideHouseDetails))
		{
			TargetSendMustBeEmptyPrompt(base.connectionToClient);
			return;
		}
		if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].showObjectOnStatusChange && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].showObjectOnStatusChange.isBoomBox && WorldManager.Instance.onTileStatusMap[xPos, yPos] > 0)
		{
			NetworkMapSharer.Instance.spawnAServerDrop(WorldManager.Instance.onTileStatusMap[xPos, yPos], 1, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		}
		if (WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].pickUpRequiresEmptyPocket && WorldManager.Instance.onTileStatusMap[xPos, yPos] >= 0 && WorldManager.Instance.onTileStatusMap[xPos, yPos] < Inventory.Instance.allItems.Length)
		{
			TargetGiveInvItemOnPickUpRequiresEmptyPockets(base.connectionToClient, WorldManager.Instance.onTileStatusMap[xPos, yPos], 1);
		}
		if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectItemChanger && WorldManager.Instance.onTileStatusMap[xPos, yPos] >= 0)
		{
			TargetSendMustBeEmptyPrompt(base.connectionToClient);
			return;
		}
		TileObject tileObjectForServerDrop = WorldManager.Instance.getTileObjectForServerDrop(WorldManager.Instance.onTileMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
		if (tileObjectForServerDrop.canBePickedUp() || ((bool)tileObjectForServerDrop.tileObjectBridge && WorldManager.Instance.waterMap[xPos, yPos]))
		{
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] > 0 && WorldManager.Instance.allObjectSettings[tileObjectForServerDrop.tileObjectId].statusObjectsPickUpFirst.Length != 0)
			{
				int num = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos], 0, WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst.Length - 1);
				if ((bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[num].placeable)
				{
					WorldManager.Instance.allObjectSettings[WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[num].placeable.tileObjectId].removeBeauty();
				}
				int invItemId = Inventory.Instance.getInvItemId(WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[num]);
				NetworkMapSharer.Instance.spawnAServerDrop(invItemId, 1, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
				NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, xPos, yPos);
			}
			else if (tileObjectForServerDrop.IsMultiTileObject())
			{
				NetworkMapSharer.Instance.RpcRemoveMultiTiledObject(WorldManager.Instance.onTileMap[xPos, yPos], xPos, yPos, WorldManager.Instance.rotationMap[xPos, yPos]);
			}
			else if (BuriedManager.manage.checkIfBuriedItem(xPos, yPos) != null)
			{
				NetworkMapSharer.Instance.RpcUpdateOnTileObject(30, xPos, yPos);
			}
			else
			{
				NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, xPos, yPos);
			}
		}
		WorldManager.Instance.returnTileObject(tileObjectForServerDrop);
		CheckIfIsFarmAnimalHouse(xPos, yPos, -1);
	}

	protected static void InvokeUserCode_CmdPickUpOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpOnTile called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpOnTile(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpOnTileInside(int xPos, int yPos, float playerHouseTransformY)
	{
		if (InsideHouseDetails.houseMapOnTile[xPos, yPos] < 0 || ((bool)WorldManager.Instance.allObjects[InsideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest && !WorldManager.Instance.allObjects[InsideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest.checkIfEmpty(xPos, yPos, InsideHouseDetails)))
		{
			return;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(InsideHouseDetails.xPos, InsideHouseDetails.yPos);
		TileObject tileObjectForHouse = WorldManager.Instance.getTileObjectForHouse(InsideHouseDetails.houseMapOnTile[xPos, yPos], displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(xPos * 2, 0f, yPos * 2), xPos, yPos, InsideHouseDetails);
		if (tileObjectForHouse.canBePickedUp())
		{
			if ((bool)WorldManager.Instance.allObjects[InsideHouseDetails.houseMapOnTile[xPos, yPos]].showObjectOnStatusChange && (bool)WorldManager.Instance.allObjects[InsideHouseDetails.houseMapOnTile[xPos, yPos]].showObjectOnStatusChange.isBoomBox && InsideHouseDetails.houseMapOnTileStatus[xPos, yPos] > 0)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(InsideHouseDetails.houseMapOnTileStatus[xPos, yPos], 1, new Vector3(tileObjectForHouse.transform.position.x, playerHouseTransformY, tileObjectForHouse.transform.position.z), InsideHouseDetails);
			}
			if ((bool)tileObjectForHouse && WorldManager.Instance.allObjectSettings[tileObjectForHouse.tileObjectId].pickUpRequiresEmptyPocket)
			{
				if (WorldManager.Instance.allObjectSettings[tileObjectForHouse.tileObjectId].dropsStatusNumberOnDeath && InsideHouseDetails.houseMapOnTileStatus[xPos, yPos] != -1)
				{
					TargetGiveInvItemOnPickUpRequiresEmptyPockets(base.connectionToClient, InsideHouseDetails.houseMapOnTileStatus[xPos, yPos], 1);
					NetworkMapSharer.Instance.RpcChangeHouseOnTile(-1, xPos, yPos, 0, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
				}
			}
			else
			{
				NetworkMapSharer.Instance.RpcChangeHouseOnTile(-1, xPos, yPos, 0, InsideHouseDetails.xPos, InsideHouseDetails.yPos);
			}
		}
		WorldManager.Instance.returnTileObject(tileObjectForHouse);
	}

	protected static void InvokeUserCode_CmdPickUpOnTileInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpOnTileInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpOnTileInside(reader.ReadInt(), reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdHarvestCrabPot(int xPos, int yPos, int drop)
	{
		NetworkMapSharer.Instance.spawnAServerDrop(drop, 1, new Vector3((float)xPos * 2f, 0.6f, yPos * 2), null, tryNotToStack: false, 3);
		NetworkMapSharer.Instance.RpcHarvestObject(0, xPos, yPos, spawnDrop: false);
	}

	protected static void InvokeUserCode_CmdHarvestCrabPot(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestCrabPot called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdHarvestCrabPot(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdHarvestOnTile(int xPos, int yPos, bool pickedUpAuto)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] <= -1)
		{
			NetworkMapSharer.Instance.RpcUpdateOnTileObjectForDesync(WorldManager.Instance.onTileMap[xPos, yPos], WorldManager.Instance.onTileStatusMap[xPos, yPos], xPos, yPos);
		}
		else if (!WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages)
		{
			NetworkMapSharer.Instance.RpcUpdateOnTileObjectForDesync(WorldManager.Instance.onTileMap[xPos, yPos], WorldManager.Instance.onTileStatusMap[xPos, yPos], xPos, yPos);
		}
		else if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.canBeHarvested(WorldManager.Instance.onTileStatusMap[xPos, yPos]))
		{
			int num = WorldManager.Instance.onTileStatusMap[xPos, yPos] + WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.takeOrAddFromStateOnHarvest;
			if (num < 0)
			{
				num = 0;
			}
			if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.diesOnHarvest)
			{
				if (pickedUpAuto)
				{
					if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
					{
						NetworkMapSharer.Instance.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.Instance.onTileMap[xPos, yPos]);
					}
					NetworkMapSharer.Instance.RpcHarvestObject(-1, xPos, yPos, spawnDrop: false);
					int num2 = -1;
					if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.isCrabPot)
					{
						num2 = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.getCrabTrapDrop(xPos, yPos);
					}
					else if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.dropsFromLootTable)
					{
						num2 = Inventory.Instance.getInvItemId(WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.dropsFromLootTable.getRandomDropFromTable());
					}
					else if ((bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestDrop)
					{
						num2 = Inventory.Instance.getInvItemId(WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestDrop);
					}
					if (num2 != -1)
					{
						DroppedItem component = Object.Instantiate(WorldManager.Instance.droppedItemPrefab, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<DroppedItem>();
						component.GetComponent<DroppedItem>().setDesiredPos(component.transform.position.y, component.transform.position.x, component.transform.position.z);
						component.NetworkstackAmount = 1;
						component.NetworkmyItemId = num2;
						NetworkServer.Spawn(component.gameObject);
						component.pickUp();
						component.RpcMoveTowardsPickedUpBy(base.netId);
					}
				}
				else
				{
					if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
					{
						NetworkMapSharer.Instance.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.Instance.onTileMap[xPos, yPos]);
					}
					NetworkMapSharer.Instance.RpcHarvestObject(-1, xPos, yPos, spawnDrop: true);
				}
			}
			else
			{
				if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
				{
					NetworkMapSharer.Instance.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.Instance.onTileMap[xPos, yPos]);
				}
				NetworkMapSharer.Instance.RpcHarvestObject(num, xPos, yPos, !pickedUpAuto);
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = num;
			}
			WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		}
		else
		{
			NetworkMapSharer.Instance.RpcUpdateOnTileObjectForDesync(WorldManager.Instance.onTileMap[xPos, yPos], WorldManager.Instance.onTileStatusMap[xPos, yPos], xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdHarvestOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestOnTile called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdHarvestOnTile(reader.ReadInt(), reader.ReadInt(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdHarvestOnTileDeath(int xPos, int yPos)
	{
		if (!WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.canBeHarvested(WorldManager.Instance.onTileStatusMap[xPos, yPos], deathCheck: true))
		{
			return;
		}
		int num = WorldManager.Instance.onTileStatusMap[xPos, yPos] + WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.takeOrAddFromStateOnHarvest;
		if (num < 0)
		{
			num = 0;
		}
		if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.diesOnHarvest)
		{
			if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
			{
				NetworkMapSharer.Instance.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.Instance.onTileMap[xPos, yPos]);
			}
			NetworkMapSharer.Instance.RpcHarvestObject(-1, xPos, yPos, spawnDrop: true);
			WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		}
		else
		{
			if (WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
			{
				NetworkMapSharer.Instance.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.Instance.onTileMap[xPos, yPos]);
			}
			NetworkMapSharer.Instance.RpcHarvestObject(num, xPos, yPos, spawnDrop: true);
			WorldManager.Instance.onTileStatusMap[xPos, yPos] = num;
		}
	}

	protected static void InvokeUserCode_CmdHarvestOnTileDeath(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestOnTileDeath called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdHarvestOnTileDeath(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdFillFood(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcOpenCloseTile(xPos, yPos, 1);
	}

	protected static void InvokeUserCode_CmdFillFood(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFillFood called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdFillFood(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdOpenClose(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
		{
			NetworkMapSharer.Instance.RpcOpenCloseTile(xPos, yPos, 1);
		}
		else
		{
			NetworkMapSharer.Instance.RpcOpenCloseTile(xPos, yPos, 0);
		}
	}

	protected static void InvokeUserCode_CmdOpenClose(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenClose called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdOpenClose(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileHeight(int newTileHeight, int newTileType, int xPos, int yPos)
	{
		if (myEquip.myPermissions.CheckIfCanTerraform() && CheckIfCanDamage(new Vector2(xPos, yPos)))
		{
			if (newTileHeight > 0)
			{
				NetworkMapSharer.Instance.RpcUpdateTileType(newTileType, xPos, yPos);
			}
			NetworkMapSharer.Instance.changeTileHeight(newTileHeight, xPos, yPos, base.connectionToClient);
		}
	}

	protected static void InvokeUserCode_CmdChangeTileHeight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileHeight called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeTileHeight(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileType(int newTileType, int xPos, int yPos)
	{
		if (!CheckIfCanDamage(new Vector2(xPos, yPos)) || !myEquip.myPermissions.CheckIfCanTerraform())
		{
			return;
		}
		WorldManager.Instance.tileTypeChunkHasChanged(xPos, yPos);
		if (((bool)WorldManager.Instance.tileTypes[newTileType].dropOnChange || WorldManager.Instance.tileTypes[newTileType].saveUnderTile || WorldManager.Instance.tileTypes[newTileType].changeToUnderTileAndChangeHeight) && !WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].dropOnChange && !WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].changeToUnderTileAndChangeHeight && (WorldManager.Instance.tileTypeMap[xPos, yPos] < 0 || WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].canBeSavedUnder))
		{
			WorldManager.Instance.tileTypeStatusMap[xPos, yPos] = WorldManager.Instance.tileTypeMap[xPos, yPos];
		}
		if (WeatherManager.Instance.rainMgr.IsActive)
		{
			switch (newTileType)
			{
			case 7:
				newTileType = 8;
				break;
			case 12:
				newTileType = 13;
				break;
			}
		}
		if ((bool)WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].dropOnChange)
		{
			NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].dropOnChange), 1, new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2));
			NetworkMapSharer.Instance.RpcUpdateTileType(WorldManager.Instance.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		else if (!WorldManager.Instance.tileTypes[newTileType].changeTileKeepUnderTile && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].changeTileKeepUnderTile)
		{
			NetworkMapSharer.Instance.RpcUpdateTileType(WorldManager.Instance.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		else if (WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[xPos, yPos]].saveUnderTile && !WorldManager.Instance.tileTypes[newTileType].changeTileKeepUnderTile)
		{
			NetworkMapSharer.Instance.RpcUpdateTileType(WorldManager.Instance.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		else
		{
			NetworkMapSharer.Instance.RpcUpdateTileType(newTileType, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdChangeTileType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileType called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeTileType(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		if (myEquip.myPermissions.CheckIfCanDamgeTiles())
		{
			if (myEquip.CurrentlyHoldingDeed())
			{
				DeedManager.manage.placeDeed(myEquip.itemCurrentlyHolding);
			}
			WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
			if (WorldManager.Instance.allObjectSettings[multiTiledObjectId].canPlaceItemsUnderIt)
			{
				NetworkMapSharer.Instance.RpcPlaceMultiTiledObjectPlaceUnder(multiTiledObjectId, xPos, yPos, rotation);
			}
			else
			{
				NetworkMapSharer.Instance.RpcPlaceMultiTiledObject(multiTiledObjectId, xPos, yPos, rotation);
			}
			if ((bool)WorldManager.Instance.allObjects[multiTiledObjectId].tileObjectFurniture || (bool)WorldManager.Instance.allObjects[multiTiledObjectId].showObjectOnStatusChange)
			{
				WorldManager.Instance.onTileStatusMap[xPos, yPos] = 0;
			}
			CheckIfIsFarmAnimalHouse(xPos, yPos, multiTiledObjectId, rotation);
		}
	}

	protected static void InvokeUserCode_CmdPlaceMultiTiledObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceMultiTiledObject called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceMultiTiledObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceBridgeTileObject(int multiTiledObjectId, int xPos, int yPos, int rotation, int length)
	{
		WorldManager.Instance.onTileChunkHasChanged(xPos, yPos);
		NetworkMapSharer.Instance.RpcPlaceBridgeTiledObject(multiTiledObjectId, xPos, yPos, rotation, length);
		int[] array = WorldManager.Instance.allObjects[multiTiledObjectId].placeBridgeTiledObject(xPos, yPos, rotation, length);
		CheckIfIsFarmAnimalHouse(array[0], array[1], multiTiledObjectId);
	}

	protected static void InvokeUserCode_CmdPlaceBridgeTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceBridgeTileObject called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceBridgeTileObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdUpdateHouseWall(int itemId, int houseX, int houseY)
	{
		Inventory.Instance.wallSlot.itemNo = itemId;
		Inventory.Instance.wallSlot.stack = 1;
		NetworkMapSharer.Instance.RpcUpdateHouseWall(itemId, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdUpdateHouseWall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateHouseWall called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdUpdateHouseWall(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdUpdateHouseFloor(int itemId, int houseX, int houseY)
	{
		Inventory.Instance.floorSlot.itemNo = itemId;
		Inventory.Instance.floorSlot.stack = 1;
		NetworkMapSharer.Instance.RpcUpdateHouseFloor(itemId, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdUpdateHouseFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateHouseFloor called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdUpdateHouseFloor(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetRotation(int xPos, int yPos, int rotation)
	{
		NetworkMapSharer.Instance.RpcSetRotation(xPos, yPos, rotation);
	}

	protected static void InvokeUserCode_CmdSetRotation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetRotation called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdSetRotation(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDepositItem(int depositItemId, int xPos, int yPos)
	{
		if (depositItemId >= 0 && (bool)Inventory.Instance.allItems[depositItemId].itemChange && Inventory.Instance.allItems[depositItemId].itemChange.checkIfCanBeDepositedServer(WorldManager.Instance.onTileMap[xPos, yPos]))
		{
			NetworkMapSharer.Instance.RpcDepositItemIntoChanger(depositItemId, xPos, yPos);
			NetworkMapSharer.Instance.startTileTimerOnServer(depositItemId, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdDepositItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDepositItem called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdDepositItem(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDepositItemInside(int depositItemId, int xPos, int yPos, int houseX, int houseY)
	{
		if (depositItemId >= 0 && (bool)Inventory.Instance.allItems[depositItemId].itemChange && Inventory.Instance.allItems[depositItemId].itemChange.checkIfCanBeDepositedServer(InsideHouseDetails.houseMapOnTile[xPos, yPos]))
		{
			NetworkMapSharer.Instance.RpcDepositItemIntoChangerInside(depositItemId, xPos, yPos, houseX, houseY);
			NetworkMapSharer.Instance.startTileTimerOnServer(depositItemId, xPos, yPos, InsideHouseDetails);
		}
	}

	protected static void InvokeUserCode_CmdDepositItemInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDepositItemInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdDepositItemInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdCurrentlyAttackingPos(int xPos, int yPos)
	{
		NetworkcurrentlyAttackingPos = new Vector2(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdCurrentlyAttackingPos(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCurrentlyAttackingPos called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdCurrentlyAttackingPos(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlayPlaceableAnimation()
	{
		RpcPlayPlaceableAnimation();
	}

	protected static void InvokeUserCode_CmdPlayPlaceableAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayPlaceableAnimation called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlayPlaceableAnimation();
		}
	}

	protected void UserCode_RpcPlayPlaceableAnimation()
	{
		myEquip.playPlaceableAnimation();
	}

	protected static void InvokeUserCode_RpcPlayPlaceableAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayPlaceableAnimation called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcPlayPlaceableAnimation();
		}
	}

	protected void UserCode_CmdGiveStatus(int newStatus, int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(newStatus, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdGiveStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveStatus called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdGiveStatus(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGiveStatusInside(int newStatus, int xPos, int yPos, int houseX, int houseY)
	{
		NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(newStatus, xPos, yPos, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdGiveStatusInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveStatusInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdGiveStatusInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetSendMustBeEmptyPrompt(NetworkConnection con)
	{
		NotificationManager.manage.pocketsFull.showMustBeEmpty();
	}

	protected static void InvokeUserCode_TargetSendMustBeEmptyPrompt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSendMustBeEmptyPrompt called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_TargetSendMustBeEmptyPrompt(NetworkClient.readyConnection);
		}
	}

	protected void UserCode_TargetGiveInvItemOnPickUpRequiresEmptyPockets(NetworkConnection con, int itemId, int stack)
	{
		Inventory.Instance.addItemToInventory(itemId, stack);
	}

	protected static void InvokeUserCode_TargetGiveInvItemOnPickUpRequiresEmptyPockets(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveInvItemOnPickUpRequiresEmptyPockets called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_TargetGiveInvItemOnPickUpRequiresEmptyPockets(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceSingleChestOutside(int newTileType, int xPos, int yPos, int rotation)
	{
		if (rotation != -1)
		{
			NetworkMapSharer.Instance.RpcSetRotation(xPos, yPos, rotation);
		}
		NetworkMapSharer.Instance.RpcUpdateOnTileObject(newTileType, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlaceSingleChestOutside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceSingleChestOutside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceSingleChestOutside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	static CharInteract()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpObjectOnTop", InvokeUserCode_CmdPickUpObjectOnTop, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpObjectOnTopOf", InvokeUserCode_CmdPickUpObjectOnTopOf, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpObjectOnTopOfInside", InvokeUserCode_CmdPickUpObjectOnTopOfInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceItemOnTopOf", InvokeUserCode_CmdPlaceItemOnTopOf, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceItemOnTopOfInside", InvokeUserCode_CmdPlaceItemOnTopOfInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdisInsidePlayerHouse", InvokeUserCode_CmdisInsidePlayerHouse, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdGoOutside", InvokeUserCode_CmdGoOutside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceItemInToTileObject", InvokeUserCode_CmdPlaceItemInToTileObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdSpawnPlaceable", InvokeUserCode_CmdSpawnPlaceable, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdSpawnVehicle", InvokeUserCode_CmdSpawnVehicle, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdFixTeleport", InvokeUserCode_CmdFixTeleport, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeOnTile", InvokeUserCode_CmdChangeOnTile, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeOnTileInside", InvokeUserCode_CmdChangeOnTileInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpOnTile", InvokeUserCode_CmdPickUpOnTile, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpOnTileInside", InvokeUserCode_CmdPickUpOnTileInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdHarvestCrabPot", InvokeUserCode_CmdHarvestCrabPot, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdHarvestOnTile", InvokeUserCode_CmdHarvestOnTile, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdHarvestOnTileDeath", InvokeUserCode_CmdHarvestOnTileDeath, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdFillFood", InvokeUserCode_CmdFillFood, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdOpenClose", InvokeUserCode_CmdOpenClose, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeTileHeight", InvokeUserCode_CmdChangeTileHeight, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeTileType", InvokeUserCode_CmdChangeTileType, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceMultiTiledObject", InvokeUserCode_CmdPlaceMultiTiledObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceBridgeTileObject", InvokeUserCode_CmdPlaceBridgeTileObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdUpdateHouseWall", InvokeUserCode_CmdUpdateHouseWall, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdUpdateHouseFloor", InvokeUserCode_CmdUpdateHouseFloor, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdSetRotation", InvokeUserCode_CmdSetRotation, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdDepositItem", InvokeUserCode_CmdDepositItem, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdDepositItemInside", InvokeUserCode_CmdDepositItemInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdCurrentlyAttackingPos", InvokeUserCode_CmdCurrentlyAttackingPos, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlayPlaceableAnimation", InvokeUserCode_CmdPlayPlaceableAnimation, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdGiveStatus", InvokeUserCode_CmdGiveStatus, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdGiveStatusInside", InvokeUserCode_CmdGiveStatusInside, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceSingleChestOutside", InvokeUserCode_CmdPlaceSingleChestOutside, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcPlaceItemOnTopOfInside", InvokeUserCode_RpcPlaceItemOnTopOfInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcUnlockClient", InvokeUserCode_RpcUnlockClient);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcPlaceItemOnTop", InvokeUserCode_RpcPlaceItemOnTop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcRemoveItemOnTop", InvokeUserCode_RpcRemoveItemOnTop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcRemoveItemOnTopInside", InvokeUserCode_RpcRemoveItemOnTopInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcPlayPlaceableAnimation", InvokeUserCode_RpcPlayPlaceableAnimation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "TargetSendMustBeEmptyPrompt", InvokeUserCode_TargetSendMustBeEmptyPrompt);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "TargetGiveInvItemOnPickUpRequiresEmptyPockets", InvokeUserCode_TargetGiveInvItemOnPickUpRequiresEmptyPockets);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVector2(currentlyAttackingPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteVector2(currentlyAttackingPos);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			Vector2 vector = currentlyAttackingPos;
			NetworkcurrentlyAttackingPos = reader.ReadVector2();
			if (!SyncVarEqual(vector, ref currentlyAttackingPos))
			{
				OnChangeAttackingPos(vector, currentlyAttackingPos);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			Vector2 vector2 = currentlyAttackingPos;
			NetworkcurrentlyAttackingPos = reader.ReadVector2();
			if (!SyncVarEqual(vector2, ref currentlyAttackingPos))
			{
				OnChangeAttackingPos(vector2, currentlyAttackingPos);
			}
		}
	}
}
