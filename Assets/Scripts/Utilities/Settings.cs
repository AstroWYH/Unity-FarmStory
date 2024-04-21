using System;
using UnityEngine;

public class Settings : MonoBehaviour
{
    
    public const float itemFadeDuration = 0.35f;

    public const float targetAlpha = 0.45f;     

    //时间相关
    public const float secondThreshold = 0.1f;    //数值越小时间越快
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;    //30天一个月
    public const int seasonHold = 3;

    //Transition
    public const float fadeDuration = 0.8f;

     //割草数量限制
    public const int reapAmount = 2;

     //NPC网格移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f;  //像素比例20*20
    public const float animationBreakTime = 5f;//动画间隔五秒
    public const int maxGridSize = 9999;//最大网格尺寸
    
    //灯光
    public const float lightChangeDuration = 25f;//切换的过渡时间
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);

    public static Vector3 playerStartPos = new Vector3(5f, -18f, 0);
    public const int playerStartMoney = 100;
}

