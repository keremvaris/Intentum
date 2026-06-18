using Intentum.AI.Calibration;

namespace Intentum.Tests.AI;

public class CalibrationTests
{
    [Fact]
    public void PlattCalibrator_WithHighScore_ReturnsCalibratedScore()
    {
        var calibrator = new PlattCalibrator(a: 2.0, b: -1.0);
        var result = calibrator.Calibrate(0.9);
        Assert.InRange(result, 0.6, 1.0);
    }

    [Fact]
    public void PlattCalibrator_WithLowScore_ReturnsLowCalibratedScore()
    {
        var calibrator = new PlattCalibrator(a: 2.0, b: -1.0);
        var result = calibrator.Calibrate(0.1);
        Assert.InRange(result, 0.0, 0.4);
    }

    [Fact]
    public void PlattCalibrator_ScoreZero_SigmoidAtZero()
    {
        var calibrator = new PlattCalibrator(a: 1.0, b: 0.0);
        var result = calibrator.Calibrate(0.0);
        Assert.Equal(0.5, result, 4);
    }

    [Fact]
    public void TemperatureCalibrator_DefaultTemperature_ReturnsStandardSigmoid()
    {
        var calibrator = new TemperatureCalibrator(temperature: 1.0);
        var result = calibrator.Calibrate(0.8);
        Assert.Equal(0.68997, result, 4);
    }

    [Fact]
    public void TemperatureCalibrator_HighTemperature_FlattensDistribution()
    {
        var calibrator = new TemperatureCalibrator(temperature: 5.0);
        var high = calibrator.Calibrate(0.9);
        var low = calibrator.Calibrate(0.1);
        Assert.True(Math.Abs(high - low) < 0.3);
    }

    [Fact]
    public void TemperatureCalibrator_LowTemperature_SharpensDistribution()
    {
        var calibrator = new TemperatureCalibrator(temperature: 0.5);
        var high = calibrator.Calibrate(0.9);
        Assert.InRange(high, 0.85, 0.87);
    }

    [Fact]
    public void PlattCalibrator_NegativeA_FlipsScore()
    {
        var calibrator = new PlattCalibrator(a: -2.0, b: 0.5);
        var highScore = calibrator.Calibrate(0.9);
        var lowScore = calibrator.Calibrate(0.1);
        Assert.True(highScore < lowScore);
    }
}
