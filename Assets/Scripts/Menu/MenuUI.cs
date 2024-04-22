using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: UI类2；主要实现Switcher的效果，用于1）主菜单；2）系统菜单
public class MenuUI : MonoBehaviour
{
    public GameObject[] panels;

    // note: bind在1）主菜单；2）系统菜单的那几个按钮上
    public void SwitchPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == index)
            {
                // note: 等于是用这个方式，实现了一个UE中switcher的效果
                panels[i].transform.SetAsLastSibling();//移到Her窗口的最下面
            }
        }
    }
    
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("EXIT GAME");
    }
}
