using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// note: UI��1��û����������ô�ණ������Ҫ�������˵���ʵ������ϵͳ�˵�������
public class UIManager : MonoBehaviour
{
    // note: ���˵�Canvas
    private GameObject menuCanvas;
    // note: ���˵�Prefab������ʵ�������˵�
    public GameObject menuPrefab;

    // note: ���Ͻ�ϵͳ����
    public Button settingsBtn;
    // note: ��ͣ���
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
        // note: �о����Ҳ���Բ�����
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
            // note: �ʣ�ΪʲôҪ��GCһ��
            System.GC.Collect();
            pausePanel.SetActive(true);
            // note: ����ʵ����ͣ��Ϸ
            Time.timeScale = 0;
        }
    }

    // note: Ŀǰû����Ӧ��bind�����˵����˳���ť
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