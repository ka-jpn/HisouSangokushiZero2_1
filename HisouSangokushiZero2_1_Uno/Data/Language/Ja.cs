using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.Data.Language.Text;
using Commander = HisouSangokushiZero2_1_Uno.Code.DefType.Commander;
using Country = HisouSangokushiZero2_1_Uno.Code.Country;
namespace HisouSangokushiZero2_1_Uno.Data.Language;

internal class Ja:ILangText {
  string ILangText.ProgressSaveText() => "セーブ中..";
  string ILangText.CompleteSaveText() => "セーブ完了";
  string ILangText.ProgressLoadText() => "ロード中..";
  string ILangText.CompleteLoadText(ReadGame read) => read.MaybeGame is { } ? "ロード完了" : read.ReadState == ReadState.Read ? "ロード失敗：ファイルが破損しています" : "ロード失敗：ファイルが見つかりません";
  string ILangText.ProgressInitText() => "ゲームを初期化しています..";
  string ILangText.CompleteInitText() => "ゲームを初期化しました";
  string ILangText.StartPlayText(ECountry? country) => $"{country}でプレイ開始";
  string ILangText.StartPlayCountryPersonsText(GameState game) => game.PlayCountry?.MyPipe(country => string.Join(',', Enum.GetValues<ERole>().SelectMany(role => Person.GetInitPersonMap(game, country, role)).Select(v => v.Key.Value))) ?? "初期人物なし";
  string[] ILangText.WinEndText(GameState game) => [$"達成勝利\n最終陣営所属人物 {string.Join(',', game.PlayCountry?.MyPipe(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game, country, role))).Select(v => v.Key.Value) ?? [])}"];
  string[] ILangText.OtherWinEndText(GameState game) => [$"{string.Join("と", game.WinCountrys)}の勝利", $"他陣営達成勝利による敗北\n最終領土数 {game.PlayCountry?.MyPipe(country => Country.GetAreaNum(game, country))}"];
  string[] ILangText.PerishEndText(GameState game) => [$"滅亡敗北\n{game.PlayCountry?.MyPipe(game.CountryMap.GetValueOrDefault)?.PerishFrom}に滅ぼされた"];
  string[] ILangText.TurnLimitOverEndText(GameState game) => [$"存続勝利\n最終領土数 {game.PlayCountry?.MyPipe(country => Country.GetAreaNum(game, country))}"];
  string ILangText.CountryText(ECountry? country) => country?.ToString() ?? "自治";
  string ILangText.CommanderToText(Commander commander) => commander.MainPerson == null && commander.SubPerson == null ? "無名武官" : $"{commander.MainPerson?.Value ?? "無名武官"}と{commander.SubPerson?.Value ?? "無名武官"}";
  string ILangText.CalcCountryInvadeText(InvadeText textInfo) => $"{textInfo.Attack.Country}の{textInfo.Attack.Commander}(ランク{textInfo.Attack.Rank})が{textInfo.Field}にて{(textInfo.DefenseSideFocusDefense ? "防衛専念の" : null)}{textInfo.Defense.Country}の中央軍の{textInfo.Defense.Commander}(ランク{textInfo.Defense.Rank})に攻撃して{GetAttackJudgeText(textInfo.Judge)}";
  string ILangText.CalcAreaInvadeText(InvadeText textInfo) => $"{textInfo.Attack.Country}の{textInfo.Attack.Commander}(ランク{textInfo.Attack.Rank})が{(textInfo.DefenseSideFocusDefense ? "防衛専念の" : null)}{textInfo.Defense.Country}領の{textInfo.Defense.Commander}(ランク{textInfo.Defense.Rank})が守備する{textInfo.Field}に侵攻して{GetAttackJudgeText(textInfo.Judge)}";
  string ILangText.GetAttackJudgeText(AttackJudge attackJudge) => attackJudge switch { AttackJudge.Crush => "大勝", AttackJudge.Win => "辛勝", AttackJudge.Lose => "惜敗", AttackJudge.Rout => "大敗" };
  string ILangText.BattleDeathCommanderPersonText(ERole role, List<PersonId> deathPersons, ECountry? battleCountry) => $"{(role == ERole.Attack ? $"{CountryText(battleCountry)}領に侵攻" : $"{CountryText(battleCountry)}軍の侵攻を守備")}した{string.Join("と", deathPersons.Select(v => v.Value))}が退却できず戦死";
  string ILangText.RoleToText(ERole? role) => role switch { ERole.Central => "中枢", ERole.Affair => "内政", ERole.Defense => "防衛", ERole.Attack => "攻撃", _ => string.Empty };
  string? ILangText.EndPhaseButtonText(Phase phase) => phase switch { Phase.Starting => null, Phase.Planning => "軍議終了", Phase.Execution => "確認", Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => "ゲームログを表示" };
  string? ILangText.AppearPersonText(ECountry country, List<PersonId> appearPersons) => appearPersons.Count != 0 ? $"{country}に{string.Join("と", appearPersons.Select(v => v.Value))}が登場" : null;
  string? ILangText.FindPersonText(ECountry country, List<PersonId> findPersons) => findPersons.Count != 0 ? $"{country}が{string.Join("と", findPersons.Select(v => v.Value))}を登用" : null;
  string? ILangText.NaturalDeathPersonText(ECountry country, List<PersonId> deathPersons) => deathPersons.Count != 0 ? $"{country}の{string.Join("と", deathPersons.Select(v => v.Value))}が死去" : null;
  string? ILangText.WarDeathBureaucracyPersonText(EArea area, List<PersonId> deathPersons) => deathPersons.Count != 0 ? $"{area}にいた{string.Join("と", deathPersons.Select(v => v.Value))}が戦死" : null;
  string ILangText.DefenseText(ECountry country, bool isTryAttack) => $"{(isTryAttack ? "(資金不足で攻撃中止)" : null)}{country}は防衛に専念";
  string ILangText.SleepText(ECountry country, int remainSleepTurn) => $"{country}は国力回復中(残り{remainSleepTurn}ターン)";
  string ILangText.ChangeHasCountryText(ECountry attackSide, ECountry? defenseSide, EArea targetArea) => $"{CountryText(defenseSide)}領の{targetArea}が{CountryText(attackSide)}領に";
  string ILangText.FallCapitalText(ECountry? country) => $"{CountryText(country)}の首都が陥落";
  string ILangText.PerishCountryText(ECountry? country) => $"{CountryText(country)}が滅亡";
  string ILangText.AppendUpdateMaxAreaNumLog(int? updatedMaxAreaNum, ECountry? defenseSide, EArea targetArea) => $"{CountryText(defenseSide)}領の{targetArea}を攻略して最大領土数を{updatedMaxAreaNum}に更新";
  string ILangText.TurnHeadLogText(GameState game) => $"------------{GetCalendarText(game) ?? ""}------------";
  string? ILangText.FallPlayerCapitalText(EArea? capital) => capital != null ? $"首都の{capital}が陥落" : null;
  string? ILangText.FallPlayerCapitalDeathPersonText(List<PersonId> deathPersons) => !deathPersons.MyIsEmpty() ? $"首都の陥落により{string.Join("と", deathPersons.Select(v => v.Value))}が死亡" : null;
  string? ILangText.BattleDeathPersonCharacterRemarkText(ERole role, List<PersonId> deathPersons, ECountry? enemy) => !deathPersons.MyIsEmpty() ? $"{(role == ERole.Attack ? $"{CountryText(enemy)}領に侵攻" : $"{CountryText(enemy)}軍の侵攻を守備")}した{string.Join("と", deathPersons.Select(v => v.Value))}が戦死しました" : null;
  string? ILangText.FallPlayerCapitalCharacterRemarkText(EArea? capital) => capital != null ? $"我々の首都の{capital}が陥落しました" : null;
  string? ILangText.FallPlayerCapitalDeathPersonCharacterRemarkText(List<PersonId> deathPersons) => !deathPersons.MyIsEmpty() ? $"首都の陥落により{string.Join("と", deathPersons.Select(v => v.Value))}が死亡しました" : null;
  string? ILangText.ChangeCapitalCharacterRemarkText(EArea prevCapital, EArea newCapial) => prevCapital != newCapial ? $"首都が{newCapial}に移りました" : null;
  string? ILangText.WarDeathBureaucracyPersonCharacterRemarkText(EArea area, List<PersonId> deathPersons) => deathPersons.Count != 0 ? $"{area}にいた{string.Join("と", deathPersons.Select(v => v.Value))}が戦死しました" : null;
  string[] ILangText.StartPlanningCharacterRemarkTexts(GameState game) {
    return game.PlayTurn == 0 ? [
      "ゲームの説明を聞きますか？\n勝利条件・敗北条件と\n戦闘のルールの説明があります",
      $"勝利条件は2種類あります\n陣営毎の勝利条件を一番乗り達成で達成勝利\nまたは{game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.EndYear}年春まで存続で存続勝利\nとなります、どちらも勝利ではありますが\n区別があることに留意してください",
      "敗北条件は2種類で\n陣営毎の勝利条件を他陣営に先に達成される\nおよび全領土失陥となります\nまた、我々の命運も共にありますので\nどうかできるだけ多くの生存者を\n後まで導いて頂きますよう",
      "戦闘のうち攻撃は首都から行軍可能な範囲ででき\n中央所属のうち攻撃の筆頭と次席のポスト\nの者が指揮し、能力差が勝敗率に影響します\n侵攻先が防衛に専念していると分が悪くなります\n無暗に攻撃することばかりが手段ではありません",
      "戦闘のうち防衛は地域毎に行われ\n侵攻された所を担当している者が防衛にあたり\n首都が侵攻され首都地域の防衛が破られると\n中央所属の防衛の筆頭と次席のポストの者が\n首都中央で応戦します\n我らが防衛に専念していると分が良くなります",
      "もし首都が落ちると仕えている我々に死者が出\n運営に支障をきたすことも考えられます\n首都の防衛は一層のご考慮が必要になる\nことでしょう",
      "今回は以上です、ご武運を！"
    ] : game.PlayTurn == 1 ? [
      "次の説明を聞きますか？\n内政値と収支の説明があります",
      "内政値は地域ごとにあり\n中央所属の内政の筆頭と次席のポストと\n地域の内政担当に配置した人物の能力により\n上昇させることができます\nただし、支配領域が広いほど\n上昇量にデバフがかかります",
      "内政担当を配置し内政値を上昇させるには\n少々支出がありますが\nその地域で戦闘があるなど\n内政値は減少する余地があるので\n適宜上げてゆかないと回らなくなります",
      "収支は収入と支出があり、うち収入は\n地域ごとの内政値と\n中央所属の内政の筆頭と次席のポストの\n者の能力が影響します\nただし支配領域が広いと\n収入効率にデバフがかかります",
      "収支のうち支出は\n支配領域が広いとより掛かる民政費\n地域の内政担当の内政のために掛かる内政費\n仕える人物への俸禄へ掛かる俸禄費\nがあります",
      "細かい計算式を把握する必要は\n基本的にありませんが\n領土を増やしたからといって\n収支に好影響なことばかりではない\nということはご理解ください",
      "今回は以上です、ご武運を"
    ] : game.PlayTurn == 2 ? [
      "次の説明を聞きますか？\n人物の加入・脱落と\n寿命の説明があります",
      "有名人物の加入は陣営ごとに決まっており\n時期が来れば人物が自動で加入します\n無名人物の加入は不確定要素で\n中央所属のうち中枢の筆頭と次席のポスト\nの者の能力により登用が行われます",
      "人物の脱落は不確定要素で\n戦死と寿命経過の要因があり\n戦死は首都陥落するか、もしくは\n防衛失敗と攻撃失敗のうち\n大敗すると可能性があります",
      "寿命は人物ごとにありますが\n必ずその時に死去ではなく\n寿命を過ぎるごとに確率が高くなり\n生存していればその時点での確率による\n死去判定があります",
      "詳しいことは\n情報のルール詳細に\n記してあります\nご武運を・・"
    ] : game.PlayTurn == 3 ? [
      $"今まで説明に来ていた{GetRemarkPersonName(game.PlayCountry,true)}が亡くなりました"
    ] : [];
  }
  string ILangText.StartExecutionFailAttackCharacterRemarkText(EArea targetArea) => $"{targetArea}への侵攻は\n資金不足のため中止されました";
  string ILangText.GetRemarkPersonName(ECountry? country, bool isAliveCharacter) => isAliveCharacter ? country switch { ECountry.魏 => "杜畿", ECountry.呉 => "韓当", ECountry.蜀漢 => "簡雍", _ => "武官" } : "文官";
  string ILangText.NoBasicWinCondText() => "選べません(勝利条件なし)";
  string ILangText.AreaPostDefenseText() => "防";
  string ILangText.AreaPostAffairText() => "政";
  string ILangText.CountryFocusDefenseText() => "(防)";
  string ILangText.CountrySleepText(GameState game,ECountry country) => $"休み {Country.GetSleepTurn(game,country)}";
  string ILangText.AreaText(EArea area, ECountry? country) => $"{area} {CountryText(country)}領";
  string ILangText.PlayerCountryPostText(PostKind postKind) => postKind.MaybeHead switch { PostHead.Main => "筆頭", PostHead.Sub => "次席", _ => $"{postKind.MaybePostNo + 1}" };
  string? ILangText.GetCalendarText(GameState game) => $"{Turn.GetYear(game)}年 {Turn.GetCalendarInYear(game) switch { YearItems.Spring => "春", YearItems.Summer => "夏", YearItems.Autumn => "秋", YearItems.Winter => "冬" }}";
  string ILangText.WinCondCaptionText(GameState game) => $"勝利条件({GetCalendarText(game)})";
  string ILangText.ScenarioCaptionText() => "シナリオ";
  string ILangText.StartYearText(ScenarioData scenarioData) => $"{scenarioData.StartYear}年開始";
  string ILangText.EndYearText(ScenarioData scenarioData) => $"{scenarioData.EndYear}年終了";
  string ILangText.ClickMapAreaText() => "マップ上のエリアをクリックしてプレイ勢力を選択";
  string ILangText.CountryParamCaptionText(ECountry? country) => $"{CountryText(country)}陣営";
  string ILangText.CountryCapitalAreaParamText(GameState game, ECountry? country) => $"首都:{Country.GetCapitalArea(game, country)?.ToString() ?? "(なし)"}";
  string ILangText.CountryFundParamText(GameState game, ECountry? country) => $"資金:{Country.GetFund(game, country):0.####}";
  string ILangText.CountryAreaNumParamText(GameState game, ECountry? country) => $"領土数:{Country.GetAreaNum(game, country)}";
  string ILangText.CountryAffairDifficultParamText(GameState game, ECountry? country) => $"内政難度:{Country.GetAffairDifficult(game, country):0.####}";
  string ILangText.CountryAffairPowerParamText(GameState game, ECountry? country) => $"内政力:{Country.GetAffairPower(game, country):0.####}";
  string ILangText.CountryTotalAffairParamText(GameState game, ECountry? country) => $"総内政値:{Country.GetTotalAffair(game, country):0.####}";
  string ILangText.CountryOutFundParamText(GameState game, ECountry? country) => $"支出:{Country.GetOutFund(game, country):0.####}";
  string ILangText.CountryInFundParamText(GameState game, ECountry? country) => $"収入:{Country.GetInFund(game, country):0.####}";
  string ILangText.PlayCountryArmyTargetAreaParamText(GameState game) => $"侵攻:{Country.GetArmyTargetArea(game, game.PlayCountry)?.ToString() ?? "なし"}";
  string ILangText.GameEndText() => "ゲーム終了";
  string ILangText.GameResultText(GameState game) => $"結果:{game.Phase switch { Phase.PerishEnd => "滅亡敗北", Phase.TurnLimitOverEnd => "存続勝利", Phase.WinEnd => "条件勝利", Phase.OtherWinEnd => "他陣営条件勝利敗北", _ => string.Empty }}";
  string ILangText.GameEndLogCaptionText() => "ゲームログ";
  string ILangText.PostGameEndLogText() => "ゲームコメントを投稿する";
  string ILangText.AutoPutPersonButtonText() => "オート配置";
  string ILangText.CountryInitInfoCaptionText() => "[陣営初期情報]";
  string ILangText.CountryWinCondCaptionText() => "[勝利条件]";
  string ILangText.CountryInitPersonCaptionText() => "[初期人物]";
  string? ILangText.ScenarioCaptionText(ScenarioId? scenarioId) => scenarioId?.MyPipe(ScenarioBase.GetScenarioData)?.MyPipe(v=>$"シナリオ:{scenarioId.Value}({v.StartYear}年開始 {v.EndYear}年終了)");
  string ILangText.CountryWinCondHeadText() => "以下の条件を全て満たす";
  string ILangText.CountryNoExistStartingPersonText() => "(人物はいません)";
  string ILangText.SelectCountryButtonText(ECountry? country) => country is ECountry.漢 or null ? $"({CountryText(country)}陣営は選べません)" : "プレイする";
  string ILangText.PersonInfoText(GameState game, PersonId personId) => $"{personId.Value}  {RoleToText(Person.GetPersonRole(game, personId))} ランク{Person.GetPersonRank(game, personId)} 齢{Turn.GetYear(game) - Person.GetPersonBirthYear(game, personId)}";
}