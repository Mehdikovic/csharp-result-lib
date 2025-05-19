using ResultLib.Core;

namespace ResultLib.Tests {
    public class Option_T1_T2_UnitTes {
        [Test]
        public void Test_Success_Should_Be_SuccessState() {
            var option = Option<string, string>.Success();
            Assert.That(option.IsSuccess());
            Assert.That(!option.IsFailed());
            Assert.That(!option.IsCanceled());
        }

        [Test]
        public void Test_Success_With_Value_Should_Be_SuccessState() {
            var option = Option<string, string>.Success("Valid Data");
            Assert.That(option.IsSuccess());
            Assert.That(option.GetResultSuccess().Unwrap(), Is.EqualTo("Valid Data"));
        }

        [Test]
        public void Test_Failed_Should_Be_FailedState() {
            var option = Option<string, string>.Failed();
            Assert.That(option.IsFailed());
            Assert.That(!option.IsSuccess());
            Assert.That(!option.IsCanceled());
        }

        [Test]
        public void Test_Failed_With_Message_Should_Store_Error() {
            var option = Option<string, string>.Failed("Failure occurred");
            Assert.That(option.IsFailed());
            Assert.That(option.GetError().Message, Is.EqualTo("Failure occurred"));
        }

        [Test]
        public void Test_Failed_With_Exception_Should_Store_Exception() {
            var exception = new Exception("Critical failure");
            var option = Option<string, string>.Failed(exception);
            Assert.That(option.IsFailed());
            Assert.That(option.GetError(), Is.EqualTo(exception));
        }

        [Test]
        public void Test_Failed_With_Value_Should_Be_CanceledState() {
            var option = Option<string, string>.Failed("Failure occurred", "Some Data");
            Assert.That(option.IsFailed());
            Assert.That(option.GetResultFailed().Unwrap(), Is.EqualTo("Some Data"));
            Assert.That(option.GetError().Message, Is.EqualTo("Failure occurred"));
        }

        [Test]
        public void Test_Canceled_Should_Be_CanceledState() {
            var option = Option<string, string>.Canceled();
            Assert.That(option.IsCanceled());
            Assert.That(!option.IsSuccess());
            Assert.That(!option.IsFailed());
        }

        [Test]
        public void Test_Canceled_With_Value_Should_Be_CanceledState() {
            var option = Option<string, string>.Canceled("Some Data");
            Assert.That(option.IsCanceled());
            Assert.That(option.GetResultCanceled().Unwrap(), Is.EqualTo("Some Data"));
        }

        [Test]
        public void Test_IsSuccessOrCanceled_Should_Be_True_For_Success_And_Canceled() {
            Assert.That(Option<string, string>.Success().IsSuccessOrCanceled(), Is.True);
            Assert.That(Option<string, string>.Canceled().IsSuccessOrCanceled(), Is.True);
            Assert.That(Option<string, string>.Failed().IsSuccessOrCanceled(), Is.False);
        }

        [Test]
        public void Test_IsSuccessOrFailed_Should_Be_True_For_Success_And_Failed() {
            Assert.That(Option<string, string>.Success().IsSuccessOrFailed(), Is.True);
            Assert.That(Option<string, string>.Failed().IsSuccessOrFailed(), Is.True);
            Assert.That(Option<string, string>.Canceled().IsSuccessOrFailed(), Is.False);
        }

        [Test]
        public void Test_IsFailedOrCanceled_Should_Be_True_For_Failed_And_Canceled() {
            Assert.That(Option<string, string>.Failed().IsFailedOrCanceled(), Is.True);
            Assert.That(Option<string, string>.Canceled().IsFailedOrCanceled(), Is.True);
            Assert.That(Option<string, string>.Success().IsFailedOrCanceled(), Is.False);
        }

        [Test]
        public void Test_GetError_Should_Return_Correct_Exception() {
            var failedOption = Option<string, string>.Failed(new Exception("Test Failure"));
            Assert.That(failedOption.GetError().Message, Is.EqualTo("Test Failure"));

            var canceledOption = Option<string, string>.Canceled();
            Assert.That(canceledOption.GetError().GetType(), Is.EqualTo(typeof(OptionOperationCanceledException)));
        }

        [Test]
        public void Test_ThrowIfFailed_Should_Throw_For_Failed_Option() {
            var option = Option<string, string>.Failed("Error occurred");
            Assert.Throws<OptionException>(() => option.ThrowIfFailed());
        }

        [Test]
        public void Test_ThrowIfCanceled_Should_Throw_For_Canceled_Option() {
            var option = Option<string, string>.Canceled();
            Assert.Throws<OptionOperationCanceledException>(() => option.ThrowIfCanceled());
        }

