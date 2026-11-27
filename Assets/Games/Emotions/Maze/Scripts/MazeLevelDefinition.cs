using System.Collections.Generic;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    [CreateAssetMenu(fileName = "MazeLevel", menuName = "GameHub/Maze/Level")]
    public sealed class MazeLevelDefinition : ScriptableObject
    {
        public const int MinSize = 2;
        public const int MaxSize = 12;

        [Header("Grid")]
        [Tooltip("Rooms per side.")]
        [SerializeField, Range(MinSize, MaxSize)] private int size = 3;

        [Header("Emotions")]
        [Tooltip("Emotions the doors can ask for, among those of the catalog.")]
        [SerializeField] private List<EmotionType> emotions = new() { EmotionType.Happy, EmotionType.Surprised };

        [Header("Passages")]
        [Tooltip("Share of the remaining walls turned into extra passages (loops).")]
        [SerializeField, Range(0f, 1f)] private float loopRatio;
        [Tooltip("Chance for a passage to close behind the player.")]
        [SerializeField, Range(0f, 1f)] private float closingChance;
        [Tooltip("Chance for a loop passage to be one-way.")]
        [SerializeField, Range(0f, 1f)] private float oneWayChance;

        [Header("Special rooms")]
        [SerializeField, Min(0)] private int hintRooms;
        [Tooltip("Traps only go in dead ends, so fewer may be placed.")]
        [SerializeField, Min(0)] private int trapRooms;
        [SerializeField, Min(0)] private int bonusRooms;

        [Header("Room shapes (relative weights)")]
        [SerializeField, Min(0f)] private float squareWeight = 1f;
        [SerializeField, Min(0f)] private float rectangleWeight;
        [SerializeField, Min(0f)] private float circleWeight;
        [Tooltip("A room with four doors cannot be triangular and stays square.")]
        [SerializeField, Min(0f)] private float triangleWeight;

        public int Size => size;

        public float LoopRatio => loopRatio;

        public float ClosingChance => closingChance;

        public float OneWayChance => oneWayChance;

        public int HintRooms => hintRooms;

        public int TrapRooms => trapRooms;

        public int BonusRooms => bonusRooms;

        public bool Uses(EmotionType emotion) => emotions.Contains(emotion);

        public float WeightOf(RoomShape shape)
        {
            return shape switch
            {
                RoomShape.Rectangle => rectangleWeight,
                RoomShape.Circle => circleWeight,
                RoomShape.Triangle => triangleWeight,
                _ => squareWeight,
            };
        }
    }
}
