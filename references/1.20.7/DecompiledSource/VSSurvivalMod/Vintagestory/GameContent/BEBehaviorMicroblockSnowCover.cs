using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent;

public class BEBehaviorMicroblockSnowCover : BlockEntityBehavior, IRotatable, IMicroblockBehavior
{
	public int SnowLevel;

	public int PrevSnowLevel;

	public int snowLayerBlockId;

	public List<uint> SnowCuboids = new List<uint>();

	public List<uint> GroundSnowCuboids = new List<uint>();

	public MeshData SnowMesh;

	private BlockEntityMicroBlock beMicroBlock;

	private CuboidWithMaterial[] aboveCuboids;

	public BEBehaviorMicroblockSnowCover(BlockEntity blockentity)
		: base(blockentity)
	{
		beMicroBlock = blockentity as BlockEntityMicroBlock;
	}

	public override void Initialize(ICoreAPI api, JsonObject properties)
	{
		base.Initialize(api, properties);
		SnowLevel = (int)base.Block.snowLevel;
		snowLayerBlockId = (base.Block as BlockMicroBlock)?.snowLayerBlockId ?? 0;
	}

	public void RotateModel(int degrees, EnumAxis? flipAroundAxis)
	{
		if (flipAroundAxis.HasValue)
		{
			SnowCuboids = new List<uint>();
			GroundSnowCuboids = new List<uint>();
			SnowLevel = 0;
			if (Api != null)
			{
				Api.World.BlockAccessor.ExchangeBlock((base.Block as BlockMicroBlock).notSnowCovered.Id, base.Pos);
			}
		}
		else
		{
			beMicroBlock.TransformList(degrees, flipAroundAxis, SnowCuboids);
			beMicroBlock.TransformList(degrees, flipAroundAxis, GroundSnowCuboids);
		}
	}

