using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;

namespace Vintagestory.API.Common;

/// <summary>
/// Represents an in game Item of Vintage Story
/// </summary>
public class Item : CollectibleObject
{
	/// <summary>
	/// The unique number of the item, dynamically assigned by the game
	/// </summary>
	public int ItemId;

	/// <summary>
	/// The item's shape. Null for automatic shape based on the texture.
	/// </summary>
	public CompositeShape Shape;

	/// <summary>
	/// Default textures to be used for this item. The Dictionary keys are the texture short names, as referenced in this item's shape ShapeElementFaces
	/// <br />(may be null on clients, prior to receipt of server assets)
	/// <br />Note: from game version 1.20.4, this is <b>null on server-side</b> (except during asset loading start-up stage)
	/// </summary>
	public Dictionary<string, CompositeTexture> Textures = new Dictionary<string, CompositeTexture>();

	/// <summary>
	/// The unique number of the item, dynamically assigned by the game
	/// </summary>
	public override int Id => ItemId;

	/// <summary>
	/// The type of the collectible object
	/// </summary>
	public override EnumItemClass ItemClass => EnumItemClass.Item;

	/// <summary>
	/// Returns the first texture in Textures
	/// </summary>
	public CompositeTexture FirstTexture
	{
		get
		{
			if (Textures != null && Textures.Count != 0)
			{
				return Textures.First().Value;
			}
			return null;
		}
	}

	/// <summary>
	/// Instantiate a new item with null model transforms; ItemTypeNet will add default transforms client-side if they are null in the ItemType packet; transforms should not be needed on a server
	/// </summary>
	public Item()
	{
	}

	/// <summary>
	/// Instantiates a new item with given item id and stacksize = 1
	/// </summary>
	/// <param name="itemId"></param>
	public Item(int itemId)
	{
		ItemId = itemId;
		MaxStackSize = 1;
	}

	/// <summary>
	/// Should return a random pixel within the items/blocks texture
	/// </summary>
	/// <param name="capi"></param>
	/// <param name="stack"></param>
	/// <returns></returns>
	public override int GetRandomColor(ICoreClientAPI capi, ItemStack stack)
	{
		if (Textures == null || Textures.Count == 0)
		{
			return 0;
		}
		BakedCompositeTexture tex = Textures?.First().Value?.Baked;
		if (tex != null)
		{
			return capi.ItemTextureAtlas.GetRandomColor(tex.TextureSubId);
		}
		return 0;
	}

	/// <summary>
	/// Creates a deep copy of the item
	/// </summary>
	/// <returns></returns>
	public Item Clone()
	{
		Item cloned = (Item)MemberwiseClone();
		cloned.Code = Code.Clone();
		if (MiningSpeed != null)
		{
			cloned.MiningSpeed = new Dictionary<EnumBlockMaterial, float>(MiningSpeed);
		}
		cloned.Textures = new Dictionary<string, CompositeTexture>();
		if (Textures != null)
		{
			foreach (KeyValuePair<string, CompositeTexture> val in Textures)
			{
				cloned.Textures[val.Key] = val.Value.Clone();
			}
		}
		if (Shape != null)
		{
			cloned.Shape = Shape.Clone();
		}
		if (Attributes != null)
		{
			cloned.Attributes = Attributes.Clone();
		}
		if (CombustibleProps != null)
		{
			cloned.CombustibleProps = CombustibleProps.Clone();
		}
		if (NutritionProps != null)
		{
			cloned.NutritionProps = NutritionProps.Clone();
		}
		if (GrindingProps != null)
		{
			cloned.GrindingProps = GrindingProps.Clone();
		}
		return cloned;
	}

	internal void CheckTextures(ILogger logger)
	{
		List<string> toRemove = null;
		int i = 0;
		foreach (KeyValuePair<string, CompositeTexture> val2 in Textures)
		{
			if (val2.Value.Base == null)
			{
				logger.Error("The texture definition {0} for #{2} in item with code {1} is invalid. The base property is null. Will skip.", i, Code, val2.Key);
				if (toRemove == null)
				{
					toRemove = new List<string>();
				}
				toRemove.Add(val2.Key);
			}
			i++;
		}
		if (toRemove == null)
		{
			return;
		}
		foreach (string val in toRemove)
		{
			Textures.Remove(val);
		}
	}

	public virtual void FreeRAMServer()
	{
		Textures = null;
	}
}
