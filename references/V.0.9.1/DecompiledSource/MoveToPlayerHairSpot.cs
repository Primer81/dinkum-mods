using UnityEngine;

public class MoveToPlayerHairSpot : MonoBehaviour
{
	private EquipItemToChar myChar;

	private NPCIdentity npcIdentity;

	public string HairPosName = "OnHairPos";

	private void Start()
	{
		myChar = GetComponentInParent<EquipItemToChar>();
		if ((bool)myChar)
		{
			if (!GetComponentInParent<HoldPos>())
			{
				GameObject hairObject = myChar.GetHairObject();
				if ((bool)hairObject)
				{
					Transform transform = hairObject.transform.Find(HairPosName);
					if ((bool)transform)
					{
						base.transform.position = transform.position;
						base.transform.rotation = transform.rotation;
					}
				}
			}
			else
			{
				base.transform.localPosition = Vector3.zero;
			}
			return;
		}
		npcIdentity = GetComponentInParent<NPCIdentity>();
		if (!npcIdentity)
		{
			return;
		}
		GameObject hairObject2 = npcIdentity.GetHairObject();
		if ((bool)hairObject2)
		{
			Transform transform2 = hairObject2.transform.Find(HairPosName);
			if ((bool)transform2)
			{
				base.transform.position = transform2.position;
				base.transform.rotation = transform2.rotation;
			}
		}
	}
}
