using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipes;
using XamlMath.Atoms;
using XamlMath.Boxes;
using XamlMath.Exceptions;
using XamlMath.Parsers.Matrices;

namespace XamlMath.Parsers;

internal static class StandardCommands
{
    private class UnderlineCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;
            var afterFormula = TexFormulaParser.ReadElement(source, position);
            position = afterFormula.position;
            var underlineFormula = context.Parser.Parse(
                afterFormula.source,
                context.Formula.TextStyle,
                context.Environment);
            var start = context.CommandNameStartPosition;
            var atomSource = source.Segment(start, position - start);
            var atom = new UnderlinedAtom(atomSource, underlineFormula.RootAtom);
            return new CommandProcessingResult(atom, position);
        }
    }

    private sealed class OverArrowCommand : ICommandParser
    {
        public static OverArrowCommand Right { get; } = new(pointsRight: true);
        public static OverArrowCommand Left { get; } = new(pointsRight: false);

        private readonly bool _pointsRight;

        private OverArrowCommand(bool pointsRight)
        {
            _pointsRight = pointsRight;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;
            var afterFormula = TexFormulaParser.ReadElement(source, position);
            position = afterFormula.position;
            var baseFormula = context.Parser.Parse(
                afterFormula.source,
                context.Formula.TextStyle,
                context.Environment.CreateChildEnvironment());
            var start = context.CommandNameStartPosition;
            var atomSource = source.Segment(start, position - start);
            var atom = new OverArrowAtom(atomSource, baseFormula.RootAtom, _pointsRight);
            return new CommandProcessingResult(atom, position);
        }
    }

    // \vdots and \ddots take no argument; they just emit a fixed run of dots.
    private sealed class DotsCommand : ICommandParser
    {
        public static DotsCommand Vertical { get; } = new(DotsAtom.DotsShape.Vertical);
        public static DotsCommand Diagonal { get; } = new(DotsAtom.DotsShape.Diagonal);

        private readonly DotsAtom.DotsShape _shape;

        private DotsCommand(DotsAtom.DotsShape shape)
        {
            _shape = shape;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var start = context.CommandNameStartPosition;
            var position = context.ArgumentsStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);
            var atom = new DotsAtom(atomSource, _shape);
            return new CommandProcessingResult(atom, position);
        }
    }

    // \hspace{<length>} inserts horizontal space of an explicit length, e.g. \hspace{2em} or \hspace{-3pt}.
    private sealed class HspaceCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;

            // \hspace* behaves identically here (there is no line breaking to make the space removable).
            if (position < source.Length && source[position] == '*')
                position++;

            var afterArg = TexFormulaParser.ReadElement(source, position);
            position = afterArg.position;
            ParseLength(afterArg.source.ToString(), out var unit, out var value);

            var start = context.CommandNameStartPosition;
            var atomSource = source.Segment(start, position - start);
            var atom = new SpaceAtom(atomSource, unit, value, 0, 0);
            return new CommandProcessingResult(atom, position);
        }

        private static void ParseLength(string text, out TexUnit unit, out double value)
        {
            text = text.Trim();
            var splitIndex = text.Length;
            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    splitIndex = i;
                    break;
                }
            }

            var numberPart = text.Substring(0, splitIndex).Trim();
            var unitPart = text.Substring(splitIndex).Trim().ToLowerInvariant();
            if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                throw new TexParseException($"Invalid \\hspace length: \"{text}\".");

            // The engine natively supports em/ex/mu/pt/pc/px; absolute units are converted to points.
            switch (unitPart)
            {
                case "em": unit = TexUnit.Em; break;
                case "ex": unit = TexUnit.Ex; break;
                case "mu": unit = TexUnit.Mu; break;
                case "pt": unit = TexUnit.Point; break;
                case "pc": unit = TexUnit.Pica; break;
                case "px": unit = TexUnit.Pixel; break;
                case "bp": unit = TexUnit.Point; value *= 72.27 / 72.0; break;
                case "in": unit = TexUnit.Point; value *= 72.27; break;
                case "cm": unit = TexUnit.Point; value *= 72.27 / 2.54; break;
                case "mm": unit = TexUnit.Point; value *= 72.27 / 25.4; break;
                default: throw new TexParseException($"Unsupported \\hspace unit: \"{unitPart}\".");
            }
        }
    }

    /// <summary>Reads one <c>{…}</c> (or single-token) argument as a formula and advances <paramref name="position"/>.</summary>
    private static TexFormula ReadArgument(CommandContext context, ref int position)
    {
        var after = TexFormulaParser.ReadElement(context.CommandSource, position);
        position = after.position;
        return context.Parser.Parse(after.source, context.Formula.TextStyle, context.Environment.CreateChildEnvironment());
    }

    // \dfrac and \tfrac: \frac forced into display or text style respectively.
    private sealed class FracStyleCommand : ICommandParser
    {
        public static FracStyleCommand Dfrac { get; } = new(TexStyle.Display);
        public static FracStyleCommand Tfrac { get; } = new(TexStyle.Text);

        private readonly TexStyle _style;

        private FracStyleCommand(TexStyle style)
        {
            _style = style;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var position = context.ArgumentsStartPosition;
            var numerator = ReadArgument(context, ref position);
            var denominator = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);
            var atom = new FractionAtom(atomSource, numerator.RootAtom, denominator.RootAtom, true)
            {
                OverrideStyle = _style
            };
            return new CommandProcessingResult(atom, position);
        }
    }

    // \cfrac[l|c|r]{a}{b}: a continued-fraction fraction — display style throughout (nested \cfrac stays
    // full size) with an optional numerator alignment.
    private sealed class CfracCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;

            var numeratorAlignment = TexAlignment.Center;
            var optional = TexFormulaParser.ReadElementGroupOptional(source, ref position, '[', ']')?.ToString().Trim();
            if (optional == "l") numeratorAlignment = TexAlignment.Left;
            else if (optional == "r") numeratorAlignment = TexAlignment.Right;

            var numerator = ReadArgument(context, ref position);
            var denominator = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = source.Segment(start, position - start);
            var atom = new FractionAtom(
                atomSource, numerator.RootAtom, denominator.RootAtom, true, numeratorAlignment, TexAlignment.Center)
            {
                OverrideStyle = TexStyle.Display,
                KeepContentStyle = true
            };
            return new CommandProcessingResult(atom, position);
        }
    }

    // \nicefrac{a}{b} and \sfrac{a}{b}: an inline "slash" fraction (raised numerator / lowered denominator).
    private sealed class SlashFractionCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var position = context.ArgumentsStartPosition;
            var numerator = ReadArgument(context, ref position);
            var denominator = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);
            var atom = new SlashFractionAtom(atomSource, numerator.RootAtom, denominator.RootAtom);
            return new CommandProcessingResult(atom, position);
        }
    }

    // \pmod{n} -> "(mod n)" after a wide space; \pod{n} -> "(n)". Used as e.g. a \equiv b \pmod{n}.
    private sealed class ParenModCommand : ICommandParser
    {
        public static ParenModCommand Pmod { get; } = new(withMod: true);
        public static ParenModCommand Pod { get; } = new(withMod: false);

        private readonly bool _withMod;

        private ParenModCommand(bool withMod)
        {
            _withMod = withMod;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;
            var afterArg = TexFormulaParser.ReadElement(source, position);
            position = afterArg.position;
            var argument = afterArg.source.ToString();

            var body = _withMod
                ? $@"\quad(\mathrm{{mod}}\;{{{argument}}})"
                : $@"\quad({{{argument}}})";
            var bodySpan = new SourceSpan("pmod", body, 0, body.Length);
            var formula = context.Parser.Parse(
                bodySpan, context.Formula.TextStyle, context.Environment.CreateChildEnvironment());
            return new CommandProcessingResult(formula.RootAtom, position);
        }
    }

    private class BinomCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;
            var afterTop = TexFormulaParser.ReadElement(source, position);
            position = afterTop.position;
            var topFormula = context.Parser.Parse(
                        afterTop.source,
                        context.Formula.TextStyle,
                        context.Environment.CreateChildEnvironment());
            var afterBottom = TexFormulaParser.ReadElement(source, position);
            position = afterBottom.position;
            var bottomFormula = context.Parser.Parse(
                        afterBottom.source,
                        context.Formula.TextStyle,
                        context.Environment.CreateChildEnvironment());
            var start = context.CommandNameStartPosition;
            var atomSource = source.Segment(start, position - start);
            var topAtom = new List<Atom?> { topFormula.RootAtom };
            var bottomAtom = new List<Atom?> { bottomFormula.RootAtom };
            var atoms = new List<List<Atom?>> { topAtom, bottomAtom };
            var matrixAtom = new MatrixAtom(atomSource, atoms, MatrixCellAlignment.Center);
            var left = new SymbolAtom(atomSource, "(", TexAtomType.Opening, true);
            var right = new SymbolAtom(atomSource, ")", TexAtomType.Closing, true);
            var fencedAtom = new FencedAtom(atomSource, matrixAtom, left, right);
            return new CommandProcessingResult(fencedAtom, position);
        }
    }

    private sealed class CancelCommand : ICommandParser
    {
        public static CancelCommand BCancel { get; } = new(StrokeBoxMode.Back);
        public static CancelCommand Cancel { get; } = new(StrokeBoxMode.Normal);
        public static CancelCommand XCancel { get; } = new(StrokeBoxMode.Both);

        private CancelCommand(StrokeBoxMode strokeBoxMode)
        {
            _strokeBoxMode = strokeBoxMode;
        }

        private readonly StrokeBoxMode _strokeBoxMode;

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var position = context.ArgumentsStartPosition;
            var afterFormula = TexFormulaParser.ReadElement(source, position);
            position = afterFormula.position;
            var contentFormula = context.Parser.Parse(afterFormula.source,
                                                      context.Formula.TextStyle,
                                                      context.Environment.CreateChildEnvironment());

            var start = context.CommandNameStartPosition;
            var atomSource = source.Segment(start, position - start);
            var cancelAtom = new CancelAtom(atomSource, contentFormula.RootAtom, _strokeBoxMode);

            return new CommandProcessingResult(cancelAtom, position);
        }
    }

    /// <summary>
    /// This command will parse the remaining part of an input string, and add it onto a new line of a formula. The
    /// new line is created as a <see cref="MatrixAtom"/>; the command will try to reuse existing atoms if possible.
    /// </summary>
    private class NewLineCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var prevFormulaAtom = context.Formula.RootAtom;

            var nextLineAtom = context.Parser.Parse(
                source.Segment(context.ArgumentsStartPosition),
                context.Formula.TextStyle,
                context.Environment).RootAtom;

            // An optimization: if the new content itself is a matrix with suitable parameters, then we won't
            // wrap it into another formula, but will combine it with the content on top.
            var newMatrix = nextLineAtom is MatrixAtom m
                && m.MatrixCellAlignment == MatrixCellAlignment.Left
                && m.HorizontalPadding == MatrixAtom.DefaultPadding
                && m.VerticalPadding == MatrixAtom.DefaultPadding
                ? m
                : null;

            var topRow = new[] {prevFormulaAtom};
            var rows = new List<IEnumerable<Atom?>> {topRow};
            if (newMatrix != null)
            {
                rows.AddRange(newMatrix.MatrixCells);
            }
            else
            {
                var bottomRow = new[] {nextLineAtom};
                rows.Add(bottomRow);
            }

            // We'll always use source = null for the resulting matrix, because it's a structural element and not a
            // useful atom generated from any particular sources.
            var atom = new MatrixAtom(null, rows, MatrixCellAlignment.Left);
            var position = source.Length; // we always parse the provided source until the end
            return new CommandProcessingResult(atom, position, AtomAppendMode.Replace);
        }
    }

    internal static readonly IReadOnlyDictionary<string, ICommandParser> Dictionary =
        new Dictionary<string, ICommandParser>
        {
            [@"\"] = new NewLineCommand(),
            ["binom"] = new BinomCommand(),
            ["cancel"] = CancelCommand.Cancel,
            ["bcancel"] = CancelCommand.BCancel,
            ["xcancel"] = CancelCommand.XCancel,
            ["cases"] = MatrixCommandParser.Cases,
            ["matrix"] = MatrixCommandParser.Matrix,
            ["pmatrix"] = MatrixCommandParser.PMatrix,
            ["underline"] = new UnderlineCommand(),
            ["overrightarrow"] = OverArrowCommand.Right,
            ["overleftarrow"] = OverArrowCommand.Left,
            ["vdots"] = DotsCommand.Vertical,
            ["ddots"] = DotsCommand.Diagonal,
            ["hspace"] = new HspaceCommand(),
            ["dfrac"] = FracStyleCommand.Dfrac,
            ["tfrac"] = FracStyleCommand.Tfrac,
            ["cfrac"] = new CfracCommand(),
            ["nicefrac"] = new SlashFractionCommand(),
            ["sfrac"] = new SlashFractionCommand(),
            ["pmod"] = ParenModCommand.Pmod,
            ["pod"] = ParenModCommand.Pod,
            ["begin"] = new ProcessEnvironmentCommand()
        };

    internal static readonly IReadOnlyDictionary<string, IEnvironmentParser> Environments =
        new Dictionary<string, IEnvironmentParser>
        {
            ["align"] = MatrixCommandParser.Align,
            ["pmatrix"] = MatrixCommandParser.PMatrix
        };
}
