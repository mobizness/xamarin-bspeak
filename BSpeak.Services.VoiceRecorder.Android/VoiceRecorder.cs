using System;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Media;
using Bspeak.Services.VoiceRecorder.Abstractions;

[assembly: UsesPermission(Manifest.Permission.RecordAudio)]

namespace Bspeak.Services.VoiceRecorder.Android
{
    public class VoiceRecorder : IVoiceRecorder, IDisposable
    {
        private float[] _buffer;
        private Action<float[]> _callback;
        private bool _isrecording;
        private AudioRecord _record;
        private Thread _thread;
        private short[] _tmpBuffer;
        private int _sampleRate;
        private int _bufferSize;

        public void Init(int sampleRate = 44100, int bufferSize = 8192)
        {
            _sampleRate = sampleRate;
            _bufferSize = bufferSize;
            _buffer = new float[bufferSize];

            _tmpBuffer = new short[bufferSize];
        }

        public Task Start(Action<float[]> callback)
        {
            if (!_isrecording)
            {
                _isrecording = true;
                var minbuffersize = AudioRecord.GetMinBufferSize(_sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
                if (_bufferSize * 2 < minbuffersize)
                    throw new ArgumentException($"MinBufferSize is {minbuffersize}byte");
                _record = new AudioRecord(AudioSource.Default, _sampleRate, ChannelIn.Mono, Encoding.Pcm16bit,
                    _bufferSize * 2);
                _thread = new Thread(ReadThread);
                _thread.Start();
            }
            _callback = callback;
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if (_isrecording)
            {
                _isrecording = false;
                Thread.Sleep(1000);
                _thread.Abort();
                _thread = null;
            }
            return Task.CompletedTask;
        }

        private void ReadThread()
        {
            _record.StartRecording();
            while (_isrecording)
            {
                var size = _record.Read(_tmpBuffer, 0, _tmpBuffer.Length);

                for (var i = 0; i < _tmpBuffer.Length; i++)
                    _buffer[i] = _tmpBuffer[i] / 32767.0f;//(_tmpBuffer[i] > 100 || _tmpBuffer[i]<-100) ? _tmpBuffer[i] / 32767.0f : 0f;

                _callback?.Invoke(_buffer);
            }
            _record.Stop();
            _record.Release();
            _record.Dispose();
        }

        public void Dispose()
        {
            _record?.Release();
            _record?.Dispose();
        }
    }
}