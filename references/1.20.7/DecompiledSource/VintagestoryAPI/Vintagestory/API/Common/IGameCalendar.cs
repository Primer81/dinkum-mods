using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common;

/// <summary>
/// Main API for retrieving anything calender or astronomy related
/// </summary>
public interface IGameCalendar
{
	/// <summary>
	/// Assigned by the survival mod. Must return the hemisphere at give location
	/// </summary>
	HemisphereDelegate OnGetHemisphere { get; set; }

	/// <summary>
	/// Assigned by the survival mod. The calendar uses this method to determine the solar altitude at given location and time. If not set, the calendar uses a default value of about 0.75
	/// </summary>
	SolarSphericalCoordsDelegate OnGetSolarSphericalCoords { get; set; }

	/// <summary>
	/// Assigned by the survival mod. Must return the latitude for given position. If not set, the calendar uses a default value of 0.5<br />
	/// -1 for south pole, 0 for equater, 1 for north pole
	/// </summary>
	GetLatitudeDelegate OnGetLatitude { get; set; }

	/// <summary>
	/// This acts as a multiplier on how much faster an ingame second passes by compared to a real life second. Affects physics, like the motion speed of waving grass. The default is 60, hence per default a day lasts 24 minutes, but it's also multiplied by CalendarSpeedMul which is 0.5 by default so the end result is 48 minutes per day
	/// This is the sum of all modifiers
	/// </summary>
	float SpeedOfTime { get; }

	/// <summary>
	/// Amount of in-game seconds that have passed since the game started
	/// </summary>
	long ElapsedSeconds { get; }

	/// <summary>
	/// Amount of in-game hours that have passed since the game started
	/// </summary>
	double ElapsedHours { get; }

	/// <summary>
	/// Amount of in-game days that have passed since the game started
	/// </summary>
	double ElapsedDays { get; }

	/// <summary>
	/// A multiplier thats applied to the progression of the calendar. Set this to 0.1 and a day will last 10 times longer, does not affect physics.
	/// </summary>
	float CalendarSpeedMul { get; set; }

	/// <summary>
	/// Amount of hours per day
	/// </summary>
	float HoursPerDay { get; }

	/// <summary>
	/// Amount of days per year
	/// </summary>
	int DaysPerYear { get; }

	/// <summary>
	/// Amount of days per month
	/// </summary>
	int DaysPerMonth { get; }

	int Month { get; }

	EnumMonth MonthName { get; }

	/// <summary>
	/// The current hour of the day as integer
	/// </summary>
	int FullHourOfDay { get; }

	/// <summary>
	/// The current hour of the day as decimal 
	/// </summary>
	float HourOfDay { get; }

	/// <summary>
	/// Total passed hours since the game has started
	/// </summary>
	double TotalHours { get; }

	/// <summary>
	/// Total passed days since the game has started
	/// </summary>
	double TotalDays { get; }

	/// <summary>
	/// The current day of the year (goes from 0 to DaysPerYear)
	/// </summary>
	int DayOfYear { get; }

	/// <summary>
	/// The current day of the year (goes from 0 to DaysPerYear)
	/// </summary>
	float DayOfYearf { get; }

	/// <summary>
	/// Returns the year. Every game begins with 1386
	/// </summary>
	int Year { get; }

	/// <summary>
	/// Returns the current season in a value of 0 to 1
	/// </summary>
	float YearRel { get; }

	/// <summary>
	/// The current moonphase
	/// </summary>
	EnumMoonPhase MoonPhase { get; }

	/// <summary>
	/// The current moonphase represented by number from 0..8
	/// </summary>
	double MoonPhaseExact { get; }

	/// <summary>
	/// The moons current brightness (higher during full moon)
	/// </summary>
	float MoonPhaseBrightness { get; }

	/// <summary>
	/// The moons current size (larger during full moon)
	/// </summary>
	float MoonSize { get; }

	float? SeasonOverride { get; set; }

	/// <summary>
	/// Can be used to adjust apparent time of day and season for rendering, e.g. to create a series of timelapse images; restore to 0 when done
	/// </summary>
	float Timelapse { get; set; }

	/// <summary>
	/// Retrieve the current daylight strength at given coordinates, the values are roughly beween 0 and 1.2f
	/// </summary>
	/// <param name="x"></param>
	/// <param name="z"></param>
	/// <returns></returns>
	float GetDayLightStrength(double x, double z);

	/// <summary>
	/// Retrieve the current daylight strength at given coordinates. The Y-Component is ignored
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	float GetDayLightStrength(BlockPos pos);

	/// <summary>
	/// Get the suns position in the sky at given date as a normalized vector
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="totalDays"></param>
	/// <returns></returns>
	Vec3f GetSunPosition(Vec3d pos, double totalDays);

	/// <summary>
	/// Get the moons position in the sky at given date as a normalized vector
	/// </summary>
	/// <param name="position"></param>
	/// <param name="totaldays"></param>
	/// <returns></returns>
	Vec3f GetMoonPosition(Vec3d position, double totaldays);

	/// <summary>
	/// The worlds current date, nicely formatted
	/// </summary>
	/// <returns></returns>
	string PrettyDate();

	/// <summary>
	/// If you want to modify the time speed, set a value here
	/// </summary>
	void SetTimeSpeedModifier(string name, float speed);

	/// <summary>
	/// To remove a previously added time speed modifier
	/// </summary>
	/// <param name="name"></param>
	void RemoveTimeSpeedModifier(string name);

	/// <summary>
	/// Returns the season at given position
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	EnumSeason GetSeason(BlockPos pos);

	/// <summary>
	/// Returns the season at given position between 0..1, respects hemisphere
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	float GetSeasonRel(BlockPos pos);

	/// <summary>
	/// Returns the hemisphere at given position
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	EnumHemisphere GetHemisphere(BlockPos pos);

	/// <summary>
	/// Adds given time to the calendar
	/// </summary>
	/// <param name="hours"></param>
	void Add(float hours);

	/// <summary>
	/// If non-null, will override the value retrieved by GetSeason(). Set to null to have seasons progress normally again.
	/// </summary>
	/// <param name="seasonRel"></param>
	void SetSeasonOverride(float? seasonRel);
}
