using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockSimpleCoating : Block
{
	public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
	{
		bool result = true;
		bool preventDefault = false;
		BlockBehavior[] blockBehaviors = BlockBehaviors;
		foreach (BlockBehavior obj in blockBehaviors)
		{
			EnumHandling handled = EnumHandling.PassThrough;
			bool behaviorResult = obj.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref handled, ref failureCode);
			if (handled != 0)
			{
				result = result && behaviorResult;
				preventDefault = true;
			}
			if (handled == EnumHandling.PreventSubsequent)
			{
				return result;
			}
		}
		if (preventDefault)
		{
			return result;
		}
		if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
		{
			return false;
		}
		if (TryAttachTo(world, blockSel.Position, blockSel.Face))
		{
			return true;
		}
		BlockFacing[] faces = BlockFacing.ALLFACES;
		for (int i = 0; i < faces.Length; i++)
		{
			if (TryAttachTo(world, blockSel.Position, faces[i]))
			{
				return true;
			}
		}
		failureCode = "requireattachable";
		return false;
	}

	public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
	{
		return GetHandbookDropsFromBreakDrops(handbookStack, forPlayer);
	}

	public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
	{
		Block block = world.BlockAccessor.GetBlock(CodeWithParts("down"));
		return new ItemStack[1]
		{
			new ItemStack(block)
		};
	}

	public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
	{
		return new ItemStack(world.BlockAccessor.GetBlock(CodeWithParts("down")));
	}

	public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
	{
		if (!CanBlockStay(world, pos))
		{
			world.BlockAccessor.BreakBlock(pos, null);
		}
	}

	private bool TryAttachTo(IWorldAccessor world, BlockPos blockpos, BlockFacing onBlockFace)
	{
		BlockFacing oppositeFace = onBlockFace.Opposite;
		BlockPos attachingBlockPos = blockpos.AddCopy(oppositeFace);
		if (world.BlockAccessor.GetBlock(attachingBlockPos).CanAttachBlockAt(world.BlockAccessor, this, attachingBlockPos, onBlockFace))
		{
			int blockId = world.BlockAccessor.GetBlock(CodeWithParts(oppositeFace.Code)).BlockId;
			world.BlockAccessor.SetBlock(blockId, blockpos);
			return true;
		}
		return false;
	}

	private bool CanBlockStay(IWorldAccessor world, BlockPos pos)
	{
		BlockFacing facing = BlockFacing.FromCode(Code.Path.Split('-')[^1]);
		return world.BlockAccessor.GetBlock(pos.AddCopy(facing)).CanAttachBlockAt(world.BlockAccessor, this, pos.AddCopy(facing), facing.Opposite);
	}

	public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
	{
		return false;
	}

	public override AssetLocation GetRotatedBlockCode(int angle)
	{
		if (LastCodePart() == "up" || LastCodePart() == "down")
		{
			return Code;
		}
		BlockFacing newFacing = BlockFacing.HORIZONTALS_ANGLEORDER[((360 - angle) / 90 + BlockFacing.FromCode(LastCodePart()).HorizontalAngleIndex) % 4];
		return CodeWithParts(newFacing.Code);
	}

	public override AssetLocation GetVerticallyFlippedBlockCode()
	{
		if (!(LastCodePart() == "up"))
		{
			return CodeWithParts("up");
		}
		return CodeWithParts("down");
	}

	public override AssetLocation GetHorizontallyFlippedBlockCode(EnumAxis axis)
	{
		BlockFacing facing = BlockFacing.FromCode(LastCodePart());
		if (facing.Axis == axis)
		{
			return CodeWithParts(facing.Opposite.Code);
		}
		return Code;
	}
}
