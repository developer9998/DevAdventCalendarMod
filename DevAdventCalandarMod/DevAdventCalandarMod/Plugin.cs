using BepInEx;
using DevAdventCalandarMod.AdventPatches;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DevAdventCalandarMod.Models;
using DevAdventCalandarMod.Scripts;

namespace DevAdventCalandarMod
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; internal set; }
        public GameObject boxObject;
        public Hand leftHand;
        public Hand rightHand;
        public List<Box> boxes = new List<Box>();
        public List<Box> boxesToOpen = new List<Box>();
        public bool Init = false;
        public bool Unlocking = false;
        public AudioClip chocolateEatSound;

        internal void Awake()
        {
            Instance = this;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        [Obsolete]
        internal void OnInitialized()
        {
            /* Code here runs after the game initializes (i.e. GorillaLocomotion.Player.Instance != null) */

            if (DateTime.Now.Month != 12)
            {
                Debug.LogError("DevAdventCalandarMod: It isn't December, therefor the mod will not work.");
                return;
            }

            leftHand = new Hand();
            leftHand.handIndicator = GorillaTagger.Instance.transform.Find("TurnParent/LeftHandTriggerCollider").GetComponent<GorillaTriggerColliderHandIndicator>();

            rightHand = new Hand();
            rightHand.handIndicator = GorillaTagger.Instance.transform.Find("TurnParent/RightHandTriggerCollider").GetComponent<GorillaTriggerColliderHandIndicator>();

            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("DevAdventCalandarMod.Resources.adventbundle");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);

            chocolateEatSound = bundle.LoadAsset<AudioClip>("yum");

            boxObject = Instantiate(bundle.LoadAsset<GameObject>("Calendar"));
            boxObject.transform.position = new Vector3(-64.96f, 11.83f, -84.2f);
            boxObject.transform.rotation = Quaternion.Euler(0f, 75.46f, 0f);
            boxObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            foreach (var collider in boxObject.GetComponentsInChildren<Collider>()) collider.enabled = false;

            int day = DateTime.Now.Day;
            Transform boxesParent = boxObject.transform.Find("TopBoxes");
            for (int i = 0; i < boxesParent.childCount; i++)
            {
                GameObject boxObjectLocal = boxesParent.GetChild(i).gameObject;
                Box boxLocal = new Box();
                boxLocal.BoxObject = boxObjectLocal;

                ParticleSystem sys = boxObjectLocal.GetComponentInChildren<ParticleSystem>();
                sys.gravityModifier = 0.02f;
                sys.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                boxLocal.DoorObject = boxObjectLocal.transform.Find("Door").gameObject;
                boxLocal.BreakSource = boxObjectLocal.GetComponent<AudioSource>();
                boxLocal.ChocolateObject = boxObjectLocal.transform.Find("chocolate").gameObject;
                boxLocal.CurrentNumber = i + 1;
                boxLocal.ParticleSystem = sys;
                boxLocal.Opened = false;
                boxes.Add(boxLocal);
                if (i + 1 < day || i + 1 == day)
                {
                    boxesToOpen.Add(boxLocal);
                }
            }

            Invoke("StartUnlock", 4);
        }

        public void StartUnlock()
        {
            StartCoroutine(Countdown());
        }

        internal IEnumerator Countdown() 
        {
            for (int i = 0; i < boxesToOpen.Count; i++)
            {
                Box boxToOpen = boxesToOpen[i];
                boxToOpen.DoorObject.SetActive(false);
                boxToOpen.ParticleSystem.Play();
                boxToOpen.BreakSource.Play();
                boxToOpen.Opened = true;
                boxToOpen.ChocolateObject.AddComponent<Chocolate>();
                yield return new WaitForSeconds(2);
            }
            yield break;
        }
    }
}
