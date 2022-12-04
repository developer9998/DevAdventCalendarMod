using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevAdventCalandarMod.Scripts
{
    public class Chocolate : MonoBehaviour
    {
        public bool isInHand;
        public bool isGone;
        public bool isInLeftHand;
        public void Update()
        {
            float dist = Vector3.Distance(Plugin.Instance.leftHand.handIndicator.transform.position, transform.position);
            if (dist <= 0.03 && !isInHand && !Plugin.Instance.leftHand.HasObject && !isGone)
            {
                isInHand = true;
                isInLeftHand = true;
                Plugin.Instance.leftHand.HasObject = true;
                gameObject.transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent, false);
                gameObject.transform.localPosition = new Vector3(-0.032f, 0.0527f, 0.0085f);
                gameObject.transform.localRotation = Quaternion.Euler(-8.548f, 10.166f, -97.32401f);
                gameObject.transform.localScale = new Vector3(2.176311f, 2.67192f, 2.67192f);
                GorillaTagger.Instance.StartVibration(true, 0.8f, 0.05f);
            }

            float distRight = Vector3.Distance(Plugin.Instance.rightHand.handIndicator.transform.position, transform.position);
            if (distRight <= 0.03 && !isInHand && !Plugin.Instance.rightHand.HasObject && !isGone)
            {
                isInHand = true;
                isInLeftHand = false;
                Plugin.Instance.rightHand.HasObject = true;
                gameObject.transform.SetParent(GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent, false);
                gameObject.transform.localPosition = new Vector3(0.0277f, 0.0612f, 0.0023f);
                gameObject.transform.localRotation = Quaternion.Euler(-4.447f, -13.739f, -257.429f);
                gameObject.transform.localScale = new Vector3(2.176311f, 2.67192f, 2.67192f);
                GorillaTagger.Instance.StartVibration(false, 0.8f, 0.05f);
            }

            float headDist = Vector3.Distance(GorillaLocomotion.Player.Instance.headCollider.transform.position, transform.position);
            if (headDist <= 0.25f && !isGone && isInHand)
            {
                isGone = true;
                gameObject.GetComponent<Renderer>().enabled = false;
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.chocolateEatSound, 1.7f);
                GorillaTagger.Instance.StartVibration(isInLeftHand, 0.9f, 0.075f);
                if (isInLeftHand) Plugin.Instance.leftHand.HasObject = false;
                else Plugin.Instance.rightHand.HasObject = false;
                Destroy(gameObject, 3);
            }
        }
    }
}
