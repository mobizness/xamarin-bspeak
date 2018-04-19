using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bspeak.Services.VoiceRecorder.Abstractions
{
    public interface IVoiceRecorder
    {
        void Init(int sampleRate = 44100, int bufferSize = 8192);
        Task Start(Action<float[]> callback);
        Task Stop();
    }
}
