using UnityEngine;

public class CrushableCarry : MonoBehaviour
{
	public int dropAmount = 4;

	public InventoryItem dropsOnCrush;

	public InventoryItem perfectGem;

	public bool inserted;

	public void CrushNow()
	{
		PickUpAndCarry component = GetComponent<PickUpAndCarry>();
		if ((bool)component)
		{
			NetworkMapSharer.Instance.DestroyCarryable(component);
		}
	}
}
