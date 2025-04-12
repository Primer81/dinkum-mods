using UnityEngine;

public class AccessoryMatchCharColour : MonoBehaviour
{
	public Renderer myRen;

	public bool matchHair = true;

	public bool matchSkin;

	private void Start()
	{
		EquipItemToChar componentInParent = base.transform.root.GetComponentInParent<EquipItemToChar>();
		if (matchHair)
		{
			if ((bool)componentInParent)
			{
				myRen.material.color = CharacterCreatorScript.create.getHairColour(componentInParent.hairColor);
			}
			else if ((bool)base.transform.root.GetComponentInParent<CharacterCreatorScript>())
			{
				myRen.material.color = CharacterCreatorScript.create.getHairColour(NetworkMapSharer.Instance.localChar.myEquip.hairColor);
			}
		}
		else if (matchSkin)
		{
			if ((bool)componentInParent)
			{
				myRen.material.color = CharacterCreatorScript.create.getSkinTone(componentInParent.skinId);
			}
			else if ((bool)base.transform.root.GetComponentInParent<CharacterCreatorScript>())
			{
				myRen.material.color = CharacterCreatorScript.create.getSkinTone(NetworkMapSharer.Instance.localChar.myEquip.skinId);
			}
		}
	}
}
