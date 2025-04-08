using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Vintagestory.ServerMods.WorldEdit;

public class PaintBrushTool : ToolBase
{
	public static string[][] dimensionNames;

	protected BlockPos[] brushPositions;

	protected internal Vec3i size;

	public virtual string Prefix => "std.brush";

	public float BrushDim1
	{
		get
		{
			return workspace.FloatValues[Prefix + "Dim1"];
		}
		set
		{
			workspace.FloatValues[Prefix + "Dim1"] = value;
		}
	}

	public float BrushDim2
	{
		get
		{
			return workspace.FloatValues[Prefix + "Dim2"];
		}
		set
		{
			workspace.FloatValues[Prefix + "Dim2"] = value;
		}
	}

	public float BrushDim3
	{
		get
		{
			return workspace.FloatValues[Prefix + "Dim3"];
		}
		set
		{
			workspace.FloatValues[Prefix + "Dim3"] = value;
		}
	}

	public EnumBrushShape BrushShape
	{
		get
		{
			return (EnumBrushShape)workspace.IntValues[Prefix + "Shape"];
		}
		set
		{
			workspace.IntValues[Prefix + "Shape"] = (int)value;
		}
	}

	public bool PreviewMode
	{
		get
		{
			return workspace.IntValues[Prefix + "previewMode"] > 0;
		}
		set
		{
			workspace.IntValues[Prefix + "previewMode"] = (value ? 1 : 0);
		}
	}

	public float CutoutDim1
	{
		get
		{
			return workspace.FloatValues[Prefix + "cutoutDim1"];
		}
		set
		{
			workspace.FloatValues[Prefix + "cutoutDim1"] = value;
		}
	}

	public float CutoutDim2
	{
		get
		{
			return workspace.FloatValues[Prefix + "cutoutDim2"];
		}
		set
		{
			workspace.FloatValues[Prefix + "cutoutDim2"] = value;
		}
	}

	public float CutoutDim3
	{
		get
		{
			return workspace.FloatValues[Prefix + "cutoutDim3"];
		}
		set
		{
			workspace.FloatValues[Prefix + "cutoutDim3"] = value;
		}
	}

	public EnumBrushMode BrushMode
	{
		get
		{
			return (EnumBrushMode)workspace.IntValues[Prefix + "Mode"];
		}
		set
		{
			workspace.IntValues[Prefix + "Mode"] = (int)value;
		}
	}

	public EnumDepthLimit DepthLimit
	{
		get
		{
			return (EnumDepthLimit)workspace.IntValues[Prefix + "DepthLimit"];
		}
		set
		{
			workspace.IntValues[Prefix + "DepthLimit"] = (int)value;
		}
	}

	public float PlacementPercentage
	{
		get
		{
			return workspace.FloatValues[Prefix + "placementPercentage"];
		}
		set
		{
			workspace.FloatValues[Prefix + "placementPercentage"] = value;
		}
	}

	public override Vec3i Size => size;

	public PaintBrushTool()
	{
	}

	public PaintBrushTool(WorldEditWorkspace workspace, IBlockAccessorRevertable blockAccessor)
		: base(workspace, blockAccessor)
	{
		if (!workspace.FloatValues.ContainsKey(Prefix + "Dim1"))
		{
			BrushDim1 = 4f;
		}
		if (!workspace.FloatValues.ContainsKey(Prefix + "Dim2"))
		{
			BrushDim2 = 4f;
		}
		if (!workspace.FloatValues.ContainsKey(Prefix + "Dim3"))
		{
			BrushDim3 = 4f;
		}
		if (!workspace.FloatValues.ContainsKey(Prefix + "cutoutDim1"))
		{
			CutoutDim1 = 0f;
		}
		if (!workspace.FloatValues.ContainsKey(Prefix + "cutoutDim2"))
		{
			CutoutDim2 = 0f;
		}
		if (!workspace.FloatValues.ContainsKey(Prefix + "cutoutDim3"))
		{
			CutoutDim3 = 0f;
		}
		if (!workspace.FloatValues.ContainsKey(Prefix + "placementPercentage"))
		{
			PlacementPercentage = 100f;
		}
		if (!workspace.IntValues.ContainsKey(Prefix + "previewMode"))
		{
			PreviewMode = true;
		}
		if (!workspace.IntValues.ContainsKey(Prefix + "Mode"))
		{
			BrushMode = EnumBrushMode.Fill;
		}
		if (!workspace.IntValues.ContainsKey(Prefix + "Shape"))
		{
			BrushShape = EnumBrushShape.Ball;
		}
		if (!workspace.IntValues.ContainsKey(Prefix + "DepthLimit"))
		{
			DepthLimit = EnumDepthLimit.NoLimit;
		}
		dimensionNames = new string[9][]
		{
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" },
			new string[3] { "Width", "Height", "Length" },
			new string[3] { "X-Radius", "Height", "Z-Radius" },
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" },
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" },
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" },
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" },
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" },
			new string[3] { "X-Radius", "Y-Radius", "Z-Radius" }
		};
		GenBrush();
	}

