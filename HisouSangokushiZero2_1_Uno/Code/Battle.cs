using HisouSangokushiZero2_1_Uno.Data.Language;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Battle {
  private record ThresholdInfo(KeyValuePair<double,double> Lower,KeyValuePair<double,double> Upper);
  private static readonly Dictionary<double,double> crushingThresholds = new() { { int.MinValue,0 },{ int.MaxValue,0 } };
  private static readonly Dictionary<double,double> winThresholds = new() { { int.MinValue,1 },{ -5,1 },{ -3,2 },{ -1,4 },{ 1,9 },{ 1.5,10.5 },{ 2,12.5 },{ 2.5,16 },{ 3,20 },{ 3.5,25 },{ 4,32 },{ 4.5,40 },{ 5,50 },{ int.MaxValue,50 } };
  private static readonly Dictionary<double,double> defeatThresholds = new() { { int.MinValue,4 },{ -4,4 },{ -3,8 },{ -2,14 },{ -1,24 },{ 0,40 },{ 1,60 },{ 2,76 },{ 3,86 },{ 4,92 },{ 5,96 },{ int.MaxValue,96 } };
  private static readonly Dictionary<double,double> routThresholds = new() { { int.MinValue,50 },{ -4,50 },{ -3.5,60 },{ -3,68 },{ -2.5,75 },{ -2,80 },{ -1.5,84 },{ -1,87.5 },{ -0.5,89.5 },{ 0,91 },{ 2,96 },{ 4,98 },{ 5,98.5 },{ int.MaxValue,98.5 } };
  private static readonly Dictionary<AttackJudge,Dictionary<double,double>> thresholdMap = Enum.GetValues<AttackJudge>().Zip([crushingThresholds,winThresholds,defeatThresholds,routThresholds]).ToDictionary();
  internal static readonly int thresholdMax = 100;
  private static ThresholdInfo GetThresholdInfo(Dictionary<double,double> v,double attackRankSuperiority) => new(v.LastOrDefault(v => v.Key <= attackRankSuperiority),v.FirstOrDefault(v => v.Key > attackRankSuperiority));
  private static double GetThresholdFromInfo(ThresholdInfo t,double attackRankSuperiority) => double.Lerp(t.Lower.Value,t.Upper.Value,((double)attackRankSuperiority - t.Lower.Key) / (t.Upper.Key - t.Lower.Key));
  private static double GetThreshold(Dictionary<double,double> thresholdMap,double attackRankSuperiority) => GetThresholdInfo(thresholdMap,attackRankSuperiority).MyPipe(v => GetThresholdFromInfo(v,attackRankSuperiority));
  internal static double GetThreshold(AttackJudge? attackJudge,double attackRankSuperiority) => attackJudge?.MyPipe(thresholdMap.GetValueOrDefault)?.MyPipe(v => GetThreshold(v,attackRankSuperiority)) ?? thresholdMax;
  internal static AttackJudge JudgeAttack(decimal attackRank,decimal defenseRank,bool defenseSideFocusDefense) => MyRandom.GenerateDouble(0,thresholdMax).MyPipe(rand => Enum.GetValues<AttackJudge>().LastOrDefault(v => rand > GetThreshold(v,(double)(attackRank - (defenseRank + (defenseSideFocusDefense ? 1 : 0))))));
  internal static class Area {
    internal static AttackResult Attack(GameState game,ECountry? defenseSide,EArea target,Army attack,bool defenseSideFocusDefense) => DefenseArmy(game,defenseSide,target).MyPipe(defense => AttackJudge(attack,defense,defenseSideFocusDefense).MyPipe(judge => new AttackResult(defense,judge,Text.AreaInvadeText(attack,defense,target,judge,defenseSideFocusDefense))));
    private static Army DefenseArmy(GameState game,ECountry? defense,EArea target) => Commander.AreaCommander(game,defense,target).MyPipe(commander => new Army(defense,commander,Commander.CommanderRank(game,commander,ERole.Defense)));
    private static AttackJudge AttackJudge(Army attack,Army defense,bool defenseSideFocusDefense) => JudgeAttack(attack.Rank,defense.Rank,defenseSideFocusDefense);
  }
  internal static class Country {
    internal static AttackResult? Attack(GameState game,ECountry? defenseSide,EArea target,Army attack,bool defenseSideFocusDefense) => DefenseArmy(game,defenseSide).MyPipe(defense => AttackJudge(attack,defense,Distance(game,defenseSide,target),defenseSideFocusDefense)?.MyPipe(judge => new AttackResult(defense,judge,Text.CountryInvadeText(attack,defense,target,judge,defenseSideFocusDefense))));
    private static Army DefenseArmy(GameState game,ECountry? defense) => Commander.GetDefenseCommander(game,defense).MyPipe(commander => new Army(defense,commander,Commander.CommanderRank(game,commander,ERole.Defense)));
    private static int? Distance(GameState game,ECountry? defense,EArea target) => defense?.MyPipe(defense => Code.Country.GetCapitalArea(game,defense)?.MyPipe(capitalArea => Code.Area.GetAreaDistance(game,defense,capitalArea,target)));
    private static AttackJudge? AttackJudge(Army attack,Army defense,int? dist,bool defenseSideFocusDefense) => (dist switch { 0 => 1, _ => (decimal?)null })?.MyPipe(coefficient => JudgeAttack(attack.Rank,defense.Rank * coefficient,defenseSideFocusDefense));
  }
}