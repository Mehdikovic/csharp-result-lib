using ResultLib.Core;

namespace ResultLib.Test;

public class Result_Tests {
    [SetUp]
    public void Setup()
    {
    }

    // BEGIN Default public constructor behavior

    [Test]
    public void Test_DefaultConstructorShouldBeInStateError() {
        var result = new Result();
        Assert.Multiple(
            () => {
                Assert.That(result.IsError());
                Assert.That(!result.IsOk());
            }
        );
    }

    [Test]
    public void Test_DefaultConstructorShouldReturnEmptyConstructorError() {
        var result = new Result();
        Assert.That(result.IsError(out string errorMessage) && errorMessage == "Result:: Must be instantiated with Static Methods or Factory.");
    }

    [Test]
    public void Test_DefaultConstructorThrowsDefaultConstructorExceptionIfUnwrapErrIsCalled() {
        var result = new Result();
        Assert.Throws<ResultDefaultConstructorException>(() => result.UnwrapErr());
    }

    [Test]
    public void Test_DefaultConstructorThrowsDefaultConstructorExceptionIfThrowIfErrorIsCalled() {
        var result = new Result();
        Assert.Throws<ResultDefaultConstructorException>(() => result.ThrowIfError());
    }

    // END Default public constructor behavior


    // BEGIN value and error state
    [Test]
    public void Test_Ok_With_Value_Should_Be_Ok() {
        var result = Result.Ok("Success");
        Assert.That(result.IsOk());
        Assert.That(result.Unwrap(), Is.EqualTo("Success"));
        Assert.Throws<ResultUnwrapErrorException>(() => result.UnwrapErr());
        Assert.DoesNotThrow(() => result.ThrowIfError());
    }

