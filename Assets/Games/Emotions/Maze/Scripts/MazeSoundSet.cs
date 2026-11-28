using UnityEngine;

namespace GameHub.Maze
{
    [CreateAssetMenu(fileName = "MazeSounds", menuName = "GameHub/Maze/Sound Set")]
    public sealed class MazeSoundSet : ScriptableObject
    {
        [SerializeField] private AudioClip doorOpen;
        [SerializeField] private AudioClip doorClose;
        [SerializeField] private AudioClip hint;
        [SerializeField] private AudioClip bonus;
        [SerializeField] private AudioClip trap;
        [SerializeField] private AudioClip win;

        public AudioClip DoorOpen => doorOpen;

        public AudioClip DoorClose => doorClose;

        public AudioClip Hint => hint;

        public AudioClip Bonus => bonus;

        public AudioClip Trap => trap;

        public AudioClip Win => win;
    }
}
