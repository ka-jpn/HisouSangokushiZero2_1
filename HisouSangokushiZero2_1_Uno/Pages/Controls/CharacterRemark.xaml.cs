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
internal sealed partial class CharacterRemark:UserControl {
  private static readonly double remarkFrameCornerRadius = 5;
  private static string nowPersonImageName = string.Empty;
  private static UIElement? parent = null;
  internal static readonly double personImageSize = 75;
  internal CharacterRemark() {
    InitializeComponent();
    MyInit(this);
    void MyInit(CharacterRemark page) {
      CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
      page.SizeChanged += (_,_) => parent?.MyApply(parent => ResizeElem(parent));
      RemarkText.Padding = new(remarkFrameCornerRadius,remarkFrameCornerRadius,remarkFrameCornerRadius * 2,remarkFrameCornerRadius);
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
  internal static void Show(CharacterRemark page,GameState game) {
    string newPersonImageName = Text.GetRemarkPersonName(game.PlayCountry,game.PlayTurn < 3);
    string[] contents = GetCharacterRemark(game);
    if(!contents.MyIsEmpty()) {
      page.PersonName.Text = newPersonImageName;
      page.RemarkText.Text = contents.FirstOrDefault() ?? string.Empty;
      page.ButtonPanel.MySetChildren([CreateNextButton(page,[.. contents.Skip(1)])]);
      parent?.MyApply(page.ResizeElem);
      if(nowPersonImageName != newPersonImageName) {
        page.PersonImage.Source = Image.GetSvgImageSource($"Person/{newPersonImageName}",personImageSize*2,personImageSize*2);
        nowPersonImageName = newPersonImageName;
      }
      UIUtil.SetVisibility(page,true);
    } else {
      UIUtil.SetVisibility(page,false);
    }
    static Button CreateNextButton(CharacterRemark page,string[] remainContents) {
      return new Button { HorizontalAlignment = HorizontalAlignment.Stretch,VerticalAlignment = VerticalAlignment.Stretch,Background = Colors.FromARGB(34,0,0,0) }.MySetChild(new TextBlock { Text = remainContents.MyIsEmpty() ? "閉じる" : "次へ" }).MyApply(button =>
        button.Click += (_,_) => {
          GameState newGameState = GameData.game.MyPipe(game => game.Phase switch {
            Phase.Planning => game with { StartPlanningCharacterRemark = remainContents },
            Phase.Execution => game with { StartExecutionCharacterRemark = remainContents },
            _ => game with { GameEndCharacterRemark = remainContents }
          });
          GameData.game = newGameState;
          Show(page,newGameState);
        }
      );
    }
    static string[] GetCharacterRemark(GameState game) {
      return game.Phase switch { Phase.Planning => game.StartPlanningCharacterRemark, Phase.Execution => game.StartExecutionCharacterRemark, _ => game.GameEndCharacterRemark };
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
    Content.Height = Math.Max(RemarkText.DesiredSize.Height,CloseButton.Height + personImageSize + PersonName.Height) + ButtonPanel.Height + 2.5 * 2;
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
  }
}