using UnityEngine;
using DevAdventCalendarMod.Models;

namespace DevAdventCalendarMod.Scripts
{
    public class Chocolate : MonoBehaviour
    {
        public bool isInLeftHand;
        public bool towardsMouth;
        public bool eatItem = false;
        public float closeDist = 0.23f;
        public float farDist = 0.25f;

        public CandyMode candyMode = CandyMode.Intact;
        public CandyEatMode candyEatMode = CandyEatMode.None;

        internal void Start()
        {
            ShowOnlySpecificRenderer(0);
        }

        public void PutInHand(bool isLeftHand, Vector3 position, Vector3 euler, Vector3 scale)
        {
            gameObject.transform.SetParent(isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent : GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = Quaternion.Euler(euler);
            gameObject.transform.localScale = scale;
            GorillaTagger.Instance.StartVibration(isLeftHand, 0.8f, 0.05f);
        }

        public void ShowOnlySpecificRenderer(int op)
        {
            for(int i = 0; i < gameObject.transform.childCount; i++) gameObject.transform.GetChild(i).GetComponent<Renderer>().enabled = i == op;
        }

        public void LateUpdate()
        {
            float dist = Vector3.Distance(Plugin.Instance.leftHand.HandIndicator.transform.position, transform.position);
            if (dist <= 0.03 && !Plugin.Instance.leftHand.HasObject && candyMode == CandyMode.Intact)
            {
                eatItem = true;
                isInLeftHand = true;
                candyMode = CandyMode.Held;
                Plugin.Instance.leftHand.HasObject = true;
                PutInHand(true, new Vector3(-0.07467857f, 0.06751788f, 0.01086549f), new Vector3(4.96279f, 186.4539f, 274.3804f), new Vector3(2.176311f, 2.67192f, -2.67192f));
                int c = Plugin.Instance.GetChocolateFromBox(gameObject).CurrentNumber - 1;
                if (DataLoader.currentData.ChocolatesPickedUp.Count == 0) DataLoader.currentData.ChocolatesPickedUp.Add(c);
                else if (!DataLoader.currentData.ChocolatesPickedUp.Contains(c)) DataLoader.currentData.ChocolatesPickedUp.Add(c);
                DataLoader.SaveData();
            }

            float distRight = Vector3.Distance(Plugin.Instance.rightHand.HandIndicator.transform.position, transform.position);
            if (distRight <= 0.03 && !Plugin.Instance.rightHand.HasObject && candyMode == CandyMode.Intact)
            {
                eatItem = true;
                isInLeftHand = false;
                candyMode = CandyMode.Held;
                Plugin.Instance.rightHand.HasObject = true;
                PutInHand(false, new Vector3(0.07455765f, 0.06690007f, 0.008822776f), new Vector3(4.96279f, 173.546f, 85.61966f), new Vector3(-2.176311f, 2.67192f, -2.67192f));
                int c = Plugin.Instance.GetChocolateFromBox(gameObject).CurrentNumber - 1;
                if (DataLoader.currentData.ChocolatesPickedUp.Count == 0) DataLoader.currentData.ChocolatesPickedUp.Add(c);
                else if (!DataLoader.currentData.ChocolatesPickedUp.Contains(c)) DataLoader.currentData.ChocolatesPickedUp.Add(c);
                DataLoader.SaveData();
            }

            float headDist = Vector3.Distance(GorillaLocomotion.Player.Instance.headCollider.transform.position, transform.position);
            bool lastTM = towardsMouth;
            bool TM = headDist <= closeDist;
            bool TML = headDist <= farDist;

            if (TM && !towardsMouth) towardsMouth = true;
            else if (!TML && towardsMouth) towardsMouth = false;

            if (lastTM != towardsMouth)
            {
                if (towardsMouth)
                {
                    if (eatItem && candyMode == CandyMode.Held && candyMode != CandyMode.Eaten)
                    {
                        eatItem = false;

                        if (candyEatMode == CandyEatMode.None)
                        {
                            candyEatMode = CandyEatMode.Half;

                            PlaySound(0);
                            GorillaTagger.Instance.StartVibration(isInLeftHand, 1f, 0.05f);

                            ShowOnlySpecificRenderer(1);
                        }
                        else if (candyEatMode == CandyEatMode.Half)
                        {
                            if (isInLeftHand) Plugin.Instance.leftHand.HasObject = false;
                            else Plugin.Instance.rightHand.HasObject = false;

                            PlaySound(1);
                            GorillaTagger.Instance.StartVibration(isInLeftHand, 1f, 0.05f);
                            candyMode = CandyMode.Eaten;
                            Plugin.Instance.EatChocolate();

                            for (int i = 0; i < gameObject.transform.childCount; i++) gameObject.transform.GetChild(i).GetComponent<Renderer>().enabled = false;
                            Destroy(gameObject, 1);
                        }
                    }
                }
                else
                {
                    eatItem = true;
                }
            }
        }

        public void PlaySound(int i) => GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.audioClips[i], 1.5f);
    }
}
