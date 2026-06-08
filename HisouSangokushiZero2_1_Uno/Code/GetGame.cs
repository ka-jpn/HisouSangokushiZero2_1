using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class GetGame {
  internal static GameState GetInitGameScenario(ScenarioId? scenario) {
    return GetScenario(scenario).MyPipe(maybeScenarioInfo => InitGame(scenario, maybeScenarioInfo)).MyPipe(InitState).MyPipe(AppendStartMessage).MyPipe(InitMaxAreaNum);
      static ScenarioData? GetScenario(ScenarioId? newScenario) => newScenario?.MyPipe(ScenarioBase.GetScenarioData);
      static GameState InitGame(ScenarioId? scenario,ScenarioData? scenarioInfo) => new(scenario,scenarioInfo?.AreaMap.ToDictionary() ?? [],scenarioInfo?.CountryMap.ToDictionary() ?? [],scenarioInfo?.PersonMap.ToDictionary() ?? [],null,null,0,Phase.Starting,[],false,[],[],[],[],[],[]);
      static GameState InitState(GameState game) => game.MyPipe(UpdateGame.UpdateCapitalArea).MyPipe(UpdateGame.InitAlivePersonPost).MyPipe(game => UpdateGame.AutoPutPostCPU(game,[])).MyPipe(ClearAllLog);
      static GameState ClearAllLog(GameState game) => game with { GameLog = [],LogMessage = [],TurnNewLog = [],StartPlanningCharacterRemark = [],StartExecutionCharacterRemark = [] };
      static GameState AppendStartMessage(GameState game) => UpdateGame.AppendLogMessage(game,[$"{BaseData.name.Value} ver.{BaseData.version.Value}"]);
      static GameState InitMaxAreaNum(GameState game) => game with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => v.Value with { MaxAreaNum = Country.GetAreaNum(game,v.Key) }) };
  }
}