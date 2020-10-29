using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Clud.Cli.Helpers
{
    public class ParseResult<T>
    {
        public T Result { get; }

        private readonly List<string> warningsList;
        private readonly List<string> errorsList;

        public IReadOnlyCollection<string> Warnings => warningsList.ToList();
        public IReadOnlyCollection<string> Errors => errorsList.ToList();

        public ParseResult(T result, IReadOnlyCollection<string> warnings, IReadOnlyCollection<string> errors)
        {
            Result = result;
            warningsList = warnings.ToList();
            errorsList = errors.ToList();
        }

        public ParseResult(T result, ValidationResult validationResult)
        {
            Result = result;
            warningsList = validationResult.Errors.Where(r => r.Severity == Severity.Warning).Select(r => r.ErrorMessage).ToList();
            errorsList = validationResult.Errors.Where(r => r.Severity == Severity.Error).Select(r => r.ErrorMessage).ToList();
        }

        public static ParseResult<T> Success(T result) => new ParseResult<T>(result, new List<string>(), new List<string>());

        public static ParseResult<T> Failed(IEnumerable<string> errors) => new ParseResult<T>(default, new List<string>(), errors.ToList());
        public static ParseResult<T> Failed(string error) => new ParseResult<T>(default, new List<string>(), new List<string> { error });

        public void AddWarningsAndErrors<S>(ParseResult<S> otherResult)
        {
            warningsList.AddRange(otherResult.Warnings);
            errorsList.AddRange(otherResult.Errors);
        }
    }
}
