#nullable enable

namespace UFeel
{
    /// <summary>Called on the thread that started <see cref="EmotionCalibration.RunAsync"/>.</summary>
    public interface IEmotionCalibrationPresenter
    {
        public void Show();
        public void Hide();
        public void ShowEmotion(EmotionType emotion);

        /// <summary>Share of the current emotion's recording time, 0..1.</summary>
        public void ShowProgress(float fraction);

        public void ShowInstruction(string text);
        public void ShowStatus(string text);
    }
}
