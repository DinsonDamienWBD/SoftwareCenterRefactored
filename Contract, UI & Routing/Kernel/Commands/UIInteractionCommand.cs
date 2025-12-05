using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Diagnostics;
using System.Collections.Generic;
using System;

namespace SoftwareCenter.Kernel.Commands;

public record UIInteractionCommand(string OwnerId, string Action, object AdditionalData) : ICommand
{
    public string Name => "UI.Interact";
    public Dictionary<string, object> Parameters { get; } = new();
    public Guid TraceId { get; } = Guid.NewGuid();
    public List<TraceHop> History { get; } = new();
}
