using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MFarm.Dialogue
{
    // note: Dialogue类3；附着在3个NPC身上
    // note: 1）装填台词dialogueList, dailogueStack；2）检测碰撞，触发NPC聊天；3）pop台词，播委托给UI传递台词，用到2个协程
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        private NPCMovement npc => GetComponent<NPCMovement>();
        public UnityEvent OnFinishEvent;
        // note: 在编辑器里，npc的身上的DialogueController上，直接将多句台词填充好
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();

        private Stack<DialoguePiece> dailogueStack;
        private bool canTalk;
        private bool isTalking;
        // note: uiSign里面有一个ui_sign.controller(animator)，其控制动画状态机，里面有一个ui_spacebar(animation)
        // note: uiSign是一个动画，在这里只是通过canTalk控制了显隐
        private GameObject uiSign;

        private void Awake()
        {
            // note: transform.GetChild(1)是因为parent是npc，然后npc下面有2个，然后uisign是第二个
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = !npc.isMoving && npc.interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = false;
            }
        }

        private void Update()
        {
            // note: 这看起来有2种实现方式。1）直接在OnTrigger里进行uisign的开关；
            // note: 2）像这样，只在OnTrigger里设置状态，然后在Tick里进行状态检测，然后设置uisign的每帧变化。
            // note: 似乎1）更好。
            uiSign.SetActive(canTalk);

            if (canTalk & Input.GetKeyDown(KeyCode.Space) && !isTalking)
            {
                StartCoroutine(DailogueRoutine());
            }
        }

        /// <summary>
        /// 构建对话堆栈
        /// </summary>
        private void FillDialogueStack()//倒叙，压栈
        {
            dailogueStack = new Stack<DialoguePiece>();
            // note: 先push第二句，再push第一句，出栈的时候就先pop第一句
            // note: 那为什么不直接用数组？可能是因为已经说过的台词，就不需要再存储
            // note: 但如果是想要玩家重新浏览台词，用这样的方式pop掉，可能就不太合适。但即使栈丢弃了，dialogueList还是始终存着
            for (int i = dialogueList.Count - 1; i > -1; i--)
            {
                dialogueList[i].isDone = false;
                dailogueStack.Push(dialogueList[i]);
            }
        }

        private IEnumerator DailogueRoutine()
        {
            isTalking = true;
            if (dailogueStack.TryPop(out DialoguePiece result))
            {
                //传到UI显示对话
                EventHandler.CallShowDialogueEvent(result);
                // note: 时间暂停
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                // note: 协程的又一种用法，waituntil()，等到对话结束再继续执行
                // note: 如果是线程的角度，这里共有3个线程。1）主线程；2）DailogueRoutine当前函数的线程；3）UI的ShowDialogue的线程。
                // note: 思考：为什么这里需要3个线程？首先3）UI那台词展示需要1s，所以肯定需要协程。而2）这里又要等3）结束，所以也需要协程。
                // note: 所以协程的关键等待，可以从yield那句看出来。
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
            }
            else
            {
                // note: 最后一句后，没词了
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                EventHandler.CallShowDialogueEvent(null);
                // note: 神来之笔，没词了，UI关闭了，又会重新把词push进去。所以这就看出来用stack的巧妙了
                // note: 这就是重新再找NPC聊天，它又有词的原因
                // note: 但通常，这里会重新push不同的台词
                FillDialogueStack();
                isTalking = false;

                // note: OnFinishEvent作为保留没开启，如果不想让NPC能再次聊天，可以用这个实现
                if (OnFinishEvent != null)
                {
                    OnFinishEvent.Invoke();
                    canTalk = false;
                }
            }
        }
    }
}
