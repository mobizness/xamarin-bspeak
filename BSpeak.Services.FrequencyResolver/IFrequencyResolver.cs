using System.Threading.Tasks;

namespace Bspeak.Services.FrequencyResolver
{
    public interface IFrequencyResolver
    {
        Task Init(int sampleRate = 44100, int fftBits = 13);
        int FindFoundamentalFrequency(float[] samples);
    }
}