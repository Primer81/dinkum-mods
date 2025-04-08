using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent.Mechanics;

public class BlockClutch : Block
{
	public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
	{
		if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
		{
			return false;
		}
		BlockFacing frontFacing = Block.SuggestedHVOrientation(byPlayer, blockSel)[0];
		BlockFacing bestFacing = frontFacing;
		if (!(world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(frontFacing)) is BlockTransmission))
		{
			BlockFacing leftFacing = BlockFacing.HORIZONTALS_ANGLEORDER[GameMath.Mod(frontFacing.HorizontalAngleIndex - 1, 4)];
			if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(leftFacing)) is BlockTransmission)
			{
				bestFacing = leftFacing;
			}
			else
			{
				BlockFacing rightFacing = leftFacing.Opposite;
				if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(rightFacing)) is BlockTransmission)
				{
					bestFacing = rightFacing;
				}
				else
				{
					BlockFacing backFacing = frontFacing.Opposite;
					if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(backFacing)) is BlockTransmission)
					{
						bestFacing = backFacing;
					}
				}
			}
		}
		return world.BlockAccessor.GetBlock(CodeWithParts(bestFacing.Code)).DoPlaceBlock(world, byPlayer, blockSel, itemstack);
	}

	public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
	{
		if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEClutch be)
		{
			return be.OnInteract(byPlayer);
		}
		return base.OnBlockInteractStart(world, byPlayer, blockSel);
	}

	public override void Activate(IWorldAccessor world, Caller caller, BlockSelection blockSel, ITreeAttribute activationArgs)
	{
		if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEClutch be && (activationArgs == null || !activationArgs.HasAttribute("engaged") || activationArgs.GetBool("engaged") != be.Engaged))
		{
			be.OnInteract(caller.Player);
		}
	}

	public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
	{
		(world.BlockAccessor.GetBlockEntity(pos) as BEClutch)?.onNeighbourChange(neibpos);
	}
}
