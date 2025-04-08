using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockBough : Block
{
	public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
	{
		if (CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
		{
			BlockFacing horVer = OrientForPlacement(world.BlockAccessor, byPlayer, blockSel).Opposite;
			AssetLocation newCode = CodeWithParts(horVer.Code.Substring(0, 1));
			world.BlockAccessor.SetBlock(world.GetBlock(newCode).BlockId, blockSel.Position);
			return true;
		}
		return false;
	}

	protected virtual BlockFacing OrientForPlacement(IBlockAccessor world, IPlayer player, BlockSelection bs)
	{
		BlockFacing[] facings = Block.SuggestedHVOrientation(player, bs);
		if (facings.Length == 0)
		{
			return BlockFacing.NORTH;
		}
		return facings[0];
	}

	public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
	{
		return new ItemStack(world.GetBlock(CodeWithParts("placed", Variant["wood"], "20", "n")));
	}
}
