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
    
    [Test]
    public void Test_DefaultConstructorShouldReturnEmptyConstructorError() {
        var result = new Result();
        Assert.That(result.IsError(out string errorMessage) && errorMessage == "Result:: Must be instantiated with Static Methods or Factory.");
    }
}
