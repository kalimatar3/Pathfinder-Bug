using System;
using System.Collections.Generic;

namespace Clouds.SignalSystem
{
    [Serializable]
    public struct SignalPacket
    {
        public SignalMessage.SignalType type;
        public string customValue; 
    }

    public interface ISignalReceiver
    {
        void OnSignalReceived(SignalMessage message);
        int ID { get; }
    }

    public interface ISignalSender
    {
        List<SignalMessage.SignalType> SignalsToEmit { get; }
        List<ISignalReceiver> Receivers { get; }
        void SendSignal(SignalMessage message);
    }
}