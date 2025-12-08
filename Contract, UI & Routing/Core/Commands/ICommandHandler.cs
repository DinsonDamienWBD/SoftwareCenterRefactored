namespace SoftwareCenter.Core.Commands;

/// <summary>
/// Defines a handler for a specific command.
/// </summary>
/// <typeparam name="TCommand">The type of command to be handled.</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    Task<IResult> Handle(TCommand command, CancellationToken cancellationToken);
}