    [Test]
    public void Test_Error_Without_Message_Should_Be_Error() {
        var result = Result.Error();
        Assert.That(result.IsError(out string message));
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));
        Assert.That(result.UnwrapErr().GetType(), Is.EqualTo(typeof(ResultException)));
        Assert.DoesNotThrow(() => result.UnwrapErr());
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_Error_With_Message_Should_Be_Error() {
        var result = Result.Error("Something went wrong");
        Assert.That(result.IsError(out string message));
        Assert.That(message, Is.EqualTo("Something went wrong"));
        Assert.That(result.UnwrapErr().GetType(), Is.EqualTo(typeof(ResultException)));
        Assert.DoesNotThrow(() => result.UnwrapErr());
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_IsOk_Should_Return_Correct_State() {
        var success = Result.Ok();
        var failure = Result.Error();

        Assert.That(success.IsOk(), Is.True);
        Assert.That(success.IsError(), Is.False);
        Assert.That(failure.IsOk(), Is.False);
        Assert.That(failure.IsError(), Is.True);
    }

    [Test]
    public void Test_Unwrap_Should_Return_Value_When_Ok() {
        var result = Result.Ok("ExpectedValue");
        Assert.That(result.Unwrap(), Is.EqualTo("ExpectedValue"));
    }

    [Test]
    public void Test_Unwrap_Should_Throw_Exception_When_Error() {
        var result = Result.Error("Some error");
        Assert.Throws<ResultUnwrapException>(() => result.Unwrap());
    }

    [Test]
    public void Test_Unwrap_With_Default_Should_Return_Default_When_Error() {
        var result = Result.Error();
        Assert.That(result.Unwrap("DefaultValue"), Is.EqualTo("DefaultValue"));
    }

    [Test]
    public void Test_Unwrap_With_Func_Should_Return_Provided_Value_When_Error() {
        var result = Result.Error();
        Assert.That(result.Unwrap(() => "FallbackValue"), Is.EqualTo("FallbackValue"));
    }

    [Test]
    public void Test_Unwrap_With_Func_Should_Throw_Exception_If_Func_Is_Null_And_Error() {
        var result = Result.Error();
        Assert.Throws<ArgumentNullException>(() => result.Unwrap(func: null));
    }

    [Test]
    public void Test_Unwrap_With_Null_Func_Should_Not_Throw_Exception_If_Is_Ok() {
        var result = Result.Ok();
        Assert.DoesNotThrow(() => result.Unwrap(func: null));
        Assert.That(result.Unwrap(func: null), Is.EqualTo(null));
    }

    [Test]
    public void Test_Unwrap_With_Null_Func_Should_Throws_Exception_When_Error() {
        var result = Result.Error();
        Assert.Throws<ArgumentNullException>(() => result.Unwrap(null));
    }

    [Test]
    public void Test_UnwrapErr_Should_Return_Exception_When_Error() {
        var result = Result.Error("Critical failure");
        var exception = result.UnwrapErr();
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Critical failure"));
    }

    [Test]
    public void Test_UnwrapErr_Should_Return_InnerException_When_Error_When_Exception_Is_Passed() {
        var result = Result.Error(new InvalidOperationException("Critical failure"));
        var exception = result.UnwrapErr();
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Result:: Something went wrong."));
        Assert.That(exception.InnerException, Is.Not.Null);
        Assert.That(exception.InnerException.Message, Is.EqualTo("Critical failure"));
    }

    [Test]
    public void Test_ThrowIf_Should_Throw_Exception_When_Error() {
        var result = Result.Error(new InvalidOperationException("Critical failure"));
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_ThrowIf_Should_Not_Throw_Exception_When_Ok() {
        var result = Result.Ok();
        Assert.DoesNotThrow(() => result.ThrowIfError());
    }

    [Test]
    public void Test_UnwrapErr_Should_Throw_Exception_When_Ok() {
        var result = Result.Ok();
        Assert.Throws<ResultUnwrapErrorException>(() => result.UnwrapErr());
    }

    [Test]
    public void Test_Some_Should_Return_True_When_Value_Is_Not_Null() {
        var result = Result.Ok("ValidValue");
        Assert.That(result.Some(out object value));
        Assert.That(value, Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Some_Should_Return_False_When_Value_Is_Null() {
        var result = Result.Ok(null);
        Assert.That(result.Some(out object value), Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void Test_Some_Should_Not_Return_Default_Value_When_Value_Is_Not_Null_And_Ok() {
        var result = Result.Ok("Default Value");
        Assert.That(result.Some("Object"), Is.EqualTo("Default Value"));
        Assert.That(result.Some(() => "Func Object"), Is.EqualTo("Default Value"));
    }

    [Test]
    public void Test_Some_Should_Return_Default_Value_When_Value_Is_Null_And_Ok() {
        var result = Result.Ok(null);
        Assert.That(result.Some("Object"), Is.EqualTo("Object"));
        Assert.That(result.Some(() => "Func Object"), Is.EqualTo("Func Object"));
    }

    [Test]
    public void Test_Some_Should_Return_Default_Value_When_Not_OK() {
        var result = Result.Error();
        Assert.That(result.Some("ValidValue"), Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Some_Should_Return_Default_Value_From_func_When_Not_OK() {
        var result = Result.Error();
        Assert.That(result.Some(() => "ValidValue"), Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Some_Should_Not_Throw_Exception_When_Default_Value_Is_Called_With_Null_And_Ok() {
        var result = Result.Ok("ValidValue");
        Assert.DoesNotThrow(() => result.Some(defaultValue: null));
        Assert.DoesNotThrow(() => result.Some(func: null));
        Assert.DoesNotThrow(() => result.Some(() => null));
    }

    [Test]
    public void Test_Some_Should_Throw_Exception_When_Default_Value_Is_Called_With_Null_And_Error() {
        var result = Result.Error("ErrorMessage");
        Assert.Throws<ArgumentNullException>(() => result.Some(defaultValue: null));
        Assert.Throws<ArgumentNullException>(() => result.Some(func: null));
        Assert.Throws<ResultInvalidSomeOperationException>(() => result.Some(() => null));
    }

    [Test]
    public void Test_Generic_Some_Should_Return_False_When_Value_Is_Not_Correctly_Typed() {
        var result = Result.Ok("ValidValue");
        Assert.That(result.Some(out int value), Is.False);
        Assert.That(value, Is.EqualTo(default(int)));
    }

    [Test]
    public void Test_Generic_Some_Should_Return_Default_Value_When_Value_Is_Not_Correctly_Typed() {
        var result = Result.Ok("ValidValue");
        Assert.That(result.Some(5), Is.EqualTo(5));
        Assert.That(result.Some(() => 6), Is.EqualTo(6));
    }

    [Test]
    public void Test_Generic_Some_Should_Return_Value() {
        var result = Result.Ok("ValidValue");
        Assert.That(result.Some<string>(), Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Unwrap_Can_Return_Null_When_Error_And_Other_Overloads_Are_Called() {
        var result = Result.Error();
        Assert.That(result.Unwrap(func: () => (int?)null), Is.EqualTo(null));

        var result2 = Result.Error();
        Assert.That(result.Unwrap(defaultValue: null), Is.EqualTo(null));
    }

    [Test]
    public void Test_Generic_Some_Should_Throw_Exception_If_Null_Or_Mistyped() {
        var result = Result.Ok(null);
        Assert.Throws<ResultInvalidSomeOperationException>(() => result.Some<string>());

        var result2 = Result.Ok(123);
        Assert.Throws<ResultInvalidSomeOperationException>(() => result2.Some<string>());
    }

    [Test]
    public void Test_FromRequired_Should_Avoid_Result_Ok_When_Value_Is_NUll() {
        var result = Result.FromRequired(null);
        Assert.That(result.IsError(), Is.True);
        Assert.That(result.IsOk(), Is.False);
    }

    [Test]
    public void Test_FromRequired_Should_return_Result_Ok_When_Value_Is_Not_NUll() {
        var result = Result.FromRequired("something");
        Assert.That(result.IsOk(), Is.True);
        Assert.That(result.IsError(), Is.False);
    }

    [Test]
    public void Test_ThrowIfError_Should_Throw_If_Result_Is_Error() {
        var result = Result.Error("Failure occurred");
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_Match_Should_Invoke_Correct_Function() {
        var okResult = Result.Ok("Data Loaded");
        var errorResult = Result.Error("Load Failed");

        string okMessage = okResult.Match(
            onOk: value => $"Success: {value}",
            onError: error => $"Error: {error.Message}"
        );

        string errorMessage = errorResult.Match(
            onOk: value => $"Success: {value}",
            onError: error => $"Error: {error.Message}"
        );

        Assert.That(okMessage, Is.EqualTo("Success: Data Loaded"));
        Assert.That(errorMessage, Is.EqualTo("Error: Load Failed"));
    }

    [Test]
    public void Test_Equals_Should_Compare_Correctly() {
        var result1 = Result.Ok("Test");
        var result2 = Result.Ok("Test");
        var result3 = Result.Error("Error");
        var result4 = Result.Ok("Error");
        var result5 = Result.Error("Error");

        Assert.That(result1 == result2);
        Assert.That(result1 != result3);
        Assert.That(result3 != result4);
        Assert.That(result3 == result5);
    }

    [Test]
    public void Test_Comparison_Should_Work_Correctly() {
        var result1 = Result.Ok(100);
        var result2 = Result.Ok(200);
        var result3 = Result.Error();

        Assert.That(result1 < result2);
        Assert.That(result2 > result1);
        Assert.That(result3 < result1);
        Assert.That(result1 > result3);
    }

    // Other
    [Test]
    public void Test_ToString_Should_Work_Correctly() {
        var result1 = Result.Ok(100);
        var result2 = Result.Error();
        var result3 = Result.Ok();

        Assert.That(result1.ToString(), Is.EqualTo("Ok = 100"));
        Assert.That(result2.ToString(), Is.EqualTo("Error = Result:: Something went wrong."));
        Assert.That(result3.ToString(), Is.EqualTo("Ok = null"));
    }
}
