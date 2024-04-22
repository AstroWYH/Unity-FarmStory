using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// note: UI类1；没有想象中那么多东西，主要负责主菜单的实例化、系统菜单的显隐
public class UIManager : MonoBehaviour
{
    // note: 主菜单Canvas
    private GameObject menuCanvas;
    // note: 主菜单Prefab，用于实例化主菜单
    public GameObject menuPrefab;

    // note: 右上角系统设置
    public Button settingsBtn;
    // note: 暂停面板
    public GameObject pausePanel;
    public Slider volumeSlider;


    private void Awake()
    {
        settingsBtn.onClick.AddListener(TogglePausePanel);
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void Start()
    {
        menuCanvas = GameObject.FindWithTag("MenuCanvas");
        Instantiate(menuPrefab, menuCanvas.transform);
    }
    private void OnAfterSceneLoadedEvent()
    {
        // note: 感觉这个也可以不销毁
        if (menuCanvas.transform.childCount > 0)
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
    }

    public void TogglePausePanel()
    {
        bool isOpen = pausePanel.activeInHierarchy;

        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            // note: 问，为什么要先GC一下
            System.GC.Collect();
            pausePanel.SetActive(true);
            // note: 可以实现暂停游戏
            Time.timeScale = 0;
        }
    }

    // note: 目前没调；应该bind在主菜单的退出按钮
    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        Instantiate(menuPrefab, menuCanvas.transform);
    }
}