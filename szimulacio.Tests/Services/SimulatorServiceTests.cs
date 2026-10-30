using backend.Services;

namespace szimulacio.Tests.Services;

[TestClass]
public class SimulatorServiceTests
{
    [TestMethod]
    public void GenerateEvent_ReturnsEventWithValidValues()
    {
        var simulatorService = new SimulatorService();

        var simulatedEvent = simulatorService.GenerateEvent();

        Assert.IsFalse(string.IsNullOrWhiteSpace(simulatedEvent.ServiceCode));
        Assert.IsTrue(Enum.IsDefined(typeof(backend.Models.EventType), simulatedEvent.EventType));
        Assert.IsTrue(Enum.IsDefined(typeof(backend.Models.Severity), simulatedEvent.Severity));
        Assert.AreNotEqual(default, simulatedEvent.OccurredAtUtc);
    }
}