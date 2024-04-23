using UnityEngine;
using UnityEngine.Events;

// note: Dialogue类2；每条对话内容的数据结构，主要起传递作用
// note: 序列化到编辑器，使编辑器中，NPC身上可以直接编辑台词
namespace MFarm.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;
        [TextArea]
        public string dialogueText;
        public bool hasToPause;
        [HideInInspector] public bool isDone;
    }
}