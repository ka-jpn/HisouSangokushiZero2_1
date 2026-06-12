using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class GameLog:UserControl {
  private static readonly ObservableCollection<string> logMessages = [];
  internal GameLog() {
    InitializeComponent();
  }
    internal static void UpdateLogMessageUI(GameState game) => logMessages.MyApply(v => v.Clear()).MyApply(v => game.LogMessage.ForEach(v.Add));
}