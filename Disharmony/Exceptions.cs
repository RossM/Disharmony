namespace Disharmony;

/// <summary>
///     Represents an error encountered while defining or applying a Disharmony patch.
/// </summary>
public class PatchException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PatchException" /> class with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PatchException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PatchException" /> class with the specified error message and
    ///     inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public PatchException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
///     Represents an error binding a patch-method parameter to a value supplied by the target operation.
/// </summary>
public class ParameterBindingException : PatchException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ParameterBindingException" /> class.
    /// </summary>
    /// <param name="argumentName">The name of the patch-method parameter that could not be bound.</param>
    /// <param name="message">The message that describes the binding error.</param>
    public ParameterBindingException(string? argumentName, string message) : base($"{argumentName ?? "<unknown>"}: {message}") { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ParameterBindingException" /> class with an inner exception.
    /// </summary>
    /// <param name="argumentName">The name of the patch-method parameter that could not be bound.</param>
    /// <param name="message">The message that describes the binding error.</param>
    /// <param name="innerException">The exception that caused the binding error.</param>
    public ParameterBindingException(string? argumentName, string message, Exception innerException) : base($"{argumentName ?? "<unknown>"}: {message}", innerException) { }
}

/// <summary>
///     Represents an invalid patch definition discovered before the patch is applied.
/// </summary>
/// <param name="method">The patch method whose definition is invalid.</param>
/// <param name="message">The message that describes the definition error.</param>
public class PatchDefinitionException(MethodInfo method, string message) : PatchException($"{method.FullName}: {message}");

/// <summary>
///     Represents an error that occurs while applying a patch at run time.
/// </summary>
/// <param name="message">The message that describes the run-time error.</param>
/// <param name="innerException">The exception that caused the run-time error.</param>
public class RuntimePatchException(string message, Exception innerException) : PatchException(message, innerException);

/// <summary>
///     Represents an error resolving a patch target or another member through reflection.
/// </summary>
/// <param name="message">The message that describes the reflection error.</param>
public class ReflectionException(string message) : PatchException(message);
