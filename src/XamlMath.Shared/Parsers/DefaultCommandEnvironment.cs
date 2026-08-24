using System.Collections.Generic;

namespace XamlMath.Parsers;

internal sealed class DefaultCommandEnvironment : ICommandEnvironment
{
    public static readonly ICommandEnvironment Instance = new DefaultCommandEnvironment();

    /// <summary>Nothing is recorded: without a recovering parse, a fault ends the parse.</summary>
    public ICollection<TexParseDiagnostic>? Diagnostics => null;

    public IReadOnlyDictionary<string, ICommandParser> AvailableCommands { get; } =
        new Dictionary<string, ICommandParser>();

    public ICommandEnvironment CreateChildEnvironment() => Instance;

    public bool ProcessUnknownCharacter(TexFormula formula, char character) => false;
}
