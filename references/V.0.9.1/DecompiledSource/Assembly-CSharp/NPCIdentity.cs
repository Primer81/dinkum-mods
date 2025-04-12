using System.Runtime.InteropServices;
using Mirror;
using UnityChan;
using UnityEngine;

public class NPCIdentity : NetworkBehaviour
{
	[SyncVar]
	public int NPCNo;

	public Transform onHeadPos;

	public EyesScript eyes;

	public MeshFilter noseMesh;

	public MeshRenderer[] eyeRenderers;

	public SkinnedMeshRenderer playerSkin;

	public SkinnedMeshRenderer shirtRen;

	public SkinnedMeshRenderer pantsRen;

	public SkinnedMeshRenderer shoeRen;

	public RuntimeAnimatorController defaultAnim;

	private GameObject onHead;

	private GameObject hatOnHead;

	private GameObject faceOnHead;

	public Transform baseTransform;

	public Transform headTransform;

	[SyncVar(hook = "SetHatOnHead")]
	public int wearingOnHead = -1;

	[SyncVar(hook = "SetFaceOnHead")]
	public int wearingOnFace = -1;

	[SyncVar(hook = "OnChangeTop")]
	public int wearingTop = -1;

	public SkinnedMeshRenderer[] NPCDresses;

	public int NetworkNPCNo
	{
		get
		{
			return NPCNo;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref NPCNo))
			{
				int nPCNo = NPCNo;
				SetSyncVar(value, ref NPCNo, 1uL);
			}
		}
	}

	public int NetworkwearingOnHead
	{
		get
		{
			return wearingOnHead;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref wearingOnHead))
			{
				int oldHat = wearingOnHead;
				SetSyncVar(value, ref wearingOnHead, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					SetHatOnHead(oldHat, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public int NetworkwearingOnFace
	{
		get
		{
			return wearingOnFace;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref wearingOnFace))
			{
				int oldFace = wearingOnFace;
				SetSyncVar(value, ref wearingOnFace, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					SetFaceOnHead(oldFace, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public int NetworkwearingTop
	{
		get
		{
			return wearingTop;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref wearingTop))
			{
				int oldTop = wearingTop;
				SetSyncVar(value, ref wearingTop, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, value: true);
					OnChangeTop(oldTop, value);
					setSyncVarHookGuard(8uL, value: false);
				}
			}
		}
	}

	private void Start()
	{
	}

	public override void OnStartClient()
	{
		changeNPCAndEquip(NPCNo);
	}

	public override void OnStartServer()
	{
		changeNPCAndEquip(NPCNo);
		if (NPCNo > 0)
		{
			GetComponent<NPCAI>().myManager = NPCManager.manage.getNPCMapAgentForNPC(NPCNo);
			GetComponent<NPCAI>().myAgent.avoidancePriority = NPCNo;
		}
	}

	public void changeNPCAndEquip(int newId)
	{
		if (newId == -1)
		{
			return;
		}
		NetworkNPCNo = newId;
		if (!NetworkMapSharer.Instance.isServer && NPCManager.manage.NPCDetails[newId].isAVillager && !NPCManager.manage.npcInvs[newId].hasBeenRequested)
		{
			return;
		}
		if (NPCManager.manage.NPCDetails[NPCNo].isAVillager)
		{
			NPCManager.manage.NPCDetails[NPCNo].SetRandomName(NPCManager.manage.npcInvs[NPCNo].nameId);
			NPCManager.manage.NPCDetails[NPCNo].SetVoiceGender(NPCManager.manage.npcInvs[NPCNo].isFem);
		}
		if ((bool)NPCManager.manage.NPCDetails[NPCNo].npcMesh)
		{
			playerSkin.sharedMesh = NPCManager.manage.NPCDetails[NPCNo].npcMesh;
		}
		else
		{
			playerSkin.sharedMesh = NPCManager.manage.defaultNpcMesh;
		}
		GetComponent<NPCJob>().NetworkvendorId = (int)NPCManager.manage.NPCDetails[newId].workLocation;
		if ((bool)NPCManager.manage.NPCDetails[newId].animationOverrride)
		{
			if (defaultAnim == null)
			{
				defaultAnim = GetComponent<Animator>().runtimeAnimatorController;
			}
			GetComponent<Animator>().runtimeAnimatorController = NPCManager.manage.NPCDetails[newId].animationOverrride;
		}
		else if (defaultAnim != null)
		{
			GetComponent<Animator>().runtimeAnimatorController = defaultAnim;
		}
		if (newId != 5 && (bool)GetComponent<AudioEchoFilter>())
		{
			Object.Destroy(GetComponent<AudioEchoFilter>());
		}
		else if (newId == 5 && !GetComponent<AudioEchoFilter>())
		{
			AudioEchoFilter audioEchoFilter = base.gameObject.AddComponent<AudioEchoFilter>();
			audioEchoFilter.delay = 10f;
			audioEchoFilter.decayRatio = 0.001f;
			audioEchoFilter.dryMix = 1f;
			audioEchoFilter.wetMix = 0.75f;
		}
		setUpNPCHead();
		setUpNPCMaterials();
	}

	public GameObject GetHairObject()
	{
		return onHead;
	}

	private void setUpNPCHead()
	{
		if (NPCManager.manage.NPCDetails[NPCNo].isAVillager)
		{
			int hairId = NPCManager.manage.npcInvs[NPCNo].hairId;
			GameObject gameObject = CharacterCreatorScript.create.allHairStyles[hairId];
			if (onHead != gameObject)
			{
				Object.Destroy(onHead);
				onHead = Object.Instantiate(gameObject, onHeadPos);
				onHead.transform.localPosition = Vector3.zero;
				onHead.layer = LayerMask.NameToLayer("NPC");
				Color hairColour = CharacterCreatorScript.create.getHairColour(NPCManager.manage.npcInvs[NPCNo].hairColorId);
				MeshRenderer[] componentsInChildren = onHead.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material.color = hairColour;
				}
				SkinnedMeshRenderer[] componentsInChildren2 = onHead.GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].material.color = hairColour;
				}
				Transform[] componentsInChildren3 = onHead.GetComponentsInChildren<Transform>();
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					componentsInChildren3[i].gameObject.layer = LayerMask.NameToLayer("NPC");
				}
			}
			eyes.setMouthMesh(null);
			eyes.changeMouthMat(CharacterCreatorScript.create.mouthTypes[NPCManager.manage.npcInvs[NPCNo].mouthId], CharacterCreatorScript.create.skinTones[NPCManager.manage.npcInvs[NPCNo].skinId].color);
			noseMesh.mesh = CharacterCreatorScript.create.noseMeshes[NPCManager.manage.npcInvs[NPCNo].noseId];
			return;
		}
		if (onHead != NPCManager.manage.NPCDetails[NPCNo].NpcHair)
		{
			Object.Destroy(onHead);
			onHead = Object.Instantiate(NPCManager.manage.NPCDetails[NPCNo].NpcHair, onHeadPos);
			SpringManager componentInChildren = onHead.GetComponentInChildren<SpringManager>();
			if ((bool)componentInChildren)
			{
				GetComponent<NPCAI>().hairSpring = componentInChildren;
			}
			onHead.transform.localPosition = Vector3.zero;
			onHead.layer = LayerMask.NameToLayer("NPC");
			Transform[] componentsInChildren3 = onHead.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].gameObject.layer = LayerMask.NameToLayer("NPC");
			}
			eyes.faceMove = onHead.GetComponentInChildren<FaceItemMoveWithMouth>();
		}
		if (NPCManager.manage.NPCDetails[NPCNo].nose == -1)
		{
			noseMesh.mesh = null;
		}
		else
		{
			noseMesh.mesh = CharacterCreatorScript.create.noseMeshes[NPCManager.manage.NPCDetails[NPCNo].nose];
		}
		eyes.setMouthMesh(NPCManager.manage.NPCDetails[NPCNo].insideMouthMesh);
	}

	public void SetHatOnHead(int oldHat, int newHat)
	{
		NetworkwearingOnHead = oldHat;
		if ((bool)hatOnHead)
		{
			Object.Destroy(hatOnHead);
		}
		if (newHat != -1)
		{
			hatOnHead = Object.Instantiate(Inventory.Instance.allItems[newHat].equipable.hatPrefab, onHeadPos);
			hatOnHead.transform.localPosition = Vector3.zero;
			hatOnHead.transform.localRotation = Quaternion.identity;
			hatOnHead.transform.localScale = NPCManager.manage.NPCDetails[NPCNo].ClothingDetails.headSize;
			SetItemTexture component = hatOnHead.GetComponent<SetItemTexture>();
			if ((bool)component)
			{
				component.setTexture(Inventory.Instance.allItems[newHat]);
			}
			if ((bool)onHead && Inventory.Instance.allItems[newHat].equipable.useRegularHair)
			{
				onHead.transform.Find("Hair").gameObject.SetActive(value: true);
				onHead.transform.Find("Hair_Hat").gameObject.SetActive(value: false);
			}
			else if ((bool)onHead && Inventory.Instance.allItems[newHat].equipable.hideHair)
			{
				hatOnHead.transform.localPosition = NPCManager.manage.NPCDetails[NPCNo].ClothingDetails.hoodPosition;
				onHead.transform.Find("Hair").gameObject.SetActive(value: false);
				onHead.transform.Find("Hair_Hat").gameObject.SetActive(value: false);
			}
			else if ((bool)onHead)
			{
				onHead.transform.Find("Hair").gameObject.SetActive(value: false);
				onHead.transform.Find("Hair_Hat").gameObject.SetActive(value: true);
			}
		}
		else if ((bool)onHead)
		{
			onHead.transform.Find("Hair").gameObject.SetActive(value: true);
			onHead.transform.Find("Hair_Hat").gameObject.SetActive(value: false);
		}
	}

	public void SetFaceOnHead(int oldFace, int newFace)
	{
		NetworkwearingOnFace = newFace;
		if ((bool)faceOnHead)
		{
			Object.Destroy(faceOnHead);
		}
		if (newFace != -1)
		{
			Transform transform = onHead.transform.Find("OnFaceDefault");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: false);
			}
			faceOnHead = Object.Instantiate(Inventory.Instance.allItems[newFace].equipable.hatPrefab, onHeadPos);
			faceOnHead.transform.localPosition = Vector3.zero;
			faceOnHead.transform.localRotation = Quaternion.identity;
			faceOnHead.transform.localScale = NPCManager.manage.NPCDetails[NPCNo].ClothingDetails.headSize;
			SetItemTexture component = faceOnHead.GetComponent<SetItemTexture>();
			if ((bool)component)
			{
				component.setTexture(Inventory.Instance.allItems[newFace]);
			}
		}
		else
		{
			Transform transform2 = onHead.transform.Find("OnFaceDefault");
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(value: true);
			}
		}
	}

	public void OnChangeTop(int oldTop, int newTop)
	{
		NetworkwearingTop = newTop;
		setUpNPCMaterials();
	}

	private void setUpNPCMaterials()
	{
		if (NPCManager.manage.NPCDetails[NPCNo].isAVillager)
		{
			playerSkin.material = CharacterCreatorScript.create.skinTones[NPCManager.manage.npcInvs[NPCNo].skinId];
			int eyesId = NPCManager.manage.npcInvs[NPCNo].eyesId;
			int eyeColorId = NPCManager.manage.npcInvs[NPCNo].eyeColorId;
			eyes.changeEyeMat(CharacterCreatorScript.create.allEyeTypes[eyesId], playerSkin.material.color);
			eyes.changeEyeColor(CharacterCreatorScript.create.eyeColours[eyeColorId]);
			InventoryItem inventoryItem = Inventory.Instance.allItems[NPCManager.manage.npcInvs[NPCNo].shirtId];
			if ((bool)inventoryItem.equipable.shirtMesh)
			{
				shirtRen.sharedMesh = inventoryItem.equipable.shirtMesh;
			}
			else
			{
				shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
			}
			InventoryItem inventoryItem2 = Inventory.Instance.allItems[NPCManager.manage.npcInvs[NPCNo].pantsId];
			if ((bool)inventoryItem2 && (bool)inventoryItem2.equipable.useAltMesh)
			{
				pantsRen.sharedMesh = inventoryItem2.equipable.useAltMesh;
			}
			else
			{
				pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
			}
			InventoryItem inventoryItem3 = Inventory.Instance.allItems[NPCManager.manage.npcInvs[NPCNo].shoesId];
			if ((bool)inventoryItem3 && (bool)inventoryItem3.equipable.useAltMesh)
			{
				shoeRen.sharedMesh = inventoryItem3.equipable.useAltMesh;
			}
			else
			{
				shoeRen.sharedMesh = EquipWindow.equip.defualtShoeMesh;
			}
			equipRenderer(inventoryItem, shirtRen);
			equipRenderer(inventoryItem2, pantsRen);
			equipRenderer(inventoryItem3, shoeRen);
			return;
		}
		playerSkin.material = NPCManager.manage.NPCDetails[NPCNo].NpcSkin;
		eyes.changeEyeMat(NPCManager.manage.NPCDetails[NPCNo].NpcEyes, playerSkin.material.color);
		eyes.changeMouthMat(NPCManager.manage.NPCDetails[NPCNo].NPCMouth, playerSkin.material.color);
		eyes.changeEyeColor(NPCManager.manage.NPCDetails[NPCNo].NpcEyesColor);
		if (NPCManager.manage.NPCDetails[NPCNo].MyDress != 0)
		{
			SetUpDress();
			shirtRen.enabled = false;
		}
		else
		{
			SetUpDress();
			shirtRen.enabled = true;
		}
		if ((bool)NPCManager.manage.NPCDetails[NPCNo].NPCShirt.equipable.shirtMesh)
		{
			shirtRen.sharedMesh = NPCManager.manage.NPCDetails[NPCNo].NPCShirt.equipable.shirtMesh;
		}
		else
		{
			shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
		}
		if ((bool)NPCManager.manage.NPCDetails[NPCNo].NPCPants.equipable.useAltMesh)
		{
			pantsRen.sharedMesh = NPCManager.manage.NPCDetails[NPCNo].NPCPants.equipable.useAltMesh;
		}
		else
		{
			pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
		}
		InventoryItem nPCShoes = NPCManager.manage.NPCDetails[NPCNo].NPCShoes;
		if ((bool)nPCShoes && (bool)nPCShoes.equipable.useAltMesh)
		{
			shoeRen.sharedMesh = nPCShoes.equipable.useAltMesh;
		}
		else
		{
			shoeRen.sharedMesh = EquipWindow.equip.defualtShoeMesh;
		}
		if (wearingTop != -1)
		{
			if (!IsWearingDress())
			{
				equipRenderer(Inventory.Instance.allItems[wearingTop], shirtRen);
			}
			else
			{
				equipRenderer(Inventory.Instance.allItems[wearingTop], GetDressRenderer());
			}
		}
		else if (!IsWearingDress())
		{
			equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCShirt, shirtRen);
		}
		else
		{
			equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCShirt, GetDressRenderer());
		}
		equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCPants, pantsRen);
		equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCShoes, shoeRen);
		if (NPCManager.manage.NPCDetails[NPCNo].isChild)
		{
			baseTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			headTransform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
		}
		else
		{
			baseTransform.localScale = Vector3.one;
			headTransform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		}
	}

	private void equipRenderer(InventoryItem invClothItem, SkinnedMeshRenderer render)
	{
		if (invClothItem != null)
		{
			render.gameObject.SetActive(value: true);
			render.material = invClothItem.equipable.material;
		}
		else
		{
			render.gameObject.SetActive(value: false);
		}
	}

	public void SetUpDress()
	{
		int num = (int)(NPCManager.manage.NPCDetails[NPCNo].MyDress - 1);
		for (int i = 0; i < NPCDresses.Length; i++)
		{
			NPCDresses[i].gameObject.SetActive(i == num);
		}
	}

	public SkinnedMeshRenderer GetDressRenderer()
	{
		int num = (int)(NPCManager.manage.NPCDetails[NPCNo].MyDress - 1);
		return NPCDresses[num];
	}

	private bool IsWearingDress()
	{
		return NPCManager.manage.NPCDetails[NPCNo].MyDress != NPCDetails.DressStyle.None;
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(NPCNo);
			writer.WriteInt(wearingOnHead);
			writer.WriteInt(wearingOnFace);
			writer.WriteInt(wearingTop);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(NPCNo);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(wearingOnHead);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(wearingOnFace);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(wearingTop);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int nPCNo = NPCNo;
			NetworkNPCNo = reader.ReadInt();
			int num = wearingOnHead;
			NetworkwearingOnHead = reader.ReadInt();
			if (!SyncVarEqual(num, ref wearingOnHead))
			{
				SetHatOnHead(num, wearingOnHead);
			}
			int num2 = wearingOnFace;
			NetworkwearingOnFace = reader.ReadInt();
			if (!SyncVarEqual(num2, ref wearingOnFace))
			{
				SetFaceOnHead(num2, wearingOnFace);
			}
			int num3 = wearingTop;
			NetworkwearingTop = reader.ReadInt();
			if (!SyncVarEqual(num3, ref wearingTop))
			{
				OnChangeTop(num3, wearingTop);
			}
			return;
		}
		long num4 = (long)reader.ReadULong();
		if ((num4 & 1L) != 0L)
		{
			int nPCNo2 = NPCNo;
			NetworkNPCNo = reader.ReadInt();
		}
		if ((num4 & 2L) != 0L)
		{
			int num5 = wearingOnHead;
			NetworkwearingOnHead = reader.ReadInt();
			if (!SyncVarEqual(num5, ref wearingOnHead))
			{
				SetHatOnHead(num5, wearingOnHead);
			}
		}
		if ((num4 & 4L) != 0L)
		{
			int num6 = wearingOnFace;
			NetworkwearingOnFace = reader.ReadInt();
			if (!SyncVarEqual(num6, ref wearingOnFace))
			{
				SetFaceOnHead(num6, wearingOnFace);
			}
		}
		if ((num4 & 8L) != 0L)
		{
			int num7 = wearingTop;
			NetworkwearingTop = reader.ReadInt();
			if (!SyncVarEqual(num7, ref wearingTop))
			{
				OnChangeTop(num7, wearingTop);
			}
		}
	}
}
