using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityChan;
using UnityEngine;

public class EquipItemToChar : NetworkBehaviour
{
	public InventoryItem itemCurrentlyHolding;

	private Animator myAnim;

	public Animator itemHolderAnim;

	public GameObject holdingPrefab;

	public Animator holdingPrefabAnimator;

	[SyncVar(hook = "equipNewItemNetwork")]
	public int currentlyHoldingItemId = -1;

	private Transform leftHandPos;

	private Transform rightHandPos;

	public float lookingWeight;

	public Transform lookable;

	public Transform holdPos;

	public Transform rightHandToolHitPos;

	public Transform rightHandHoldPos;

	public SkinnedMeshRenderer skinRen;

	public SkinnedMeshRenderer shirtRen;

	public SkinnedMeshRenderer pantsRen;

	public SkinnedMeshRenderer shoeRen;

	public SkinnedMeshRenderer dressRen;

	public SkinnedMeshRenderer skirtRen;

	public SkinnedMeshRenderer longDressRen;

	public MeshRenderer noseRen;

	public Transform onHeadPosition;

	public EyesScript eyes;

	public InventoryItem dogWhistleItem;

	private GameObject itemOnHead;

	private GameObject hairOnHead;

	private GameObject itemOnFace;

	public NameTag myNameTag;

	[SyncVar(hook = "onChangeName")]
	public string playerName = "";

	[SyncVar]
	public bool usingItem;

	[SyncVar]
	public bool blocking;

	[SyncVar(hook = "onHairColourChange")]
	public int hairColor;

	[SyncVar(hook = "onFaceChange")]
	public int faceId = -1;

	[SyncVar(hook = "onHeadChange")]
	public int headId = -1;

	[SyncVar(hook = "onHairChange")]
	public int hairId = -1;

	[SyncVar(hook = "onChangeShirt")]
	public int shirtId = -1;

	[SyncVar(hook = "onChangePants")]
	public int pantsId = -1;

	[SyncVar(hook = "onChangeShoes")]
	public int shoeId = -1;

	[SyncVar(hook = "onChangeEyes")]
	public int eyeId;

	[SyncVar(hook = "onChangeEyeColor")]
	public int eyeColor;

	[SyncVar(hook = "onChangeSkin")]
	public int skinId = 1;

	[SyncVar(hook = "onNoseChange")]
	public int noseId = 1;

	[SyncVar(hook = "onMouthChange")]
	public int mouthId = 1;

	[SyncVar(hook = "changeOpenBag")]
	public bool bagOpenEmoteOn;

	[SyncVar(hook = "ChangeHasBag")]
	public bool hasBag;

	[SyncVar(hook = "OnChangeBagColour")]
	public int bagColour;

	[SyncVar(hook = "OnDisguiseChange")]
	public int disguiseId = -1;

	private bool localShowingBag;

	private int localBagColour;

	[SyncVar]
	public float compScore;

	[SyncVar(hook = "OnSizeChange")]
	public Vector3 size = Vector3.one;

	public float localCompScore;

	private bool swimming;

	private bool doingEmote;

	private bool carrying;

	private bool driving;

	private bool petting;

	private bool whistling;

	private bool lookingAtMap;

	private bool lookingAtJournal;

	private bool crafting;

	private bool cooking;

	private bool layingDown;

	private bool climbing;

	public GameObject carryingingOverHeadObject;

	public GameObject holdingMapPrefab;

	public GameObject craftingHammer;

	public GameObject cookingPan;

	public GameObject packOnBack;

	private GameObject disguiseTileObject;

	public ConversationObject confirmDeedConvoSO;

	public ConversationObject confirmDeedNotOnIsland;

	public ConversationObject confirmDeedNotServerSO;

	public bool nameHasBeenUpdated;

	public TileHighlighter highlighter;

	private int holdingToolAnimation;

	private int usingAnimation;

	private int usingStanceAnimation;

	private CharMovement myChar;

	public Transform baseTransform;

	public Transform headTransform;

	private int islandId;

	public Permissions myPermissions;

	public bool lookLock;

	private float leftHandWeight = 1f;

	private Vehicle inVehicle;

	private Transform leftFoot;

	private Transform rightFoot;

	private static Vector3 dif;

	private ToolDoesDamage useTool;

	private MeleeAttacks toolWeapon;

	private Coroutine doingEmotion;

	private bool fishInHandPlaying;

	private bool bugInHandPlaying;

	private Vector3 aimLookablePos;

	private bool IsInsidePlayerHouse { get; set; }

	private bool IsInsideBuilding { get; set; }

	public bool IsCurrentlyHoldingItem => (object)itemCurrentlyHolding != null;

	public int NetworkcurrentlyHoldingItemId
	{
		get
		{
			return currentlyHoldingItemId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyHoldingItemId))
			{
				int oldItem = currentlyHoldingItemId;
				SetSyncVar(value, ref currentlyHoldingItemId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					equipNewItemNetwork(oldItem, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public string NetworkplayerName
	{
		get
		{
			return playerName;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref playerName))
			{
				string oldName = playerName;
				SetSyncVar(value, ref playerName, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					onChangeName(oldName, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public bool NetworkusingItem
	{
		get
		{
			return usingItem;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref usingItem))
			{
				bool flag = usingItem;
				SetSyncVar(value, ref usingItem, 4uL);
			}
		}
	}

	public bool Networkblocking
	{
		get
		{
			return blocking;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref blocking))
			{
				bool flag = blocking;
				SetSyncVar(value, ref blocking, 8uL);
			}
		}
	}

	public int NetworkhairColor
	{
		get
		{
			return hairColor;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hairColor))
			{
				int oldColour = hairColor;
				SetSyncVar(value, ref hairColor, 16uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(16uL))
				{
					setSyncVarHookGuard(16uL, value: true);
					onHairColourChange(oldColour, value);
					setSyncVarHookGuard(16uL, value: false);
				}
			}
		}
	}

