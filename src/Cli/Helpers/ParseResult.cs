using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;

namespace Clud.Cli.Helpers
{
    public class ParseResult<T>
    {
        public T Result { get; }
        public IReadOnlyCollection<string> Warnings { get; }
        public IReadOnlyCollection<string> Errors { get; }

        public ParseResult(T result, IReadOnlyCollection<string> warnings, IReadOnlyCollection<string> errors)
        {
            Result = result;
            Warnings = warnings;
            Errors = errors;
        }

        public ParseResult(T result, ValidationResult validationResult)
        {
            Result = result;
            Warnings = validationResult.Errors.Where(r => r.Severity == Severity.Warning).Select(r => r.ErrorMessage).ToList();
            Errors = validationResult.Errors.Where(r => r.Severity == Severity.Error).Select(r => r.ErrorMessage).ToList();
        }
    }

    // public class SuccessResult<T> : ParseResult
    // {
    //     public T Result { get; }
    //     public IReadOnlyCollection<string> Warnings { get; }
    //
    //     public SuccessResult(T result, IReadOnlyCollection<string> warnings)
    //     {
    //         Result = result;
    //         Warnings = warnings;
    //     }
    // }
    //
    // public class ErrorResult : ParseResult
    // {
    //     public IReadOnlyCollection<string> Errors { get; }
    //
    //     public ErrorResult(IReadOnlyCollection<string> errors)
    //     {
    //         Errors = errors;
    //     }
    // }
}
