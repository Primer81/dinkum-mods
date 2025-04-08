using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Client.Tesselation;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

public class BlockFenceStackAware : BlockFence
{
	private ICoreClientAPI capi;

	private Dictionary<string, MeshData> continousFenceMeches;

	private string cntCode;

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		capi = api as ICoreClientAPI;
		if (capi != null)
		{
			continousFenceMeches = ObjectCacheUtil.GetOrCreate(capi, Code.Domain + ":" + FirstCodePart() + "-continousFenceMeches", () => new Dictionary<string, MeshData>());
			cntCode = Code.ToShortString();
		}
	}

	public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
	{
		if (chunkExtBlocks[extIndex3d + TileSideEnum.MoveIndex[4]] is BlockFence)
		{
			int var = GameMath.MurmurHash3Mod(pos.X, pos.Y, pos.Z, 8) + 1;
			if (!continousFenceMeches.TryGetValue(cntCode + var, out var mesh))
			{
				AssetLocation loc = Shape.Base.Clone();
				loc.Path = loc.Path.Replace("-top", "");
				loc.WithPathAppendixOnce(".json");
				loc.WithPathPrefixOnce("shapes/");
				Shape shape = Vintagestory.API.Common.Shape.TryGet(capi, loc);
				CompositeTexture ct = Textures["wall"];
				int prevSubid = ct.Baked.TextureSubId;
				ct.Baked.TextureSubId = ct.Baked.BakedVariants[GameMath.MurmurHash3Mod(pos.X, pos.Y, pos.Z, ct.Alternates.Length)].TextureSubId;
				capi.Tesselator.TesselateShape(this, shape, out mesh, new Vec3f(Shape.rotateX, Shape.rotateY, Shape.rotateZ), Shape.QuantityElements, Shape.SelectiveElements);
				ct.Baked.TextureSubId = prevSubid;
				continousFenceMeches[cntCode] = mesh;
			}
			sourceMesh = mesh;
		}
	}
}
