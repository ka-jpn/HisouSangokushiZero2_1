using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Turn {
	internal static int GetYear(GameState game) => GetYear(game.NowScenario,game.PlayTurn ?? 0);
	internal static int GetInYear(GameState game) => GetInYear(game.PlayTurn ??0 );
	internal static YearItems? GetCalendarInYear(GameState game) => GetCalendarInYear(game.PlayTurn ?? 0);
	internal static int GetYear(ScenarioId? scenario,int turn) => (scenario?.MyPipe(ScenarioBase.GetScenarioData)?.StartYear ?? 0) + turn / Enum.GetValues<YearItems>().Length;
	internal static int GetInYear(int turn) => turn % Enum.GetValues<YearItems>().Length;
	internal static YearItems? GetCalendarInYear(int turn) => Enum.GetValues<YearItems>().MyNullable().ElementAtOrDefault(GetInYear(turn));
}