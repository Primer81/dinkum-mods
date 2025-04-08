using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockTeleporter : Block
{
	public SimpleParticleProperties idleParticles;

	public SimpleParticleProperties insideParticles;

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		idleParticles = new SimpleParticleProperties(1f, 1f, ColorUtil.ToRgba(255, 34, 47, 44), new Vec3d(), new Vec3d(), new Vec3f(-0.1f, -0.1f, -0.1f), new Vec3f(0.1f, 0.1f, 0.1f), 1.5f, 0f, 0.5f, 0.75f);
		idleParticles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -0.6f);
		idleParticles.AddPos.Set(1.0, 2.0, 1.0);
		idleParticles.addLifeLength = 0.5f;
		insideParticles = new SimpleParticleProperties(1f, 1f, ColorUtil.ToRgba(255, 92, 111, 107), new Vec3d(), new Vec3d(), new Vec3f(-0.2f, -0.2f, -0.2f), new Vec3f(0.2f, 0.2f, 0.2f), 1.5f, 0f, 0.5f, 0.75f);
		insideParticles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -0.6f);
		insideParticles.AddPos.Set(1.0, 2.0, 1.0);
		insideParticles.addLifeLength = 0.5f;
	}

	public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
	{
		if (byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
		{
			TreeAttribute tree = new TreeAttribute();
			tree.SetInt("posX", blockSel.Position.X);
			tree.SetInt("posY", blockSel.Position.InternalY);
			tree.SetInt("posZ", blockSel.Position.Z);
			tree.SetString("playerUid", byPlayer.PlayerUID);
			api.Event.PushEvent("configTeleporter", tree);
			return true;
		}
		return base.OnBlockInteractStart(world, byPlayer, blockSel);
	}

	public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
	{
		base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
		if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityTeleporter be)
		{
			be.OnEntityCollide(entity);
		}
	}
}
