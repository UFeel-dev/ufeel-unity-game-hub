using System.Collections.Generic;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    [CreateAssetMenu(fileName = "MazeCatalog", menuName = "GameHub/Maze/Catalog")]
    public sealed class MazeCatalog : ScriptableObject
    {
        [SerializeField] private List<MazeLevelDefinition> levels = new();

        [Tooltip("Emotions available to the levels, with their door colour.")]
        [SerializeField] private List<EmotionInfo> emotions = new()
        {
            new(EmotionType.Happy, new Color(1f, 0.84f, 0.25f, 1f)),
            new(EmotionType.Surprised, new Color(1f, 0.45f, 0.75f, 1f)),
            new(EmotionType.Sad, new Color(0.25f, 0.55f, 1f, 1f)),
            new(EmotionType.Angry, new Color(0.95f, 0.25f, 0.2f, 1f)),
            new(EmotionType.Fearful, new Color(0.6f, 0.4f, 1f, 1f)),
            new(EmotionType.Disgusted, new Color(0.55f, 0.78f, 0.2f, 1f)),
        };

        [Header("Unlimited mode")]
        [Tooltip("Rules of every Unlimited maze. Its size is replaced by the sizes below.")]
        [SerializeField] private MazeLevelDefinition unlimited;
        [Tooltip("Size of the first Unlimited maze. Each cleared maze adds one room per side.")]
        [SerializeField, Range(MazeLevelDefinition.MinSize, MazeLevelDefinition.MaxSize)] private int unlimitedStartSize = 5;
        [SerializeField, Range(MazeLevelDefinition.MinSize, MazeLevelDefinition.MaxSize)] private int unlimitedMaxSize = 10;

        public IReadOnlyList<MazeLevelDefinition> Levels => levels;

        public IReadOnlyList<EmotionInfo> Emotions => emotions;

        public MazeLevelDefinition Unlimited => unlimited;

        public int UnlimitedSize(int round)
        {
            return Mathf.Min(unlimitedStartSize + round, Mathf.Max(unlimitedStartSize, unlimitedMaxSize));
        }

        public List<EmotionInfo> EmotionsOf(MazeLevelDefinition level)
        {
            List<EmotionInfo> result = new();
            foreach (EmotionInfo info in emotions)
            {
                if (level.Uses(info.Emotion) && !result.Exists(item => item.Emotion == info.Emotion))
                    result.Add(info);
            }

            if (result.Count == 0 && emotions.Count > 0)
            {
                Debug.LogError($"Maze level {level.name} uses no emotion of the catalog; using {emotions[0].Emotion}.", level);
                result.Add(emotions[0]);
            }

            return result;
        }

        [ContextMenu("Reset saved progress")]
        private void ResetProgress()
        {
            MazeSave.Reset();
        }
    }
}
