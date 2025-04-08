using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockLupine : BlockPlant
{
	private Block[] uncommonVariants;

	private Block[] rareVariants;

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		uncommonVariants = new Block[2]
		{
			api.World.GetBlock(CodeWithVariant("color", "white")),
			api.World.GetBlock(CodeWithVariant("color", "red"))
		};
		rareVariants = new Block[1] { api.World.GetBlock(CodeWithVariant("color", "orange")) };
	}

	public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, IRandom worldGenRand, BlockPatchAttributes attributes = null)
	{
		if (!base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand, attributes))
		{
			return false;
		}
		double rnd = worldGenRand.NextDouble();
		if (rnd < 1.0 / 300.0)
		{
			GenRareColorPatch(blockAccessor, pos, rareVariants[worldGenRand.NextInt(rareVariants.Length)], worldGenRand);
		}
		else if (rnd < 1.0 / 120.0)
		{
			GenRareColorPatch(blockAccessor, pos, uncommonVariants[worldGenRand.NextInt(uncommonVariants.Length)], worldGenRand);
		}
		return true;
	}

	private void GenRareColorPatch(IBlockAccessor blockAccessor, BlockPos pos, Block block, IRandom worldGenRand)
	{
		int cnt = 2 + worldGenRand.NextInt(6);
		int tries = 30;
		BlockPos npos = pos.Copy();
		while (cnt > 0 && tries-- > 0)
		{
			npos.Set(pos).Add(worldGenRand.NextInt(5) - 2, 0, worldGenRand.NextInt(5) - 2);
			npos.Y = blockAccessor.GetTerrainMapheightAt(npos) + 1;
			Block nblock = blockAccessor.GetBlock(npos);
			if ((nblock.IsReplacableBy(block) || nblock is BlockLupine) && CanPlantStay(blockAccessor, npos))
			{
				blockAccessor.SetBlock(block.BlockId, npos);
				cnt--;
			}
		}
	}
}
