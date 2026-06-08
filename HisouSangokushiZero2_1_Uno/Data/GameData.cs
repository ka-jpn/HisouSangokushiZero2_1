using HisouSangokushiZero2_1_Uno.Code;
using System;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Data;
internal static class GameData {
  internal static DateTime startGameDateTime = new();
  internal static TimeSpan startingPlayTotalTime = new();
  internal static GameState game = GetGame.GetInitGameScenario(null);
  internal static Lang lang = Lang.Ja;
  internal static ILangText langText = new Language.Ja();
}