	public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int byDegrees, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAroundAxis)
	{
		uint[] snowcuboidValues = (tree["snowcuboids"] as IntArrayAttribute)?.AsUint;
		SnowCuboids = ((snowcuboidValues == null) ? new List<uint>(0) : new List<uint>(snowcuboidValues));
		uint[] groundsnowvalues = (tree["groundSnowCuboids"] as IntArrayAttribute)?.AsUint;
		GroundSnowCuboids = ((groundsnowvalues == null) ? new List<uint>(0) : new List<uint>(groundsnowvalues));
		tree["snowcuboids"] = new IntArrayAttribute(SnowCuboids.ToArray());
		tree["groundSnowCuboids"] = new IntArrayAttribute(GroundSnowCuboids.ToArray());
	}

	private void buildSnowCuboids(BoolArray16x16x16 Voxels)
	{
		List<uint> snowCuboids = new List<uint>();
		List<uint> groundSnowCuboids = new List<uint>();
		BlockEntityMicroBlock aboveBe = Api?.World.BlockAccessor.GetBlockEntity(base.Pos.UpCopy()) as BlockEntityMicroBlock;
		CuboidWithMaterial[] newAboveCuboids = null;
		bool[,] snowVoxelVisited = new bool[16, 16];
		for (int dy = 15; dy >= 0; dy--)
		{
			for (int dx = 0; dx < 16; dx++)
			{
				for (int dz = 0; dz < 16; dz++)
				{
					if (snowVoxelVisited[dx, dz])
					{
						continue;
					}
					bool ground = dy == 0 && !Voxels[dx, dy, dz];
					if (!ground && !Voxels[dx, dy, dz])
					{
						continue;
					}
					if (dy == 15 && aboveBe != null)
					{
						if (newAboveCuboids == null)
						{
							newAboveCuboids = new CuboidWithMaterial[aboveBe.VoxelCuboids.Count];
							for (int j = 0; j < newAboveCuboids.Length; j++)
							{
								BlockEntityMicroBlock.FromUint(aboveBe.VoxelCuboids[j], newAboveCuboids[j] = new CuboidWithMaterial());
							}
						}
						for (int i = 0; i < newAboveCuboids.Length; i++)
						{
							newAboveCuboids[i].Contains(dx, dy, dz);
						}
					}
					CuboidWithMaterial cub = new CuboidWithMaterial
					{
						Material = 0,
						X1 = dx,
						Y1 = dy,
						Z1 = dz,
						X2 = dx,
						Y2 = dy + 1,
						Z2 = dz
					};
					bool didGrowAny = true;
					while (didGrowAny)
					{
						didGrowAny = false;
						didGrowAny |= TrySnowableSurfaceGrowX(cub, Voxels, snowVoxelVisited, ground);
						didGrowAny |= TrySnowableSurfaceGrowZ(cub, Voxels, snowVoxelVisited, ground);
					}
					if (cub.SizeX == 0 || cub.SizeZ == 0)
					{
						continue;
					}
					for (int z = cub.Z1; z < cub.Z2; z++)
					{
						for (int x = cub.X1; x < cub.X2; x++)
						{
							snowVoxelVisited[x, z] = true;
						}
					}
					if (ground)
					{
						groundSnowCuboids.Add(BlockEntityMicroBlock.ToUint(cub));
					}
					else
					{
						snowCuboids.Add(BlockEntityMicroBlock.ToUint(cub));
					}
					break;
				}
			}
		}
		aboveCuboids = newAboveCuboids;
		GroundSnowCuboids = groundSnowCuboids;
		SnowCuboids = snowCuboids;
	}

	private void GenSnowMesh()
	{
		if (beMicroBlock != null)
		{
			beMicroBlock.ConvertToVoxels(out var Voxels, out var _);
			buildSnowCuboids(Voxels);
		}
		if (SnowCuboids.Count > 0 && SnowLevel > 0)
		{
			SnowMesh = BlockEntityMicroBlock.CreateMesh(Api as ICoreClientAPI, SnowCuboids, new int[1] { snowLayerBlockId }, null, 0, beMicroBlock.OriginalVoxelCuboids, base.Pos);
			SnowMesh.Translate(0f, 0.0625f, 0f);
			SnowMesh.Scale(new Vec3f(0.5f, 0f, 0.5f), 0.999f, 1f, 0.999f);
			if (Api.World.BlockAccessor.IsSideSolid(base.Pos.X, base.Pos.Y - 1, base.Pos.Z, BlockFacing.UP))
			{
				SnowMesh.AddMeshData(BlockEntityMicroBlock.CreateMesh(Api as ICoreClientAPI, GroundSnowCuboids, new int[1] { snowLayerBlockId }, null, 0, beMicroBlock.OriginalVoxelCuboids, base.Pos));
			}
		}
		else
		{
			SnowMesh = null;
		}
	}

	protected bool TrySnowableSurfaceGrowX(CuboidWithMaterial cub, BoolArray16x16x16 voxels, bool[,] voxelVisited, bool ground)
	{
		if (cub.X2 > 15)
		{
			return false;
		}
		int z;
		for (z = cub.Z1; z < cub.Z2; z++)
		{
			z = Math.Min(15, z);
			if (aboveCuboids != null)
			{
				for (int i = 0; i < aboveCuboids.Length; i++)
				{
					if (aboveCuboids[i].Contains(cub.X2, 0, z))
					{
						return false;
					}
				}
			}
			if (voxels[cub.X2, cub.Y1, z] == ground || voxelVisited[cub.X2, z] || (cub.Y1 < 15 && voxels[cub.X2, cub.Y1 + 1, z]))
			{
				return false;
			}
		}
		cub.X2++;
		return true;
	}

	protected bool TrySnowableSurfaceGrowZ(CuboidWithMaterial cub, BoolArray16x16x16 voxels, bool[,] voxelVisited, bool ground)
	{
		if (cub.Z2 > 15)
		{
			return false;
		}
		int x;
		for (x = cub.X1; x < cub.X2; x++)
		{
			x = Math.Min(15, x);
			if (aboveCuboids != null)
			{
				for (int i = 0; i < aboveCuboids.Length; i++)
				{
					if (aboveCuboids[i].Contains(x, 0, cub.Z2))
					{
						return false;
					}
				}
			}
			if (voxels[x, cub.Y1, cub.Z2] == ground || voxelVisited[x, cub.Z2] || (cub.Y1 < 15 && voxels[x, cub.Y1 + 1, cub.Z2]))
			{
				return false;
			}
		}
		cub.Z2++;
		return true;
	}

	public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
	{
		SnowLevel = (int)base.Block.snowLevel;
		if (SnowLevel == 0)
		{
			if (Api.World.BlockAccessor.GetBlockEntity(base.Pos.UpCopy()) is BlockEntityMicroBlock abovebe && abovebe.Block.snowLevel > 0f && abovebe.VolumeRel < 0.0625f)
			{
				SnowLevel = (int)abovebe.Block.snowLevel;
			}
			if (SnowLevel == 0)
			{
				return false;
			}
		}
		if (PrevSnowLevel != SnowLevel || SnowMesh == null)
		{
			GenSnowMesh();
			PrevSnowLevel = SnowLevel;
		}
		mesher.AddMeshData(SnowMesh);
		return false;
	}

	public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
	{
		uint[] snowvalues = (tree["snowcuboids"] as IntArrayAttribute)?.AsUint;
		uint[] groundsnowvalues = (tree["groundSnowCuboids"] as IntArrayAttribute)?.AsUint;
		if (snowvalues != null && groundsnowvalues != null)
		{
			SnowCuboids = new List<uint>(snowvalues);
			GroundSnowCuboids = new List<uint>(groundsnowvalues);
		}
		else
		{
			SnowMesh = null;
		}
	}

	public override void ToTreeAttributes(ITreeAttribute tree)
	{
		if (SnowCuboids.Count > 0)
		{
			tree["snowcuboids"] = new IntArrayAttribute(SnowCuboids.ToArray());
		}
		if (GroundSnowCuboids.Count > 0)
		{
			tree["groundSnowCuboids"] = new IntArrayAttribute(GroundSnowCuboids.ToArray());
		}
	}

	public void RebuildCuboidList(BoolArray16x16x16 voxels, byte[,,] voxelMaterial)
	{
		buildSnowCuboids(voxels);
	}

	public void RegenMesh()
	{
		SnowLevel = (int)base.Block.snowLevel;
		if (SnowLevel == 0)
		{
			BlockEntityMicroBlock abovebe = Api.World.BlockAccessor.GetBlockEntity(base.Pos.Up()) as BlockEntityMicroBlock;
			base.Pos.Down();
			if (abovebe != null && abovebe.Block.snowLevel > 0f && abovebe.VolumeRel < 0.0625f)
			{
				SnowLevel = (int)abovebe.Block.snowLevel;
			}
			if (SnowLevel == 0)
			{
				return;
			}
		}
		GenSnowMesh();
	}
}
