using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent;

public class WeatherSystemServer : WeatherSystemBase
{
	public ICoreServerAPI sapi;

	public IServerNetworkChannel serverChannel;

	internal WeatherSimulationSnowAccum snowSimSnowAccu;

	protected WeatherPatternAssetsPacket packetForClient;

	private float? overrideprecip;

	private double daysoffset;

	public override float? OverridePrecipitation
	{
		get
		{
			return overrideprecip;
		}
		set
		{
			overrideprecip = value;
			sapi.WorldManager.SaveGame.StoreData("overrideprecipitation", (!overrideprecip.HasValue) ? null : SerializerUtil.Serialize(overrideprecip.Value));
		}
	}

	public override double RainCloudDaysOffset
	{
		get
		{
			return daysoffset;
		}
		set
		{
			daysoffset = value;
			sapi.WorldManager.SaveGame.StoreData("precipitationdaysoffset", SerializerUtil.Serialize(daysoffset));
		}
	}

	public override bool ShouldLoad(EnumAppSide side)
	{
		return side == EnumAppSide.Server;
	}

	public override void StartServerSide(ICoreServerAPI api)
	{
		sapi = api;
		LoadConfigs();
		serverChannel = api.Network.GetChannel("weather");
		sapi.Event.RegisterGameTickListener(OnServerGameTick, 200);
		sapi.Event.GameWorldSave += OnSaveGameSaving;
		sapi.Event.SaveGameLoaded += Event_SaveGameLoaded;
		sapi.Event.OnGetClimate += base.Event_OnGetClimate;
		sapi.Event.PlayerJoin += Event_PlayerJoin;
		snowSimSnowAccu = new WeatherSimulationSnowAccum(sapi, this);
	}

	public void ReloadConfigs()
	{
		api.Assets.Reload(new AssetLocation("config/"));
		LoadConfigs(isReload: true);
		IPlayer[] allOnlinePlayers = sapi.World.AllOnlinePlayers;
		foreach (IPlayer val in allOnlinePlayers)
		{
			serverChannel.SendPacket(packetForClient, val as IServerPlayer);
		}
	}

	public void LoadConfigs(bool isReload = false)
	{
		WeatherSystemConfig nowconfig = api.Assets.Get<WeatherSystemConfig>(new AssetLocation("config/weather.json"));
		if (isReload)
		{
			nowconfig.Init(api.World);
		}
		GeneralConfig = nowconfig;
		WeatherPatternConfig[][] array = (from val in api.Assets.GetMany<WeatherPatternConfig[]>(api.World.Logger, "config/weatherpatterns/")
			orderby val.Key.ToString()
			select val.Value).ToArray();
		WeatherConfigs = new WeatherPatternConfig[0];
		WeatherPatternConfig[][] array2 = array;
		foreach (WeatherPatternConfig[] val4 in array2)
		{
			WeatherConfigs = WeatherConfigs.Append(val4);
		}
		WindPatternConfig[][] array3 = (from val in api.Assets.GetMany<WindPatternConfig[]>(api.World.Logger, "config/windpatterns/")
			orderby val.Key.ToString()
			select val.Value).ToArray();
		WindConfigs = new WindPatternConfig[0];
		WindPatternConfig[][] array4 = array3;
		foreach (WindPatternConfig[] val3 in array4)
		{
			WindConfigs = WindConfigs.Append(val3);
		}
		WeatherEventConfig[][] array5 = (from val in api.Assets.GetMany<WeatherEventConfig[]>(api.World.Logger, "config/weatherevents/")
			orderby val.Key.ToString()
			select val.Value).ToArray();
		WeatherEventConfigs = new WeatherEventConfig[0];
		WeatherEventConfig[][] array6 = array5;
		foreach (WeatherEventConfig[] val2 in array6)
		{
			WeatherEventConfigs = WeatherEventConfigs.Append(val2);
		}
		api.World.Logger.Notification("Reloaded {0} weather patterns, {1} wind patterns and {2} weather events", WeatherConfigs.Length, WindConfigs.Length, WeatherEventConfigs.Length);
		WeatherPatternAssets p = new WeatherPatternAssets
		{
			GeneralConfig = GeneralConfig,
			WeatherConfigs = WeatherConfigs,
			WindConfigs = WindConfigs,
			WeatherEventConfigs = WeatherEventConfigs
		};
		packetForClient = new WeatherPatternAssetsPacket
		{
			Data = JsonUtil.ToString(p)
		};
	}

	private void Event_PlayerJoin(IServerPlayer byPlayer)
	{
		serverChannel.SendPacket(packetForClient, byPlayer);
		serverChannel.SendPacket(new WeatherCloudYposPacket
		{
			CloudYRel = CloudLevelRel
		}, byPlayer);
		sendConfigUpdate(byPlayer);
	}

