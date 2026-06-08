using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Turn {
	internal static int GetYear(GameState game) => (game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.StartYear ?? 0) + (game.PlayTurn ?? 0) / Enum.GetValues<YearItems>().Length;
	internal static int GetInYear(GameState game) => (game.PlayTurn ?? 0) % Enum.GetValues<YearItems>().Length;
	internal static YearItems? GetCalendarInYear(GameState game) => Enum.GetValues<YearItems>().MyNullable().ElementAtOrDefault(GetInYear(game));
}