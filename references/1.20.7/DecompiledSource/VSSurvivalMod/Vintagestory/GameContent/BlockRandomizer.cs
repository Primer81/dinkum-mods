using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockRandomizer : Block
{
	public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
	{
		ItemStack stack = base.OnPickBlock(world, pos);
		BlockEntityBlockRandomizer be = world.BlockAccessor.GetBlockEntity<BlockEntityBlockRandomizer>(pos);
		if (be != null)
		{
			stack.Attributes["chances"] = new FloatArrayAttribute(be.Chances);
			be.Inventory.ToTreeAttributes(stack.Attributes);
		}
		return stack;
	}

	public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
	{
		GetBlockEntity<BlockEntityBlockRandomizer>(blockSel.Position)?.OnInteract(byPlayer);
		return true;
	}

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		CustomBlockLayerHandler = true;
	}
}