        [Test]
        public void Test_Equals_Should_Compare_Correctly() {
            var option1 = Option<string, string>.Success("Data");
            var option2 = Option<string, string>.Success("Data");
            var option3 = Option<string, string>.Failed("Error");

            Assert.That(option1 == option2);
            Assert.That(option1 != option3);
        }

        [Test]
        public void Test_Comparison_Should_Work_Correctly() {
            var option1 = Option<string, string>.Success("Alpha");
            var option2 = Option<string, string>.Success();
            var option3 = Option<string, string>.Success("Beta");

            var option4 = Option<string, string>.Failed("Error");
            var option5 = Option<string, string>.Failed();

            var option6 = Option<string, string>.Canceled("Cancelled");
            var option7 = Option<string, string>.Canceled();


            Assert.That(
                option3 > option1
                && option1 > option2
                && option2 > option4
                && option4 > option5
                && option5 > option6
                && option6 > option7
            );
        }

        // cast
        [Test]
        public void Test_Casting_Ok() {
            Option<string, int> r = Option<string, int>.Success("ok");
            Option option = r.ToOption(); // boxing

            Option<string, int> newOpt = option.ToOption<string, int>();

            Assert.That(newOpt.IsSuccess(), Is.True);
            Assert.That(newOpt.GetResultSuccess().Unwrap(), Is.EqualTo("ok"));
        }

        [Test]
        public void Test_Casting_Error_String() {
            Option<string, int> r = Option<string, int>.Failed("error occurred!");
            Option option = r.ToOption(); // boxing

            Option<string, int> newOpt = option.ToOption<string, int>();

            Assert.That(newOpt.IsFailed(), Is.True);
            Assert.That(newOpt.GetError().Message, Is.EqualTo("error occurred!"));
        }

        [Test]
        public void Test_Casting_Exception() {
            Option<string, int> r = Option<string, int>.Failed(new InvalidOperationException("error happened!"));
            Option option = r.ToOption(); // boxing

            Option<string, int> newOpt = option.ToOption<string, int>();

            Assert.That(newOpt.IsFailed(), Is.True);
            Assert.That(newOpt.GetError().Message, Is.EqualTo("error happened!"));
            Assert.That(newOpt.GetError().GetType(), Is.EqualTo(typeof(InvalidOperationException)));
        }

        [Test]
        public void Test_Casting_Should_Fail_If_Result_Data_Type_Does_Not_Match() {
            Option<int, double> oSuccess = Option<int, double>.Success(10);
            Option<int, double> oFailed = Option<int, double>.Failed("error happened!", 10.0);
            Option<int, double> oCanceled = Option<int, double>.Canceled(10.0);

            Option option = oSuccess.ToOption(); // boxing
            Assert.Throws<OptionInvalidExplicitCastException>(() => option.ToOption<double, double>());

            option = oFailed.ToOption(); // boxing
            Assert.Throws<OptionInvalidExplicitCastException>(() => option.ToOption<int, int>());

            option = oCanceled.ToOption(); // boxing
            Assert.Throws<OptionInvalidExplicitCastException>(() => option.ToOption<int, char>());
        }

        [Test]
        public void Test_Casting_Should_Success_If_Result_Does_Not_Have_Data() {
            Option<int, double> oSuccess = Option<int, double>.Success();
            Option<int, double> oFailed = Option<int, double>.Failed("error happened!");
            Option<int, double> oCanceled = Option<int, double>.Canceled();

            Option option = oSuccess.ToOption(); // boxing
            var o1 = option.ToOption<string, char>();
            Assert.That(o1.IsSuccess(), Is.True);

            option = oFailed.ToOption(); // boxing
            var o2 = option.ToOption<string, char>();
            Assert.That(option.IsFailed(), Is.True);

            option = oCanceled.ToOption(); // boxing
            var o3 = option.ToOption<string, char>();
            Assert.That(option.IsCanceled(), Is.True);
        }

        [Test]
        public void Test_Casting_Should_Success_If_We_Cast_Back_To_Option_T_If_State_Match() {
            Option<int, double> oSuccess = Option<int, double>.Success(10);
            Option<int, double> oFailed = Option<int, double>.Failed("error happened!", 20.0);
            Option<int, string> oCanceled = Option<int, string>.Canceled("cancelled");

            Option option = oSuccess.ToOption(); // boxing
            var o1 = option.ToOption<int>();
            Assert.That(o1.IsSuccess(), Is.True);
            Assert.That(o1.GetResult().Unwrap(), Is.EqualTo(10));

            option = oFailed.ToOption(); // boxing
            var o2 = option.ToOption<double>();
            Assert.That(o2.IsFailed(), Is.True);
            Assert.That(o2.GetResult().Unwrap(), Is.EqualTo(20.0));

            option = oCanceled.ToOption(); // boxing
            var o3 = option.ToOption<string>();
            Assert.That(o3.IsCanceled(), Is.True);
            Assert.That(o3.GetResult().Unwrap(), Is.EqualTo("cancelled"));
        }

