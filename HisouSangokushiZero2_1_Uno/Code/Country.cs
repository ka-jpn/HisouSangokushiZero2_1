using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Country {
    private static readonly Color nonCountryColor = new(255,240,240,240);
    internal static Color GetCountryColor(GameState game,ECountry? country) => country?.MyPipe(v=>game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.CountryMap.GetValueOrDefault(v))?.ViewColor ?? nonCountryColor;
    internal static decimal GetTotalAffair(GameState game,ECountry? country) => country == null ? 0 : game.AreaMap.Where(v => v.Value.Country == country).Sum(v => v.Value.AffairParam.AffairNow * (v.Key == country?.MyPipe(game.CountryMap.GetValueOrDefault)?.CapitalArea ? 1.5m : 1m));
		internal static decimal GetAffairPower(GameState game,ECountry? country) => Commander.GetAffairsCommander(game,country).MyPipe(v => Commander.CommanderRank(game,v,ERole.Affair)).MyPipe(affairsRank => affairsRank/5m+1);
    internal static decimal GetAffairDifficult(GameState game,ECountry? country) => country == null ? 1 : Math.Round((decimal)Math.Pow(GetAreaNum(game,country),0.5),4);
		internal static decimal GetInFund(GameState game,ECountry? country) => GetAreaNum(game,country)==0 ? 0 : Math.Round(GetTotalAffair(game,country)*GetAffairPower(game,country)/GetAffairDifficult(game,country)+10m/GetAreaNum(game,country),4);
		internal static decimal GetOutFund(GameState game,ECountry? country) {
      Dictionary<PersonId,PersonData> deployedPersonMap = (country?.MyPipe(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role).ExceptBy(Person.GetWaitPostPersonMap(game,country,role).Keys,v => v.Key))) ?? []).ToDictionary();
			decimal backCost = GetAreaNum(game,country)==0 ? 0 : Math.Round((decimal)(1-Math.Pow(0.9,(double)GetAffairDifficult(game,country)))*GetTotalAffair(game,country)/GetAffairDifficult(game,country),4);
			decimal roleCost = deployedPersonMap.Sum(v => v.Value.Post?.PostKind.MaybeHead==PostHead.Main ? 1 : v.Value.Post?.PostKind.MaybeHead==PostHead.Sub ? 0.5m : v.Value.Post?.PostKind.MaybeArea!=null ? 0.5m : 0);
      decimal affairCost = deployedPersonMap.Sum(v => v.Value.Post?.PostKind.MaybeArea != null && v.Value.Post?.PostRole == ERole.Affair ? Person.CalcRoleRank(game,v.Key,ERole.Affair) * 2 : 0);
			decimal personCost = deployedPersonMap.Sum(v => Person.GetPersonRank(game,v.Key)/10m);
			return backCost+roleCost+affairCost+personCost;
		}
		internal static EArea? GetArmyTargetArea(GameState game,ECountry? counry) => counry?.MyPipe(game.ArmyTargetMap.GetValueOrDefault);
    internal static List<EArea> GetAreas(GameState game,ECountry? country) => [.. game.AreaMap.Where(v => v.Value.Country == country).Select(v => v.Key)];
    internal static int GetAreaNum(GameState game,ECountry? country) => country == null ? 0 : GetAreas(game,country).Count;
    internal static ECountry? GetAreaCountry(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.Country;
		internal static int? SearchPersonRank(GameState game,ECountry country) {
			decimal mainSearchRank = Person.GetPostPerson(game,country,new(ERole.Central,new(PostHead.Main)))?.MyPipe(v => Person.CalcRoleRank(game,v.Key,ERole.Central))??0;
			decimal subSearchRank = Person.GetPostPerson(game,country,new(ERole.Central,new(PostHead.Sub)))?.MyPipe(v => Person.CalcRoleRank(game,v.Key,ERole.Central))??0;
			decimal searchPersonRank = mainSearchRank+subSearchRank/2;
			return !MyRandom.RandomJudge((double)(searchPersonRank+1)/30) ? null : MyRandom.RandomJudge((double)searchPersonRank/100) ? 2 : 1;
		}
    internal static bool IsFocusDefense(GameState game,ECountry? country) => game.Phase == Phase.Execution && country != null && game.ArmyTargetMap.TryGetValue(country.Value,out EArea? target) && target == null;
    internal static decimal CalcAttackFund(GameState game,ECountry country) => Commander.GetAttackCommander(game,country).MyPipe(v => Commander.CommanderRank(game,v,ERole.Attack)).MyPipe(attackRank => (attackRank+1)*50);
		internal static bool CanPayAttackFund(GameState game, ECountry country) => CalcAttackFund(game,country)<= game.CountryMap.GetValueOrDefault(country)?.Fund;
    internal static int GetSleepTurn(GameState game, ECountry? country) => country?.MyPipe(game.CountryMap.GetValueOrDefault)?.SleepTurnNum??0;
		internal static bool IsSleep(GameState game, ECountry? country) => GetSleepTurn(game,country) > 0;
    internal static bool SuccessAttack(GameState game,ECountry country) => CanPayAttackFund(game,country) && !IsSleep(game,country);
    internal static decimal GetFund(GameState game,ECountry? country) => country?.MyPipe(game.CountryMap.GetValueOrDefault)?.Fund??0;
    internal static ECountry? GetPerishFrom(GameState game,ECountry country) => game.CountryMap.GetValueOrDefault(country)?.PerishFrom;
    internal static bool IsPerish(GameState game,ECountry country) => GetPerishFrom(game,country) != null;
    internal static int HasAreaCount(GameState game,ECountry country,EArea[] areas) => GetAreas(game,country).Intersect(areas).Count();
    internal static bool HasAreas(GameState game,ECountry country,EArea[] areas) => HasAreaCount(game,country,areas) == areas.Length;
    internal static EArea? GetCapitalArea(GameState game,ECountry? country) => country?.MyPipe(game.CountryMap.GetValueOrDefault)?.CapitalArea;
    internal static EArea? CalcCapitalArea(GameState game,ECountry country) => country == ECountry.漢 ? null : Area.GetConnectCapitalCountryAreas(game,country).Except(GetCapitalArea(game,country).MyMaybeToList().MyPipe(v=> HasAreas(game,country,[..v]) ? [] : v)).MyPipe(v => v.MyIsEmpty() ? Area.GetCountryAreas(game,country) : v).MyNullable().MaxBy(v =>v?.MyPipe(game.AreaMap.GetValueOrDefault)?.AffairParam.AffairNow);
  }
}