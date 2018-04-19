using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AudioToolbox;
using Bspeak.Services.VoiceRecorder.Abstractions;

namespace Bspeak.Services.VoiceRecorder.iOS
{
    public class VoiceRecorder : IVoiceRecorder, IDisposable
    {
        private InputAudioQueue _audioQueue;
        private List<IntPtr> _bufferPtrs;
        private int _bufferSize;
        private Action<float[]> _callback;
        private bool _isrecording;
        private int _sampleRate;

        public void Dispose()
        {
            Stop();
            foreach (var ptr in _bufferPtrs)
                _audioQueue.FreeBuffer(ptr);
            _audioQueue.QueueDispose();
            _audioQueue.Dispose();
        }

        public void Init(int sampleRate = 44100, int bufferSize = 8192)
        {
            _sampleRate = sampleRate;
            _bufferSize = bufferSize;

            var description = new AudioStreamBasicDescription
            {
                SampleRate = sampleRate,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsPacked | AudioFormatFlags.IsSignedInteger,
                BitsPerChannel = 16,
                ChannelsPerFrame = 1,
                BytesPerFrame = 2,
                FramesPerPacket = 1,
                BytesPerPacket = 2,
                Reserved = 0
            };

            _audioQueue = new InputAudioQueue(description);
            _bufferPtrs = new List<IntPtr>();
            for (var i = 0; i < 3; i++)
            {
                IntPtr ptr;
                _audioQueue.AllocateBufferWithPacketDescriptors(bufferSize * description.BytesPerPacket, bufferSize,
                    out ptr);
                _audioQueue.EnqueueBuffer(ptr, bufferSize, null);
                _bufferPtrs.Add(ptr);
            }
            _audioQueue.InputCompleted += AudioQueueOnInputCompleted;
        }

        public Task Start(Action<float[]> callback)
        {
            if (!_isrecording)
            {
                var status = _audioQueue.Start();
                if (status != AudioQueueStatus.Ok)
                    throw new Exception(status.ToString());
                _isrecording = true;
            }
            _callback = callback;
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if (_isrecording)
            {
                _audioQueue.Stop(true);
                _isrecording = false;
            }
            return Task.CompletedTask;
        }

        private void AudioQueueOnInputCompleted(object sender, InputCompletedEventArgs args)
        {
            var buffer = (AudioQueueBuffer) Marshal.PtrToStructure(args.IntPtrBuffer, typeof(AudioQueueBuffer));
            var tmpBuffer = new byte[buffer.AudioDataByteSize];
            var result = new float[buffer.AudioDataByteSize / 2];
            Marshal.Copy(buffer.AudioData, tmpBuffer, 0, (int) buffer.AudioDataByteSize);

            for (int index = 0, i = 0; i < tmpBuffer.Length; index++, i += 2)
            {
                var x = BitConverter.ToInt16(tmpBuffer, i);
                result[index] = Math.Abs(x) > 100 ? x / 32767.0f : 0f;
            }
            _callback?.Invoke(result);

            _audioQueue.EnqueueBuffer(args.IntPtrBuffer, _bufferSize, args.PacketDescriptions);
        }
    }
}