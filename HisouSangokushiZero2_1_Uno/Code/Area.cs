using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Area {
  private static Road[]? GetRoads(GameState game) => game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.RoadConnections;
  private static Point ConvertPointFromAreaPosition(Point areaPosition,Size mapSize,Size areaSize,Point mapGridCount,double infoFrameWidth) => new(areaPosition.X * (mapSize.Width - areaSize.Width - infoFrameWidth) / (mapGridCount.X - 1) + infoFrameWidth + areaSize.Width / 2,areaPosition.Y * (mapSize.Height - areaSize.Height - infoFrameWidth) / (mapGridCount.Y - 1) + infoFrameWidth + areaSize.Height / 2);
  internal static Point? GetAreaPoint(GameState game,EArea areaName,Size mapSize,Size areaSize,Point mapGridCount,double infoFrameWidth) => game.AreaMap.GetValueOrDefault(areaName)?.Position.MyPipe(areaPos => ConvertPointFromAreaPosition(areaPos,mapSize,areaSize,mapGridCount,infoFrameWidth));
  internal static List<EArea> GetCountryAreas(GameState game,ECountry country) => [.. game.AreaMap.Where(v => v.Value.Country == country).Select(v => v.Key)];
  internal static List<EArea> GetConnectCapitalCountryAreas(GameState game,ECountry country) => [.. country == ECountry.漢 ? GetCountryAreas(game,country) : GetAreaDistancesFromCapital(game,country).SelectMany(v => v)];
  internal static List<EArea> GetCellEachAdjacentAnotherCountryAreas(GameState game,ECountry country) => [.. GetConnectCapitalCountryAreas(game,country).SelectMany(areaInfo => GetAdjacentAnotherCountryAreas(game,country,areaInfo))];
  internal static List<EArea> GetAdjacentAnotherCountryAllAreas(GameState game,ECountry country) => [.. GetCellEachAdjacentAnotherCountryAreas(game,country).Distinct()];
  internal static bool IsPlayerSelectable(GameState game,EArea? area) => area == null || (game.PlayCountry?.MyPipe(playCountry => GetConnectCapitalCountryAreas(game,playCountry).Concat(Country.IsSleep(game,playCountry) ? [] : GetAdjacentAnotherCountryAllAreas(game,playCountry))).Contains(area.Value) ?? true);
  internal static List<EArea> GetAdjacentAnotherCountryAreas(GameState game,ECountry country,EArea area) => [.. GetAdjacentAreas(area,GetRoads(game)).Except(GetCountryAreas(game,country))];
  internal static List<EArea> GetAdjacentTargetCountryAreas(GameState game,ECountry country,EArea area) => [.. GetAdjacentAreas(area,GetRoads(game)).Intersect(GetCountryAreas(game,country))];
  private static List<EArea> GetAdjacentAreas(EArea area,Road[]? roads) => [.. roads?.Where(v => v.From == area).Select(v => v.To) ?? [],.. roads?.Where(v => v.To == area).Select(v => v.From) ?? []];
  internal static int? GetAreaDistance(GameState game,ECountry country,EArea src,EArea dst) => new List<EArea[]>([[src]]).MyPipe(v => RecGetAreaDistances(game,country,v)).MyGetIndex(v => v.Contains(dst));
  private static List<EArea[]> GetAreaDistancesFromCapital(GameState game,ECountry country) => RecGetAreaDistances(game,country,[[..Country.GetCapitalArea(game,country).MyMaybeToList()??[]]]);
  private static List<EArea[]> RecGetAreaDistances(GameState game,ECountry country,List<EArea[]> confirm) => GetMoreOneDistanceAreas(game,country,confirm).MyPipe(v => v.Length == 0 ? confirm : RecGetAreaDistances(game,country,[.. confirm,v]));
  private static EArea[] GetMoreOneDistanceAreas(GameState game,ECountry country,List<EArea[]> map) => [.. map.Last().SelectMany(v => GetAdjacentTargetCountryAreas(game,country,v)).Distinct().Except(map.SelectMany(v => v))];
  internal static List<EArea> CalcOrdDefenseAreas(GameState game,ECountry country) {
    return [.. GetConnectCapitalCountryAreas(game,country).OrderByDescending(v => ComputeAreaPressure(game,v))];
    static decimal ComputeAreaPressure(GameState game,EArea area) => ComputeAdjacentAnotherCountryAreas(game,area).Sum(adjacent => AdjacentAreaPersonRank(game,adjacent) + 1);
    static decimal AdjacentAreaPersonRank(GameState game,EArea adjacent) => Country.GetAreaCountry(game,adjacent)?.MyPipe(v => Person.GetPostPerson(game,v,new(ERole.Defense,new(adjacent))))?.MyPipe(v => Person.CalcRoleRank(game,v.Key,ERole.Defense)) ?? 0m;
    static List<EArea> ComputeAdjacentAnotherCountryAreas(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.Country?.MyPipe(country => GetAdjacentAnotherCountryAreas(game,country,area)) ?? [];
  }
  internal static List<EArea> CalcOrdAffairAreas(GameState game,ECountry country) {
    return [.. GetConnectCapitalCountryAreas(game,country).OrderByDescending(v => ComputeAreaRemAffairs(game,v))];
    static decimal ComputeAreaRemAffairs(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.AffairParam.MyPipe(v => v.AffairsMax - v.AffairNow)??0;
  }
}