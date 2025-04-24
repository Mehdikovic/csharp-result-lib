using ResultLib.ResultFactoryExtensions;

namespace ResultLib.Test {
    public class Result_FactoryExtensions_UnitTest {
        [SetUp]
        public void Setup()
        {
        }
        
        [Test]
        public void Test_Create_With_Extensions_From_Required() {
            Result<int> r2 = 1.FromRequired();
            Assert.That(r2.IsOk(out int value), Is.True);
            Assert.That(value, Is.EqualTo(1));
            
            Result<string> r3 = "string".FromRequired();
            Assert.That(r3.IsOk(out string value3), Is.True);
            Assert.That(value3, Is.EqualTo("string"));
            
            Result<string> r = ((string)null).FromRequired();
            Assert.That(r.IsOk(), Is.False);
            Assert.That(r.IsError(), Is.True);
        }

        [Test]
        public void Test_Create_With_Extensions_With_Ok() {
            Result<string> r = ((string)null).Ok();
            Assert.That(r.IsOk(), Is.True);

            Result<int> r2 = 1.Ok();
            Assert.That(r2.IsOk(out int value), Is.True);
            Assert.That(value, Is.EqualTo(1));
            
            Result<string> r3 = "string".Ok();
            Assert.That(r3.IsOk(out string value3), Is.True);
            Assert.That(value3, Is.EqualTo("string"));
        }
        
        [Test]
        public void Test_Create_With_Extensions_Error() {
            Result<string> r = ((string)null).Error();
            Assert.That(r.IsError(), Is.True);

            Result<int> r2 = 1.Error();
            Assert.That(r2.IsError(), Is.True);
            
            Result<string> r3 = "string".Error();
            Assert.That(r3.IsError(), Is.True);
        }
    }
}
