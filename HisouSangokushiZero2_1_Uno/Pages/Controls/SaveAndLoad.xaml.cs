using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uno.Extensions.Specialized;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record SaveSlotData(int SlotIndex,string GameVersion,string? NowScenario,string? PlayCountry,string? PlayTurn,string? PlayTime,string? LastSaveDate,string? HasSaveData);
public sealed partial class SaveAndLoad:UserControl {
  internal const double minScaleFactor = 0.65;
  internal const double scrollMaxWidth = UIUtil.fixModeMaxWidth * minScaleFactor;
  private static bool isWritemode = false;
  private static Action<ReadGame?> pressSlotAfterProcess = _ => { };
  private static Action pressCloseProcess = MyUtil.MyUtil.nothing;
  private static List<ReadMeta> saveSlots = [];
  private static List<bool> hasSaveDataList = [];
  internal static readonly ObservableCollection<SaveSlotData> saveSlotTexts = [];
  internal SaveAndLoad() {
    InitializeComponent();
    MyInit(this);
    void MyInit(SaveAndLoad page) {
      page.SizeChanged += (_, _) => ResizeElem(RenderSize);
      CloseButton.Click += (_, _) => pressCloseProcess();
    }
  }
  private static async Task RefreshSaveSlotView() {
    hasSaveDataList = await Storage.GetHasSaveDataList();
    saveSlots = await Storage.ReadMetaDataList();
    saveSlotTexts.Clear();
    Enumerable.Range(0,10).ToList().ForEach(index => {
      (saveSlots.ElementAtOrDefault(index)?.ReadState == ReadState.Read && saveSlots.ElementAtOrDefault(index)?.MaybeMeta is MetaData meta ? new SaveSlotData(
        SlotIndex: index,
        GameVersion: $"ゲームバージョン：{meta.GameVersion}",
        NowScenario: $"シナリオ：{meta.NowScenario?.Value}",
        PlayCountry: $"プレイ国名：{meta.PlayCountry?.ToString() ?? "(選択前)"}",
        PlayTurn: $"ターン数：{meta.PlayTurn?.ToString() ?? "(開始前)"}",
        PlayTime: $"プレイ時間：{Math.Floor(meta.TotalPlayTime.TotalMinutes)}分",
        LastSaveDate: $"最終保存：{meta.LastSaveDate:yyyy/MM/dd HH:mm}",
        HasSaveData: null
      ) : new SaveSlotData(
        SlotIndex: index,
        GameVersion: string.Empty,
        NowScenario: null,
        PlayCountry: null,
        PlayTurn: null,
        PlayTime: null,
        LastSaveDate: null,
        HasSaveData: hasSaveDataList.ElementAtOrDefault(index) ? "セーブデータがありますがここに表示されるメタデータがありません\n(再保存でメタデータが付加されます)" : "セーブデータなし"
      )).MyApply(saveSlotTexts.Add);
    });
  }
  private static int IndexToFileNo(int index) => index + 1;
  internal static async Task Show(SaveAndLoad page,bool isWrite,Action<ReadGame?> afterProcess,Action closeProcess,Windows.Foundation.Size parentSize) {
    isWritemode = isWrite;
    pressSlotAfterProcess = afterProcess;
    pressCloseProcess = closeProcess;
    await RefreshSaveSlotView();
    page.Title.Text = isWrite ? "セーブデータ選択" : "ロードデータ選択";
    page.ResizeElem(parentSize);
  }
  internal void ResizeElem(Windows.Foundation.Size size) {
    double scaleFactor = CookScaleFactor(UIUtil.GetScaleFactor(size with { Height = 0 }));
    double contentWidth = RenderSize.Width / scaleFactor;
    double contentHeight = RenderSize.Height / scaleFactor;
    Content.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    Content.Margin = new(0,0,contentWidth * (scaleFactor - 1),contentHeight * (scaleFactor - 1));
    static double CookScaleFactor(double scaleFactor) => scaleFactor switch { < minScaleFactor => scaleFactor / minScaleFactor, > 1 => scaleFactor, _ => 1 };
  }
  private async void SaveSlot_PointerPressed(object sender,PointerRoutedEventArgs e) {
    await (sender is StackPanel panel && panel.DataContext is SaveSlotData slotData ? PressSaveSlot(slotData) : Task.CompletedTask);
    async Task PressSaveSlot(SaveSlotData slotData) {
      await (isWritemode ? WriteSlot() : ReadSlot());
      async Task WriteSlot(){
        await Storage.WriteStorageData(GameData.game,GameData.startingPlayTotalTime,IndexToFileNo(slotData.SlotIndex));
        await Task.Yield();
        await RefreshSaveSlotView();
        pressSlotAfterProcess(null);
      }
      async Task ReadSlot(){
        if (hasSaveDataList.ElementAtOrDefault(slotData.SlotIndex)) {
          GameData.startingPlayTotalTime = saveSlots.ElementAtOrDefault(slotData.SlotIndex)?.MaybeMeta?.TotalPlayTime ?? TimeSpan.Zero;
          pressSlotAfterProcess(await Storage.ReadStorageData(IndexToFileNo(slotData.SlotIndex)));
        }
      }
    }
  }
}