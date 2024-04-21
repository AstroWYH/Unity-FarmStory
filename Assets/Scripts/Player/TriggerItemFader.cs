using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItemFader : MonoBehaviour
{
   
   private void OnTriggerEnter2D(Collider2D other)
   {
     ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();///找到所有子物体

     if (faders.Length > 0)
     {
        foreach (var item in faders)
        {
            item.FadeOut();
        }
     }
   }

   private void OnTriggerExit2D(Collider2D other)
   {
     ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();

      if (faders.Length > 0)
      {
        foreach (var item in faders)
        {
            item.FadeIn();
        }
      }
   }
}
