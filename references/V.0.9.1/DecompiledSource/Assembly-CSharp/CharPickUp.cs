using System.Collections;
using System.Runtime.InteropServices;
using I2.Loc;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharPickUp : NetworkBehaviour
{
	public LayerMask pickUpLayerMask;

	public LayerMask vehicleMask;

	public CharInteract myInteract;

	public GameObject[] hideStuff;

	private CharTalkUse myTalkTo;

	private CharMovement myChar;

	public bool sitting;

	private int sittingInSeat;

	public int sittingXpos;

	public int sittingYPos;

	public Transform sittingPosition;

	public bool drivingVehicle;

	private uint drivingVehicleId;

	[SyncVar(hook = "onCarryChanged")]
	private uint carryingObjectNetId;

	public EquipItemToChar myEquip;

	[SyncVar(hook = "onSittingChanged")]
	public Vector3 sittingPos;

	public bool holdingPickUp;

	public NetworkFishingRod netRod;

	public Vehicle currentlyDriving;

	public Vector3 positionBeforeSitting;

	public VehicleSecondSeat currentPassengerPos;

	public uint vehiclePassengerId;

	private bool sittingOnFloor;

	private bool justSat;

	private AnimalCarryBox carriedAnimal;

	private TrappedAnimal trappedAnimal;

	private bool sittingInHairDresserSeat;

	private int sittingInTuckshopChair = -1;

	private Vector3 lastLayedDownPos;

	private HouseDetails sleepingInside;

	private int sittingLayingOrStanding;

	public uint NetworkcarryingObjectNetId
	{
		get
		{
			return carryingObjectNetId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref carryingObjectNetId))
			{
				uint oldCarry = carryingObjectNetId;
				SetSyncVar(value, ref carryingObjectNetId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					onCarryChanged(oldCarry, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public Vector3 NetworksittingPos
	{
		get
		{
			return sittingPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref sittingPos))
			{
				Vector3 old = sittingPos;
				SetSyncVar(value, ref sittingPos, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onSittingChanged(old, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		myChar = GetComponent<CharMovement>();
		myTalkTo = GetComponent<CharTalkUse>();
		myInteract = GetComponent<CharInteract>();
		myEquip = GetComponent<EquipItemToChar>();
		netRod = GetComponent<NetworkFishingRod>();
		hideStuff[3] = StatusManager.manage.statusWindow.gameObject;
		hideStuff[2] = Inventory.Instance.quickSlotBar.gameObject;
	}

	public override void OnStopClient()
	{
		if (!base.isServer)
		{
			return;
		}
		if (carryingObjectNetId != 0)
		{
			NetworkIdentity.spawned[carryingObjectNetId].GetComponent<PickUpAndCarry>().NetworkbeingCarriedBy = 0u;
		}
		if (sittingInTuckshopChair != -1)
		{
			CmdGetUpFromTuckshopSeat(sittingInTuckshopChair);
		}
		if (sittingInHairDresserSeat)
		{
			CmdGetUpFromHairDresserSeat();
		}
		else if (sitting && !sittingOnFloor)
		{
			if (myInteract.InsideHouseDetails != null)
			{
				int sitPosition = Mathf.Clamp(myInteract.InsideHouseDetails.houseMapOnTileStatus[sittingXpos, sittingYPos] - sittingInSeat, 0, 3);
				NetworkMapSharer.Instance.RpcGetUp(sitPosition, sittingXpos, sittingYPos, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos);
			}
			else
			{
				int sitPosition2 = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[sittingXpos, sittingYPos] - sittingInSeat, 0, 3);
				NetworkMapSharer.Instance.RpcGetUp(sitPosition2, sittingXpos, sittingYPos, -1, -1);
			}
		}
		if (drivingVehicle)
		{
			CmdStopDriving(drivingVehicleId);
		}
	}

	private IEnumerator justSatDelay()
	{
		yield return new WaitForSeconds(0.5f);
		justSat = false;
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (!Inventory.Instance.CanMoveCharacter())
		{
			if (GiveNPC.give.giveWindowOpen || Inventory.Instance.invOpen)
			{
				if (GiveNPC.give.giveWindowOpen)
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.InGiveMenu);
				}
				else if (Inventory.Instance.dragSlot.itemNo > -1 && Inventory.Instance.allItems[Inventory.Instance.dragSlot.itemNo].checkIfStackable())
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.InChestWhileHoldingItem);
				}
				else
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.InChest);
				}
			}
			else
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.None);
			}
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (myChar.swimming && !myChar.underWater)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.Dive);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (netRod.lineIsCasted)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.Fishing);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (carryingObjectNetId != 0)
		{
			if ((!myEquip.isInside() && (bool)carriedAnimal) || (!myEquip.isInside() && (bool)trappedAnimal))
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.CarryingAnimal);
			}
			else
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.CarryingItem);
			}
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (myInteract.IsPlacingDeed)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.multiTiledPlacing);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (drivingVehicle)
		{
			if ((bool)currentlyDriving)
			{
				if (currentlyDriving.saveId == 2)
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.DrivingBoostVehicle);
				}
				else if (currentlyDriving.saveId == 9)
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.DrivingTractor);
				}
				else if (currentlyDriving.saveId == 8 || currentlyDriving.saveId == 13)
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.DrivingMower);
				}
				else
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.StopDriving);
				}
			}
			else
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.StopDriving);
			}
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if ((bool)currentPassengerPos)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.GetUp);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (sitting)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.GetUp);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (myInteract.myEquip.IsCurrentlyHoldingSinglePlaceableItem() && myInteract.myEquip.itemCurrentlyHolding.placeable.GetRotationFromMap())
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.singleTiledPlacing);
		}
		else if (myInteract.CanCurrentTileBePickedUp())
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.PickUp);
		}
		else
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.None);
		}
		int xPos;
		int yPos;
		if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out var hitInfo, 3.1f, pickUpLayerMask))
		{
			DroppedItem component = hitInfo.transform.GetComponent<DroppedItem>();
			if ((bool)component)
			{
				if (holdingPickUp)
				{
					if (Inventory.Instance.checkIfItemCanFit(component.myItemId, component.stackAmount))
					{
						SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
						_ = base.isServer;
						if (myEquip.myPermissions.CheckIfCanPickUp())
						{
							CmdPickUp(component.netId);
							component.pickUpLocal();
						}
						else
						{
							NotificationManager.manage.pocketsFull.ShowRequirePermission();
						}
					}
					else
					{
						NotificationManager.manage.turnOnPocketsFullNotification(holdingPickUp);
					}
					NotificationManager.manage.showButtonPrompt(Inventory.Instance.allItems[component.myItemId].getInvItemName(), "B", hitInfo.transform.position);
				}
				else if (component.stackAmount > 1 && !Inventory.Instance.allItems[component.myItemId].hasFuel && !Inventory.Instance.allItems[component.myItemId].hasColourVariation)
				{
					NotificationManager.manage.showButtonPrompt(Inventory.Instance.allItems[component.myItemId].getInvItemName() + " X " + component.stackAmount, "B", hitInfo.transform.position);
				}
				else
				{
					NotificationManager.manage.showButtonPrompt(Inventory.Instance.allItems[component.myItemId].getInvItemName(), "B", hitInfo.transform.position);
				}
				return;
			}
			if (NetworkMapSharer.Instance.localChar.underWater)
			{
				BugTypes componentInParent = hitInfo.transform.GetComponentInParent<BugTypes>();
				if ((bool)componentInParent && componentInParent.isUnderwaterCreature)
				{
					NotificationManager.manage.showButtonPrompt(string.Format(ConversationGenerator.generate.GetToolTip("Tip_Catch"), Inventory.Instance.allItems[componentInParent.getBugTypeId()].getInvItemName()), "B", hitInfo.transform.position);
				}
				return;
			}
			InteractableObject interactableObject = hitInfo.transform.GetComponentInParent<InteractableObject>();
			if ((bool)interactableObject && hitInfo.collider.tag != "Wheelbarrow")
			{
				if (hitInfo.collider.tag == "Multiple")
				{
					interactableObject = hitInfo.collider.GetComponent<InteractableObject>();
				}
				if (interactableObject.showingToolTip(hitInfo.collider.transform, this))
				{
					return;
				}
			}
			NotificationManager.manage.hideButtonPrompt();
		}
		else if (myTalkTo.npcInRange != -1)
		{
			if (((bool)myTalkTo.npcTryToTalk && myTalkTo.npcTryToTalk.IsValidConversationTargetForAnyPlayer()) || ((bool)myTalkTo.npcTryToTalk && myTalkTo.npcTryToTalk.canBeTalkedToFollowing()))
			{
				NotificationManager.manage.showButtonPrompt(string.Format(ConversationGenerator.generate.GetToolTip("Tip_TalkTo"), NPCManager.manage.NPCDetails[myTalkTo.npcInRange].GetNPCName()), "B", myTalkTo.npcTryToTalk.transform.position + Vector3.up);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt(NPCManager.manage.NPCDetails[myTalkTo.npcInRange].GetNPCName() + " " + (LocalizedString)"ToolTips/Tip_IsBusy", "no", myTalkTo.npcTryToTalk.transform.position + Vector3.up);
			}
		}
		else if (!myInteract.IsInside() && myInteract.GetTileCloseToMyPosNeedsPrompt(out xPos, out yPos) != 0)
		{
			NotificationManager.manage.showButtonPrompt(myInteract.GetTranslatedStringForTilePromptType(myInteract.GetTileCloseToMyPosNeedsPrompt(out xPos, out yPos), xPos, yPos), "B", new Vector3(xPos * 2, base.transform.position.y, yPos * 2));
		}
		else
		{
			NotificationManager.manage.hideButtonPrompt();
		}
	}

	public void pressX()
	{
		if (carryingObjectNetId != 0)
		{
			if ((!myEquip.isInside() && (bool)carriedAnimal) || (!myEquip.isInside() && (bool)trappedAnimal))
			{
				if (carryingObjectNetId != 0 && myEquip.isCarrying())
				{
					if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 2.5f, Vector3.down, out var hitInfo, 15f, GetComponent<CharMovement>().jumpLayers))
					{
						CmdDropAndRelase(hitInfo.point.y);
						myEquip.setCarrying(newCarrying: false);
					}
					else
					{
						SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
					}
				}
			}
			else if ((myEquip.isInside() && (bool)carriedAnimal) || (myEquip.isInside() && (bool)trappedAnimal))
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			}
			return;
		}
		if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out var hitInfo2, 3f, vehicleMask))
		{
			VehicleHitBox componentInParent = hitInfo2.transform.GetComponentInParent<VehicleHitBox>();
			if ((bool)componentInParent && componentInParent.connectedTo.canBePainted && myEquip.currentlyHoldingItemId > 0 && (bool)Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponent<PaintCan>())
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PaintVehicle);
				CmdPaintVehicle(componentInParent.connectedTo.netId, (int)Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponent<PaintCan>().colourId);
				Inventory.Instance.consumeItemInHand();
			}
		}
		if (!Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out hitInfo2, 3f, pickUpLayerMask))
		{
			return;
		}
		ItemDepositAndChanger componentInParent2 = hitInfo2.transform.GetComponentInParent<ItemDepositAndChanger>();
		if ((bool)componentInParent2 && (bool)myInteract.myEquip.itemCurrentlyHolding && (bool)myInteract.myEquip.itemCurrentlyHolding == componentInParent2.canDepositThisItem(myInteract.myEquip.itemCurrentlyHolding, myInteract.InsideHouseDetails))
		{
			myInteract.InsertItemInTo(componentInParent2, myInteract.myEquip.currentlyHoldingItemId);
		}
		if ((bool)myInteract.myEquip.itemCurrentlyHolding && myInteract.myEquip.itemCurrentlyHolding == HouseEditor.edit.houseKitItem)
		{
			CustomiseHouseZone componentInParent3 = hitInfo2.transform.GetComponentInParent<CustomiseHouseZone>();
			if ((bool)componentInParent3)
			{
				componentInParent3.useCustomisationKit();
			}
		}
		if (myEquip.itemCurrentlyHolding == SignManager.manage.signWritingPen)
		{
			WritableSign componentInParent4 = hitInfo2.transform.GetComponentInParent<WritableSign>();
			if ((bool)componentInParent4 && !componentInParent4.toSpin && !componentInParent4.isAnimalTank && !componentInParent4.isMannequin && !componentInParent4.isToolRack && !componentInParent4.isDisplayCase)
			{
				componentInParent4.editSign();
				return;
			}
		}
		TileObjectGrowthStages componentInParent5 = hitInfo2.transform.GetComponentInParent<TileObjectGrowthStages>();
		if ((bool)componentInParent5 && (bool)myInteract.myEquip.itemCurrentlyHolding)
		{
			for (int i = 0; i < componentInParent5.itemsToPlace.Length; i++)
			{
				if (myInteract.myEquip.itemCurrentlyHolding == componentInParent5.itemsToPlace[i])
				{
					TileObject componentInParent6 = componentInParent5.GetComponentInParent<TileObject>();
					if (WorldManager.Instance.onTileStatusMap[componentInParent6.xPos, componentInParent6.yPos] < componentInParent5.maxStageToReachByPlacing)
					{
						Inventory.Instance.consumeItemInHand();
						CmdChangeStatus(componentInParent6.xPos, componentInParent6.yPos, WorldManager.Instance.onTileStatusMap[componentInParent6.xPos, componentInParent6.yPos] + 1);
					}
				}
			}
		}
		Wheelbarrow componentInParent7 = hitInfo2.transform.GetComponentInParent<Wheelbarrow>();
		if ((bool)componentInParent7 && (bool)myInteract.myEquip.itemCurrentlyHolding)
		{
			if (((bool)myInteract.myEquip.itemCurrentlyHolding == myEquip.IsHoldingShovel() && componentInParent7.totalDirt <= 0) || (componentInParent7.isHoldingAShovel(myInteract.myEquip.itemCurrentlyHolding) && componentInParent7.totalDirt >= 10))
			{
				GetComponent<Animator>().SetTrigger("Clang");
			}
			else if ((bool)myInteract.myEquip.itemCurrentlyHolding == myEquip.IsHoldingShovel() && componentInParent7.totalDirt > 0)
			{
				myEquip.itemCurrentlyHolding.changeToWhenUsed = WorldManager.Instance.tileTypes[componentInParent7.topDirtId].uniqueShovel;
				GetComponent<Animator>().SetTrigger("WheelBarrow");
				Inventory.Instance.changeItemInHand();
				CmdRemoveFromBarrow(componentInParent7.netId);
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.Wheelbarrow);
			}
			else if (componentInParent7.isHoldingAShovel(myInteract.myEquip.itemCurrentlyHolding) && componentInParent7.totalDirt < 10)
			{
				CmdAddToBarrow(componentInParent7.netId, myInteract.myEquip.itemCurrentlyHolding.resultingTileType[0]);
				GetComponent<Animator>().SetTrigger("WheelBarrow");
				Inventory.Instance.changeItemInHand();
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.Wheelbarrow);
			}
		}
		FarmAnimal componentInParent8 = hitInfo2.transform.GetComponentInParent<FarmAnimal>();
		if ((bool)componentInParent8 && (bool)componentInParent8.canBeHarvested && componentInParent8.canBeHarvested.checkIfCanHarvest(myEquip.itemCurrentlyHolding))
		{
			if (componentInParent8.canBeHarvested.taskWhenHarvested != 0)
			{
				DailyTaskGenerator.generate.doATask(componentInParent8.canBeHarvested.taskWhenHarvested);
			}
			if (componentInParent8.canBeHarvested.harvestToInv)
			{
				CmdHarvestAnimalToInv(componentInParent8.netId);
			}
			else
			{
				CmdHarvestAnimal(componentInParent8.netId);
			}
			return;
		}
		if (hitInfo2.transform.CompareTag("Paintable"))
		{
			TileObject componentInParent9 = hitInfo2.transform.GetComponentInParent<TileObject>();
			if ((myInteract.InsideHouseDetails == null && !WorldManager.Instance.CheckTileClientLock(componentInParent9.xPos, componentInParent9.yPos)) || (myInteract.InsideHouseDetails != null && !WorldManager.Instance.checkTileClientLockHouse(componentInParent9.xPos, componentInParent9.yPos, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos)))
			{
				ItemPaintVarient component = WorldManager.Instance.allObjectSettings[componentInParent9.tileObjectId].GetComponent<ItemPaintVarient>();
				if ((bool)componentInParent9 && (bool)component && myEquip.currentlyHoldingItemId > 0 && (bool)Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponent<PaintCan>() && component.varients[(int)Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponent<PaintCan>().colourId].tileObjectId != componentInParent9.tileObjectId)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PaintVehicle);
					CmdChangeTileVariation((int)Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponent<PaintCan>().colourId, componentInParent9.xPos, componentInParent9.yPos);
					Inventory.Instance.consumeItemInHand();
				}
			}
		}
		ItemSign componentInParent10 = hitInfo2.transform.GetComponentInParent<ItemSign>();
		if ((bool)componentInParent10 && componentInParent10.isSilo && myEquip.currentlyHoldingItemId == Inventory.Instance.getInvItemId(componentInParent10.itemCanPlaceIn))
		{
			TileObject componentInParent11 = componentInParent10.GetComponentInParent<TileObject>();
			if (WorldManager.Instance.onTileStatusMap[componentInParent11.xPos, componentInParent11.yPos] < 200)
			{
				Inventory.Instance.consumeItemInHand();
				myInteract.CmdPlayPlaceableAnimation();
				CmdAddToSilo(componentInParent11.xPos, componentInParent11.yPos);
			}
		}
		AnimalAI componentInParent12 = hitInfo2.transform.GetComponentInParent<AnimalAI>();
		if (!componentInParent12 || myEquip.currentlyHoldingItemId <= 0 || !Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponent<PlaceOnAnimal>())
		{
			return;
		}
		PlaceOnAnimal[] componentsInChildren = Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponentsInChildren<PlaceOnAnimal>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (componentInParent12.animalId == componentsInChildren[j].toPlaceOn.animalId)
			{
				CmdPlaceOntoAnimal(myEquip.currentlyHoldingItemId, componentInParent12.netId);
				Inventory.Instance.consumeItemInHand();
			}
		}
	}

	public bool isCarryingSomething()
	{
		if (carryingObjectNetId != 0)
		{
			return true;
		}
		return false;
	}

	public void pressY()
	{
		if (!Inventory.Instance.CanMoveCharacter())
		{
			return;
		}
		if (drivingVehicle)
		{
			GetComponent<CharMovement>().getOutVehicle();
			drivingVehicle = false;
			CmdStopDriving(drivingVehicleId);
			CameraController.control.ConnectVehicle(null);
			myEquip.setDriving(newDriving: false);
		}
		else if ((bool)currentPassengerPos)
		{
			myChar.getOutVehiclePassenger();
			CmdStopPassenger(vehiclePassengerId);
			currentPassengerPos = null;
			myEquip.setDriving(newDriving: false);
		}
		else
		{
			if (!sitting || justSat)
			{
				return;
			}
			if (sittingInHairDresserSeat && !ConversationManager.manage.IsConversationActive)
			{
				StopCoroutine(talkToHairDresserOnceInSeat());
				CmdGetUpFromHairDresserSeat();
				sitting = false;
			}
			else if (sittingInTuckshopChair != -1)
			{
				CmdGetUpFromTuckshopSeat(sittingInTuckshopChair);
				sittingInTuckshopChair = -1;
				sitting = false;
			}
			else if (!sittingInHairDresserSeat)
			{
				myEquip.setLayDown(newLayingDown: false);
				CmdGetUp(sittingInSeat, sittingXpos, sittingYPos);
				sitting = false;
			}
			if (sittingOnFloor)
			{
				sittingOnFloor = false;
				sitting = false;
				CmdGetUpFromFloor();
			}
			else if (!sitting)
			{
				GetComponent<Rigidbody>().isKinematic = false;
				GetComponent<Animator>().SetTrigger("Standing");
				GetComponent<Animator>().SetBool("SittingOrLaying", value: false);
				Vector3 normalized = (positionBeforeSitting - base.transform.position).normalized;
				normalized.y = 0f;
				base.transform.rotation = Quaternion.LookRotation(normalized);
				if (Vector3.Distance(base.transform.position, positionBeforeSitting) < 4f)
				{
					base.transform.position = positionBeforeSitting;
				}
				else
				{
					base.transform.position = base.transform.position + base.transform.forward;
				}
			}
			justSat = true;
			StartCoroutine(justSatDelay());
		}
	}

	[ClientRpc]
	public void RpcStopDrivingFromServer()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcStopDrivingFromServer", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcStopPassengerFromServer()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcStopPassengerFromServer", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void fallOffVehicleOnPassOut()
	{
		if (drivingVehicle)
		{
			GetComponent<CharMovement>().getOutVehicle();
			drivingVehicle = false;
			CmdStopDriving(drivingVehicleId);
			CameraController.control.ConnectVehicle(null);
			myEquip.setDriving(newDriving: false);
		}
		if ((bool)currentPassengerPos)
		{
			myChar.getOutVehiclePassenger();
			CmdStopPassenger(vehiclePassengerId);
			currentPassengerPos = null;
			myEquip.setDriving(newDriving: false);
		}
	}

	public void dropItemOnPassOut()
	{
		if (carryingObjectNetId == 0 || !myEquip.isCarrying())
		{
			return;
		}
		if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 1.6f, Vector3.down, out var hitInfo, 15f, pickUpLayerMask) && (bool)hitInfo.transform && hitInfo.transform.tag == "DropOffSpot")
		{
			CmdPutDownObjectInDropPoint(hitInfo.transform.position);
			myEquip.setCarrying(newCarrying: false);
		}
		else if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 1.6f, Vector3.down, out hitInfo, 15f, GetComponent<CharMovement>().jumpLayers))
		{
			Physics.Raycast(base.transform.position + Vector3.up * 15f + base.transform.forward * 1.6f, Vector3.down, out var hitInfo2, 30f, GetComponent<CharMovement>().jumpLayers);
			if (hitInfo2.transform.gameObject.layer == LayerMask.NameToLayer("Building") || hitInfo2.transform.gameObject.tag == "Walls")
			{
				CmdDropObject();
				myEquip.setCarrying(newCarrying: false);
				return;
			}
			if (hitInfo.transform.tag == "DropOffSpot")
			{
				CmdPutDownObjectInDropPoint(hitInfo.transform.position);
			}
			else
			{
				CmdPutDownObject(hitInfo.point.y);
			}
			myEquip.setCarrying(newCarrying: false);
		}
		else
		{
			CmdDropObject();
			myEquip.setCarrying(newCarrying: false);
		}
	}

	public bool pickUp()
	{
		if (!MenuButtonsTop.menu.closed || !Inventory.Instance.CanMoveCharacter())
		{
			return true;
		}
		if (carryingObjectNetId != 0 && myEquip.isCarrying())
		{
			if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 1.6f, Vector3.down, out var hitInfo, 15f, pickUpLayerMask) && (bool)hitInfo.transform && hitInfo.transform.tag == "DropOffSpot")
			{
				CmdPutDownObjectInDropPoint(hitInfo.transform.position);
				myEquip.setCarrying(newCarrying: false);
			}
			else if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 1.6f, Vector3.down, out hitInfo, 15f, GetComponent<CharMovement>().jumpLayers))
			{
				Physics.Raycast(base.transform.position + Vector3.up * 15f + base.transform.forward * 1.6f, Vector3.down, out var hitInfo2, 30f, GetComponent<CharMovement>().jumpLayers);
				if (hitInfo2.transform.gameObject.layer == LayerMask.NameToLayer("Building") || hitInfo2.transform.gameObject.tag == "Walls")
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
				}
				else
				{
					if (hitInfo.transform.tag == "DropOffSpot")
					{
						CmdPutDownObjectInDropPoint(hitInfo.transform.position);
					}
					else
					{
						CmdPutDownObject(hitInfo.point.y);
					}
					myEquip.setCarrying(newCarrying: false);
				}
			}
			else
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			}
			return true;
		}
		if (drivingVehicle)
		{
			return true;
		}
		if (sitting && !justSat)
		{
			return true;
		}
		if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out var hitInfo3, 3.1f, pickUpLayerMask))
		{
			if (hitInfo3.collider.tag == "Multiple")
			{
				InteractableObject component = hitInfo3.collider.GetComponent<InteractableObject>();
				if ((bool)component.isVehicle)
				{
					if (!drivingVehicle)
					{
						if (myEquip.myPermissions.CheckIfCanInteractWithVehicles())
						{
							drivingVehicle = true;
							drivingVehicleId = component.isVehicle.netId;
							GetComponent<CharMovement>().getInVehicle(component.isVehicle);
							CmdStartDriving(component.isVehicle.netId);
							InputMaster.input.connectRumbleToVehicle(component.isVehicle.GetComponent<VehicleMakeParticles>());
							CameraController.control.ConnectVehicle(component.isVehicle);
							component.isVehicle.transform.position = component.isVehicle.transform.position;
							myEquip.setDriving(newDriving: true);
							currentlyDriving = component.isVehicle;
						}
						else
						{
							NotificationManager.manage.pocketsFull.ShowRequirePermission();
						}
					}
					return true;
				}
				if ((bool)component.isFarmAnimal)
				{
					if (component.isFarmAnimal.canPat)
					{
						if (!component.isFarmAnimal.hasBeenPatted)
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PetAnimals);
						}
						CmdPetAnimal(component.isFarmAnimal.netId);
					}
					return true;
				}
			}
			DroppedItem component2 = hitInfo3.transform.GetComponent<DroppedItem>();
			if ((bool)component2)
			{
				if (Inventory.Instance.checkIfItemCanFit(component2.myItemId, component2.stackAmount))
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
					if (myEquip.myPermissions.CheckIfCanPickUp())
					{
						CmdPickUp(component2.netId);
						component2.pickUpLocal();
					}
					else
					{
						NotificationManager.manage.pocketsFull.ShowRequirePermission();
					}
				}
				else
				{
					NotificationManager.manage.turnOnPocketsFullNotification();
				}
				return true;
			}
			PickUpAndCarry componentInParent = hitInfo3.transform.GetComponentInParent<PickUpAndCarry>();
			if ((bool)componentInParent && componentInParent.canBePickedUp && !componentInParent.IsCarriedByPlayer)
			{
				CmdPickUpObject(componentInParent.netId);
				myEquip.setCarrying(newCarrying: true);
				return true;
			}
			ShopBuyDrop componentInParent2 = hitInfo3.transform.GetComponentInParent<ShopBuyDrop>();
			if ((bool)componentInParent2)
			{
				if (componentInParent2.canTalkToKeeper() && !componentInParent2.isKeeperWorking())
				{
					if (componentInParent2.canTalkToKeeper() && (bool)componentInParent2.closedConvo)
					{
						componentInParent2.TryAndBuyItem();
					}
				}
				else
				{
					componentInParent2.TryAndBuyItem();
				}
				return true;
			}
			ChestPlaceable componentInParent3 = hitInfo3.transform.GetComponentInParent<ChestPlaceable>();
			if ((bool)componentInParent3)
			{
				if (componentInParent3.isStash)
				{
					ContainerManager.manage.openStash(0);
					CmdOpenStash(0);
				}
				else
				{
					CmdOpenChest(componentInParent3.myXPos(), componentInParent3.myYPos());
				}
				return true;
			}
			ReadableSign component3 = hitInfo3.transform.GetComponent<ReadableSign>();
			if ((bool)component3)
			{
				Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
				component3.readSign();
				return true;
			}
			if ((bool)hitInfo3.transform.GetComponentInParent<MailBoxShowsMail>())
			{
				MailManager.manage.openMailWindow();
				return true;
			}
			if ((bool)hitInfo3.transform.GetComponentInParent<BulletinBoardShowNewMessage>())
			{
				if (!base.isServer && !BulletinBoard.board.clientLoaded)
				{
					BulletinBoard.board.clientLoaded = true;
					Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
					CmdFillBulletinBoard();
					return true;
				}
				Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
				BulletinBoard.board.openWindow();
				return true;
			}
			WorkTable componentInParent4 = hitInfo3.transform.GetComponentInParent<WorkTable>();
			if ((bool)componentInParent4)
			{
				if ((bool)componentInParent4.tableText)
				{
					componentInParent4.checkForItemAndChangeText();
					Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
					componentInParent4.tableText.readSign();
				}
				else
				{
					Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
					TileObject componentInParent5 = componentInParent4.GetComponentInParent<TileObject>();
					CraftingManager.manage.openCloseCraftMenuWithTableCoords(isMenuOpen: true, componentInParent5.xPos, componentInParent5.yPos, componentInParent4.typeOfCrafting);
				}
				return true;
			}
			FurnitureStatus componentInParent6 = hitInfo3.transform.GetComponentInParent<FurnitureStatus>();
			if ((bool)componentInParent6 && !justSat)
			{
				if (hitInfo3.transform == componentInParent6.seatPosition1.transform)
				{
					sittingInSeat = 1;
				}
				else if (hitInfo3.transform == componentInParent6.seatPosition2.transform)
				{
					sittingInSeat = 2;
				}
				sitting = true;
				sittingPosition = hitInfo3.transform;
				sittingXpos = componentInParent6.showingX;
				sittingYPos = componentInParent6.showingY;
				GetComponent<Rigidbody>().isKinematic = true;
				if ((bool)componentInParent6.GetComponent<HairDresserSeat>())
				{
					sittingInHairDresserSeat = true;
					positionBeforeSitting = base.transform.position;
					StartCoroutine("talkToHairDresserOnceInSeat");
					sitting = true;
					CmdSitInHairDresserSeat(sittingPosition.position);
				}
				else if ((bool)componentInParent6.GetComponent<TuckshopSeat>())
				{
					sittingInTuckshopChair = componentInParent6.GetComponent<TuckshopSeat>().mySeatId;
					positionBeforeSitting = base.transform.position;
					StartCoroutine("talkToHairDresserOnceInSeat");
					sitting = true;
					CmdSitInTuckshopSeat(sittingPosition.position, sittingInTuckshopChair);
				}
				else
				{
					int num = -1;
					if (myInteract.IsInsidePlayerHouse)
					{
						CmdSitDown(sittingInSeat, componentInParent6.showingX, componentInParent6.showingY, sittingPosition.position, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos);
						num = myInteract.InsideHouseDetails.houseMapOnTile[componentInParent6.showingX, componentInParent6.showingY];
						if (!componentInParent6.isSeat)
						{
							myEquip.setLayDown(newLayingDown: true);
							WorldManager.Instance.confirmSleepSign.signConvo = WorldManager.Instance.GetSleepText();
							WorldManager.Instance.confirmSleepSign.readSign();
						}
						positionBeforeSitting = base.transform.position;
						if (componentInParent6.isSeat && componentInParent6.isToilet)
						{
							StatusManager.manage.ClearFoodAndBuffs();
						}
					}
					else
					{
						CmdSitDown(sittingInSeat, componentInParent6.showingX, componentInParent6.showingY, sittingPosition.position, -1, -1);
						num = WorldManager.Instance.onTileMap[componentInParent6.showingX, componentInParent6.showingY];
						if (!componentInParent6.isSeat)
						{
							WorldManager.Instance.confirmSleepSign.signConvo = WorldManager.Instance.GetSleepText();
							WorldManager.Instance.confirmSleepSign.readSign();
						}
						positionBeforeSitting = base.transform.position;
						if (componentInParent6.isSeat && componentInParent6.isToilet)
						{
							StatusManager.manage.ClearFoodAndBuffs();
						}
					}
					if (num >= 0 && (bool)WorldManager.Instance.allObjects[num].tileObjectFurniture && !WorldManager.Instance.allObjects[num].tileObjectFurniture.isSeat)
					{
						myEquip.setLayDown(newLayingDown: true);
						lastLayedDownPos = base.transform.position;
						sleepingInside = myInteract.InsideHouseDetails;
						positionBeforeSitting = base.transform.position;
					}
				}
				justSat = true;
				StartCoroutine(justSatDelay());
				return true;
			}
			MineControls component4 = hitInfo3.transform.GetComponent<MineControls>();
			if ((bool)component4)
			{
				component4.useControls();
				return true;
			}
			Vehicle componentInParent7 = hitInfo3.transform.GetComponentInParent<Vehicle>();
			if ((bool)componentInParent7 && hitInfo3.collider.tag != "Wheelbarrow")
			{
				if (hitInfo3.collider.tag == "VehicleStorage")
				{
					VehicleStorage component5 = componentInParent7.GetComponent<VehicleStorage>();
					if ((bool)component5)
					{
						CmdOpenVehicleStorage(component5.netId);
						return true;
					}
				}
				if (hitInfo3.collider.tag == "Passenger")
				{
					VehicleSecondSeat component6 = componentInParent7.GetComponent<VehicleSecondSeat>();
					if ((bool)component6)
					{
						vehiclePassengerId = component6.netId;
						currentPassengerPos = NetworkIdentity.spawned[vehiclePassengerId].GetComponent<VehicleSecondSeat>();
						myChar.getInVehiclePassenger();
						CmdStartPassenger(component6.netId);
						myEquip.setDriving(newDriving: true);
						return true;
					}
				}
				if (LicenceManager.manage.allLicences[7].getCurrentLevel() < componentInParent7.requiresLicenceLevel)
				{
					NotificationManager.manage.pocketsFull.showNoLicence(LicenceManager.LicenceTypes.Vehicle);
				}
				else if (!drivingVehicle)
				{
					if (myEquip.myPermissions.CheckIfCanInteractWithVehicles())
					{
						drivingVehicle = true;
						drivingVehicleId = componentInParent7.netId;
						GetComponent<CharMovement>().getInVehicle(componentInParent7);
						CmdStartDriving(componentInParent7.netId);
						InputMaster.input.connectRumbleToVehicle(componentInParent7.GetComponent<VehicleMakeParticles>());
						CameraController.control.ConnectVehicle(componentInParent7);
						componentInParent7.transform.position = componentInParent7.transform.position;
						myEquip.setDriving(newDriving: true);
						currentlyDriving = componentInParent7;
					}
					else
					{
						NotificationManager.manage.pocketsFull.ShowRequirePermission();
					}
				}
				return true;
			}
			FarmAnimal componentInParent8 = hitInfo3.transform.GetComponentInParent<FarmAnimal>();
			if ((bool)componentInParent8)
			{
				if (componentInParent8.canPat)
				{
					if (!componentInParent8.hasBeenPatted)
					{
						DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PetAnimals);
					}
					CmdPetAnimal(componentInParent8.netId);
				}
				return true;
			}
			MuseumPainting componentInParent9 = hitInfo3.transform.GetComponentInParent<MuseumPainting>();
			if ((bool)componentInParent9)
			{
				if (componentInParent9.checkIfMuseumKeeperCanBeTalkedTo() && componentInParent9.checkIfMuseumKeeperIsAtWork())
				{
					componentInParent9.askAboutPainting();
				}
				return true;
			}
			WritableSign componentInParent10 = hitInfo3.transform.GetComponentInParent<WritableSign>();
			if ((bool)componentInParent10 && (bool)componentInParent10.toSpin)
			{
				componentInParent10.editSign();
				return true;
			}
			if (NetworkMapSharer.Instance.localChar.underWater)
			{
				BugTypes componentInParent11 = hitInfo3.transform.GetComponentInParent<BugTypes>();
				if ((bool)componentInParent11 && componentInParent11.isUnderwaterCreature)
				{
					if (Inventory.Instance.addItemToInventory(componentInParent11.getBugTypeId(), 1))
					{
						CharLevelManager.manage.addToDayTally(componentInParent11.getBugTypeId(), 1, 3);
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Fishing, 4);
						SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpUnderwaterCreature);
						PediaManager.manage.addCaughtToList(componentInParent11.getBugTypeId());
						CmdCatchUnderwater(componentInParent11.netId);
						DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CatchACritter);
					}
					else
					{
						NotificationManager.manage.turnOnPocketsFullNotification();
					}
				}
			}
			CamerTripod componentInParent12 = hitInfo3.transform.GetComponentInParent<CamerTripod>();
			if ((bool)componentInParent12)
			{
				componentInParent12.useTripod();
			}
			BoomBox componentInParent13 = hitInfo3.transform.GetComponentInParent<BoomBox>();
			if ((bool)componentInParent13)
			{
				componentInParent13.InteractWithBoomBox();
			}
			CharMovement componentInParent14 = hitInfo3.transform.GetComponentInParent<CharMovement>();
			if ((bool)componentInParent14)
			{
				componentInParent14.reviveBox.SetActive(value: false);
				CmdReviveOtherChar(componentInParent14.netId);
				return true;
			}
			ItemSign componentInParent15 = hitInfo3.transform.GetComponentInParent<ItemSign>();
			if ((bool)componentInParent15 && !componentInParent15.isSilo && (bool)myEquip.itemCurrentlyHolding)
			{
				TileObject component7 = componentInParent15.GetComponent<TileObject>();
				if (myInteract.InsideHouseDetails == null)
				{
					CmdChangeSignItem(myEquip.currentlyHoldingItemId, component7.xPos, component7.yPos, -1, -1);
				}
				else
				{
					CmdChangeSignItem(myEquip.currentlyHoldingItemId, component7.xPos, component7.yPos, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos);
				}
			}
			return false;
		}
		return myTalkTo.talkOrUse();
	}

	private void onCarryChanged(uint oldCarry, uint newCarry)
	{
		if (base.isLocalPlayer)
		{
			carriedAnimal = null;
			trappedAnimal = null;
			if (newCarry != 0)
			{
				carriedAnimal = NetworkIdentity.spawned[newCarry].GetComponent<AnimalCarryBox>();
				trappedAnimal = NetworkIdentity.spawned[newCarry].GetComponent<TrappedAnimal>();
			}
		}
		NetworkcarryingObjectNetId = newCarry;
	}

	private void onSittingChanged(Vector3 old, Vector3 newSittingPos)
	{
		NetworksittingPos = newSittingPos;
	}

	public void PressSitOnFloor()
	{
		if (!sitting && !drivingVehicle && !sittingOnFloor)
		{
			sittingPosition = null;
			positionBeforeSitting = base.transform.position;
			sittingOnFloor = true;
			sitting = true;
			CmdSitOnFloor();
		}
	}

	[Command]
	public void CmdChangeSignItem(int itemId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeSignItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdAddToSilo(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdAddToSilo", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestAnimalToInv(uint animalToHarvest)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToHarvest);
		SendCommandInternal(typeof(CharPickUp), "CmdHarvestAnimalToInv", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestAnimal(uint animalToHarvest)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToHarvest);
		SendCommandInternal(typeof(CharPickUp), "CmdHarvestAnimal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPetAnimal(uint animalToPet)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToPet);
		SendCommandInternal(typeof(CharPickUp), "CmdPetAnimal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdReviveOtherChar(uint reviveId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(reviveId);
		SendCommandInternal(typeof(CharPickUp), "CmdReviveOtherChar", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator reviveDelayServer(uint reviveId)
	{
		yield return new WaitForSeconds(1f);
		NetworkIdentity.spawned[reviveId].GetComponent<Damageable>().Networkhealth = 5;
		NetworkIdentity.spawned[reviveId].GetComponent<CharMovement>().Networkstamina = 50;
		NetworkMapSharer.Instance.TargetGiveStamina(NetworkIdentity.spawned[reviveId].connectionToClient);
	}

	private IEnumerator reviveDelayClient(CharMovement myChar)
	{
		myChar.reviveBox.SetActive(value: false);
		StartCoroutine(playPetAnimation());
		yield return new WaitForSeconds(1f);
		myChar.myAnim.SetBool("Fainted", value: false);
	}

	[ClientRpc]
	public void RpcReviveDelay(uint reviveId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(reviveId);
		SendRPCInternal(typeof(CharPickUp), "RpcReviveDelay", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSitInHairDresserSeat(Vector3 newSittingPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newSittingPos);
		SendCommandInternal(typeof(CharPickUp), "CmdSitInHairDresserSeat", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSitInTuckshopSeat(Vector3 newSittingPos, int seatId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newSittingPos);
		writer.WriteInt(seatId);
		SendCommandInternal(typeof(CharPickUp), "CmdSitInTuckshopSeat", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdGetUpFromHairDresserSeat()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdGetUpFromHairDresserSeat", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdGetUpFromFloor()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdGetUpFromFloor", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSitOnFloor()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdSitOnFloor", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdGetUpFromTuckshopSeat(int gettingUpFrom)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(gettingUpFrom);
		SendCommandInternal(typeof(CharPickUp), "CmdGetUpFromTuckshopSeat", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSitDown(int seatNo, int xPos, int yPos, Vector3 newSittingPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(seatNo);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteVector3(newSittingPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharPickUp), "CmdSitDown", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void confirmSleep()
	{
		TownManager.manage.lastSleptPos = lastLayedDownPos;
		TownManager.manage.sleepInsideHouse = sleepingInside;
		CmdConfirmSleep();
	}

	[Command]
	private void CmdConfirmSleep()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdConfirmSleep", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void SetReadyToSleep(bool ready)
	{
		if (ready)
		{
			NetworkNavMesh.nav.addSleepingChar(base.transform);
		}
		else
		{
			NetworkNavMesh.nav.removeSleepingChar(base.transform);
		}
	}

	[ClientRpc]
	private void RpcCharPetAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcCharPetAnimation", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator playPetAnimation()
	{
		GetComponent<Animator>().SetTrigger("Pet");
		if (base.isLocalPlayer)
		{
			myEquip.setPetting(newPetting: true);
			CharMovement myMove = GetComponent<CharMovement>();
			myMove.attackLockOn(isOn: true);
			yield return new WaitForSeconds(1.8f);
			myMove.attackLockOn(isOn: false);
			myEquip.setPetting(newPetting: false);
		}
	}

	[ClientRpc]
	private void RpcSittingLayingOrStanding(int stat)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stat);
		SendRPCInternal(typeof(CharPickUp), "RpcSittingLayingOrStanding", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcChangeSittinOnFloor(bool isSittinOnFloor)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isSittinOnFloor);
		SendRPCInternal(typeof(CharPickUp), "RpcChangeSittinOnFloor", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public bool isLayingDown()
	{
		return sittingLayingOrStanding == 2;
	}

	[Command]
	private void CmdGetUp(int seatNo, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(seatNo);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdGetUp", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPickUp(uint pickUpId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(pickUpId);
		SendCommandInternal(typeof(CharPickUp), "CmdPickUp", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetAddPickupToInv(NetworkConnection con, int itemId, int stack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(stack);
		SendTargetRPCInternal(con, typeof(CharPickUp), "TargetAddPickupToInv", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenChest(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdOpenChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestChestForCrafting(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdRequestChestForCrafting", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenVehicleStorage(uint vehicleNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleNetId);
		SendCommandInternal(typeof(CharPickUp), "CmdOpenVehicleStorage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenStash(int stashId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stashId);
		SendCommandInternal(typeof(CharPickUp), "CmdOpenStash", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPlayOpenStashSound()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcPlayOpenStashSound", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeOneInChest(int xPos, int yPos, int slotNo, int newSlotId, int newStackNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(slotNo);
		writer.WriteInt(newSlotId);
		writer.WriteInt(newStackNo);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeOneInChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakeItemsFromChestInChest(int itemId, int amountToRemove, int remoteTablePosX, int remoteTablePosY, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(amountToRemove);
		writer.WriteInt(remoteTablePosX);
		writer.WriteInt(remoteTablePosY);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharPickUp), "CmdTakeItemsFromChestInChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeOneInVehicleStorage(uint vehicleId, int slotNo, int newSlotId, int newStackNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleId);
		writer.WriteInt(slotNo);
		writer.WriteInt(newSlotId);
		writer.WriteInt(newStackNo);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeOneInVehicleStorage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStartDriving(uint vehicleToDrive)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleToDrive);
		SendCommandInternal(typeof(CharPickUp), "CmdStartDriving", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStartPassenger(uint passengerVehicle)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(passengerVehicle);
		SendCommandInternal(typeof(CharPickUp), "CmdStartPassenger", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStopPassenger(uint passengerVehicle)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(passengerVehicle);
		SendCommandInternal(typeof(CharPickUp), "CmdStopPassenger", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcNetworkTransformOnForVehicle(bool shouldNetworkTransformSync, Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(shouldNetworkTransformSync);
		writer.WriteVector3(position);
		SendRPCInternal(typeof(CharPickUp), "RpcNetworkTransformOnForVehicle", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStopDriving(uint vehicleToDrive)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleToDrive);
		SendCommandInternal(typeof(CharPickUp), "CmdStopDriving", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDropCarriedItem()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcDropCarriedItem", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpObject(uint pickedUpObjectNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(pickedUpObjectNetId);
		SendCommandInternal(typeof(CharPickUp), "CmdPickUpObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPutDownObject(float heightDroppedAt)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(heightDroppedAt);
		SendCommandInternal(typeof(CharPickUp), "CmdPutDownObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDropObject()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdDropObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPutDownObjectInDropPoint(Vector3 dropPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(dropPos);
		SendCommandInternal(typeof(CharPickUp), "CmdPutDownObjectInDropPoint", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDropAndRelase(float heightDroppedAt)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(heightDroppedAt);
		SendCommandInternal(typeof(CharPickUp), "CmdDropAndRelase", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeStatus(int xPos, int yPos, int newStatus)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(newStatus);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceOntoAnimal(int itemPlacingOn, uint animalNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemPlacingOn);
		writer.WriteUInt(animalNetId);
		SendCommandInternal(typeof(CharPickUp), "CmdPlaceOntoAnimal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdFillBulletinBoard()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdFillBulletinBoard", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdAddToBarrow(uint barrowId, int tileTypeToAdd)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(barrowId);
		writer.WriteInt(tileTypeToAdd);
		SendCommandInternal(typeof(CharPickUp), "CmdAddToBarrow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdRemoveFromBarrow(uint barrowId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(barrowId);
		SendCommandInternal(typeof(CharPickUp), "CmdRemoveFromBarrow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPaintVehicle(uint vehicleId, int colourId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleId);
		writer.WriteInt(colourId);
		SendCommandInternal(typeof(CharPickUp), "CmdPaintVehicle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPaintVehicle(uint vehicle, int colourId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicle);
		writer.WriteInt(colourId);
		SendRPCInternal(typeof(CharPickUp), "RpcPaintVehicle", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator talkToHairDresserOnceInSeat()
	{
		yield return new WaitForSeconds(0.25f);
		while (sittingInHairDresserSeat)
		{
			if ((bool)NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Barber) && NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Barber).isAtWork() && NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Barber).IsValidConversationTargetForAnyPlayer())
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Barber), HairDresserSeat.seat.hairDresserConvo);
				break;
			}
			yield return null;
		}
	}

	public void CmdCatchUnderwater(uint idToCatch)
	{
		NetworkNavMesh.nav.UnSpawnAnAnimal(NetworkIdentity.spawned[idToCatch].GetComponent<AnimalAI>(), saveToMap: false);
	}

	[Command]
	public void CmdChangeTileVariation(int newColour, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newColour);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeTileVariation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void ConfirmUseMineElevator()
	{
		if (MineEnterExit.mineEntrance.checkIfAllPlayersAreInElevator())
		{
			if (!RealWorldTimeLight.time.underGround)
			{
				Inventory.Instance.removeAmountOfItem(Inventory.Instance.getInvItemId(Inventory.Instance.minePass), 1);
				NetworkMapSharer.Instance.canUseMineControls = false;
				CmdGoDownMines();
			}
			else
			{
				NetworkMapSharer.Instance.canUseMineControls = false;
				CmdGoUpMines();
			}
		}
		else
		{
			StartCoroutine(warnNoPlayersInLift());
		}
	}

	public void ConfirmUseAirshipTakeOff()
	{
		if (AirportEntranceExit.entrance.CheckIfAllPlayersAreInAirShip())
		{
			if (!RealWorldTimeLight.time.offIsland)
			{
				Inventory.Instance.removeAmountOfItem(Inventory.Instance.getInvItemId(Inventory.Instance.boardingPass), 1);
				NetworkMapSharer.Instance.canUseMineControls = false;
				CmdTakeAirShipOffIsland();
			}
			else
			{
				NetworkMapSharer.Instance.canUseMineControls = false;
				CmdTakeAirShipHome();
			}
		}
		else
		{
			StartCoroutine(warnNoPlayersInLift());
		}
	}

	private IEnumerator warnNoPlayersInLift()
	{
		while (ConversationManager.manage.IsConversationActive)
		{
			yield return null;
		}
		ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, MineEnterExit.mineEntrance.allPlayersMustBeInLiftConvo);
	}

	[Command]
	public void CmdGoDownMines()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdGoDownMines", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakeAirShipOffIsland()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdTakeAirShipOffIsland", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGoUpMines()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdGoUpMines", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakeAirShipHome()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdTakeAirShipHome", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdEditSignDetails(int xPos, int yPos, int houseX, int houseY, string message)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		writer.WriteString(message);
		SendCommandInternal(typeof(CharPickUp), "CmdEditSignDetails", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateSignDetails(int xPos, int yPos, int houseX, int houseY, string message)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		writer.WriteString(message);
		SendRPCInternal(typeof(CharPickUp), "RpcUpdateSignDetails", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void ChangeKinematicForLevelChange(bool shouldBeKinematic)
	{
		if (base.isLocalPlayer && !sitting)
		{
			GetComponent<Rigidbody>().isKinematic = shouldBeKinematic;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcStopDrivingFromServer()
	{
		if (base.isLocalPlayer)
		{
			CameraController.control.ConnectVehicle(null);
		}
		GetComponent<CharMovement>().getOutVehicle();
		drivingVehicle = false;
		drivingVehicleId = 0u;
		myEquip.setDriving(newDriving: false);
		myChar.normalNetworkTransform.ResetObjectsInterpolation();
		myChar.normalNetworkTransform.syncPosition = true;
		myChar.normalNetworkTransform.syncRotation = true;
	}

	protected static void InvokeUserCode_RpcStopDrivingFromServer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopDrivingFromServer called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcStopDrivingFromServer();
		}
	}

	protected void UserCode_RpcStopPassengerFromServer()
	{
		myChar.getOutVehiclePassenger();
		currentPassengerPos = null;
		myEquip.setDriving(newDriving: false);
		myChar.normalNetworkTransform.ResetObjectsInterpolation();
		myChar.normalNetworkTransform.syncPosition = true;
		myChar.normalNetworkTransform.syncRotation = true;
	}

	protected static void InvokeUserCode_RpcStopPassengerFromServer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopPassengerFromServer called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcStopPassengerFromServer();
		}
	}

	protected void UserCode_CmdChangeSignItem(int itemId, int xPos, int yPos, int houseX, int houseY)
	{
		if (houseX == -1 && houseY == -1)
		{
			NetworkMapSharer.Instance.RpcGiveOnTileStatus(itemId, xPos, yPos);
		}
		else
		{
			NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(itemId, xPos, yPos, houseX, houseY);
		}
	}

	protected static void InvokeUserCode_CmdChangeSignItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeSignItem called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeSignItem(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdAddToSilo(int xPos, int yPos)
	{
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos] + 1, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdAddToSilo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddToSilo called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdAddToSilo(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdHarvestAnimalToInv(uint animalToHarvest)
	{
		NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>().harvestFromServer();
		NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>().TargetGiveItemToNonLocal(base.connectionToClient, NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>().getHarvestedItem().getItemId());
	}

	protected static void InvokeUserCode_CmdHarvestAnimalToInv(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestAnimalToInv called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdHarvestAnimalToInv(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdHarvestAnimal(uint animalToHarvest)
	{
		FarmAnimalHarvest component = NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>();
		component.harvestFromServer();
		NetworkMapSharer.Instance.spawnAServerDrop(Inventory.Instance.getInvItemId(component.getHarvestedItem()), 1, component.transform.position + Vector3.up);
	}

	protected static void InvokeUserCode_CmdHarvestAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestAnimal called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdHarvestAnimal(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdPetAnimal(uint animalToPet)
	{
		RpcCharPetAnimation();
		if (NetworkIdentity.spawned.ContainsKey(animalToPet))
		{
			NetworkIdentity.spawned[animalToPet].GetComponent<FarmAnimal>().checkEffectOnPet(base.netId);
		}
	}

	protected static void InvokeUserCode_CmdPetAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPetAnimal called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPetAnimal(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdReviveOtherChar(uint reviveId)
	{
		RpcReviveDelay(reviveId);
		StartCoroutine(reviveDelayServer(reviveId));
	}

	protected static void InvokeUserCode_CmdReviveOtherChar(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReviveOtherChar called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdReviveOtherChar(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcReviveDelay(uint reviveId)
	{
		StartCoroutine(reviveDelayClient(NetworkIdentity.spawned[reviveId].GetComponent<CharMovement>()));
	}

	protected static void InvokeUserCode_RpcReviveDelay(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReviveDelay called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcReviveDelay(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSitInHairDresserSeat(Vector3 newSittingPos)
	{
		sittingInHairDresserSeat = true;
		RpcSittingLayingOrStanding(1);
		NetworkMapSharer.Instance.NetworkhairDresserSeatOccupied = true;
	}

	protected static void InvokeUserCode_CmdSitInHairDresserSeat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSitInHairDresserSeat called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdSitInHairDresserSeat(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdSitInTuckshopSeat(Vector3 newSittingPos, int seatId)
	{
		sittingInTuckshopChair = seatId;
		RpcSittingLayingOrStanding(1);
		TuckshopManager.manage.sitInSeat(seatId);
	}

	protected static void InvokeUserCode_CmdSitInTuckshopSeat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSitInTuckshopSeat called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdSitInTuckshopSeat(reader.ReadVector3(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGetUpFromHairDresserSeat()
	{
		NetworksittingPos = Vector3.zero;
		sittingInHairDresserSeat = false;
		RpcSittingLayingOrStanding(0);
		NetworkMapSharer.Instance.NetworkhairDresserSeatOccupied = false;
	}

	protected static void InvokeUserCode_CmdGetUpFromHairDresserSeat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetUpFromHairDresserSeat called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGetUpFromHairDresserSeat();
		}
	}

	protected void UserCode_CmdGetUpFromFloor()
	{
		sittingOnFloor = false;
		sitting = false;
		RpcChangeSittinOnFloor(isSittinOnFloor: false);
	}

	protected static void InvokeUserCode_CmdGetUpFromFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetUpFromFloor called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGetUpFromFloor();
		}
	}

	protected void UserCode_CmdSitOnFloor()
	{
		sittingOnFloor = true;
		sitting = true;
		RpcChangeSittinOnFloor(isSittinOnFloor: true);
	}

	protected static void InvokeUserCode_CmdSitOnFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSitOnFloor called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdSitOnFloor();
		}
	}

	protected void UserCode_CmdGetUpFromTuckshopSeat(int gettingUpFrom)
	{
		NetworksittingPos = Vector3.zero;
		sittingInTuckshopChair = -1;
		RpcSittingLayingOrStanding(0);
		TuckshopManager.manage.getUpFromSeat(gettingUpFrom);
	}

	protected static void InvokeUserCode_CmdGetUpFromTuckshopSeat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetUpFromTuckshopSeat called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGetUpFromTuckshopSeat(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSitDown(int seatNo, int xPos, int yPos, Vector3 newSittingPos, int houseX, int houseY)
	{
		sitting = true;
		sittingInSeat = seatNo;
		sittingXpos = xPos;
		sittingYPos = yPos;
		NetworksittingPos = newSittingPos;
		HouseDetails houseDetails = null;
		if (houseX != -1 && houseY != -1)
		{
			houseDetails = HouseManager.manage.getHouseInfo(houseX, houseY);
		}
		if (houseX != -1 && houseY != -1)
		{
			int newSitPosition = houseDetails.houseMapOnTileStatus[xPos, yPos] + seatNo;
			NetworkMapSharer.Instance.RpcSitDown(newSitPosition, xPos, yPos, houseX, houseY);
		}
		else
		{
			int newSitPosition2 = WorldManager.Instance.onTileStatusMap[xPos, yPos] + seatNo;
			NetworkMapSharer.Instance.RpcSitDown(newSitPosition2, xPos, yPos, -1, -1);
		}
		bool flag = false;
		if ((houseDetails == null && WorldManager.Instance.onTileMap[xPos, yPos] > -1 && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].tileObjectFurniture.isSeat) || (houseDetails != null && houseDetails.houseMapOnTile[xPos, yPos] > -1 && WorldManager.Instance.allObjects[houseDetails.houseMapOnTile[xPos, yPos]].tileObjectFurniture.isSeat))
		{
			flag = true;
		}
		if (flag)
		{
			RpcSittingLayingOrStanding(1);
		}
		else
		{
			RpcSittingLayingOrStanding(2);
		}
	}

	protected static void InvokeUserCode_CmdSitDown(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSitDown called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdSitDown(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadVector3(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdConfirmSleep()
	{
		SetReadyToSleep(ready: true);
	}

	protected static void InvokeUserCode_CmdConfirmSleep(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmSleep called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdConfirmSleep();
		}
	}

	protected void UserCode_RpcCharPetAnimation()
	{
		StartCoroutine(playPetAnimation());
	}

	protected static void InvokeUserCode_RpcCharPetAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCharPetAnimation called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcCharPetAnimation();
		}
	}

	protected void UserCode_RpcSittingLayingOrStanding(int stat)
	{
		switch (stat)
		{
		case 0:
			GetComponent<Animator>().SetTrigger("Standing");
			GetComponent<Animator>().SetBool("SittingOrLaying", value: false);
			GetComponent<AnimateCharFace>().stopFaceSleeping();
			break;
		case 1:
			GetComponent<Animator>().SetTrigger("Sitting");
			GetComponent<Animator>().SetBool("SittingOrLaying", value: true);
			GetComponent<AnimateCharFace>().stopFaceSleeping();
			break;
		case 2:
			GetComponent<Animator>().SetTrigger("Laying");
			GetComponent<Animator>().SetBool("SittingOrLaying", value: true);
			GetComponent<AnimateCharFace>().setFaceSleeping();
			break;
		}
		sittingLayingOrStanding = stat;
	}

	protected static void InvokeUserCode_RpcSittingLayingOrStanding(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSittingLayingOrStanding called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcSittingLayingOrStanding(reader.ReadInt());
		}
	}

	protected void UserCode_RpcChangeSittinOnFloor(bool isSittinOnFloor)
	{
		GetComponent<Animator>().SetBool("SittingOnFloor", isSittinOnFloor);
		if (isSittinOnFloor)
		{
			GetComponent<Animator>().SetTrigger("Sitting");
		}
	}

	protected static void InvokeUserCode_RpcChangeSittinOnFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeSittinOnFloor called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcChangeSittinOnFloor(reader.ReadBool());
		}
	}

	protected void UserCode_CmdGetUp(int seatNo, int xPos, int yPos)
	{
		NetworksittingPos = Vector3.zero;
		if (myInteract.InsideHouseDetails != null)
		{
			int sitPosition = Mathf.Clamp(myInteract.InsideHouseDetails.houseMapOnTileStatus[xPos, yPos] - seatNo, 0, 3);
			NetworkMapSharer.Instance.RpcGetUp(sitPosition, xPos, yPos, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos);
		}
		else
		{
			int sitPosition2 = Mathf.Clamp(WorldManager.Instance.onTileStatusMap[xPos, yPos] - seatNo, 0, 3);
			NetworkMapSharer.Instance.RpcGetUp(sitPosition2, xPos, yPos, -1, -1);
		}
		SetReadyToSleep(ready: false);
		RpcSittingLayingOrStanding(0);
		sitting = false;
	}

	protected static void InvokeUserCode_CmdGetUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetUp called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGetUp(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUp(uint pickUpId)
	{
		if (myEquip.myPermissions.CheckIfCanPickUp())
		{
			if (NetworkIdentity.spawned.ContainsKey(pickUpId))
			{
				DroppedItem component = NetworkIdentity.spawned[pickUpId].GetComponent<DroppedItem>();
				if (!component.HasBeenPickedUp())
				{
					TargetAddPickupToInv(base.connectionToClient, component.myItemId, component.stackAmount);
					component.pickUp();
					component.RpcMoveTowardsPickedUpBy(base.netId);
				}
			}
		}
		else
		{
			NetworkMapSharer.Instance.TargetGivePermissionError(base.connectionToClient);
		}
	}

	protected static void InvokeUserCode_CmdPickUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUp called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPickUp(reader.ReadUInt());
		}
	}

	protected void UserCode_TargetAddPickupToInv(NetworkConnection con, int itemId, int stack)
	{
		if (!Inventory.Instance.addItemToInventory(itemId, stack))
		{
			Vector3 position = NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position;
			position.y = NetworkMapSharer.Instance.localChar.transform.position.y;
			Vector3 position2 = NetworkMapSharer.Instance.localChar.transform.position;
			myChar.CmdDropItem(itemId, stack, position2, position);
		}
	}

	protected static void InvokeUserCode_TargetAddPickupToInv(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetAddPickupToInv called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_TargetAddPickupToInv(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdOpenChest(int xPos, int yPos)
	{
		MonoBehaviour.print("Opening a chest");
		if (myEquip.myPermissions.CheckIfCanOpenChest())
		{
			MonoBehaviour.print("Can Open chest so I'm doing it");
			ContainerManager.manage.openChestFromServer(base.connectionToClient, xPos, yPos, myInteract.InsideHouseDetails);
		}
		else
		{
			MonoBehaviour.print("Giving Chest Permission Error");
			NetworkMapSharer.Instance.TargetGivePermissionError(base.connectionToClient);
		}
	}

	protected static void InvokeUserCode_CmdOpenChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenChest called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdOpenChest(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestChestForCrafting(int xPos, int yPos)
	{
		ContainerManager.manage.SyncChestFromServerForCrafting(base.connectionToClient, xPos, yPos, myInteract.InsideHouseDetails);
	}

	protected static void InvokeUserCode_CmdRequestChestForCrafting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestChestForCrafting called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdRequestChestForCrafting(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdOpenVehicleStorage(uint vehicleNetId)
	{
		ContainerManager.manage.openVehicleStorage(base.connectionToClient, vehicleNetId);
	}

	protected static void InvokeUserCode_CmdOpenVehicleStorage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenVehicleStorage called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdOpenVehicleStorage(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdOpenStash(int stashId)
	{
		RpcPlayOpenStashSound();
	}

	protected static void InvokeUserCode_CmdOpenStash(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenStash called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdOpenStash(reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlayOpenStashSound()
	{
	}

	protected static void InvokeUserCode_RpcPlayOpenStashSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOpenStashSound called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcPlayOpenStashSound();
		}
	}

	protected void UserCode_CmdChangeOneInChest(int xPos, int yPos, int slotNo, int newSlotId, int newStackNo)
	{
		ContainerManager.manage.changeSlotInChest(xPos, yPos, slotNo, newSlotId, newStackNo, myInteract.InsideHouseDetails);
	}

	protected static void InvokeUserCode_CmdChangeOneInChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOneInChest called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeOneInChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdTakeItemsFromChestInChest(int itemId, int amountToRemove, int remoteTablePosX, int remoteTablePosY, int houseX, int houseY)
	{
		CraftingManager.manage.RemoveAmountOfItemFromChestsNearby(itemId, amountToRemove, remoteTablePosX, remoteTablePosY, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdTakeItemsFromChestInChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeItemsFromChestInChest called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdTakeItemsFromChestInChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeOneInVehicleStorage(uint vehicleId, int slotNo, int newSlotId, int newStackNo)
	{
		ContainerManager.manage.changeSlotInVehicleStorage(vehicleId, slotNo, newSlotId, newStackNo);
	}

	protected static void InvokeUserCode_CmdChangeOneInVehicleStorage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOneInVehicleStorage called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeOneInVehicleStorage(reader.ReadUInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdStartDriving(uint vehicleToDrive)
	{
		if (!myEquip.myPermissions.CheckIfCanInteractWithVehicles())
		{
			return;
		}
		drivingVehicle = true;
		drivingVehicleId = vehicleToDrive;
		NetworkIdentity component = NetworkIdentity.spawned[vehicleToDrive].GetComponent<NetworkIdentity>();
		if (component.connectionToClient != base.connectionToClient)
		{
			if (component.connectionToClient != null)
			{
				component.RemoveClientAuthority();
			}
			component.AssignClientAuthority(base.connectionToClient);
		}
		component.GetComponent<Vehicle>().startDriving(base.netId);
		drivingVehicleId = vehicleToDrive;
		RpcNetworkTransformOnForVehicle(shouldNetworkTransformSync: false, base.transform.position);
	}

	protected static void InvokeUserCode_CmdStartDriving(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartDriving called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdStartDriving(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdStartPassenger(uint passengerVehicle)
	{
		vehiclePassengerId = passengerVehicle;
		currentPassengerPos = NetworkIdentity.spawned[passengerVehicle].GetComponent<VehicleSecondSeat>();
		currentPassengerPos.NetworkpassengerId = base.netId;
		RpcNetworkTransformOnForVehicle(shouldNetworkTransformSync: false, base.transform.position);
	}

	protected static void InvokeUserCode_CmdStartPassenger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartPassenger called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdStartPassenger(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdStopPassenger(uint passengerVehicle)
	{
		NetworkIdentity.spawned[passengerVehicle].GetComponent<VehicleSecondSeat>().NetworkpassengerId = 0u;
		currentPassengerPos = null;
		vehiclePassengerId = 0u;
		RpcNetworkTransformOnForVehicle(shouldNetworkTransformSync: true, base.transform.position);
	}

	protected static void InvokeUserCode_CmdStopPassenger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopPassenger called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdStopPassenger(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcNetworkTransformOnForVehicle(bool shouldNetworkTransformSync, Vector3 position)
	{
		if (shouldNetworkTransformSync)
		{
			myChar.normalNetworkTransform.ResetObjectsInterpolation();
		}
		myChar.normalNetworkTransform.syncPosition = shouldNetworkTransformSync;
		myChar.normalNetworkTransform.syncRotation = shouldNetworkTransformSync;
		myChar.transform.position = position;
	}

	protected static void InvokeUserCode_RpcNetworkTransformOnForVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNetworkTransformOnForVehicle called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcNetworkTransformOnForVehicle(reader.ReadBool(), reader.ReadVector3());
		}
	}

	protected void UserCode_CmdStopDriving(uint vehicleToDrive)
	{
		drivingVehicle = false;
		drivingVehicleId = 0u;
		NetworkIdentity.spawned[vehicleToDrive].GetComponent<Vehicle>().StopDriving();
		RpcNetworkTransformOnForVehicle(shouldNetworkTransformSync: true, base.transform.position);
	}

	protected static void InvokeUserCode_CmdStopDriving(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopDriving called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdStopDriving(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcDropCarriedItem()
	{
		if (base.isLocalPlayer)
		{
			myEquip.setCarrying(newCarrying: false);
		}
		NetworkcarryingObjectNetId = 0u;
	}

	protected static void InvokeUserCode_RpcDropCarriedItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDropCarriedItem called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcDropCarriedItem();
		}
	}

	protected void UserCode_CmdPickUpObject(uint pickedUpObjectNetId)
	{
		NetworkcarryingObjectNetId = pickedUpObjectNetId;
		PickUpAndCarry component = NetworkIdentity.spawned[pickedUpObjectNetId].GetComponent<PickUpAndCarry>();
		NetworkIdentity.spawned[base.netId].GetComponent<CharMovement>();
		component.SetCarriedByPlayerNetId(base.netId);
		component.ChangeAuthority(base.connectionToClient);
	}

	protected static void InvokeUserCode_CmdPickUpObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObject called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPickUpObject(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdPutDownObject(float heightDroppedAt)
	{
		if (NetworkIdentity.spawned.ContainsKey(carryingObjectNetId))
		{
			NetworkIdentity.spawned[carryingObjectNetId].GetComponent<PickUpAndCarry>().dropAndPlaceAtPos(heightDroppedAt);
		}
		NetworkcarryingObjectNetId = 0u;
	}

	protected static void InvokeUserCode_CmdPutDownObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPutDownObject called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPutDownObject(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdDropObject()
	{
		NetworkIdentity.spawned[carryingObjectNetId].GetComponent<PickUpAndCarry>().dropAndPlaceAtPos(WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)]);
		NetworkIdentity.spawned[carryingObjectNetId].GetComponent<PickUpAndCarry>().StopAllCoroutines();
		NetworkIdentity.spawned[carryingObjectNetId].transform.position = base.transform.position;
		NetworkcarryingObjectNetId = 0u;
	}

	protected static void InvokeUserCode_CmdDropObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropObject called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdDropObject();
		}
	}

	protected void UserCode_CmdPutDownObjectInDropPoint(Vector3 dropPos)
	{
		NetworkIdentity.spawned[carryingObjectNetId].GetComponent<PickUpAndCarry>().DropAndPlaceAtDropPos(dropPos);
		NetworkcarryingObjectNetId = 0u;
	}

	protected static void InvokeUserCode_CmdPutDownObjectInDropPoint(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPutDownObjectInDropPoint called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPutDownObjectInDropPoint(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdDropAndRelase(float heightDroppedAt)
	{
		PickUpAndCarry component = NetworkIdentity.spawned[carryingObjectNetId].GetComponent<PickUpAndCarry>();
		component.dropAndPlaceAtPos(heightDroppedAt);
		component.NetworkcanBePickedUp = false;
		if ((bool)NetworkIdentity.spawned[carryingObjectNetId].GetComponent<TrappedAnimal>())
		{
			NetworkIdentity.spawned[carryingObjectNetId].GetComponent<TrappedAnimal>().OpenOnServerWhenOnFloor();
		}
		if ((bool)NetworkIdentity.spawned[carryingObjectNetId].GetComponent<AnimalCarryBox>())
		{
			NetworkIdentity.spawned[carryingObjectNetId].GetComponent<AnimalCarryBox>().OpenOnServerWhenOnFloor();
		}
		NetworkcarryingObjectNetId = 0u;
	}

	protected static void InvokeUserCode_CmdDropAndRelase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropAndRelase called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdDropAndRelase(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdChangeStatus(int xPos, int yPos, int newStatus)
	{
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(newStatus, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdChangeStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeStatus called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeStatus(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceOntoAnimal(int itemPlacingOn, uint animalNetId)
	{
		PlaceOnAnimal[] componentsInChildren = Inventory.Instance.allItems[myEquip.currentlyHoldingItemId].GetComponentsInChildren<PlaceOnAnimal>();
		PlaceOnAnimal placeOnAnimal = componentsInChildren[0];
		AnimalAI component = NetworkIdentity.spawned[animalNetId].GetComponent<AnimalAI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].toPlaceOn.animalId == component.animalId)
			{
				placeOnAnimal = componentsInChildren[i];
				break;
			}
		}
		NetworkNavMesh.nav.UnSpawnAnAnimal(component, saveToMap: false);
		if ((bool)placeOnAnimal.becomePet)
		{
			if (placeOnAnimal.specialVariation == -1)
			{
				FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(placeOnAnimal.becomePet.animalId, component.getVariationNo(), placeOnAnimal.defaultName, component.transform.position);
			}
			else
			{
				FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(placeOnAnimal.becomePet.animalId, placeOnAnimal.specialVariation, placeOnAnimal.defaultName, component.transform.position);
			}
		}
		_ = (bool)placeOnAnimal.becomeVehicle;
	}

	protected static void InvokeUserCode_CmdPlaceOntoAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceOntoAnimal called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPlaceOntoAnimal(reader.ReadInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_CmdFillBulletinBoard()
	{
		NetworkMapSharer.Instance.getBulletinBoardAndSend(base.connectionToClient);
	}

	protected static void InvokeUserCode_CmdFillBulletinBoard(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFillBulletinBoard called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdFillBulletinBoard();
		}
	}

	protected void UserCode_CmdAddToBarrow(uint barrowId, int tileTypeToAdd)
	{
		NetworkIdentity.spawned[barrowId].GetComponent<Wheelbarrow>().insertDirt(tileTypeToAdd);
	}

	protected static void InvokeUserCode_CmdAddToBarrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddToBarrow called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdAddToBarrow(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRemoveFromBarrow(uint barrowId)
	{
		NetworkIdentity.spawned[barrowId].GetComponent<Wheelbarrow>().removeDirt();
	}

	protected static void InvokeUserCode_CmdRemoveFromBarrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveFromBarrow called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdRemoveFromBarrow(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdPaintVehicle(uint vehicleId, int colourId)
	{
		NetworkIdentity.spawned[vehicleId].GetComponent<Vehicle>().setVariation(colourId);
		RpcPaintVehicle(vehicleId, colourId);
	}

	protected static void InvokeUserCode_CmdPaintVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPaintVehicle called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPaintVehicle(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPaintVehicle(uint vehicle, int colourId)
	{
		Vehicle component = NetworkIdentity.spawned[vehicle].GetComponent<Vehicle>();
		ParticleManager.manage.paintVehicle.GetComponent<ParticleSystemRenderer>().sharedMaterial = EquipWindow.equip.vehicleColours[colourId];
		SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.paintSound, component.transform.position);
		if (component.meshToChangeColours.Length != 0)
		{
			for (int i = 0; i < component.meshToChangeColours.Length; i++)
			{
				if (component.meshToChangeColours[i].gameObject.activeInHierarchy)
				{
					ParticleSystem.ShapeModule shape = ParticleManager.manage.paintVehicle.shape;
					shape.enabled = true;
					shape.shapeType = ParticleSystemShapeType.Mesh;
					shape.mesh = component.meshToChangeColours[i].GetComponent<MeshFilter>().mesh;
					shape.scale = Vector3.one;
					ParticleManager.manage.paintVehicle.transform.position = component.meshToChangeColours[i].transform.position;
					ParticleManager.manage.paintVehicle.transform.rotation = Quaternion.Euler(-90f, component.meshToChangeColours[i].transform.rotation.eulerAngles.y, 0f);
					ParticleManager.manage.paintVehicle.Emit(50);
				}
			}
		}
		else
		{
			if (component.meshRenderersToTintColours.Length == 0)
			{
				return;
			}
			for (int j = 0; j < component.meshRenderersToTintColours.Length; j++)
			{
				if (component.meshRenderersToTintColours[j].gameObject.activeInHierarchy)
				{
					ParticleSystem.ShapeModule shape2 = ParticleManager.manage.paintVehicle.shape;
					shape2.enabled = true;
					shape2.shapeType = ParticleSystemShapeType.Mesh;
					shape2.mesh = component.meshRenderersToTintColours[j].GetComponent<MeshFilter>().mesh;
					shape2.scale = Vector3.one;
					ParticleManager.manage.paintVehicle.transform.position = component.meshRenderersToTintColours[j].transform.position;
					ParticleManager.manage.paintVehicle.transform.rotation = Quaternion.Euler(-90f, component.meshRenderersToTintColours[j].transform.rotation.eulerAngles.y, 0f);
					ParticleManager.manage.paintVehicle.Emit(50);
				}
			}
		}
	}

	protected static void InvokeUserCode_RpcPaintVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPaintVehicle called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcPaintVehicle(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileVariation(int newColour, int xPos, int yPos)
	{
		if (myInteract.InsideHouseDetails != null)
		{
			int tileObjectId = WorldManager.Instance.allObjectSettings[myInteract.InsideHouseDetails.houseMapOnTile[xPos, yPos]].GetComponent<ItemPaintVarient>().varients[newColour].tileObjectId;
			NetworkMapSharer.Instance.RpcChangeTileObjectToColourVarient(tileObjectId, newColour, xPos, yPos, myInteract.InsideHouseDetails.xPos, myInteract.InsideHouseDetails.yPos);
		}
		else
		{
			int tileObjectId2 = WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].GetComponent<ItemPaintVarient>().varients[newColour].tileObjectId;
			NetworkMapSharer.Instance.RpcChangeTileObjectToColourVarient(tileObjectId2, newColour, xPos, yPos, -1, -1);
		}
	}

	protected static void InvokeUserCode_CmdChangeTileVariation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileVariation called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeTileVariation(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGoDownMines()
	{
		NetworkMapSharer.Instance.RpcMoveUnderGround();
	}

	protected static void InvokeUserCode_CmdGoDownMines(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoDownMines called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGoDownMines();
		}
	}

	protected void UserCode_CmdTakeAirShipOffIsland()
	{
		NetworkMapSharer.Instance.RpcMoveOffIsland();
	}

	protected static void InvokeUserCode_CmdTakeAirShipOffIsland(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeAirShipOffIsland called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdTakeAirShipOffIsland();
		}
	}

	protected void UserCode_CmdGoUpMines()
	{
		NetworkMapSharer.Instance.RpcMoveAboveGround();
	}

	protected static void InvokeUserCode_CmdGoUpMines(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoUpMines called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGoUpMines();
		}
	}

	protected void UserCode_CmdTakeAirShipHome()
	{
		NetworkMapSharer.Instance.RpcReturnHomeFromOffIsland();
	}

	protected static void InvokeUserCode_CmdTakeAirShipHome(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeAirShipHome called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdTakeAirShipHome();
		}
	}

	protected void UserCode_CmdEditSignDetails(int xPos, int yPos, int houseX, int houseY, string message)
	{
		RpcUpdateSignDetails(xPos, yPos, houseX, houseY, message);
	}

	protected static void InvokeUserCode_CmdEditSignDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEditSignDetails called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdEditSignDetails(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadString());
		}
	}

	protected void UserCode_RpcUpdateSignDetails(int xPos, int yPos, int houseX, int houseY, string message)
	{
		SignManager.manage.changeSignDetails(xPos, yPos, houseX, houseY, message);
	}

	protected static void InvokeUserCode_RpcUpdateSignDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateSignDetails called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcUpdateSignDetails(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadString());
		}
	}

	static CharPickUp()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeSignItem", InvokeUserCode_CmdChangeSignItem, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdAddToSilo", InvokeUserCode_CmdAddToSilo, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdHarvestAnimalToInv", InvokeUserCode_CmdHarvestAnimalToInv, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdHarvestAnimal", InvokeUserCode_CmdHarvestAnimal, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPetAnimal", InvokeUserCode_CmdPetAnimal, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdReviveOtherChar", InvokeUserCode_CmdReviveOtherChar, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdSitInHairDresserSeat", InvokeUserCode_CmdSitInHairDresserSeat, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdSitInTuckshopSeat", InvokeUserCode_CmdSitInTuckshopSeat, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGetUpFromHairDresserSeat", InvokeUserCode_CmdGetUpFromHairDresserSeat, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGetUpFromFloor", InvokeUserCode_CmdGetUpFromFloor, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdSitOnFloor", InvokeUserCode_CmdSitOnFloor, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGetUpFromTuckshopSeat", InvokeUserCode_CmdGetUpFromTuckshopSeat, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdSitDown", InvokeUserCode_CmdSitDown, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdConfirmSleep", InvokeUserCode_CmdConfirmSleep, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGetUp", InvokeUserCode_CmdGetUp, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPickUp", InvokeUserCode_CmdPickUp, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdOpenChest", InvokeUserCode_CmdOpenChest, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdRequestChestForCrafting", InvokeUserCode_CmdRequestChestForCrafting, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdOpenVehicleStorage", InvokeUserCode_CmdOpenVehicleStorage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdOpenStash", InvokeUserCode_CmdOpenStash, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeOneInChest", InvokeUserCode_CmdChangeOneInChest, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdTakeItemsFromChestInChest", InvokeUserCode_CmdTakeItemsFromChestInChest, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeOneInVehicleStorage", InvokeUserCode_CmdChangeOneInVehicleStorage, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdStartDriving", InvokeUserCode_CmdStartDriving, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdStartPassenger", InvokeUserCode_CmdStartPassenger, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdStopPassenger", InvokeUserCode_CmdStopPassenger, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdStopDriving", InvokeUserCode_CmdStopDriving, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPickUpObject", InvokeUserCode_CmdPickUpObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPutDownObject", InvokeUserCode_CmdPutDownObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdDropObject", InvokeUserCode_CmdDropObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPutDownObjectInDropPoint", InvokeUserCode_CmdPutDownObjectInDropPoint, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdDropAndRelase", InvokeUserCode_CmdDropAndRelase, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeStatus", InvokeUserCode_CmdChangeStatus, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPlaceOntoAnimal", InvokeUserCode_CmdPlaceOntoAnimal, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdFillBulletinBoard", InvokeUserCode_CmdFillBulletinBoard, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdAddToBarrow", InvokeUserCode_CmdAddToBarrow, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdRemoveFromBarrow", InvokeUserCode_CmdRemoveFromBarrow, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPaintVehicle", InvokeUserCode_CmdPaintVehicle, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeTileVariation", InvokeUserCode_CmdChangeTileVariation, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGoDownMines", InvokeUserCode_CmdGoDownMines, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdTakeAirShipOffIsland", InvokeUserCode_CmdTakeAirShipOffIsland, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGoUpMines", InvokeUserCode_CmdGoUpMines, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdTakeAirShipHome", InvokeUserCode_CmdTakeAirShipHome, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdEditSignDetails", InvokeUserCode_CmdEditSignDetails, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcStopDrivingFromServer", InvokeUserCode_RpcStopDrivingFromServer);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcStopPassengerFromServer", InvokeUserCode_RpcStopPassengerFromServer);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcReviveDelay", InvokeUserCode_RpcReviveDelay);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcCharPetAnimation", InvokeUserCode_RpcCharPetAnimation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcSittingLayingOrStanding", InvokeUserCode_RpcSittingLayingOrStanding);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcChangeSittinOnFloor", InvokeUserCode_RpcChangeSittinOnFloor);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcPlayOpenStashSound", InvokeUserCode_RpcPlayOpenStashSound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcNetworkTransformOnForVehicle", InvokeUserCode_RpcNetworkTransformOnForVehicle);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcDropCarriedItem", InvokeUserCode_RpcDropCarriedItem);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcPaintVehicle", InvokeUserCode_RpcPaintVehicle);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcUpdateSignDetails", InvokeUserCode_RpcUpdateSignDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "TargetAddPickupToInv", InvokeUserCode_TargetAddPickupToInv);
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(carryingObjectNetId);
			writer.WriteVector3(sittingPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(carryingObjectNetId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteVector3(sittingPos);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = carryingObjectNetId;
			NetworkcarryingObjectNetId = reader.ReadUInt();
			if (!SyncVarEqual(num, ref carryingObjectNetId))
			{
				onCarryChanged(num, carryingObjectNetId);
			}
			Vector3 vector = sittingPos;
			NetworksittingPos = reader.ReadVector3();
			if (!SyncVarEqual(vector, ref sittingPos))
			{
				onSittingChanged(vector, sittingPos);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			uint num3 = carryingObjectNetId;
			NetworkcarryingObjectNetId = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref carryingObjectNetId))
			{
				onCarryChanged(num3, carryingObjectNetId);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			Vector3 vector2 = sittingPos;
			NetworksittingPos = reader.ReadVector3();
			if (!SyncVarEqual(vector2, ref sittingPos))
			{
				onSittingChanged(vector2, sittingPos);
			}
		}
	}
}
