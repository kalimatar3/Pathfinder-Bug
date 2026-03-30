namespace Clouds.SignalSystem
{
    /// <summary>
    /// Interface dành cho các Receiver có khả năng xử lý hiệu ứng UI (Animations, Content Updates)
    /// </summary>
    public interface IUIReceiver : ISignalReceiver
    {
        void PlayAnimations(SignalMessage message);
        void UpdateContents(SignalMessage message);
    }
}