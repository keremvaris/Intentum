using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.Education;

public static class EducationRules
{
    public static Func<BehaviorSpace, RuleMatch?> StudentDropoutRisk(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("engagement.declining", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("assignment.missed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("attendance.issue", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("grade.declining", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("StudentDropoutRisk", confidence,
                $"Risk signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> AcademicIntegrityViolation(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("submission.rapid", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("submission.multiple.attempts", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("access.off.hours", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("copy.paste.excessive", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("AcademicIntegrityViolation", confidence,
                $"Integrity signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> CourseRecommendation(
        int minViews = 2,
        double confidence = 0.75) => space =>
    {
        var distinctCourseViews = space.Events
            .Where(e => e.Action.StartsWith("course.view.", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Action)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (distinctCourseViews >= minViews)
            return new RuleMatch("CourseRecommendation", confidence,
                $"Distinct course views: {distinctCourseViews}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> LearningResourceNeeds(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.StartsWith("help.requested.", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("LearningResourceNeeds", confidence,
                $"Help requests: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> EarlyWarning(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("grade.declining", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("behavioral.flag", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("performance.drop", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("teacher.concern", StringComparison.OrdinalIgnoreCase));

        if (signals >= 1)
            return new RuleMatch("EarlyWarning", confidence,
                $"Warning signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> InterventionRequired(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("intervention.declined", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("performance.sustained_low", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("support.resistant", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("progress.stagnant", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("InterventionRequired", confidence,
                $"Intervention signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> GiftedStudent(
        int minAdvancedActivities = 2,
        double confidence = 0.7) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("course.advanced.completed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("enrichment.requested", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("grade.exceeds", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("acceleration.recommended", StringComparison.OrdinalIgnoreCase));

        if (signals >= minAdvancedActivities)
            return new RuleMatch("GiftedStudent", confidence,
                $"Advanced activity signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> CareerPathInterest(
        double confidence = 0.75) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("course.career_focused", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("extracurricular.career", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("internship.interested", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("mentorship.requested", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("CareerPathInterest", confidence,
                $"Career signals: {signals}");
        return null;
    };

    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        StudentDropoutRisk(),
        AcademicIntegrityViolation(),
        CourseRecommendation(),
        LearningResourceNeeds(),
        EarlyWarning(),
        InterventionRequired(),
        GiftedStudent(),
        CareerPathInterest()
    ];
}
