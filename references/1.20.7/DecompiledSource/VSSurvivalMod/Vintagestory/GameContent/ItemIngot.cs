using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Vintagestory.GameContent;

public class ItemIngot : Item, IAnvilWorkable
{
	private bool isBlisterSteel;

	public override void OnLoaded(ICoreAPI api)
	{
		base.OnLoaded(api);
		isBlisterSteel = Variant["metal"] == "blistersteel";
	}

	public string GetMetalType()
	{
		return LastCodePart();
	}

	public int GetRequiredAnvilTier(ItemStack stack)
	{
		string metalcode = Variant["metal"];
		int tier = 0;
		if (api.ModLoader.GetModSystem<SurvivalCoreSystem>().metalsByCode.TryGetValue(metalcode, out var var))
		{
			tier = var.Tier - 1;
		}
		JsonObject attributes = stack.Collectible.Attributes;
		if (attributes != null && attributes["requiresAnvilTier"].Exists)
		{
			tier = stack.Collectible.Attributes["requiresAnvilTier"].AsInt(tier);
		}
		return tier;
	}

	public List<SmithingRecipe> GetMatchingRecipes(ItemStack stack)
	{
		return (from r in api.GetSmithingRecipes()
			where r.Ingredient.SatisfiesAsIngredient(stack)
			orderby r.Output.ResolvedItemstack.Collectible.Code
			select r).ToList();
	}

	public bool CanWork(ItemStack stack)
	{
		float temperature = stack.Collectible.GetTemperature(api.World, stack);
		float meltingpoint = stack.Collectible.GetMeltingPoint(api.World, null, new DummySlot(stack));
		JsonObject attributes = stack.Collectible.Attributes;
		if (attributes != null && attributes["workableTemperature"].Exists)
		{
			return stack.Collectible.Attributes["workableTemperature"].AsFloat(meltingpoint / 2f) <= temperature;
		}
		return temperature >= meltingpoint / 2f;
	}

	public ItemStack TryPlaceOn(ItemStack stack, BlockEntityAnvil beAnvil)
	{
		if (!CanWork(stack))
		{
			return null;
		}
		Item item = api.World.GetItem(new AssetLocation("workitem-" + Variant["metal"]));
		if (item == null)
		{
			return null;
		}
		ItemStack workItemStack = new ItemStack(item);
		workItemStack.Collectible.SetTemperature(api.World, workItemStack, stack.Collectible.GetTemperature(api.World, stack));
		if (beAnvil.WorkItemStack == null)
		{
			CreateVoxelsFromIngot(api, ref beAnvil.Voxels, isBlisterSteel);
		}
		else
		{
			if (isBlisterSteel)
			{
				return null;
			}
			if (!string.Equals(beAnvil.WorkItemStack.Collectible.Variant["metal"], stack.Collectible.Variant["metal"]))
			{
				if (api.Side == EnumAppSide.Client)
				{
					(api as ICoreClientAPI).TriggerIngameError(this, "notequal", Lang.Get("Must be the same metal to add voxels"));
				}
				return null;
			}
			if (AddVoxelsFromIngot(ref beAnvil.Voxels) == 0)
			{
				if (api.Side == EnumAppSide.Client)
				{
					(api as ICoreClientAPI).TriggerIngameError(this, "requireshammering", Lang.Get("Try hammering down before adding additional voxels"));
				}
				return null;
			}
		}
		return workItemStack;
	}

	public static void CreateVoxelsFromIngot(ICoreAPI api, ref byte[,,] voxels, bool isBlisterSteel = false)
	{
		voxels = new byte[16, 6, 16];
		for (int x = 0; x < 7; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 3; z++)
				{
					voxels[4 + x, y, 6 + z] = 1;
					if (isBlisterSteel)
					{
						if (api.World.Rand.NextDouble() < 0.5)
						{
							voxels[4 + x, y + 1, 6 + z] = 1;
						}
						if (api.World.Rand.NextDouble() < 0.5)
						{
							voxels[4 + x, y + 1, 6 + z] = 2;
						}
					}
				}
			}
		}
	}

	public static int AddVoxelsFromIngot(ref byte[,,] voxels)
	{
		int totalAdded = 0;
		for (int x = 0; x < 7; x++)
		{
			for (int z = 0; z < 3; z++)
			{
				int y = 0;
				int added = 0;
				for (; y < 6; y++)
				{
					if (added >= 2)
					{
						break;
					}
					if (voxels[4 + x, y, 6 + z] == 0)
					{
						voxels[4 + x, y, 6 + z] = 1;
						added++;
						totalAdded++;
					}
				}
			}
		}
		return totalAdded;
	}

	public ItemStack GetBaseMaterial(ItemStack stack)
	{
		return stack;
	}

	public EnumHelveWorkableMode GetHelveWorkableMode(ItemStack stack, BlockEntityAnvil beAnvil)
	{
		return EnumHelveWorkableMode.NotWorkable;
	}
}
