#nullable enable

namespace UFeel
{
    /// <summary>Called on the thread that started <see cref="EyeTrackingCalibration.RunAsync"/>.</summary>
    public interface IEyeTrackingCalibrationPresenter
    {
        public void Show();
        public void Hide();

        /// <summary>Normalized screen position (0..1, origin top-left).</summary>
        public void ShowTarget(float x, float y);

        public void ShowInstruction(string text);
        public void ShowStatus(string text);
    }
}
