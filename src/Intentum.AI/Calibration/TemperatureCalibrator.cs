namespace Intentum.AI.Calibration;

public sealed class TemperatureCalibrator : IConfidenceCalibrator
{
    private readonly double _temperature;

    public TemperatureCalibrator(double temperature = 1.0)
    {
        _temperature = Math.Max(0.01, temperature);
    }

    public double Calibrate(double rawScore)
    {
        return 1.0 / (1.0 + Math.Exp(-rawScore / _temperature));
    }
}
