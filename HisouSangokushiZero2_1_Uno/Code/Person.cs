using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.Code.UIUtil;
using PostType = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
namespace HisouSangokushiZero2_1_Uno.Code {
  internal static class Person {
    private static readonly int majorityAge = 16;
    private static PersonData? GetPersonData(GameState game,PersonId person) => game.PersonMap.GetValueOrDefault(person);
    private static bool IsInitPerson(GameState game,PersonId person) => GetAppearYear(game.NowScenario,person) < Turn.GetYear(game);
    private static bool IsAppearPerson(GameState game,PersonId person) => GetAppearYear(game.NowScenario,person) == Turn.GetYear(game) && Turn.GetInYear(game) == Enum.GetValues<YearItems>().Length / 2;
    private static bool IsAlivePerson(GameState game,PersonId person) => GetPersonData(game,person)?.Post != null;
    private static bool IsWaitPostPerson(GameState game,PersonId person) => GetPersonData(game,person)?.Post is PostType post && post.PostKind.MyPipe(v => v.MaybeArea == null && v.MaybeHead == null && v.MaybePostNo == null);
    private static bool IsNaturalDeathPerson(GameState game,PersonId person,int year,int inYear) => GetPersonData(game,person).MyPipe(personData=> year >= personData?.DeathYear && personData?.Post != null && personData.GameDeathTurn == null) && IsNaturalDeathJudge(game,person,year,inYear);
    private static bool IsNaturalDeathJudge(GameState game,PersonId person,int year,int inYear) => GetPersonData(game,person)?.MyPipe(personData => (personData.DeathYear + 10 - year).MyPipe(v => v <= 0 || MyRandom.RandomJudge(1 / (v * Enum.GetValues<YearItems>().Length - inYear) * (year - personData.BirthYear) / 60))) ?? false;
    private static Dictionary<PersonId,PersonData> GetRolePersonMap(GameState game,ECountry country,ERole role) => game.PersonMap.Where(v => GetPersonData(game,v.Key)?.MyPipe(personData => personData.Country == country && (v.Value.Post?.MyPipe(v => v.PostRole == role) ?? personData.Role == role)) ?? false).ToDictionary();
    internal static Dictionary<PersonId,PersonData> GetInitPersonMap(GameState game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsInitPerson(game,v.Key)).ToDictionary();
    internal static Dictionary<PersonId,PersonData> GetAppearPersonMap(GameState game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsAppearPerson(game,v.Key)).ToDictionary();
    internal static Dictionary<PersonId,PersonData> GetAlivePersonMap(GameState game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsAlivePerson(game,v.Key)).ToDictionary();
    internal static Dictionary<PersonId,PersonData> GetWaitPostPersonMap(GameState game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsWaitPostPerson(game,v.Key)).ToDictionary();
    internal static Dictionary<PersonId,PersonData> GetNaturalDeathPostPersonMap(GameState game,ECountry country,ERole role,int year,int inYear) => GetRolePersonMap(game,country,role).Where(v => IsNaturalDeathPerson(game,v.Key,year,inYear)).ToDictionary();
    internal static KeyValuePair<PersonId,PersonData>? GetPostPerson(GameState game,ECountry country,PostType post) => game.PersonMap.MyNullable().FirstOrDefault(v => GetPersonData(game,v?.Key??new(string.Empty))?.Country == country && v?.Value.Post == post);
    internal static int CalcRoleRank(GameState game,PersonId person,ERole? role) => GetPersonData(game,person)?.MyPipe(v => v.Rank + (v.Role == role ? 0 : -1)) ?? 0;
    internal static int GetAppearYear(ScenarioId? scenario,PersonId person) => scenario?.MyPipe(ScenarioBase.GetScenarioData)?.PersonMap.GetValueOrDefault(person)?.MyPipe(v => v.GameAppearYear ?? (v.BirthYear + majorityAge)) ?? 0;
    internal static int GetAppearYear(PersonData person) => person.GameAppearYear ?? (person.BirthYear + majorityAge);
    internal static Dictionary<PersonId,PersonData> FindPerson(GameState game,ECountry country) {
      return Country.SearchPersonRank(game,country) is int findPersonRank ? GeneratPerson(game,country,findPersonRank) : [];
      static Dictionary<PersonId,PersonData> GeneratPerson(GameState game,ECountry country,int personRank) {
        PersonId person = new($"{country}無名{game.CountryMap.GetValueOrDefault(country)?.AnonymousPersonNum + 1}");
        ERole personRole = Enum.GetValues<ERole>().MyPickAny();
        int birthYear = Turn.GetYear(game) - majorityAge - MyRandom.GenerateInt(0,40);
        int deathYear = Math.Max(birthYear + majorityAge + MyRandom.GenerateInt(0,60),Turn.GetYear(game) + 3);
        return new([new(person,new(personRole,personRank,birthYear,deathYear,country,null,null,new(personRole,new())))]);
      }
    }
    internal static int GetPersonRank(GameState game,PersonId person) => GetPersonData(game,person)?.Rank ?? 0;
    internal static int GetPersonBirthYear(GameState game,PersonId person) => GetPersonData(game,person)?.BirthYear ?? 0;
    internal static ECountry? GetPersonCountry(GameState game,PersonId person) => GetPersonData(game,person)?.Country;
    internal static ERole? GetPersonRole(GameState game,PersonId person) => GetPersonData(game,person)?.Role;
  }
}