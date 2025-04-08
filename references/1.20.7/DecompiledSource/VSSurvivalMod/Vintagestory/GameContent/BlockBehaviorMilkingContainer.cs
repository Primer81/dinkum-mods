using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockBehaviorMilkingContainer : BlockBehavior
{
	private ICoreAPI api;

	private BlockLiquidContainerBase lcblock;

	public BlockBehaviorMilkingContainer(Block block)
		: base(block)
	{
	}

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		this.api = api;
		lcblock = block as BlockLiquidContainerBase;
		if (lcblock == null)
		{
			throw new InvalidOperationException($"Block with code {block.Code} has behavior MilkingContainer, but its block class does not inherit from BlockLiquidContainerBase, which is required");
		}
	}

	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
	{
		EntityBehaviorMilkable bh;
		if (entitySel == null || (bh = entitySel.Entity.GetBehavior<EntityBehaviorMilkable>()) == null)
		{
			return;
		}
		if (lcblock.GetContent(slot.Itemstack) != null)
		{
			if (api is ICoreClientAPI capi)
			{
				capi.TriggerIngameError(this, "useemptybucket", Lang.Get("Use an empty bucket for milking"));
			}
		}
		else
		{
			bh.TryBeginMilking();
			handling = EnumHandling.PreventDefault;
			handHandling = EnumHandHandling.PreventDefault;
		}
	}

	public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
	{
		EntityBehaviorMilkable bh;
		if (entitySel != null && (bh = entitySel.Entity.GetBehavior<EntityBehaviorMilkable>()) != null)
		{
			IPlayer player = (byEntity as EntityPlayer).Player;
			if (player == null)
			{
				return false;
			}
			if (!bh.CanContinueMilking(player, secondsUsed))
			{
				return false;
			}
			handling = EnumHandling.PreventDefault;
			if (api.Side == EnumAppSide.Client)
			{
				ModelTransform tf = new ModelTransform();
				tf.EnsureDefaultValues();
				tf.Translation.Set(-1f * (float)Easings.EaseOutBack(Math.Min(secondsUsed * 1.5f, 1f)), (float)Easings.EaseOutBack(Math.Min(1f, secondsUsed * 1.5f)) * 0.3f, -1f * (float)Easings.EaseOutBack(Math.Min(secondsUsed * 1.5f, 1f)));
				if (secondsUsed > 1f)
				{
					tf.Translation.X += (float)GameMath.MurmurHash3Mod((int)(secondsUsed * 3f), 0, 0, 10) / 600f;
					tf.Translation.Y += (float)GameMath.MurmurHash3Mod(0, (int)(secondsUsed * 3f), 0, 10) / 600f;
				}
				byEntity.Controls.UsingHeldItemTransformBefore = tf;
			}
			if (api.Side == EnumAppSide.Server)
			{
				return true;
			}
			return secondsUsed < 3f;
		}
		return false;
	}

	public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
	{
		EntityBehaviorMilkable bh;
		if (secondsUsed > 2.95f && entitySel != null && (bh = entitySel.Entity.GetBehavior<EntityBehaviorMilkable>()) != null)
		{
			bh.MilkingComplete(slot, byEntity);
			handling = EnumHandling.PreventDefault;
		}
	}
}
