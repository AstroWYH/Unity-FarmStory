using System.Collections;
using System.Collections.Generic;
using MFarm.AStar;
using System;
using MFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

// note: NPC类2；挂在每个NPC身上；
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour, ISaveable
{
    // note: 序列化NPC日程
    public ScheduleDataList_SO scheduleData;
    private SortedSet<ScheduleDetails> scheduleSet;
    // note: 虽然但是，这个currentSchedule纯没起作用
    private ScheduleDetails currentSchedule;

    //临时存储信息
    // note: 编辑器赋予，这2个主要和Astar寻路相关
    public string currentScene;
    private string targetScene;
    // note: 这2个也主要和Astar寻路相关，和网格有关
    private Vector3Int currentGridPosition;
    // note: 从编辑器npc身上来的，girl1 girl2 老人分别可能有1-2个schedule，这是目的地
    private Vector3Int tragetGridPosition;
    // note: 看起来只是NPC要走的下一个格子，但还没搞清楚girdpos和worldpos的明显区别，debug看好像有点像
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;

    // note: 编辑器NPC身上没找到，也无其他引用，暂作用不明
    public string StartScene { set => currentScene = value; }

    [Header("移动属性")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;
    // note: 下一步和当前步的方向；并会设到动画里，决定npc的朝向动画
    private Vector2 dir;
    // note: 没走到终点tragetGridPosition，就为true
    public bool isMoving;

    //Components
    // note: 控制移动
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    // note: 通过一些属性值，设置动画表现；和animOverride关联，把活细分给animOverride
    private Animator anim;
    // note: 可能是找的persistent scene里的grid，未启用，test用；主要用来坐标转换
    private Grid gird;
    // note: npc具体要走的步子
    private Stack<MovementStep> movementSteps;
    // note: 可能不用这个，也行；比如就返回那个IExxx
    private Coroutine npcMoveRoutine;

    private bool isInitialised;
    // note: 走一格的过程中为true
    private bool npcMove;
    private bool sceneLoaded;
    // note: 编辑器设的，在对话那有用
    public bool interactable;
    public bool isFirstLoad;
    private Season currentSeason;
    //动画计时器
    private float animationBreakTime;
    private bool canPlayStopAnimaiton;
    // note: 目前看起来没起作用
    private AnimationClip stopAnimationClip;
    public AnimationClip blankAnimationClip;
    // note: 没特别看懂，从编辑器npc的animator看是细分anim，但又好像不是；从代码看是给blank和stop替换动画，但似乎没起作用
    private AnimatorOverrideController animOverride;

    // note: 把时分合在一起，方便
    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();

        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
        scheduleSet = new SortedSet<ScheduleDetails>();

        // note: 序列化日程数据内存化
        foreach (var schedule in scheduleData.scheduleList)
        {
            scheduleSet.Add(schedule);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void Update()
    {
        if (sceneLoaded)
            SwitchAnimation();

        //计时器
        animationBreakTime -= Time.deltaTime;
        canPlayStopAnimaiton = animationBreakTime <= 0;
    }

    private void FixedUpdate()
    {
        if (sceneLoaded)
            Movement();
    }
    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        if (npcMoveRoutine != null)
            StopCoroutine(npcMoveRoutine);
    }

    private void OnStartNewGameEvent(int obj)
    {
        //isInitialised = false;
        isFirstLoad = true;
    }

    // note: 到点NPC就干活，走路
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        // note: 避免hour和minute分开判断麻烦，所以写在一起
        int time = (hour * 100) + minute;
        currentSeason = season;

        ScheduleDetails matchSchedule = null;
        foreach (var schedule in scheduleSet)
        {
            if (schedule.Time == time)
            {
                if (schedule.day != day && schedule.day != 0)
                    continue;
                if (schedule.season != season)
                    continue;
                // note: NPC的活匹配上了；初步看起来，NPC的活就是在指定时间走到指定位置，然后做个动作
                matchSchedule = schedule;
            }
            else if (schedule.Time > time)
            {
                // note: 这个活的时间还没到，继续看下个
                break;
            }
        }
        if (matchSchedule != null)
            BuildPath(matchSchedule);
    }

    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        gird = FindObjectOfType<Grid>();
        CheckVisiable();

        if (!isInitialised)
        {
            InitNPC();
            isInitialised = true;
        }

        sceneLoaded = true;

        // note: 不是第一次加载游戏，才走？
        if (!isFirstLoad)
        {
            currentGridPosition = gird.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int)tragetGridPosition, stopAnimationClip, interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }

    private void CheckVisiable()
    {
        if (currentScene == SceneManager.GetActiveScene().name)
            SetActiveInScene();
        else
            SetInactiveInScene();
    }

    // note: 目前没看到太大作用，后面再看
    private void InitNPC()
    {
        targetScene = currentScene;

        //保持在当前坐标的网格中心点
        // note: 这俩值，转换前后都是-6, -10, 0
        currentGridPosition = gird.WorldToCell(transform.position);
        // note: -5.50, -9.50, 0.00
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        tragetGridPosition = currentGridPosition;
    }

    /// <summary>
    /// 主要移动方法
    /// </summary>
    // note: 具体每帧检测是否需要npc移动
    private void Movement()
    {
        if (!npcMove) // note: 如果npc在移动，就什么都不做
        {
            if (movementSteps.Count > 0) // note: 如果有步子stack可以动，就移动；一个步子step是一格
            {
                MovementStep step = movementSteps.Pop();

                currentScene = step.sceneName;

                CheckVisiable();

                nextGridPosition = (Vector3Int)step.gridCoordinate;
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(nextGridPosition, stepTime);
            }
            else if (!isMoving && canPlayStopAnimaiton) // note: 如果没步子stack可以动，就停下来跳舞
            {
                // note: 看起来只做了一个强制面向镜头的动作；并没有停止动画
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    // note: 因为移动耗时，又不能阻碍主线程，所以用协程
    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    // note: 一次就移一个格子
    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPostion(gridPos);

        //还有时间用来移动
        if (stepTime > GameTime)
        {
            //用来移动的时间差，以秒为单位
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //实际移动距离
            float distance = Vector3.Distance(transform.position, nextWorldPosition);
            //实际移动速度
            float speed = Mathf.Max(minSpeed, (distance / timeToMove / Settings.secondThreshold));

            if (speed <= maxSpeed)
            {
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    dir = (nextWorldPosition - transform.position).normalized;

                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        //如果时间已经到了就瞬移
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;

        npcMove = false;
    }

    /// <summary>
    /// 根据Schedule构建路径
    /// </summary>
    /// <param name="schedule"></param>
    // note: 到点时调用，AStar构建寻路；主要是取schedule，然后构建movementSteps具体要走的步子
    public void BuildPath(ScheduleDetails schedule)
    {
        // note: 更新当前的Schedule，NPC准备干活去（走路）
        movementSteps.Clear();
        currentSchedule = schedule;
        targetScene = schedule.targetScene;
        // note: 在这取的，从编辑器npc身上来的，girl1 girl2 老人分别可能有1-2个schedule
        tragetGridPosition = (Vector3Int)schedule.targetGridPosition;
        // note: 感觉这一段有问题，赋值过去又赋值回来
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;

        // note: 原地图寻路
        if (schedule.targetScene == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementSteps);
        }
        else if (schedule.targetScene != currentScene) // note: 跨地图寻路
        {
            //这个东西得加上,从哪来去到哪
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if (sceneRoute != null)
            {
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];

                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        // note: 第二段路，从黑暗走向目标
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    else
                    {
                        fromPos = path.fromGridCell;
                    }

                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                    {
                        // note: 第一段路，从出发点走向黑暗
                        gotoPos = schedule.targetGridPosition;
                    }
                    else
                    {
                        gotoPos = path.gotoGridCell;
                    }

                    // note: 会寻路2次；主要会给movementSteps填充sceneName和gridCoordinate
                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementSteps);
                }
            }
        }

        if (movementSteps.Count > 1)
        {
            //更新每一步对应的时间戳
            UpdateTimeOnPath();
        }
    }

    /// <summary>
    /// 更新路径上每一步的时间
    /// </summary>
    // note: 完善movementStep的细节，把每个步子应该走的时间填上
    private void UpdateTimeOnPath()
    {
        MovementStep previousSetp = null;

        TimeSpan currentGameTime = GameTime;

        foreach (MovementStep step in movementSteps)
        {
            if (previousSetp == null)
                previousSetp = step;

            // note: 给每一步MovementStep填充时间，这一步的时间是多少，下一步的时间又是多少
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime;

            // note: 还能斜着走？这看着只是把斜着走的时间做了特殊处理，时间*1.41
            if (MoveInDiagonal(step, previousSetp))
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            else
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

            //累加获得下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
            //循环下一步
            previousSetp = step;
        }
    }

    /// <summary>
    /// 判断是否走斜方向
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="previousStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }

    /// <summary>
    /// 网格坐标返回世界坐标中心点
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    private Vector3 GetWorldPostion(Vector3Int gridPos)
    {
        Vector3 worldPos = gird.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2);
    }

    // note: 每帧都要控制npc朝向
    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPostion(tragetGridPosition);

        // note: 控制walk或idle的动画
        anim.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX", dir.x);
            anim.SetFloat("DirY", dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }
    }

    private IEnumerator SetStopAnimation()
    {
        //强制面向镜头
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime = Settings.animationBreakTime;
        if (stopAnimationClip != null)
        {
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);
            // note: gpt说这是设置了然后等下一帧生效，然后就可以关闭设置了
            yield return null;
            anim.SetBool("EventAnimation", false);
        }
        else
        {
            animOverride[stopAnimationClip] = blankAnimationClip;
            anim.SetBool("EventAnimation", false);
        }
    }

    #region 设置NPC显示情况
    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;

        // note: 盲猜这个是NPC阴影
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;

        transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(tragetGridPosition));
        saveData.characterPosDict.Add("currentPosition", new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this.targetScene;
        if (stopAnimationClip != null)
        {
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
        }
        saveData.interactable = this.interactable;//是否可以互动
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        isInitialised = true;
        isFirstLoad = false;

        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;

        Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        tragetGridPosition = gridPos;

        if (saveData.animationInstanceID != 0)
        {
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }

}
