using System;
using System.Threading.Tasks;

namespace Bspeak.Services.FrequencyResolver
{
    public class FrequencyResolver : IFrequencyResolver
    {
        private readonly float[] a = new float[2];
        private readonly float[] b = new float[3];
        private readonly float[] mem1 = new float[4];
        private readonly float[] mem2 = new float[4];
        private int[] _bitreverse;
        private int _bits;
        private float[] _freqTable;
        private int _sampleRate;
        private float[] _window;
        private float[] _xi;

        public async Task Init(int sampleRate = 44100, int fftBits = 13)
        {
            if (fftBits > 15)
                throw new ArgumentOutOfRangeException($"{fftBits} is too many bits, max is 15");

            _sampleRate = sampleRate;
            _bits = fftBits;
            var fftSize = 1 << fftBits;
            _bitreverse = new int[fftSize];
            _freqTable = new float[fftSize];
            _window = new float[fftSize];
            _xi = new float[fftSize];
            await Task.Run(InitInternal);
        }

        public int FindFoundamentalFrequency(float[] samples)
        {
            ApplyLowPassFilter(samples, mem1, mem2, a, b);
            ApplyWindow(_window, samples);
            ApplyFFT(samples, _xi, _bits, _bitreverse);
            var peak = FindPeak(_freqTable, samples, _xi, _bits);
            return peak;
        }

        private static void ApplyLowPassFilter(float[] samples, float[] mem1, float[] mem2, float[] a, float[] b)
        {
            for (var j = 0; j < samples.Length; ++j)
            {
                samples[j] = ProcessSecondOrderFilter(samples[j], mem1, a, b);
                samples[j] = ProcessSecondOrderFilter(samples[j], mem2, a, b);
            }
        }

        private static float ProcessSecondOrderFilter(float x, float[] mem, float[] a, float[] b)
        {
            var ret = b[0] * x + b[1] * mem[0] + b[2] * mem[1]
                      - a[0] * mem[2] - a[1] * mem[3];

            mem[1] = mem[0];
            mem[0] = x;
            mem[3] = mem[2];
            mem[2] = ret;

            return ret;
        }

        private static void ApplyWindow(float[] window, float[] samples)
        {
            for (var i = 0; i < samples.Length; i++)
                samples[i] *= window[i];
        }

        private static int FindPeak(float[] freqTable, float[] xr, float[] xi, int bits)
        {
            float maxVal = -1;
            var maxIndex = -1;
            for (var j = 0; j < 1 << (bits - 1); ++j)
            {
                var v = xr[j] * xr[j] + xi[j] * xi[j];

                if (v > maxVal)
                {
                    maxVal = v;
                    maxIndex = j;
                }
            }
            var freq = freqTable[maxIndex];
            return (int) freq;
        }

        private static void ApplyFFT(float[] xr, float[] xi, int bits, int[] bitreverse)
        {
            Array.Clear(xi, 0, xi.Length);
            var n = 1 << bits;
            var n2 = n / 2;

            for (var l = 0; l < bits; ++l)
            {
                for (var k = 0; k < n; k += n2)
                for (var i = 0; i < n2; ++i, ++k)
                {
                    var p = bitreverse[k / n2];
                    var ang = 6.283185 * p / n;
                    var c = Math.Cos(ang);
                    var s = Math.Sin(ang);
                    /*
                                        sincos( ang, &ds, &dc );
                                        s = ds;
                                        c = dc;
                        */
                    var kn2 = k + n2;
                    var tr = (float) (xr[kn2] * c + xi[kn2] * s);
                    var ti = (float) (xi[kn2] * c - xr[kn2] * s);
                    xr[kn2] = xr[k] - tr;
                    xi[kn2] = xi[k] - ti;
                    xr[k] += tr;
                    xi[k] += ti;
                }
                n2 /= 2;
            }

            for (var k = 0; k < n; ++k)
            {
                var i = bitreverse[k];
                if (i <= k)
                    continue;
                var tr = xr[k];
                var ti = xi[k];
                xr[k] = xr[i];
                xr[k] = xr[i];
                xr[i] = tr;
                xr[i] = ti;
            }

            var f = 1.0f / n;
            for (var i = 0; i < n; ++i)
            {
                xr[i] *= f;
                xi[i] *= f;
            }
        }

        private Task InitInternal()
        {
            for (var i = 0; i < _freqTable.Length; i++)
                _freqTable[i] = _sampleRate * i / (float) _freqTable.Length;

            for (var i = 0; i < _window.Length; ++i)
                _window[i] = (float) (0.5 * (1 - Math.Cos(2 * Math.PI * i / (_window.Length - 1.0))));

            ComputeSecondOrderLowPassParameters(_sampleRate, 330, a, b);
            for (var i = _bitreverse.Length - 1; i >= 0; --i)
            {
                var k = 0;
                for (var j = 0; j < _bits; ++j)
                {
                    k *= 2;
                    if ((i & (1 << j)) != 0)
                        k += 1;
                }
                _bitreverse[i] = k;
            }
            return Task.FromResult(0);
        }

        private void ComputeSecondOrderLowPassParameters(int srate, float f, float[] a, float[] b)
        {
            var w0 = 2 * Math.PI * f / srate;
            var cosw0 = Math.Cos(w0);
            var sinw0 = Math.Sin(w0);
            //float alpha = sinw0/2;
            var alpha = sinw0 / 2 * Math.Sqrt(2);

            var a0 = 1 + alpha;
            a[0] = (float) (-2 * cosw0 / a0);
            a[1] = (float) ((1 - alpha) / a0);
            b[0] = (float) ((1 - cosw0) / 2 / a0);
            b[1] = (float) ((1 - cosw0) / a0);
            b[2] = b[0];
        }
    }
}