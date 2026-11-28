namespace GameHub.Common
{
    public sealed class CalibrationToast
    {
        private const float MessageSeconds = 6f;

        private readonly Toast toast;

        private string? hint;
        private float messageRemaining;

        public CalibrationToast(Toast toast)
        {
            this.toast = toast;
        }

        public bool IsShowingMessage => messageRemaining > 0f;

        public void ShowMessage(string message)
        {
            messageRemaining = MessageSeconds;
            hint = null;
            toast.Show(message, MessageSeconds);
        }

        public void Tick(float deltaTime, string? wantedHint)
        {
            messageRemaining -= deltaTime;
            if (!IsShowingMessage && wantedHint != hint)
            {
                hint = wantedHint;
                if (hint != null)
                    toast.Show(hint, float.PositiveInfinity);
                else
                    toast.Hide();
            }

            toast.Tick(deltaTime);
        }
    }
}
