using UnityEngine;

public class ClothingDisplay : MonoBehaviour
{
	public MeshRenderer ren;

	public Transform hatPos;

	private GameObject headObject;

	public SetItemTexture fishTexture;

	public Transform bugPos;

	private int showingId = -1;

	private GameObject myBugModel;

	public void updateStatus(int clothingId)
	{
		if (clothingId <= -1)
		{
			return;
		}
		if ((bool)Inventory.Instance.allItems[clothingId].bug)
		{
			if (showingId != clothingId)
			{
				if (myBugModel != null)
				{
					Object.Destroy(myBugModel.gameObject);
				}
				myBugModel = Object.Instantiate(Inventory.Instance.allItems[clothingId].bug.insectType, bugPos);
				myBugModel.transform.localPosition = Vector3.zero;
				myBugModel.transform.localRotation = Quaternion.identity;
				myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.Instance.allItems[clothingId]);
				myBugModel.GetComponent<Animator>().SetTrigger("Captured");
				showingId = clothingId;
			}
			else
			{
				myBugModel.GetComponent<Animator>().SetTrigger("Captured");
			}
		}
		else if ((bool)Inventory.Instance.allItems[clothingId].fish)
		{
			if (showingId != clothingId)
			{
				fishTexture.setTexture(Inventory.Instance.allItems[clothingId]);
				fishTexture.changeSizeOfTrans(Inventory.Instance.allItems[clothingId].transform.localScale);
				fishTexture.gameObject.SetActive(value: true);
				fishTexture.GetComponentInChildren<Animator>().SetFloat("Offset", Random.Range(0f, 1f));
				showingId = clothingId;
			}
		}
		else if (((bool)Inventory.Instance.allItems[clothingId].equipable && Inventory.Instance.allItems[clothingId].equipable.hat) || ((bool)Inventory.Instance.allItems[clothingId].equipable && Inventory.Instance.allItems[clothingId].equipable.face))
		{
			if (headObject != null)
			{
				Object.Destroy(headObject);
			}
			headObject = Object.Instantiate(Inventory.Instance.allItems[clothingId].equipable.hatPrefab, hatPos);
			headObject.transform.localPosition = Vector3.zero;
			headObject.transform.localRotation = Quaternion.identity;
			setUpHeadLod();
			if ((bool)headObject.GetComponent<SetItemTexture>())
			{
				headObject.GetComponent<SetItemTexture>().setTexture(Inventory.Instance.allItems[clothingId]);
			}
		}
		else
		{
			if ((!Inventory.Instance.allItems[clothingId].equipable || !Inventory.Instance.allItems[clothingId].equipable.shirt) && (!Inventory.Instance.allItems[clothingId].equipable || !Inventory.Instance.allItems[clothingId].equipable.pants) && (!Inventory.Instance.allItems[clothingId].equipable || !Inventory.Instance.allItems[clothingId].equipable.shoes))
			{
				return;
			}
			if (Inventory.Instance.allItems[clothingId].equipable.dress)
			{
				if (Inventory.Instance.allItems[clothingId].equipable.longDress)
				{
					ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultLongDress;
				}
				else
				{
					ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultDress;
				}
			}
			else if ((bool)Inventory.Instance.allItems[clothingId].equipable.shirtMesh)
			{
				ren.GetComponent<MeshFilter>().mesh = Inventory.Instance.allItems[clothingId].equipable.shirtMesh;
			}
			else if ((bool)Inventory.Instance.allItems[clothingId].equipable.useAltMesh)
			{
				ren.GetComponent<MeshFilter>().mesh = Inventory.Instance.allItems[clothingId].equipable.useAltMesh;
			}
			else if (Inventory.Instance.allItems[clothingId].equipable.shirt)
			{
				ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultShirtMesh;
			}
			else if (Inventory.Instance.allItems[clothingId].equipable.pants)
			{
				ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultPants;
			}
			else if (Inventory.Instance.allItems[clothingId].equipable.shoes)
			{
				ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defualtShoeMesh;
			}
			ren.material = Inventory.Instance.allItems[clothingId].equipable.material;
		}
	}

	public void setUpHeadLod()
	{
		if ((bool)headObject)
		{
			LODGroup lODGroup = headObject.AddComponent<LODGroup>();
			LOD[] array = new LOD[2];
			Renderer[] componentsInChildren = headObject.GetComponentsInChildren<MeshRenderer>();
			array[0] = new LOD(0.01f, componentsInChildren);
			array[1] = new LOD(0f, null);
			lODGroup.SetLODs(array);
			lODGroup.RecalculateBounds();
		}
	}

	public int getShowingId()
	{
		return showingId;
	}
}
