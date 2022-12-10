using BepInEx;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GorillaNetworking;
using DevAdventCalendarMod.Models;
using DevAdventCalendarMod.Scripts;
using DevAdventCalendarMod.Patches;

namespace DevAdventCalendarMod
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; internal set; }

        /* Models */
        public Hand leftHand;
        public Hand rightHand;
        public List<Box> boxes = new List<Box>();
        public List<Box> boxesToOpen = new List<Box>();
        public GiveoutMode mode = GiveoutMode.None;
        public Transform endingBoxObject;
        public Note noteComponent;

        public bool Unlocking;
        public bool Initialized;
        public bool FinishedChocolate = true;
        public Chocolate DevChocolate;
        public int chocolatesEaten = 0;

        public GameObject boxObject;
        public List<AudioClip> audioClips = new List<AudioClip>();

        public Dictionary<string, Vector3[]> locations = new Dictionary<string, Vector3[]>()
        {
            { "JoinPublicRoom - City Back",             new Vector3[] { new Vector3(-64.9369f, 11.83f, -84.3542f), new Vector3(0f, 74.175f, 0f) } },
            { "JoinPublicRoom - Mountain For Computer", new Vector3[] { new Vector3(-25.075f, 17.813f, -94.35f),   new Vector3(0, -40.829f, 0)  } }
        };

        internal void Awake()
        {
            Instance = this;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        internal void OnNetworkSwitched(GorillaNetworkJoinTrigger trigger)
        {
            if (locations.ContainsKey(trigger.name))
            {
                boxObject.transform.position = locations[trigger.name][0];
                boxObject.transform.rotation = Quaternion.Euler(locations[trigger.name][1]);
            }
        }

        public void EatChocolate()
        {
            chocolatesEaten++;
            if (chocolatesEaten >= 25) SwitchMode(GiveoutMode.LastDay, 0.75f);
        }

        internal void OnInitialized()
        {
            if (Initialized) return;
            Initialized = true;

            DataLoader.LoadData();

            if (DataLoader.currentData.ChocolatesPickedUp.Count != 0) chocolatesEaten += DataLoader.currentData.ChocolatesPickedUp.Count;

            leftHand = new Hand();
            leftHand.HandIndicator = GorillaTagger.Instance.transform.Find("TurnParent/LeftHandTriggerCollider").GetComponent<GorillaTriggerColliderHandIndicator>();

            rightHand = new Hand();
            rightHand.HandIndicator = GorillaTagger.Instance.transform.Find("TurnParent/RightHandTriggerCollider").GetComponent<GorillaTriggerColliderHandIndicator>();

            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("DevAdventCalendarMod.Resources.adventbundle");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);

            audioClips.Add(bundle.LoadAsset<AudioClip>("yum"));
            audioClips.Add(bundle.LoadAsset<AudioClip>("yum2"));

            boxObject = Instantiate(bundle.LoadAsset<GameObject>("Calendar"));
            boxObject.transform.position = new Vector3(-64.9369f, 11.83f, -84.3542f);
            boxObject.transform.rotation = Quaternion.Euler(0f, 74.175f, 0f);
            boxObject.transform.localScale = new Vector3(0.014626f, 0.014626f, 0.014626f);

            foreach (var collider in boxObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            int dayInteger = DateTime.Now.Day;
            Transform boxesParent = boxObject.transform.Find("TopBoxes");
            endingBoxObject = boxObject.transform.Find("box");
            noteComponent = endingBoxObject.transform.Find("note").gameObject.AddComponent<Note>();
            DevChocolate = boxObject.transform.Find("chocolate (1)").gameObject.AddComponent<Chocolate>();
            DevChocolate.gameObject.SetActive(false);
            for (int i = 0; i < boxesParent.childCount; i++)
            {
                int currentInteger = i + 1;

                GameObject boxObjectLocal = boxesParent.GetChild(i).gameObject;
                Box boxLocal = new Box();
                boxLocal.BoxObject = boxObjectLocal;

                ParticleSystem sys = boxObjectLocal.GetComponentInChildren<ParticleSystem>();
                sys.gravityModifier = 0.02f;
                sys.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                boxLocal.DoorObject = boxObjectLocal.transform.Find("Door").gameObject;
                boxLocal.BreakSource = boxObjectLocal.GetComponent<AudioSource>();
                boxLocal.ChocolateObject = boxObjectLocal.transform.Find("chocolate").gameObject;
                boxLocal.ChocolateObject.SetActive(false); 
                boxLocal.CurrentNumber = currentInteger;
                boxLocal.ParticleSystem = sys;
                boxLocal.Opened = false;

                boxes.Add(boxLocal);
                if (i < dayInteger) boxesToOpen.Add(boxLocal);
            }

            if (DataLoader.currentData == null) Scripts.Logger.LogMessage("No data found!", 0);
            else Scripts.Logger.LogMessage("Data found", 0);

            endingBoxObject.gameObject.SetActive(!DataLoader.currentData.GiftOpened);

            if (DataLoader.currentData.DoorsOpened.Count != 0)
            {
                for (int i = 0; i < DataLoader.currentData.DoorsOpened.Count; i++)
                {
                    if (i != dayInteger && i < dayInteger) OpenDoor(i, false);
                }
            }
        }

        internal void LateUpdate()
        {
            if (!Initialized) return;

            if (Vector3.Distance(GorillaLocomotion.Player.Instance.bodyCollider.transform.position, boxObject.transform.position) <= 1.25f)
            {
                if (mode == GiveoutMode.None)
                {
                    mode = GiveoutMode.GivingOut;
                    StartUnlock();
                }
            }

            if ((Vector3.Distance(leftHand.HandIndicator.transform.position, endingBoxObject.transform.position) <= 0.05f || Vector3.Distance(rightHand.HandIndicator.transform.position, endingBoxObject.transform.position) <= 0.05f) && mode == GiveoutMode.LastDay && !DataLoader.currentData.GiftOpened)
            {
                mode = GiveoutMode.GiftOpen;
                DataLoader.currentData.GiftOpened = true;
                DataLoader.SaveData();
                SwitchMode(GiveoutMode.GiftOpened, 0.75f);
            }

            if ((Vector3.Distance(leftHand.HandIndicator.transform.position, endingBoxObject.transform.position) <= 0.2f || Vector3.Distance(rightHand.HandIndicator.transform.position, endingBoxObject.transform.position) <= 0.2f) && mode == GiveoutMode.GiftOpened)
            {
                mode = GiveoutMode.GiftRead;
                SwitchMode(GiveoutMode.NoteReady, 1.25f);
                OpenGift();
            }

            if ((Vector3.Distance(leftHand.HandIndicator.transform.position, noteComponent.transform.position) <= 0.08f || Vector3.Distance(rightHand.HandIndicator.transform.position, noteComponent.transform.position) <= 0.08f) && mode == GiveoutMode.NoteReady)
            {
                mode = GiveoutMode.NoteOpen;
                SwitchMode(GiveoutMode.NoteOpened, 1.25f);
                DevChocolate.gameObject.SetActive(true);
                DataLoader.currentData.DevChocolatePickedUp = true;
                DataLoader.SaveData();
                Invoke("OpenNote", 0.25f);
            }

            if (mode == GiveoutMode.GiftOpen || mode == GiveoutMode.GiftOpened)
            {
                endingBoxObject.transform.localPosition = Vector3.Lerp(endingBoxObject.transform.localPosition, new Vector3(-17.9f, -17.63f, -69.5f), 0.05f);
                endingBoxObject.transform.localScale = Vector3.Lerp(endingBoxObject.transform.localScale, new Vector3(8.759723f, 8.759723f, 8.759723f), 0.05f);
                endingBoxObject.transform.localRotation = Quaternion.Lerp(endingBoxObject.transform.localRotation, Quaternion.Euler(0, -31.855f, 0), 0.05f);
            }

            if (mode == GiveoutMode.NoteOpen || mode == GiveoutMode.NoteOpened)
            {
                noteComponent.transform.localPosition = Vector3.Lerp(noteComponent.transform.localPosition, new Vector3(0.003f, 1.552f, -0.057f), 0.05f);
            }
        }

        public void StartUnlock() => StartCoroutine(Countdown());

        public void GiftCooldown()
        {
            mode = GiveoutMode.GiftOpen;
        }

        public void OpenGift()
        {
            endingBoxObject.GetComponentInChildren<Animator>().SetBool("PresentOpen", true);
            endingBoxObject.GetComponent<AudioSource>().Play();
        }

        public void OpenNote()
        {
            noteComponent.adjustor = 0;
            noteComponent.GetComponent<AudioSource>().Play();
        }

        public bool IsOpened(int index)
        {
            Box boxToCheck = boxesToOpen[index];
            return boxToCheck.Opened;
        }

        public void SwitchMode(GiveoutMode mode, float delay) => StartCoroutine(SwitchModeEnum(mode, delay));

        internal IEnumerator SwitchModeEnum(GiveoutMode m, float d)
        {
            if (d != 0) yield return new WaitForSeconds(d);
            mode = m;
            yield break;
        }

        public void OpenDoor(int index, bool particleEffects)
        {
            Box boxToOpen = boxesToOpen[index];
            Chocolate chocolate = boxToOpen.ChocolateObject.AddComponent<Chocolate>();

            bool showChocolate = true;
            if (DataLoader.currentData.ChocolatesPickedUp.Count != 0)
            {
                if (DataLoader.currentData.ChocolatesPickedUp.Contains(index)) showChocolate = false;
            }

            chocolate.gameObject.SetActive(showChocolate);
            boxToOpen.DoorObject.SetActive(false);
            
            if (particleEffects)
            {
                boxToOpen.ParticleSystem.Play();
                boxToOpen.BreakSource.Play();
                DataLoader.currentData.DoorsOpened.Add(index);
                DataLoader.SaveData();
            }

            boxToOpen.Opened = true;
        }

        public Box GetChocolateFromBox(GameObject chocolate)
        {
            for(int i = 0; i < boxes.Count; i++)
            {
                if (boxes[i].ChocolateObject == chocolate) return boxes[i];
            }
            return null;
        }

        internal IEnumerator Countdown()
        {
            for (int i = 0; i < boxesToOpen.Count; i++)
            {
                if (!IsOpened(i))
                {
                    OpenDoor(i, true);

                    yield return new WaitForSeconds(0.25f);
                }
            }
            yield break;
        }
    }
}
