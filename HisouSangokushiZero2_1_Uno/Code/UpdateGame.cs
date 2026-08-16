using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using PostType = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class UpdateGame {
  internal static GameState SetPersonPost(GameState game,Dictionary<PersonId,PostType> postMap) => game with { PersonMap = game.PersonMap.ToDictionary(v => v.Key,v => postMap.TryGetValue(v.Key,out PostType? post) ? v.Value with { Post = post } : v.Value) };
  internal static GameState RemovePersonPost(GameState game,List<PersonId> removePersons) => game with { PersonMap = removePersons.Aggregate(game.PersonMap,(fold,value) => fold.MyUpdate(value,(_,param) => param with { Post = null,GameDeathTurn = game.PlayTurn })) };
  internal static GameState InitAlivePersonPost(GameState game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetInitPost(game,country,role))).ToDictionary().MyPipe(v => SetPersonPost(game,v));
  internal static GameState InitAppearPersonPost(GameState game) {
    Dictionary<ECountry,Dictionary<PersonId,PostType>> appearPersonMap = game.CountryMap.Keys.Where(v => !Country.IsPerish(game,v)).Select(country => (country, Enum.GetValues<ERole>().SelectMany(role => Post.GetInitAppearPost(game,country,role)).ToDictionary())).ToDictionary();
    Dictionary<ECountry,Dictionary<PersonId,PersonData>> findPersonMap = game.CountryMap.Keys.Where(v => !Country.IsPerish(game,v)).Select(country => (country, (appearPersonMap.GetValueOrDefault(country) ?? []).Count == 0 ? Person.FindPerson(game,country) : [])).ToDictionary();
    List<string?> appendLog = [
      ..game.CountryMap.Keys.SelectMany(country=> new List<string?>([
        appearPersonMap.GetValueOrDefault(country)?.MyPipe(countryAppearPerson => Text.AppearPersonText(country,[..countryAppearPerson.Keys])),
        findPersonMap.GetValueOrDefault(country)?.MyPipe(countryfindPerson => Text.FindPersonText(country,[.. countryfindPerson.Keys]))
      ]))
    ];
    return appearPersonMap.Values.SelectMany(v => v).ToDictionary().MyPipe(v => SetPersonPost(game,v)).MyPipe(game => game with {
      PersonMap = game.PersonMap.Concat(findPersonMap.Values.SelectMany(v => v)).ToDictionary(),
      CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => findPersonMap.GetValueOrDefault(v.Key)?.Count != 0 ? v.Value with { AnonymousPersonNum = v.Value.AnonymousPersonNum + 1 } : v.Value),
    }).MyPipe(game => AppendLogMessage(game,appendLog)).MyPipe(game => game.PlayCountry?.MyPipe(v => AppearPersonRemark(game,[..findPersonMap.GetValueOrDefault(v)?.Keys.ToArray()??[], ..appearPersonMap.GetValueOrDefault(v)?.Keys.ToArray()??[]])) ?? game);
    static GameState AppearPersonRemark(GameState game,List<PersonId> appearPersons) => AppendStartPlanningRemark(game,[Text.AppearPersonCharacterRemarkText(appearPersons)]);
  }
  internal static GameState PutWaitPersonPost(GameState game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetPutWaitPost(game,country,role))).ToDictionary().MyPipe(v => SetPersonPost(game,v));
  internal static GameState RemoveNaturalDeathPersonPost(GameState game, int year, int inYear){
    return game.CountryMap.Keys.Select(country => (country, Enum.GetValues<ERole>().SelectMany(role => Person.GetNaturalDeathPostPersonMap(game, country, role, year, inYear).Keys).ToList())).ToDictionary().MyPipe(deathPersonMap => RemoveDeathPersonPost(game, [.. deathPersonMap.Values.SelectMany(v => v)], [.. deathPersonMap.Select(v => Text.NaturalDeathPersonText(v.Key, [.. v.Value]))]).MyPipe(game => AppendNaturalDeathRemark(game, deathPersonMap)));
    static GameState AppendNaturalDeathRemark(GameState game, Dictionary<ECountry,List<PersonId>> naturalDeathPersonMap) => AppendStartPlanningRemark(game, [Text.NaturalDeathPersonRemarkText([.. game.PlayCountry?.MyPipe(naturalDeathPersonMap.GetValueOrDefault) ?? []])]);
  }
  internal static GameState RemoveWarDeathBureaucracyPersonPost(GameState game,EArea area,List<PersonId> deathPersons) => RemoveDeathPersonPost(game,deathPersons,[Text.WarDeathBureaucracyPersonText(area,deathPersons)]);
  internal static GameState RemoveWarDeathCommanderPersonPost(GameState game,ERole role,ECountry? enemy,List<PersonId> deathPersons) => RemoveDeathPersonPost(game,deathPersons,[Text.BattleDeathCommanderPersonText(role,deathPersons,enemy)]);
  private static GameState RemoveDeathPersonPost(GameState game,List<PersonId> deathPersons,List<string?> appendLog) => RemovePersonPost(game,deathPersons).MyPipe(game => AppendLogMessage(game,appendLog));
  internal static GameState AutoPutPostCPU(GameState game,ECountry[] exceptCountrys) => game.CountryMap.Keys.Except(game.PlayCountry.MyMaybeToList().Concat(exceptCountrys)).SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetAutoPutPost(game,country,role))).ToDictionary().MyPipe(v => SetPersonPost(game,v));
  internal static GameState PutPersonFromUI(GameState game,PersonId? putPerson,PostType? putPost) => putPerson != null && putPost != null ? SetPersonPost(game,new() { { putPerson,putPost } }) : game;
  internal static GameState AttachGameStartData(GameState game,ECountry? countryName) => countryName is ECountry country ? game with { PlayCountry = country,PlayTurn = 0 } : game;
  internal static GameState AppendGameStartLog(GameState game) {
    return game.MyPipe(game => AppendLogMessage(game,[Text.StartPlayText(game.PlayCountry)])).MyPipe(game => AppendTurnNewLog(game,[Text.StartPlayText(game.PlayCountry)]))
      .MyPipe(game => AppendGameLog(game,[string.Join(" ",[Text.StartPlayText(game.PlayCountry),Text.CountryAreaNumParamText(game,game.PlayCountry),Text.StartPlayCountryPersonsText(game)])]));
  }
  internal static GameState UpdateCapitalArea(GameState game) {
    return game.MyPipe(AppendPlayerCaptialDiffText) with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,countryInfo => countryInfo.Value with { CapitalArea = Country.CalcCapitalArea(game,countryInfo.Key) }) };
    GameState AppendPlayerCaptialDiffText(GameState game) {
      EArea? prevPlayerCountryCapital = game.PlayCountry?.MyPipe(game.CountryMap.GetValueOrDefault)?.CapitalArea;
      EArea? newPlayerCountryCapital = game.PlayCountry?.MyPipe(playCountry => Country.CalcCapitalArea(game,playCountry));
      return prevPlayerCountryCapital != newPlayerCountryCapital && prevPlayerCountryCapital != null && newPlayerCountryCapital != null ? AppendStartPlanningRemark(game,[Text.ChangeCapitalCharacterRemarkText(prevPlayerCountryCapital.Value,newPlayerCountryCapital.Value)]) : game;
    }
  }
  internal static GameState PayAttackFunds(GameState game,ECountry country) => game with { CountryMap = game.CountryMap.MyUpdate(country,(_,countryInfo) => countryInfo with { Fund = countryInfo.Fund - Country.CalcAttackFund(game,country) }) };
  internal static GameState AppendGameLog(GameState game,List<string?> appendMessages) => game with { GameLog = [.. game.GameLog,.. appendMessages.MyNonNull().Select(v => $"{Text.GetCalendarText(game.NowScenario,game.PlayTurn ?? 0)}:{v}")] };
  internal static GameState AppendLogMessage(GameState game,List<string?> appendMessages) => game with { LogMessage = [.. game.LogMessage,.. appendMessages.MyNonNull()] };
  internal static GameState AppendTurnNewLog(GameState game,List<string?> appendMessages) => game with { TurnNewLog = [.. game.TurnNewLog,.. appendMessages.MyNonNull()] };
  internal static GameState AppendStartPlanningRemark(GameState game,List<string?> appendMessages) => game with { StartPlanningCharacterRemark = [.. game.StartPlanningCharacterRemark ?? [],.. appendMessages.MyNonNull()] };
  internal static GameState AppendStartExecutionRemark(GameState game,List<string?> appendMessages) => game with { StartExecutionCharacterRemark = [.. game.StartExecutionCharacterRemark ?? [],.. appendMessages.MyNonNull()] };
    private static GameState SleepCountry(GameState game,ECountry attackCountry,int sleepTurnNum) => game with { CountryMap = game.CountryMap.MyUpdate(attackCountry,(_,countryInfo) => countryInfo with { SleepTurnNum = sleepTurnNum }) };
  private static GameState DeathCommander(GameState game,Army army,ERole role,ECountry? enemy) {
    List<PersonId> deathPersons = [.. new PersonId?[] { army.Commander.MainPerson,army.Commander.SubPerson }.MyNonNull().Where(_ => MyRandom.RandomJudge(0.25))];
    return deathPersons.Count == 0 ? game : game.MyPipe(game => AppendLog(game,army,role,enemy,deathPersons)).MyPipe(game => RemoveWarDeathCommanderPersonPost(game,role,enemy,deathPersons));
    static GameState AppendLog(GameState game,Army army,ERole role,ECountry? enemy,List<PersonId> deathPersons) => army.Country == game.PlayCountry ? AppendGameLog(game,[Text.BattleDeathCommanderPersonText(role,deathPersons,enemy)]).MyPipe(game => AppendStartPlanningRemark(game,[Text.BattleDeathPersonCharacterRemarkText(role,deathPersons,enemy)])) : game;
  }
  internal static GameState NextTurn(GameState game) {
    return game.MyPipe(UpdateCapitalArea).MyPipe(AddTurn).MyPipe(AddTurnHeadLog).MyPipe(InOutFunds).MyPipe(AddAffair).MyPipe(InitAppearPersonPost).MyPipe(RemoveDeathPersonPost).MyPipe(PutWaitPersonPost);
    static GameState AddTurn(GameState game) => game with { PlayTurn = game.PlayTurn + 1 };
    static GameState InOutFunds(GameState game) => game with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => v.Value with { Fund = v.Value.Fund + Country.GetInFund(game,v.Key) - Country.GetOutFund(game,v.Key) }) };
    static GameState AddAffair(GameState game) => game with {
      AreaMap = game.AreaMap.ToDictionary(area => area.Key,area => area.Value with {
        AffairParam = area.Value.AffairParam with {
          AffairNow = Math.Clamp(area.Value.AffairParam.AffairNow + AddNowAfair(game,area),0,area.Value.AffairParam.AffairsMax),
          AffairsMax = area.Value.AffairParam.AffairsMax + Math.Round(area.Value.AffairParam.AffairsMax * 0.001m + 0.01m + AddNowAfair(game,area) * 0.05m,4)
        }
      })
    };
    static GameState RemoveDeathPersonPost(GameState game) => Turn.GetInYear(game) == Enum.GetValues<YearItems>().Length / 2 ? RemoveNaturalDeathPersonPost(game,Turn.GetYear(game),Turn.GetInYear(game)) : game;
    static decimal AddNowAfair(GameState game,KeyValuePair<EArea,AreaData> area) {
      decimal countryAffairPower = Country.GetAffairPower(game,area.Value.Country) / Country.GetAffairDifficult(game,area.Value.Country);
      decimal personAffairPower = game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post?.PostRole == ERole.Affair && v?.Value.Post?.PostKind == new PostKind(area.Key))?.MyPipe(v => Person.CalcRoleRank(game,v.Key,ERole.Affair)) ?? 0;
      decimal areaAffairPower = 1 - area.Value.AffairParam.AffairNow / area.Value.AffairParam.AffairsMax;
      return Math.Round(countryAffairPower * personAffairPower * areaAffairPower,4);
    }
    static GameState AddTurnHeadLog(GameState game) => AppendLogMessage(game,[Text.TurnHeadLogText(game)]);
  }
  internal static GameState UpdateFillDreamCondition(GameState game) {
    Dictionary<ECountry, FillDream> newFillDreamsValue = game.FillDreams.ToDictionary(v => v.Key, v => game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.FillDreamConditionMap.GetValueOrDefault(v.Key)?.JudgeFunc(game) is false ? FillDream.None : FillDream.Passed);
    List<ECountry> fillDreamSides = [.. game.FillDreams.Where(v=>v.Value == FillDream.None && newFillDreamsValue.GetValueOrDefault(v.Key) == FillDream.Passed).Select(v=>v.Key)];
    List<ECountry> lostDreamSides = [.. game.FillDreams.Where(v=>v.Value == FillDream.Passed && newFillDreamsValue.GetValueOrDefault(v.Key) == FillDream.None).Select(v=>v.Key)];
    List<string?> message = [Text.FillDreamCountrysText(fillDreamSides),Text.LostDreamCountrysText(lostDreamSides)];
    List<string?> fillDreamRemark = [Text.FillDreamAnotherCountrysRemarkText([.. fillDreamSides.Where(v => v != game.PlayCountry)]),fillDreamSides.MyNullable().Contains(game.PlayCountry) ? Text.FillDreamRemarkText(): null];
    List<string?> lostDreamRemark = [Text.LostDreamAnotherCountrysRemarkText([.. lostDreamSides.Where(v => v != game.PlayCountry)]),lostDreamSides.MyNullable().Contains(game.PlayCountry) ? Text.LostDreamRemarkText(): null];
    return (game with { FillDreams = newFillDreamsValue }).MyPipe(v => AppendLogMessage(v,message)).MyPipe(v => AppendTurnNewLog(v,message)).MyPipe(v => AppendGameLog(v,message)).MyPipe(v => AppendStartPlanningRemark(v,[.. fillDreamRemark,.. lostDreamRemark]));
  }
  internal static GameState UpdateHegemonyTurn(GameState game){
    List<ECountry> addHegemonyTurnCountry = [.. HegemonyPoint.hegemonyPoints.Where(v=>IsHegemony(v.Value)).Select(v=>v.Key)];
    Dictionary<ECountry,int> newHegemonyTurns = game.HegemonyTurns.ToDictionary(v => v.Key,v => v.Value + (addHegemonyTurnCountry.Contains(v.Key) ? 1 : 0));
    List<string?> hegemonyAnotherCountrysRemarkText = [.. Enumerable.Range(1,2).Select(count=>Text.HegemonyAnotherCountrysRemarkText(count,[.. addHegemonyTurnCountry.Where(v => v != game.PlayCountry && newHegemonyTurns.GetValueOrDefault(v) == count)]))];
    List<string?> hegemonyRemarkText = [addHegemonyTurnCountry.MyNullable().Contains(game.PlayCountry) ? game.PlayCountry?.MyPipe(newHegemonyTurns.GetValueOrDefault).MyPipe(Text.HegemonyRemarkText) : null];
    return (game with { HegemonyTurns = newHegemonyTurns }).MyPipe(game => AppendStartPlanningRemark(game,[.. hegemonyAnotherCountrysRemarkText,.. hegemonyRemarkText]));
  }
  internal static bool IsHegemony(double hegemonyPoint) => hegemonyPoint >= HegemonyPoint.totalHegemonyPoint * 0.4;
  internal static GameState GameEndJudge(GameState game) {
    return SetWinCountrys(game).MyPipe(game => IsPerish(game) ? PerishEnd(game) : IsWinEnd(game) ? WinEnd(game) : IsOtherWinEnd(game) ? OtherWinEnd(game) : IsTurnLimitOver(game) ? TurnLimitOverEnd(game) : game);
    static GameState SetWinCountrys(GameState game) => game with { WinCountrys = [.. game.HegemonyTurns.Where(v => v.Value >= 3).Select(v => v.Key)] };
    static GameState WinEnd(GameState game) => (game with { Phase = Phase.WinEnd }).MyPipe(game => AppendGameLog(game, [Text.WinEndText(game)]));
    static GameState OtherWinEnd(GameState game) => (game with { Phase = Phase.OtherWinEnd }).MyPipe(game => AppendGameLog(game, [Text.OtherWinEndText(game)]));
    static GameState PerishEnd(GameState game) => (game with { Phase = Phase.PerishEnd }).MyPipe(game => AppendGameLog(game, [Text.PerishEndText(game)]));
    static GameState TurnLimitOverEnd(GameState game) => (game with { Phase = Phase.TurnLimitOverEnd }).MyPipe(game => AppendGameLog(game, [Text.TurnLimitOverEndText(game)]));
    static bool IsWinEnd(GameState game) => game.PlayCountry?.MyPipe(game.WinCountrys.Contains) ?? false;
    static bool IsOtherWinEnd(GameState game) => !IsWinEnd(game) && game.WinCountrys.Length != 0;
    static bool IsTurnLimitOver(GameState game) => Turn.GetYear(game) >= game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.EndYear;
    static bool IsPerish(GameState game) => game.PlayCountry?.MyPipe(game.CountryMap.GetValueOrDefault)?.PerishFrom != null;
  }
  internal static GameState Attack(GameState game,ECountry attackCountry,EArea targetArea,ECountry? defenseCountry,bool defenseSideFocusDefense) {
    Army attackArmy = Commander.GetAttackCommander(game,attackCountry).MyPipe(commander => new Army(attackCountry,commander,Commander.CommanderRank(game,commander,ERole.Attack)));
    AttackResult? countryBattle = Battle.Country.Attack(game,defenseCountry,targetArea,attackArmy,defenseSideFocusDefense);
    AttackResult areaBattle = Battle.Area.Attack(game,defenseCountry,targetArea,attackArmy,defenseSideFocusDefense);
    return game.MyPipe(game => PayAttackFunds(game,attackCountry)).MyPipe(game => AttackSideDamage(game,attackCountry)).MyPipe(game => ExeBattle(game,areaBattle,countryBattle,attackCountry,targetArea,attackArmy));
    static GameState AttackSideDamage(GameState game,ECountry attackCountry) => game with { AreaMap = game.AreaMap.ToDictionary(v => v.Key,v => v.Value.Country == attackCountry ? v.Value with { AffairParam = v.Value.AffairParam with { AffairNow = Math.Round(v.Value.AffairParam.AffairNow * 0.99m,4) } } : v.Value) };
    static GameState ExeBattle(GameState game,AttackResult areaBattle,AttackResult? countryBattle,ECountry attackCountry,EArea targetArea,Army attackArmy) {
      return game.MyPipe(game => BattleDefenseSideArea(game,areaBattle,attackCountry,targetArea,attackArmy,countryBattle != null)).MyPipe(game => areaBattle.Judge is AttackJudge.Lose or AttackJudge.Rout ? game : BattleDefenseSideCentral(game,countryBattle,attackCountry,targetArea,attackArmy));
      static GameState BattleDefenseSideArea(GameState game,AttackResult areaBattle,ECountry attackCountry,EArea targetArea,Army attackArmy,bool isCentralBattle) => AppendLogMessage(game,[areaBattle.InvadeText]).MyPipe(game => areaBattle.Judge.MyPipe(judge => AreaAttack(game,attackCountry,targetArea,attackArmy,areaBattle.Defense,judge,isCentralBattle)));
      static GameState BattleDefenseSideCentral(GameState game,AttackResult? countryBattle,ECountry attackCountry,EArea targetArea,Army attackArmy) => countryBattle?.Judge.MyPipe(judge => AppendLogMessage(game,[countryBattle.InvadeText]).MyPipe(game => CountryAttack(game,attackCountry,targetArea,attackArmy,countryBattle.Defense,judge))) ?? game;
    }
    static GameState CountryAttack(GameState game,ECountry attackSide,EArea target,Army attack,Army defense,AttackJudge judge) {
      return judge switch { AttackJudge.Crush => Crush(game,attackSide,target,defense), AttackJudge.Win => Win(game,attackSide,target,defense), AttackJudge.Lose => Lose(game,attackSide,target,defense), AttackJudge.Rout => Rout(game,attackSide,target,attack,defense) };
      static GameState Crush(GameState game,ECountry attackSide,EArea target,Army defense) => game.MyPipe(game => FailAreaDefense(game,attackSide,defense.Country,target,false)).MyPipe(game => DeathCommander(game,defense,ERole.Defense,attackSide));
      static GameState Win(GameState game,ECountry attackSide,EArea target,Army defense) => game.MyPipe(game => FailAreaDefense(game,attackSide,defense.Country,target,false)).MyPipe(game => SleepCountry(game,attackSide,1));
      static GameState Lose(GameState game,ECountry attackSide,EArea target,Army defense) => game.MyPipe(game => SuccessAreaDefense(game,attackSide,defense.Country,target)).MyPipe(game => SleepCountry(game,attackSide,1));
      static GameState Rout(GameState game,ECountry attackSide,EArea target,Army attack,Army defense) => game.MyPipe(game => SuccessAreaDefense(game,attackSide,defense.Country,target)).MyPipe(game => SleepCountry(game,attackSide,3).MyPipe(game => DeathCommander(game,attack,ERole.Attack,defense.Country)));
    }
    static GameState AreaAttack(GameState game,ECountry attackSide,EArea target,Army attack,Army defense,AttackJudge judge,bool isCentralBattle) {
      return judge switch { AttackJudge.Crush => Crush(game,attackSide,target,defense,isCentralBattle), AttackJudge.Win => Win(game,attackSide,target,defense,isCentralBattle), AttackJudge.Lose => Lose(game,attackSide,target,defense,isCentralBattle), AttackJudge.Rout => Rout(game,attackSide,target,attack,defense,isCentralBattle) };
      static GameState Crush(GameState game,ECountry attackSide,EArea target,Army defense,bool isCentralBattle) => game.MyPipe(game => FailAreaDefense(game,attackSide,defense.Country,target,isCentralBattle)).MyPipe(game => DeathCommander(game,defense,ERole.Defense,attackSide));
      static GameState Win(GameState game,ECountry attackSide,EArea target,Army defense,bool isCentralBattle) => game.MyPipe(game => FailAreaDefense(game,attackSide,defense.Country,target,isCentralBattle)).MyPipe(game => SleepCountry(game,attackSide,1));
      static GameState Lose(GameState game,ECountry attackSide,EArea target,Army defense,bool isCentralBattle) => game.MyPipe(game => SuccessAreaDefense(game,attackSide,defense.Country,target)).MyPipe(game => SleepCountry(game,attackSide,1)).MyPipe(game => DamageArea(game,target));
      static GameState Rout(GameState game,ECountry attackSide,EArea target,Army attack,Army defense,bool isCentralBattle) => game.MyPipe(game => SuccessAreaDefense(game,attackSide,defense.Country,target)).MyPipe(game => DeathCommander(game,attack,ERole.Attack,defense.Country)).MyPipe(game => SleepCountry(game,attackSide,3)).MyPipe(game => DamageArea(game,target));
      static GameState DamageArea(GameState game,EArea targetArea) => game with { AreaMap = game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { AffairParam = areaInfo.AffairParam with { AffairNow = Math.Round(areaInfo.AffairParam.AffairNow * 0.95m,4) } }) };
    }
    static GameState FailAreaDefense(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea,bool isExistFollow){
      return game.MyPipe(game => isExistFollow ? game : game.MyPipe(game => AttachChangeHasCountryRemark(game,attackSide,defenseSide,targetArea)).MyPipe(game => ChangeHasCountry(game,attackSide,defenseSide,targetArea)));
      static GameState AttachChangeHasCountryRemark(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) {
        return game.MyPipe(game => AttachGetAreaRemark(game,attackSide,defenseSide,targetArea)).MyPipe(game => AttachLostAreaRemark(game,attackSide,defenseSide,targetArea));
        static GameState AttachGetAreaRemark(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) => attackSide == game.PlayCountry ? AppendStartPlanningRemark(game,[Text.GetAreaCharacterRemarkText(defenseSide,targetArea)]) : game;
        static GameState AttachLostAreaRemark(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) => defenseSide == game.PlayCountry ? AppendStartPlanningRemark(game,[Text.LostAreaCharacterRemarkText(attackSide,targetArea)]) : game;
      }
      static GameState ChangeHasCountry(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) {
        return AppendChangeHasCountryLog(game,attackSide,defenseSide,targetArea).MyPipe(game => UpdateAreaMap(game,attackSide,targetArea)).MyPipe(game => DeathBureaucracy(game,defenseSide,targetArea)).MyPipe(game => MakeEmptyPost(game,targetArea)).MyPipe(game => defenseSide?.MyPipe(v => IsPerishCountry(game,targetArea,v) ? PerishSide(game,attackSide,v,targetArea) : IsFallCapital(game,targetArea,v) ? FallCapital(game,v,targetArea) : game) ?? game).MyPipe(game => FallArea(game,attackSide,defenseSide,targetArea));
        static GameState UpdateAreaMap(GameState game,ECountry attackCountry,EArea targetArea) => game with { AreaMap = game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { Country = attackCountry }) };
        static GameState DeathBureaucracy(GameState game,ECountry? defenseSide,EArea area) {
          List<PersonId> deathPersons = [.. game.PersonMap.Where(v => v.Value.Post?.MyPipe(v => v.PostKind.MaybeArea == area && v.PostRole != ERole.Defense) ?? false).Select(v => v.Key).Where(_ => MyRandom.RandomJudge(0.25))];
          return RemoveWarDeathBureaucracyPersonPost(game,area,deathPersons).MyPipe(game => defenseSide == game.PlayCountry ? AppendDeathPersonLog(game,area,deathPersons) : game);
          static GameState AppendDeathPersonLog(GameState game,EArea area,List<PersonId> deathPersons) => AppendGameLog(game,[Text.WarDeathBureaucracyPersonText(area,deathPersons)]).MyPipe(game => AppendStartPlanningRemark(game,[Text.WarDeathBureaucracyPersonCharacterRemarkText(area,deathPersons)]));
        }
        static GameState MakeEmptyPost(GameState game,EArea targetArea) => game with { PersonMap = game.PersonMap.ToDictionary(v => v.Key,v => v.Value.Post?.PostKind == new PostKind(targetArea) ? v.Value with { Post = v.Value.Post with { PostKind = new() } } : v.Value) };
        static bool IsPerishCountry(GameState game,EArea targetArea,ECountry? defenseSide) => defenseSide?.MyPipe(country => Country.GetAreaNum(game,country)) == 0;
        static bool IsFallCapital(GameState game,EArea targetArea,ECountry? defenseSide) => defenseSide?.MyPipe(game.CountryMap.GetValueOrDefault)?.CapitalArea == targetArea;
        static GameState PerishSide(GameState game,ECountry attackSide,ECountry defenseSide,EArea area) {
          return game.MyPipe(game => AppendLogs(game,defenseSide)).MyPipe(game => RemoveWarDeathBureaucracyPersonPost(game,area,GetDefenseSidePerson(game,defenseSide))).MyPipe(game => AttachPerishFrom(game,defenseSide,attackSide)).MyPipe(game => attackSide == game.PlayCountry ? AppendPerishToRemark(game,defenseSide) : game);
          static List<PersonId> GetDefenseSidePerson(GameState game,ECountry? defenseSide) => [.. defenseSide?.MyPipe(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role)).Select(v => v.Key)) ?? []];
          static GameState AppendLogs(GameState game,ECountry? defenseSide) => AppendLogMessage(game,[Text.PerishCountryText(defenseSide)]).MyPipe(game => AppendTurnNewLog(game,[Text.PerishCountryText(defenseSide)])).MyPipe(game => AppendGameLog(game,[Text.PerishCountryText(defenseSide)]));
          static GameState AttachPerishFrom(GameState game,ECountry? defenseSide,ECountry attackSide) => defenseSide?.MyPipe(country => game with { CountryMap = game.CountryMap.MyUpdate(country,(_,info) => info with { PerishFrom = attackSide }) }) ?? game;
          static GameState AppendPerishToRemark(GameState game,ECountry defenseSide) => AppendStartPlanningRemark(game,[Text.PerishSideCharacterRemarkText(defenseSide)]);
        }
        static GameState AppendChangeHasCountryLog(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) {
          return AppendLogMessage(game,[Text.ChangeHasCountryText(attackSide,defenseSide,targetArea)]).MyPipe(game => AppendTurnNewLog(game,[Text.ChangeHasCountryText(attackSide,defenseSide,targetArea)]));
        }
        static GameState FallCapital(GameState game,ECountry country,EArea area) {
          List<PersonId> defenseCountryCapitalPersons = [.. Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role)).Where(v => v.Value.Post?.PostKind.MaybeArea == null).Select(v => v.Key)];
          List<PersonId> defenseCountrySortiePersons = [.. game.ArmyTargetMap.GetValueOrDefault(country) == null ? [] : Commander.GetAttackCommander(game,country).MyPipe(v => new List<PersonId?> { v.MainPerson,v.SubPerson }.MyNonNull())];
          List<PersonId> deathPersons = [.. defenseCountryCapitalPersons.Except(defenseCountrySortiePersons).Where(_ => MyRandom.RandomJudge(0.5))];
          return game.MyPipe(game => AppendFallCapitalTextToTurnNewLog(game,country)).MyPipe(game => AppendFallCapitalTextToNewLog(game,country)).MyPipe(game => AppendFallCapitalTextToGameLog(game,country,area,deathPersons)).MyPipe(game => AppendFallCapitalTextToStartPlanningLog(game,country,area,deathPersons)).MyPipe(game => RemoveWarDeathBureaucracyPersonPost(game,area,deathPersons));
          static GameState AppendFallCapitalTextToTurnNewLog(GameState game,ECountry country) => AppendTurnNewLog(game,[Text.FallCapitalText(country)]);
          static GameState AppendFallCapitalTextToNewLog(GameState game,ECountry country) => AppendLogMessage(game,[Text.FallCapitalText(country)]);
          static GameState AppendFallCapitalTextToGameLog(GameState game,ECountry country,EArea area,List<PersonId> deathPersons) => AppendGameLog(game,country == game.PlayCountry ? [Text.FallPlayerCapitalText(area),Text.FallPlayerCapitalDeathPersonText(deathPersons)] : []);
          static GameState AppendFallCapitalTextToStartPlanningLog(GameState game,ECountry country,EArea area,List<PersonId> deathPersons) => AppendStartPlanningRemark(game,country == game.PlayCountry ? [Text.FallPlayerCapitalCharacterRemarkText(area),Text.FallPlayerCapitalDeathPersonCharacterRemarkText(deathPersons)] : []);
        }
        static GameState FallArea(GameState game,ECountry attackCountry,ECountry? defenseCountry,EArea targetArea) {
          return game.MyPipe(game => IsPlayerUpdateMaxAreaNum(game,defenseCountry,targetArea)).MyPipe(game => FallDamageArea(game,targetArea)).MyPipe(game => UpdateMaxAreaNum(game,attackCountry,targetArea));
          static GameState IsPlayerUpdateMaxAreaNum(GameState game,ECountry? defenseCountry,EArea targetArea) {
            return game.PlayCountry?.MyPipe(game.CountryMap.GetValueOrDefault)?.MaxAreaNum < game.PlayCountry?.MyPipe(country => Country.GetAreaNum(game,country)) ? AppendUpdateMaxAreaNumLog(game,defenseCountry,targetArea) : game;
            static GameState AppendUpdateMaxAreaNumLog(GameState game,ECountry? defenseCountry,EArea targetArea) => AppendGameLog(game,[Text.AppendUpdateMaxAreaNumLog(game.PlayCountry?.MyPipe(country => Country.GetAreaNum(game,country)),defenseCountry,targetArea)]);
          }
          static GameState FallDamageArea(GameState game,EArea targetArea) {
            return game with { AreaMap = game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { AffairParam = areaInfo.AffairParam with { AffairNow = Math.Round(areaInfo.AffairParam.AffairNow * 0.9m,4),AffairsMax = Math.Round(areaInfo.AffairParam.AffairsMax * 0.95m,4) } }) };
          }
          static GameState UpdateMaxAreaNum(GameState game,ECountry attackCountry,EArea targetArea) {
            return game with { CountryMap = game.CountryMap.MyUpdate(attackCountry,(_,countryInfo) => countryInfo with { MaxAreaNum = Math.Max(countryInfo.MaxAreaNum ?? 0,Country.GetAreaNum(game,attackCountry)) }) };
          }
        }
      }
    }
    static GameState SuccessAreaDefense(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea){
      return game.MyPipe(game => AttachNotChangeHasCountryRemark(game,attackSide,defenseSide,targetArea));
      static GameState AttachNotChangeHasCountryRemark(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) {
        return game.MyPipe(game => AttachNotGetAreaRemark(game,attackSide,defenseSide,targetArea)).MyPipe(game => AttachNotLostAreaRemark(game,attackSide,defenseSide,targetArea));
        static GameState AttachNotGetAreaRemark(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) => attackSide == game.PlayCountry ? AppendStartPlanningRemark(game,[Text.NotGetAreaCharacterRemarkText(defenseSide,targetArea)]) : game;
        static GameState AttachNotLostAreaRemark(GameState game,ECountry attackSide,ECountry? defenseSide,EArea targetArea) => defenseSide == game.PlayCountry ? AppendStartPlanningRemark(game,[Text.NotLostAreaCharacterRemarkText(attackSide,targetArea)]) : game;
      }
    }
  }
  internal static GameState Defense(GameState game,ECountry country,bool isTryAttack) => game.MyPipe(game => isTryAttack ? game with { ArmyTargetMap = game.ArmyTargetMap.MyRemove(country) } : game).MyPipe(game => AppendLogMessage(game,[Text.DefenseText(country,isTryAttack)]));
  internal static GameState Sleep(GameState game, ECountry country) => AppendLogMessage(game, [Text.SleepText(country, Country.GetSleepTurn(game, country))]);
  internal static GameState Rest(GameState game) => game with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => Country.IsSleep(game,v.Key) && game.ArmyTargetMap.GetValueOrDefault(v.Key) is null ? v.Value with { SleepTurnNum = v.Value.SleepTurnNum - 1 } : v.Value)};
  internal static GameState CalcArmyTarget(GameState game) {
    Dictionary<ECountry,EArea?> playerArmyTargetMap = new(game.PlayCountry is ECountry player ? [new(player,null)]:[]);
    Dictionary<ECountry,EArea?> NPCArmyTargetMap = game.CountryMap.Keys.Where(v => Country.GetAreaNum(game,v) >= 1).Except(game.PlayCountry.MyMaybeToList()).Where(country => !Country.IsSleep(game,country)).ToDictionary(country => country,country => country == ECountry.漢 ? null : RandomSelectNPCAttackTarget(game,country));
    return game with { ArmyTargetMap = new([.. NPCArmyTargetMap,.. playerArmyTargetMap]) };
    static EArea? RandomSelectNPCAttackTarget(GameState game,ECountry country) {
      List<EArea> targetAreas = Area.GetCellEachAdjacentAnotherCountryAreas(game,country);
      Dictionary<EArea,int> targetAreaCountMap = targetAreas.CountBy(v => v).ToDictionary();
      List<EArea?> selectWeightTargetAreas = [.. targetAreaCountMap.SelectMany(v => Enumerable.Repeat(v.Key,v.Value * v.Value * (Country.GetAreaCountry(game,v.Key)?.MyPipe(v => game.FillDreams.GetValueOrDefault(v) is FillDream.Passed || IsHegemony(HegemonyPoint.hegemonyPoints.GetValueOrDefault(v))) is true ? 10 :1))).MyNullable().Append(null)];
      return selectWeightTargetAreas.MyPickAny().MyPipe(area => area?.MyPipe(game.AreaMap.GetValueOrDefault)?.Country == null && MyRandom.RandomJudge(0.9) ? null : area);
    }
  }
}