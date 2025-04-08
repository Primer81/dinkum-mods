using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Vintagestory.GameContent;

public class BECheeseCurdsBundle : BlockEntityContainer
{
	private static SimpleParticleProperties props;

	private InventoryGeneric inv;

	private bool squeezed;

	private EnumCurdsBundleState state;

	private float meshangle;

	private Vec3f animRot = new Vec3f();

	private long listenerId;

	private float secondsPassed;

	public override InventoryBase Inventory => inv;

	public override string InventoryClassName => "curdsbundle";

	public bool Rotten
	{
		get
		{
			bool rotten = false;
			for (int i = 0; i < Inventory.Count; i++)
			{
				rotten |= Inventory[i].Itemstack?.Collectible.Code.Path == "rot";
			}
			return rotten;
		}
	}

	public virtual float MeshAngle
	{
		get
		{
			return meshangle;
		}
		set
		{
			meshangle = value;
			animRot.Y = value * (180f / (float)Math.PI);
		}
	}

	public bool Squuezed => squeezed;

	public EnumCurdsBundleState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
			MarkDirty(redrawOnClient: true);
		}
	}

	private BlockEntityAnimationUtil animUtil => GetBehavior<BEBehaviorAnimatable>()?.animUtil;

	static BECheeseCurdsBundle()
	{
		props = new SimpleParticleProperties(0.5f, 1.3f, ColorUtil.ColorFromRgba(248, 243, 227, 255), new Vec3d(), new Vec3d(), new Vec3f(), new Vec3f(), 2f, 1f, 0.05f, 0.2f, EnumParticleModel.Quad);
	}

	public BECheeseCurdsBundle()
	{
		inv = new InventoryGeneric(1, null, null);
	}

	public override void Initialize(ICoreAPI api)
	{
		base.Initialize(api);
		inv.LateInitialize("curdsbundle-" + Pos, api);
		if (api.Side == EnumAppSide.Client)
		{
			animUtil?.InitializeAnimator("curdbundle", (base.Block as BlockCheeseCurdsBundle).GetShape(EnumCurdsBundleState.BundledStick), null, animRot);
		}
	}

	public override void OnBlockPlaced(ItemStack byItemStack = null)
	{
		if (byItemStack != null)
		{
			inv[0].Itemstack = (byItemStack.Block as BlockCheeseCurdsBundle)?.GetContents(byItemStack);
		}
	}

	internal void StartSqueeze(IPlayer byPlayer)
	{
		if (state == EnumCurdsBundleState.BundledStick && listenerId == 0L)
		{
			if (Api.Side == EnumAppSide.Client)
			{
				startSqueezeAnim();
			}
			else
			{
				(Api as ICoreServerAPI).Network.BroadcastBlockEntityPacket(Pos, 1010);
			}
			Api.World.PlaySoundAt(new AssetLocation("sounds/player/wetclothsqueeze.ogg"), (double)Pos.X + 0.5, (double)Pos.InternalY + 0.5, (double)Pos.Z + 0.5, byPlayer, randomizePitch: false);
			listenerId = Api.World.RegisterGameTickListener(onSqueezing, 20);
			secondsPassed = 0f;
		}
	}

	private void startSqueezeAnim()
	{
		animUtil.StartAnimation(new AnimationMetaData
		{
			Animation = "twist",
			Code = "twist",
			AnimationSpeed = 0.25f,
			EaseOutSpeed = 3f,
			EaseInSpeed = 3f
		});
	}

	public override void OnReceivedServerPacket(int packetid, byte[] data)
	{
		if (packetid == 1010)
		{
			startSqueezeAnim();
		}
		base.OnReceivedServerPacket(packetid, data);
	}

	private void onSqueezing(float dt)
	{
		secondsPassed += dt;
		if (secondsPassed > 5f)
		{
			animUtil?.StopAnimation("twist");
			squeezed = true;
			Api.World.UnregisterGameTickListener(listenerId);
		}
		if (Api.Side == EnumAppSide.Server)
		{
			Vec3d pos = RandomParticlePos(BlockFacing.HORIZONTALS[Api.World.Rand.Next(4)]);
			props.MinPos.Set(pos.X, (float)Pos.Y + 1f / 32f, pos.Z);
			props.AddPos.Set(0.0, 0.375, 0.0);
			Api.World.SpawnParticles(props);
		}
	}

	public override void OnBlockRemoved()
	{
		base.OnBlockRemoved();
		Api.World.UnregisterGameTickListener(listenerId);
	}

	public override void OnBlockBroken(IPlayer byPlayer = null)
	{
		base.OnBlockBroken(byPlayer);
		ItemStack stack = new ItemStack(Api.World.GetBlock(new AssetLocation("linen-normal-down")));
		Api.World.SpawnItemEntity(stack, Pos.ToVec3d().AddCopy(0.5, 0.25, 0.5));
	}

	public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
	{
		base.FromTreeAttributes(tree, worldForResolving);
		squeezed = tree.GetBool("squeezezd");
		state = (EnumCurdsBundleState)tree.GetInt("state");
		MeshAngle = tree.GetFloat("meshAngle", MeshAngle);
	}

	public override void ToTreeAttributes(ITreeAttribute tree)
	{
		base.ToTreeAttributes(tree);
		tree.SetBool("squeezezd", squeezed);
		tree.SetInt("state", (int)state);
		tree.SetFloat("meshAngle", MeshAngle);
	}

	public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
	{
		if (!base.OnTesselation(mesher, tessThreadTesselator))
		{
			mesher.AddMeshData((base.Block as BlockCheeseCurdsBundle).GetMesh(state, meshangle));
		}
		return true;
	}

	public Vec3d RandomParticlePos(BlockFacing facing = null)
	{
		Random rand = Api.World.Rand;
		IBlockAccessor blockAccess = Api.World.BlockAccessor;
		Cuboidf box = base.Block.GetParticleBreakBox(blockAccess, Pos, facing);
		if (facing == null)
		{
			return new Vec3d((double)((float)Pos.X + box.X1 + 1f / 32f) + rand.NextDouble() * (double)(box.XSize - 0.0625f), (double)((float)Pos.Y + box.Y1 + 1f / 32f) + rand.NextDouble() * (double)(box.YSize - 0.0625f), (double)((float)Pos.Z + box.Z1 + 1f / 32f) + rand.NextDouble() * (double)(box.ZSize - 0.0625f));
		}
		bool haveBox = box != null;
		Vec3i facev = facing.Normali;
		Vec3d vec3d = new Vec3d((float)Pos.X + 0.5f + (float)facev.X / 1.9f + ((!haveBox || facing.Axis != 0) ? 0f : ((facev.X > 0) ? (box.X2 - 1f) : box.X1)), (float)Pos.Y + 0.5f + (float)facev.Y / 1.9f + ((!haveBox || facing.Axis != EnumAxis.Y) ? 0f : ((facev.Y > 0) ? (box.Y2 - 1f) : box.Y1)), (float)Pos.Z + 0.5f + (float)facev.Z / 1.9f + ((!haveBox || facing.Axis != EnumAxis.Z) ? 0f : ((facev.Z > 0) ? (box.Z2 - 1f) : box.Z1)));
		vec3d.Add((rand.NextDouble() - 0.5) * (double)(1 - Math.Abs(facev.X)), (rand.NextDouble() - 0.5) * (double)(1 - Math.Abs(facev.Y)) - (double)((facing == BlockFacing.DOWN) ? 0.1f : 0f), (rand.NextDouble() - 0.5) * (double)(1 - Math.Abs(facev.Z)));
		return vec3d;
	}
}