        [Test]
        public void Test_Casting_Should_Fail_If_We_Cast_Back_To_Option_T_And_Result_Has_Value_If_State_Does_Not_Match() {
            Option<int, double> oSuccess = Option<int, double>.Success(10);
            Option<int, double> oFailed = Option<int, double>.Failed("error happened!", 20.0);
            Option<int, string> oCanceled = Option<int, string>.Canceled("cancelled");

            Option option = oSuccess.ToOption(); // boxing
            Assert.Throws<OptionInvalidExplicitCastException>(() => option.ToOption<double>());

            option = oFailed.ToOption(); // boxing
            Assert.Throws<OptionInvalidExplicitCastException>(() => option.ToOption<string>());

            option = oCanceled.ToOption(); // boxing
            Assert.Throws<OptionInvalidExplicitCastException>(() => option.ToOption<int>());
        }

        [Test]
        public void Test_Casting_Should_Success_If_We_Cast_Back_To_Option_T_If_Result_Does_Not_Have_Data_And_Mistyped() {
            Option<int, double> oSuccess = Option<int, double>.Success();
            Option<int, double> oFailed = Option<int, double>.Failed("error happened!");
            Option<int, double> oCanceled = Option<int, double>.Canceled();

            Option option = oSuccess.ToOption();
            var o1 = option.ToOption<double>();// boxing
            Assert.That(o1.IsSuccess(), Is.True);

            option = oFailed.ToOption(); // boxing
            var o2 = option.ToOption<string>();// boxing
            Assert.That(o2.IsFailed(), Is.True);

            option = oCanceled.ToOption(); // boxing
            var o3 = option.ToOption<int>();// boxing
            Assert.That(o3.IsCanceled(), Is.True);
        }
        
        // other
        [Test]
        public void Test_Merging_Options_Should_Work_If_Success_And_Types_Match_Or_Dont() {
            var option1 = Option<int, string, string>.Success(100);
            Option<int, string> o = option1.ToMergedOption();
            Assert.That(o.IsSuccess(), Is.True);
            Assert.That(o.GetResultSuccess().Some(), Is.EqualTo(100));
            
            var option2 = Option<int, string, double>.Success(100);
            Option<int, string> o2 = option1.ToMergedOption();
            Assert.That(o.IsSuccess(), Is.True);
            Assert.That(o.GetResultSuccess().Some(), Is.EqualTo(100));
        }
        
        [Test]
        public void Test_Merging_Options_Should_Work_If_FailedOrCanceled_And_Types_Match() {
            var option1 = Option<int, string, string>.FailedValue("failed");
            Option<int, string> o1 = option1.ToMergedOption();
            Assert.That(o1.IsFailed(), Is.True);
            Assert.That(o1.GetResultFailed().Some(), Is.EqualTo("failed"));
            Assert.That(o1.GetResultCanceled().IsOk(), Is.EqualTo(false));
            
            var option2 = Option<int, string, string>.Canceled("cancelled");
            Option<int, string> o2 = option2.ToMergedOption();
            Assert.That(o2.IsCanceled(), Is.True);
            Assert.That(o2.GetResultCanceled().Some(), Is.EqualTo("cancelled"));
            Assert.That(o2.GetResultFailed().IsOk(), Is.EqualTo(false));
        }
        
        [Test]
        public void Test_Merging_Options_Should_Not_Work_If_FailedOrCanceled_And_Types_Dont_Match() {
            var option1 = Option<int, string, double>.FailedValue("failed");
            Assert.Throws<OptionException>(() => option1.ToMergedOption());
            
            var option2 = Option<int, int, string>.Canceled("cancelled");
            Assert.Throws<OptionException>(() => option2.ToMergedOption());
        }
        
        // other
        [Test]
        public void Test_ToString_Should_Work_Correctly() {
            var option1 = Option<string, string>.Success("100");
            var option2 = Option<string, string>.Failed();
            var option3 = Option<string, string>.Canceled();

            Assert.That(option1.ToString(), Is.EqualTo("Success; Value: {Ok = 100}"));
            Assert.That(option2.ToString(), Is.EqualTo("Failed; Error = something went wrong in Option.; Value: {Error = something went wrong in Result.}"));
            Assert.That(option3.ToString(), Is.EqualTo("Canceled; Value: {Error = something went wrong in Result.}"));
        }
    }
}
