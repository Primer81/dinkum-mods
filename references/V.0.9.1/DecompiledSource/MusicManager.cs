using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public enum indoorMusic
	{
		Default,
		Rayne,
		John,
		Franklyn,
		Melvin,
		Clover,
		Ned,
		Fletch,
		Irwin,
		Sally,
		Theodore,
		Milburn,
		Pub,
		Airport
	}

	public enum otherMusic
	{
		Pop,
		HipHop,
		Rock,
		Blues
	}

	public GameObject jamesLogo;

	public static MusicManager manage;

	public NetworkMapSharer share;

	private static float baseMusicVolume = 0.03f;

	public float musicMasterVolume = 0.03f;

	public AudioSource insideMusic;

	public AudioSource outsideMusic;

	public AudioSource menuMusic;

	public AudioSource dangerMusic;

	public MusicSource m_insideMusic;

	public MusicSource m_outsideMusic;

	public MusicSource m_menuMusic;

	public MusicSource m_dangerMusic;

	public AudioSource dangerMusicVictory;

	public AudioClip menuSong;

	public AudioClip[] dayTimeSongs;

	public AudioClip[] dayTimeSongsSummer;

	public AudioClip[] dayTimeSongsSpring;

	public AudioClip[] dayTimeSongsAutumn;

	public AudioClip[] dayTimeSongsWinter;

	public AudioClip[] nightTimeSongs;

	public AudioClip rainyDaySong;

	public AudioClip snowyDaySong;

	public AudioClip stormySong;

	public AudioClip windyDaySong;

	public AudioClip undergroundSong;

	public AudioClip undergroundForestSong;

	public AudioClip undergroundLavaSong;

	public AudioClip tropicalIslandSong;

	public AudioClip insideSong;

	public AudioClip shopSong;

	public AudioClip comabatSong;

	public AudioClip chrissyMenuMusic;

	public AudioClip compSong;

	public AudioClip[] shopMusic;

	public AudioClip[] boomBoxMusic;

	private bool inside;

	private bool inDanger;

	public LayerMask predators;

	public WaitForSeconds one = new WaitForSeconds(1f);

	public Coroutine musicCoroutineRunning;

	private int songForDay;

	private void Awake()
	{
		manage = this;
		m_insideMusic = new MusicSource(insideMusic);
		m_outsideMusic = new MusicSource(outsideMusic);
		m_menuMusic = new MusicSource(menuMusic);
		m_dangerMusic = new MusicSource(dangerMusic);
		share.onChangeMaps.AddListener(changeLevelMusic);
	}

	private IEnumerator Start()
	{
		changeVolume(musicMasterVolume);
		while (jamesLogo.activeInHierarchy)
		{
			yield return null;
		}
		if (RealWorldEventChecker.check.getCurrentEvent() == RealWorldEventChecker.TimedEvent.Chrissy)
		{
			m_menuMusic.changeMusicClip(chrissyMenuMusic);
			m_menuMusic.play();
			m_menuMusic.fastForward(0.5f);
		}
		else
		{
			m_menuMusic.changeMusicClip(menuSong);
			m_menuMusic.play();
		}
		m_dangerMusic.changeMusicClip(comabatSong);
	}

	public void changeLevelMusic()
	{
		m_outsideMusic.changeMusicClip(getOutsideMusic());
	}

	public void openCharacterCreator()
	{
		m_menuMusic.changeMusicClip(insideSong);
		m_menuMusic.play();
		NetworkPlayersManager.manage.IsPlayingSinglePlayer = true;
	}

	public void closeCharacterCreator()
	{
		m_menuMusic.changeMusicClip(menuSong);
		m_menuMusic.play();
	}

	public void changeVolume(float newVolume)
	{
		musicMasterVolume = newVolume;
		m_menuMusic.updateVolumeToMaster();
		m_outsideMusic.updateVolumeToMaster();
		m_insideMusic.updateVolumeToMaster();
	}

	public void changeFromMenu()
	{
		m_menuMusic.fadeOut();
		if (!TownManager.manage.firstConnect && musicCoroutineRunning == null)
		{
			musicCoroutineRunning = StartCoroutine(musicLoop());
			StartCoroutine(checkForDanger());
		}
	}

	public void startCutsceneMusic()
	{
		m_menuMusic.fadeOut();
	}

	public void ChangeCharacterInsideOrOutside(bool newInside, indoorMusic musicToPlay, bool noMusic = false)
	{
		inside = newInside;
		if (!m_insideMusic.isCurrentClipPlaying(shopMusic[(int)musicToPlay]))
		{
			m_insideMusic.changeMusicClip(shopMusic[(int)musicToPlay]);
		}
		if (newInside)
		{
			if (!noMusic)
			{
				m_insideMusic.play();
			}
			else
			{
				m_insideMusic.Stop();
			}
		}
	}

	public void enterBoomBoxZone()
	{
		if (inside)
		{
			m_insideMusic.fadeOut();
		}
		else
		{
			m_outsideMusic.fadeOut();
		}
	}

	public void exitBoomBoxZone()
	{
		if (inside)
		{
			m_insideMusic.fadeIn();
		}
		else
		{
			m_outsideMusic.fadeIn();
		}
	}

	private IEnumerator checkForDanger()
	{
		while (true)
		{
			if (NetworkNavMesh.nav.isPlayerInDangerNearCamera())
			{
				inDanger = true;
			}
			else
			{
				inDanger = false;
			}
			yield return one;
		}
	}

	public void stopMusic()
	{
		if (musicCoroutineRunning != null)
		{
			StopCoroutine(musicCoroutineRunning);
			musicCoroutineRunning = null;
		}
		m_outsideMusic.pause(newPaused: true);
		m_insideMusic.pause(newPaused: true);
		m_dangerMusic.pause(newPaused: true);
	}

	public void startMusic()
	{
		if (musicCoroutineRunning == null)
		{
			musicCoroutineRunning = StartCoroutine(musicLoop());
		}
	}

	private IEnumerator musicLoop()
	{
		yield return new WaitForSeconds(1f);
		m_insideMusic.changeMusicClip(insideSong);
		m_outsideMusic.changeMusicClip(getOutsideMusic());
		m_outsideMusic.play();
		m_insideMusic.play();
		m_insideMusic.pause(newPaused: true);
		m_dangerMusic.play();
		m_dangerMusic.pause(newPaused: true);
		if (inside)
		{
			m_outsideMusic.pause(newPaused: true);
			m_insideMusic.fadeIn();
		}
		else
		{
			m_insideMusic.pause(newPaused: true);
			m_outsideMusic.fadeIn();
		}
		bool lastInside = inside;
		bool lastInDanger = inDanger;
		float lastTimeSincePlayed = 0f;
		int num = Random.Range(240, 360);
		int nextPlayTime = num;
		while (true)
		{
			yield return null;
			if (lastInDanger != inDanger)
			{
				lastInDanger = inDanger;
				if (inDanger)
				{
					if (!inside)
					{
						m_outsideMusic.fadeOut(1f);
					}
					else
					{
						m_insideMusic.fadeOut(1f);
					}
					m_dangerMusic.fadeIn(1f);
				}
				else
				{
					while (dangerMusicVictory.isPlaying)
					{
						yield return null;
					}
					if (!inside)
					{
						m_outsideMusic.fadeIn(2f);
					}
					else
					{
						m_insideMusic.fadeIn(2f);
					}
					m_dangerMusic.fadeOut(1f);
				}
				while (inDanger)
				{
					yield return null;
				}
			}
			if (lastInside != inside)
			{
				lastInside = inside;
				if (lastInside)
				{
					m_outsideMusic.fadeOut();
					m_insideMusic.fadeIn();
				}
				else
				{
					m_outsideMusic.fadeIn();
					m_insideMusic.fadeOut();
				}
			}
			if (!inside && !m_outsideMusic.isPlaying())
			{
				if (lastTimeSincePlayed > (float)nextPlayTime)
				{
					m_outsideMusic.changeMusicClip(getOutsideMusic());
					m_outsideMusic.setLocalVolume(0f);
					m_outsideMusic.play();
					m_outsideMusic.fadeIn(15f);
					nextPlayTime = Random.Range(240, 360);
					lastTimeSincePlayed = 0f;
				}
				else
				{
					lastTimeSincePlayed += Time.deltaTime;
				}
			}
			if (RealWorldTimeLight.time.currentHour == RealWorldTimeLight.time.getSunSetTime() && RealWorldTimeLight.time.currentMinute > 55 && !inside && m_outsideMusic.isPlaying())
			{
				m_outsideMusic.fadeOut(10f);
			}
			if (RealWorldTimeLight.time.currentHour == RealWorldTimeLight.time.getSunSetTime() + 1 && RealWorldTimeLight.time.currentMinute == 0 && !inside && m_outsideMusic.isPaused)
			{
				m_outsideMusic.Stop();
				m_outsideMusic.isPaused = false;
				lastTimeSincePlayed = 500f;
			}
			if (!inside && !m_outsideMusic.isPaused && m_outsideMusic.isPlaying())
			{
				m_outsideMusic.checkForLastSecondsFade();
			}
		}
	}

	public void playNewSong(float fadeInTime = 15f)
	{
		m_outsideMusic.changeMusicClip(getOutsideMusic());
		m_outsideMusic.setLocalVolume(0f);
		m_outsideMusic.play();
		m_outsideMusic.fadeIn(fadeInTime);
	}

	public void playCompMusic()
	{
		outsideMusic.loop = true;
		playNewSong(1f);
	}

	public void stopCompMusic()
	{
		outsideMusic.loop = false;
		playNewSong();
	}

	public IEnumerator fadeOut(MusicSource source, float fadeTime = 3f)
	{
		while (source.getLocalVolume() > 0f)
		{
			yield return null;
			source.adjustLocalVolume((0f - Time.deltaTime) / fadeTime);
		}
		source.setLocalVolume(0f);
		source.pause(newPaused: true);
		source.runningCoroutine = null;
	}

	public IEnumerator fadeIn(MusicSource source, float fadeTime = 3f)
	{
		source.pause(newPaused: false);
		while (source.getLocalVolume() < 1f)
		{
			yield return null;
			source.adjustLocalVolume(Time.deltaTime / fadeTime);
		}
		source.setLocalVolume(1f);
		source.runningCoroutine = null;
	}

	public void switchUnderWater(bool newUnderWater)
	{
		if (newUnderWater)
		{
			outsideMusic.pitch = 0.75f;
			insideMusic.pitch = 0.75f;
		}
		else
		{
			outsideMusic.pitch = 1f;
			insideMusic.pitch = 1f;
		}
	}

	public AudioClip getOutsideMusic()
	{
		bool flag = WorldManager.Instance.day != songForDay;
		songForDay = WorldManager.Instance.day;
		if (CatchingCompetitionManager.manage.inCompetition && CatchingCompetitionManager.manage.competitionActive())
		{
			return compSong;
		}
		if (RealWorldTimeLight.time.underGround)
		{
			if (RealWorldTimeLight.time.mineLevel <= 0)
			{
				return undergroundSong;
			}
			if (RealWorldTimeLight.time.mineLevel == 1)
			{
				return undergroundForestSong;
			}
			return undergroundLavaSong;
		}
		if (RealWorldTimeLight.time.offIsland)
		{
			return tropicalIslandSong;
		}
		if (RealWorldTimeLight.time.currentHour >= RealWorldTimeLight.time.getSunSetTime() || RealWorldTimeLight.time.currentHour == 0)
		{
			if (TownEventManager.manage.townEventOn != 0)
			{
				return nightTimeSongs[0];
			}
			return nightTimeSongs[Random.Range(0, nightTimeSongs.Length)];
		}
		if (WeatherManager.Instance.rainMgr.IsActive && (flag || Random.Range(0, 5) <= 3))
		{
			if (WeatherManager.Instance.SnowFallPossible())
			{
				return snowyDaySong;
			}
			if (WeatherManager.Instance.stormMgr.IsActive)
			{
				return stormySong;
			}
			return rainyDaySong;
		}
		if (WeatherManager.Instance.windMgr.IsActive && (flag || Random.Range(0, dayTimeSongs.Length + 1) <= 2))
		{
			return windyDaySong;
		}
		if (WorldManager.Instance.month == 1 && (flag || Random.Range(0, dayTimeSongs.Length + dayTimeSongsSummer.Length) <= 0))
		{
			return dayTimeSongsSummer[Random.Range(0, dayTimeSongsSummer.Length)];
		}
		if (WorldManager.Instance.month == 2 && (flag || Random.Range(0, dayTimeSongs.Length + dayTimeSongsAutumn.Length) <= 0))
		{
			return dayTimeSongsAutumn[Random.Range(0, dayTimeSongsAutumn.Length)];
		}
		if (WorldManager.Instance.month == 3 && (flag || Random.Range(0, dayTimeSongs.Length + dayTimeSongsWinter.Length) <= 0))
		{
			return dayTimeSongsWinter[Random.Range(0, dayTimeSongsWinter.Length)];
		}
		if (WorldManager.Instance.month == 4 && (flag || Random.Range(0, dayTimeSongs.Length + dayTimeSongsSpring.Length) <= 0))
		{
			return dayTimeSongsSpring[Random.Range(0, dayTimeSongsSpring.Length)];
		}
		return dayTimeSongs[Random.Range(0, dayTimeSongs.Length)];
	}

	public void PlayCombatMusicStinger()
	{
		if (dangerMusic.isPlaying)
		{
			dangerMusicVictory.volume = dangerMusic.volume;
			dangerMusicVictory.Play();
			m_dangerMusic.fadeOut(0.1f);
			inDanger = false;
		}
	}
}
