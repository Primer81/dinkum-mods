using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Vintagestory.API.Config;

/// <summary>
/// The games current version
/// </summary>
public static class GameVersion
{
	/// <summary>
	/// Assembly Info Version number in the format: major.minor.revision
	/// </summary>
	public const string OverallVersion = "1.20.7";

	/// <summary>
	/// Whether this is a stable or unstable version
	/// </summary>
	public const EnumGameBranch Branch = EnumGameBranch.Stable;

	/// <summary>
	/// Version number in the format: major.minor.revision[appendix]
	/// </summary>
	public const string ShortGameVersion = "1.20.7";

	/// <summary>
	/// Version number in the format: major.minor.revision [release title]
	/// </summary>
	public static string LongGameVersion = "v1.20.7 (" + EnumGameBranch.Stable.ToString() + ")";

	/// <summary>
	/// Assembly Info Version number in the format: major.minor.revision
	/// </summary>
	public const string AssemblyVersion = "1.0.0.0";

	/// <summary>
	/// Version of the Mod API
	/// </summary>
	public const string APIVersion = "1.20.0";

	/// <summary>
	/// Version of the Network Protocol
	/// </summary>
	public const string NetworkVersion = "1.20.8";

	/// <summary>
	/// Version of the world generator - a change in version will insert a smoothed chunk between old and new version
	/// </summary>
	public const int WorldGenVersion = 2;

	/// <summary>
	/// Version of the savegame database
	/// </summary>
	public static int DatabaseVersion = 2;

	/// <summary>
	/// Version of the chunkdata compression for individual WorldChunks (0 is Deflate; 1 is ZSTD and palettised)  Also affects compression of network packets sent
	/// </summary>
	public const int ChunkdataVersion = 2;

	/// <summary>
	/// "Version" of the block and item mapping. This number gets increased by 1 when remappings are needed
	/// </summary>
	public static int BlockItemMappingVersion = 1;

	/// <summary>
	/// Copyright notice
	/// </summary>
	public const string CopyRight = "Copyright © 2016-2024 Anego Studios";

	private static string[] separators = new string[2] { ".", "-" };

	public static EnumReleaseType ReleaseType => GetReleaseType("1.20.7");

	public static int[] SplitVersionString(string version)
	{
		int hyphenIndex = version.IndexOf('-');
		string majorMinorVersion = ((hyphenIndex < 1) ? version : version.Substring(0, hyphenIndex));
		if (majorMinorVersion.CountChars('.') == 1)
		{
			majorMinorVersion += ".0";
			version = ((hyphenIndex < 1) ? majorMinorVersion : (majorMinorVersion + version.Substring(hyphenIndex)));
		}
		string[] parts = version.Split(separators, StringSplitOptions.None);
		if (parts.Length <= 3)
		{
			parts = parts.Append("3");
		}
		else if (parts[3] == "rc")
		{
			parts[3] = "2";
		}
		else if (parts[3] == "pre")
		{
			parts[3] = "1";
		}
		else
		{
			parts[3] = "0";
		}
		int[] versions = new int[parts.Length];
		for (int i = 0; i < parts.Length; i++)
		{
			int.TryParse(parts[i], out var ver);
			versions[i] = ver;
		}
		return versions;
	}

	public static EnumReleaseType GetReleaseType(string version)
	{
		return SplitVersionString(version)[3] switch
		{
			0 => EnumReleaseType.Development, 
			1 => EnumReleaseType.Preview, 
			2 => EnumReleaseType.Candidate, 
			3 => EnumReleaseType.Stable, 
			_ => throw new ArgumentException("Unknown release type"), 
		};
	}

	/// <summary>
	/// Returns true if given version has the same major and minor version. Ignores revision.
	/// </summary>
	/// <param name="version"></param>
	/// <returns></returns>
	public static bool IsCompatibleApiVersion(string version)
	{
		int[] partsTheirs = SplitVersionString(version);
		int[] partsMine = SplitVersionString("1.20.0");
		if (partsTheirs.Length < 2)
		{
			return false;
		}
		if (partsMine[0] == partsTheirs[0])
		{
			return partsMine[1] == partsTheirs[1];
		}
		return false;
	}

	/// <summary>
	/// Returns true if given version has the same major and minor version. Ignores revision.
	/// </summary>
	/// <param name="version"></param>
	/// <returns></returns>
	public static bool IsCompatibleNetworkVersion(string version)
	{
		int[] partsTheirs = SplitVersionString(version);
		int[] partsMine = SplitVersionString("1.20.8");
		if (partsTheirs.Length < 2)
		{
			return false;
		}
		if (partsMine[0] == partsTheirs[0])
		{
			return partsMine[1] == partsTheirs[1];
		}
		return false;
	}

	/// <summary>
	/// Returns true if supplied version is the same or higher as the current version
	/// </summary>
	/// <param name="version"></param>
	/// <returns></returns>
	public static bool IsAtLeastVersion(string version)
	{
		return IsAtLeastVersion(version, "1.20.7");
	}

	/// <summary>
	/// Returns true if supplied version is the same or higher as the reference version
	/// </summary>
	/// <param name="version"></param>
	/// <param name="reference"></param>
	/// <returns></returns>
	public static bool IsAtLeastVersion(string version, string reference)
	{
		int[] partsMin = SplitVersionString(reference);
		int[] partsCur = SplitVersionString(version);
		for (int i = 0; i < partsMin.Length; i++)
		{
			if (i >= partsCur.Length)
			{
				return false;
			}
			if (partsMin[i] > partsCur[i])
			{
				return false;
			}
			if (partsMin[i] < partsCur[i])
			{
				return true;
			}
		}
		return true;
	}

	public static bool IsLowerVersionThan(string version, string reference)
	{
		if (version != reference)
		{
			return !IsNewerVersionThan(version, reference);
		}
		return false;
	}

	/// <summary>
	/// Returns true if supplied version is the higher as the reference version
	/// </summary>
	/// <param name="version"></param>
	/// <param name="reference"></param>
	/// <returns></returns>
	public static bool IsNewerVersionThan(string version, string reference)
	{
		int[] partsMin = SplitVersionString(reference);
		int[] partsCur = SplitVersionString(version);
		for (int i = 0; i < partsMin.Length; i++)
		{
			if (i >= partsCur.Length)
			{
				return false;
			}
			if (partsMin[i] > partsCur[i])
			{
				return false;
			}
			if (partsMin[i] < partsCur[i])
			{
				return true;
			}
		}
		return false;
	}

	public static void EnsureEqualVersionOrKillExecutable(ICoreAPI api, string version, string reference, string modName)
	{
		if (version != reference)
		{
			if (api.Side == EnumAppSide.Server)
			{
				Exception ex = new Exception(Lang.Get("versionmismatch-server", modName + ".dll"));
				((ICoreServerAPI)api).Server.ShutDown();
				throw ex;
			}
			Exception e = new Exception(Lang.Get("versionmismatch-client", modName + ".dll"));
			((ICoreClientAPI)api).Event.EnqueueMainThreadTask(delegate
			{
				throw e;
			}, "killgame");
		}
	}
}
