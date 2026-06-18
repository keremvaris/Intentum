using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Education;

namespace Intentum.Tests;

public sealed class EducationRulesTests
{
    [Fact]
    public void StudentDropoutRisk_WithMultipleRiskSignals_ReturnsMatch()
    {
        var rule = EducationRules.StudentDropoutRisk();
        var space = new BehaviorSpace()
            .Observe("system", "engagement.declining")
            .Observe("system", "assignment.missed");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("StudentDropoutRisk", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void StudentDropoutRisk_WithNoSignals_ReturnsNull()
    {
        var rule = EducationRules.StudentDropoutRisk();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AcademicIntegrityViolation_WithMultipleSuspiciousActions_ReturnsMatch()
    {
        var rule = EducationRules.AcademicIntegrityViolation();
        var space = new BehaviorSpace()
            .Observe("system", "submission.rapid")
            .Observe("system", "access.off.hours");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AcademicIntegrityViolation", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void AcademicIntegrityViolation_WithNoSuspiciousActions_ReturnsNull()
    {
        var rule = EducationRules.AcademicIntegrityViolation();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void CourseRecommendation_WithMinViews_ReturnsMatch()
    {
        var rule = EducationRules.CourseRecommendation(minViews: 2);
        var space = new BehaviorSpace()
            .Observe("student", "course.view.math")
            .Observe("student", "course.view.science");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("CourseRecommendation", match.Name);
        Assert.Equal(0.75, match.Score);
    }

    [Fact]
    public void CourseRecommendation_WithSingleView_ReturnsNull()
    {
        var rule = EducationRules.CourseRecommendation(minViews: 2);
        var space = new BehaviorSpace()
            .Observe("student", "course.view.math");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void LearningResourceNeeds_WithMultipleHelpRequests_ReturnsMatch()
    {
        var rule = EducationRules.LearningResourceNeeds();
        var space = new BehaviorSpace()
            .Observe("student", "help.requested.math")
            .Observe("student", "help.requested.science");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("LearningResourceNeeds", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void LearningResourceNeeds_WithSingleHelpRequest_ReturnsNull()
    {
        var rule = EducationRules.LearningResourceNeeds();
        var space = new BehaviorSpace()
            .Observe("student", "help.requested.math");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void EarlyWarning_WithGradeDeclining_ReturnsMatch()
    {
        var rule = EducationRules.EarlyWarning();
        var space = new BehaviorSpace()
            .Observe("system", "grade.declining");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("EarlyWarning", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void EarlyWarning_WithNoSignals_ReturnsNull()
    {
        var rule = EducationRules.EarlyWarning();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void InterventionRequired_WithMultipleSignals_ReturnsMatch()
    {
        var rule = EducationRules.InterventionRequired();
        var space = new BehaviorSpace()
            .Observe("system", "intervention.declined")
            .Observe("system", "progress.stagnant");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("InterventionRequired", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void InterventionRequired_WithNoSignals_ReturnsNull()
    {
        var rule = EducationRules.InterventionRequired();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void GiftedStudent_WithMultipleAdvancedActivities_ReturnsMatch()
    {
        var rule = EducationRules.GiftedStudent(minAdvancedActivities: 2);
        var space = new BehaviorSpace()
            .Observe("system", "course.advanced.completed")
            .Observe("system", "enrichment.requested");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("GiftedStudent", match.Name);
        Assert.Equal(0.7, match.Score);
    }

    [Fact]
    public void GiftedStudent_WithSingleAdvancedActivity_ReturnsNull()
    {
        var rule = EducationRules.GiftedStudent(minAdvancedActivities: 2);
        var space = new BehaviorSpace()
            .Observe("system", "course.advanced.completed");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void CareerPathInterest_WithMultipleCareerSignals_ReturnsMatch()
    {
        var rule = EducationRules.CareerPathInterest();
        var space = new BehaviorSpace()
            .Observe("student", "course.career_focused")
            .Observe("student", "internship.interested");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("CareerPathInterest", match.Name);
        Assert.Equal(0.75, match.Score);
    }

    [Fact]
    public void CareerPathInterest_WithNoSignals_ReturnsNull()
    {
        var rule = EducationRules.CareerPathInterest();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AllRules_ReturnsEightRules()
    {
        var rules = EducationRules.AllRules();

        Assert.Equal(8, rules.Count);
    }
}
