using UnityEngine;

public class BugExhibit : MonoBehaviour
{
	public int showingBugId;

	private GameObject myBugModel;

	public AnimateShopAnimal myAnimate;

	public bool canBeCaught;

	public void placeBugAndShowDisplay(int bugId, Vector2 walkSize)
	{
		if (bugId != showingBugId)
		{
			showingBugId = bugId;
			if ((bool)myBugModel)
			{
				Object.Destroy(myBugModel);
			}
			if ((bool)Inventory.Instance.allItems[bugId].bug)
			{
				myBugModel = Object.Instantiate(Inventory.Instance.allItems[bugId].bug.insectType, base.transform);
			}
			else
			{
				myBugModel = Object.Instantiate(Inventory.Instance.allItems[bugId].underwaterCreature.creatureModel, base.transform);
			}
			myBugModel.transform.localPosition = Vector3.zero;
			myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.Instance.allItems[bugId]);
			myAnimate.walkAnim = myBugModel.GetComponentInChildren<Animator>();
			myAnimate.walkDistance = walkSize;
			if ((bool)Inventory.Instance.allItems[bugId].bug)
			{
				myAnimate.resetStartingPosAndRandomisePos(Inventory.Instance.allItems[bugId].bug.bugBaseSpeed);
			}
			else
			{
				myAnimate.resetStartingPosAndRandomisePos(Inventory.Instance.allItems[bugId].underwaterCreature.baseSpeed);
			}
		}
	}
}
