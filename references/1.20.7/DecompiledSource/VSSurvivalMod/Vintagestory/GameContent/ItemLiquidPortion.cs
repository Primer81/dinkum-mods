using Vintagestory.API.Common;

namespace Vintagestory.GameContent;

internal class ItemLiquidPortion : Item
{
	public override void OnGroundIdle(EntityItem entityItem)
	{
		entityItem.Die(EnumDespawnReason.Removed);
		if (entityItem.World.Side == EnumAppSide.Server)
		{
			WaterTightContainableProps props = BlockLiquidContainerBase.GetContainableProps(entityItem.Itemstack);
			float litres = (float)entityItem.Itemstack.StackSize / props.ItemsPerLitre;
			entityItem.World.SpawnCubeParticles(entityItem.SidedPos.XYZ, entityItem.Itemstack, 0.75f, (int)(litres * 2f), 0.45f);
			entityItem.World.PlaySoundAt(new AssetLocation("sounds/environment/smallsplash"), (float)entityItem.SidedPos.X, (float)entityItem.SidedPos.InternalY, (float)entityItem.SidedPos.Z);
		}
		base.OnGroundIdle(entityItem);
	}
}
