using UFeel;

public class EmotionCondition : FogCondition
{
    private EmotionType _emotionType;

    public EmotionCondition(EmotionType emotionType)
    {
        _emotionType = emotionType;
    }

    public override bool CanPass()
    {
        return MazeManager.GetEmotionLevel(_emotionType) >= 2.0f;
    }
}