	public int NetworkfaceId
	{
		get
		{
			return faceId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref faceId))
			{
				int oldId = faceId;
				SetSyncVar(value, ref faceId, 32uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(32uL))
				{
					setSyncVarHookGuard(32uL, value: true);
					onFaceChange(oldId, value);
					setSyncVarHookGuard(32uL, value: false);
				}
			}
		}
	}

	public int NetworkheadId
	{
		get
		{
			return headId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref headId))
			{
				int oldId = headId;
				SetSyncVar(value, ref headId, 64uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(64uL))
				{
					setSyncVarHookGuard(64uL, value: true);
					onHeadChange(oldId, value);
					setSyncVarHookGuard(64uL, value: false);
				}
			}
		}
	}

	public int NetworkhairId
	{
		get
		{
			return hairId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hairId))
			{
				int oldId = hairId;
				SetSyncVar(value, ref hairId, 128uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(128uL))
				{
					setSyncVarHookGuard(128uL, value: true);
					onHairChange(oldId, value);
					setSyncVarHookGuard(128uL, value: false);
				}
			}
		}
	}

	public int NetworkshirtId
	{
		get
		{
			return shirtId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref shirtId))
			{
				int oldId = shirtId;
				SetSyncVar(value, ref shirtId, 256uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(256uL))
				{
					setSyncVarHookGuard(256uL, value: true);
					onChangeShirt(oldId, value);
					setSyncVarHookGuard(256uL, value: false);
				}
			}
		}
	}

	public int NetworkpantsId
	{
		get
		{
			return pantsId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref pantsId))
			{
				int oldId = pantsId;
				SetSyncVar(value, ref pantsId, 512uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(512uL))
				{
					setSyncVarHookGuard(512uL, value: true);
					onChangePants(oldId, value);
					setSyncVarHookGuard(512uL, value: false);
				}
			}
		}
	}

	public int NetworkshoeId
	{
		get
		{
			return shoeId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref shoeId))
			{
				int oldId = shoeId;
				SetSyncVar(value, ref shoeId, 1024uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1024uL))
				{
					setSyncVarHookGuard(1024uL, value: true);
					onChangeShoes(oldId, value);
					setSyncVarHookGuard(1024uL, value: false);
				}
			}
		}
	}

	public int NetworkeyeId
	{
		get
		{
			return eyeId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref eyeId))
			{
				int oldId = eyeId;
				SetSyncVar(value, ref eyeId, 2048uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2048uL))
				{
					setSyncVarHookGuard(2048uL, value: true);
					onChangeEyes(oldId, value);
					setSyncVarHookGuard(2048uL, value: false);
				}
			}
		}
	}

	public int NetworkeyeColor
	{
		get
		{
			return eyeColor;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref eyeColor))
			{
				int oldId = eyeColor;
				SetSyncVar(value, ref eyeColor, 4096uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4096uL))
				{
					setSyncVarHookGuard(4096uL, value: true);
					onChangeEyeColor(oldId, value);
					setSyncVarHookGuard(4096uL, value: false);
				}
			}
		}
	}

	public int NetworkskinId
	{
		get
		{
			return skinId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref skinId))
			{
				int oldSkin = skinId;
				SetSyncVar(value, ref skinId, 8192uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8192uL))
				{
					setSyncVarHookGuard(8192uL, value: true);
					onChangeSkin(oldSkin, value);
					setSyncVarHookGuard(8192uL, value: false);
				}
			}
		}
	}

	public int NetworknoseId
	{
		get
		{
			return noseId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref noseId))
			{
				int oldNose = noseId;
				SetSyncVar(value, ref noseId, 16384uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(16384uL))
				{
					setSyncVarHookGuard(16384uL, value: true);
					onNoseChange(oldNose, value);
					setSyncVarHookGuard(16384uL, value: false);
				}
			}
		}
	}

	public int NetworkmouthId
	{
		get
		{
			return mouthId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref mouthId))
			{
				int oldMouth = mouthId;
				SetSyncVar(value, ref mouthId, 32768uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(32768uL))
				{
					setSyncVarHookGuard(32768uL, value: true);
					onMouthChange(oldMouth, value);
					setSyncVarHookGuard(32768uL, value: false);
				}
			}
		}
	}

	public bool NetworkbagOpenEmoteOn
	{
		get
		{
			return bagOpenEmoteOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bagOpenEmoteOn))
			{
				bool oldBagOpen = bagOpenEmoteOn;
				SetSyncVar(value, ref bagOpenEmoteOn, 65536uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(65536uL))
				{
					setSyncVarHookGuard(65536uL, value: true);
					changeOpenBag(oldBagOpen, value);
					setSyncVarHookGuard(65536uL, value: false);
				}
			}
		}
	}

	public bool NetworkhasBag
	{
		get
		{
			return hasBag;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hasBag))
			{
				bool oldHasBag = hasBag;
				SetSyncVar(value, ref hasBag, 131072uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(131072uL))
				{
					setSyncVarHookGuard(131072uL, value: true);
					ChangeHasBag(oldHasBag, value);
					setSyncVarHookGuard(131072uL, value: false);
				}
			}
		}
	}

	public int NetworkbagColour
	{
		get
		{
			return bagColour;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bagColour))
			{
				int oldColour = bagColour;
				SetSyncVar(value, ref bagColour, 262144uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(262144uL))
				{
					setSyncVarHookGuard(262144uL, value: true);
					OnChangeBagColour(oldColour, value);
					setSyncVarHookGuard(262144uL, value: false);
				}
			}
		}
	}

	public int NetworkdisguiseId
	{
		get
		{
			return disguiseId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref disguiseId))
			{
				int oldDisguise = disguiseId;
				SetSyncVar(value, ref disguiseId, 524288uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(524288uL))
				{
					setSyncVarHookGuard(524288uL, value: true);
					OnDisguiseChange(oldDisguise, value);
					setSyncVarHookGuard(524288uL, value: false);
				}
			}
		}
	}

	public float NetworkcompScore
	{
		get
		{
			return compScore;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref compScore))
			{
				float num = compScore;
				SetSyncVar(value, ref compScore, 1048576uL);
			}
		}
	}

	public Vector3 Networksize
	{
		get
		{
			return size;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref size))
			{
				Vector3 oldSize = size;
				SetSyncVar(value, ref size, 2097152uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2097152uL))
				{
					setSyncVarHookGuard(2097152uL, value: true);
					OnSizeChange(oldSize, value);
					setSyncVarHookGuard(2097152uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
		myChar = GetComponent<CharMovement>();
	}

	private void Awake()
	{
		myAnim = GetComponent<Animator>();
		holdingToolAnimation = Animator.StringToHash("HoldingTool");
		usingAnimation = Animator.StringToHash("Using");
		usingStanceAnimation = Animator.StringToHash("UsingStance");
	}

	public override void OnStartLocalPlayer()
	{
		Inventory.Instance.localChar = this;
		CmdChangeSkin(Inventory.Instance.skinTone);
		CmdChangeHairId(Inventory.Instance.playerHair);
		CmdSendName(Inventory.Instance.playerName, SaveLoad.saveOrLoad.newFileSaver.ReadIslandSeedFromSaveFile());
		CmdSendEquipedClothes(EquipWindow.equip.getEquipSlotsArray());
		CmdChangeHairColour(Inventory.Instance.playerHairColour);
		CmdChangeEyes(Inventory.Instance.playerEyes, Inventory.Instance.playerEyeColor);
		CmdChangeFaceId(EquipWindow.equip.faceSlot.itemNo);
		CmdChangeNose(Inventory.Instance.nose);
		CmdChangeMouth(Inventory.Instance.mouth);
		Inventory.Instance.equipNewSelectedSlot();
	}

	public override void OnStartServer()
	{
		StartCoroutine(nameDelay());
	}

	private IEnumerator nameDelay()
	{
		while (!nameHasBeenUpdated)
		{
			yield return null;
		}
		if (!base.isLocalPlayer)
		{
			RpcCharacterJoinedPopup(playerName, NetworkMapSharer.Instance.islandName);
		}
	}

	[ClientRpc]
	private void RpcCharacterJoinedPopup(string newName, string sendIslandName)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(newName);
		writer.WriteString(sendIslandName);
		SendRPCInternal(typeof(EquipItemToChar), "RpcCharacterJoinedPopup", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void setNameTagOnOff()
	{
		if (!base.isLocalPlayer)
		{
			if (disguiseId != -1)
			{
				myNameTag.turnOff();
			}
			else if (OptionsMenu.options.nameTagsOn)
			{
				myNameTag.turnOn(playerName);
			}
			else
			{
				myNameTag.turnOff();
			}
		}
	}

	public override void OnStartClient()
	{
		equipNewItemNetwork(currentlyHoldingItemId, currentlyHoldingItemId);
		onChangeShirt(shirtId, shirtId);
		onChangePants(pantsId, pantsId);
		onChangeShoes(shoeId, shoeId);
		onHeadChange(headId, headId);
		onChangeEyes(eyeId, eyeId);
		onChangeEyeColor(eyeColor, eyeColor);
		onFaceChange(faceId, faceId);
		onHairChange(hairId, hairId);
		onChangeSkin(skinId, skinId);
		onNoseChange(noseId, noseId);
		onMouthChange(mouthId, mouthId);
		OnSizeChange(size, size);
		OnDisguiseChange(disguiseId, disguiseId);
		OptionsMenu.options.nameTagSwitch.AddListener(setNameTagOnOff);
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			animateOnUse(usingItem, blocking);
		}
	}

	public void removeLeftHand()
	{
		leftHandWeight = 0f;
		leftHandPos = null;
	}

	public void attachLeftHand()
	{
		Transform transform = holdingPrefab.transform.Find("Animation/LeftHandle");
		if ((bool)transform)
		{
			leftHandPos = transform;
			leftHandWeight = 1f;
		}
		else
		{
			leftHandPos = null;
			leftHandWeight = 0f;
		}
	}

	public void onChangeName(string oldName, string newName)
	{
		nameHasBeenUpdated = true;
		NetworkplayerName = newName;
		if (!base.isLocalPlayer)
		{
			RenderMap.Instance.changeMapIconName(base.transform, newName);
			setNameTagOnOff();
		}
	}

	private void OnDisable()
	{
		NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("PlayerLeft"), playerName));
	}

	public void setLookLock(bool isLocked)
	{
		lookLock = isLocked;
	}

	public void animateOnUse(bool beingUsed, bool blocking)
	{
		if ((bool)inVehicle)
		{
			return;
		}
		if ((bool)holdingPrefabAnimator || (currentlyHoldingItemId < -1 && currentlyHoldingItemId != -2))
		{
			holdingPrefabAnimator.SetBool(usingAnimation, beingUsed);
			if (beingUsed || lookLock)
			{
				if ((bool)itemCurrentlyHolding && itemCurrentlyHolding.hasUseAnimationStance && !itemCurrentlyHolding.consumeable)
				{
					myAnim.SetBool(usingStanceAnimation, value: true);
				}
				else
				{
					myAnim.SetBool(usingStanceAnimation, value: false);
				}
				float b = 0f;
				if ((bool)lookable && currentlyHoldingItemId < -1)
				{
					b = 0.05f;
				}
				else if ((bool)lookable && (bool)itemCurrentlyHolding)
				{
					b = ((!itemCurrentlyHolding.placeable) ? 0.05f : 0.05f);
				}
				lookingWeight = Mathf.Lerp(lookingWeight, b, Time.deltaTime * 10f);
			}
			else
			{
				lookingWeight = Mathf.Lerp(lookingWeight, 0f, Time.deltaTime * 8f);
				myAnim.SetBool(usingStanceAnimation, value: false);
			}
		}
		else if ((bool)holdingPrefab)
		{
			myAnim.SetBool(usingAnimation, beingUsed);
			if (lookLock)
			{
				if (itemCurrentlyHolding.hasUseAnimationStance && !itemCurrentlyHolding.consumeable)
				{
					myAnim.SetBool(usingStanceAnimation, value: true);
				}
				else
				{
					myAnim.SetBool(usingStanceAnimation, value: false);
				}
			}
			else if (itemCurrentlyHolding.hasUseAnimationStance && !itemCurrentlyHolding.consumeable)
			{
				myAnim.SetBool(usingStanceAnimation, beingUsed);
			}
			else
			{
				myAnim.SetBool(usingStanceAnimation, value: false);
			}
		}
		else
		{
			lookingWeight = Mathf.Lerp(lookingWeight, 0f, Time.deltaTime * 10f);
			myAnim.SetBool(usingStanceAnimation, value: false);
		}
	}

	public void equipNewItem(int inventoryItemNo)
	{
		if (base.isLocalPlayer && !carrying)
		{
			myAnim.SetBool("CarryingItem", value: false);
		}
		Inventory.Instance.checkQuickSlotDesc();
		setLeftHandWeight(1f);
		if (base.isLocalPlayer && IsInsideBuilding)
		{
			if (IsInsidePlayerHouse)
			{
				if ((inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].isFurniture) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].itemChange) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].consumeable) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].canBePlacedInHouse) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].fish) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].bug) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.shirt) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.pants) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.hat) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.face) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.shoes) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.flooring) && (inventoryItemNo <= -1 || !Inventory.Instance.allItems[inventoryItemNo].equipable || !Inventory.Instance.allItems[inventoryItemNo].equipable.wallpaper))
				{
					inventoryItemNo = -1;
				}
			}
			else if (inventoryItemNo > -1 && !Inventory.Instance.allItems[inventoryItemNo].canBeUsedInShops)
			{
				inventoryItemNo = -1;
			}
		}
		if (((base.isLocalPlayer && swimming) || (base.isLocalPlayer && doingEmote)) && (doingEmote || (swimming && inventoryItemNo > -1 && !Inventory.Instance.allItems[inventoryItemNo].canUseUnderWater)))
		{
			inventoryItemNo = -1;
		}
		if (base.isLocalPlayer && carrying)
		{
			inventoryItemNo = -2;
		}
		if (base.isLocalPlayer && layingDown)
		{
			inventoryItemNo = -1;
		}
		if (base.isLocalPlayer && lookingAtMap)
		{
			inventoryItemNo = -3;
		}
		if (base.isLocalPlayer && lookingAtJournal)
		{
			inventoryItemNo = Inventory.Instance.getInvItemId(GiftedItemWindow.gifted.journalItem);
		}
		if (base.isLocalPlayer && crafting)
		{
			inventoryItemNo = -4;
		}
		if (base.isLocalPlayer && cooking)
		{
			inventoryItemNo = -5;
		}
		if ((base.isLocalPlayer && driving) || (base.isLocalPlayer && petting) || (base.isLocalPlayer && climbing))
		{
			inventoryItemNo = -1;
		}
		if (base.isLocalPlayer && whistling)
		{
			inventoryItemNo = Inventory.Instance.getInvItemId(dogWhistleItem);
		}
		if (((bool)holdingPrefab && currentlyHoldingItemId != inventoryItemNo) || inventoryItemNo < 0)
		{
			Object.Destroy(holdingPrefab);
			holdingPrefabAnimator = null;
			holdingPrefab = null;
			itemCurrentlyHolding = null;
		}
		if (inventoryItemNo <= -1)
		{
			myAnim.SetInteger(holdingToolAnimation, -1);
		}
		if (!itemCurrentlyHolding && inventoryItemNo != -1)
		{
			switch (inventoryItemNo)
			{
			case -5:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(cookingPan, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -4:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(craftingHammer, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -3:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(holdingMapPrefab, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -2:
				if (holdingPrefab == null)
				{
					myAnim.SetBool("CarryingItem", value: true);
				}
				break;
			default:
				itemCurrentlyHolding = Inventory.Instance.allItems[inventoryItemNo];
				clearHandPlaceable();
				if (((bool)Inventory.Instance.allItems[inventoryItemNo].equipable && Inventory.Instance.allItems[inventoryItemNo].equipable.cloths && Inventory.Instance.allItems[inventoryItemNo].equipable.hat) || ((bool)Inventory.Instance.allItems[inventoryItemNo].equipable && Inventory.Instance.allItems[inventoryItemNo].equipable.cloths && Inventory.Instance.allItems[inventoryItemNo].equipable.face))
				{
					holdingPrefab = Object.Instantiate(EquipWindow.equip.holdingHatOrFaceObject, holdPos);
					holdingPrefab.GetComponent<SpawnHatOrFaceInside>().setUpForObject(inventoryItemNo);
				}
				else if (Inventory.Instance.allItems[inventoryItemNo].useRightHandAnim)
				{
					holdingPrefab = Object.Instantiate(Inventory.Instance.allItems[inventoryItemNo].itemPrefab, rightHandHoldPos);
					if (myAnim.GetInteger(holdingToolAnimation) != (int)Inventory.Instance.allItems[inventoryItemNo].myAnimType)
					{
						myAnim.SetTrigger("ChangeItem");
					}
					myAnim.SetInteger(holdingToolAnimation, (int)Inventory.Instance.allItems[inventoryItemNo].myAnimType);
					useTool = holdingPrefab.GetComponent<ToolDoesDamage>();
					toolWeapon = holdingPrefab.GetComponent<MeleeAttacks>();
				}
				else
				{
					holdingPrefab = Object.Instantiate(Inventory.Instance.allItems[inventoryItemNo].itemPrefab, holdPos);
					myAnim.SetInteger(holdingToolAnimation, -1);
				}
				holdingPrefab.transform.localPosition = Vector3.zero;
				break;
			}
			if ((bool)holdingPrefab)
			{
				SetItemTexture componentInChildren = holdingPrefab.GetComponentInChildren<SetItemTexture>();
				if ((bool)componentInChildren)
				{
					componentInChildren.setTexture(Inventory.Instance.allItems[inventoryItemNo]);
					if ((bool)componentInChildren.changeSize)
					{
						componentInChildren.changeSizeOfTrans(Inventory.Instance.allItems[inventoryItemNo].transform.localScale);
					}
				}
				holdingPrefabAnimator = holdingPrefab.GetComponent<Animator>();
				leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandle");
				rightHandPos = holdingPrefab.transform.Find("Animation/RightHandle");
				lookable = holdingPrefab.transform.Find("Animation/Lookable");
				if ((bool)itemCurrentlyHolding && !itemCurrentlyHolding.useRightHandAnim && (bool)holdingPrefabAnimator && itemCurrentlyHolding.isATool && (bool)leftHandPos && (bool)rightHandPos && !itemCurrentlyHolding.ignoreTwoArmAnim)
				{
					myAnim.SetBool("TwoArms", value: true);
				}
				else
				{
					myAnim.SetBool("TwoArms", value: false);
				}
			}
		}
		else
		{
			myAnim.SetBool("TwoArms", value: false);
		}
		NetworkcurrentlyHoldingItemId = inventoryItemNo;
		highlighter.checkIfHidden(itemCurrentlyHolding);
		CmdEquipNewItem(inventoryItemNo);
		Inventory.Instance.CheckIfBagInInventory();
	}

	public void placeHandPlaceable()
	{
		if (!itemCurrentlyHolding || ((!itemCurrentlyHolding.equipable || !itemCurrentlyHolding.equipable.cloths) && !itemCurrentlyHolding.fish && !itemCurrentlyHolding.bug) || itemCurrentlyHolding == EquipWindow.equip.minersHelmet || itemCurrentlyHolding == EquipWindow.equip.emptyMinersHelmet)
		{
			return;
		}
		if ((bool)itemCurrentlyHolding.fish)
		{
			if (itemCurrentlyHolding.transform.localScale.z >= 1.5f)
			{
				itemCurrentlyHolding.placeable = EquipWindow.equip.largeFishTank;
			}
			else if (itemCurrentlyHolding.transform.localScale.z <= 0.4f)
			{
				itemCurrentlyHolding.placeable = EquipWindow.equip.smallFishTank;
			}
			else
			{
				itemCurrentlyHolding.placeable = EquipWindow.equip.fishTank;
			}
		}
		else if ((bool)itemCurrentlyHolding.bug)
		{
			itemCurrentlyHolding.placeable = EquipWindow.equip.bugTank;
		}
		else if ((bool)itemCurrentlyHolding.equipable && itemCurrentlyHolding.equipable.shirt)
		{
			itemCurrentlyHolding.placeable = EquipWindow.equip.shirtPlaceable;
		}
		else if (((bool)itemCurrentlyHolding.equipable && itemCurrentlyHolding.equipable.hat) || ((bool)itemCurrentlyHolding.equipable && itemCurrentlyHolding.equipable.face))
		{
			itemCurrentlyHolding.placeable = EquipWindow.equip.hatPlaceable;
		}
		else if ((bool)itemCurrentlyHolding.equipable && itemCurrentlyHolding.equipable.pants)
		{
			itemCurrentlyHolding.placeable = EquipWindow.equip.pantsPlaceable;
		}
		else if ((bool)itemCurrentlyHolding.equipable && itemCurrentlyHolding.equipable.shoes)
		{
			itemCurrentlyHolding.placeable = EquipWindow.equip.shoePlaceable;
		}
	}

	public void clearHandPlaceable()
	{
		if ((bool)itemCurrentlyHolding && (((bool)itemCurrentlyHolding.equipable && itemCurrentlyHolding.equipable.cloths) || (bool)itemCurrentlyHolding.fish || (bool)itemCurrentlyHolding.bug))
		{
			itemCurrentlyHolding.placeable = null;
		}
	}

	public bool usesHandPlaceable()
	{
		if ((bool)itemCurrentlyHolding && ((bool)itemCurrentlyHolding.equipable || (bool)itemCurrentlyHolding.fish || (bool)itemCurrentlyHolding.bug))
		{
			return true;
		}
		return false;
	}

	public bool needsHandPlaceable()
	{
		if ((bool)itemCurrentlyHolding)
		{
			if ((bool)itemCurrentlyHolding.placeable)
			{
				return false;
			}
			if ((bool)itemCurrentlyHolding.equipable || (bool)itemCurrentlyHolding.fish || (bool)itemCurrentlyHolding.bug)
			{
				return true;
			}
		}
		return false;
	}

	public void equipNewItemNetwork(int oldItem, int inventoryItemNo)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		if (oldItem == -2 && inventoryItemNo != -2)
		{
			myAnim.SetBool("CarryingItem", value: false);
		}
		setLeftHandWeight(1f);
		if ((bool)holdingPrefab && oldItem != inventoryItemNo)
		{
			Object.Destroy(holdingPrefab);
			holdingPrefabAnimator = null;
			holdingPrefab = null;
			itemCurrentlyHolding = null;
		}
		if (inventoryItemNo <= -1)
		{
			myAnim.SetInteger(holdingToolAnimation, -1);
		}
		if (!itemCurrentlyHolding && inventoryItemNo != -1)
		{
			switch (inventoryItemNo)
			{
			case -5:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(cookingPan, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -4:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(craftingHammer, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -3:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(holdingMapPrefab, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -2:
				if (holdingPrefab == null)
				{
					myAnim.SetBool("CarryingItem", value: true);
				}
				break;
			default:
				itemCurrentlyHolding = Inventory.Instance.allItems[inventoryItemNo];
				clearHandPlaceable();
				if (((bool)Inventory.Instance.allItems[inventoryItemNo].equipable && Inventory.Instance.allItems[inventoryItemNo].equipable.cloths && Inventory.Instance.allItems[inventoryItemNo].equipable.hat) || ((bool)Inventory.Instance.allItems[inventoryItemNo].equipable && Inventory.Instance.allItems[inventoryItemNo].equipable.cloths && Inventory.Instance.allItems[inventoryItemNo].equipable.face))
				{
					holdingPrefab = Object.Instantiate(EquipWindow.equip.holdingHatOrFaceObject, holdPos);
					holdingPrefab.GetComponent<SpawnHatOrFaceInside>().setUpForObject(inventoryItemNo);
				}
				else if (Inventory.Instance.allItems[inventoryItemNo].useRightHandAnim)
				{
					holdingPrefab = Object.Instantiate(Inventory.Instance.allItems[inventoryItemNo].itemPrefab, rightHandHoldPos);
					if (myAnim.GetInteger(holdingToolAnimation) != (int)Inventory.Instance.allItems[inventoryItemNo].myAnimType)
					{
						myAnim.SetTrigger("ChangeItem");
					}
					myAnim.SetInteger(holdingToolAnimation, (int)Inventory.Instance.allItems[inventoryItemNo].myAnimType);
					useTool = holdingPrefab.GetComponent<ToolDoesDamage>();
					toolWeapon = holdingPrefab.GetComponent<MeleeAttacks>();
				}
				else
				{
					holdingPrefab = Object.Instantiate(Inventory.Instance.allItems[inventoryItemNo].itemPrefab, holdPos);
					myAnim.SetInteger(holdingToolAnimation, -1);
				}
				holdingPrefab.transform.localPosition = Vector3.zero;
				break;
			}
			if ((bool)holdingPrefab)
			{
				SetItemTexture componentInChildren = holdingPrefab.GetComponentInChildren<SetItemTexture>();
				if ((bool)componentInChildren)
				{
					componentInChildren.setTexture(Inventory.Instance.allItems[inventoryItemNo]);
					if ((bool)componentInChildren.changeSize)
					{
						componentInChildren.changeSizeOfTrans(Inventory.Instance.allItems[inventoryItemNo].transform.localScale);
					}
				}
				holdingPrefab.transform.localPosition = Vector3.zero;
				holdingPrefabAnimator = holdingPrefab.GetComponent<Animator>();
				leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandle");
				rightHandPos = holdingPrefab.transform.Find("Animation/RightHandle");
				lookable = holdingPrefab.transform.Find("Animation/Lookable");
				if ((bool)itemCurrentlyHolding && !itemCurrentlyHolding.useRightHandAnim && (bool)holdingPrefabAnimator && itemCurrentlyHolding.isATool && (bool)leftHandPos && (bool)rightHandPos)
				{
					myAnim.SetBool("TwoArms", value: true);
				}
				else
				{
					myAnim.SetBool("TwoArms", value: false);
				}
			}
		}
		else
		{
			myAnim.SetBool("TwoArms", value: false);
		}
		NetworkcurrentlyHoldingItemId = inventoryItemNo;
		if (!base.isLocalPlayer)
		{
			placeHandPlaceable();
		}
	}

	public void setLeftHandWeight(float newWeight)
	{
		leftHandWeight = 1f;
	}

	public bool isInVehicle()
	{
		return inVehicle;
	}

	public void setVehicleHands(Vehicle drivingVehicle)
	{
		rightHandPos = drivingVehicle.rightHandle;
		leftHandPos = drivingVehicle.leftHandle;
		leftFoot = drivingVehicle.leftFoot;
		rightFoot = drivingVehicle.rightFoot;
		myAnim.SetBool("TwoArms", value: true);
		if ((bool)drivingVehicle.leftHandle)
		{
			setLeftHandWeight(1f);
		}
		inVehicle = drivingVehicle;
		lookable = drivingVehicle.lookAtPos;
		lookingWeight = 1f;
	}

	public void stopVehicleHands()
	{
		inVehicle = null;
		lookingWeight = 0f;
		if (itemCurrentlyHolding == null)
		{
			rightHandPos = null;
			leftHandPos = null;
			leftFoot = null;
			rightFoot = null;
			lookable = null;
			lookingWeight = 0f;
			myAnim.SetBool("TwoArms", value: false);
		}
	}

	private Vector3 GetSmoothIKTarget(Vector3 targetPosition)
	{
		return targetPosition;
	}

	private void OnAnimatorIK()
	{
		if ((bool)rightHandPos)
		{
			myAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
			myAnim.SetIKPosition(AvatarIKGoal.RightHand, GetSmoothIKTarget(rightHandPos.position + dif));
			myAnim.SetIKRotation(AvatarIKGoal.RightHand, rightHandPos.rotation);
		}
		if ((bool)leftHandPos && leftHandWeight > 0f)
		{
			myAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
			myAnim.SetIKPosition(AvatarIKGoal.LeftHand, GetSmoothIKTarget(leftHandPos.position + dif));
			myAnim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPos.rotation);
		}
		if ((bool)lookable && (bool)inVehicle)
		{
			myAnim.SetLookAtPosition(lookable.position);
			myAnim.SetLookAtWeight(1f, 1f, 1f);
		}
		if ((bool)inVehicle)
		{
			if ((bool)leftFoot)
			{
				myAnim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
				myAnim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
				myAnim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot.position);
				myAnim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoot.rotation);
			}
			if ((bool)rightFoot)
			{
				myAnim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
				myAnim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
				myAnim.SetIKPosition(AvatarIKGoal.RightFoot, rightFoot.position);
				myAnim.SetIKRotation(AvatarIKGoal.RightFoot, rightFoot.rotation);
			}
		}
	}

	public bool isCarrying()
	{
		return carrying;
	}

	public void setCarrying(bool newCarrying)
	{
		if (newCarrying != carrying)
		{
			carrying = newCarrying;
			if (carrying)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public void setLayDown(bool newLayingDown)
	{
		if (newLayingDown != layingDown)
		{
			layingDown = newLayingDown;
			if (layingDown)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public bool getSwimming()
	{
		return swimming;
	}

	public void doDamageNow()
	{
		if ((bool)holdingPrefab && (bool)useTool)
		{
			useTool.doDamageNow();
		}
	}

	public void checkRefill()
	{
		if ((bool)holdingPrefab && (bool)useTool)
		{
			useTool.checkRefill();
		}
	}

	public void playToolParticles()
	{
		if ((bool)holdingPrefab)
		{
			holdingPrefab.GetComponent<ActivateAnimationParticles>().emitParticles(20);
		}
	}

	public void playToolSound()
	{
		if ((bool)holdingPrefab)
		{
			holdingPrefab.GetComponent<ActivateAnimationParticles>().playSound();
		}
	}

	public void lookLockFrames(int frame)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.turnOnLookLockForFramesWithoutUsing(frame);
		}
	}

	public void startAttack()
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.attack();
		}
	}

	public void lockPosForFrames(int frames)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.lockPosForFrames(frames);
		}
	}

	public void toolDoesDamageToolPosNo(int noToUse)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.toolDoesDamageToolPosNo(noToUse);
		}
	}

	public void makeSwingSound()
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.playSwordSwingSound();
		}
	}

	public void playSwingPartsForFrames(int frames)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.playSwingPartsForFrames();
		}
	}

	public void checkForClang()
	{
		if ((bool)useTool && useTool.checkIfNeedClang())
		{
			useTool.playClangSound();
			myAnim.SetTrigger("Clang");
			if (base.isLocalPlayer)
			{
				InputMaster.input.doRumble(0.35f, 1f);
			}
		}
	}

	public void startCrafting()
	{
		StartCoroutine(playCraftingAnimation());
	}

	public void startCooking()
	{
		StartCoroutine(playCookingAnimation());
	}

	public IEnumerator playCraftingAnimation()
	{
		if (!crafting)
		{
			crafting = true;
			equipNewItem(currentlyHoldingItemId);
			yield return new WaitForSeconds(1.5f);
			crafting = false;
			Inventory.Instance.equipNewSelectedSlot();
		}
	}

	public IEnumerator playCookingAnimation()
	{
		if (!cooking)
		{
			cooking = true;
			equipNewItem(currentlyHoldingItemId);
			yield return new WaitForSeconds(1.5f);
			cooking = false;
			Inventory.Instance.equipNewSelectedSlot();
		}
	}

	public void setNewLookingAtJournal(bool isLookingAtJournalNow)
	{
		if (isLookingAtJournalNow != lookingAtJournal)
		{
			lookingAtJournal = isLookingAtJournalNow;
			if (lookingAtJournal)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public void setNewLookingAtMap(bool newLookingAtMap)
	{
		if (newLookingAtMap != lookingAtMap)
		{
			lookingAtMap = newLookingAtMap;
			if (lookingAtMap)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public void setPetting(bool newPetting)
	{
		if (newPetting != petting)
		{
			petting = newPetting;
			if (petting)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public bool isWhistling()
	{
		return whistling;
	}

	public void CharWhistles()
	{
		MonoBehaviour.print("Calling whistle");
		StartCoroutine(playWhistle());
	}

	private IEnumerator playWhistle()
	{
		if (base.isLocalPlayer)
		{
			setWhistling(newWhistle: true);
			GetComponent<CharMovement>();
			for (float whistleTimer = 1f; whistleTimer > 0f; whistleTimer -= Time.deltaTime)
			{
				yield return null;
			}
			setWhistling(newWhistle: false);
		}
	}

	public void setWhistling(bool newWhistle)
	{
		if (newWhistle != whistling)
		{
			whistling = newWhistle;
			if (whistling)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public void setSwimming(bool newSwimming)
	{
		if (newSwimming != swimming)
		{
			swimming = newSwimming;
			if (swimming)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public void setDoingEmote(bool newEmote)
	{
		if (newEmote == doingEmote)
		{
			return;
		}
		doingEmote = newEmote;
		if (base.isLocalPlayer)
		{
			if (doingEmote)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public bool isInside()
	{
		return IsInsideBuilding;
	}

	public bool IsInsideNonPlayerHouse()
	{
		if (IsInsideBuilding && !IsInsidePlayerHouse)
		{
			return true;
		}
		return false;
	}

	public void setInsideOrOutside(bool insideOrOut, bool playersHouse)
	{
		if (IsInsideBuilding != insideOrOut)
		{
			IsInsideBuilding = insideOrOut;
			IsInsidePlayerHouse = playersHouse;
			equipNewItem(currentlyHoldingItemId);
		}
		if (!insideOrOut)
		{
			Inventory.Instance.equipNewSelectedSlot();
		}
	}

	public bool IsDriving()
	{
		return driving;
	}

	public bool IsClimbing()
	{
		return climbing;
	}

	public void setDriving(bool newDriving)
	{
		if (newDriving != driving)
		{
			driving = newDriving;
			if (driving)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	public void setClimbing(bool newClimbing)
	{
		if (newClimbing != climbing)
		{
			climbing = newClimbing;
			if (climbing)
			{
				equipNewItem(currentlyHoldingItemId);
			}
			else
			{
				Inventory.Instance.equipNewSelectedSlot();
			}
		}
	}

	private IEnumerator swapAnim()
	{
		itemHolderAnim.SetTrigger("PutAway");
		yield return new WaitForSeconds(0.5f);
		itemHolderAnim.SetTrigger("PutAway");
	}

	private void equipMaterialFromInvItem(int inventoryItem, SkinnedMeshRenderer renToPutOn)
	{
		if (inventoryItem == -1)
		{
			if (renToPutOn != shoeRen)
			{
				renToPutOn.gameObject.SetActive(value: true);
				renToPutOn.material = EquipWindow.equip.underClothes;
			}
			else
			{
				renToPutOn.gameObject.SetActive(value: false);
			}
		}
		else
		{
			renToPutOn.gameObject.SetActive(value: true);
			renToPutOn.material = Inventory.Instance.allItems[inventoryItem].equipable.material;
		}
	}

	private void equipHeadItem(int itemNoToEquip)
	{
		NetworkheadId = itemNoToEquip;
		if (itemOnHead != null)
		{
			Object.Destroy(itemOnHead);
		}
		if (hairOnHead != null)
		{
			Object.Destroy(hairOnHead);
		}
		if (hairId >= 0 && (itemNoToEquip < 0 || !Inventory.Instance.allItems[itemNoToEquip].equipable || !Inventory.Instance.allItems[itemNoToEquip].equipable.hideHair))
		{
			if (itemNoToEquip >= 0 && (bool)Inventory.Instance.allItems[itemNoToEquip].equipable && Inventory.Instance.allItems[itemNoToEquip].equipable.useHelmetHair)
			{
				hairOnHead = Object.Instantiate(CharacterCreatorScript.create.allHairStyles[0], onHeadPosition);
			}
			else
			{
				hairOnHead = Object.Instantiate(CharacterCreatorScript.create.allHairStyles[hairId], onHeadPosition);
			}
			hairOnHead.transform.localPosition = Vector3.zero;
			hairOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
			if ((bool)hairOnHead.GetComponent<SpringManager>())
			{
				GetComponent<CharNetworkAnimator>().hairSpring = hairOnHead.GetComponent<SpringManager>();
			}
		}
		if (itemNoToEquip >= 0)
		{
			if ((bool)hairOnHead && (bool)Inventory.Instance.allItems[itemNoToEquip].equipable && !Inventory.Instance.allItems[itemNoToEquip].equipable.useRegularHair)
			{
				hairOnHead.transform.Find("Hair").gameObject.SetActive(value: false);
				hairOnHead.transform.Find("Hair_Hat").gameObject.SetActive(value: true);
				hairOnHead.transform.localPosition = Vector3.zero;
				hairOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
			}
			itemOnHead = Object.Instantiate(Inventory.Instance.allItems[itemNoToEquip].equipable.hatPrefab, onHeadPosition);
			if ((bool)itemOnHead.GetComponent<SetItemTexture>())
			{
				itemOnHead.GetComponent<SetItemTexture>().setTexture(Inventory.Instance.allItems[itemNoToEquip]);
				if ((bool)itemOnHead.GetComponent<SetItemTexture>().changeSize)
				{
					itemOnHead.GetComponent<SetItemTexture>().changeSizeOfTrans(Inventory.Instance.allItems[itemNoToEquip].transform.localScale);
				}
			}
			itemOnHead.transform.localPosition = Vector3.zero;
			itemOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}
		equipHairColour(hairColor);
		StopCoroutine("hairBounce");
		StartCoroutine("hairBounce");
	}

	private void equipHairColour(int colourNo)
	{
		if ((bool)hairOnHead)
		{
			colourNo = Mathf.Clamp(colourNo, 0, CharacterCreatorScript.create.allHairColours.Length - 1);
			if ((bool)hairOnHead.GetComponentInChildren<MeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<MeshRenderer>().material.color = CharacterCreatorScript.create.getHairColour(colourNo);
			}
			if ((bool)hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>().material.color = CharacterCreatorScript.create.getHairColour(colourNo);
			}
		}
	}

	private void onHairColourChange(int oldColour, int newHairColour)
	{
		NetworkhairColor = Mathf.Clamp(newHairColour, 0, CharacterCreatorScript.create.allHairColours.Length - 1);
		equipHairColour(newHairColour);
	}

	private void onChangeSkin(int oldSkin, int newSkin)
	{
		NetworkskinId = Mathf.Clamp(newSkin, 0, CharacterCreatorScript.create.skinTones.Length - 1);
		skinRen.material = CharacterCreatorScript.create.skinTones[skinId];
		eyes.changeSkinColor(CharacterCreatorScript.create.skinTones[skinId].color);
	}

	private void onMouthChange(int oldMouth, int newMouth)
	{
		NetworkmouthId = newMouth;
		eyes.changeMouthMat(CharacterCreatorScript.create.mouthTypes[mouthId], CharacterCreatorScript.create.skinTones[skinId].color);
	}

	private void onNoseChange(int oldNose, int newNose)
	{
		NetworknoseId = newNose;
		eyes.noseMesh.GetComponent<MeshFilter>().sharedMesh = CharacterCreatorScript.create.noseMeshes[noseId];
	}

	private void onHeadChange(int oldId, int newId)
	{
		_ = (bool)myAnim;
		NetworkheadId = newId;
		equipHeadItem(newId);
	}

	private void onHairChange(int oldId, int newId)
	{
		_ = (bool)myAnim;
		NetworkhairId = Mathf.Clamp(newId, 0, CharacterCreatorScript.create.allHairStyles.Length);
		equipHeadItem(headId);
	}

	private void onFaceChange(int oldId, int newId)
	{
		if ((bool)itemOnFace)
		{
			Object.Destroy(itemOnFace);
		}
		if (newId > -1)
		{
			itemOnFace = Object.Instantiate(Inventory.Instance.allItems[newId].equipable.hatPrefab, onHeadPosition);
			itemOnFace.transform.localPosition = Vector3.zero;
			itemOnFace.transform.localRotation = Quaternion.Euler(Vector3.zero);
			if ((bool)itemOnFace.GetComponent<SetItemTexture>())
			{
				itemOnFace.GetComponent<SetItemTexture>().setTexture(Inventory.Instance.allItems[newId]);
			}
		}
		NetworkfaceId = newId;
	}

	private void onChangeShirt(int oldId, int newId)
	{
		_ = (bool)myAnim;
		NetworkshirtId = newId;
		if (newId != -1 && Inventory.Instance.allItems[shirtId].equipable.dress)
		{
			shirtRen.gameObject.SetActive(value: false);
			if (Inventory.Instance.allItems[shirtId].equipable.dress && !Inventory.Instance.allItems[shirtId].equipable.longDress)
			{
				dressRen.gameObject.SetActive(value: true);
				longDressRen.gameObject.SetActive(value: false);
				equipMaterialFromInvItem(newId, dressRen);
			}
			else
			{
				dressRen.gameObject.SetActive(value: false);
				longDressRen.gameObject.SetActive(value: true);
				equipMaterialFromInvItem(newId, longDressRen);
			}
			GetComponent<CharNetworkAnimator>().SetDressOnOrOff(isDressOn: true);
			return;
		}
		shirtRen.gameObject.SetActive(value: true);
		dressRen.gameObject.SetActive(value: false);
		longDressRen.gameObject.SetActive(value: false);
		GetComponent<CharNetworkAnimator>().SetDressOnOrOff(isDressOn: false);
		if (newId != -1 && (bool)Inventory.Instance.allItems[shirtId].equipable.shirtMesh)
		{
			shirtRen.sharedMesh = Inventory.Instance.allItems[shirtId].equipable.shirtMesh;
		}
		else if (newId == -1)
		{
			shirtRen.sharedMesh = EquipWindow.equip.tShirtMesh;
		}
		else
		{
			shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
		}
		equipMaterialFromInvItem(newId, shirtRen);
	}

	private void onChangePants(int oldId, int newId)
	{
		_ = (bool)myAnim;
		if (newId != -1 && Inventory.Instance.allItems[newId].equipable.dress)
		{
			skirtRen.gameObject.SetActive(value: true);
			equipMaterialFromInvItem(newId, skirtRen);
			GetComponent<CharNetworkAnimator>().SetDressOnOrOff(isDressOn: true);
			pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
			equipMaterialFromInvItem(-1, pantsRen);
		}
		else
		{
			skirtRen.gameObject.SetActive(value: false);
			if (newId != -1 && (bool)Inventory.Instance.allItems[newId].equipable.useAltMesh)
			{
				pantsRen.sharedMesh = Inventory.Instance.allItems[newId].equipable.useAltMesh;
			}
			else
			{
				pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
			}
			equipMaterialFromInvItem(newId, pantsRen);
		}
		NetworkpantsId = newId;
	}

	private void onChangeShoes(int oldId, int newId)
	{
		_ = (bool)myAnim;
		if (newId != -1 && (bool)Inventory.Instance.allItems[newId].equipable.useAltMesh)
		{
			shoeRen.sharedMesh = Inventory.Instance.allItems[newId].equipable.useAltMesh;
		}
		else
		{
			shoeRen.sharedMesh = EquipWindow.equip.defualtShoeMesh;
		}
		NetworkshoeId = newId;
		equipMaterialFromInvItem(newId, shoeRen);
	}

	private void onChangeEyes(int oldId, int newId)
	{
		eyes.changeEyeMat(CharacterCreatorScript.create.allEyeTypes[newId], CharacterCreatorScript.create.skinTones[skinId].color);
		NetworkeyeId = newId;
	}

	private void onChangeEyeColor(int oldId, int newColor)
	{
		eyes.changeEyeColor(CharacterCreatorScript.create.eyeColours[newColor]);
		NetworkeyeColor = newColor;
	}

	public void doEmotion(int emotion)
	{
		if (doingEmotion != null)
		{
			StopCoroutine(doingEmotion);
		}
		doingEmotion = StartCoroutine(doEmote(emotion));
	}

	public bool checkIfDoingEmote()
	{
		return doingEmote;
	}

	private IEnumerator doEmote(int emotion)
	{
		setDoingEmote(newEmote: true);
		GetComponent<AnimateCharFace>().emotionsLocked = false;
		myAnim.SetInteger("Emotion", emotion);
		yield return new WaitForSeconds(2.5f);
		GetComponent<AnimateCharFace>().emotionsLocked = true;
		myAnim.SetInteger("Emotion", 0);
		GetComponent<AnimateCharFace>().stopFaceEmotion();
		doingEmotion = null;
		setDoingEmote(newEmote: false);
	}

	public void breakItemAnimation()
	{
		GetComponent<AnimateCharFace>().emotionsLocked = false;
		myAnim.SetInteger("Emotion", 6);
		Invoke("delayStop", 0.75f);
	}

	private void delayStop()
	{
		GetComponent<AnimateCharFace>().emotionsLocked = true;
		myAnim.SetInteger("Emotion", 0);
		GetComponent<AnimateCharFace>().stopFaceEmotion();
	}

	[ClientRpc]
	private void RpcBreakItem()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(EquipItemToChar), "RpcBreakItem", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdBrokenItem()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdBrokenItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeSkin(int newSkin)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newSkin);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeSkin", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeFaceId(int newFaceId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newFaceId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeFaceId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHairId(int newHairId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHairId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeHairId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUpdateCharAppearence(int newEyeColor, int newEyeStyle, int newSkinTone, int newNose, int newMouth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEyeColor);
		writer.WriteInt(newEyeStyle);
		writer.WriteInt(newSkinTone);
		writer.WriteInt(newNose);
		writer.WriteInt(newMouth);
		SendCommandInternal(typeof(EquipItemToChar), "CmdUpdateCharAppearence", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeEyes(int newEyeId, int newEyeColor)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEyeId);
		writer.WriteInt(newEyeColor);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeEyes", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHeadId(int newHeadId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHeadId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeHeadId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeShirtId(int newShirtId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newShirtId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeShirtId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangePantsId(int newPantsId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newPantsId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangePantsId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeShoesId(int newShoesId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newShoesId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeShoesId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdEquipNewItem(int newEquip)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEquip);
		SendCommandInternal(typeof(EquipItemToChar), "CmdEquipNewItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUsingItem(bool isUsing)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isUsing);
		SendCommandInternal(typeof(EquipItemToChar), "CmdUsingItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHairColour(int newHair)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHair);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeHairColour", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeEyeColour(int newEye)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEye);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeEyeColour", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendEquipedClothes(int[] clothesArray)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, clothesArray);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSendEquipedClothes", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeNose(int newNose)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newNose);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeNose", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeMouth(int newMouth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newMouth);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeMouth", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendName(string setPlayerName, int setIslandId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(setPlayerName);
		writer.WriteInt(setIslandId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSendName", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdMakeHairDresserSpin()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdMakeHairDresserSpin", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCallforBugNet(int netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(netId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdCallforBugNet", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdOpenBag()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdOpenBag", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCloseBag()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdCloseBag", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void changeOpenBag(bool oldBagOpen, bool newBagOpen)
	{
		NetworkbagOpenEmoteOn = newBagOpen;
		if (newBagOpen)
		{
			if (doingEmotion != null)
			{
				StopCoroutine(doingEmotion);
			}
			doingEmotion = StartCoroutine(bagOpenEmote());
		}
	}

	public void SetShowingBag(bool newHasBag, int colourId)
	{
		if (localBagColour != colourId)
		{
			localBagColour = colourId;
			CmdChangeBagColour(colourId);
		}
		if (currentlyHoldingItemId == ChestWindow.chests.swagSack.getItemId())
		{
			if (localShowingBag)
			{
				localShowingBag = false;
				CmdSetBagStatus(newHasBag: false);
			}
		}
		else if (localShowingBag != newHasBag)
		{
			localShowingBag = newHasBag;
			CmdSetBagStatus(newHasBag);
		}
	}

	public void ChangeHasBag(bool oldHasBag, bool newHasBag)
	{
		NetworkhasBag = newHasBag;
		packOnBack.SetActive(hasBag);
		if (hasBag)
		{
			StartCoroutine(BagAppears());
		}
	}

	public void OnChangeBagColour(int oldColour, int newColour)
	{
		NetworkbagColour = newColour;
		packOnBack.GetComponent<Backpack>().ChangeColour(newColour);
		ChangeBagInHandDelay();
	}

	public void OnDisguiseChange(int oldDisguise, int newDisguise)
	{
		NetworkdisguiseId = newDisguise;
		if ((bool)disguiseTileObject)
		{
			Object.Destroy(disguiseTileObject);
		}
		if (oldDisguise != newDisguise)
		{
			SoundManager.Instance.playASoundAtPoint(SoundManager.Instance.transformSound, base.transform.position);
			if (!base.isLocalPlayer)
			{
				setNameTagOnOff();
				if (newDisguise == -1)
				{
					RenderMap.Instance.trackOtherPlayers(base.transform);
				}
				else
				{
					RenderMap.Instance.unTrackOtherPlayers(base.transform);
				}
			}
		}
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position + Vector3.up, 10);
		if (disguiseId <= -2000)
		{
			int num = Mathf.Abs(disguiseId + 2000);
			int num2 = Mathf.FloorToInt((float)num / 10f);
			int num3 = num - num2 * 10;
			MonoBehaviour.print(num2);
			MonoBehaviour.print(num3);
			disguiseTileObject = Object.Instantiate(AnimalManager.manage.allAnimals[num2].gameObject, base.transform);
			disguiseTileObject.transform.localPosition = Vector3.zero;
			disguiseTileObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			AnimalAI component = disguiseTileObject.GetComponent<AnimalAI>();
			AnimalVariation component2 = disguiseTileObject.GetComponent<AnimalVariation>();
			if ((bool)component2)
			{
				component2.SetVariationForDisguise(num3);
			}
			if ((bool)component.GetComponent<AnimateAnimalAI>())
			{
				component.GetComponent<AnimateAnimalAI>().enabled = false;
			}
			Object.Destroy(component.GetComponent<Damageable>());
			component.GetComponent<ProximityChecker>().enabled = false;
			component.enabled = false;
			component.myAgent.enabled = false;
			if ((bool)component.GetComponent<AnimalAI_Attack>())
			{
				component.GetComponent<AnimalAI_Attack>().enabled = false;
			}
			if ((bool)component.GetComponent<AnimalAILookForFood>())
			{
				component.GetComponent<AnimalAILookForFood>().enabled = false;
			}
			if ((bool)component.GetComponent<AnimalAI_Sleep>())
			{
				component.GetComponent<AnimalAI_Sleep>().enabled = false;
			}
			Collider[] componentsInChildren = disguiseTileObject.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			StartCoroutine(AnimateFakeAnimal(num2, disguiseId, component.GetComponent<Animator>()));
			HideCharacterForDisguise();
			return;
		}
		if (disguiseId >= 0)
		{
			disguiseTileObject = Object.Instantiate(WorldManager.Instance.allObjects[disguiseId].gameObject, base.transform);
			disguiseTileObject.transform.localPosition = Vector3.zero;
			disguiseTileObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			TileObject component3 = disguiseTileObject.GetComponent<TileObject>();
			moveToWaterLevel[] componentsInChildren2 = component3.GetComponentsInChildren<moveToWaterLevel>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].ResetToWaterLevel();
			}
			if ((bool)component3.tileObjectConnect)
			{
				if (!component3.tileObjectConnect.IsLadder && !component3.tileObjectConnect.inverted)
				{
					component3.tileObjectConnect.rightConnect.SetActive(value: false);
					component3.tileObjectConnect.leftConnect.SetActive(value: false);
					component3.tileObjectConnect.upConnect.SetActive(value: false);
					component3.tileObjectConnect.downConnect.SetActive(value: false);
				}
				if ((bool)component3.tileObjectConnect.secondConnect && !component3.tileObjectConnect.secondConnect.inverted)
				{
					component3.tileObjectConnect.secondConnect.rightConnect.SetActive(value: false);
					component3.tileObjectConnect.secondConnect.leftConnect.SetActive(value: false);
					component3.tileObjectConnect.secondConnect.upConnect.SetActive(value: false);
					component3.tileObjectConnect.secondConnect.downConnect.SetActive(value: false);
				}
				if ((bool)component3.tileOnOff && component3.tileOnOff.isGate)
				{
					component3.tileObjectConnect.leftConnect.SetActive(value: true);
					component3.tileObjectConnect.rightConnect.SetActive(value: true);
					component3.GetComponentInChildren<Animator>().enabled = false;
				}
			}
			if ((bool)component3.tileObjectWritableSign)
			{
				if ((bool)component3.tileObjectWritableSign.signText)
				{
					component3.tileObjectWritableSign.signText.text = "Totally Genuine";
				}
				if ((bool)component3.tileObjectWritableSign.otherSide)
				{
					component3.tileObjectWritableSign.otherSide.text = "Totally Genuine";
				}
			}
			if ((bool)component3.tileObjectGrowthStages)
			{
				component3.tileObjectGrowthStages.setStageForHands(component3.tileObjectGrowthStages.objectStages.Length - 1);
			}
			if (component3.IsMultiTileObject())
			{
				disguiseTileObject.transform.localPosition += new Vector3((float)component3.GetXSize() / 2f, 0f, (float)component3.GetYSize() / 2f);
			}
			Collider[] componentsInChildren3 = disguiseTileObject.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].enabled = false;
			}
			HideCharacterForDisguise();
			return;
		}
		skinRen.gameObject.SetActive(value: true);
		pantsRen.gameObject.SetActive(value: true);
		if (shirtId >= 0)
		{
			if (Inventory.Instance.allItems[shirtId].equipable.dress)
			{
				if (Inventory.Instance.allItems[shirtId].equipable.longDress)
				{
					longDressRen.gameObject.SetActive(value: true);
				}
				else
				{
					dressRen.gameObject.SetActive(value: true);
				}
			}
			else
			{
				shirtRen.gameObject.SetActive(value: true);
			}
		}
		else
		{
			shirtRen.gameObject.SetActive(value: true);
		}
		if (pantsId >= 0 && Inventory.Instance.allItems[pantsId].equipable.dress)
		{
			skirtRen.gameObject.SetActive(value: true);
		}
		if (shoeId >= 0)
		{
			shoeRen.gameObject.SetActive(value: true);
		}
		if ((bool)itemOnHead)
		{
			itemOnHead.SetActive(value: true);
		}
		if ((bool)itemOnFace)
		{
			itemOnFace.SetActive(value: true);
		}
		if ((bool)hairOnHead)
		{
			hairOnHead.SetActive(value: true);
		}
		eyes.eyeInside.gameObject.SetActive(value: true);
		eyes.mouthInside.gameObject.SetActive(value: true);
		noseRen.gameObject.SetActive(value: true);
		packOnBack.SetActive(hasBag);
	}

	private IEnumerator AnimateFakeAnimal(int animalId, int showingDisguise, Animator anim)
	{
		while (showingDisguise == disguiseId)
		{
			if ((bool)anim)
			{
				anim.SetFloat("WalkingSpeed", myAnim.GetFloat("WalkSpeed"));
			}
			if (AnimalManager.manage.allAnimals[animalId].flyingAnimal && (bool)disguiseTileObject)
			{
				if (myAnim.GetFloat("WalkSpeed") > 1f)
				{
					disguiseTileObject.transform.localPosition = Vector3.Lerp(disguiseTileObject.transform.localPosition, new Vector3(0f, 3f, 0f), Time.deltaTime * 5f);
				}
				else
				{
					disguiseTileObject.transform.localPosition = Vector3.Lerp(disguiseTileObject.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
				}
			}
			yield return null;
		}
	}

	private void HideCharacterForDisguise()
	{
		skinRen.gameObject.SetActive(value: false);
		shirtRen.gameObject.SetActive(value: false);
		pantsRen.gameObject.SetActive(value: false);
		shoeRen.gameObject.SetActive(value: false);
		dressRen.gameObject.SetActive(value: false);
		skirtRen.gameObject.SetActive(value: false);
		longDressRen.gameObject.SetActive(value: false);
		if ((bool)itemOnHead)
		{
			itemOnHead.SetActive(value: false);
		}
		if ((bool)itemOnFace)
		{
			itemOnFace.SetActive(value: false);
		}
		if ((bool)hairOnHead)
		{
			hairOnHead.SetActive(value: false);
		}
		eyes.eyeInside.gameObject.SetActive(value: false);
		eyes.mouthInside.gameObject.SetActive(value: false);
		noseRen.gameObject.SetActive(value: false);
		packOnBack.gameObject.SetActive(value: false);
	}

	[Command]
	public void CmdSetDisguise(int newDisguise)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newDisguise);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSetDisguise", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void ChangeBagInHandDelay()
	{
		if ((bool)holdingPrefab)
		{
			Backpack component = holdingPrefab.GetComponent<Backpack>();
			if ((bool)component)
			{
				component.ChangeColour(bagColour);
			}
		}
	}

	private IEnumerator BagAppears()
	{
		float timer2 = 0f;
		while (timer2 < 1f)
		{
			packOnBack.transform.localScale = Vector3.Lerp(new Vector3(0.6f, 0.6f, 0.6f), new Vector3(1.1f, 1.1f, 1.1f), timer2);
			timer2 += Time.deltaTime * 16f;
			yield return null;
		}
		timer2 = 0f;
		while (timer2 < 1f)
		{
			packOnBack.transform.localScale = Vector3.Lerp(new Vector3(1.2f, 1.2f, 1.2f), Vector3.one, timer2);
			timer2 += Time.deltaTime * 9f;
			yield return null;
		}
		packOnBack.transform.localScale = Vector3.one;
	}

	private IEnumerator bagOpenEmote()
	{
		setDoingEmote(newEmote: true);
		GetComponent<AnimateCharFace>().emotionsLocked = false;
		myAnim.SetInteger("Emotion", 5);
		while (bagOpenEmoteOn)
		{
			yield return null;
		}
		GetComponent<AnimateCharFace>().emotionsLocked = true;
		myAnim.SetInteger("Emotion", 0);
		GetComponent<AnimateCharFace>().stopFaceEmotion();
		doingEmotion = null;
		setDoingEmote(newEmote: false);
	}

	[ClientRpc]
	public void RpcPutBugNetInHand(int netId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(netId);
		SendRPCInternal(typeof(EquipItemToChar), "RpcPutBugNetInHand", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator bugNetWait(int netId)
	{
		while (!itemCurrentlyHolding || !itemCurrentlyHolding.bug)
		{
			yield return null;
		}
		while (!holdingPrefabAnimator)
		{
			yield return null;
		}
		holdingPrefabAnimator.GetComponent<Animator>().SetTrigger("UseBugNet");
		holdingPrefab.transform.Find("Animation/LeftHandleNet/" + netId).gameObject.SetActive(value: true);
		leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandleNet");
		myAnim.SetBool("TwoArms", value: true);
	}

	public bool IsCurrentlyHoldingSinglePlaceableItem()
	{
		if (IsCurrentlyHoldingItem && (bool)itemCurrentlyHolding.placeable && !itemCurrentlyHolding.placeable.IsMultiTileObject())
		{
			return true;
		}
		return false;
	}

	public bool CurrentlyHoldingMultiTiledPlaceableItem()
	{
		if (IsCurrentlyHoldingItem && (bool)itemCurrentlyHolding.placeable && itemCurrentlyHolding.placeable.IsMultiTileObject())
		{
			return true;
		}
		return false;
	}

	public bool CurrentlyHoldingDeed()
	{
		if (IsCurrentlyHoldingItem && itemCurrentlyHolding.isDeed)
		{
			return true;
		}
		return false;
	}

	public bool CurrentlyHoldingBridge()
	{
		if (IsCurrentlyHoldingItem && (bool)itemCurrentlyHolding.placeable && (bool)itemCurrentlyHolding.placeable.tileObjectBridge)
		{
			return true;
		}
		return false;
	}

	public void catchAndShowFish(int fishId, bool fromPond)
	{
		if (!fishInHandPlaying)
		{
			BugAndFishCelebration.bugAndFishCel.openWindow(fishId);
			if (!fromPond)
			{
				PediaManager.manage.addCaughtToList(fishId);
			}
			fishInHandPlaying = true;
			StartCoroutine(fishLandsInHand(fishId, fromPond));
		}
	}

	public void catchAndShowBug(int bugId)
	{
		if (!bugInHandPlaying)
		{
			BugAndFishCelebration.bugAndFishCel.openWindow(bugId);
			PediaManager.manage.addCaughtToList(bugId);
			bugInHandPlaying = true;
			StartCoroutine(bugCatchHoldInHand(bugId, currentlyHoldingItemId));
		}
	}

	private IEnumerator fishLandsInHand(int fishId, bool fromPond)
	{
		equipNewItem(fishId);
		Inventory.Instance.quickBarLocked(isLocked: true);
		if (!fromPond)
		{
			CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Fishing, (int)Mathf.Clamp((float)Inventory.Instance.allItems[fishId].value / 200f, 1f, 30f));
			CharLevelManager.manage.addToDayTally(fishId, 1, 3);
		}
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CatchFish);
		if (CatchingCompetitionManager.manage.inCompetition && CatchingCompetitionManager.manage.competitionActive() && CatchingCompetitionManager.manage.IsFishCompToday() && CatchingCompetitionManager.manage.getScoreForFish(fishId) != 0f)
		{
			if (!fromPond)
			{
				addToLocalCompScore(CatchingCompetitionManager.manage.getScoreForFish(fishId));
				BugAndFishCelebration.bugAndFishCel.openCompPoints(CatchingCompetitionManager.manage.getScoreForFish(fishId));
			}
			else
			{
				BugAndFishCelebration.bugAndFishCel.openCompPoints(0f);
			}
		}
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || (ConversationManager.manage.IsConversationActive && ConversationManager.manage.lastConversationTarget.isSign))
		{
			yield return null;
		}
		Inventory.Instance.quickBarLocked(isLocked: false);
		equipNewItem(Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemNo);
		fishInHandPlaying = false;
	}

	private IEnumerator bugCatchHoldInHand(int bugId, int netId)
	{
		equipNewItem(bugId);
		CmdCallforBugNet(netId);
		if (base.isLocalPlayer)
		{
			StartCoroutine(bugNetWait(netId));
		}
		Inventory.Instance.quickBarLocked(isLocked: true);
		CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.BugCatching, (int)Mathf.Clamp((float)Inventory.Instance.allItems[bugId].value / 200f, 1f, 100f));
		CharLevelManager.manage.addToDayTally(bugId, 1, 4);
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CatchBugs);
		if (CatchingCompetitionManager.manage.inCompetition && CatchingCompetitionManager.manage.competitionActive() && CatchingCompetitionManager.manage.IsBugCompToday())
		{
			addToLocalCompScore(CatchingCompetitionManager.manage.getScoreForBug(bugId));
			BugAndFishCelebration.bugAndFishCel.openCompPoints(CatchingCompetitionManager.manage.getScoreForBug(bugId));
		}
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || (ConversationManager.manage.IsConversationActive && ConversationManager.manage.lastConversationTarget.isSign))
		{
			yield return null;
		}
		Inventory.Instance.quickBarLocked(isLocked: false);
		equipNewItem(Inventory.Instance.invSlots[Inventory.Instance.selectedSlot].itemNo);
		bugInHandPlaying = false;
	}

	private IEnumerator hairBounce()
	{
		float journey = 0f;
		float duration = 0.35f;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.hairChangeBounce.Evaluate(time);
			float num = Mathf.LerpUnclamped(0.95f, 1f, t);
			if ((bool)hairOnHead)
			{
				hairOnHead.transform.localScale = new Vector3(num, 1f + (1f - num), 1f + (1f - num));
			}
			if ((bool)itemOnHead)
			{
				itemOnHead.transform.localScale = new Vector3(num, 1f + (1f - num), 1f + (1f - num));
			}
			yield return null;
		}
	}

	public void playPlaceableAnimation()
	{
		if ((bool)holdingPrefab && (bool)holdingPrefabAnimator)
		{
			holdingPrefabAnimator.SetTrigger("PlaceItemAnimation");
		}
	}

	public bool IsHoldingShovel()
	{
		if ((object)itemCurrentlyHolding == null)
		{
			return false;
		}
		return itemCurrentlyHolding.tag.Equals("shovel_empty");
	}

	[Command]
	public void CmdChangeLookableForAiming(Vector3 newPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newPos);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeLookableForAiming", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcMoveLookableForRanged(Vector3 newPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newPos);
		SendRPCInternal(typeof(EquipItemToChar), "RpcMoveLookableForRanged", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdFireProjectileAtDir(Vector3 spawnPos, Vector3 direction, float strength, int projectileId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(spawnPos);
		writer.WriteVector3(direction);
		writer.WriteFloat(strength);
		writer.WriteInt(projectileId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdFireProjectileAtDir", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnProjectileObject(int itemId, Vector3 spawnPos, Vector3 direction, float strength, int invId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteVector3(spawnPos);
		writer.WriteVector3(direction);
		writer.WriteFloat(strength);
		writer.WriteInt(invId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSpawnProjectileObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcFireAtAngle(Vector3 spawnPos, Vector3 forward, float strength, int projectileId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(spawnPos);
		writer.WriteVector3(forward);
		writer.WriteFloat(strength);
		writer.WriteInt(projectileId);
		SendRPCInternal(typeof(EquipItemToChar), "RpcFireAtAngle", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetNewCompScore(float newScore)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(newScore);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSetNewCompScore", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void addToLocalCompScore(float addAmount)
	{
		localCompScore += addAmount;
	}

	[Command]
	public void CmdChangeSize(Vector3 newSize)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newSize);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeSize", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnSizeChange(Vector3 oldSize, Vector3 newSize)
	{
		base.transform.localScale = size;
	}

	public GameObject GetHairObject()
	{
		return hairOnHead;
	}

	[Command]
	public void CmdSetBagStatus(bool newHasBag)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newHasBag);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSetBagStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeBagColour(int newId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeBagColour", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	static EquipItemToChar()
	{
		dif = new Vector3(0f, -0.18f, 0f);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdBrokenItem", InvokeUserCode_CmdBrokenItem, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeSkin", InvokeUserCode_CmdChangeSkin, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeFaceId", InvokeUserCode_CmdChangeFaceId, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeHairId", InvokeUserCode_CmdChangeHairId, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdUpdateCharAppearence", InvokeUserCode_CmdUpdateCharAppearence, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeEyes", InvokeUserCode_CmdChangeEyes, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeHeadId", InvokeUserCode_CmdChangeHeadId, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeShirtId", InvokeUserCode_CmdChangeShirtId, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangePantsId", InvokeUserCode_CmdChangePantsId, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeShoesId", InvokeUserCode_CmdChangeShoesId, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdEquipNewItem", InvokeUserCode_CmdEquipNewItem, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdUsingItem", InvokeUserCode_CmdUsingItem, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeHairColour", InvokeUserCode_CmdChangeHairColour, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeEyeColour", InvokeUserCode_CmdChangeEyeColour, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSendEquipedClothes", InvokeUserCode_CmdSendEquipedClothes, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeNose", InvokeUserCode_CmdChangeNose, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeMouth", InvokeUserCode_CmdChangeMouth, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSendName", InvokeUserCode_CmdSendName, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdMakeHairDresserSpin", InvokeUserCode_CmdMakeHairDresserSpin, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdCallforBugNet", InvokeUserCode_CmdCallforBugNet, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdOpenBag", InvokeUserCode_CmdOpenBag, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdCloseBag", InvokeUserCode_CmdCloseBag, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSetDisguise", InvokeUserCode_CmdSetDisguise, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeLookableForAiming", InvokeUserCode_CmdChangeLookableForAiming, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdFireProjectileAtDir", InvokeUserCode_CmdFireProjectileAtDir, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSpawnProjectileObject", InvokeUserCode_CmdSpawnProjectileObject, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSetNewCompScore", InvokeUserCode_CmdSetNewCompScore, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeSize", InvokeUserCode_CmdChangeSize, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSetBagStatus", InvokeUserCode_CmdSetBagStatus, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeBagColour", InvokeUserCode_CmdChangeBagColour, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcCharacterJoinedPopup", InvokeUserCode_RpcCharacterJoinedPopup);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcBreakItem", InvokeUserCode_RpcBreakItem);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcPutBugNetInHand", InvokeUserCode_RpcPutBugNetInHand);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcMoveLookableForRanged", InvokeUserCode_RpcMoveLookableForRanged);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcFireAtAngle", InvokeUserCode_RpcFireAtAngle);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcCharacterJoinedPopup(string newName, string sendIslandName)
	{
		if (base.isLocalPlayer)
		{
			NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("WelcomeToIsland"), sendIslandName));
		}
		else
		{
			NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("PlayerJoined"), newName));
		}
	}

	protected static void InvokeUserCode_RpcCharacterJoinedPopup(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCharacterJoinedPopup called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcCharacterJoinedPopup(reader.ReadString(), reader.ReadString());
		}
	}

	protected void UserCode_RpcBreakItem()
	{
		ParticleManager.manage.emitBrokenItemPart(holdPos.position + holdPos.forward);
	}

	protected static void InvokeUserCode_RpcBreakItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBreakItem called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcBreakItem();
		}
	}

	protected void UserCode_CmdBrokenItem()
	{
		RpcBreakItem();
		NetworkMapSharer.Instance.RpcBreakToolReact(base.netId);
	}

	protected static void InvokeUserCode_CmdBrokenItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBrokenItem called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdBrokenItem();
		}
	}

	protected void UserCode_CmdChangeSkin(int newSkin)
	{
		NetworkskinId = newSkin;
	}

	protected static void InvokeUserCode_CmdChangeSkin(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeSkin called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeSkin(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeFaceId(int newFaceId)
	{
		NetworkfaceId = newFaceId;
	}

	protected static void InvokeUserCode_CmdChangeFaceId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeFaceId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeFaceId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHairId(int newHairId)
	{
		NetworkhairId = newHairId;
	}

	protected static void InvokeUserCode_CmdChangeHairId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHairId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeHairId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdUpdateCharAppearence(int newEyeColor, int newEyeStyle, int newSkinTone, int newNose, int newMouth)
	{
		NetworkskinId = newSkinTone;
		NetworkeyeId = newEyeStyle;
		NetworkeyeColor = newEyeColor;
		NetworknoseId = newNose;
		NetworkmouthId = newMouth;
	}

	protected static void InvokeUserCode_CmdUpdateCharAppearence(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateCharAppearence called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdUpdateCharAppearence(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeEyes(int newEyeId, int newEyeColor)
	{
		NetworkeyeId = newEyeId;
		NetworkeyeColor = newEyeColor;
	}

	protected static void InvokeUserCode_CmdChangeEyes(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeEyes called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeEyes(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHeadId(int newHeadId)
	{
		NetworkheadId = newHeadId;
	}

	protected static void InvokeUserCode_CmdChangeHeadId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHeadId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeHeadId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeShirtId(int newShirtId)
	{
		NetworkshirtId = newShirtId;
	}

	protected static void InvokeUserCode_CmdChangeShirtId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeShirtId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeShirtId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangePantsId(int newPantsId)
	{
		NetworkpantsId = newPantsId;
	}

	protected static void InvokeUserCode_CmdChangePantsId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangePantsId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangePantsId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeShoesId(int newShoesId)
	{
		NetworkshoeId = newShoesId;
	}

	protected static void InvokeUserCode_CmdChangeShoesId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeShoesId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeShoesId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdEquipNewItem(int newEquip)
	{
		NetworkcurrentlyHoldingItemId = newEquip;
	}

	protected static void InvokeUserCode_CmdEquipNewItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipNewItem called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdEquipNewItem(reader.ReadInt());
		}
	}

	protected void UserCode_CmdUsingItem(bool isUsing)
	{
		NetworkusingItem = isUsing;
	}

	protected static void InvokeUserCode_CmdUsingItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUsingItem called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdUsingItem(reader.ReadBool());
		}
	}

	protected void UserCode_CmdChangeHairColour(int newHair)
	{
		NetworkhairColor = newHair;
	}

	protected static void InvokeUserCode_CmdChangeHairColour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHairColour called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeHairColour(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeEyeColour(int newEye)
	{
		NetworkeyeId = newEye;
	}

	protected static void InvokeUserCode_CmdChangeEyeColour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeEyeColour called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeEyeColour(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendEquipedClothes(int[] clothesArray)
	{
		NetworkheadId = clothesArray[0];
		NetworkshirtId = clothesArray[1];
		NetworkpantsId = clothesArray[2];
		NetworkshoeId = clothesArray[3];
	}

	protected static void InvokeUserCode_CmdSendEquipedClothes(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendEquipedClothes called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSendEquipedClothes(GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_CmdChangeNose(int newNose)
	{
		NetworknoseId = newNose;
	}

	protected static void InvokeUserCode_CmdChangeNose(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeNose called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeNose(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeMouth(int newMouth)
	{
		NetworkmouthId = newMouth;
	}

	protected static void InvokeUserCode_CmdChangeMouth(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeMouth called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeMouth(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendName(string setPlayerName, int setIslandId)
	{
		NetworkplayerName = setPlayerName;
		islandId = setIslandId;
		myPermissions = UserPermissions.Instance.GetPermissions(setPlayerName, setIslandId, base.isLocalPlayer);
	}

	protected static void InvokeUserCode_CmdSendName(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendName called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSendName(reader.ReadString(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdMakeHairDresserSpin()
	{
		NetworkMapSharer.Instance.RpcSpinChair();
	}

	protected static void InvokeUserCode_CmdMakeHairDresserSpin(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMakeHairDresserSpin called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdMakeHairDresserSpin();
		}
	}

	protected void UserCode_CmdCallforBugNet(int netId)
	{
		RpcPutBugNetInHand(netId);
	}

	protected static void InvokeUserCode_CmdCallforBugNet(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCallforBugNet called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdCallforBugNet(reader.ReadInt());
		}
	}

	protected void UserCode_CmdOpenBag()
	{
		NetworkbagOpenEmoteOn = true;
	}

	protected static void InvokeUserCode_CmdOpenBag(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenBag called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdOpenBag();
		}
	}

	protected void UserCode_CmdCloseBag()
	{
		NetworkbagOpenEmoteOn = false;
	}

	protected static void InvokeUserCode_CmdCloseBag(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCloseBag called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdCloseBag();
		}
	}

	protected void UserCode_CmdSetDisguise(int newDisguise)
	{
		NetworkdisguiseId = newDisguise;
	}

	protected static void InvokeUserCode_CmdSetDisguise(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDisguise called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSetDisguise(reader.ReadInt());
		}
	}

	protected void UserCode_RpcPutBugNetInHand(int netId)
	{
		if (!base.isLocalPlayer)
		{
			StartCoroutine(bugNetWait(netId));
		}
	}

	protected static void InvokeUserCode_RpcPutBugNetInHand(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPutBugNetInHand called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcPutBugNetInHand(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeLookableForAiming(Vector3 newPos)
	{
		RpcMoveLookableForRanged(newPos);
	}

	protected static void InvokeUserCode_CmdChangeLookableForAiming(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeLookableForAiming called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeLookableForAiming(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcMoveLookableForRanged(Vector3 newPos)
	{
		aimLookablePos = newPos;
	}

	protected static void InvokeUserCode_RpcMoveLookableForRanged(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveLookableForRanged called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcMoveLookableForRanged(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdFireProjectileAtDir(Vector3 spawnPos, Vector3 direction, float strength, int projectileId)
	{
		RpcFireAtAngle(spawnPos, direction, strength, projectileId);
	}

	protected static void InvokeUserCode_CmdFireProjectileAtDir(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFireProjectileAtDir called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdFireProjectileAtDir(reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSpawnProjectileObject(int itemId, Vector3 spawnPos, Vector3 direction, float strength, int invId)
	{
		ProjectileSpawn component = Object.Instantiate(Inventory.Instance.allItems[itemId].itemPrefab.GetComponent<RangedWeapon>().spawnAndFire, spawnPos, Quaternion.identity).GetComponent<ProjectileSpawn>();
		component.SetInvItem(invId);
		component.SyncDirectionAndSpeed(direction, strength);
		NetworkServer.Spawn(component.gameObject, base.connectionToClient);
	}

	protected static void InvokeUserCode_CmdSpawnProjectileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnProjectileObject called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSpawnProjectileObject(reader.ReadInt(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcFireAtAngle(Vector3 spawnPos, Vector3 forward, float strength, int projectileId)
	{
		Object.Instantiate(NetworkMapSharer.Instance.projectile, spawnPos, holdPos.rotation).GetComponent<Projectile>().SetUpProjectile(projectileId, base.transform, forward, strength);
	}

	protected static void InvokeUserCode_RpcFireAtAngle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFireAtAngle called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcFireAtAngle(reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetNewCompScore(float newScore)
	{
		NetworkcompScore = newScore;
		CatchingCompetitionManager.manage.updateCurrentLeader();
	}

	protected static void InvokeUserCode_CmdSetNewCompScore(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNewCompScore called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSetNewCompScore(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdChangeSize(Vector3 newSize)
	{
		Networksize = newSize;
	}

	protected static void InvokeUserCode_CmdChangeSize(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeSize called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeSize(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdSetBagStatus(bool newHasBag)
	{
		NetworkhasBag = newHasBag;
	}

	protected static void InvokeUserCode_CmdSetBagStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetBagStatus called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSetBagStatus(reader.ReadBool());
		}
	}

	protected void UserCode_CmdChangeBagColour(int newId)
	{
		NetworkbagColour = newId;
	}

	protected static void InvokeUserCode_CmdChangeBagColour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeBagColour called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeBagColour(reader.ReadInt());
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(currentlyHoldingItemId);
			writer.WriteString(playerName);
			writer.WriteBool(usingItem);
			writer.WriteBool(blocking);
			writer.WriteInt(hairColor);
			writer.WriteInt(faceId);
			writer.WriteInt(headId);
			writer.WriteInt(hairId);
			writer.WriteInt(shirtId);
			writer.WriteInt(pantsId);
			writer.WriteInt(shoeId);
			writer.WriteInt(eyeId);
			writer.WriteInt(eyeColor);
			writer.WriteInt(skinId);
			writer.WriteInt(noseId);
			writer.WriteInt(mouthId);
			writer.WriteBool(bagOpenEmoteOn);
			writer.WriteBool(hasBag);
			writer.WriteInt(bagColour);
			writer.WriteInt(disguiseId);
			writer.WriteFloat(compScore);
			writer.WriteVector3(size);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(currentlyHoldingItemId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteString(playerName);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(usingItem);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(blocking);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteInt(hairColor);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteInt(faceId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteInt(headId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteInt(hairId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteInt(shirtId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteInt(pantsId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteInt(shoeId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteInt(eyeId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteInt(eyeColor);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteInt(skinId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteInt(noseId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteInt(mouthId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteBool(bagOpenEmoteOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x20000L) != 0L)
		{
			writer.WriteBool(hasBag);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x40000L) != 0L)
		{
			writer.WriteInt(bagColour);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x80000L) != 0L)
		{
			writer.WriteInt(disguiseId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x100000L) != 0L)
		{
			writer.WriteFloat(compScore);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x200000L) != 0L)
		{
			writer.WriteVector3(size);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = currentlyHoldingItemId;
			NetworkcurrentlyHoldingItemId = reader.ReadInt();
			if (!SyncVarEqual(num, ref currentlyHoldingItemId))
			{
				equipNewItemNetwork(num, currentlyHoldingItemId);
			}
			string text = playerName;
			NetworkplayerName = reader.ReadString();
			if (!SyncVarEqual(text, ref playerName))
			{
				onChangeName(text, playerName);
			}
			bool flag = usingItem;
			NetworkusingItem = reader.ReadBool();
			bool flag2 = blocking;
			Networkblocking = reader.ReadBool();
			int num2 = hairColor;
			NetworkhairColor = reader.ReadInt();
			if (!SyncVarEqual(num2, ref hairColor))
			{
				onHairColourChange(num2, hairColor);
			}
			int num3 = faceId;
			NetworkfaceId = reader.ReadInt();
			if (!SyncVarEqual(num3, ref faceId))
			{
				onFaceChange(num3, faceId);
			}
			int num4 = headId;
			NetworkheadId = reader.ReadInt();
			if (!SyncVarEqual(num4, ref headId))
			{
				onHeadChange(num4, headId);
			}
			int num5 = hairId;
			NetworkhairId = reader.ReadInt();
			if (!SyncVarEqual(num5, ref hairId))
			{
				onHairChange(num5, hairId);
			}
			int num6 = shirtId;
			NetworkshirtId = reader.ReadInt();
			if (!SyncVarEqual(num6, ref shirtId))
			{
				onChangeShirt(num6, shirtId);
			}
			int num7 = pantsId;
			NetworkpantsId = reader.ReadInt();
			if (!SyncVarEqual(num7, ref pantsId))
			{
				onChangePants(num7, pantsId);
			}
			int num8 = shoeId;
			NetworkshoeId = reader.ReadInt();
			if (!SyncVarEqual(num8, ref shoeId))
			{
				onChangeShoes(num8, shoeId);
			}
			int num9 = eyeId;
			NetworkeyeId = reader.ReadInt();
			if (!SyncVarEqual(num9, ref eyeId))
			{
				onChangeEyes(num9, eyeId);
			}
			int num10 = eyeColor;
			NetworkeyeColor = reader.ReadInt();
			if (!SyncVarEqual(num10, ref eyeColor))
			{
				onChangeEyeColor(num10, eyeColor);
			}
			int num11 = skinId;
			NetworkskinId = reader.ReadInt();
			if (!SyncVarEqual(num11, ref skinId))
			{
				onChangeSkin(num11, skinId);
			}
			int num12 = noseId;
			NetworknoseId = reader.ReadInt();
			if (!SyncVarEqual(num12, ref noseId))
			{
				onNoseChange(num12, noseId);
			}
			int num13 = mouthId;
			NetworkmouthId = reader.ReadInt();
			if (!SyncVarEqual(num13, ref mouthId))
			{
				onMouthChange(num13, mouthId);
			}
			bool flag3 = bagOpenEmoteOn;
			NetworkbagOpenEmoteOn = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref bagOpenEmoteOn))
			{
				changeOpenBag(flag3, bagOpenEmoteOn);
			}
			bool flag4 = hasBag;
			NetworkhasBag = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref hasBag))
			{
				ChangeHasBag(flag4, hasBag);
			}
			int num14 = bagColour;
			NetworkbagColour = reader.ReadInt();
			if (!SyncVarEqual(num14, ref bagColour))
			{
				OnChangeBagColour(num14, bagColour);
			}
			int num15 = disguiseId;
			NetworkdisguiseId = reader.ReadInt();
			if (!SyncVarEqual(num15, ref disguiseId))
			{
				OnDisguiseChange(num15, disguiseId);
			}
			float num16 = compScore;
			NetworkcompScore = reader.ReadFloat();
			Vector3 vector = size;
			Networksize = reader.ReadVector3();
			if (!SyncVarEqual(vector, ref size))
			{
				OnSizeChange(vector, size);
			}
			return;
		}
		long num17 = (long)reader.ReadULong();
		if ((num17 & 1L) != 0L)
		{
			int num18 = currentlyHoldingItemId;
			NetworkcurrentlyHoldingItemId = reader.ReadInt();
			if (!SyncVarEqual(num18, ref currentlyHoldingItemId))
			{
				equipNewItemNetwork(num18, currentlyHoldingItemId);
			}
		}
		if ((num17 & 2L) != 0L)
		{
			string text2 = playerName;
			NetworkplayerName = reader.ReadString();
			if (!SyncVarEqual(text2, ref playerName))
			{
				onChangeName(text2, playerName);
			}
		}
		if ((num17 & 4L) != 0L)
		{
			bool flag5 = usingItem;
			NetworkusingItem = reader.ReadBool();
		}
		if ((num17 & 8L) != 0L)
		{
			bool flag6 = blocking;
			Networkblocking = reader.ReadBool();
		}
		if ((num17 & 0x10L) != 0L)
		{
			int num19 = hairColor;
			NetworkhairColor = reader.ReadInt();
			if (!SyncVarEqual(num19, ref hairColor))
			{
				onHairColourChange(num19, hairColor);
			}
		}
		if ((num17 & 0x20L) != 0L)
		{
			int num20 = faceId;
			NetworkfaceId = reader.ReadInt();
			if (!SyncVarEqual(num20, ref faceId))
			{
				onFaceChange(num20, faceId);
			}
		}
		if ((num17 & 0x40L) != 0L)
		{
			int num21 = headId;
			NetworkheadId = reader.ReadInt();
			if (!SyncVarEqual(num21, ref headId))
			{
				onHeadChange(num21, headId);
			}
		}
		if ((num17 & 0x80L) != 0L)
		{
			int num22 = hairId;
			NetworkhairId = reader.ReadInt();
			if (!SyncVarEqual(num22, ref hairId))
			{
				onHairChange(num22, hairId);
			}
		}
		if ((num17 & 0x100L) != 0L)
		{
			int num23 = shirtId;
			NetworkshirtId = reader.ReadInt();
			if (!SyncVarEqual(num23, ref shirtId))
			{
				onChangeShirt(num23, shirtId);
			}
		}
		if ((num17 & 0x200L) != 0L)
		{
			int num24 = pantsId;
			NetworkpantsId = reader.ReadInt();
			if (!SyncVarEqual(num24, ref pantsId))
			{
				onChangePants(num24, pantsId);
			}
		}
		if ((num17 & 0x400L) != 0L)
		{
			int num25 = shoeId;
			NetworkshoeId = reader.ReadInt();
			if (!SyncVarEqual(num25, ref shoeId))
			{
				onChangeShoes(num25, shoeId);
			}
		}
		if ((num17 & 0x800L) != 0L)
		{
			int num26 = eyeId;
			NetworkeyeId = reader.ReadInt();
			if (!SyncVarEqual(num26, ref eyeId))
			{
				onChangeEyes(num26, eyeId);
			}
		}
		if ((num17 & 0x1000L) != 0L)
		{
			int num27 = eyeColor;
			NetworkeyeColor = reader.ReadInt();
			if (!SyncVarEqual(num27, ref eyeColor))
			{
				onChangeEyeColor(num27, eyeColor);
			}
		}
		if ((num17 & 0x2000L) != 0L)
		{
			int num28 = skinId;
			NetworkskinId = reader.ReadInt();
			if (!SyncVarEqual(num28, ref skinId))
			{
				onChangeSkin(num28, skinId);
			}
		}
		if ((num17 & 0x4000L) != 0L)
		{
			int num29 = noseId;
			NetworknoseId = reader.ReadInt();
			if (!SyncVarEqual(num29, ref noseId))
			{
				onNoseChange(num29, noseId);
			}
		}
		if ((num17 & 0x8000L) != 0L)
		{
			int num30 = mouthId;
			NetworkmouthId = reader.ReadInt();
			if (!SyncVarEqual(num30, ref mouthId))
			{
				onMouthChange(num30, mouthId);
			}
		}
		if ((num17 & 0x10000L) != 0L)
		{
			bool flag7 = bagOpenEmoteOn;
			NetworkbagOpenEmoteOn = reader.ReadBool();
			if (!SyncVarEqual(flag7, ref bagOpenEmoteOn))
			{
				changeOpenBag(flag7, bagOpenEmoteOn);
			}
		}
		if ((num17 & 0x20000L) != 0L)
		{
			bool flag8 = hasBag;
			NetworkhasBag = reader.ReadBool();
			if (!SyncVarEqual(flag8, ref hasBag))
			{
				ChangeHasBag(flag8, hasBag);
			}
		}
		if ((num17 & 0x40000L) != 0L)
		{
			int num31 = bagColour;
			NetworkbagColour = reader.ReadInt();
			if (!SyncVarEqual(num31, ref bagColour))
			{
				OnChangeBagColour(num31, bagColour);
			}
		}
		if ((num17 & 0x80000L) != 0L)
		{
			int num32 = disguiseId;
			NetworkdisguiseId = reader.ReadInt();
			if (!SyncVarEqual(num32, ref disguiseId))
			{
				OnDisguiseChange(num32, disguiseId);
			}
		}
		if ((num17 & 0x100000L) != 0L)
		{
			float num33 = compScore;
			NetworkcompScore = reader.ReadFloat();
		}
		if ((num17 & 0x200000L) != 0L)
		{
			Vector3 vector2 = size;
			Networksize = reader.ReadVector3();
			if (!SyncVarEqual(vector2, ref size))
			{
				OnSizeChange(vector2, size);
			}
		}
	}
}
