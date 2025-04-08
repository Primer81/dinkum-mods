using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

public class ItemHoneyComb : Item
{
	public float ContainedHoneyLitres = 0.2f;

	private WorldInteraction[] interactions;

	public bool CanSqueezeInto(Block block, BlockSelection blockSel)
	{
		BlockPos pos = blockSel?.Position;
		if (block is BlockLiquidContainerTopOpened blcto)
		{
			if (!(pos == null))
			{
				return !blcto.IsFull(pos);
			}
			return true;
		}
		if (pos != null && api.World.BlockAccessor.GetBlockEntity(pos) is BlockEntityGroundStorage beg)
		{
			ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);
			if (squeezeIntoSlot?.Itemstack?.Block is BlockLiquidContainerTopOpened bowl)
			{
				return !bowl.IsFull(squeezeIntoSlot.Itemstack);
			}
		}
		return false;
	}

	public override void OnLoaded(ICoreAPI api)
	{
		if (api.Side != EnumAppSide.Client)
		{
			return;
		}
		_ = api;
		interactions = ObjectCacheUtil.GetOrCreate(api, "honeyCombInteractions", delegate
		{
			List<ItemStack> list = new List<ItemStack>();
			foreach (Block current in api.World.Blocks)
			{
				if (!(current.Code == null) && CanSqueezeInto(current, null))
				{
					list.Add(new ItemStack(current));
				}
			}
			return new WorldInteraction[1]
			{
				new WorldInteraction
				{
					ActionLangCode = "heldhelp-squeeze",
					HotKeyCode = "shift",
					MouseButton = EnumMouseButton.Right,
					Itemstacks = list.ToArray()
				}
			};
		});
	}

	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
	{
		if (blockSel?.Block != null && CanSqueezeInto(blockSel.Block, blockSel) && byEntity.Controls.ShiftKey)
		{
			handling = EnumHandHandling.PreventDefault;
			if (api.World.Side == EnumAppSide.Client)
			{
				byEntity.World.PlaySoundAt(new AssetLocation("sounds/player/squeezehoneycomb"), byEntity, null, randomizePitch: true, 16f, 0.5f);
			}
		}
		else
		{
			base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
		}
	}

	public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
	{
		if (blockSel?.Block != null && CanSqueezeInto(blockSel.Block, blockSel))
		{
			if (!byEntity.Controls.ShiftKey)
			{
				return false;
			}
			if (byEntity.World is IClientWorldAccessor)
			{
				byEntity.StartAnimation("squeezehoneycomb");
			}
			return secondsUsed < 2f;
		}
		return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
	}

	public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
	{
		byEntity.StopAnimation("squeezehoneycomb");
		if (blockSel != null)
		{
			Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
			if (CanSqueezeInto(block, blockSel))
			{
				if (secondsUsed < 1.9f)
				{
					return;
				}
				IWorldAccessor world = byEntity.World;
				if (!CanSqueezeInto(block, blockSel))
				{
					return;
				}
				ItemStack honeyStack = new ItemStack(world.GetItem(new AssetLocation("honeyportion")), 99999);
				if (block is BlockLiquidContainerTopOpened blockCnt2)
				{
					if (blockCnt2.TryPutLiquid(blockSel.Position, honeyStack, ContainedHoneyLitres) == 0)
					{
						return;
					}
				}
				else if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundStorage beg)
				{
					ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);
					if (squeezeIntoSlot != null && squeezeIntoSlot?.Itemstack?.Block != null && CanSqueezeInto(squeezeIntoSlot.Itemstack.Block, null))
					{
						BlockLiquidContainerTopOpened blockCnt = squeezeIntoSlot.Itemstack.Block as BlockLiquidContainerTopOpened;
						blockCnt.TryPutLiquid(squeezeIntoSlot.Itemstack, honeyStack, ContainedHoneyLitres);
						beg.MarkDirty(redrawOnClient: true);
					}
				}
				slot.TakeOut(1);
				slot.MarkDirty();
				IPlayer byPlayer = null;
				if (byEntity is EntityPlayer)
				{
					byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
				}
				ItemStack stack = new ItemStack(world.GetItem(new AssetLocation("beeswax")));
				if (byPlayer != null && !byPlayer.InventoryManager.TryGiveItemstack(stack))
				{
					byEntity.World.SpawnItemEntity(stack, byEntity.SidedPos.XYZ);
				}
				return;
			}
		}
		base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
	}

	public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
	{
		byEntity.StopAnimation("squeezehoneycomb");
		return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);
	}

	public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
	{
		return interactions.Append(base.GetHeldInteractionHelp(inSlot));
	}
}
