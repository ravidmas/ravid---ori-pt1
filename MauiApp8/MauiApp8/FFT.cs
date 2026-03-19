using System.Numerics;

namespace MauiApp8;

public static class FftHelper
{
    /// <summary>
    /// buffer = PCM 16-bit mono (little endian).
    /// bytesRecorded = how many bytes in buffer are valid.
    /// fftSize must be a power of 2 (e.g. 512/1024/2048).
    /// Returns magnitudes of first half of spectrum.
    /// </summary>
    public static double[] ComputeFftFromPcm16(
        byte[] buffer,
        int bytesRecorded,
        int fftSize)
    {
        // How many 16-bit samples we have:
        int sampleCount = Math.Min(bytesRecorded / 2, fftSize);
        var complex = new Complex[fftSize];

        // Fill with normalized samples + Hann window
        for (int i = 0; i < sampleCount; i++)
        {
            short sample16 = BitConverter.ToInt16(buffer, i * 2);
            double sample = sample16 / 32768.0; // [-1, 1]
            double window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (fftSize - 1)));
            complex[i] = new Complex(sample * window, 0.0);
        }

        // Zero-pad the rest
        for (int i = sampleCount; i < fftSize; i++)
        {
            complex[i] = Complex.Zero;
        }

        // In-place FFT using Cooley-Tukey algorithm
        CooleyTukeyFft(complex);

        int half = fftSize / 2;
        double[] magnitudes = new double[half];
        for (int i = 0; i < half; i++)
            magnitudes[i] = complex[i].Magnitude;

        return magnitudes;
    }

    /// <summary>
    /// Cooley-Tukey FFT algorithm (in-place, radix-2)
    /// Input array length must be a power of 2
    /// </summary>
    private static void CooleyTukeyFft(Complex[] data)
    {
        int n = data.Length;

        // Check if n is a power of 2
        if (n == 0 || (n & (n - 1)) != 0)
            throw new ArgumentException("Array length must be a power of 2");

        // Bit-reversal permutation
        int j = 0;
        for (int i = 0; i < n - 1; i++)
        {
            if (i < j)
            {
                // Swap
                var temp = data[i];
                data[i] = data[j];
                data[j] = temp;
            }

            int k = n / 2;
            while (k <= j)
            {
                j -= k;
                k /= 2;
            }
            j += k;
        }

        // Cooley-Tukey decimation-in-time radix-2 FFT
        for (int len = 2; len <= n; len *= 2)
        {
            double angle = -2.0 * Math.PI / len;
            Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

            for (int i = 0; i < n; i += len)
            {
                Complex w = Complex.One;
                for (int j2 = 0; j2 < len / 2; j2++)
                {
                    Complex u = data[i + j2];
                    Complex v = w * data[i + j2 + len / 2];

                    data[i + j2] = u + v;
                    data[i + j2 + len / 2] = u - v;

                    w *= wlen;
                }
            }
        }
    }

    /// <summary>
    /// Find the dominant frequency in Hz from FFT magnitudes
    /// </summary>
    public static double GetDominantFrequency(double[] magnitudes, int sampleRate)
    {
        if (magnitudes == null || magnitudes.Length == 0)
            return 0;

        // Find the index of maximum magnitude
        int maxIndex = 0;
        double maxMagnitude = magnitudes[0];

        for (int i = 1; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] > maxMagnitude)
            {
                maxMagnitude = magnitudes[i];
                maxIndex = i;
            }
        }

        // Convert bin index to frequency
        double frequencyResolution = (double)sampleRate / (2 * magnitudes.Length);
        return maxIndex * frequencyResolution;
    }

    /// <summary>
    /// Apply smoothing to FFT magnitudes
    /// </summary>
    public static double[] SmoothMagnitudes(double[] magnitudes, int windowSize = 3)
    {
        if (magnitudes == null || magnitudes.Length == 0)
            return magnitudes;

        double[] smoothed = new double[magnitudes.Length];
        int halfWindow = windowSize / 2;

        for (int i = 0; i < magnitudes.Length; i++)
        {
            double sum = 0;
            int count = 0;

            for (int j = Math.Max(0, i - halfWindow); j <= Math.Min(magnitudes.Length - 1, i + halfWindow); j++)
            {
                sum += magnitudes[j];
                count++;
            }

            smoothed[i] = sum / count;
        }

        return smoothed;
    }

    /// <summary>
    /// Convert magnitude to decibels
    /// </summary>
    public static double MagnitudeToDb(double magnitude)
    {
        return 20 * Math.Log10(Math.Max(magnitude, 1e-10));
    }

    /// <summary>
    /// Convert all magnitudes to decibels
    /// </summary>
    public static double[] MagnitudesToDb(double[] magnitudes)
    {
        double[] db = new double[magnitudes.Length];
        for (int i = 0; i < magnitudes.Length; i++)
        {
            db[i] = MagnitudeToDb(magnitudes[i]);
        }
        return db;
    }
}