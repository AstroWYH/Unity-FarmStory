using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MFarm.Save
{
    // note: 存档类2，MVC的C，业务逻辑（存档）管理类，实现了Save(), Load(), Register()等一系列方法
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        // note: 全部需要注册的模块
        private List<ISaveable> saveableList = new List<ISaveable>();

        // note: MVC的M，Json存档数据的内存表现
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
            // note: 新游戏赋值；index是存档的索引[1、2、3]
            currentDataIndex = index;
        }

        // note: 每个模块各自在其Start()里将自己注册进来
        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
                saveableList.Add(saveable);
        }

        // note: Awake()调用，从Json里把3个存档都取出来，存到dataSlots[3]中
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


        // note: 存档，目前看起来只在按I时调用
        private void Save(int index)
        {
            DataSlot data = new DataSlot();

            foreach (var saveable in saveableList)
            {
                // note: 这看起来有优化的空间，因为每个模块都返回自己的GameSaveData，但只利用了一小部分GameSaveData
                // note: 可能用一个base类统一表示，各模块子类分别添加自己的字段更合理
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
            // note: 加载已有存档赋值
            currentDataIndex = index;

            var resultPath = jsonFolder + "data" + index + ".json";

            var stringData = File.ReadAllText(resultPath);

            var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);

            // note: 通过接口ISaveable，把每个实现ISaveable接口的模块的值，从Json里重新恢复一下
            foreach (var saveable in saveableList)
            {
                saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
            }
            // note: 这可能有点问题，这里没把从Json读到的DataSlot存到dataSlots[3]中
        }
    }
}
