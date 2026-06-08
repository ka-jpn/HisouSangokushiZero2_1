using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using Uno.Extensions.Specialized;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Route {
  internal record Edge(EArea To,double Cost);
  internal record Connect(double? TotalCost,EArea From);
  internal static EArea[] SolveAtackArmyRoute(GameState game,ECountry country,EArea target) {
    Road[] roadConnections = game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.RoadConnections ?? [];
    List<EArea> connectCapitalCountryAreas = Area.GetConnectCapitalCountryAreas(game,country);
    EArea[] targetFrontAreas = [.. roadConnections.Select(v => v.From == target ? v.To : v.To == target ? v.From : (EArea?)null).MyNonNull().Where(connectCapitalCountryAreas.Contains)];
    Road[] countryAreaRoad = [.. roadConnections.Where(v => new EArea[] { v.From,v.To }.All(connectCapitalCountryAreas.Contains)) ?? []];
    EArea? maybeCapitalArea = Country.GetCapitalArea(game,country);
    List<(EArea[], double?)> routes = [.. maybeCapitalArea?.MyPipe(capitalArea => targetFrontAreas.Select(v => SolveRoute(countryAreaRoad,capitalArea,v))) ?? []];
    EArea[] minToute = routes.MinBy(v => v.Item2 + EasinessToCost(roadConnections.Select(road => v.Item1.Last() == road.From ? road.Easiness : v.Item1.Last() == road.To ? road.EasinessReverse ?? road.Easiness : (int?)null).MyNonNull().FirstOrDefault())).Item1;
    return [.. minToute.Append(target)];
  }
  internal static (EArea[], double?) SolveRoute(Road[] road,EArea start,EArea end) {
    Dictionary<EArea,Connect> connectMap = SolveConnects(road,start);
    EArea[] route = [end];
    EArea search = end;
    while(search != start) {
      EArea? from = connectMap.GetValueOrDefault(search)?.From;
      if(from is not null) {
        search = from.Value;
        route = [.. route.Prepend(from.Value)];
      } else {
        route = [];
      }
    }
    return (route, connectMap.GetValueOrDefault(end)?.TotalCost);
  }
  internal static Dictionary<EArea,Connect> SolveConnects(Road[] road,EArea start) {
    if(road.Length == 0) {
      return new([new(start,new(0,start))]);
    } else {
      PriorityQueue<(EArea from, EArea to),double> priorityQueue = new();
      priorityQueue.Enqueue((start, start),0);
      Dictionary<EArea,Connect> costs = road.SelectMany(v => new List<EArea>() { v.From,v.To }).Distinct().ToDictionary(v => v,v => new Connect(null,v));
      Dictionary<EArea,bool> visited = road.SelectMany(v => new List<EArea>() { v.From,v.To }).Distinct().ToDictionary(v => v,v => false);
      while(priorityQueue.TryDequeue(out var nodeIndex,out var currentCost)) {
        if(visited[nodeIndex.to]) { continue; }
        costs[nodeIndex.to] = new(currentCost,nodeIndex.from);
        visited[nodeIndex.to] = true;
        foreach(Edge edge in road.Select(v => nodeIndex.to == v.From ? new Edge(v.To,EasinessToCost(v.Easiness)) : nodeIndex.to == v.To ? new Edge(v.From,EasinessToCost(v.EasinessReverse ?? v.Easiness)) : null).MyNonNull()) {
          priorityQueue.Enqueue((nodeIndex.to, edge.To),currentCost + edge.Cost);
        }
      }
      return costs;
    }
  }
  private static int EasinessToCost(int Easiness) => 100 - Easiness;
}