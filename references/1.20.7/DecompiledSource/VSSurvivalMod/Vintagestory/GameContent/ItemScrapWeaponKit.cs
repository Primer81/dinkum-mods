using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class ItemScrapWeaponKit : Item
{
	private float curX;

	private float curY;

	private float prevSecUsed;

	private LCGRandom rnd;

	private ItemStack[] craftResultStacks;

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		rnd = new LCGRandom(api.World.Seed);
		JsonItemStack[] jstacks = Attributes["craftingResults"].AsObject<JsonItemStack[]>();
		List<ItemStack> stacklist = new List<ItemStack>();
		foreach (JsonItemStack jstack in jstacks)
		{
			jstack.Resolve(api.World, "Scrap weapon kit craft result");
			if (jstack.ResolvedItemstack != null)
			{
				stacklist.Add(jstack.ResolvedItemstack);
			}
		}
		craftResultStacks = stacklist.ToArray();
	}

	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
	{
		if (slot.Itemstack.TempAttributes.GetBool("consumed"))
		{
			return;
		}
		handling = EnumHandHandling.PreventDefault;
		IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
		if (byPlayer == null)
		{
			return;
		}
		byEntity.World.RegisterCallback(delegate
		{
			if (byEntity.Controls.HandUse == EnumHandInteract.HeldItemInteract)
			{
				byPlayer.Entity.World.PlaySoundAt(new AssetLocation("sounds/player/messycraft"), byPlayer, byPlayer);
			}
		}, 250);
	}

	public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
	{
		if (byEntity.World is IClientWorldAccessor)
		{
			ModelTransform tf = new ModelTransform();
			tf.EnsureDefaultValues();
			float nowx = 0f;
			float nowy = 0f;
			if (secondsUsed > 0.3f)
			{
				int cnt = (int)(secondsUsed * 10f);
				rnd.InitPositionSeed(cnt, 0);
				float targetx = 3f * (rnd.NextFloat() - 0.5f);
				float targety = 1.5f * (rnd.NextFloat() - 0.5f);
				float dt = secondsUsed - prevSecUsed;
				nowx = (curX - targetx) * dt * 2f;
				nowy = (curY - targety) * dt * 2f;
			}
			tf.Translation.Set(nowx - Math.Min(1.5f, secondsUsed * 4f), nowy, 0f);
			byEntity.Controls.UsingHeldItemTransformBefore = tf;
			curX = nowx;
			curY = nowy;
			prevSecUsed = secondsUsed;
		}
		if (api.World.Side == EnumAppSide.Server)
		{
			return true;
		}
		return secondsUsed < 4.6f;
	}

	public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
	{
		return false;
	}

	public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
	{
		if (secondsUsed > 4.5f)
		{
			if (api.Side == EnumAppSide.Server)
			{
				ItemStack resultstack = craftResultStacks[api.World.Rand.Next(craftResultStacks.Length)];
				slot.Itemstack = resultstack.Clone();
				slot.MarkDirty();
			}
			else
			{
				slot.Itemstack.TempAttributes.SetBool("consumed", value: true);
			}
		}
	}

	public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
	{
		return new WorldInteraction[1]
		{
			new WorldInteraction
			{
				ActionLangCode = "heldhelp-craftscrapweapon",
				MouseButton = EnumMouseButton.Right
			}
		};
	}
}
