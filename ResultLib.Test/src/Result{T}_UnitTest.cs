using ResultLib.Core;

namespace ResultLib.Test;

public class Result_T_Tests {
    [SetUp]
    public void Setup()
    {
    }

    // BEGIN Default public constructor behavior

    [Test]
    public void Test_DefaultConstructorShouldBeInStateError() {
        var result = new Result<string>();
        Assert.Multiple(
            () => {
                Assert.That(result.IsError());
                Assert.That(!result.IsOk());
            }
        );
    }

    [Test]
    public void Test_DefaultConstructorShouldReturnEmptyConstructorError() {
        var result = new Result<string>();
        Assert.That(result.IsError(out string errorMessage) && errorMessage == "Result:: Must be instantiated with Static Methods or Factory.");
    }

    [Test]
    public void Test_DefaultConstructorThrowsDefaultConstructorExceptionIfUnwrapErrIsCalled() {
        var result = new Result<string>();
        Assert.Throws<ResultDefaultConstructorException>(() => result.UnwrapErr());
    }

    [Test]
    public void Test_DefaultConstructorThrowsDefaultConstructorExceptionIfThrowIfErrorIsCalled() {
        var result = new Result<string>();
        Assert.Throws<ResultDefaultConstructorException>(() => result.ThrowIfError());
    }

    // END Default public constructor behavior


    // BEGIN value and error state
    [Test]
    public void Test_Ok_With_Value_Should_Be_Ok() {
        var result = Result<string>.Ok("Success");
        Assert.That(result.IsOk());
        Assert.That(result.Unwrap(), Is.EqualTo("Success"));
        Assert.Throws<ResultUnwrapErrorException>(() => result.UnwrapErr());
        Assert.DoesNotThrow(() => result.ThrowIfError());
    }

