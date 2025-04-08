using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

public class BlockGlowworms : Block
{
	public string[] bases = new string[4] { "base1", "base1-short", "base2", "base2-short" };

	public string[] segments = new string[4] { "segment1", null, "segment2", null };

	public string[] ends = new string[4] { "end1", null, "end2", null };

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
	}

	public Block GetBlock(IWorldAccessor world, string rocktype, string thickness)
	{
		return world.GetBlock(CodeWithParts(rocktype, thickness));
	}

	public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
	{
		if (!IsAttached(world, pos))
		{
			world.BlockAccessor.BreakBlock(pos, null);
		}
	}

	public bool IsAttached(IWorldAccessor world, BlockPos pos)
	{
		Block upBlock = world.BlockAccessor.GetBlock(pos.UpCopy());
		if (!upBlock.SideSolid[BlockFacing.DOWN.Index])
		{
			return upBlock is BlockGlowworms;
		}
		return true;
	}

	public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldGenRand, BlockPatchAttributes attributes = null)
	{
		bool didplace = false;
		if (blockAccessor.GetBlock(pos).Replaceable < 6000)
		{
			return false;
		}
		BlockPos npos = pos.Copy();
		for (int i = 0; i < 150 + worldGenRand.NextInt(30); i++)
		{
			npos.X = pos.X + worldGenRand.NextInt(11) - 5;
			npos.Y = pos.Y + worldGenRand.NextInt(11) - 5;
			npos.Z = pos.Z + worldGenRand.NextInt(11) - 5;
			if (npos.Y <= api.World.SeaLevel - 10 && npos.Y >= 25 && blockAccessor.GetBlock(npos).Replaceable >= 6000)
			{
				didplace |= TryGenGlowWorm(blockAccessor, npos, worldGenRand);
			}
		}
		return didplace;
	}

	private bool TryGenGlowWorm(IBlockAccessor blockAccessor, BlockPos pos, IRandom worldGenRand)
	{
		bool didplace = false;
		for (int dy = 0; dy < 5; dy++)
		{
			Block block = blockAccessor.GetBlockAbove(pos, dy, 1);
			if (block.SideSolid[BlockFacing.DOWN.Index])
			{
				GenHere(blockAccessor, pos.AddCopy(0, dy - 1, 0), worldGenRand);
				break;
			}
			if (block.Id != 0)
			{
				break;
			}
		}
		return didplace;
	}

	private void GenHere(IBlockAccessor blockAccessor, BlockPos pos, IRandom worldGenRand)
	{
		int rnd = worldGenRand.NextInt(bases.Length);
		Block placeblock = api.World.GetBlock(CodeWithVariant("type", bases[rnd]));
		blockAccessor.SetBlock(placeblock.Id, pos);
		if (segments[rnd] == null)
		{
			return;
		}
		placeblock = api.World.GetBlock(CodeWithVariant("type", segments[rnd]));
		int len = worldGenRand.NextInt(3);
		while (len-- > 0)
		{
			pos.Down();
			if (blockAccessor.GetBlock(pos).Replaceable > 6000)
			{
				blockAccessor.SetBlock(placeblock.Id, pos);
			}
		}
		pos.Down();
		placeblock = api.World.GetBlock(CodeWithVariant("type", ends[rnd]));
		if (blockAccessor.GetBlock(pos).Replaceable > 6000)
		{
			blockAccessor.SetBlock(placeblock.Id, pos);
		}
	}
}
