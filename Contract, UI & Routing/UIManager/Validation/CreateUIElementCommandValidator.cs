using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Errors;
using System;
using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Validation
{
    /// <summary>
    /// Validates the <see cref="CreateUIElementCommand"/> to ensure it has valid parameters.
    /// </summary>
    public class CreateUIElementCommandValidator : ICommandValidator<CreateUIElementCommand>
    {
        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command to validate.</param>
        /// <param name="traceContext">The trace context for the operation.</param>
        /// <exception cref="ValidationException">Thrown if the command is invalid.</exception>
        public Task Validate(CreateUIElementCommand command, ITraceContext traceContext)
        {
            if (string.IsNullOrWhiteSpace(command.ElementType))
            {
                throw new ValidationException($"{nameof(command.ElementType)} cannot be null or whitespace.");
            }

            if (!Enum.TryParse(command.ElementType, true, out SoftwareCenter.Core.UI.ElementType _))
            {
                throw new ValidationException($"Invalid ElementType '{command.ElementType}'.");
            }

            return Task.CompletedTask;
        }
    }
}
