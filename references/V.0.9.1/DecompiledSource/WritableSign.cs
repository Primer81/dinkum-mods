using System.Collections;
using TMPro;
using UnityEngine;

public class WritableSign : MonoBehaviour
{
	public TextMeshPro signText;

	public TextMeshPro otherSide;

	public GameObject roofToScale;

	public Transform toSpin;

	public float currentSpinSetTo;

	public bool spinOnZ = true;

	[Header("Fish Pond and terrarium")]
	public bool isAnimalTank;

	public GameObject[] fishObjects;

	public SetItemTexture[] fishRens;

	public BugExhibit[] bugExhibits;

	[Header("Is mannequin")]
	public bool isMannequin;

	public SkinnedMeshRenderer shirt;

	public SkinnedMeshRenderer pants;

	public SkinnedMeshRenderer shoes;

	public Transform headPos;

	private GameObject showingOnHead;

	private GameObject showingOnFace;

	[Header("Is tool rack")]
	public bool isToolRack;

	public Transform toolPos1;

	public Transform toolPos2;

	public Transform toolPos3;

	public bool isDisplayCase;

	private string displayingText = "-1";

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isWritableSign = this;
	}

	public void updateSignText(int xPos, int yPos, int houseX, int houseY)
	{
		string text = SignManager.manage.getSignText(xPos, yPos, houseX, houseY);
		if (displayingText == text)
		{
			return;
		}
		displayingText = text;
		if (isDisplayCase)
		{
			if (text == "")
			{
				text = "<-1> <-1>";
			}
			string[] upDisplayCase = text.Split(' ');
			SetUpDisplayCase(upDisplayCase);
		}
		else if (isToolRack)
		{
			if (text == "")
			{
				text = "<-1> <-1> <-1>";
			}
			string[] upToolRack = text.Split(' ');
			SetUpToolRack(upToolRack);
		}
		else if (isMannequin)
		{
			if (text == "")
			{
				text = "<-1> <-1> <-1> <-1> <-1>";
			}
			string[] upMannequin = text.Split(' ');
			SetUpMannequin(upMannequin);
		}
		else if (isAnimalTank)
		{
			if (text == "")
			{
				text = "<-1> <-1> <-1> <-1> <-1>";
			}
			string[] array = text.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				ShowACreature(i, array[i]);
			}
		}
		else if ((bool)toSpin)
		{
			float result = 0f;
			float.TryParse(text, out result);
			if (currentSpinSetTo != result)
			{
				StopAllCoroutines();
				currentSpinSetTo = result;
				if (spinOnZ)
				{
					StartCoroutine(SpinToZAngle(result));
				}
				else
				{
					StartCoroutine(SpinToYAngle(result));
				}
			}
		}
		else if ((bool)roofToScale)
		{
			string text2 = text;
			if (text2 != null && text2 != "")
			{
				string[] array2 = text2.Split('x');
				int[] array3 = new int[3];
				int.TryParse(array2[0], out array3[0]);
				int.TryParse(array2[1], out array3[1]);
				int.TryParse(array2[2], out array3[2]);
				roofToScale.transform.localScale = new Vector3(array3[0], 4f, array3[1]);
			}
		}
		else
		{
			signText.text = text;
			if ((bool)otherSide)
			{
				otherSide.text = text;
			}
		}
	}

	public void editSign()
	{
		TileObject componentInParent = GetComponentInParent<TileObject>();
		if ((bool)toSpin)
		{
			SignManager.manage.spinSignWheel(componentInParent.xPos, componentInParent.yPos);
		}
		else
		{
			SignManager.manage.openSignWritingWindow(componentInParent.xPos, componentInParent.yPos);
		}
	}

	private IEnumerator SpinToZAngle(float angle)
	{
		float totalSpinTime = 4f;
		ASound component = GetComponent<ASound>();
		if ((bool)component)
		{
			StartCoroutine(makeTickSounds(component, totalSpinTime));
		}
		float elapsedTime = 0f;
		float currentAngle = toSpin.localEulerAngles.z;
		float num = (angle - currentAngle + 360f) % 360f;
		if (num < 180f)
		{
			num += 360f;
		}
		int num2 = Mathf.FloorToInt(totalSpinTime);
		num += 360f * (float)num2;
		float finalAngle = currentAngle + num;
		while (elapsedTime < totalSpinTime)
		{
			elapsedTime += Time.deltaTime;
			float num3 = elapsedTime / totalSpinTime;
			float num4 = Mathf.Lerp(currentAngle, finalAngle, num3 * num3 * (3f - 2f * num3));
			toSpin.localEulerAngles = new Vector3(0f, 0f, num4 % 360f);
			yield return null;
		}
		toSpin.localEulerAngles = new Vector3(0f, 0f, angle);
	}

	private IEnumerator SpinToYAngle(float angle)
	{
		float totalSpinTime = 4f;
		ASound component = GetComponent<ASound>();
		if ((bool)component)
		{
			StartCoroutine(makeTickSounds(component, totalSpinTime));
		}
		float elapsedTime = 0f;
		float currentAngle = toSpin.localEulerAngles.y;
		float num = (angle - currentAngle + 360f) % 360f;
		if (num > 180f)
		{
			num -= 360f;
		}
		int num2 = Mathf.FloorToInt(totalSpinTime / 4f);
		num += 360f * (float)num2;
		float finalAngle = currentAngle + num;
		while (elapsedTime < totalSpinTime)
		{
			elapsedTime += Time.deltaTime;
			float num3 = elapsedTime / totalSpinTime;
			float num4 = Mathf.Lerp(currentAngle, finalAngle, num3 * num3 * (3f - 2f * num3));
			toSpin.localEulerAngles = new Vector3(0f, num4 % 360f, 0f);
			yield return null;
		}
		toSpin.localEulerAngles = new Vector3(0f, angle, 0f);
	}

	public IEnumerator makeTickSounds(ASound spinSound, float spinTime)
	{
		float nextTickIn = 0.01f;
		float tickTimer = 1f;
		while (spinTime > 0f)
		{
			spinTime -= Time.deltaTime;
			if (tickTimer > nextTickIn)
			{
				tickTimer = 0f;
				nextTickIn += 0.01f;
				spinSound.playSoundForAnimator();
			}
			else
			{
				tickTimer += Time.deltaTime;
			}
			yield return null;
		}
		spinSound.playSoundForAnimator();
		spinSound.playSoundForAnimator();
	}

	public void ShowACreature(int animalDummyId, string animalIdText)
	{
		animalIdText = animalIdText.Trim('<', '>');
		int result = -1;
		int.TryParse(animalIdText, out result);
		bool flag = false;
		if (bugExhibits.Length != 0)
		{
			flag = true;
			if (result >= 0 && result < Inventory.Instance.allItems.Length && (bool)Inventory.Instance.allItems[result].bug)
			{
				bugExhibits[animalDummyId].placeBugAndShowDisplay(result, new Vector2(1.8f, 1.8f));
				StartCoroutine(EnableObjectAndScaleIn(bugExhibits[animalDummyId].gameObject));
				bugExhibits[animalDummyId].GetComponent<AnimalBookWorksOnTileObject>().showingId = result;
			}
		}
		if (result >= 0 && result < Inventory.Instance.allItems.Length && (bool)Inventory.Instance.allItems[result].fish)
		{
			fishRens[animalDummyId].setTexture(Inventory.Instance.allItems[result]);
			fishRens[animalDummyId].changeSizeOfTrans(Inventory.Instance.allItems[result].transform.localScale);
			fishRens[animalDummyId].transform.localPosition = new Vector3(0f, (0f - Inventory.Instance.allItems[result].transform.localScale.y) / 2f, 0f);
			StartCoroutine(EnableObjectAndScaleIn(fishObjects[animalDummyId]));
			fishObjects[animalDummyId].GetComponent<AnimalBookWorksOnTileObject>().showingId = result;
		}
		else if (result == -1)
		{
			if (flag)
			{
				StartCoroutine(DisableObjectAndScaleOut(bugExhibits[animalDummyId].gameObject));
			}
			else
			{
				StartCoroutine(DisableObjectAndScaleOut(fishObjects[animalDummyId]));
			}
		}
	}

	public IEnumerator EnableObjectAndScaleIn(GameObject enable)
	{
		if (!enable.activeSelf)
		{
			yield return new WaitForSeconds(1f);
			enable.transform.localScale = Vector3.zero;
			enable.SetActive(value: true);
			float timer = 0f;
			while (timer < 1f)
			{
				enable.transform.localScale = new Vector3(timer, timer, timer);
				timer = Mathf.Clamp(timer + Time.deltaTime * 2f, 0f, 1f);
				yield return null;
			}
		}
		enable.transform.localScale = Vector3.one;
	}

	public IEnumerator DisableObjectAndScaleOut(GameObject enable)
	{
		if (enable.activeSelf)
		{
			float timer = 1f;
			while (timer > 0f)
			{
				enable.transform.localScale = new Vector3(timer, timer, timer);
				timer = Mathf.Clamp(timer - Time.deltaTime * 2f, 0f, 1f);
				yield return null;
			}
			enable.transform.localScale = Vector3.zero;
			enable.SetActive(value: false);
		}
	}

	public void SetUpMannequin(string[] allObjects)
	{
		for (int i = 0; i < allObjects.Length; i++)
		{
			allObjects[i] = allObjects[i].Trim('<', '>');
		}
		int result = -1;
		int result2 = -1;
		int result3 = -1;
		int result4 = -1;
		int result5 = -1;
		int.TryParse(allObjects[0], out result);
		int.TryParse(allObjects[1], out result2);
		int.TryParse(allObjects[2], out result3);
		int.TryParse(allObjects[3], out result4);
		int.TryParse(allObjects[4], out result5);
		SpawnHeadAndFace(result, result2);
		ShowShirt(result3);
		ShowPants(result4);
		ShowShoes(result5);
	}

	private void SpawnHeadAndFace(int headId, int faceId)
	{
		if (showingOnHead != null)
		{
			Object.Destroy(showingOnHead);
		}
		if (showingOnFace != null)
		{
			Object.Destroy(showingOnFace);
		}
		if (headId > -1 && headId < Inventory.Instance.allItems.Length && (bool)Inventory.Instance.allItems[headId].equipable && Inventory.Instance.allItems[headId].equipable.hat)
		{
			showingOnHead = Object.Instantiate(Inventory.Instance.allItems[headId].equipable.hatPrefab, headPos);
			showingOnHead.transform.localPosition = Vector3.zero;
			showingOnHead.transform.localRotation = Quaternion.identity;
			SetItemTexture componentInChildren = showingOnHead.GetComponentInChildren<SetItemTexture>();
			if ((bool)componentInChildren)
			{
				componentInChildren.setTexture(Inventory.Instance.allItems[headId]);
				if ((bool)componentInChildren.changeSize)
				{
					componentInChildren.changeSizeOfTrans(Inventory.Instance.allItems[headId].transform.localScale);
				}
			}
		}
		if (faceId <= -1 || faceId >= Inventory.Instance.allItems.Length || !Inventory.Instance.allItems[faceId].equipable || !Inventory.Instance.allItems[faceId].equipable.face)
		{
			return;
		}
		showingOnFace = Object.Instantiate(Inventory.Instance.allItems[faceId].equipable.hatPrefab, headPos);
		showingOnFace.transform.localPosition = Vector3.zero;
		showingOnFace.transform.localRotation = Quaternion.identity;
		SetItemTexture componentInChildren2 = showingOnFace.GetComponentInChildren<SetItemTexture>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.setTexture(Inventory.Instance.allItems[faceId]);
			if ((bool)componentInChildren2.changeSize)
			{
				componentInChildren2.changeSizeOfTrans(Inventory.Instance.allItems[faceId].transform.localScale);
			}
		}
	}

	private void ShowShirt(int id)
	{
		shirt.gameObject.SetActive(id > -1);
		if (id <= -1)
		{
			return;
		}
		if (Inventory.Instance.allItems[id].equipable.dress)
		{
			if (Inventory.Instance.allItems[id].equipable.longDress)
			{
				shirt.sharedMesh = EquipWindow.equip.defaultLongDress;
			}
			else
			{
				shirt.sharedMesh = EquipWindow.equip.defaultDress;
			}
		}
		else if ((bool)Inventory.Instance.allItems[id].equipable.shirtMesh)
		{
			shirt.sharedMesh = Inventory.Instance.allItems[id].equipable.shirtMesh;
		}
		else if (Inventory.Instance.allItems[id].equipable.shirt)
		{
			shirt.sharedMesh = EquipWindow.equip.defaultShirtMesh;
		}
		shirt.material = Inventory.Instance.allItems[id].equipable.material;
	}

	private void ShowShoes(int id)
	{
		shoes.gameObject.SetActive(id > -1);
		if (id > -1)
		{
			if ((bool)Inventory.Instance.allItems[id].equipable.useAltMesh)
			{
				shoes.sharedMesh = Inventory.Instance.allItems[id].equipable.useAltMesh;
			}
			else
			{
				shoes.sharedMesh = EquipWindow.equip.defualtShoeMesh;
			}
			shoes.material = Inventory.Instance.allItems[id].equipable.material;
		}
	}

	private void ShowPants(int id)
	{
		pants.gameObject.SetActive(id > -1);
		if (id > -1)
		{
			if (Inventory.Instance.allItems[id].equipable.dress)
			{
				pants.sharedMesh = EquipWindow.equip.defaultSkirt;
			}
			else if ((bool)Inventory.Instance.allItems[id].equipable.useAltMesh)
			{
				pants.sharedMesh = Inventory.Instance.allItems[id].equipable.useAltMesh;
			}
			else
			{
				pants.sharedMesh = EquipWindow.equip.defaultPants;
			}
			pants.material = Inventory.Instance.allItems[id].equipable.material;
		}
	}

	private void SetUpToolRack(string[] tools)
	{
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i] = tools[i].Trim('<', '>');
		}
		int result = -1;
		int result2 = -1;
		int result3 = -1;
		int.TryParse(tools[0], out result);
		int.TryParse(tools[1], out result2);
		int.TryParse(tools[2], out result3);
		SpawnTool(result, toolPos1);
		SpawnTool(result2, toolPos2);
		SpawnTool(result3, toolPos3);
	}

	private void SetUpDisplayCase(string[] tools)
	{
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i] = tools[i].Trim('<', '>');
		}
		int result = -1;
		int result2 = -1;
		int.TryParse(tools[0], out result);
		int.TryParse(tools[1], out result2);
		if ((bool)toolPos1)
		{
			SpawnItemForCase(result, toolPos1, signText);
		}
		if ((bool)toolPos2)
		{
			SpawnItemForCase(result2, toolPos2, otherSide);
		}
	}

	private void SpawnTool(int id, Transform spawnInTransform)
	{
		foreach (Transform item in spawnInTransform)
		{
			Object.Destroy(item.gameObject);
		}
		if (id <= -1)
		{
			return;
		}
		GameObject gameObject = ((!Inventory.Instance.allItems[id].altDropPrefab) ? Object.Instantiate(Inventory.Instance.allItems[id].itemPrefab, spawnInTransform) : Object.Instantiate(Inventory.Instance.allItems[id].altDropPrefab, spawnInTransform));
		if ((bool)gameObject.GetComponentInChildren<Animator>())
		{
			gameObject.GetComponentInChildren<Animator>().enabled = false;
		}
		if (!StandUpInStand(id))
		{
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
		}
		else
		{
			gameObject.transform.localPosition = new Vector3(0.18f, 1f, -0.03f);
			gameObject.transform.localEulerAngles = new Vector3(270f, 90f, 0f);
		}
		SetItemTexture componentInChildren = gameObject.GetComponentInChildren<SetItemTexture>();
		if ((bool)componentInChildren)
		{
			componentInChildren.setTexture(Inventory.Instance.allItems[id]);
			if ((bool)componentInChildren.changeSize)
			{
				componentInChildren.changeSizeOfTrans(Inventory.Instance.allItems[id].transform.localScale);
			}
		}
	}

	private bool StandUpInStand(int id)
	{
		if ((bool)Inventory.Instance.allItems[id].altDropPrefab)
		{
			DropDisplayPos component = Inventory.Instance.allItems[id].altDropPrefab.GetComponent<DropDisplayPos>();
			if ((bool)component)
			{
				if (component.StandUp)
				{
					return true;
				}
				if (component.NormalPos)
				{
					return false;
				}
			}
		}
		if (!Inventory.Instance.allItems[id].useRightHandAnim && !Inventory.Instance.allItems[id].isPowerTool)
		{
			return false;
		}
		if (Inventory.Instance.allItems[id].isPowerTool && Inventory.Instance.allItems[id].ignoreTwoArmAnim)
		{
			return false;
		}
		if (Inventory.Instance.allItems[id].myAnimType != InventoryItem.typeOfAnimation.WateringCan && Inventory.Instance.allItems[id].myAnimType != InventoryItem.typeOfAnimation.UpgradedWateringCan && Inventory.Instance.allItems[id].myAnimType != InventoryItem.typeOfAnimation.Glider)
		{
			return true;
		}
		return false;
	}

	private void SpawnItemForCase(int id, Transform spawnInTransform, TextMeshPro toWriteNameIn)
	{
		foreach (Transform item in spawnInTransform)
		{
			Object.Destroy(item.gameObject);
		}
		if (id <= -1)
		{
			if ((bool)toWriteNameIn)
			{
				toWriteNameIn.text = "";
			}
			fishObjects[1].SetActive(value: false);
			fishObjects[0].SetActive(value: true);
			return;
		}
		if ((bool)toWriteNameIn)
		{
			toWriteNameIn.text = Inventory.Instance.allItems[id].getInvItemName();
		}
		GameObject gameObject;
		if (!Inventory.Instance.allItems[id].equipable || !Inventory.Instance.allItems[id].equipable.cloths)
		{
			gameObject = ((!Inventory.Instance.allItems[id].altDropPrefab) ? Object.Instantiate(Inventory.Instance.allItems[id].itemPrefab, spawnInTransform) : Object.Instantiate(Inventory.Instance.allItems[id].altDropPrefab, spawnInTransform));
		}
		else if (Inventory.Instance.allItems[id].equipable.hat || Inventory.Instance.allItems[id].equipable.face)
		{
			gameObject = Object.Instantiate(EquipWindow.equip.hatPlaceable.gameObject, spawnInTransform);
			gameObject.GetComponent<ClothingDisplay>().updateStatus(id);
		}
		else if (Inventory.Instance.allItems[id].equipable.shirt)
		{
			gameObject = Object.Instantiate(EquipWindow.equip.shirtPlaceable.gameObject, spawnInTransform);
			gameObject.GetComponent<ClothingDisplay>().updateStatus(id);
		}
		else if (Inventory.Instance.allItems[id].equipable.pants)
		{
			gameObject = Object.Instantiate(EquipWindow.equip.pantsPlaceable.gameObject, spawnInTransform);
			gameObject.GetComponent<ClothingDisplay>().updateStatus(id);
		}
		else
		{
			gameObject = Object.Instantiate(EquipWindow.equip.shoePlaceable.gameObject, spawnInTransform);
			gameObject.GetComponent<ClothingDisplay>().updateStatus(id);
		}
		Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
		if ((bool)componentInChildren)
		{
			componentInChildren.enabled = false;
		}
		if ((bool)Inventory.Instance.allItems[id].equipable && Inventory.Instance.allItems[id].equipable.cloths)
		{
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		}
		else
		{
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
		}
		if (StandUpInStand(id))
		{
			gameObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);
			gameObject.transform.localEulerAngles = new Vector3(270f, 180f, 0f);
			fishObjects[0].SetActive(value: false);
			fishObjects[1].SetActive(value: true);
		}
		else
		{
			fishObjects[1].SetActive(value: false);
			fishObjects[0].SetActive(value: true);
		}
		SetItemTexture componentInChildren2 = gameObject.GetComponentInChildren<SetItemTexture>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.setTexture(Inventory.Instance.allItems[id]);
			if ((bool)componentInChildren2.changeSize)
			{
				componentInChildren2.changeSizeOfTrans(Inventory.Instance.allItems[id].transform.localScale);
			}
		}
		if ((bool)Inventory.Instance.allItems[id].underwaterCreature)
		{
			StartCoroutine(AfterBug(gameObject));
		}
	}

	private IEnumerator AfterBug(GameObject spawn)
	{
		yield return null;
		Animator[] componentsInChildren = spawn.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Rebind();
		}
	}
}
