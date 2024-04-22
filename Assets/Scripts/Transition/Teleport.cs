using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Transition
{
    // note: 传送类1，用于放置在场景中，相当于触发盒子的作用
    public class Teleport : MonoBehaviour
    {
        public string sceneToGo;
        public Vector3 positionToGo;//坐标

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))//判断是否是玩家
            {
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);
            }
        }
    }
}
