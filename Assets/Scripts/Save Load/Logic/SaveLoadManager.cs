using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MFarm.Save
{
    // note: �浵��2��MVC��C��ҵ���߼����浵�������࣬ʵ����Save(), Load(), Register()��һϵ�з���
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        // note: ȫ����Ҫע���ģ��
        private List<ISaveable> saveableList = new List<ISaveable>();

        // note: MVC��M��Json�浵���ݵ��ڴ����
        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        private string jsonFolder;
        private int currentDataIndex;

        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
            ReadSaveData();
        }
        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                Save(currentDataIndex);
            if (Input.GetKeyDown(KeyCode.O))
                Load(currentDataIndex);
        }


        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }
        private void OnStartNewGameEvent(int index)
        {
            // note: ����Ϸ��ֵ��index�Ǵ浵������[1��2��3]
            currentDataIndex = index;
        }

        // note: ÿ��ģ���������Start()�ｫ�Լ�ע�����
        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
                saveableList.Add(saveable);
        }

        // note: Awake()���ã���Json���3���浵��ȡ�������浽dataSlots[3]��
        private void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < dataSlots.Count; i++)
                {
                    var resultPath = jsonFolder + "data" + i + ".json";
                    if (File.Exists(resultPath))
                    {
                        var stringData = File.ReadAllText(resultPath);
                        var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }


        // note: �浵��Ŀǰ������ֻ�ڰ�Iʱ����
        private void Save(int index)
        {
            DataSlot data = new DataSlot();

            foreach (var saveable in saveableList)
            {
                // note: �⿴�������Ż��Ŀռ䣬��Ϊÿ��ģ�鶼�����Լ���GameSaveData����ֻ������һС����GameSaveData
                // note: ������һ��base��ͳһ��ʾ����ģ������ֱ�����Լ����ֶθ�����
                data.dataDict.Add(saveable.GUID, saveable.GenerateSaveData());
            }
            dataSlots[index] = data;

            var resultPath = jsonFolder + "data" + index + ".json";

            var jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);

            if (!File.Exists(resultPath))
            {
                Directory.CreateDirectory(jsonFolder);
            }
            Debug.Log("DATA" + index + "SAVED!");
            File.WriteAllText(resultPath, jsonData);
        }

        public void Load(int index)
        {
            // note: �������д浵��ֵ
            currentDataIndex = index;

            var resultPath = jsonFolder + "data" + index + ".json";

            var stringData = File.ReadAllText(resultPath);

            var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);

            // note: ͨ���ӿ�ISaveable����ÿ��ʵ��ISaveable�ӿڵ�ģ���ֵ����Json�����»ָ�һ��
            foreach (var saveable in saveableList)
            {
                saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
            }
            // note: ������е����⣬����û�Ѵ�Json������DataSlot�浽dataSlots[3]��
        }
    }
}
