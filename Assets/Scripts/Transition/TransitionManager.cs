using System.Collections;
using System.Collections.Generic;
using MFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFarm.Transition
{
    // note: 传送类2；1）监听Teleport触发盒子的事件，负责过渡渐变的功能；会播老场景卸载前、新场景加载后等委托
    // note: 2）负责新游戏启动、Load读档加载、游戏结束退出的过渡渐变，会播老场景卸载前、新场景加载后等委托
    // note: 3）老场景卸载前、新场景加载后，这2个协议很重要，很多模块都会注册
    public class TransitionManager : Singleton<TransitionManager>, ISaveable
    {
        [SceneName]
        public string startSceneName = string.Empty;
        private CanvasGroup fadeCanvasGroup;
        private bool isFade;

        public string GUID => GetComponent<DataGUID>().guid;

        protected override void Awake()
        {
            base.Awake();
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed, 0);

            // note: 为什么是这个类来加载UI？
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }

        private void OnTransitionEvent(string sceneToGo, Vector3 positionToGo)
        {
            if (!isFade)
                StartCoroutine(Transition(sceneToGo, positionToGo));
        }

        /// <summary>
        /// 场景切换
        /// </summary>
        /// <param name="sceneName">目标场景</param>
        /// <param name="targetPosition">目标位置</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            // note: 逐渐变黑；暂停，直到Fade()执行完再继续
            yield return Fade(1);

            // note: 暂停，直到UnloadSceneAsync卸载完再继续
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            yield return LoadSceneSetActive(sceneName);
            //移动人物坐标
            // note: 只有Player注册；看起来这里可以优化，这个委托可能不需要，可以直接用CallAfterSceneLoadedEvent()一起来实现
            EventHandler.CallMoveToPosition(targetPosition);
            EventHandler.CallAfterSceneLoadedEvent();
            // note: 逐渐变亮；暂停，直到Fade()执行完再继续
            yield return Fade(0);
        }

        /// <summary>
        /// 加载场景并设置为激活
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            SceneManager.SetActiveScene(newScene);
        }

        /// <summary>
        /// 淡入淡出场景
        /// </summary>
        /// <param name="targetAlpha">1是黑，0是透明</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;

            fadeCanvasGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            fadeCanvasGroup.blocksRaycasts = false;

            isFade = false;
        }

        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1f);

            // note: 1)走这个if是在游戏过程中，加载存档调用Load，然后调RestoreData；2)不走则是新开游戏
            if (SceneManager.GetActiveScene().name != "PersistentScene")    //在游戏过程中 加载另外游戏进度
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }

        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }
}