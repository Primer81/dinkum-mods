using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BlockEntityAnvilPart : BlockEntityContainer
{
	public MultiTextureMeshRef BaseMeshRef;

	public MultiTextureMeshRef FluxMeshRef;

	public MultiTextureMeshRef TopMeshRef;

	private InventoryGeneric inv;

	public int hammerHits;

	private AnvilPartRenderer renderer;

	public override InventoryBase Inventory => inv;

	public override string InventoryClassName => "anvilpart";

	public BlockEntityAnvilPart()
	{
		inv = new InventoryGeneric(3, null, null);
	}

	public override void Initialize(ICoreAPI api)
	{
		base.Initialize(api);
		if (api is ICoreClientAPI capi)
		{
			renderer = new AnvilPartRenderer(capi, this);
			updateMeshRefs();
		}
	}

	private void updateMeshRefs()
	{
		if (Api.Side != EnumAppSide.Server)
		{
			ICoreClientAPI capi = Api as ICoreClientAPI;
			BaseMeshRef = capi.TesselatorManager.GetDefaultBlockMeshRef(base.Block);
			if (!inv[1].Empty && FluxMeshRef == null)
			{
				capi.Tesselator.TesselateShape(base.Block, Shape.TryGet(Api, "shapes/block/metal/anvil/build-flux.json"), out var meshdata2);
				FluxMeshRef = capi.Render.UploadMultiTextureMesh(meshdata2);
			}
			if (!inv[2].Empty && TopMeshRef == null)
			{
				capi.Tesselator.TesselateShape(base.Block, Shape.TryGet(Api, "shapes/block/metal/anvil/build-top.json"), out var meshdata);
				TopMeshRef = capi.Render.UploadMultiTextureMesh(meshdata);
			}
		}
	}

	public override void OnBlockPlaced(ItemStack byItemStack = null)
	{
		if (byItemStack != null)
		{
			inv[0].Itemstack = byItemStack.Clone();
			inv[0].Itemstack.StackSize = 1;
		}
	}

	public void OnHammerHitOver(IPlayer byPlayer, Vec3d hitPosition)
	{
		if (!inv[1].Empty && !inv[2].Empty && TestReadyToMerge(triggerMessage: false))
		{
			hammerHits++;
			float temp = inv[2].Itemstack.Collectible.GetTemperature(Api.World, inv[2].Itemstack);
			if (temp > 800f)
			{
				BlockEntityAnvil.bigMetalSparks.MinPos = Pos.ToVec3d().Add(hitPosition.X, hitPosition.Y, hitPosition.Z);
				BlockEntityAnvil.bigMetalSparks.VertexFlags = (byte)GameMath.Clamp((int)(temp - 700f) / 2, 32, 128);
				Api.World.SpawnParticles(BlockEntityAnvil.bigMetalSparks, byPlayer);
				BlockEntityAnvil.smallMetalSparks.MinPos = Pos.ToVec3d().Add(hitPosition.X, hitPosition.Y, hitPosition.Z);
				BlockEntityAnvil.smallMetalSparks.VertexFlags = (byte)GameMath.Clamp((int)(temp - 770f) / 3, 32, 128);
				Api.World.SpawnParticles(BlockEntityAnvil.smallMetalSparks, byPlayer);
			}
			if (hammerHits > 11 && Api.Side == EnumAppSide.Server)
			{
				Api.World.BlockAccessor.SetBlock(Api.World.GetBlock(new AssetLocation("anvil-" + base.Block.Variant["metal"])).Id, Pos);
			}
		}
	}

	public bool TestReadyToMerge(bool triggerMessage = true)
	{
		if (inv[0].Itemstack.Collectible.GetTemperature(Api.World, inv[0].Itemstack) < 800f)
		{
			if (triggerMessage && Api is ICoreClientAPI capi)
			{
				capi.TriggerIngameError(this, "bottomtoocold", Lang.GetWithFallback("weldanvil-bottomtoocold", "Bottom half to cold to weld, reheat the part on the forge."));
			}
			return false;
		}
		if (inv[1].Empty)
		{
			if (triggerMessage && Api is ICoreClientAPI capi2)
			{
				capi2.TriggerIngameError(this, "fluxmissing", Lang.GetWithFallback("weldanvil-fluxmissing", "Must apply powdered borax as next step."));
			}
			return false;
		}
		if (inv[2].Empty)
		{
			if (triggerMessage && Api is ICoreClientAPI capi3)
			{
				capi3.TriggerIngameError(this, "tophalfmissing", Lang.GetWithFallback("weldanvil-tophalfmissing", "Add the top half anvil first."));
			}
			return false;
		}
		if (inv[2].Itemstack.Collectible.GetTemperature(Api.World, inv[2].Itemstack) < 800f)
		{
			if (triggerMessage && Api is ICoreClientAPI capi4)
			{
				capi4.TriggerIngameError(this, "toptoocold", Lang.GetWithFallback("weldanvil-toptoocold", "Top half to cold to weld, reheat the part on the forge."));
			}
			return false;
		}
		return true;
	}

	public bool OnInteract(IPlayer byPlayer)
	{
		if (base.Block.Variant["part"] != "base")
		{
			return false;
		}
		ItemSlot hotbarslot = byPlayer.InventoryManager.ActiveHotbarSlot;
		if (inv[0].Itemstack.Collectible.GetTemperature(Api.World, inv[0].Itemstack) < 800f)
		{
			if (Api.Side == EnumAppSide.Client)
			{
				(Api as ICoreClientAPI).TriggerIngameError(this, "bottomtoocold", Lang.GetWithFallback("weldanvil-bottomtoocold", "Bottom half to cold to weld, reheat the part on the forge."));
			}
			return false;
		}
		if (inv[1].Empty)
		{
			ItemStack itemstack = hotbarslot.Itemstack;
			if (itemstack != null && (itemstack.Collectible?.Attributes?.IsTrue("isFlux")).GetValueOrDefault())
			{
				inv[1].Itemstack = hotbarslot.TakeOut(1);
				updateMeshRefs();
				return true;
			}
			if (Api.Side == EnumAppSide.Client)
			{
				(Api as ICoreClientAPI).TriggerIngameError(this, "fluxmissing", Lang.GetWithFallback("weldanvil-fluxmissing", "Must apply powdered borax as next step."));
			}
			return false;
		}
		if (inv[2].Empty)
		{
			if (hotbarslot.Itemstack?.Block is BlockAnvilPart && hotbarslot.Itemstack.Block.Variant["part"] == "top")
			{
				Api.World.PlaySoundAt(base.Block.Sounds.Place, Pos, 0.0, byPlayer);
				inv[2].Itemstack = hotbarslot.TakeOut(1);
				updateMeshRefs();
				return true;
			}
			return false;
		}
		return true;
	}

	public override void OnBlockUnloaded()
	{
		base.OnBlockUnloaded();
		renderer?.Dispose();
		FluxMeshRef?.Dispose();
		TopMeshRef?.Dispose();
	}

	public override void OnBlockRemoved()
	{
		base.OnBlockRemoved();
		renderer?.Dispose();
		FluxMeshRef?.Dispose();
		TopMeshRef?.Dispose();
	}

	public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
	{
		base.FromTreeAttributes(tree, worldForResolving);
	}

	public override void ToTreeAttributes(ITreeAttribute tree)
	{
		base.ToTreeAttributes(tree);
	}

	public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
	{
		return true;
	}

	public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
	{
	}
}
