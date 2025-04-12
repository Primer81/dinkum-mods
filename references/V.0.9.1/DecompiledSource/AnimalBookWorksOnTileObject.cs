using UnityEngine;

public class AnimalBookWorksOnTileObject : MonoBehaviour
{
	public NameTag myNameTag;

	public ClothingDisplay showingObject;

	public bool isBug;

	public bool isFish;

	public int showingId = -1;

	public void OnEnable()
	{
		if (isBug)
		{
			AnimalManager.manage.lookAtBugBook.AddListener(openBugBook);
		}
		if (isFish)
		{
			AnimalManager.manage.lookAtFishBook.AddListener(openFishBook);
		}
	}

	public void OnDisable()
	{
		if (isBug)
		{
			AnimalManager.manage.lookAtBugBook.RemoveListener(openBugBook);
		}
		if (isFish)
		{
			AnimalManager.manage.lookAtFishBook.RemoveListener(openFishBook);
		}
		myNameTag.turnOff();
	}

	public void openBugBook()
	{
		if ((bool)showingObject)
		{
			showingId = showingObject.getShowingId();
		}
		if (showingId < 0)
		{
			myNameTag.turnOff();
		}
		else if (base.gameObject.activeSelf && AnimalManager.manage.bugBookOpen)
		{
			if (showingId >= 0 && PediaManager.manage.isInPedia(showingId))
			{
				myNameTag.turnOn("<sprite=11>" + Inventory.Instance.allItems[showingId].value + "\n" + Inventory.Instance.allItems[showingId].getInvItemName());
			}
			else
			{
				myNameTag.turnOn("<sprite=11>????\n?????");
			}
			myNameTag.transform.parent = null;
			myNameTag.transform.localScale = Vector3.one;
			myNameTag.transform.parent = base.transform;
			if ((bool)myNameTag)
			{
				myNameTag.enableMeshes();
			}
		}
		else
		{
			myNameTag.turnOff();
		}
	}

	public void openFishBook()
	{
		if ((bool)showingObject)
		{
			showingId = showingObject.getShowingId();
		}
		if (showingId < 0)
		{
			myNameTag.turnOff();
		}
		else if (base.gameObject.activeSelf && AnimalManager.manage.fishBookOpen)
		{
			if (showingId >= 0 && PediaManager.manage.isInPedia(showingId))
			{
				myNameTag.turnOn("<sprite=11>" + Inventory.Instance.allItems[showingId].value + "\n" + Inventory.Instance.allItems[showingId].getInvItemName());
			}
			else
			{
				myNameTag.turnOn("<sprite=11>????\n?????");
			}
			myNameTag.transform.parent = null;
			myNameTag.transform.localScale = Vector3.one;
			myNameTag.transform.parent = base.transform;
			if ((bool)myNameTag)
			{
				myNameTag.enableMeshes();
			}
		}
		else
		{
			myNameTag.turnOff();
		}
	}
}
