using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Explain:UserControl {
  private static readonly double attackJudgePointSize = 10;
  private static Dictionary<AttackJudge,Dictionary<double,Rectangle>> judgeThresholdPointRects = [];
  private static Dictionary<double,Rectangle> judgeMaxPointRects = [];
  private static Dictionary<AttackJudge,Dictionary<double,TextBlock>> judgeThresholdPointTexts = [];
  private static Dictionary<double,TextBlock> rankDiffTexts = [];
  private static UIElement? parent = null;
  internal Explain() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Explain page) {
      Color crushThresholdFill = new(255, 240, 190, 190),winThresholdFill = new(255,235,235,160),loseThresholdFill = new(255,175,240,175),routThresholdFill = new(255, 190, 190, 240);
      Color crushThresholdEdge = new(255, 240, 135, 135),winThresholdEdge = new(255, 230, 230, 65),loseThresholdEdge = new(255, 105, 225, 105),routThresholdEdge = new(255, 135, 135, 240);
      Color maxThresholdEdge = new(255, 165, 165, 165);
      Dictionary<AttackJudge,Dictionary<double,Point>> judgePointMap = Enum.GetValues<AttackJudge>().ToDictionary(v => v,v => GetJudgeShapeCrds(v));
      Dictionary<double,Point> judgeMaxPoints = GetJudgeShapeCrds(null);
      AttachEvent(page);
      SetAttackJudgeExplain();
      void AttachEvent(Explain page) {
        page.SizeChanged += (_,_) => parent?.MyApply(ResizeElem);
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          Size judgeViewSize = new(Math.Min(AttackJudgeExplainPanel.MaxWidth,contentWidth - BattleExplainCaption.DesiredSize.Width),AttackJudgeExplainPanel.Height);
          ContentPanel.Width = contentWidth;
          ContentPanel.Children.ToList().ForEach(child => child.Measure(new(ContentPanel.Width, double.PositiveInfinity)));
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
          ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
          AttackJudgeExplainPanel.Width = judgeViewSize.Width;
          judgePointMap.GetValueOrDefault(AttackJudge.Crush)?.Values.Concat(judgePointMap.GetValueOrDefault(AttackJudge.Win)?.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApply(crds => AttackCrushShape.Points = [.. crds.Select(v => new Windows.Foundation.Point(v.X,v.Y))]);
          judgePointMap.GetValueOrDefault(AttackJudge.Win)?.Values.Concat(judgePointMap.GetValueOrDefault(AttackJudge.Lose)?.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApply(crds => AttackWinShape.Points = [.. crds.Select(v => new Windows.Foundation.Point(v.X,v.Y))]);
          judgePointMap.GetValueOrDefault(AttackJudge.Lose)?.Values.Concat(judgePointMap.GetValueOrDefault(AttackJudge.Rout)?.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApply(crds => AttackLoseShape.Points = [.. crds.Select(v => new Windows.Foundation.Point(v.X,v.Y))]);
          judgePointMap.GetValueOrDefault(AttackJudge.Rout)?.Values.Concat(judgeMaxPoints.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApply(crds => AttackRoutShape.Points = [.. crds.Select(v=>new Windows.Foundation.Point(v.X,v.Y))]);
          judgeThresholdPointRects.ToList().ForEach(judge => judge.Value.ToList().ForEach(elemMap => UpdateJudgePointCrds(judgeViewSize,elemMap.Value,judgePointMap.GetValueOrDefault(judge.Key)?.GetValueOrDefault(elemMap.Key) ?? new(0,0))));
          judgeMaxPointRects.ToList().ForEach(elemMap => UpdateJudgePointCrds(judgeViewSize,elemMap.Value,judgeMaxPoints.GetValueOrDefault(elemMap.Key) ?? new(0,0)));
          judgeThresholdPointTexts.ToList().ForEach(judge => judge.Value.ToList().ForEach(elemMap => UpdateJudgePointCrds(judgeViewSize,elemMap.Value,judgePointMap.GetValueOrDefault(judge.Key)?.GetValueOrDefault(elemMap.Key) ?? new(0,0))));
          rankDiffTexts.ToList().ForEach(rankDiff => UpdateJudgePointCrds(judgeViewSize,rankDiff.Value,judgePointMap.GetValueOrDefault(AttackJudge.Crush)?.GetValueOrDefault(rankDiff.Key) ?? new(0,0)));
          static void UpdateJudgePointCrds(Size size,UIElement elem,Point crd) => CookPoint(size,crd).MyApply(cookedCrd => {
            Canvas.SetLeft(elem,cookedCrd.X - elem.DesiredSize.Width / 2); Canvas.SetTop(elem,cookedCrd.Y - elem.DesiredSize.Height / 2);
          });
          static Point CookPoint(Size size,Point point) => point with { X = point.X * size.Width,Y = point.Y * size.Height };
        }
      }
      void SetAttackJudgeExplain() {
        AttackCrushFillColor.Background = ThresholdFillColor(AttackJudge.Crush).ToBrush();
        AttackCrushEdgeColor.Background = ThresholdEdgeColor(AttackJudge.Crush).ToBrush();
        AttackWinFillColor.Background = ThresholdFillColor(AttackJudge.Win).ToBrush();
        AttackWinEdgeColor.Background = ThresholdEdgeColor(AttackJudge.Win).ToBrush();
        AttackLoseFillColor.Background = ThresholdFillColor(AttackJudge.Lose).ToBrush();
        AttackLoseEdgeColor.Background = ThresholdEdgeColor(AttackJudge.Lose).ToBrush();
        AttackRoutFillColor.Background = ThresholdFillColor(AttackJudge.Rout).ToBrush();
        AttackRoutEdgeColor.Background = ThresholdEdgeColor(AttackJudge.Rout).ToBrush();
        AttackCrushShape.MyApply(v => v.Fill = ThresholdFillColor(AttackJudge.Crush).ToBrush());
        AttackWinShape.MyApply(v => v.Fill = ThresholdFillColor(AttackJudge.Win).ToBrush());
        AttackLoseShape.MyApply(v => v.Fill = ThresholdFillColor(AttackJudge.Lose).ToBrush());
        AttackRoutShape.MyApply(v => v.Fill = ThresholdFillColor(AttackJudge.Rout).ToBrush());
        judgeThresholdPointRects = Enum.GetValues<AttackJudge>().ToDictionary(v => v,v => CreateRects(v));
        judgeMaxPointRects = CreateRects(null);
        judgeThresholdPointTexts = Enum.GetValues<AttackJudge>().Skip(1).ToDictionary(v => v,v => CreateTexts(v));
        rankDiffTexts = CreateRankDiffTexts();
        AttackJudgeThresholdPointPanel.MySetChildren([.. judgeMaxPointRects.Values,.. judgeThresholdPointRects.Values.SelectMany(v => v.Values),.. judgeThresholdPointTexts.Values.SelectMany(v => v.Values)]);
        AttackRankDiffTextPanel.MySetChildren([.. rankDiffTexts.Values]);
        Color ThresholdEdgeColor(AttackJudge? attackJudge) => attackJudge switch { AttackJudge.Crush => crushThresholdEdge, AttackJudge.Win => winThresholdEdge, AttackJudge.Lose => loseThresholdEdge, AttackJudge.Rout => routThresholdEdge, _ => maxThresholdEdge };
        Color ThresholdFillColor(AttackJudge attackJudge) => attackJudge switch { AttackJudge.Crush => crushThresholdFill, AttackJudge.Win => winThresholdFill, AttackJudge.Lose => loseThresholdFill, AttackJudge.Rout => routThresholdFill };
        Dictionary<double,TextBlock> CreateTexts(AttackJudge? attackJudge) => (attackJudge?.MyPipe(judgePointMap.GetValueOrDefault) ?? judgeMaxPoints).Where(v => v.Key is -5 or -4 or -3 or -2 or -1 or 0 or 1 or 2 or 3 or 4 or 5).ToDictionary(v => v.Key,v => new TextBlock { Text = $"{v.Value.Y * 100:0.#}" });
        Dictionary<double,Rectangle> CreateRects(AttackJudge? attackJudge) => (attackJudge?.MyPipe(judgePointMap.GetValueOrDefault) ?? judgeMaxPoints).ToDictionary(v => v.Key,_ => new Rectangle { Width = attackJudgePointSize,Height = attackJudgePointSize,Fill = ThresholdEdgeColor(attackJudge).ToBrush() });
        Dictionary<double,TextBlock> CreateRankDiffTexts() => judgeMaxPoints.Where(v => v.Key is -5 or -4 or -3 or -2 or -1 or 0 or 1 or 2 or 3 or 4 or 5).ToDictionary(v => v.Key,v => new TextBlock { Text = $"{v.Key:0.#}" });
      }
      static Dictionary<double,Point> GetJudgeShapeCrds(AttackJudge? attackJudge) {
        return Enumerable.Range(-11,23).Select(v => v * 0.5).ToDictionary(v => v,v => GetJudgePoint(attackJudge,v));
        static Point GetJudgePoint(AttackJudge? attackJudge,double rankDiff) => new(rankDiff * 0.09 + 0.5,Battle.GetThreshold(attackJudge,rankDiff)/Battle.thresholdMax);
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}