	public void sendConfigUpdate(IServerPlayer byPlayer)
	{
		serverChannel.SendPacket(new WeatherConfigPacket
		{
			OverridePrecipitation = OverridePrecipitation,
			RainCloudDaysOffset = RainCloudDaysOffset
		}, byPlayer);
	}

	public void broadCastConfigUpdate()
	{
		serverChannel.BroadcastPacket(new WeatherConfigPacket
		{
			OverridePrecipitation = OverridePrecipitation,
			RainCloudDaysOffset = RainCloudDaysOffset
		});
	}

	private void Event_SaveGameLoaded()
	{
		byte[] data = sapi.WorldManager.SaveGame.GetData("overrideprecipitation");
		if (data != null)
		{
			overrideprecip = SerializerUtil.Deserialize<float>(data);
		}
		data = sapi.WorldManager.SaveGame.GetData("precipitationdaysoffset");
		if (data != null)
		{
			daysoffset = SerializerUtil.Deserialize<double>(data);
		}
		Initialize();
		InitDummySim();
		WeatherDataSlowAccess = getWeatherDataReader();
		GeneralConfig.Init(api.World);
		if (sapi.WorldManager.SaveGame.WorldConfiguration != null)
		{
			CloudLevelRel = sapi.WorldManager.SaveGame.WorldConfiguration.GetString("cloudypos", "1").ToFloat(1f);
		}
	}

	public void SendWeatherStateUpdate(WeatherState state)
	{
		int regionSize = sapi.WorldManager.RegionSize;
		byte[] data = SerializerUtil.Serialize(state);
		List<IServerPlayer> playersToSend = new List<IServerPlayer>(sapi.World.AllOnlinePlayers.Length);
		IPlayer[] allOnlinePlayers = sapi.World.AllOnlinePlayers;
		foreach (IPlayer plr in allOnlinePlayers)
		{
			int plrRegionX = (int)plr.Entity.ServerPos.X / regionSize;
			int plrRegionZ = (int)plr.Entity.ServerPos.Z / regionSize;
			if (Math.Abs(state.RegionX - plrRegionX) <= 1 && Math.Abs(state.RegionZ - plrRegionZ) <= 1)
			{
				playersToSend.Add(plr as IServerPlayer);
			}
		}
		if (playersToSend.Count > 0)
		{
			serverChannel.SendPacket(state, data, playersToSend.ToArray());
		}
		sapi.WorldManager.GetMapRegion(state.RegionX, state.RegionZ)?.SetModdata("weatherState", data);
	}

	private void OnServerGameTick(float dt)
	{
		sapi.World.FrameProfiler.Enter("weathersimulation");
		foreach (KeyValuePair<long, IMapRegion> val in sapi.WorldManager.AllLoadedMapRegions)
		{
			WeatherSimulationRegion orCreateWeatherSimForRegion = getOrCreateWeatherSimForRegion(val.Key, val.Value);
			orCreateWeatherSimForRegion.TickEvery25ms(dt);
			sapi.World.FrameProfiler.Mark("finishedtick");
			orCreateWeatherSimForRegion.UpdateWeatherData();
			sapi.World.FrameProfiler.Mark("updatedata");
		}
		rainOverlaySnap.SetAmbient(rainOverlayPattern);
		sapi.World.FrameProfiler.Leave();
	}

	private void OnSaveGameSaving()
	{
		HashSet<long> toRemove = new HashSet<long>();
		foreach (KeyValuePair<long, WeatherSimulationRegion> val in weatherSimByMapRegion)
		{
			IMapRegion mapregion = sapi.WorldManager.GetMapRegion(val.Key);
			if (mapregion != null)
			{
				mapregion.SetModdata("weatherState", val.Value.ToBytes());
			}
			else
			{
				toRemove.Add(val.Key);
			}
		}
		foreach (long key in toRemove)
		{
			weatherSimByMapRegion.Remove(key);
		}
	}

	public override void SpawnLightningFlash(Vec3d pos)
	{
		TriggerOnLightningImpactStart(ref pos, out var handling);
		if (handling == EnumHandling.PassThrough)
		{
			LightningFlashPacket pkt = new LightningFlashPacket
			{
				Pos = pos,
				Seed = api.World.Rand.Next()
			};
			serverChannel.BroadcastPacket(pkt);
			LightningFlash lflash = new LightningFlash(this, api, pkt.Seed, pkt.Pos);
			simLightning.lightningFlashes.Add(lflash);
		}
	}
}
