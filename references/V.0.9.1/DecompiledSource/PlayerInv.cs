using System;
using UnityEngine;

[Serializable]
public class PlayerInv
{
	public string playerName = "Noob";

	public string islandName = "Dinkum";

	public int money;

	public int hair = 1;

	public int hairColour;

	public int eyeStyle;

	public int eyeColour;

	public int nose;

	public int mouth;

	public int face = -1;

	public int head = -1;

	public int body = -1;

	public int pants = 2;

	public int shoes = -1;

	public int skinTone = 1;

	public int[] itemsInInvSlots = new int[32];

	public int[] stacksInSlots = new int[32];

	public int bankBalance;

	public long bankBalanceOverflow;

	public ulong accountOverflow;

	public float stamina = 100f;

	public float staminaMax;

	public int health = 100;

	public int healthMax;

	public bool[] catalogue;

	public long savedTime;

	public int snagsEaten;

	public int faceStack;

	public int headStack;

	public int bodyStack;

	public int pantsStack;

	public int shoesStack;

	public void SaveInv()
	{
		money = Inventory.Instance.wallet;
		bankBalance = BankMenu.menu.accountBalance;
		accountOverflow = BankMenu.menu.accountOverflow;
		playerName = Inventory.Instance.playerName;
		islandName = Inventory.Instance.islandName;
		eyeStyle = Inventory.Instance.playerEyes;
		eyeColour = Inventory.Instance.playerEyeColor;
		skinTone = Inventory.Instance.skinTone;
		nose = Inventory.Instance.nose;
		mouth = Inventory.Instance.mouth;
		face = EquipWindow.equip.faceSlot.itemNo;
		faceStack = EquipWindow.equip.faceSlot.stack;
		hair = Inventory.Instance.playerHair;
		hairColour = Inventory.Instance.playerHairColour;
		head = EquipWindow.equip.hatSlot.itemNo;
		headStack = EquipWindow.equip.hatSlot.stack;
		body = EquipWindow.equip.shirtSlot.itemNo;
		bodyStack = EquipWindow.equip.shirtSlot.stack;
		pants = EquipWindow.equip.pantsSlot.itemNo;
		pantsStack = EquipWindow.equip.pantsSlot.stack;
		shoes = EquipWindow.equip.shoeSlot.itemNo;
		shoesStack = EquipWindow.equip.shoeSlot.stack;
		health = StatusManager.manage.connectedDamge.health;
		healthMax = StatusManager.manage.connectedDamge.maxHealth;
		stamina = StatusManager.manage.getStamina();
		staminaMax = StatusManager.manage.getStaminaMax();
		snagsEaten = StatusManager.manage.snagsEaten;
		catalogue = CatalogueManager.manage.collectedItem;
		savedTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
		itemsInInvSlots = new int[Inventory.Instance.invSlots.Length];
		stacksInSlots = new int[Inventory.Instance.invSlots.Length];
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			itemsInInvSlots[i] = Inventory.Instance.invSlots[i].itemNo;
			stacksInSlots[i] = Inventory.Instance.invSlots[i].stack;
		}
	}

	public void LoadInv()
	{
		Inventory.Instance.changeWalletToLoad(money);
		BankMenu.menu.accountBalance = bankBalance;
		BankMenu.menu.accountOverflow = accountOverflow;
		if (hair < 0)
		{
			hair = Mathf.Abs(hair + 1);
		}
		Inventory.Instance.playerHair = hair;
		Inventory.Instance.playerHairColour = hairColour;
		Inventory.Instance.playerEyes = eyeStyle;
		Inventory.Instance.nose = nose;
		Inventory.Instance.mouth = mouth;
		Inventory.Instance.playerEyeColor = eyeColour;
		Inventory.Instance.skinTone = skinTone;
		Inventory.Instance.playerName = playerName;
		Inventory.Instance.islandName = islandName;
		EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(head, Min1(headStack));
		EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(face, Min1(faceStack));
		EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(body, Min1(bodyStack));
		EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(pants, Min1(pantsStack));
		EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(shoes, Min1(shoesStack));
		StatusManager.manage.snagsEaten = snagsEaten;
		if (catalogue != null)
		{
			for (int i = 0; i < catalogue.Length; i++)
			{
				if (i <= CatalogueManager.manage.collectedItem.Length - 1)
				{
					CatalogueManager.manage.collectedItem[i] = catalogue[i];
				}
			}
		}
		StatusManager.manage.loadStatus(health, healthMax, stamina, staminaMax);
		for (int j = 0; j < itemsInInvSlots.Length; j++)
		{
			Inventory.Instance.invSlots[j].itemNo = itemsInInvSlots[j];
			Inventory.Instance.invSlots[j].stack = stacksInSlots[j];
			Inventory.Instance.invSlots[j].updateSlotContentsAndRefresh(itemsInInvSlots[j], stacksInSlots[j]);
		}
	}

	private int Min1(int intIn)
	{
		if (intIn <= 0)
		{
			return 1;
		}
		return intIn;
	}
}
