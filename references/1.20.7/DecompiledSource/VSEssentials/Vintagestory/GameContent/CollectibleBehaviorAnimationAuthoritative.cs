using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Vintagestory.GameContent;

public class CollectibleBehaviorAnimationAuthoritative : CollectibleBehavior
{
	public delegate void OnBeginHitEntityDelegate(EntityAgent byEntity, ref EnumHandling handling);

	protected AssetLocation strikeSound;

	public EnumHandInteract strikeSoundHandInteract = EnumHandInteract.HeldItemAttack;

	private bool onlyOnEntity;

	public event OnBeginHitEntityDelegate OnBeginHitEntity;

	public CollectibleBehaviorAnimationAuthoritative(CollectibleObject collObj)
		: base(collObj)
	{
	}

	public override void Initialize(JsonObject properties)
	{
		base.Initialize(properties);
		strikeSound = AssetLocation.Create(properties["strikeSound"].AsString("sounds/player/strike"), collObj.Code.Domain);
		onlyOnEntity = properties["onlyOnEntity"].AsBool();
	}

	public static float getHitDamageAtFrame(EntityAgent byEntity, string animCode)
	{
		if (byEntity.Properties.Client.AnimationsByMetaCode.TryGetValue(animCode, out var animdata))
		{
			JsonObject attributes = animdata.Attributes;
			if (attributes != null && attributes["damageAtFrame"].Exists)
			{
				return animdata.Attributes["damageAtFrame"].AsFloat(-1f) / animdata.AnimationSpeed;
			}
		}
		return -1f;
	}

	public static float getSoundAtFrame(EntityAgent byEntity, string animCode)
	{
		if (byEntity.Properties.Client.AnimationsByMetaCode.TryGetValue(animCode, out var animdata))
		{
			JsonObject attributes = animdata.Attributes;
			if (attributes != null && attributes["soundAtFrame"].Exists)
			{
				return animdata.Attributes["soundAtFrame"].AsFloat(-1f) / animdata.AnimationSpeed;
			}
		}
		return -1f;
	}

	public override string GetHeldTpUseAnimation(ItemSlot activeHotbarSlot, Entity byEntity, ref EnumHandling bhHandling)
	{
		bhHandling = EnumHandling.PreventDefault;
		return "interactstatic";
	}

	public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
	{
		if (!onlyOnEntity || entitySel != null)
		{
			StartAttack(slot, byEntity);
			handling = EnumHandling.PreventSubsequent;
			handHandling = EnumHandHandling.PreventDefault;
		}
	}

	public override bool OnHeldAttackCancel(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handling)
	{
		handling = EnumHandling.PreventSubsequent;
		return false;
	}

	public override bool OnHeldAttackStep(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
	{
		handling = EnumHandling.PreventSubsequent;
		return StepAttack(slot, byEntity);
	}

	public override void OnHeldAttackStop(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
	{
	}

	public void StartAttack(ItemSlot slot, EntityAgent byEntity)
	{
		string anim = collObj.GetHeldTpHitAnimation(slot, byEntity);
		byEntity.Attributes.SetInt("didattack", 0);
		byEntity.AnimManager.RegisterFrameCallback(new AnimFrameCallback
		{
			Animation = anim,
			Frame = getSoundAtFrame(byEntity, anim),
			Callback = delegate
			{
				playStrikeSound(byEntity);
			}
		});
		byEntity.AnimManager.RegisterFrameCallback(new AnimFrameCallback
		{
			Animation = anim,
			Frame = getHitDamageAtFrame(byEntity, anim),
			Callback = delegate
			{
				hitEntity(byEntity);
			}
		});
	}

	public bool StepAttack(ItemSlot slot, EntityAgent byEntity)
	{
		string animCode = collObj.GetHeldTpHitAnimation(slot, byEntity);
		return byEntity.AnimManager.IsAnimationActive(animCode);
	}

	protected virtual void playStrikeSound(EntityAgent byEntity)
	{
		IPlayer byPlayer = (byEntity as EntityPlayer).Player;
		if (byPlayer != null && byEntity.Controls.HandUse == strikeSoundHandInteract)
		{
			byPlayer.Entity.World.PlaySoundAt(strikeSound, byPlayer.Entity, byPlayer, 0.9f + (float)byEntity.World.Rand.NextDouble() * 0.2f, 16f, 0.35f);
		}
	}

	public virtual void hitEntity(EntityAgent byEntity)
	{
		EnumHandling handling = EnumHandling.PassThrough;
		this.OnBeginHitEntity?.Invoke(byEntity, ref handling);
		if (handling != 0)
		{
			return;
		}
		EntitySelection entitySel = (byEntity as EntityPlayer)?.EntitySelection;
		long selectedEntityId = (entitySel?.Entity?.EntityId).GetValueOrDefault();
		long mountEntityId = (byEntity?.MountedOn?.Entity?.EntityId).GetValueOrDefault();
		if (byEntity.World.Side == EnumAppSide.Client)
		{
			IClientWorldAccessor world = byEntity.World as IClientWorldAccessor;
			if (byEntity.Attributes.GetInt("didattack") == 0)
			{
				if (entitySel != null && selectedEntityId != mountEntityId)
				{
					world.TryAttackEntity(entitySel);
				}
				byEntity.Attributes.SetInt("didattack", 1);
				world.AddCameraShake(0.25f);
			}
		}
		else if (byEntity.Attributes.GetInt("didattack") == 0 && entitySel != null && selectedEntityId != mountEntityId)
		{
			byEntity.Attributes.SetInt("didattack", 1);
		}
	}
}
