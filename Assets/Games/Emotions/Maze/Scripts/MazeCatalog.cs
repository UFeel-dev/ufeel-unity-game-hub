using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Maze
{
    [CreateAssetMenu(fileName = "MazeCatalog", menuName = "GameHub/Maze/Catalog")]
    public sealed class MazeCatalog : ScriptableObject
    {
        [SerializeField] private List<MazeLevelDefinition> levels = new();

        [Header("Unlimited mode")]
        [Tooltip("Rules of every Unlimited maze. Its size is replaced by the sizes below.")]
        [SerializeField] private MazeLevelDefinition unlimited;
        [Tooltip("Size of the first Unlimited maze. Each cleared maze adds one room per side.")]
        [SerializeField, Range(MazeLevelDefinition.MinSize, MazeLevelDefinition.MaxSize)] private int unlimitedStartSize = 5;
        [SerializeField, Range(MazeLevelDefinition.MinSize, MazeLevelDefinition.MaxSize)] private int unlimitedMaxSize = 10;

        public IReadOnlyList<MazeLevelDefinition> Levels => levels;

        public MazeLevelDefinition Unlimited => unlimited;

        public int UnlimitedSize(int round)
        {
            return Mathf.Min(unlimitedStartSize + round, Mathf.Max(unlimitedStartSize, unlimitedMaxSize));
        }

        [ContextMenu("Reset saved progress")]
        private void ResetProgress()
        {
            MazeSave.Reset();
        }
    }
}
