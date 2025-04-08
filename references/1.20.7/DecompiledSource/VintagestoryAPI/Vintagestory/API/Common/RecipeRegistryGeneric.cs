using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Datastructures;

namespace Vintagestory.API.Common;

public class RecipeRegistryGeneric<T> : RecipeRegistryBase where T : IByteSerializable, new()
{
	public List<T> Recipes;

	public RecipeRegistryGeneric()
	{
		Recipes = new List<T>();
	}

	public RecipeRegistryGeneric(List<T> recipes)
	{
		Recipes = recipes;
	}

	public override void FromBytes(IWorldAccessor resolver, int quantity, byte[] data)
	{
		using MemoryStream ms = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(ms);
		for (int i = 0; i < quantity; i++)
		{
			T rec = new T();
			rec.FromBytes(reader, resolver);
			Recipes.Add(rec);
		}
	}

	public override void ToBytes(IWorldAccessor resolver, out byte[] data, out int quantity)
	{
		using FastMemoryStream ms = new FastMemoryStream();
		ToBytes(resolver, out data, out quantity, ms);
	}

	public void ToBytes(IWorldAccessor resolver, out byte[] data, out int quantity, FastMemoryStream ms)
	{
		quantity = Recipes.Count;
		ms.Reset();
		BinaryWriter writer = new BinaryWriter(ms);
		foreach (T recipe in Recipes)
		{
			recipe.ToBytes(writer);
		}
		data = ms.ToArray();
	}

	public virtual void FreeRAMServer()
	{
		foreach (T recipe in Recipes)
		{
			if (recipe is GridRecipe gr)
			{
				gr.FreeRAMServer();
			}
		}
	}
}
