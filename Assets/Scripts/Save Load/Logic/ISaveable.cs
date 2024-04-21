namespace MFarm.Save
{
    // note: 存档类1，每个想要存档的模块，都需要实现这个接口
    public interface ISaveable
    {
        string GUID { get; }
        
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        GameSaveData GenerateSaveData();
        void RestoreData(GameSaveData saveData);
    }
}