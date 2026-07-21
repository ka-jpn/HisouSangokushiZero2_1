using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages.Common;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Windows.UI.Core;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Commander = HisouSangokushiZero2_1_Uno.Code.Commander;
using Image = HisouSangokushiZero2_1_Uno.Code.Image;
using Post = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
public sealed partial class Game:Page {
  private record AreaElems(Border Back,TextBlock AreaNameText,StackPanel DefensePersonPanel,StackPanel AffairPersonPanel,Post DefensePost,Post AffairPost,TextBlock AffairText,TextBlock ExText,Grid WrapPanel);
  private static readonly double countryPostPanelWidth = 255;
  private static readonly Dictionary<Post,StackPanel> playerCountryPostPersonPanel = Enum.GetValues<ERole>().SelectMany(role => new List<(Post, StackPanel)>([
    .. Enum.GetValues<PostHead>().Select(headPost=>(new Post(role,new(headPost)),new StackPanel())),
    .. Enumerable.Range(0,UIUtil.capitalPieceCellNum).Select(cellNo=>(new Post(role,new(cellNo)),new StackPanel()))
  ])).ToDictionary();
  private static readonly Dictionary<EArea,AreaElems> areaElemsMap = [];
  private static readonly Dictionary<ERole, Grid> countryPostPanelMap = [];
  private readonly List<TaskToken> animationTaskTokens = [];
  private ERole activePanelRole = ERole.Central;
  private (Panel panel, Post post)? pointerover = null;
  private (Panel panel, PersonId person)? pick = null;
  private double lastScaleFactor = double.NaN;
  private double mapScale = 1;
  internal static double zoomLevel = 0;
  internal static readonly double initContentGridMaxWidth = UIUtil.GetContentMaxWidth();
  public Game() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Game page) {
      AttachEvent(page);
      SetCountryPostsPanel(GameData.game);
      MapImage.Source = Image.GetSvgImageSource("Map",UIUtil.mapSize.Width*2,UIUtil.mapSize.Height*2);
      UIUtil.SwitchViewModeActions.Add(RefreshViewMode);
      UIUtil.ChangeScaleActions.Add(ResizeMap);
      Ask.Init(MainGrid);
      CharacterRemark.Init(MainGrid);
      UIUtil.SaveGameActions.Add(async () => {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await SaveAndLoad.Show(SaveDataPanel, true, async _ => {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => ShowMessage([Text.ProgressSaveText()]));
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => UIUtil.SetVisibility(SaveDataPanel, false));
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => ShowMessage([Text.CompleteSaveText()]));
          }, () => UIUtil.SetVisibility(SaveDataPanel, false), MainGrid.RenderSize));
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => UIUtil.SetVisibility(SaveDataPanel, true));
        });
      });
      UIUtil.LoadGameActions.Add(async () => {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await SaveAndLoad.Show(SaveDataPanel,false,async maybeRead => maybeRead?.MyPipe(async read => {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.ProgressLoadText()]));
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UIUtil.SetVisibility(SaveDataPanel,false));
            await Task.Yield();
            await (read.MaybeGame?.MyPipe(InitGame) ?? Task.CompletedTask);
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.CompleteLoadText(read)]));
          }),() => UIUtil.SetVisibility(SaveDataPanel,false),MainGrid.RenderSize));
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UIUtil.SetVisibility(SaveDataPanel,true));
        });
      });
      UIUtil.InitGameActions.Add(async () => {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.ProgressInitText()]));
          await InitGame(GetGame.GetInitGameScenario(GameData.game.NowScenario));
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.CompleteInitText()]));
        });
      });
      _ = LoadPage(GameData.game);
      async Task LoadPage(GameState startGame) {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,RefreshViewMode);
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => page.ContentGrid.RenderSize.MyPipe(v=>new[]{UIUtil.GetScaleFactor(v),UIUtil.GetScaleFactor(v with { Height = 0 })}.Average()).MyApply(v => LoadingText.FontSize = 32*v).MyApply(v => LoadingText.LineHeight = 32*v)).MyApply(v => UIUtil.SetVisibility(LoadingText, true));
          await InitGame(startGame);
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,ResizeMap);
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => { UIUtil.SetVisibility(LoadingImagePanel,false); UIUtil.SetVisibility(LoadingText, false); });
        });
      }
      void AttachEvent(Game page) {
        MainGrid.SizeChanged += (_, _) => ResizeMap();
        InfoFramePanel.SizeChanged += (_, _) => ResizeInfo();
        OpenLogButton.Click += (_,_) => { UIUtil.ReverseVisibility(GameLogPanel); UIUtil.SetVisibility(ExInfoPanel, false); };
        OpenInfoButton.Click += (_,_) => { UIUtil.ReverseVisibility(ExInfoPanel); UIUtil.SetVisibility(GameLogPanel, false); };
        page.PointerMoved += (_,e) => (pick is { } && MovePersonCanvas.Children.SingleOrDefault() is UIElement personPanel ? () => MovePerson(personPanel,e) : MyUtil.MyUtil.nothing).Invoke();
        page.PointerReleased += (_,e) => (pick is { } ? () => GameData.game = PutPersonPanel(GameData.game) : MyUtil.MyUtil.nothing).Invoke();
        TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
      }
      void ResizeMap() {
        double mapScaleFactor = UIUtil.SolveMapScale(ContentGrid.RenderSize.Height, MainGrid.RenderSize) * GetZoomFactor();
        double horizontalScaleFactor = UIUtil.GetScaleFactor(ContentGrid.RenderSize with { Height = 0 });
        StateInfo.ResizeElem(StateInfoPanel, horizontalScaleFactor, mapScaleFactor);
        HegemonyPoint.ResizeElem(HegemonyPointPanel, horizontalScaleFactor, mapScaleFactor);
        RelayoutCountryPostUI(mapScaleFactor);
        if (mapScaleFactor != lastScaleFactor) {
          RescaleMap(mapScaleFactor);
          lastScaleFactor = mapScaleFactor;
        }
        mapScale = mapScaleFactor;
        void RescaleMap(double scaleFactor) {
          ScaleTransform mapScaleTransform = new() { ScaleX = mapScaleFactor, ScaleY = mapScaleFactor };
          CountryPostsPanel.Margin = new(0, 0, MainGrid.RenderSize.Width * (scaleFactor - 1), CountryPostsPanel.Height * (scaleFactor - 1));
          CountryPostsPanel.RenderTransform = mapScaleTransform;
          MapImage.Width = UIUtil.mapSize.Width * scaleFactor;
          MapImage.Height = UIUtil.mapSize.Height * scaleFactor;
          MapElementsCanvas.Margin = new(0, 0, MapPanel.RenderSize.Width / scaleFactor * (scaleFactor - 1), MapPanel.RenderSize.Height / scaleFactor * (scaleFactor - 1));
          MapElementsCanvas.RenderTransform = mapScaleTransform;
          MapAnimationElementsCanvas.Margin = new(0, 0, MapPanel.RenderSize.Width / scaleFactor * (scaleFactor - 1), MapPanel.RenderSize.Height / scaleFactor * (scaleFactor - 1));
          MapAnimationElementsCanvas.RenderTransform = mapScaleTransform;
          TurnLogPanel.Margin = new(UIUtil.infoFrameWidth * scaleFactor, 0, 0, 0);
          TurnLogPanel.RenderTransform = mapScaleTransform;
          TurnFillDreamConditionPanel.Margin = new(UIUtil.infoFrameWidth * scaleFactor, UIUtil.infoFrameWidth * scaleFactor, 0, 0);
          MovePersonCanvas.RenderTransform = mapScaleTransform;
          RescaleTurnFillDreamConditionPanelUI(scaleFactor);
        }
        void RelayoutCountryPostUI(double scaleFactor) {
          double PostPanelLeftUnit = (MainGrid.RenderSize.Width / scaleFactor - countryPostPanelWidth) / (countryPostPanelMap.Count - 1);
          countryPostPanelMap.Values.Select((elem, index) => (elem, index)).ToList().ForEach(v => Canvas.SetLeft(v.elem, PostPanelLeftUnit * v.index));
        }
      }
      void ResizeInfo() {
        InfoLayoutPanel.Margin = new(0, 0, InfoFramePanel.RenderSize.Width / mapScale * (mapScale - 1), InfoFramePanel.RenderSize.Height / mapScale * (mapScale - 1));
        InfoLayoutPanel.RenderTransform = new ScaleTransform() { ScaleX = mapScale, ScaleY = mapScale };
        InfoLayoutPanel.Width = InfoFramePanel.RenderSize.Width / mapScale;
      }
      void SetCountryPostsPanel(GameState game) {
        Dictionary<ERole, Color> countryRolePanelColorMap = new([
          new(ERole.Central,new Color(255,240,240,210)),new(ERole.Affair,new Color(255,240,240,240)),
            new(ERole.Defense,new Color(255,210,210,240)),new(ERole.Attack,new Color(255,240,210,210))
        ]);
        Dictionary<ERole, Grid> rolePanelMap = countryRolePanelColorMap.ToDictionary(v => v.Key, v => CreateCountryPostPanel(game, v.Key, v.Value));
        rolePanelMap.ToList().ForEach(v => v.Value.PointerEntered += (_, _) => UpdateCountryPostPanelZIndex(v.Key));
        countryPostPanelMap.Clear();
        rolePanelMap.ToList().ForEach(v => countryPostPanelMap.Add(v.Key, v.Value));
        CountryPostsPanel.Children.Clear();
        rolePanelMap.Reverse().ToList().ForEach(v => CountryPostsPanel.Children.Add(v.Value));
      }
      void RefreshViewMode() {
        SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
        ContentGrid.MaxWidth = UIUtil.GetContentMaxWidth();
      }
    }
  }
  private async Task InitGame(GameState newGameState) {
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => CleanUI(newGameState));
    await Task.Yield();
    await SetInitUI(newGameState);
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateAreaPanels(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateCountryPosts(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateCountryInfoPanel(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => GameLog.UpdateLogMessageUI(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowCharacterRemark(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => HegemonyPoint.Show(HegemonyPointPanel,newGameState,UIUtil.GetScaleFactor(ContentGrid.RenderSize with { Height = 0 })));
    GameData.game = newGameState;
    GameData.startGameDateTime = DateTime.Now;
    void CleanUI(GameState game) {
      new List<UIElement>([AskPanel,CharacterRemarkPanel]).ForEach(v => UIUtil.SetVisibility(v,false));
      MovePersonCanvas.MySetChildren([]);
      MapAnimationElementsCanvas.MySetChildren([]);
    }
    async Task SetInitUI(GameState game) {
      areaElemsMap.Clear();
      game.AreaMap.ToDictionary(v => v.Key, v => CreateAreaElems(game, v.Key)).ToList().ForEach(v => areaElemsMap.Add(v.Key, v.Value));
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,MapElementsCanvas.Children.Clear);
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.RoadConnections.ToList().ForEach(road => MapElementsCanvas.Children.Add(MaybeCreateRoadLine(game,road))));
      await Task.Yield();
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => areaElemsMap.ToList().ForEach(v => MapElementsCanvas.Children.Add(CreateAreaPanelFromAreaElems(game,v))));
      static Line? MaybeCreateRoadLine(GameState game,Road road) {
        return Area.GetAreaPoint(game,road.From,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth) is Point from && Area.GetAreaPoint(game,road.To,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth) is Point to ? CreateRoadLine(road,from,to) : null;
        static Line CreateRoadLine(Road road,Point from,Point to) => new() { X1 = from.X,Y1 = from.Y,X2 = to.X,Y2 = to.Y,Stroke = (road.Kind switch { RoadKind.Land => UIUtil.landRoadColor, RoadKind.Water => UIUtil.waterRoadColor }).ToBrush(),StrokeThickness = 5 * Math.Pow(road.Easiness,1.7) / 2 + 10 };
      }
      static AreaElems CreateAreaElems(GameState game,EArea area) {
        Border areaBackBorder = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius,BorderBrush = Colors.Red };
        TextBlock areaNameText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,2,0,-1) };
        StackPanel areaDefensePersonPanel = [];
        StackPanel areaAffairPersonPanel = [];
        Post areaDefensePost = new(ERole.Defense,new(area));
        Post areaAffairPost = new(ERole.Affair,new(area));
        TextBlock affairText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,-1) };
        TextBlock exText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,-1,0,-3) };
        Grid areaWrapPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height };
        return new(areaBackBorder,areaNameText,areaDefensePersonPanel,areaAffairPersonPanel,areaDefensePost,areaAffairPost,affairText,exText,areaWrapPanel);
      }
      Grid CreateAreaPanelFromAreaElems(GameState game,KeyValuePair<EArea,AreaElems> areaElemInfo) {
        Grid areaPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius };
        StackPanel personPutAreaPanel = new() {
          HorizontalAlignment = HorizontalAlignment.Center,Orientation = Orientation.Horizontal,
          BorderBrush = GetPostFrameColor(game,areaElemInfo.Key).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Margin = new(0,-2,0,0)
        };
        areaPanel.PointerPressed += (_,_) => PushArea.Push(areaElemInfo.Key);
        areaPanel.PointerExited += (_,_) => PushArea.Exit();
        areaPanel.PointerReleased += (_,_) => GameData.game = PushArea.Release(this,GameData.game,areaElemInfo.Key);
        Area.GetAreaPoint(game,areaElemInfo.Key,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth)?.MyApply(v => Canvas.SetLeft(areaPanel,v.X - UIUtil.areaSize.Width / 2)).MyApply(v => Canvas.SetTop(areaPanel,v.Y - UIUtil.areaSize.Height / 2));
        return areaPanel.MySetChildren([
          areaElemInfo.Value.Back,
          new StackPanel{ Width = UIUtil.areaSize.Width,VerticalAlignment = VerticalAlignment.Center }.MySetChildren([
            areaElemInfo.Value.AreaNameText,
            personPutAreaPanel.MySetChildren([
              CreatePersonPutPanel(game,areaElemInfo.Value.DefensePost,Text.AreaPostDefenseText(),areaElemInfo.Value.DefensePersonPanel),
              CreatePersonPutPanel(game,areaElemInfo.Value.AffairPost,Text.AreaPostAffairText(),areaElemInfo.Value.AffairPersonPanel)
            ]),
            areaElemInfo.Value.AffairText,
            areaElemInfo.Value.ExText
          ]),
          areaElemInfo.Value.WrapPanel
        ]);
      }
    }
  }
  private void UpdateAreaPanels(GameState game) {
    double capitalBorderWidth = 1.75;
    areaElemsMap.ToList().ForEach(areaElems => {
      AreaData? areaData = game.AreaMap.GetValueOrDefault(areaElems.Key);
      areaElems.Value.Back.BorderThickness = new(game.CountryMap.Values.Select(v => v.CapitalArea).Contains(areaElems.Key) ? capitalBorderWidth : 0);
      areaElems.Value.Back.Background = Country.GetCountryColor(game,areaData?.Country).ToBrush();
      areaElems.Value.AreaNameText.Text = Text.AreaText(areaElems.Key, areaData?.Country);
      areaElems.Value.AreaNameText.RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,5 / UIUtil.CalcTextWidthForFullWidth(areaElems.Value.AreaNameText.Text)),CenterX = UIUtil.CalcTextWidthForFullWidth(areaElems.Value.AreaNameText.Text) * BasicStyle.fontsize / 2 };
      areaElems.Value.DefensePersonPanel.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.DefensePost)?.MyPipe(param => CreatePersonPanel(game,param)).MyMaybeToList() ?? []]);
      areaElems.Value.AffairPersonPanel.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.AffairPost)?.MyPipe(param => CreatePersonPanel(game,param)).MyMaybeToList() ?? []]);
      areaElems.Value.AffairText.Text = $"{Math.Floor(areaData?.AffairParam.AffairNow ?? 0)}/{Math.Floor(areaData?.AffairParam.AffairsMax ?? 0)}";
      areaElems.Value.ExText.Text = areaData?.Country?.MyPipe(country => (Country.IsSleep(game,country) ? Text.CountrySleepText(game, country) : null) + (Country.IsFocusDefense(game,country) ? Text.CountryFocusDefenseText() : null));
      areaElems.Value.WrapPanel.Background = Area.IsPlayerSelectable(game,areaElems.Key) ? null : UIUtil.grayoutColor.ToBrush();
    });
  }
  private void UpdateCountryPosts(GameState game) {
    playerCountryPostPersonPanel.ToList().ForEach(countryPostInfo => countryPostInfo.Value.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Country == game.PlayCountry && v?.Value.Post == countryPostInfo.Key)?.MyPipe(param => CreatePersonPanel(game,param)).MyMaybeToList() ?? []]));
    UIUtil.SetVisibility(CountryPostsPanel,IsShowCountryPostPanel(game.Phase));
    static bool IsShowCountryPostPanel(Phase phase) => phase is Phase.Planning or Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd;
  }
  private void UpdateCountryInfoPanel(GameState game) {
    List<UIElement> contents = game.Phase switch {
      Phase.Starting => ShowSelectScenario(game),
      Phase.Planning or Phase.Execution => ShowCountryInfo(game),
      Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => ShowEndGameInfo(game)
    };
    string? buttonText = Text.EndPhaseButtonText(game.Phase);
    StateInfo.Show(StateInfoPanel,contents,buttonText,ButtonAction);
    GameState ButtonAction(GameState game) {
      animationTaskTokens.Clear();
      return game.Phase switch {
        Phase.Starting => throw new Exception(),
        Phase.Planning => game.MyPipe(EndPlanningPhase).MyApply(UpdateCountryInfoPanel),
        Phase.Execution => game.MyPipe(EndExecutionPhase).MyApply(UpdateCountryInfoPanel),
        Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => game.MyApply(ShowCharacterRemark).MyApply(ShowGameEndLogButtonClick)
      };
    }
    List<UIElement> ShowSelectScenario(GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApply(v=>Grid.SetColumn(v,0)),
        new TextBlock { Text=Text.ScenarioCaptionText(),VerticalAlignment=VerticalAlignment.Center }.MyApply(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,2)),
        new ComboBox {
          Width=100, VerticalAlignment=VerticalAlignment.Stretch,
          SelectedIndex=ScenarioBase.GetScenarioIds().MyGetIndex(v=>v==game.NowScenario)??0,
          Foreground=Colors.Black, Background=Colors.White, Padding=new(10,0,0,0), Margin=new(0,1),
          ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApply(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UIUtil.GetScaleFactor(MainGrid.RenderSize)))),
        }.MyApply(elem => ScenarioBase.GetScenarioIds().Select(v=>v.Value).ToList().ForEach(elem.Items.Add)).MyApply(v=>
          v.SelectionChanged+=(_,_)=>(v.SelectedItem as string)?.MyApply(async text => await InitGame(GetGame.GetInitGameScenario(new(text))))
        ).MyApply(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,4)),
        new TextBlock{ Text=game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.MyPipe(Text.StartYearText),VerticalAlignment=VerticalAlignment.Center }.MyApply(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,6)),
        new TextBlock{ Text=game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.MyPipe(Text.EndYearText),VerticalAlignment=VerticalAlignment.Center}.MyApply(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,8)),
        new TextBlock{ Text=Text.ClickMapAreaText(),VerticalAlignment=VerticalAlignment.Center }.MyApply(v=>Grid.SetColumn(v,9)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,10)),
     ]).MyApply(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(5, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(5, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))];
    static List<UIElement> ShowCountryInfo(GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApply(v=>Grid.SetColumn(v,0)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.GetCalendarText(game.NowScenario,game.PlayTurn ?? 0) }, new TextBlock{ Text=Text.CountryParamCaptionText(game.PlayCountry) },
        ]).MyApply(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,2)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.CountryCapitalAreaParamText(game,game.PlayCountry) }, new TextBlock{ Text=Text.CountryFundParamText(game,game.PlayCountry) },
        ]).MyApply(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,4)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.CountryAreaNumParamText(game,game.PlayCountry) }, new TextBlock{ Text=Text.CountryAffairDifficultParamText(game,game.PlayCountry) },
        ]).MyApply(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,6)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.CountryAffairPowerParamText(game,game.PlayCountry) }, new TextBlock{ Text=Text.CountryTotalAffairParamText(game,game.PlayCountry) },
        ]).MyApply(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,8)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.CountryInFundParamText(game,game.PlayCountry) }, new TextBlock{ Text=Text.CountryOutFundParamText(game,game.PlayCountry) },
        ]).MyApply(v=>Grid.SetColumn(v,9)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,10)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.PlayCountryArmyTargetAreaParamText(game) },
        ]).MyApply(v=>Grid.SetColumn(v,11)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,12)),
      ]).MyApply(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))
    ];
    static List<UIElement> ShowEndGameInfo(GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApply(v=>Grid.SetColumn(v,0)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.GetCalendarText(game.NowScenario,game.PlayTurn ?? 0) }, new TextBlock{ Text=Text.CountryParamCaptionText(game.PlayCountry) },
        ]).MyApply(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,2)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.GameEndText() }, new TextBlock{ Text=Text.GameResultText(game) },
        ]).MyApply(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApply(v=>Grid.SetColumn(v,4)),
      ]).MyApply(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))
    ];
    void ShowGameEndLogButtonClick(GameState game) {
      string title = Text.GameEndLogCaptionText();
      List<TextBlock> contents = [.. game.GameLog.Select(log => new TextBlock { Text = log })];
      string okButtonText = Text.PostGameEndLogText();
      Ask.SetElems(AskPanel,title,contents,okButtonText,() => LogButtonClick(game),false);
      static void LogButtonClick(GameState game) {
        string url = $"https://karintougames.com/siteContents/gameComment.php?caption={BaseData.name.Value} ver.{BaseData.version.Value}&comment={HttpUtility.UrlEncode(string.Join('\n',game.GameLog))}";
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS($"top.location.href='{url}';");
#else
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
#endif
      }
    }
    GameState EndPlanningPhase(GameState game) {
      ResetTurnUI();
      return game.MyPipe(ResetPlanningCharacterRemark).MyPipe(game => UpdateGame.AutoPutPostCPU(game,[ECountry.漢])).MyPipe(game => game with { Phase = Phase.Execution }).MyApply(UpdateAreaPanels).MyApply(ExecutionMoveFlag).MyPipe(ArmyAttack).MyApply(GameLog.UpdateLogMessageUI).MyApply(ShowCharacterRemark);
      void ResetTurnUI() => new List<UIElement>([CharacterRemarkPanel,CountryPostsPanel]).ForEach(v => UIUtil.SetVisibility(v,false));
      static GameState ResetPlanningCharacterRemark(GameState game) => game with { StartPlanningCharacterRemark = [] };
      void ExecutionMoveFlag(GameState game) {
        game.ArmyTargetMap.Where(v => v.Value != null && Country.IsSuccessAttack(game,v.Key)).ToDictionary(attackInfo => GetFlag(game,attackInfo.Key),attackInfo => CalcFlagMovePos(game,attackInfo)).MyApply(flagMap => MapAnimationElementsCanvas.MySetChildren([.. flagMap.Keys])).MyApply(flagMap => MoveFlags(flagMap));
        static Grid GetFlag(GameState game,ECountry attackCountry) {
          return CreateFlag(game,attackCountry).MyPipe(v => AttachFlag(game,v,attackCountry));
          static Grid CreateFlag(GameState game,ECountry attackCountry) {
            string flagText = attackCountry.ToString();
            double flagTextMaxWidthForFullWidth = 2.2, flagTextScale = 1.6, flagTextWidthForFullWidth = UIUtil.CalcTextWidthForFullWidth(flagText),flagPoleWidth = 3;
            TextBlock flagTextBlock = new() {
              Text = flagText,Width = Math.Min(flagTextMaxWidthForFullWidth,flagTextWidthForFullWidth) * BasicStyle.fontsize * flagTextScale,
              HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,
              RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,flagTextMaxWidthForFullWidth / flagTextWidthForFullWidth) * flagTextScale,ScaleY = flagTextScale,CenterY = BasicStyle.fontsize / 2 }
            };
            Grid flagPole = new() { Width = flagPoleWidth,Height = 55,Background = Colors.White,HorizontalAlignment = HorizontalAlignment.Left,VerticalAlignment = VerticalAlignment.Top };
            Grid flagPanel = new() { Width = 60,Height = 38,Background = Country.GetCountryColor(game,attackCountry).ToBrush(),HorizontalAlignment = HorizontalAlignment.Left,VerticalAlignment = VerticalAlignment.Top,RenderTransform = new TranslateTransform{X = flagPoleWidth/2} };
            Microsoft.UI.Xaml.Controls.Image attackImage = new() { Source = Image.GetSvgImageSource("Army",200,200), Width = 65,Height = 55,HorizontalAlignment = HorizontalAlignment.Right };
            return new Grid() { Width = 90,Height = 65 }.MySetChildren([attackImage,flagPole,flagPanel.MySetChildren([flagTextBlock])]);
          }
          static Grid AttachFlag(GameState game,Grid rawFlag,ECountry attackCountry) {
            decimal attackRank = Commander.CommanderRank(game,Commander.GetAttackCommander(game,attackCountry),ERole.Attack);
            Grid attackRankPanel = new Grid() { HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Bottom }.MySetChildren([..UIUtil.CreateWithShadow(CreateRankText,1,Colors.White)]);
            return new Grid() { Width = rawFlag.Width,Height = rawFlag.Height,Visibility = Visibility.Collapsed }.MySetChildren([rawFlag,attackRankPanel]);
            TextBlock CreateRankText() => new() { Text = $"Rank{attackRank}",FontSize = 21 };
          }
        }
        static List<Point> CalcFlagMovePos(GameState game,KeyValuePair<ECountry,EArea?> attackInfo) {
          int totalMoveFrame = 90;
          EArea[] route = attackInfo.Value?.MyPipe(target => Route.SolveAtackArmyRoute(game,attackInfo.Key,target)) ?? [];
          List<(Point from, Point to)> routeAreaPoints = [.. route.Select(v => AreaToPoint(game,v)).MyNonNull().MyAdjacentCombinations()];
          List<Point> posList = [.. routeAreaPoints.SelectMany(v => Enumerable.Range(0,totalMoveFrame).Select(index => new Point(double.Lerp(v.from.X,v.to.X,index / (double)totalMoveFrame),double.Lerp(v.from.Y,v.to.Y,index / (double)totalMoveFrame)))).MyPipe(v => v.Chunk(v.Count() / totalMoveFrame).Select(v => v.FirstOrDefault()).Append(routeAreaPoints.LastOrDefault().to).MyNonNull())];
          return posList;
          static Point? AreaToPoint(GameState game,EArea area) => Area.GetAreaPoint(game,area,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth);
        }
        void MoveFlags(Dictionary<Grid,List<Point>> flags) {
          TaskToken token = new TaskToken().MyApply(animationTaskTokens.Add);
          DateTime startTime = DateTime.Now;
          Task.Run(async () => {
            await flags.MyAsyncForEachConcurrent(async v => {
              await v.Value.Select((value,index)=>(value, index)).MyAsyncForEachSequential(async pos => {
                if(pos.index == 0) {
                  await FirstMoveFlag(startTime, v.Key, pos.value, pos.index);
                } else if(pos.index == v.Value.Count - 1) {
                  await EndMoveFlag(startTime, v.Key, pos.value, pos.index);
                } else {
                  await MoveFlag(token, startTime, v.Key, pos.value, pos.index);
                }
              });
            });
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateAreaPanels(GameData.game));
            animationTaskTokens.Remove(token);
          });
          async Task FirstMoveFlag(DateTime startTime, Grid flagPanel, Point pos, int index) => await MoveFlagWithAction(startTime, flagPanel, pos, index, () => flagPanel.Visibility = Visibility.Visible);
          async Task MoveFlag(TaskToken token,DateTime startTime,Grid flagPanel,Point pos,int index) {
            if(!animationTaskTokens.Contains(token)) return;
            if(nextWaitSeconds(startTime, index) <= 0) return;
            await MoveFlagWithAction(startTime, flagPanel, pos, index, null);
          }
          async Task EndMoveFlag(DateTime startTime, Grid flagPanel, Point pos, int index) => await MoveFlagWithAction(startTime, flagPanel, pos, index, null);
          async Task MoveFlagWithAction(DateTime startTime,Grid flagPanel,Point pos,int index,Action? action) {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => { Canvas.SetLeft(flagPanel,pos.X - flagPanel.Width / 2); Canvas.SetTop(flagPanel,pos.Y - flagPanel.Height / 2); action?.Invoke(); });
            await nextWaitSeconds(startTime, index).MyPipe(v =>v > 0 ? Task.Delay(TimeSpan.FromSeconds(v)) : Task.CompletedTask);
          }
          static double nextWaitSeconds(DateTime startTime, int index) => UIUtil.nextStepDelaySeconds * (index+1) - (DateTime.Now - startTime).TotalSeconds;
        }
      }
      static GameState ArmyAttack(GameState game) {
        return game.CountryMap.Keys.Where(v => Country.GetAreaNum(game,v) >= 1).OrderBy(country => Country.GetTotalAffair(game,country)).Aggregate(game,(game,country) => {
          return game.ArmyTargetMap.GetValueOrDefault(country) is EArea target ? TryAttack(game,country,target) : !Country.IsSleep(game,country) ? ExeDefense(game,country) : ExeSleep(game,country);
          static GameState TryAttack(GameState game,ECountry country,EArea targetArea) {
            return Country.IsSuccessAttack(game,country) ? ExeAttack(game,country,targetArea) : FailAttack(game,country,targetArea);
            static GameState ExeAttack(GameState game,ECountry country,EArea targetArea) => targetArea.MyPipe(game.AreaMap.GetValueOrDefault)?.Country.MyPipe(defeseSide => UpdateGame.Attack(game,country,targetArea,defeseSide,Country.IsFocusDefense(game,defeseSide))) ?? game;
            static GameState FailAttack(GameState game,ECountry country,EArea targetArea) => game.MyPipe(game => UpdateGame.Defense(game,country,true)).MyPipe(game => country == game.PlayCountry ? UpdateGame.AppendStartExecutionRemark(game,[Text.StartExecutionFailAttackCharacterRemarkText(targetArea)]) : game);
          }
          static GameState ExeDefense(GameState game,ECountry country) => game.MyPipe(game => UpdateGame.Defense(game,country,false));
          static GameState ExeSleep(GameState game,ECountry country) => game.MyPipe(game => UpdateGame.Sleep(game,country));
        });
      }
    }
    GameState EndExecutionPhase(GameState game) {
      MapAnimationElementsCanvas.MySetChildren([]);
      UIUtil.SetVisibility(CharacterRemarkPanel,false);
      UIUtil.SetVisibility(CountryPostsPanel,true);
      return game.MyPipe(NextTurn).MyPipe(UpdateGame.GameEndJudge).MyPipe(SwitchEnd);
      GameState NextTurn(GameState game) {
        return game.MyPipe(ResetExecutionCharacterRemark).MyPipe(UpdateGame.NextTurn).MyPipe(v => v with { Phase = Phase.Planning }).MyPipe(UpdateGame.Rest).MyApply(UpdateAreaPanels).MyPipe(UpdateHegemonyPoint).MyPipe(game => UpdateGame.AppendStartPlanningRemark(game,[.. Text.StartPlanningCharacterRemarkTexts(game)])).MyPipe(UpdateInfo);
        GameState UpdateHegemonyPoint(GameState game) => game.MyPipe(UpdateGame.UpdateFillDreamCondition).MyApply(game => HegemonyPoint.Show(HegemonyPointPanel,game,UIUtil.GetScaleFactor(ContentGrid.RenderSize with { Height = 0 }))).MyPipe(UpdateGame.UpdateHegemonyTurn);
        GameState UpdateInfo(GameState game) => game.MyApply(UpdateCountryPosts).MyApply(UpdateTurnLogUI).MyApply(UpdateTurnFillDreamConditionUI).MyApply(GameLog.UpdateLogMessageUI).MyApply(ShowCharacterRemark);
        static GameState ResetExecutionCharacterRemark(GameState game) => game with { StartExecutionCharacterRemark = [] };
      }
      GameState SwitchEnd(GameState game){
        return game.Phase switch {
          Phase.PerishEnd => (game with { GameEndCharacterRemark = ["我らは拠り所を失いました\nもうすぐ追手が\nうわー"] }).MyApply(ShowCharacterRemark),
          Phase.TurnLimitOverEnd => (game with { GameEndCharacterRemark = ["我らは覇を唱えることができませんでした\n次の世代に託しましょう"] }).MyApply(ShowCharacterRemark),
          Phase.WinEnd => (game with { GameEndCharacterRemark = ["我らは覇を3回唱えました\n勝利です！\nこれからは安寧の世を目指しましょう"] }).MyApply(ShowCharacterRemark),
          Phase.OtherWinEnd => (game with { GameEndCharacterRemark = ["他の者が覇を3回唱えました\n我らは及ばず・・\n敗北です"] }).MyApply(ShowCharacterRemark),
          _ => game.MyPipe(UpdateGame.CalcArmyTarget)
        };
        
      }
    }
  }
  private Grid CreatePersonPutPanel(GameState game,Post post,string backText,StackPanel personPutInnerPanel) {
    Grid personPutPanel = new() { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,BorderBrush = GetPostFrameColor(game,post.PostKind.MaybeArea).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Background = Colors.Transparent };
    TextBlock personPutBackText = new() {
      Text = backText,Foreground = Windows.UI.Color.FromArgb(100,100,100,100),HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,Margin = new(0,2,0,-2),
      RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personPutFontScale,ScaleY = UIUtil.personPutFontScale,CenterX = UIUtil.CalcTextWidthForFullWidth(backText) * BasicStyle.fontsize / 2,CenterY = BasicStyle.fontsize / 2 }
    };
    personPutPanel.PointerEntered += (_,_) => EnterPersonPutPanel(GameData.game,personPutInnerPanel,post);
    personPutPanel.PointerExited += (_,_) => ExitPersonPutPanel(personPutInnerPanel);
    return personPutPanel.MySetChildren([personPutBackText,personPutInnerPanel]);
    void EnterPersonPutPanel(GameState game,StackPanel personPutInnerPanel,Post post) {
      if(game.Phase != Phase.Starting && (post.PostKind.MaybeArea?.MyPipe(area => game.AreaMap.GetValueOrDefault(area)?.Country == game.PlayCountry) ?? true)) {
        pointerover?.MyApply(v => v.panel.Background = Colors.Transparent);
        personPutInnerPanel.Background = Windows.UI.Color.FromArgb(150,255,255,255);
        pointerover = (personPutInnerPanel, post);
      }
    }
    void ExitPersonPutPanel(StackPanel personPutInnerPanel) {
      if(pointerover != null) {
        personPutInnerPanel.Background = Colors.Transparent;
        pointerover = null;
      }
    }
  }
  private Grid CreatePersonPanel(GameState game,KeyValuePair<PersonId,PersonData> person) {
    double minFullWidthLength = 2.25;
    double margin = FontSize * (UIUtil.CalcTextWidthForFullWidth(person.Key.Value) - 2);
    Grid panel = new Grid { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,Background = Country.GetCountryColor(game,Person.GetPersonCountry(game,person.Key)).ToBrush() }.MySetChildren([
      new StackPanel { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=Windows.UI.Color.FromArgb((byte)(20*Person.GetPersonRank(game,person.Key)),0,0,0) }.MySetChildren([
        GetRankPanel(game,person),
        new TextBlock { Text=person.Key.Value,TextAlignment=TextAlignment.Center,Margin=new(-margin/2,0),RenderTransform=new ScaleTransform{ ScaleX=minFullWidthLength/Math.Max(minFullWidthLength,UIUtil.CalcTextWidthForFullWidth(person.Key.Value))*UIUtil.personNameFontScale,ScaleY=UIUtil.personNameFontScale,CenterX=UIUtil.personPutSize.Width/2+margin/minFullWidthLength  }  }
      ])
    ]);
    panel.PointerPressed += (_,e) => PickPersonPanel(GameData.game,e,panel,person.Key);
    return panel;
    StackPanel GetRankPanel(GameState game,KeyValuePair<PersonId,PersonData> person) {
      int postRank = Person.CalcRoleRank(game,person.Key,person.Value.Post?.PostRole);
      int personRank = Person.GetPersonRank(game,person.Key);
      return new StackPanel { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personRankFontScale,ScaleY = UIUtil.personRankFontScale,CenterX = FontSize / 2 } }.MySetChildren(GetRankTextBlock(game,person.Key,postRank,personRank == postRank));
      static List<UIElement> GetRankTextBlock(GameState game,PersonId person,int rank,bool isMatchRole) => [
        new TextBlock() { Margin = new(0,-0.2,0,0),Text = rank.ToString(),Foreground = isMatchRole ? Colors.Black : Colors.Red },
        .. (isMatchRole?null:new Microsoft.UI.Xaml.Controls.Image() { Source = Image.GetSvgImageSource($"{Person.GetPersonRole(game,person)}",80,80),VerticalAlignment = VerticalAlignment.Top,Width = BasicStyle.textHeight * 0.75,Height = BasicStyle.textHeight * 0.75 }).MyMaybeToList()
      ];
    }
    void PickPersonPanel(GameState game,PointerRoutedEventArgs e,Panel personPanel,PersonId person) {
      if(game.Phase != Phase.Starting && Person.GetPersonCountry(game,person) == game.PlayCountry && personPanel.Parent is Panel parentPanel) {
        personPanel.IsHitTestVisible = false;
        parentPanel.MySetChildren([]);
        MovePersonCanvas.MySetChildren([personPanel]);
        MovePerson(personPanel,e);
        pick = (parentPanel, person);
      }
    }
  }
  private void MovePerson(UIElement personPanel,PointerRoutedEventArgs e) {
    Canvas.SetLeft(personPanel,e.GetCurrentPoint(MovePersonCanvas).Position.X - UIUtil.personPutSize.Width / 2);
    Canvas.SetTop(personPanel,e.GetCurrentPoint(MovePersonCanvas).Position.Y - UIUtil.personPutSize.Height / 2);
  }
  private Grid CreateCountryPostPanel(GameState game,ERole role,Color backColor) {
    return new Grid() { Width = countryPostPanelWidth, Background = backColor.ToBrush() }.MySetChildren([
      new StackPanel() {HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
        new StackPanel() { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
          new TextBlock { Text = Text.RoleToText(role) },
          new Microsoft.UI.Xaml.Controls.Image { Source = Code.Image.GetSvgImageSource($"{role}",80,80),Width = BasicStyle.textHeight,Height = BasicStyle.textHeight,VerticalAlignment = VerticalAlignment.Center }
        ]),
        CreateCountryPosts(game,role)
      ])
    ]);
    StackPanel CreateCountryPosts(GameState game,ERole role) {
      return new StackPanel().MySetChildren([
        new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
            CreatePersonHeadPostPanel(game,role),
            CreateAutoPutPersonButton(game,role)
          ]),
          CreatePersonPostPanelElems(game,role)
      ]);
      Button CreateAutoPutPersonButton(GameState game,ERole role) {
        Button autoPutPersonButton = new Button { Width = UIUtil.personPutSize.Width * 3,VerticalAlignment = VerticalAlignment.Stretch,Background = Windows.UI.Color.FromArgb(100,100,100,100) }.MyApply(v => v.Content = new TextBlock { Text = Text.AutoPutPersonButtonText() });
        autoPutPersonButton.Click += (_,_) => GameData.game = AutoPutPersonButtonClick(GameData.game);
        return autoPutPersonButton;
        GameState AutoPutPersonButtonClick(GameState game) => game.PlayCountry?.MyPipe(country => Code.Post.GetAutoPutPost(game,country,role)).MyPipe(postMap => UpdateGame.SetPersonPost(game,postMap)).MyApply(UpdateAreaPanels).MyApply(UpdateCountryPosts).MyApply(UpdateCountryInfoPanel) ?? game;
      }
      StackPanel CreatePersonHeadPostPanel(GameState game,ERole role) {
        return new StackPanel { Orientation = Orientation.Horizontal,BorderBrush = GetPostFrameColor(game,null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([
          .. Enum.GetValues<PostHead>().Select(v=> new Post(role,new(v)).MyPipe(post=> CreatePersonPutPanel(game,post,Text.PlayerCountryPostText(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post)??[]))),
          ]);
      }
      StackPanel CreatePersonPostPanelElems(GameState game,ERole role) {
        return new StackPanel { BorderBrush = GetPostFrameColor(game,null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([.. Enumerable.Range(0,UIUtil.capitalPieceRowNum).Select(row => GetPersonPostLinePanel(game,role,row,game.PersonMap.Where(v => Person.GetPersonCountry(game,v.Key) == game.PlayCountry).ToDictionary()))]);
        StackPanel GetPersonPostLinePanel(GameState game,ERole role,int rowNo,Dictionary<PersonId,PersonData> personMap) => new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
          .. Enumerable.Range(0,UIUtil.capitalPieceColumnNum).Select(i => new Post(role,new(rowNo * UIUtil.capitalPieceColumnNum + i)).MyPipe(post=> CreatePersonPutPanel(game,post,Text.PlayerCountryPostText(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post) ?? [])))
        ]);
      }
    }
  }
  private static Color GetPostFrameColor(GameState game,EArea? area) => area != null && (game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.ChinaAreas ?? []).Contains(area.Value) ? new Color(150,100,100,30) : new Color(120,0,0,0);
  private void RescaleTurnFillDreamConditionPanelUI(double scaleFactor) {
    TurnFillDreamConditionPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = TurnFillDreamConditionPanel.RenderSize.Width / 2 };
  }
  private async void UpdateTurnLogUI(GameState game) {
    DateTime startTime = DateTime.Now;
    TimeSpan startAnimationDelay = TimeSpan.FromSeconds(6);
    int transparentFrameCount = 60;
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),Height = game.TurnNewLog.Count * BasicStyle.textHeight,IsHitTestVisible = false
    }.MySetChildren([.. game.TurnNewLog.Select(logText => new TextBlock() { Text = logText })]);
    game.TurnNewLog.Clear();
    TurnLogPanel.Children.Add(panel);
    ResizeTurnLogUI();
    await Task.Run(async () => {
      await Task.Delay(startAnimationDelay);
      await Enumerable.Range(1,transparentFrameCount).MyAsyncForEachSequential(async v => {
        double nextWaitSeconds = UIUtil.nextStepDelaySeconds * v - (DateTime.Now - startTime).TotalSeconds;
        if(nextWaitSeconds <= 0) return;
        await Task.Delay(TimeSpan.FromSeconds(nextWaitSeconds));
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => panel.Opacity = 1 - (double)v / transparentFrameCount);
      });
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,()=>TurnLogPanel.Children.Remove(panel));
      await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,ResizeTurnLogUI);
    });
    void ResizeTurnLogUI() => TurnLogPanel.Height = TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * UIUtil.GetScaleFactor(MainGrid.RenderSize) *GetZoomFactor();
  }
  private async void UpdateTurnFillDreamConditionUI(GameState game) {
    DateTime startTime = DateTime.Now;
    TimeSpan startAnimationDelay = TimeSpan.FromSeconds(6);
    int transparentFrameCount = 60;
    double shadowWidth = 0.7;
    Dictionary<string,bool?> fillDreamConditionMap = game.PlayCountry?.MyPipe(v => game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.FillDreamConditionMap.GetValueOrDefault(v))?.ProgressExplainFunc(game) ?? [];
    StackPanel panel = new StackPanel() { Background = Windows.UI.Color.FromArgb(187,255,255,255),IsHitTestVisible = false }.MySetChildren([
      new TextBlock() { Text = Text.FillDreamConditionCaptionText(game) },
      .. fillDreamConditionMap.Select(fillDreamCondition => new StackPanel(){ Orientation = Orientation.Horizontal }.MySetChildren([
        new Grid(){ Width = BasicStyle.fontsize }.MySetChildren(fillDreamCondition.Value is bool isClearCond ? [..UIUtil.CreateWithShadow(() => CreateFillDreamConditionCheckText(isClearCond),shadowWidth,Colors.Black)]:[]),
        new TextBlock() { Text = fillDreamCondition.Key }
      ]))
    ]);
    TurnFillDreamConditionPanel.MySetChildren([panel]);
    RescaleTurnFillDreamConditionPanelUI(UIUtil.GetScaleFactor(MainGrid.RenderSize) * GetZoomFactor());
    await Task.Run(async () => {
      await Task.Delay(startAnimationDelay);
      await Enumerable.Range(1,transparentFrameCount).MyAsyncForEachSequential(async v => {
        double nextWaitSeconds = UIUtil.nextStepDelaySeconds * v - (DateTime.Now - startTime).TotalSeconds;
        if(nextWaitSeconds <= 0) return;
        await Task.Delay(TimeSpan.FromSeconds(nextWaitSeconds));
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => panel.Opacity = 1 - (double)v / transparentFrameCount);
      });
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => TurnFillDreamConditionPanel.Children.Remove(panel));
    });
    static TextBlock CreateFillDreamConditionCheckText(bool isClear) => new() { Text = isClear ? "✓" : "✗",Foreground = isClear ? Colors.Green : Colors.Red };
  }
  private void ShowMessage(string[] messages) {
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),
      Height = messages.Length * BasicStyle.textHeight,
      IsHitTestVisible = false
    }.MySetChildren([.. messages.Select(logText => new TextBlock() { Text = logText })]).MyApply(async elem => {
      elem.Opacity = 1;
      await Task.Delay(6000);
      await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => { elem.Opacity = 1 - v; });
        await Task.Delay(15);
      });
      TurnLogPanel.Children.Remove(elem);
      ResizeTurnLogUI();
    });
    TurnLogPanel.Children.Add(panel);
    ResizeTurnLogUI();
    void ResizeTurnLogUI() => TurnLogPanel.Height = TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * UIUtil.GetScaleFactor(MainGrid.RenderSize) * GetZoomFactor();
  }
  private static double GetZoomFactor() => Math.Pow(1.1,zoomLevel);
  private GameState PutPersonPanel(GameState game) {
    if(pick != null) {
      GameState newGameState = game.MyPipe(SwapPerson).MyPipe(PutPerson);
      MovePersonCanvas.MySetChildren([]);
      pick = null;
      UpdateCountryInfoPanel(newGameState);
      return newGameState;
    } else {
      return game;
    }
    GameState SwapPerson(GameState game) {
      KeyValuePair<PersonId,PersonData>? maybeDestPersonInfo = game.PersonMap.MyNullable().FirstOrDefault(v => Person.GetPersonCountry(game,v?.Key ?? new(string.Empty)) == game.PlayCountry && v?.Value.Post == pointerover?.post);
      return UpdateGame.PutPersonFromUI(game,maybeDestPersonInfo?.Key,pick?.person.MyPipe(game.PersonMap.GetValueOrDefault)?.Post).MyApply(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == maybeDestPersonInfo?.Key)?.MyPipe(destPersonInfo => pick?.panel.MySetChildren([CreatePersonPanel(game,destPersonInfo)])));
    }
    GameState PutPerson(GameState game) {
      return UpdateGame.PutPersonFromUI(game,pick?.person,pointerover?.post ?? pick?.person.MyPipe(game.PersonMap.GetValueOrDefault)?.Post).MyApply(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == pick?.person)?.MyPipe(putPersonInfo => (pointerover?.panel ?? pick?.panel)?.MySetChildren([CreatePersonPanel(game,putPersonInfo)])));
    }
  }
  private void UpdateCountryPostPanelZIndex(ERole toActiveRole) {
    if(activePanelRole == countryPostPanelMap.MyNullable().FirstOrDefault(v => v?.Value == CountryPostsPanel.Children.LastOrDefault() as Grid)?.Key) {
      List<Grid> resetZIndexPanels = GetResetZIndexPanels((activePanelRole, toActiveRole) switch {
        (ERole.Central, ERole.Affair) => [ERole.Affair],
        (ERole.Central, ERole.Defense) => [ERole.Affair,ERole.Defense],
        (ERole.Central, ERole.Attack) => [ERole.Affair,ERole.Defense,ERole.Attack],
        (ERole.Affair, ERole.Central) => [ERole.Central],
        (ERole.Affair, ERole.Defense) => [ERole.Defense],
        (ERole.Affair, ERole.Attack) => [ERole.Defense,ERole.Attack],
        (ERole.Defense, ERole.Central) => [ERole.Affair,ERole.Central],
        (ERole.Defense, ERole.Affair) => [ERole.Affair],
        (ERole.Defense, ERole.Attack) => [ERole.Attack],
        (ERole.Attack, ERole.Central) => [ERole.Defense,ERole.Affair,ERole.Central],
        (ERole.Attack, ERole.Affair) => [ERole.Defense,ERole.Affair],
        (ERole.Attack, ERole.Defense) => [ERole.Defense],
        _ => []
      });
      if(resetZIndexPanels.Count != 0) {
        resetZIndexPanels.ToList().ForEach(v => {
          CountryPostsPanel.Children.Remove(v);
          CountryPostsPanel.Children.Add(v);
        });
        activePanelRole = toActiveRole;       
      }
    }
    static List<Grid> GetResetZIndexPanels(ERole[] resetZIndexRoles) => resetZIndexRoles.Select(countryPostPanelMap.GetValueOrDefault).MyNonNull();
  }
  private void ShowCharacterRemark(GameState game) => CharacterRemark.Show(CharacterRemarkPanel,game);
  private static class PushArea {
    private static EArea? pushArea = null;
    internal static void Push(EArea area) => pushArea = area;
    internal static void Exit() => pushArea = null;
    internal static GameState Release(Game page,GameState game,EArea area) {
      ECountry? areaCountry = game.AreaMap.GetValueOrDefault(area)?.Country;
      return pushArea != area ? game : game.Phase == Phase.Starting ? ShowSelectPlayCountryPanel(game,areaCountry) : Area.IsPlayerSelectable(game,area) ? SelectTarget(game,areaCountry != game.PlayCountry && !Country.IsSleep(game,game.PlayCountry) ? area : null) : game;
      GameState ShowSelectPlayCountryPanel(GameState game,ECountry? pushCountry) {
        string title = Text.CountryParamCaptionText(pushCountry);
        List<TextBlock> contents = [
          .. Text.ScenarioCaptionText(game.NowScenario).MyMaybeToList().Select(Make),
          Make(Text.CountryInitInfoCaptionText()),
          Make(Text.CountryCapitalAreaParamText(game, pushCountry)),
          Make(Text.CountryFundParamText(game, pushCountry)),
          Make(Text.CountryAreaNumParamText(game, pushCountry)),
          Make(Text.CountryAffairDifficultParamText(game, pushCountry)),
          Make(Text.CountryAffairPowerParamText(game, pushCountry)),
          Make(Text.CountryTotalAffairParamText(game, pushCountry)),
          Make(Text.CountryInFundParamText(game, pushCountry)),
          Make(Text.CountryOutFundParamText(game, pushCountry)),
          Make(Text.CountryFillDreamConditionCaptionText()),
          .. (pushCountry?.MyPipe(country=>game.NowScenario?.MyPipe(ScenarioBase.GetScenarioData)?.FillDreamConditionMap.GetValueOrDefault(country)?.Messages.MyPipe(v =>new List<string>([..v.Basic??[],..v.Extra??[]])))).MyPipe(v=>v is [] or null?[Text.NoFillDreamConditionText()]:v.Prepend(Text.CountryFillDreamConditionHeadText())).Select(Make),
          Make(Text.CountryInitPersonCaptionText()),
          .. (pushCountry is null?[]:Enum.GetValues<ERole>().SelectMany(role=>Person.GetInitPersonMap(game,pushCountry.Value,role).Keys.OrderBy(v=>Person.GetPersonBirthYear(game,v)).Select(v=>Text.PersonInfoText(game,v)))).MyPipe(v=>v.MyIsEmpty()?[Text.CountryNoExistStartingPersonText()]:v).Select(Make),
        ];
        string okButtonText = Text.SelectCountryButtonText(pushCountry);
        Action? okButtonAction = pushCountry is ECountry.漢 or null ? null : () => GameData.game = ClickOkButtonAction(GameData.game,pushCountry.Value);
        Ask.SetElems(page.AskPanel,title,contents,okButtonText,okButtonAction,true);
        return game;
        static TextBlock Make(string text) => new() { Text = text,HorizontalAlignment = HorizontalAlignment.Center };
        GameState ClickOkButtonAction(GameState game,ECountry playCountry) {
          return SelectPlayCountry(game,playCountry).MyPipe(StartGame).MyPipe(game => UpdateGame.AppendLogMessage(game,[Text.TurnHeadLogText(game)])).MyPipe(game => UpdateGame.AppendStartPlanningRemark(game,[.. Text.StartPlanningCharacterRemarkTexts(game)])).MyApply(page.UpdateCountryInfoPanel).MyApply(page.ShowCharacterRemark);
        }
        GameState SelectPlayCountry(GameState game,ECountry playCountry) => UpdateGame.AttachGameStartData(game,playCountry).MyApply( page.UpdateCountryPosts);
        GameState StartGame(GameState game) {
          UIUtil.SetVisibility(page.CountryPostsPanel,true);
          return (game with { Phase = Phase.Planning }).MyPipe(UpdateGame.AppendGameStartLog).MyApply(page.UpdateAreaPanels).MyApply(GameLog.UpdateLogMessageUI).MyApply(page.UpdateTurnLogUI).MyApply(page.UpdateTurnFillDreamConditionUI).MyPipe(UpdateGame.CalcArmyTarget);
        }
      }
      GameState SelectTarget(GameState game,EArea? area) => game.PlayCountry?.MyPipe(playCountry => game.Phase == Phase.Planning && !Country.IsSleep(game,playCountry) ? (game with { ArmyTargetMap = game.ArmyTargetMap.MyUpdate(playCountry,(_,_) => area) }).MyApply(page.UpdateCountryInfoPanel) : null) ?? game;
    }
  }
}