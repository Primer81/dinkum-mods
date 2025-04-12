using Mirror;

public class VehicleStorage : NetworkBehaviour
{
	public int[] invSlots = new int[24];

	public int[] invSlotStacks = new int[24];

	public void onDeath()
	{
		for (int i = 0; i < invSlots.Length; i++)
		{
			if (invSlots[i] > -1)
			{
				NetworkMapSharer.Instance.spawnAServerDrop(invSlots[i], invSlotStacks[i], base.transform.position);
			}
		}
	}

	public void loadStorage(int[] slots, int[] stacks)
	{
		invSlots = slots;
		invSlotStacks = stacks;
	}

	public override void OnStopClient()
	{
		if (ChestWindow.chests.isVehicleStorage == this)
		{
			ChestWindow.chests.closeChestInWindow();
			MenuButtonsTop.menu.closeButtonDelay();
		}
	}

	private void MirrorProcessed()
	{
	}
}
