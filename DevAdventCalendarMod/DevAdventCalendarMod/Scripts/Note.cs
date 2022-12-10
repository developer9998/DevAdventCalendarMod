using System;
using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Text;
using UnityEngine;

namespace DevAdventCalendarMod.Scripts
{
    public class Note : MonoBehaviour
    {
        public float adjustor = 110;
        public List<Transform> noteRigList = new List<Transform>();

        public void Start()
        {
            Transform origin = gameObject.transform.Find("Armature");
            Transform lastObject = null;
            for (int i = 0; i < 8; i++)
            {
                if (lastObject == null) lastObject = origin.GetChild(0);
                else lastObject = lastObject.GetChild(0);

                noteRigList.Add(lastObject);
            }

            StartCoroutine(AnimationLoop());
        }

        IEnumerator AnimationLoop()
        {
            while (true)
            {
                foreach (var transformObject in noteRigList)
                {
                    transformObject.transform.localRotation = Quaternion.Lerp(transformObject.transform.localRotation, Quaternion.Euler(adjustor, 0, 0), 0.03f);
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
