namespace Intentum.AI.Calibration;

public sealed class PlattCalibrator : IConfidenceCalibrator
{
    private readonly double _a;
    private readonly double _b;

    public PlattCalibrator(double a = 1.0, double b = 0.0)
    {
        _a = a;
        _b = b;
    }

    public double Calibrate(double rawScore)
    {
        var logit = _a * rawScore + _b;
        return 1.0 / (1.0 + Math.Exp(-logit));
    }
}
