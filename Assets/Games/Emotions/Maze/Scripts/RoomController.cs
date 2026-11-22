using System.Collections.Generic;
using TMPro;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    public class RoomController : MonoBehaviour
    {
        [Header("Fogs")]
        [SerializeField] private GameObject northFog;
        [SerializeField] private GameObject southFog;
        [SerializeField] private GameObject eastFog;
        [SerializeField] private GameObject westFog;

        [Header("Labels")]
        [SerializeField] private TMP_Text northText;
        [SerializeField] private TMP_Text southText;
        [SerializeField] private TMP_Text eastText;
        [SerializeField] private TMP_Text westText;

        private readonly EmotionType[] doorEmotions = new EmotionType[4];

        public Vector2Int Coordinates { get; private set; }

        public void Setup(Vector2Int coordinates)
        {
            Coordinates = coordinates;
            EmotionInfo[] emotions = PickDoorEmotions(coordinates);

            for (int i = 0; i < emotions.Length; i++)
            {
                Direction direction = (Direction)i;
                doorEmotions[i] = emotions[i].Emotion;
                FogOf(direction).GetComponent<Renderer>().material.color = emotions[i].Color;
                LabelOf(direction).text = emotions[i].Emotion.ToString();
                if (!MazeGrid.IsInside(coordinates + MazeGrid.OffsetOf(direction)))
                    FogOf(direction).SetActive(false);
            }
        }

        public EmotionType GetDoorEmotion(Direction direction)
        {
            return doorEmotions[(int)direction];
        }

        private static EmotionInfo[] PickDoorEmotions(Vector2Int coordinates)
        {
            EmotionInfo[] emotions = new EmotionInfo[4];
            List<int> openDoors = new();
            for (int i = 0; i < emotions.Length; i++)
            {
                emotions[i] = EmotionInfo.Random();
                if (MazeGrid.IsInside(coordinates + MazeGrid.OffsetOf((Direction)i)))
                    openDoors.Add(i);
            }

            if (openDoors.Count == 0)
                return emotions;

            EmotionType first = emotions[openDoors[0]].Emotion;
            if (openDoors.TrueForAll(door => emotions[door].Emotion == first))
            {
                int changed = openDoors[Random.Range(0, openDoors.Count)];
                emotions[changed] = EmotionInfo.RandomExcept(first);
            }

            return emotions;
        }

        private GameObject FogOf(Direction direction)
        {
            return direction switch
            {
                Direction.North => northFog,
                Direction.South => southFog,
                Direction.East => eastFog,
                _ => westFog,
            };
        }

        private TMP_Text LabelOf(Direction direction)
        {
            return direction switch
            {
                Direction.North => northText,
                Direction.South => southText,
                Direction.East => eastText,
                _ => westText,
            };
        }
    }
}
