namespace Oip.Settings.Tests;

[TestFixture]
public class BaseAppSettingsTests
{
    private class TestAppSettings : BaseAppSettings<TestAppSettings>
    {
        public string TestProperty { get; set; } = "initial";
    }

    [SetUp]
    public void Setup()
    {
        // Reset state before each test
        TestAppSettings.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        // Clear subscriptions after each test
        TestAppSettings.Instance.OnChange -= null;
    }

    [Test]
    public void OnChange_Event_ShouldBeInvoked_WhenRebindIsCalled()
    {
        // Arrange
        bool eventInvoked = false;
        TestAppSettings.Instance.OnChange += () => eventInvoked = true;

        var settings = TestAppSettings.Instance;

        // Act
        settings.Rebind();

        // Assert
        Assert.That(eventInvoked, Is.True, "OnChange event should be invoked when Rebind is called");
    }

    [Test]
    public void OnChange_Event_ShouldBeInvokedOnlyOnce_WhenMultipleSubscribers()
    {
        // Arrange
        int invocationCount = 0;
        TestAppSettings.Instance.OnChange += () => invocationCount++;
        TestAppSettings.Instance.OnChange += () => invocationCount++;

        var settings = TestAppSettings.Instance;

        // Act
        settings.Rebind();

        // Assert
        Assert.That(invocationCount, Is.EqualTo(2), "Event should be invoked for each subscriber");
    }

    [Test]
    public void OnChange_Event_ShouldNotBeInvoked_WhenNoSubscribers()
    {
        // Arrange
        var settings = TestAppSettings.Instance;

        // Act & Assert - should not throw exceptions when there are no subscribers
        Assert.DoesNotThrow(() => settings.Rebind());
    }

    [Test]
    public void OnChange_Event_ShouldWorkCorrectly_WhenSubscriberIsAddedAndRemoved()
    {
        // Arrange
        int invocationCount = 0;
        Action handler = () => invocationCount++;

        var settings = TestAppSettings.Instance;

        // Act - add and immediately remove subscriber
        TestAppSettings.Instance.OnChange += handler;
        TestAppSettings.Instance.OnChange -= handler;
        settings.Rebind();

        // Assert
        Assert.That(invocationCount, Is.EqualTo(0), "Event should not be invoked for removed subscriber");
    }

    [Test]
    public void OnChange_Event_ShouldBeThreadSafe()
    {
        // Arrange
        int invocationCount = 0;
        object lockObject = new object();

        TestAppSettings.Instance.OnChange += () =>
        {
            lock (lockObject)
            {
                invocationCount++;
            }
        };

        var settings = TestAppSettings.Instance;

        // Act - simulate multi-threaded calls
        Parallel.Invoke(
            () => settings.Rebind(),
            () => settings.Rebind(),
            () => settings.Rebind()
        );

        // Assert
        Assert.That(invocationCount, Is.EqualTo(3), "Event should be invoked 7 times in multi-threaded environment");
    }
}