using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Image = HisouSangokushiZero2_1_Uno.Code.Image;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Character:UserControl {
  private const double padding = 5;
  private static readonly double remarkFrameCornerRadius = 5;
  private static string nowPersonImageName = string.Empty;
  private static UIElement? parent = null;
  private static Action closeButtonAction = () => { };
  private static Action nextButtonAction = () => { };
  private static Action okButtonAction = () => { };
  private static Action noButtonAction = () => { };
  internal static readonly double personImageSize = 75;
  internal Character() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Character page) {
      CloseButton.Click += (_,_) => closeButtonAction(); 
      NextButton.Click += (_,_) => nextButtonAction();
      OkButton.Click += (_,_) => okButtonAction();
      NoButton.Click += (_,_) => noButtonAction();
      page.SizeChanged += (_,_) => parent?.MyApply(ResizeElem);
      RemarkText.Padding = new(remarkFrameCornerRadius,remarkFrameCornerRadius,remarkFrameCornerRadius * 2,remarkFrameCornerRadius);
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
  internal static void ShowRemark(Character page,GameState game) {
    string newPersonImageName = Text.GetRemarkPersonName(game.PlayCountry,game.PlayTurn < 3);
    string[] contents = GetCharacterRemark(game);
    if(!contents.MyIsEmpty()) {
      page.PersonName.Text = newPersonImageName;
      page.RemarkText.Text = contents.FirstOrDefault() ?? string.Empty;
      parent?.MyApply(page.ResizeElem);
      if(nowPersonImageName != newPersonImageName) {
        page.PersonImage.Source = Image.GetSvgImageSource($"Person/{newPersonImageName}",personImageSize*2,personImageSize*2);
        nowPersonImageName = newPersonImageName;
      }
      closeButtonAction = () => UIUtil.SetVisibility(page,false);
      page.NextButtonText.Text = contents.Skip(1).MyIsEmpty() ? "閉じる" : "次へ";
      nextButtonAction = () => {
        GameState newGameState = game.Phase switch {
          Phase.Planning => game with { StartPlanningCharacterRemark = [ ..contents.Skip(1)] },
          Phase.Execution => game with { StartExecutionCharacterRemark = [ ..contents.Skip(1)] },
          Phase.PerishEnd or Phase.OtherWinEnd or Phase.TurnLimitOverEnd or Phase.WinEnd => game with { GameEndCharacterRemark = [ ..contents.Skip(1)] },
          _ => throw new Exception()
        };
          GameData.game = newGameState;
          ShowRemark(page,newGameState);
      };

      UIUtil.SetVisibility(page.RemarkButtonPanel, true);
      UIUtil.SetVisibility(page.AskButtonPanel, false);
      UIUtil.SetVisibility(page,true);
    } else {
      UIUtil.SetVisibility(page,false);
    }
    static string[] GetCharacterRemark(GameState game) {
      return game.Phase switch { Phase.Planning => game.StartPlanningCharacterRemark, Phase.Execution => game.StartExecutionCharacterRemark, _ => game.GameEndCharacterRemark };
    }
  }
  internal static async Task<GameState> ShowAsk(Character page,GameState game,string askText,Func<bool,GameState> onAnswer) {
    TaskCompletionSource<GameState> tcs = new();
    string newPersonImageName = Text.GetRemarkPersonName(game.PlayCountry,game.PlayTurn < 3);
    page.PersonName.Text = newPersonImageName;
    page.RemarkText.Text = askText;
    page.OkButtonText.Text = "はい";
    page.NoButtonText.Text = "いいえ";
    okButtonAction = () => ClickAnswerButton(true);
    noButtonAction = () => ClickAnswerButton(false);
    parent?.MyApply(page.ResizeElem);
    closeButtonAction = () => ClickAnswerButton(false);
    UIUtil.SetVisibility(page.RemarkButtonPanel, false);
    UIUtil.SetVisibility(page.AskButtonPanel, true);
    UIUtil.SetVisibility(page,true);
    return await tcs.Task;
    void ClickAnswerButton(bool answer){
      UIUtil.SetVisibility(page,false);
      tcs.SetResult(onAnswer(answer));
    }
  }
  internal void ResizeElem(UIElement parent) {
    double contentScale = 1.2;
    double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize with { Width = parent.RenderSize.Width / contentScale }) * contentScale;
    double sideMargin = UIUtil.infoFrameWidth * scaleFactor;
    double contentMaxWidth = (parent.RenderSize.Width - sideMargin * 2) / scaleFactor;
    double textMaxWidth = contentMaxWidth - personImageSize - 5 * 2;
    RemarkText.Measure(parent.RenderSize with { Width = textMaxWidth });
    Content.Width = RemarkText.DesiredSize.Width + personImageSize + 5 * 2;
    Content.Height = Math.Max(RemarkText.DesiredSize.Height,CloseButton.Height + personImageSize + PersonName.Height) + RemarkButtonPanel.MyPipe(v=>v.Visibility == Visibility.Visible ? v.Height : 0) + AskButtonPanel.MyPipe(v=>v.Visibility == Visibility.Visible ? v.Height : 0) + 2.5 * 2;
    Content.Margin = new(Content.Width * (scaleFactor - 1) / 2,Content.Height * (scaleFactor - 1) / 2);
    Content.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = Content.Width / 2,CenterY = Content.Height / 2 };
    RemarkText.MaxWidth = textMaxWidth;
    RemarkFrame.Data = remarkFrameCornerRadius.MyPipe(v => RemarkText.DesiredSize.MyPipe(size =>
      new PathGeometry {
        Figures = [ new PathFigure {
          StartPoint=new(size.Width-v,size.Height-v),
          Segments = [
            new ArcSegment{ Point=new(size.Width-v*2,size.Height),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(v,size.Height) },
            new ArcSegment{ Point=new(0,size.Height-v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(0,v) },
            new ArcSegment{ Point=new(v,0),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(size.Width-v*2,0) },
            new ArcSegment{ Point=new(size.Width-v,v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(size.Width-v,size.Height-v*2) },
            new ArcSegment{ Point=new(size.Width,size.Height-v*2),Size=new(v,v) },
            new ArcSegment{ Point=new(size.Width-v,size.Height-v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
          ],
          IsClosed=true
        } ]
      }
    ));
    ButtonMargin.Width = padding * scaleFactor;
  }
}