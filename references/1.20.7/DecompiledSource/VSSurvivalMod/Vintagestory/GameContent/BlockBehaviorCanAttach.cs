using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

public class BlockBehaviorCanAttach : BlockBehavior
{
	private string[] sides;

	public BlockBehaviorCanAttach(Block block)
		: base(block)
	{
	}

	public override void Initialize(JsonObject properties)
	{
		sides = properties["sides"].AsArray(new string[0]);
		base.Initialize(properties);
	}

	public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace, ref EnumHandling handling, Cuboidi attachmentArea = null)
	{
		if (sides.Contains(blockFace.Code))
		{
			handling = EnumHandling.PreventDefault;
			return true;
		}
		handling = EnumHandling.PassThrough;
		return false;
	}
}
