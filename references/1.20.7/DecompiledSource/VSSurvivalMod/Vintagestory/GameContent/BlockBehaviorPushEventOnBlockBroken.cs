using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockBehaviorPushEventOnBlockBroken : BlockBehavior
{
	private string eventName;

	public BlockBehaviorPushEventOnBlockBroken(Block block)
		: base(block)
	{
	}

	public override void Initialize(JsonObject properties)
	{
		base.Initialize(properties);
		eventName = properties["eventName"]?.AsString();
	}

	public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
	{
		handling = EnumHandling.PassThrough;
		if (byPlayer != null)
		{
			TreeAttribute tree = new TreeAttribute();
			tree.SetInt("x", pos.X);
			tree.SetInt("y", pos.Y);
			tree.SetInt("z", pos.Z);
			world.Api.Event.PushEvent(eventName, tree);
		}
	}
}
