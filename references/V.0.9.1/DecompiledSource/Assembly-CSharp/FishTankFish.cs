using UnityEngine;

public class FishTankFish : MonoBehaviour
{
	public SkinnedMeshRenderer fishRen;

	public Transform fishSizeTrans;

	public AnimateShopAnimal myAnimator;

	public int showingFishId;

	public void skinMeshAndSetAnimation(int invId, Vector2 walkSize)
	{
		showingFishId = invId;
		fishSizeTrans.localScale = Inventory.Instance.allItems[invId].fish.fishScale();
		fishRen.material = Inventory.Instance.allItems[invId].equipable.material;
		if ((bool)Inventory.Instance.allItems[invId].equipable.useAltMesh)
		{
			fishRen.sharedMesh = Inventory.Instance.allItems[invId].equipable.useAltMesh;
		}
		myAnimator.walkDistance = walkSize;
		myAnimator.setMinWaitTimeFish();
		myAnimator.resetStartingPosAndRandomisePos(0.75f);
		myAnimator.walkAnim = GetComponentInChildren<Animator>();
	}
}
