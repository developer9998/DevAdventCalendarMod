using System;
using UnityEngine;

namespace DevAdventCalandarMod.Models
{
    [Serializable]
    public class Box
    {
        public AudioSource BreakSource { get; set; }
        public GameObject BoxObject { get; set; }
        public GameObject DoorObject { get; set; }
        public GameObject ChocolateObject { get; set; }
        public ParticleSystem ParticleSystem { get; set; }

        /* CSharp DataTypes */
        public bool Opened { get; set; }
        public int CurrentNumber { get; set; }
    }
}
