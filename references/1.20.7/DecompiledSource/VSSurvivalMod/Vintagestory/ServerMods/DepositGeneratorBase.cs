using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Vintagestory.ServerMods;

public abstract class DepositGeneratorBase
{
	public ICoreServerAPI Api;

	protected const int chunksize = 32;

	public LCGRandom DepositRand;

	public NormalizedSimplexNoise DistortNoiseGen;

	protected DepositVariant variant;

	public bool blockCallBacks = true;

	public DepositGeneratorBase(ICoreServerAPI api, DepositVariant variant, LCGRandom depositRand, NormalizedSimplexNoise noiseGen)
	{
		this.variant = variant;
		Api = api;
		DepositRand = depositRand;
		DistortNoiseGen = noiseGen;
	}

	public abstract void GenDeposit(IBlockAccessor blockAccessor, IServerChunk[] chunks, int originChunkX, int originChunkZ, BlockPos pos, ref Dictionary<BlockPos, DepositVariant> subDepositsToPlace);

	public virtual void Init()
	{
	}

	public virtual DepositVariant[] Resolve(DepositVariant sourceVariant)
	{
		return new DepositVariant[1] { sourceVariant };
	}

	public abstract float GetMaxRadius();

	public abstract void GetPropickReading(BlockPos pos, int oreDist, int[] blockColumn, out double ppt, out double totalFactor);

	public abstract void GetYMinMax(BlockPos pos, out double miny, out double maxy);
}
