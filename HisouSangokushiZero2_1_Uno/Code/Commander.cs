using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using CommanderType = HisouSangokushiZero2_1_Uno.Code.DefType.Commander;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Commander {
		private static Dictionary<PostHead,PersonId?> GetPersonMap(GameState game,ECountry country,ERole role) => Enum.GetValues<PostHead>().ToDictionary(v => v,v => Person.GetPostPerson(game,country,new(role,new(v)))?.Key);
		private static Dictionary<PostHead,PersonId?> GetAreaDefensePersonMap(GameState game,ECountry defense,EArea battleArea) => new([new(PostHead.Main,Person.GetPostPerson(game,defense,new(ERole.Defense,new(battleArea)))?.Key)]);
		private static CommanderType PersonMapToCommander(Dictionary<PostHead,PersonId?> targetPersonMap) => new(targetPersonMap.GetValueOrDefault(PostHead.Main),targetPersonMap.GetValueOrDefault(PostHead.Sub));
		internal static CommanderType GetCommander(GameState game,ECountry? country,ERole role) => (country?.MyPipe(v => GetPersonMap(game,v,role))?? []).MyPipe(PersonMapToCommander);
		internal static CommanderType GetCentralCommander(GameState game,ECountry? country) => GetCommander(game,country,ERole.Central);
		internal static CommanderType GetAffairsCommander(GameState game,ECountry? affairsCountry) => GetCommander(game,affairsCountry,ERole.Affair);
		internal static CommanderType GetDefenseCommander(GameState game,ECountry? defenseCountry) => GetCommander(game,defenseCountry,ERole.Defense);
		internal static CommanderType GetAttackCommander(GameState game,ECountry? attackCountry) => GetCommander(game,attackCountry,ERole.Attack);
		internal static CommanderType AreaCommander(GameState game,ECountry? defenseCountry,EArea defenseArea) => (defenseCountry?.MyPipe(v => GetAreaDefensePersonMap(game,v,defenseArea))?? []).MyPipe(PersonMapToCommander);
		internal static decimal CommanderRank(GameState game,CommanderType commander,ERole role) => (commander.MainPerson?.MyPipe(v => Person.CalcRoleRank(game,v,role))??0)+(commander.SubPerson?.MyPipe(v => Person.CalcRoleRank(game,v,role)/2m)??0);
	}
}
