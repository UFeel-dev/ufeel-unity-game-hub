using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Maze
{
    [CreateAssetMenu(fileName = "MazeSounds", menuName = "GameHub/Maze/Sound Set")]
    public sealed class MazeSoundSet : ScriptableObject
    {
        [SerializeField] private AudioClip ambient;
        [SerializeField] private AudioClip charge;
        [SerializeField] private AudioClip doorOpen;
        [SerializeField] private AudioClip doorClose;
        [SerializeField] private AudioClip hint;
        [SerializeField] private AudioClip bonus;
        [SerializeField] private AudioClip trap;
        [SerializeField] private AudioClip win;
        [SerializeField] private List<AudioClip> steps = new();

        public AudioClip Ambient => ambient;

        public AudioClip Charge => charge;

        public AudioClip DoorOpen => doorOpen;

        public AudioClip DoorClose => doorClose;

        public AudioClip Hint => hint;

        public AudioClip Bonus => bonus;

        public AudioClip Trap => trap;

        public AudioClip Win => win;

        public IReadOnlyList<AudioClip> Steps => steps;
    }
}
