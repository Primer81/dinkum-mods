using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockBehaviorHeatSource : BlockBehavior, IHeatSource
{
	private float heatStrength;

	public BlockBehaviorHeatSource(Block block)
		: base(block)
	{
	}

	public override void Initialize(JsonObject properties)
	{
		heatStrength = properties["heatStrength"].AsFloat();
		base.Initialize(properties);
	}

	public float GetHeatStrength(IWorldAccessor world, BlockPos heatSourcePos, BlockPos heatReceiverPos)
	{
		if (block.EntityClass != null && world.BlockAccessor.GetBlockEntity(heatSourcePos) is IHeatSource behs)
		{
			return behs.GetHeatStrength(world, heatSourcePos, heatReceiverPos);
		}
		return heatStrength;
	}
}
