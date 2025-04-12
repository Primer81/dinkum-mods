using UnityEngine;

public class TileTypes : MonoBehaviour
{
	public enum tiles
	{
		Dirt,
		Grass,
		RockyDirt,
		Sand,
		TropicalGrass,
		RedSand,
		BrickPath,
		TilledDirt,
		WetTilledDirt,
		MineStoneTile,
		MineDirtTile,
		CementPath,
		TilledDirtFertilizer,
		WetTilledDirtFertilizer,
		Mud,
		PineGrass,
		WoodPath,
		BasicRockPath
	}

	public Material myTileMaterial;

	public bool sideOfTileSame;

	public Color tileColorOnMap;

	public InventoryItem dropOnChange;

	public bool saveUnderTile;

	public bool changeTileKeepUnderTile;

	public bool changeToUnderTileAndChangeHeight;

	public ASound onPickUp;

	public ASound onPutDown;

	public ASound onHeightUp;

	public ASound onHeightDown;

	public int changeToOnHeightChange = -1;

	public int onChangeParticle = -1;

	public int onHeightChangePart = -1;

	public int changeParticleAmount = 25;

	public int specialDustPart = -1;

	public int footStepParticle = -1;

	public ASound footStepSound;

	public bool isPath;

	public bool isDirt;

	public bool isStone;

	public bool isTilledDirt;

	public bool isWetTilledDirt;

	public bool isFertilizedDirt;

	public bool isWetFertilizedDirt;

	public bool isGrassGrowable;

	public bool canBeSavedUnder = true;

	public int wetVersion = -1;

	public int dryVersion = -1;

	public InventoryItem uniqueShovel;

	public int mowedVariation = -1;

	public bool isMowedGrass;

	public bool collectsSnow;
}