	public override bool OnWorldEditCommand(WorldEdit worldEdit, TextCommandCallingArgs callerArgs)
	{
		IServerPlayer player = (IServerPlayer)callerArgs.Caller.Player;
		CmdArgs args = callerArgs.RawArgs;
		switch (args[0])
		{
		case "pp":
		{
			PlacementPercentage = 0f;
			float percentage = 0f;
			if (args.Length > 1 && float.TryParse(args[1], out percentage))
			{
				PlacementPercentage = percentage;
			}
			WorldEdit.Good(player, workspace.ToolName + " placement percentage " + (int)percentage + "% set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tm":
		{
			EnumBrushMode brushMode = EnumBrushMode.Fill;
			if (args.Length > 1)
			{
				int.TryParse(args[1], out var index);
				if (Enum.IsDefined(typeof(EnumBrushMode), index))
				{
					brushMode = (EnumBrushMode)index;
				}
			}
			BrushMode = brushMode;
			WorldEdit.Good(player, workspace.ToolName + " mode " + brushMode.ToString() + " set.");
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tdl":
		{
			EnumDepthLimit depthLimit = EnumDepthLimit.NoLimit;
			if (args.Length > 1)
			{
				int.TryParse(args[1], out var index2);
				if (Enum.IsDefined(typeof(EnumDepthLimit), index2))
				{
					depthLimit = (EnumDepthLimit)index2;
				}
			}
			DepthLimit = depthLimit;
			WorldEdit.Good(player, workspace.ToolName + " depth limit set to " + depthLimit);
			workspace.ResendBlockHighlights();
			return true;
		}
		case "ts":
		{
			EnumBrushShape shape = EnumBrushShape.Ball;
			if (args.Length > 1)
			{
				int.TryParse(args[1], out var index3);
				if (Enum.IsDefined(typeof(EnumBrushShape), index3))
				{
					shape = (EnumBrushShape)index3;
				}
			}
			BrushShape = shape;
			WorldEdit.Good(player, workspace.ToolName + " shape " + BrushShape.ToString() + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tsx":
		case "tsy":
		case "tsz":
		{
			float size2 = 0f;
			if (args.Length > 1)
			{
				float.TryParse(args[1], out size2);
			}
			if (args[0] == "tsx")
			{
				BrushDim1 = size2;
			}
			if (args[0] == "tsy")
			{
				BrushDim2 = size2;
			}
			if (args[0] == "tsz")
			{
				BrushDim3 = size2;
			}
			string text5 = dimensionNames[(int)BrushShape][0] + "=" + BrushDim1;
			text5 = text5 + ", " + dimensionNames[(int)BrushShape][1] + "=" + BrushDim2;
			text5 = text5 + ", " + dimensionNames[(int)BrushShape][2] + "=" + BrushDim3;
			WorldEdit.Good(player, workspace.ToolName + " dimensions " + text5 + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tr":
		{
			BrushDim1 = 0f;
			if (args.Length > 1 && float.TryParse(args[1], out var size3))
			{
				BrushDim1 = size3;
			}
			if (args.Length > 2 && float.TryParse(args[2], out size3))
			{
				BrushDim2 = size3;
			}
			else
			{
				BrushDim2 = BrushDim1;
			}
			if (args.Length > 3 && float.TryParse(args[3], out size3))
			{
				BrushDim3 = size3;
			}
			else
			{
				BrushDim3 = BrushDim2;
			}
			string text6 = dimensionNames[(int)BrushShape][0] + "=" + BrushDim1;
			text6 = text6 + ", " + dimensionNames[(int)BrushShape][1] + "=" + BrushDim2;
			text6 = text6 + ", " + dimensionNames[(int)BrushShape][2] + "=" + BrushDim3;
			WorldEdit.Good(player, workspace.ToolName + " dimensions " + text6 + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tcx":
		case "tcy":
		case "tcz":
		{
			float size4 = 0f;
			if (args.Length > 1)
			{
				float.TryParse(args[1], out size4);
			}
			if (args[0] == "tcx")
			{
				CutoutDim1 = size4;
			}
			if (args[0] == "tcy")
			{
				CutoutDim2 = size4;
			}
			if (args[0] == "tcz")
			{
				CutoutDim3 = size4;
			}
			string text4 = dimensionNames[(int)BrushShape][0] + "=" + CutoutDim1;
			text4 = text4 + ", " + dimensionNames[(int)BrushShape][1] + "=" + CutoutDim2;
			text4 = text4 + ", " + dimensionNames[(int)BrushShape][2] + "=" + CutoutDim3;
			WorldEdit.Good(player, workspace.ToolName + " cutout dimensions " + text4 + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tcr":
		{
			CutoutDim1 = 0f;
			if (args.Length > 1 && float.TryParse(args[1], out var size))
			{
				CutoutDim1 = size;
			}
			if (args.Length > 2 && float.TryParse(args[2], out size))
			{
				CutoutDim2 = size;
			}
			else
			{
				CutoutDim2 = CutoutDim1;
			}
			if (args.Length > 3 && float.TryParse(args[3], out size))
			{
				CutoutDim3 = size;
			}
			else
			{
				CutoutDim3 = CutoutDim2;
			}
			string text3 = dimensionNames[(int)BrushShape][0] + "=" + CutoutDim1;
			text3 = text3 + ", " + dimensionNames[(int)BrushShape][1] + "=" + CutoutDim2;
			text3 = text3 + ", " + dimensionNames[(int)BrushShape][2] + "=" + CutoutDim3;
			WorldEdit.Good(player, "Cutout " + workspace.ToolName + " dimensions " + text3 + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tgr":
		{
			BrushDim1++;
			BrushDim2++;
			BrushDim3++;
			string text2 = dimensionNames[(int)BrushShape][0] + "=" + BrushDim1;
			text2 = text2 + ", " + dimensionNames[(int)BrushShape][1] + "=" + BrushDim2;
			text2 = text2 + ", " + dimensionNames[(int)BrushShape][2] + "=" + BrushDim3;
			WorldEdit.Good(player, workspace.ToolName + " dimensions " + text2 + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		case "tsr":
		{
			BrushDim1 = Math.Max(0f, BrushDim1 - 1f);
			BrushDim2 = Math.Max(0f, BrushDim2 - 1f);
			BrushDim3 = Math.Max(0f, BrushDim3 - 1f);
			string text = dimensionNames[(int)BrushShape][0] + "=" + BrushDim1;
			text = text + ", " + dimensionNames[(int)BrushShape][1] + "=" + BrushDim2;
			text = text + ", " + dimensionNames[(int)BrushShape][2] + "=" + BrushDim3;
			WorldEdit.Good(player, workspace.ToolName + " dimensions " + text + " set.");
			GenBrush();
			workspace.ResendBlockHighlights();
			return true;
		}
		default:
			return false;
		}
	}

	public override void ApplyToolBuild(WorldEdit worldEdit, Block placedBlock, int oldBlockId, BlockSelection blockSel, BlockPos targetPos, ItemStack withItemStack)
	{
		ToolBase.PlaceOldBlock(worldEdit, oldBlockId, blockSel, placedBlock);
		PerformBrushAction(worldEdit, placedBlock, oldBlockId, blockSel, targetPos, withItemStack);
		ba.Commit();
	}

	public virtual void PerformBrushAction(WorldEdit worldEdit, Block placedBlock, int oldBlockId, BlockSelection blockSel, BlockPos targetPos, ItemStack withItemStack)
	{
		if (BrushDim1 <= 0f)
		{
			return;
		}
		Block selectedBlock = (blockSel.DidOffset ? ba.GetBlockOnSide(blockSel.Position, blockSel.Face.Opposite) : ba.GetBlock(blockSel.Position));
		int selectedBlockId = selectedBlock.Id;
		EnumBrushMode brushMode = BrushMode;
		int blockId = withItemStack?.Block.Id ?? 0;
		if (!workspace.MayPlace(placedBlock, brushPositions.Length))
		{
			return;
		}
		EnumDepthLimit depthLimit = DepthLimit;
		float pp = PlacementPercentage / 100f;
		Random rnd = worldEdit.sapi.World.Rand;
		for (int i = 0; i < brushPositions.Length; i++)
		{
			if (rnd.NextDouble() > (double)pp)
			{
				continue;
			}
			BlockPos dpos = targetPos.AddCopy(brushPositions[i].X, brushPositions[i].Y, brushPositions[i].Z);
			bool skip = false;
			switch (depthLimit)
			{
			case EnumDepthLimit.Top1:
				skip = isAir(ba, dpos) || !isAir(ba, dpos, 1);
				break;
			case EnumDepthLimit.Top2:
				skip = isAir(ba, dpos) || (!isAir(ba, dpos, 1) && !isAir(ba, dpos, 2));
				break;
			case EnumDepthLimit.Top3:
				skip = isAir(ba, dpos) || (!isAir(ba, dpos, 1) && !isAir(ba, dpos, 2) && !isAir(ba, dpos, 3));
				break;
			case EnumDepthLimit.Top4:
				skip = isAir(ba, dpos) || (!isAir(ba, dpos, 1) && !isAir(ba, dpos, 2) && !isAir(ba, dpos, 3) && !isAir(ba, dpos, 4));
				break;
			}
			if (!skip && brushMode switch
			{
				EnumBrushMode.ReplaceAir => ba.GetBlock(dpos, 0).Id == 0, 
				EnumBrushMode.ReplaceNonAir => ba.GetBlock(dpos, 0).Id != 0, 
				EnumBrushMode.ReplaceSelected => ba.GetBlock(dpos, (!selectedBlock.ForFluidsLayer) ? 1 : 2).Id == selectedBlockId, 
				_ => true, 
			})
			{
				if (placedBlock.ForFluidsLayer)
				{
					ba.SetBlock(blockId, dpos, 2);
					ba.SetBlock(0, dpos);
				}
				else
				{
					ba.SetBlock(0, dpos, 2);
					ba.SetBlock(blockId, dpos, withItemStack);
				}
			}
		}
	}

	public bool isAir(IBlockAccessor blockAccessor, BlockPos pos, int dy = 0)
	{
		return blockAccessor.GetBlockAbove(pos, dy).Id == 0;
	}

	public override EnumHighlightShape GetBlockHighlightShape()
	{
		if (brushPositions.Length > 300000)
		{
			return EnumHighlightShape.Cube;
		}
		if (BrushShape == EnumBrushShape.Cuboid)
		{
			return EnumHighlightShape.Cube;
		}
		if (BrushShape == EnumBrushShape.Cylinder)
		{
			return EnumHighlightShape.Cylinder;
		}
		return base.GetBlockHighlightShape();
	}

	public override List<BlockPos> GetBlockHighlights()
	{
		if (brushPositions.Length > 300000)
		{
			return new List<BlockPos>
			{
				new BlockPos(-size.X / 2, -size.Y / 2, -size.Z / 2),
				new BlockPos(size.X / 2, size.Y / 2, size.Z / 2)
			};
		}
		return new List<BlockPos>(brushPositions);
	}

	internal void GenBrush()
	{
		List<BlockPos> positions = new List<BlockPos>();
		float dim1 = BrushDim1;
		float dim2 = BrushDim2;
		float dim3 = BrushDim3;
		if (dim2 == 0f)
		{
			dim2 = dim1;
		}
		if (dim3 == 0f)
		{
			dim3 = dim2;
		}
		int xRadInt = (int)Math.Ceiling(dim1);
		int yRadInt = (int)Math.Ceiling(dim2);
		int zRadInt = (int)Math.Ceiling(dim3);
		int dim1Int = (int)dim1;
		int dim2Int = (int)dim2;
		int dim3Int = (int)dim3;
		float xRadSqInv = 1f / (dim1 * dim1);
		float yRadSqInv = 1f / (dim2 * dim2);
		float zRadSqInv = 1f / (dim3 * dim3);
		size = new Vec3i((int)Math.Ceiling(dim1), (int)Math.Ceiling(dim2), (int)Math.Ceiling(dim3));
		int cutoutDim1Int = (int)CutoutDim1;
		int cutoutDim2Int = (int)CutoutDim2;
		int cutoutDim3Int = (int)CutoutDim3;
		float xCutRadSqInv = 1f / (CutoutDim1 * CutoutDim1);
		float yCutRadSqInv = 1f / (CutoutDim2 * CutoutDim2);
		float zCutRadSqInv = 1f / (CutoutDim3 * CutoutDim3);
		switch (BrushShape)
		{
		case EnumBrushShape.Ball:
		{
			for (int dx3 = -xRadInt; dx3 <= xRadInt; dx3++)
			{
				for (int dy3 = -yRadInt; dy3 <= yRadInt; dy3++)
				{
					for (int dz3 = -zRadInt; dz3 <= zRadInt; dz3++)
					{
						if (!((float)(dx3 * dx3) * xRadSqInv + (float)(dy3 * dy3) * yRadSqInv + (float)(dz3 * dz3) * zRadSqInv > 1f) && !((float)(dx3 * dx3) * xCutRadSqInv + (float)(dy3 * dy3) * yCutRadSqInv + (float)(dz3 * dz3) * zCutRadSqInv < 1f))
						{
							positions.Add(new BlockPos(dx3, dy3, dz3));
						}
					}
				}
			}
			size = new Vec3i((int)Math.Ceiling(2f * dim1), (int)Math.Ceiling(2f * dim2), (int)Math.Ceiling(2f * dim3));
			break;
		}
		case EnumBrushShape.Cuboid:
		{
			int notminx = -cutoutDim1Int;
			int notmaxx = cutoutDim1Int;
			int notminy = -cutoutDim2Int - 1;
			int notmaxy = cutoutDim2Int;
			int notminz = -cutoutDim3Int;
			int notmaxz = cutoutDim3Int;
			for (int dx2 = 0; dx2 < dim1Int; dx2++)
			{
				for (int dy2 = 0; dy2 < dim2Int; dy2++)
				{
					for (int dz2 = 0; dz2 < dim3Int; dz2++)
					{
						if (dx2 < notminx || dx2 >= notmaxx || dy2 < notminy || dy2 >= notmaxy || dz2 < notminz || dz2 >= notmaxz)
						{
							int x = dx2 - dim1Int / 2;
							int y = dy2 - dim2Int / 2;
							int z = dz2 - dim3Int / 2;
							positions.Add(new BlockPos(x, y, z));
						}
					}
				}
			}
			break;
		}
		case EnumBrushShape.Cylinder:
		{
			for (int dx = -xRadInt; dx <= xRadInt; dx++)
			{
				for (int dz = -zRadInt; dz <= zRadInt; dz++)
				{
					if ((float)(dx * dx) * xRadSqInv + (float)(dz * dz) * zRadSqInv > 1f || (float)(dx * dx) * xCutRadSqInv + (float)(dz * dz) * zCutRadSqInv < 1f)
					{
						continue;
					}
					for (int dy = 0; dy < dim2Int; dy++)
					{
						int y = (int)Math.Ceiling((float)dy - (float)dim2Int / 2f);
						if (Math.Abs(y) >= cutoutDim2Int)
						{
							positions.Add(new BlockPos(dx, y, dz));
						}
					}
				}
			}
			size = new Vec3i((int)Math.Ceiling(2f * dim1), (int)Math.Ceiling(dim2), (int)Math.Ceiling(2f * dim3));
			break;
		}
		case EnumBrushShape.HalfBallUp:
			positions = HalfBall(-xRadInt, 0, -zRadInt, xRadInt, yRadInt, zRadInt, 0, -yRadInt / 2, 0, xRadSqInv, yRadSqInv, zRadSqInv, xCutRadSqInv, yCutRadSqInv, zCutRadSqInv);
			size = new Vec3i((int)Math.Ceiling(2f * dim1), (int)Math.Ceiling(1f * dim2), (int)Math.Ceiling(2f * dim3));
			break;
		case EnumBrushShape.HalfBallDown:
			positions = HalfBall(-xRadInt, -yRadInt, -zRadInt, xRadInt, 0, zRadInt, 0, yRadInt / 2, 0, xRadSqInv, yRadSqInv, zRadSqInv, xCutRadSqInv, yCutRadSqInv, zCutRadSqInv);
			size = new Vec3i((int)Math.Ceiling(2f * dim1), (int)Math.Ceiling(1f * dim2), (int)Math.Ceiling(2f * dim3));
			break;
		case EnumBrushShape.HalfBallWest:
			positions = HalfBall(-xRadInt, -yRadInt, -zRadInt, 0, yRadInt, zRadInt, xRadInt / 2, 0, 0, xRadSqInv, yRadSqInv, zRadSqInv, xCutRadSqInv, yCutRadSqInv, zCutRadSqInv);
			size = new Vec3i((int)Math.Ceiling(1f * dim1), (int)Math.Ceiling(2f * dim2), (int)Math.Ceiling(2f * dim3));
			break;
		case EnumBrushShape.HalfBallSouth:
			positions = HalfBall(-xRadInt, -yRadInt, 0, xRadInt, yRadInt, zRadInt, 0, 0, -zRadInt / 2, xRadSqInv, yRadSqInv, zRadSqInv, xCutRadSqInv, yCutRadSqInv, zCutRadSqInv);
			size = new Vec3i((int)Math.Ceiling(2f * dim1), (int)Math.Ceiling(2f * dim2), (int)Math.Ceiling(1f * dim3));
			break;
		case EnumBrushShape.HalfBallNorth:
			positions = HalfBall(-xRadInt, -yRadInt, -zRadInt, xRadInt, yRadInt, 0, 0, 0, zRadInt / 2, xRadSqInv, yRadSqInv, zRadSqInv, xCutRadSqInv, yCutRadSqInv, zCutRadSqInv);
			size = new Vec3i((int)Math.Ceiling(2f * dim1), (int)Math.Ceiling(2f * dim2), (int)Math.Ceiling(1f * dim3));
			break;
		case EnumBrushShape.HalfBallEast:
			positions = HalfBall(0, -yRadInt, -zRadInt, xRadInt, yRadInt, zRadInt, -xRadInt / 2, 0, 0, xRadSqInv, yRadSqInv, zRadSqInv, xCutRadSqInv, yCutRadSqInv, zCutRadSqInv);
			size = new Vec3i((int)Math.Ceiling(1f * dim1), (int)Math.Ceiling(2f * dim2), (int)Math.Ceiling(1f * dim3));
			break;
		}
		brushPositions = positions.ToArray();
	}

	private List<BlockPos> HalfBall(int minx, int miny, int minz, int maxx, int maxy, int maxz, int offX, int offY, int offZ, float xRadSqInv, float yRadSqInv, float zRadSqInv, float xCutRadSqInv, float yCutRadSqInv, float zCutRadSqInv)
	{
		List<BlockPos> positions = new List<BlockPos>();
		size = new Vec3i(maxx - minx, maxy - miny, maxz - minz);
		for (int dx = minx; dx <= maxx; dx++)
		{
			for (int dy = miny; dy <= maxy; dy++)
			{
				for (int dz = minz; dz <= maxz; dz++)
				{
					if (!((float)(dx * dx) * xRadSqInv + (float)(dy * dy) * yRadSqInv + (float)(dz * dz) * zRadSqInv > 1f) && !((float)(dx * dx) * xCutRadSqInv + (float)(dy * dy) * yCutRadSqInv + (float)(dz * dz) * zCutRadSqInv < 1f))
					{
						positions.Add(new BlockPos(dx + offX, dy + offY, dz + offZ));
					}
				}
			}
		}
		return positions;
	}
}
