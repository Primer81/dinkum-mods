using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

public class BlockEntityBookshelf : BlockEntityDisplay, IRotatable
{
	private InventoryGeneric inv;

	private Block block;

	private MeshData mesh;

	private string type;

	private string material;

	private float[] mat;

	public override InventoryBase Inventory => inv;

	public override string InventoryClassName => "bookshelf";

	public override string AttributeTransformCode => "onshelfTransform";

	public float MeshAngleRad { get; set; }

	public string Type => type;

	public string Material => material;

	public int[] UsableSlots
	{
		get
		{
			if (!(block is BlockBookshelf bs))
			{
				return new int[0];
			}
			bs.UsableSlots.TryGetValue(type, out var slots);
			return slots;
		}
	}

	public BlockEntityBookshelf()
	{
		inv = new InventoryGeneric(14, "bookshelf-0", null);
	}

	private void initShelf()
	{
		if (Api == null || type == null || !(base.Block is BlockBookshelf bookshelf))
		{
			return;
		}
		if (Api.Side == EnumAppSide.Client)
		{
			mesh = bookshelf.GetOrCreateMesh(type, material);
			mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(MeshAngleRad)
				.Translate(-0.5f, -0.5f, -0.5f)
				.Values;
		}
		if (!bookshelf.UsableSlots.ContainsKey(type))
		{
			type = bookshelf.UsableSlots.First().Key;
		}
		int[] usableslots = UsableSlots;
		for (int i = 0; i < Inventory.Count; i++)
		{
			if (!usableslots.Contains(i))
			{
				Inventory[i].MaxSlotStackSize = 0;
			}
		}
	}

	public override void Initialize(ICoreAPI api)
	{
		block = api.World.BlockAccessor.GetBlock(Pos);
		base.Initialize(api);
		if (mesh == null && type != null)
		{
			initShelf();
		}
	}

	public override void OnBlockPlaced(ItemStack byItemStack = null)
	{
		base.OnBlockPlaced(byItemStack);
		if (type == null)
		{
			type = byItemStack?.Attributes.GetString("type");
		}
		if (material == null)
		{
			material = byItemStack?.Attributes.GetString("material");
		}
		initShelf();
	}

	internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
	{
		ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
		CollectibleObject colObj = slot.Itemstack?.Collectible;
		bool shelvable = colObj?.Attributes != null && colObj.Attributes["bookshelveable"].AsBool();
		if (slot.Empty || !shelvable)
		{
			if (TryTake(byPlayer, blockSel))
			{
				return true;
			}
			return false;
		}
		if (shelvable)
		{
			AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;
			if (TryPut(slot, blockSel))
			{
				Api.World.PlaySoundAt((sound != null) ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, randomizePitch: true, 16f);
				int blockSelSelectionBoxIndex = blockSel.SelectionBoxIndex - 5;
				Api.World.Logger.Audit("{0} Put 1x{1} into Bookshelf slotid {2} at {3}.", byPlayer.PlayerName, inv[blockSelSelectionBoxIndex].Itemstack.Collectible.Code, blockSelSelectionBoxIndex, Pos);
				return true;
			}
			return false;
		}
		return false;
	}

	private bool TryPut(ItemSlot slot, BlockSelection blockSel)
	{
		int index = blockSel.SelectionBoxIndex - 5;
		if (index < 0 || index >= inv.Count)
		{
			return false;
		}
		if (!UsableSlots.Contains(index))
		{
			return false;
		}
		for (int i = 0; i < inv.Count; i++)
		{
			int slotnum = (index + i) % inv.Count;
			if (inv[slotnum].Empty)
			{
				int num = slot.TryPutInto(Api.World, inv[slotnum]);
				MarkDirty();
				return num > 0;
			}
		}
		return false;
	}

	private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
	{
		int index = blockSel.SelectionBoxIndex - 5;
		if (index < 0 || index >= inv.Count)
		{
			return false;
		}
		if (!inv[index].Empty)
		{
			ItemStack stack = inv[index].TakeOut(1);
			if (byPlayer.InventoryManager.TryGiveItemstack(stack))
			{
				AssetLocation sound = stack.Block?.Sounds?.Place;
				Api.World.PlaySoundAt((sound != null) ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, randomizePitch: true, 16f);
			}
			Api.World.Logger.Audit("{0} Took 1x{1} from Bookshelf slotid {2} at {3}.", byPlayer.PlayerName, stack.Collectible.Code, index, Pos);
			if (stack.StackSize > 0)
			{
				Api.World.SpawnItemEntity(stack, Pos);
			}
			MarkDirty();
			return true;
		}
		return false;
	}

	protected override float[][] genTransformationMatrices()
	{
		tfMatrices = new float[Inventory.Count][];
		for (int i = 0; i < Inventory.Count; i++)
		{
			float x = (float)(i % 7) * 2f / 16f + 0.0625f - 0.5f + 0.0625f;
			float y = (float)(i / 7) * 7.5f / 16f + 0.0625f;
			float z = -0.25f;
			Vec3f off = new Vec3f(x, y, z);
			off = new Matrixf().RotateY(MeshAngleRad).TransformVector(off.ToVec4f(0f)).XYZ;
			tfMatrices[i] = new Matrixf().Translate(off.X, off.Y, off.Z).Translate(0.5f, 0f, 0.5f).RotateY(MeshAngleRad)
				.Translate(-0.5f, 0f, -0.5f)
				.Values;
		}
		return tfMatrices;
	}

	public override void ToTreeAttributes(ITreeAttribute tree)
	{
		base.ToTreeAttributes(tree);
		tree.SetString("type", type);
		tree.SetString("material", material);
		tree.SetFloat("meshAngleRad", MeshAngleRad);
	}

	public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
	{
		base.FromTreeAttributes(tree, worldForResolving);
		type = tree.GetString("type");
		material = tree.GetString("material");
		MeshAngleRad = tree.GetFloat("meshAngleRad");
		initShelf();
		RedrawAfterReceivingTreeAttributes(worldForResolving);
	}

	public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
	{
		mesher.AddMeshData(mesh, mat);
		base.OnTesselation(mesher, tessThreadTesselator);
		return true;
	}

	public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
	{
		if (forPlayer.CurrentBlockSelection == null)
		{
			base.GetBlockInfo(forPlayer, sb);
			return;
		}
		int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex - 5;
		if (index < 0 || index >= inv.Count)
		{
			base.GetBlockInfo(forPlayer, sb);
			return;
		}
		ItemSlot slot = inv[index];
		if (slot.Empty)
		{
			sb.AppendLine(Lang.Get("Empty"));
		}
		else
		{
			sb.AppendLine(slot.Itemstack.GetName());
		}
	}

	public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
	{
		MeshAngleRad = tree.GetFloat("meshAngleRad");
		MeshAngleRad -= (float)degreeRotation * ((float)Math.PI / 180f);
		tree.SetFloat("meshAngleRad", MeshAngleRad);
	}
}
