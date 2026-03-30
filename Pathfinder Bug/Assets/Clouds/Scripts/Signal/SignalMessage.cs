using System;
using System.Collections.Generic;

namespace Clouds.SignalSystem
{
    [Serializable]
    public struct SignalMessage
    {
        public enum SignalType 
        { 
            None = 0, 
            OnShow, 
            OnClose, 
            Command1, Command2, Command3
        }

        public SignalType Type;
        public object Value; // Dữ liệu đi kèm (payload)
        public int SenderID; // ID của người gửi
        public int TargetID; // ID của người nhận mục tiêu (-1 là Global)

        public static SignalMessage Create(SignalType type, object value = null, int senderID = -1, int targetID = -1) 
            => new SignalMessage { Type = type, Value = value, SenderID = senderID, TargetID = targetID };
    }

    public static class UISignalBus
    {
        private static readonly List<ISignalReceiver> _receivers = new();

        public static void Subscribe(ISignalReceiver receiver)
        {
            if (!_receivers.Contains(receiver)) _receivers.Add(receiver);
        }

        public static void Unsubscribe(ISignalReceiver receiver)
        {
            if (_receivers.Contains(receiver)) _receivers.Remove(receiver);
        }

        public static void Emit(SignalMessage message)
        {
            for (int i = _receivers.Count - 1; i >= 0; i--)
            {
                var receiver = _receivers[i];
                if (receiver == null) 
                {
                    _receivers.RemoveAt(i);
                    continue;
                }
                receiver.OnSignalReceived(message);
            }
        }
    }
}