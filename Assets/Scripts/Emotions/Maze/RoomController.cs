using System.Collections.Generic;
using TMPro;
using UFeel;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Room Data")]
    public RoomData Data;

    [Header("Fog References")]
    public GameObject NorthFog;
    public GameObject SouthFog;
    public GameObject EastFog;
    public GameObject WestFog;
    public TMP_Text NorthText;
    public TMP_Text SouthText;
    public TMP_Text EastText;
    public TMP_Text WestText;

    public void Setup(int x, int y)
    {
        EmotionInfo[] emotions = PickRoomEmotions(x, y);
        EmotionInfo northData = emotions[(int)Direction.North];
        EmotionInfo southData = emotions[(int)Direction.South];
        EmotionInfo eastData = emotions[(int)Direction.East];
        EmotionInfo westData = emotions[(int)Direction.West];

        Data = new RoomData
        {
            X = x,
            Z = y,
            NorthCondition = new EmotionCondition(northData.Emotion),
            SouthCondition = new EmotionCondition(southData.Emotion),
            EastCondition = new EmotionCondition(eastData.Emotion),
            WestCondition = new EmotionCondition(westData.Emotion)
        };

        NorthFog.GetComponent<Renderer>().material.color = northData.Color;
        SouthFog.GetComponent<Renderer>().material.color = southData.Color;
        EastFog.GetComponent<Renderer>().material.color = eastData.Color;
        WestFog.GetComponent<Renderer>().material.color = westData.Color;

        NorthText.text = northData.Emotion.ToString();
        SouthText.text = southData.Emotion.ToString();
        EastText.text = eastData.Emotion.ToString();
        WestText.text = westData.Emotion.ToString();

        DisableBorders();
    }

    private static EmotionInfo[] PickRoomEmotions(int x, int z)
    {
        EmotionInfo[] emotions = new EmotionInfo[4];
        for (int i = 0; i < emotions.Length; i++)
        {
            emotions[i] = EmotionInfo.GetRandomEmotion();
        }

        List<int> openDirections = new();
        if (z < MazeManager.MazeSize - 1)
            openDirections.Add((int)Direction.North);
        if (z > 0)
            openDirections.Add((int)Direction.South);
        if (x < MazeManager.MazeSize - 1)
            openDirections.Add((int)Direction.East);
        if (x > 0)
            openDirections.Add((int)Direction.West);
        if (openDirections.Count < 2)
            openDirections = new List<int> { 0, 1, 2, 3 };

        EmotionType first = emotions[openDirections[0]].Emotion;
        if (openDirections.TrueForAll(direction => emotions[direction].Emotion == first))
        {
            int changed = openDirections[Random.Range(0, openDirections.Count)];
            emotions[changed] = EmotionInfo.GetRandomEmotionExcept(first);
        }

        return emotions;
    }

    public void DisableBorders()
    {
        if (Data.X == 0 && WestFog != null)
        {
            WestFog.SetActive(false);
        }

        if (Data.X == MazeManager.MazeSize - 1 && EastFog != null)
        {
            EastFog.SetActive(false);
        }

        if (Data.Z == 0 && SouthFog != null)
        {
            SouthFog.SetActive(false);
        }

        if (Data.Z == MazeManager.MazeSize - 1 && NorthFog != null)
        {
            NorthFog.SetActive(false);
        }
    }

    public FogCondition GetCondition(Direction direction)
    {
        return direction switch
        {
            Direction.North => Data.NorthCondition,
            Direction.South => Data.SouthCondition,
            Direction.East => Data.EastCondition,
            Direction.West => Data.WestCondition,
            _ => Data.NorthCondition
        };
    }
}
