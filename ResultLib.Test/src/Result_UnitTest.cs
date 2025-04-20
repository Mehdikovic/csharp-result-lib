namespace ResultLib.Test;

public class Tests {
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_DefaultConstructorShouldBeInStateError() {
        var result = new Result();
        Assert.Multiple(() =>
        {
            Assert.That(result.IsError());
            Assert.That(!result.IsOk());
        });
    }
}
