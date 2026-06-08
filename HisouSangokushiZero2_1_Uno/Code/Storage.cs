using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.MyUtil;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Storage {
  private static string CreateMetaFileName(int fileNo) => $"save{fileNo}.meta";
  private static string CreateSaveFileName(int fileNo) => $"save{fileNo}.dat";
  private static void Write(string folderName, string fileName, byte[] writeData) => File.WriteAllBytes(Path.Combine(folderName, fileName), writeData);
  private static StorageFolder GetStorageFolder() => ApplicationData.Current.LocalFolder;
  private static async Task<List<StorageFile>> GetFilesAsync(StorageFolder folder) => [.. await folder.GetFilesAsync()];
  private static bool ExistsFile(string folderName, string fileName) => File.Exists(Path.Combine(folderName, fileName));
  private static byte[] Read(string folderName, string fileName) => File.ReadAllBytes(Path.Combine(folderName, fileName));
  internal static async Task WriteStorageData(GameState game,TimeSpan startingPlayTotalTime,int fileNo) {
    TimeSpan totalPlayTime = DateTime.Now - GameData.startGameDateTime + startingPlayTotalTime;
    DateTime lastSaveTime = DateTime.Now + new TimeSpan(9, 0, 0);
    MetaData metaData = new(game.NowScenario, game.PlayCountry, game.PlayTurn, totalPlayTime, lastSaveTime, fileNo, BaseData.version.Value);
    GetStorageFolder().MyApply(myFolder => Write(myFolder.Path,CreateMetaFileName(fileNo), MessagePackSerializer.Serialize(metaData)));
    await Task.Yield();
    GetStorageFolder().MyApply(myFolder => Write(myFolder.Path,CreateSaveFileName(fileNo), MessagePackSerializer.Serialize(game)));
  }
  internal static async Task<ReadGame> ReadStorageData(int fileNo) {
    bool newSaveFileExists = GetStorageFolder().MyPipe(myFolder => ExistsFile(myFolder.Path,CreateSaveFileName(fileNo)));
    bool oldSaveFileExists = GetStorageFolder().MyPipe(myFolder => ExistsFile(Path.Combine(myFolder.Path, BaseData.name.Value), CreateSaveFileName(fileNo)));
    return newSaveFileExists ? new(ReadState.Read, MessagePackSerializer.Deserialize<GameState?>(Read(GetStorageFolder().Path, CreateSaveFileName(fileNo)))?.MyPipe(FillState)) :
      oldSaveFileExists ? new(ReadState.Read, MessagePackSerializer.Deserialize<GameState?>(Read(Path.Combine(GetStorageFolder().Path, BaseData.name.Value), CreateSaveFileName(fileNo)))?.MyPipe(FillState)) :
      new(ReadState.NotFind, null);
    static GameState FillState(GameState game) => game with {
      LogMessage = game.LogMessage ?? [],
      StartPlanningCharacterRemark = game.StartPlanningCharacterRemark ?? [],
      StartExecutionCharacterRemark = game.StartExecutionCharacterRemark ?? []
    };
  }
  internal static async Task DeleteStorageData(int fileNo) {
    File.Delete(Path.Combine(GetStorageFolder().Path, BaseData.name.Value, CreateSaveFileName(fileNo)));
    File.Delete(Path.Combine(GetStorageFolder().Path, CreateMetaFileName(fileNo)));
    File.Delete(Path.Combine(GetStorageFolder().Path, CreateSaveFileName(fileNo)));
  }
  internal static async Task<List<ReadMeta>> ReadMetaDataList() {
    List<StorageFile> newMetaFiles = await GetStorageFolder().MyPipe(GetFilesAsync);
    List<StorageFile?> slotMetaFiles = [.. Enumerable.Range(1, 10).Select(fileNo => newMetaFiles.FirstOrDefault(v => v.Name == CreateMetaFileName(fileNo)))];
    List<byte[]?> metaRawDatas = [.. slotMetaFiles.Select(slotMetaFile => slotMetaFile?.MyPipe(v=> Read(GetStorageFolder().Path,v.Path)))];
    List<MetaData?> metaDatas = [.. metaRawDatas.Select(metaRawData => metaRawData?.MyPipe(v => MessagePackSerializer.Deserialize<MetaData?>(v)))];
    return [.. metaDatas.Select(maybeMetaData => maybeMetaData?.MyPipe(v => new ReadMeta(ReadState.Read, v)) ?? new ReadMeta(ReadState.NotFind, null))];
  }
  internal static async Task<List<bool>> GetHasSaveDataList() {
    List<StorageFile> newSaveFiles = await GetStorageFolder().MyPipe(GetFilesAsync);
    List<StorageFile> oldSaveFiles = Directory.Exists(Path.Combine(GetStorageFolder().Path, BaseData.name.Value)) ? await (await GetStorageFolder().GetFolderAsync(BaseData.name.Value)).MyPipe(GetFilesAsync) : [];
    List<List<StorageFile>> saveFiles = [newSaveFiles, oldSaveFiles];
    return [.. Enumerable.Range(1, 10).Select(fileNo => saveFiles.Any(v => v.FirstOrDefault(v => v.Name == CreateSaveFileName(fileNo)) is { }))];
  }
}