namespace MFarm.Save
{
    // note: �浵��1��ÿ����Ҫ�浵��ģ�飬����Ҫʵ������ӿ�
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