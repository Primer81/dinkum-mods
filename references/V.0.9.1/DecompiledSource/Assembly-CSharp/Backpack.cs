using UnityEngine;

public class Backpack : MonoBehaviour
{
	private CharMovement isChar;

	private EquipItemToChar charEquip;

	public Material[] colours;

	public Renderer[] myRens;

	public bool isBagOnBack;

	private void Start()
	{
		if (!isBagOnBack)
		{
			isChar = GetComponentInParent<CharMovement>();
			charEquip = GetComponentInParent<EquipItemToChar>();
			ChangeColourOnHold();
		}
	}

	public void doDamageNow()
	{
		if ((bool)isChar && isChar.isLocalPlayer)
		{
			ContainerManager.manage.openStash(1);
		}
	}

	private void ChangeColourOnHold()
	{
		if ((bool)charEquip)
		{
			ChangeColour(charEquip.bagColour);
		}
	}

	public void ChangeColour(int newColour)
	{
		for (int i = 0; i < myRens.Length; i++)
		{
			myRens[i].sharedMaterial = colours[newColour];
		}
	}
}
