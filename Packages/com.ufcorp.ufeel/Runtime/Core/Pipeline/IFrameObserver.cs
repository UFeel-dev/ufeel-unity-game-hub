#nullable enable

namespace UFeel
{
    public interface IFrameObserver
    {
        /// <summary>Runs on the session thread after each capture. Must not block (e.g. on the UI thread).</summary>
        public void OnFrameCaptured(UFeelProcessor processor);
    }
}
