using HealthTech.Gateway.Security;
using Xunit;

namespace HealthTech.Gateway.Tests;

public sealed class BffOnboardingDecisionTests
{
    [Fact]
    public void Requires_onboarding_for_authenticated_user_without_membership()
    {
        var decision = BffOnboardingDecision.Create(
            authenticated: true,
            hasMemberships: false,
            onboardingComplete: false);

        Assert.True(decision.RequiresOnboarding);
    }

    [Fact]
    public void Does_not_require_onboarding_for_authenticated_user_with_completed_membership()
    {
        var decision = BffOnboardingDecision.Create(
            authenticated: true,
            hasMemberships: true,
            onboardingComplete: true);

        Assert.False(decision.RequiresOnboarding);
    }

    [Fact]
    public void Anonymous_session_never_requires_onboarding()
    {
        var decision = BffOnboardingDecision.Create(
            authenticated: false,
            hasMemberships: false,
            onboardingComplete: false);

        Assert.False(decision.RequiresOnboarding);
    }
}
