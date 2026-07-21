using System;
using System.Collections.Generic;
using System.Linq;
using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class HegemonyPoint:UserControl {
  private const double baseContentHeight = 25;
  internal static Dictionary<ECountry,double> hegemonyPoints = [];
  internal static double? totalHegemonyPoint = null;
  private static Dictionary<ECountry,StackPanel> hegemonyPointPanels = [];
  private static readonly Func<int,double> calcAreaNumPoint = (areaNum) => areaNum * 5;
  private static readonly Func<decimal,double> calcAffairPoint = (affair) => Math.Pow((double)affair,0.75);
  private static readonly double hasHanCapitalBonus = 100;
  private static readonly double fillDreamBonus = 500;
  internal HegemonyPoint() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(HegemonyPoint page) {
      page.Content.Height = baseContentHeight;
    }
  }
  internal static void Show(HegemonyPoint page,GameState game,double scaleFactorX){
    hegemonyPoints = game.CountryMap.Where(v => v.Key != ECountry.漢).ToDictionary(v => v.Key,v => AddHegemonyPoint(game,v.Key)+calcAreaNumPoint(Country.GetAreaNum(game,v.Key))+calcAffairPoint(Country.GetTotalAffair(game,v.Key))).Where(v => v.Value > 0).OrderByDescending(v => v.Value).ToDictionary();
    totalHegemonyPoint = hegemonyPoints.Values.Sum();
    hegemonyPointPanels = hegemonyPoints.ToDictionary(v => v.Key, v => new StackPanel{ Height = baseContentHeight, Background = Country.GetCountryColor(game,v.Key).ToBrush() }.MySetChildren([new TextBlock{ Text = Text.CountryText(v.Key) }]));
    hegemonyPointPanels.ToList().ForEach(v => ToolTipService.SetToolTip(v.Value,$"{Text.CountryText(v.Key)}\n制覇ポイント{hegemonyPoints.GetValueOrDefault(v.Key):0.##}\n{HegemonyPointSectionToolTip(game,v.Key)}制覇率{hegemonyPoints.GetValueOrDefault(v.Key)/totalHegemonyPoint*100:0.##}%"));
    page.Content.MySetChildren([.. hegemonyPointPanels.Values]);
    CalcEachHegemonyPointPanelSizePos(page,scaleFactorX);
    static double AddHegemonyPoint(GameState game, ECountry side) => (!Country.IsPerish(game,ECountry.漢) && Country.HasAreas(game, side, [EArea.洛陽]) ? hasHanCapitalBonus : 0) + (game.FillDreams.GetValueOrDefault(side) is FillDream.Passed ? fillDreamBonus : 0);
    static string HegemonyPointSectionToolTip(GameState game, ECountry side){
      string areaNumPointText = Country.GetAreaNum(game,side).MyPipe(v => $"({v}エリア所持:{calcAreaNumPoint(v)}ポイント)\n");
      string affairPointText = Country.GetTotalAffair(game,side).MyPipe(v => $"({v:0.####}内政値:{calcAffairPoint(v):0.##}ポイント)\n");
      string hasHanCapitalBonusText = game.CountryMap.ContainsKey(ECountry.漢) && !Country.IsPerish(game, ECountry.漢) && Country.HasAreas(game, side, [EArea.洛陽]) ? $"(漢生存＆洛陽保持:{hasHanCapitalBonus}ポイント)\n" : "";
      string fillDreamBonusText = game.FillDreams.GetValueOrDefault(side) is FillDream.Passed ? $"(悲願達成中:{fillDreamBonus}ポイント)\n" : "";
      return areaNumPointText + affairPointText + hasHanCapitalBonusText + fillDreamBonusText;
    }
  }
  internal static void ResizeElem(HegemonyPoint page,double scaleFactorX,double scaleFactorY){
    page.Content.RenderTransform = new ScaleTransform() { ScaleX = scaleFactorX,ScaleY = scaleFactorY };
    page.Content.Margin = new(0, 0, page.RenderSize.Width * (1 - 1 / scaleFactorX), baseContentHeight * (scaleFactorY - 1));
    page.Content.Clip = new(){ Rect = new(0,0,page.RenderSize.Width / scaleFactorX,baseContentHeight)};
    CalcEachHegemonyPointPanelSizePos(page,scaleFactorX);
  }
  private static void CalcEachHegemonyPointPanelSizePos(HegemonyPoint page,double scaleFactorX){
    Dictionary<ECountry, double> panelSizes = hegemonyPointPanels.ToDictionary(elem => elem.Key,elem => (hegemonyPoints.GetValueOrDefault(elem.Key) / totalHegemonyPoint ?? 0) * page.RenderSize.Width / scaleFactorX);
    hegemonyPointPanels.ToList().ForEachWithIndex((elem,index) => { elem.Value.Width = Math.Ceiling(panelSizes.GetValueOrDefault(elem.Key)); Canvas.SetLeft(elem.Value,panelSizes.Values.Take(index).Sum()); });
  }
}