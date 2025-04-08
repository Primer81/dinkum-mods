using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Vintagestory.GameContent;

public class EntityGlowingAgent : EntityAgent
{
	protected float aggroTime;

	protected bool onlyWhenAggroed;

	protected byte[] lightHsv = new byte[3] { 10, 5, 10 };

	public override byte[] LightHsv
	{
		get
		{
			if (!onlyWhenAggroed)
			{
				return lightHsv;
			}
			if (aggroTime <= 0f)
			{
				return null;
			}
			lightHsv[2] = (byte)(((double)aggroTime < 0.5) ? 5u : 8u);
			return lightHsv;
		}
	}

	public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
	{
		base.Initialize(properties, api, InChunkIndex3d);
		if (api.Side == EnumAppSide.Server)
		{
			GetBehavior<EntityBehaviorTaskAI>().TaskManager.OnTaskStarted += EntityGlowingAgent_OnTaskStarted;
		}
		else
		{
			WatchedAttributes.RegisterModifiedListener("aggroTime", delegate
			{
				aggroTime = WatchedAttributes.GetFloat("aggroTime");
			});
		}
		aggroTime = WatchedAttributes.GetFloat("aggroTime");
		lightHsv = properties.Attributes["lightHsv"].AsObject(new byte[3] { 10, 5, 10 });
		onlyWhenAggroed = properties.Attributes["onlyWhenAggroed"].AsBool(defaultValue: true);
	}

	private void EntityGlowingAgent_OnTaskStarted(IAiTask task)
	{
		if (task is AiTaskSeekEntity)
		{
			WatchedAttributes.SetFloat("aggroTime", 10f);
		}
	}

	public override void OnGameTick(float dt)
	{
		if (aggroTime > 0f)
		{
			aggroTime -= dt;
		}
		base.OnGameTick(dt);
	}
}
