using dotty.Loggers.ElapsedLoggers.Abstract;
using NAudio.Wave;
using System.Diagnostics;

namespace dotty.Services
{
    public class AudioService
    {
        private ILogger<AudioService> _logger;
        private IElapsedLogger _elapsedLogger;
        private HttpClient _client;

        public AudioService(ILogger<AudioService> logger, IElapsedLogger elapsedLogger, HttpClient client)
        {
            this._logger = logger;
            this._elapsedLogger = elapsedLogger;
            this._client = client;
        }

        public async Task<AudioPoints> CalculatePoints(string url, int scale, int? relativeStart, int? customDuration)
        {
            this._elapsedLogger.Start("fetchingAudio", $"Fetching Audio from {url}");
            var audioStream = await this.FetchAudio(url);
            this._elapsedLogger.Stop("fetchingAudio", $"Finished fetching audio.");

            var points = await this.CalculatePoints(audioStream, scale, relativeStart, customDuration);
            return points;
        }

        public async Task<AudioPoints> CalculatePoints(Stream audioStream, int scale, int? relativeStart, int? customDuration)
        {
            var waveReader = new WaveFileReader(audioStream);
            int duration = customDuration ?? (int)waveReader.TotalTime.TotalMilliseconds;
            int startTime = relativeStart ?? 0;

            this._elapsedLogger.Start("Calculating Y Values");
            float[][] yValues = await this.GetYValues(waveReader, scale);
            this._elapsedLogger.Stop("Calculating Y Values");

            this._elapsedLogger.Start("Calculating X Values");
            int[][] xValues = this.GetXValues(yValues, startTime, duration);
            this._elapsedLogger.Stop("Calculating X Values");

            return new AudioPoints(xValues, yValues);
        }

        private async Task<Stream> FetchAudio(string url)
        {
            var res = await this._client.GetAsync(url);
            var audioStream = await res.Content.ReadAsStreamAsync();
            return audioStream;
        }

        private int[][] GetXValues(float[][] yValues, int start, int duration)
        {
            var xValues = new int[yValues.Length][];

            for (int i = 0; i < xValues.Length; i++)
            {
                int timePerValue = duration / yValues[i].Length; ;
                xValues[i] = new int[yValues[i].Length];

                for (int j = 0; j < yValues[i].Length; j++)
                {
                    xValues[i][j] = start + j * timePerValue;
                }
            }

            return xValues;
        }

        private async Task<float[][]> GetYValues(WaveFileReader waveReader, int scale)
        {
            this._logger.LogInformation($"Size of audio buffer: {waveReader.Length}");
            var audioBuffer = new byte[waveReader.Length];
            await waveReader.ReadAsync(audioBuffer);
            waveReader.Close();

            var waveformat = waveReader.WaveFormat;

            int blockSize = waveformat.BlockAlign;
            int channels = waveformat.Channels;
            int sampleSize = sizeof(float);
            int samplesPerChannel = audioBuffer.Length / sampleSize / channels;

            int reducedChannelSize = samplesPerChannel / scale;
            if (samplesPerChannel % scale != 0)
            {
                reducedChannelSize++;
            }

            float[][] channelBuffers = new float[channels][];
            for (int i = 0; i < channelBuffers.Length; i++)
            {
                channelBuffers[i] = new float[reducedChannelSize];
            }

            int numberOfElements;
            float[] totalPerChannel = new float[channels];

            for (int byteIndex = 0, step = scale * blockSize; byteIndex < audioBuffer.Length; byteIndex += step)
            {
                numberOfElements = byteIndex + step < audioBuffer.Length
                    ? scale
                    : (audioBuffer.Length - byteIndex) / blockSize;
                Array.Fill(totalPerChannel, 0);

                for (int blockIndex = 0; blockIndex < scale && byteIndex + blockIndex * blockSize < audioBuffer.Length; blockIndex++)
                {
                    for (int channelIndex = 0; channelIndex < channels; channelIndex++)
                    {
                        float value =
                            BitConverter.ToSingle(audioBuffer, byteIndex + blockIndex * blockSize + sampleSize * channelIndex);
                        float absValue = Math.Abs(value);
                        totalPerChannel[channelIndex] += absValue;
                    }
                }

                for (int channelIndex = 0; channelIndex < channels; channelIndex++)
                {
                    float total = totalPerChannel[channelIndex];
                    float average = Math.Min(total / numberOfElements, 1.0f);
                    channelBuffers[channelIndex][byteIndex / (blockSize * scale)] = average;
                }
            }

            return channelBuffers;
        }
    }
}
