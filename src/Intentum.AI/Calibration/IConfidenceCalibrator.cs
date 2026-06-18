namespace Intentum.AI.Calibration;

public interface IConfidenceCalibrator
{
    double Calibrate(double rawScore);
}