    [Test]
    public void Test_Error_Without_Message_Should_Be_Error() {
        var result = Result<string>.Error();
        Assert.That(result.IsError(out string message));
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));
        Assert.That(result.UnwrapErr().GetType(), Is.EqualTo(typeof(ResultException)));
        Assert.DoesNotThrow(() => result.UnwrapErr());
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_Error_With_Message_Should_Be_Error() {
        var result = Result<string>.Error("Something went wrong");
        Assert.That(result.IsError(out string message));
        Assert.That(message, Is.EqualTo("Something went wrong"));
        Assert.That(result.UnwrapErr().GetType(), Is.EqualTo(typeof(ResultException)));
        Assert.DoesNotThrow(() => result.UnwrapErr());
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_IsOk_Should_Return_Correct_State() {
        var success = Result<string>.Ok();
        var failure = Result<string>.Error();

        Assert.That(success.IsOk(), Is.True);
        Assert.That(success.IsError(), Is.False);
        Assert.That(failure.IsOk(), Is.False);
        Assert.That(failure.IsError(), Is.True);
    }

    [Test]
    public void Test_Unwrap_Should_Return_Value_When_Ok() {
        var result = Result<string>.Ok("ExpectedValue");
        Assert.That(result.Unwrap(), Is.EqualTo("ExpectedValue"));
    }

    [Test]
    public void Test_Unwrap_Should_Throw_Exception_When_Error() {
        var result = Result<string>.Error("Some error");
        Assert.Throws<ResultUnwrapException>(() => result.Unwrap());
    }

    [Test]
    public void Test_Unwrap_With_Default_Should_Return_Default_When_Error() {
        var result = Result<string>.Error();
        Assert.That(result.Unwrap("DefaultValue"), Is.EqualTo("DefaultValue"));
    }

    [Test]
    public void Test_Unwrap_With_Func_Should_Return_Provided_Value_When_Error() {
        var result = Result<string>.Error();
        Assert.That(result.Unwrap(() => "FallbackValue"), Is.EqualTo("FallbackValue"));
    }

    [Test]
    public void Test_Unwrap_With_Func_Should_Throw_Exception_If_Func_Is_Null_And_Error() {
        var result = Result<string>.Error();
        Assert.Throws<ArgumentNullException>(() => result.Unwrap(func: null));
    }

    [Test]
    public void Test_Unwrap_With_Null_Func_Should_Not_Throw_Exception_If_Is_Ok() {
        var result = Result<string>.Ok();
        Assert.DoesNotThrow(() => result.Unwrap(func: null));
        Assert.That(result.Unwrap(func: null), Is.EqualTo(null));
    }

    [Test]
    public void Test_Unwrap_With_Null_Func_Should_Throws_Exception_When_Error() {
        var result = Result<string>.Error();
        Assert.Throws<ArgumentNullException>(() => result.Unwrap(func: null));
    }

    [Test]
    public void Test_UnwrapErr_Should_Return_Exception_When_Error() {
        var result = Result<string>.Error("Critical failure");
        var exception = result.UnwrapErr();
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Critical failure"));
    }

    [Test]
    public void Test_UnwrapErr_Should_Return_InnerException_When_Error_When_Exception_Is_Passed() {
        var result = Result<string>.Error(new InvalidOperationException("Critical failure"));
        var exception = result.UnwrapErr();
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Result:: Something went wrong."));
        Assert.That(exception.InnerException, Is.Not.Null);
        Assert.That(exception.InnerException.Message, Is.EqualTo("Critical failure"));
    }

    [Test]
    public void Test_ThrowIf_Should_Throw_Exception_When_Error() {
        var result = Result<string>.Error(new InvalidOperationException("Critical failure"));
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_ThrowIf_Should_Not_Throw_Exception_When_Ok() {
        var result = Result<string>.Ok();
        Assert.DoesNotThrow(() => result.ThrowIfError());
    }

    [Test]
    public void Test_UnwrapErr_Should_Throw_Exception_When_Ok() {
        var result = Result<string>.Ok();
        Assert.Throws<ResultUnwrapErrorException>(() => result.UnwrapErr());
    }

    [Test]
    public void Test_Some_Should_Return_True_When_Value_Is_Not_Null() {
        var result = Result<string>.Ok("ValidValue");
        Assert.That(result.Some(out string value));
        Assert.That(value, Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Some_Should_Return_False_When_Value_Is_Null() {
        var result = Result<string>.Ok(null);
        Assert.That(result.Some(out string value), Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void Test_Some_Should_Not_Return_Default_Value_When_Value_Is_Not_Null_And_Ok() {
        var result = Result<string>.Ok("Default Value");
        Assert.That(result.Some("Object"), Is.EqualTo("Default Value"));
        Assert.That(result.Some(() => "Func Object"), Is.EqualTo("Default Value"));
    }

    [Test]
    public void Test_Some_Should_Return_Default_Value_When_Value_Is_Null_And_Ok() {
        var result = Result<string>.Ok(null);
        Assert.That(result.Some("Object"), Is.EqualTo("Object"));
        Assert.That(result.Some(() => "Func Object"), Is.EqualTo("Func Object"));
    }

    [Test]
    public void Test_Some_Should_Return_Default_Value_When_Not_OK() {
        var result = Result<string>.Error();
        Assert.That(result.Some("ValidValue"), Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Some_Should_Return_Default_Value_From_func_When_Not_OK() {
        var result = Result<string>.Error();
        Assert.That(result.Some(() => "ValidValue"), Is.EqualTo("ValidValue"));
    }

    [Test]
    public void Test_Some_Should_Not_Throw_Exception_When_Default_Value_Is_Called_With_Null_And_Ok() {
        var result = Result<string>.Ok("ValidValue");
        Assert.DoesNotThrow(() => result.Some(defaultValue: null));
        Assert.DoesNotThrow(() => result.Some(func: null));
        Assert.DoesNotThrow(() => result.Some(() => null));
    }

    [Test]
    public void Test_Some_Should_Throw_Exception_When_Default_Value_Is_Called_With_Null_And_Error() {
        var result = Result<string>.Error("ErrorMessage");
        Assert.Throws<ArgumentNullException>(() => result.Some(defaultValue: null));
        Assert.Throws<ArgumentNullException>(() => result.Some(func: null));
        Assert.Throws<ResultInvalidSomeOperationException>(() => result.Some(() => null));
    }

    [Test]
    public void Test_FromRequired_Should_Avoid_Result_Ok_When_Value_Is_NUll() {
        var result = Result<string>.FromRequired(null);
        Assert.That(result.IsError(), Is.True);
        Assert.That(result.IsOk(), Is.False);
    }

    [Test]
    public void Test_FromRequired_Should_return_Result_Ok_When_Value_Is_Not_NUll() {
        var result = Result<string>.FromRequired("something");
        Assert.That(result.IsOk(), Is.True);
        Assert.That(result.IsError(), Is.False);
    }

    [Test]
    public void Test_ThrowIfError_Should_Throw_If_Result_Is_Error() {
        var result = Result<string>.Error("Failure occurred");
        Assert.Throws<ResultException>(() => result.ThrowIfError());
    }

    [Test]
    public void Test_Match_Should_Invoke_Correct_Function() {
        var okResult = Result<string>.Ok("Data Loaded");
        var errorResult = Result<string>.Error("Load Failed");

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
        var result1 = Result<string>.Ok("Test");
        var result2 = Result<string>.Ok("Test");
        var result3 = Result<string>.Error("Error");
        var result4 = Result<string>.Ok("Error");
        var result5 = Result<string>.Error("Error");

        Assert.That(result1 == result2);
        Assert.That(result1 != result3);
        Assert.That(result3 != result4);
        Assert.That(result3 == result5);
    }

    [Test]
    public void Test_Comparison_Should_Work_Correctly() {
        var result1 = Result<int>.Ok(100);
        var result2 = Result<int>.Ok(200);
        var result3 = Result<int>.Error();

        Assert.That(result1 < result2);
        Assert.That(result2 > result1);
        Assert.That(result3 < result1);
        Assert.That(result1 > result3);
    }

    // casting
    [Test]
    public void Test_Implicit_Casting_Should_Work_Correctly_If_Ok_And_Correctly_Typed() {
        Result r = Result.Ok(100);
        Result<int> r2 = r;
        Assert.That(r == r2);
        Assert.That((int)r.Unwrap() == r2.Unwrap());
    }

    [Test]
    public void Test_Implicit_Casting_Should_Work_Correctly_If_Ok_Even_With_Null() {
        Result r = Result.Ok();
        Result<int> r2 = r;
        Result<string> r3 = r;
        Assert.That(r == r2);
        Assert.That(r == r3);
        Assert.That(r.Unwrap(), Is.Null);
        Assert.That(r2.Unwrap(), Is.EqualTo(default(int)));
        Assert.That(r3.Unwrap(), Is.EqualTo(default(string)));
    }

    [Test]
    public void Test_Implicit_Casting_Should_Not_Work_If_Ok_But_Wrongly_Typed() {
        Assert.Throws<ResultInvalidImplicitCastException>(
            () => {
                Result r = Result.Ok(100);
                Result<string> r2 = r;
            }
        );
    }

    [Test]
    public void Test_Implicit_Casting_Should_Work_Correctly_If_Error_And_Correctly_Typed() {
        Result r = Result.Error("error");
        Result<int> r2 = r;
        Assert.That(r == r2);
        Assert.That(r.IsError(), Is.True);
        Assert.That(r2.IsError(), Is.True);
    }

    [Test]
    public void Test_Implicit_Casting_Should_Work_Correctly_If_Error_And_Wrongly_Typed() {
        Result r = Result.Error("error");
        Result<string> r2 = r;
        Result<int> r3 = r;
        Assert.That(r == r2);
        Assert.That(r == r3);
        Assert.That(r.IsError(), Is.True);
        Assert.That(r2.IsError(), Is.True);
        Assert.That(r3.IsError(), Is.True);
    }

    [Test]
    public void Test_Explicit_Casting_Should_Work_Correctly_If_Ok_And_Correctly_Typed() {
        Result r = Result.Ok(100);
        Result<int> r2 = r.ToResult<int>();
        Assert.That(r == r2);
        Assert.That((int)r.Unwrap() == r2.Unwrap());
    }

    [Test]
    public void Test_Explicit_Casting_Should_Work_Correctly_If_Ok_Even_With_Null() {
        Result r = Result.Ok();
        Result<int> r2 = r.ToResult<int>();
        Result<string> r3 = r.ToResult<string>();
        Assert.That(r == r2);
        Assert.That(r == r3);
        Assert.That(r.Unwrap(), Is.Null);
        Assert.That(r2.Unwrap(), Is.EqualTo(default(int)));
        Assert.That(r3.Unwrap(), Is.EqualTo(default(string)));
    }

    [Test]
    public void Test_Explicit_Casting_Should_Not_Work_If_Ok_But_Wrongly_Typed() {
        Assert.Throws<ResultInvalidExplicitCastException>(
            () => {
                Result r = Result.Ok(100);
                Result<string> r2 = r.ToResult<string>();
            }
        );
    }

    [Test]
    public void Test_Explicit_Casting_Should_Work_Correctly_If_Error_And_Correctly_Typed() {
        Result r = Result.Error("error");
        Result<int> r2 = r.ToResult<int>();
        Assert.That(r == r2);
        Assert.That(r.IsError(), Is.True);
        Assert.That(r2.IsError(), Is.True);
    }

    [Test]
    public void Test_Explicit_Casting_Should_Work_Correctly_If_Error_And_Wrongly_Typed() {
        Result r = Result.Error("error");
        Result<string> r2 = r.ToResult<string>();
        Result<int> r3 = r.ToResult<int>();
        Assert.That(r == r2);
        Assert.That(r == r3);
        Assert.That(r.IsError(), Is.True);
        Assert.That(r2.IsError(), Is.True);
        Assert.That(r3.IsError(), Is.True);
    }

    // error forwarding
    [Test]
    public void Test_ForwardError_Throws_Exception_If_Ok() {
        Result r = Result.Ok();
        Assert.Throws<ResultInvalidForwardException>(() => r.ForwardError());
    }

    [Test]
    public void Test_ForwardError_Should_Work_Correctly_If_Error() {
        Result r = Result.Error("error happened!");
        Result newResult = r.ForwardError();
        Assert.That(newResult.IsError(out string message), Is.True);
        Assert.That(message, Is.EqualTo("error happened!"));
    }

    [Test]
    public void Test_ForwardError_Should_Propagate_InnerException_Correctly_If_Error() {
        Result r = Result.Error(new InvalidCastException("error happened!"));
        Result newResult = r.ForwardError(); // to Result
        Assert.That(newResult.IsError(out string message), Is.True);
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));

        Assert.That(newResult.IsError(out ResultException exception), Is.True);
        Assert.That(exception!.InnerException!.GetType(), Is.EqualTo(typeof(InvalidCastException)));
        Assert.That(exception!.InnerException!.Message, Is.EqualTo("error happened!"));
    }

    [Test]
    public void Test_ForwardError_Should_Implicitly_Cast_And_Propagate_InnerException_Correctly_If_Error() {
        Result r = Result.Error(new InvalidCastException("error happened!"));
        Result<int> newResult = r.ForwardError(); // to Result then Result<int>
        Assert.That(newResult.IsError(out string message), Is.True);
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));

        Assert.That(newResult.IsError(out ResultException exception), Is.True);
        Assert.That(exception!.InnerException!.GetType(), Is.EqualTo(typeof(InvalidCastException)));
        Assert.That(exception!.InnerException!.Message, Is.EqualTo("error happened!"));
    }

    [Test]
    public void Test_ForwardError_Should_Explicitly_Cast_And_Propagate_InnerException_Correctly_If_Error() {
        Result r = Result.Error(new InvalidCastException("error happened!"));
        Result<int> newResult = r.ForwardError<int>(); // to Result<int>
        Assert.That(newResult.IsError(out string message), Is.True);
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));

        Assert.That(newResult.IsError(out ResultException exception), Is.True);
        Assert.That(exception!.InnerException!.GetType(), Is.EqualTo(typeof(InvalidCastException)));
        Assert.That(exception!.InnerException!.Message, Is.EqualTo("error happened!"));
    }

    [Test]
    public void Test_ForwardError_Should_Propagate_InnerException_Correctly_If_Error_And_Cast_Implicitly() {
        Result<int> r = Result<int>.Error(new InvalidCastException("error happened!"));
        Result newResult = r.ForwardError(); // to Result
        Assert.That(newResult.IsError(out string message), Is.True);
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));

        Assert.That(newResult.IsError(out ResultException exception), Is.True);
        Assert.That(exception!.InnerException!.GetType(), Is.EqualTo(typeof(InvalidCastException)));
        Assert.That(exception!.InnerException!.Message, Is.EqualTo("error happened!"));
    }

    [Test]
    public void Test_ForwardError_Should_Propagate_InnerException_Correctly_If_Error_And_Cast_Implicitly_To_Different_Type() {
        Result<int> r = Result<int>.Error(new InvalidCastException("error happened!"));
        Result<string> newResult = r.ForwardError<int, string>(); // to Result<string>
        Assert.That(newResult.IsError(out string message), Is.True);
        Assert.That(message, Is.EqualTo("Result:: Something went wrong."));

        Assert.That(newResult.IsError(out ResultException exception), Is.True);
        Assert.That(exception!.InnerException!.GetType(), Is.EqualTo(typeof(InvalidCastException)));
        Assert.That(exception!.InnerException!.Message, Is.EqualTo("error happened!"));
    }
}
