using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeedButton : MonoBehaviour
{
	public bool buyDeedButton;

	private int deedInventoryNumber;

	public Image deedIcon;

	public TextMeshProUGUI deedNameText;

	public TextMeshProUGUI deedDescText;

	public bool moveABuildingButton;

	private int buildingNo = -1;

	public void setUpButton(int deedNo)
	{
		deedInventoryNumber = deedNo;
		deedIcon.sprite = Inventory.Instance.allItems[deedNo].getSprite();
		deedNameText.text = Inventory.Instance.allItems[deedNo].getInvItemName();
		deedDescText.text = Inventory.Instance.allItems[deedNo].getItemDescription(deedNo);
		if (moveABuildingButton && (bool)Inventory.Instance.allItems[deedInventoryNumber].placeable)
		{
			if ((bool)Inventory.Instance.allItems[deedInventoryNumber].placeable.tileObjectGrowthStages)
			{
				deedNameText.text = BuildingManager.manage.getBuildingName(Inventory.Instance.allItems[deedInventoryNumber].placeable.tileObjectGrowthStages.changeToWhenGrown.tileObjectId);
			}
			else
			{
				deedNameText.text = BuildingManager.manage.getBuildingName(Inventory.Instance.allItems[deedInventoryNumber].placeable.tileObjectId);
			}
		}
	}

	public void setUpBuildingButton(int deedNo, int newBuildingNo)
	{
		setUpButton(deedNo);
		buildingNo = newBuildingNo;
	}

	public void pressButton()
	{
		if (!moveABuildingButton)
		{
			DeedManager.manage.openConfirmDeedWindow(deedInventoryNumber);
		}
		else
		{
			BuildingManager.manage.askToMoveBuilding(buildingNo);
		}
	}
}
