using static HisouSangokushiZero2_1_Uno.Code.DefType;
using PostType = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using HisouSangokushiZero2_1_Uno.MyUtil;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Post {																																																																			
		private static List<PostType> GetCapitalRolePosts(ECountry country,ERole role) => country==ECountry.漢 ? [] : [new PostType(role,new(PostHead.Main)),new PostType(role,new(PostHead.Sub))];
		private static List<PostType> GetCapitalRoleStockPosts(ECountry country,ERole role) => country==ECountry.漢 ? [] : [.. Enumerable.Range(0,UIUtil.capitalPieceCellNum).Select(i => new PostType(role,new(i)))];
		private static List<PostType> GetAreaRolePosts(GameState game,ECountry country,ERole role) => [.. (role==ERole.Defense ? Area.CalcOrdDefenseAreas(game,country) : role==ERole.Affair ? Area.CalcOrdAffairAreas(game,country) : []).Select(area => new PostType(role,new(area)))];
		private static List<PostType> GetEmptyCapitalStockPosts(GameState game,ECountry country,ERole role) => [.. GetCapitalRoleStockPosts(country,role).Except(game.PersonMap.Where(v =>Person.GetPersonCountry(game,v.Key)==country).Select(v => v.Value.Post)).MyNonNull()];
		internal static Dictionary<PersonId,PostType> GetInitPost(GameState game,ECountry country,ERole role) => Person.GetInitPersonMap(game,country,role).ToDictionary(v => v.Key,v => new PostType(role,new()));
		internal static Dictionary<PersonId,PostType> GetInitAppearPost(GameState game,ECountry country,ERole role) => Person.GetAppearPersonMap(game,country,role).ToDictionary(v => v.Key,v => new PostType(role,new()));
		internal static Dictionary<PersonId,PostType> GetPutWaitPost(GameState game,ECountry country,ERole role) => Person.GetWaitPostPersonMap(game,country,role).Keys.Zip(GetEmptyCapitalStockPosts(game,country,role)).ToDictionary();
		internal static Dictionary<PersonId,PostType> GetAutoPutPost(GameState game,ECountry country,ERole role) => Area.GetConnectCapitalCountryAreas(game,country).MyPipe(areas=>Person.GetAlivePersonMap(game,country,role).Where(v=>v.Value.Post?.PostKind.MaybeArea.MyPipe(v=>v==null||areas.Contains(v.Value))??false).OrderByDescending(personInfo =>Person.CalcRoleRank(game,personInfo.Key,role)).Select(person => person.Key).Zip([.. GetCapitalRolePosts(country,role),.. GetAreaRolePosts(game,country,role),.. GetCapitalRoleStockPosts(country,role)]).ToDictionary());
	}
}
