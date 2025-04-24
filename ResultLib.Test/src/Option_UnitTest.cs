using ResultLib.Core;

namespace ResultLib.Tests {
    public class Option_UnitTes {
        [Test]
        public void Test_Success_Should_Be_SuccessState() {
            var option = Option.Success();
            Assert.That(option.IsSuccess());
            Assert.That(!option.IsFailed());
            Assert.That(!option.IsCanceled());
        }

        [Test]
        public void Test_Success_With_Value_Should_Be_SuccessState() {
            var option = Option.Success("Valid Data");
            Assert.That(option.IsSuccess());
            Assert.That(option.GetResult().Unwrap(), Is.EqualTo("Valid Data"));
        }

        [Test]
        public void Test_Failed_Should_Be_FailedState() {
            var option = Option.Failed();
            Assert.That(option.IsFailed());
            Assert.That(!option.IsSuccess());
            Assert.That(!option.IsCanceled());
        }

        [Test]
        public void Test_Failed_With_Message_Should_Store_Error() {
            var option = Option.Failed("Failure occurred");
            Assert.That(option.IsFailed());
            Assert.That(option.GetError().Message, Is.EqualTo("Failure occurred"));
        }

        [Test]
        public void Test_Failed_With_Exception_Should_Store_Exception() {
            var exception = new Exception("Critical failure");
            var option = Option.Failed(exception);
            Assert.That(option.IsFailed());
            Assert.That(option.GetError(), Is.EqualTo(exception));
        }

        [Test]
        public void Test_Failed_With_Value_Should_Be_CanceledState() {
            var option = Option.Failed("Failure occurred", "Some Data");
            Assert.That(option.IsFailed());
            Assert.That(option.GetResult().Unwrap(), Is.EqualTo("Some Data"));
            Assert.That(option.GetError().Message, Is.EqualTo("Failure occurred"));
        }

        [Test]
        public void Test_Canceled_Should_Be_CanceledState() {
            var option = Option.Canceled();
            Assert.That(option.IsCanceled());
            Assert.That(!option.IsSuccess());
            Assert.That(!option.IsFailed());
        }

        [Test]
        public void Test_Canceled_With_Value_Should_Be_CanceledState() {
            var option = Option.Canceled("Some Data");
            Assert.That(option.IsCanceled());
            Assert.That(option.GetResult().Unwrap(), Is.EqualTo("Some Data"));
        }

        [Test]
        public void Test_IsSuccessOrCanceled_Should_Be_True_For_Success_And_Canceled() {
            Assert.That(Option.Success().IsSuccessOrCanceled(), Is.True);
            Assert.That(Option.Canceled().IsSuccessOrCanceled(), Is.True);
            Assert.That(Option.Failed().IsSuccessOrCanceled(), Is.False);
        }

        [Test]
        public void Test_IsSuccessOrFailed_Should_Be_True_For_Success_And_Failed() {
            Assert.That(Option.Success().IsSuccessOrFailed(), Is.True);
            Assert.That(Option.Failed().IsSuccessOrFailed(), Is.True);
            Assert.That(Option.Canceled().IsSuccessOrFailed(), Is.False);
        }

        [Test]
        public void Test_IsFailedOrCanceled_Should_Be_True_For_Failed_And_Canceled() {
            Assert.That(Option.Failed().IsFailedOrCanceled(), Is.True);
            Assert.That(Option.Canceled().IsFailedOrCanceled(), Is.True);
            Assert.That(Option.Success().IsFailedOrCanceled(), Is.False);
        }

        [Test]
        public void Test_GetError_Should_Return_Correct_Exception() {
            var failedOption = Option.Failed(new Exception("Test Failure"));
            Assert.That(failedOption.GetError().Message, Is.EqualTo("Test Failure"));

            var canceledOption = Option.Canceled();
            Assert.That(canceledOption.GetError().GetType(), Is.EqualTo(typeof(OptionOperationCanceledException)));
        }

        [Test]
        public void Test_ThrowIfFailed_Should_Throw_For_Failed_Option() {
            var option = Option.Failed("Error occurred");
            Assert.Throws<OptionException>(() => option.ThrowIfFailed());
        }

        [Test]
        public void Test_ThrowIfCanceled_Should_Throw_For_Canceled_Option() {
            var option = Option.Canceled();
            Assert.Throws<OptionOperationCanceledException>(() => option.ThrowIfCanceled());
        }

        [Test]
        public void Test_Equals_Should_Compare_Correctly() {
            var option1 = Option.Success("Data");
            var option2 = Option.Success("Data");
            var option3 = Option.Failed("Error");

            Assert.That(option1 == option2);
            Assert.That(option1 != option3);
        }

        [Test]
        public void Test_Comparison_Should_Work_Correctly() {
            var option1 = Option.Success("Alpha");
            var option2 = Option.Success();
            var option3 = Option.Success("Beta");

            var option4 = Option.Failed("Error");
            var option5 = Option.Failed();

            var option6 = Option.Canceled("Cancelled");
            var option7 = Option.Canceled();


            Assert.That(
                option3 > option1
                && option1 > option2
                && option2 > option4
                && option4 > option5
                && option5 > option6
                && option6 > option7
            );
        }
        
        // other
        [Test]
        public void Test_ToString_Should_Work_Correctly() {
            var option1 =Option.Success(100);
            var option2 = Option.Failed();
            var option3 = Option.Canceled();

            Assert.That(option1.ToString(), Is.EqualTo("Success; Value: {Ok = 100}"));
            Assert.That(option2.ToString(), Is.EqualTo("Failed; Error = something went wrong in Option.; Value: {Error = something went wrong in Result.}"));
            Assert.That(option3.ToString(), Is.EqualTo("Canceled; Value: {Error = something went wrong in Result.}"));
        }
    }
}
