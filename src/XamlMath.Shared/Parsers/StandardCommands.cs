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

    // \displaystyle, \textstyle, \scriptstyle and \scriptscriptstyle are switches, not one-argument commands:
    // they apply from where they appear to the end of the enclosing group. Reading only the next element would
    // leave the scripts of e.g. "\displaystyle\sum_{i=1}^{n}" outside the switch, which is where the style
    // actually matters (display style is what moves the limits above and below the operator).
    private sealed class StyleCommand : ICommandParser
    {
        public static StyleCommand Display { get; } = new(TexStyle.Display);
        public static StyleCommand Text { get; } = new(TexStyle.Text);
        public static StyleCommand Script { get; } = new(TexStyle.Script);
        public static StyleCommand ScriptScript { get; } = new(TexStyle.ScriptScript);

        private readonly TexStyle _style;

        private StyleCommand(TexStyle style)
        {
            _style = style;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var source = context.CommandSource;
            var start = context.CommandNameStartPosition;

            // The rest of the group is the argument. It keeps the current environment rather than a child one, so
            // that a switch inside a matrix cell doesn't swallow the row and cell separators.
            var rest = source.Segment(context.ArgumentsStartPosition);
            var formula = context.Parser.Parse(rest, context.Formula.TextStyle, context.Environment);

            var atomSource = source.Segment(start, source.Length - start);
            var atom = new StyleAtom(atomSource, formula.RootAtom, _style);
            return new CommandProcessingResult(atom, source.Length);
        }
    }

    // \overset{ann}{base}, \underset{ann}{base} and \stackrel{ann}{rel}: the annotation is set in script size
    // above or below the base. \stackrel differs from \overset only in the spacing it gets: its result is a
    // relation (it exists to stack something over an arrow), so it is typed as one.
    private sealed class StackedAnnotationCommand : ICommandParser
    {
        public static StackedAnnotationCommand Overset { get; } = new(over: true, asRelation: false);
        public static StackedAnnotationCommand Underset { get; } = new(over: false, asRelation: false);
        public static StackedAnnotationCommand Stackrel { get; } = new(over: true, asRelation: true);

        private const double AnnotationSpace = 2.5; // mu, the same order as the \overbrace-style annotations

        private readonly bool _over;
        private readonly bool _asRelation;

        private StackedAnnotationCommand(bool over, bool asRelation)
        {
            _over = over;
            _asRelation = asRelation;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var position = context.ArgumentsStartPosition;
            var annotation = ReadArgument(context, ref position);
            var baseFormula = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);

            Atom atom = new UnderOverAtom(
                atomSource,
                baseFormula.RootAtom,
                annotation.RootAtom,
                TexUnit.Mu,
                AnnotationSpace,
                true,
                _over);

            if (_asRelation)
                atom = new TypedAtom(atomSource, atom, TexAtomType.Relation, TexAtomType.Relation);

            return new CommandProcessingResult(atom, position);
        }
    }

    // \phantom{x} and its one-dimensional variants: the content is measured and then not drawn, so it reserves
    // space without printing anything.
    private sealed class PhantomCommand : ICommandParser
    {
        public static PhantomCommand Both { get; } = new(useWidth: true, useHeight: true);
        public static PhantomCommand Horizontal { get; } = new(useWidth: true, useHeight: false);
        public static PhantomCommand Vertical { get; } = new(useWidth: false, useHeight: true);

        private readonly bool _useWidth;
        private readonly bool _useHeight;

        private PhantomCommand(bool useWidth, bool useHeight)
        {
            _useWidth = useWidth;
            _useHeight = useHeight;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var position = context.ArgumentsStartPosition;
            var content = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);
            var atom = new PhantomAtom(atomSource, content.RootAtom, _useWidth, _useHeight, _useHeight);
            return new CommandProcessingResult(atom, position);
        }
    }

    // \smash{x} draws the content and reports no height, \math?lap{x} draws it and reports no width. Both are the
    // inverse of \phantom: ink without extent rather than extent without ink.
    private sealed class SmashCommand : ICommandParser
    {
        public static SmashCommand Smash { get; } = new(null);
        public static SmashCommand Llap { get; } = new(TexAlignment.Left);
        public static SmashCommand Rlap { get; } = new(TexAlignment.Right);
        public static SmashCommand Clap { get; } = new(TexAlignment.Center);

        private readonly TexAlignment? _lapAlignment;

        private SmashCommand(TexAlignment? lapAlignment)
        {
            _lapAlignment = lapAlignment;
        }

        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var position = context.ArgumentsStartPosition;
            var content = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);
            var atom = _lapAlignment is { } alignment
                ? (Atom)new LapAtom(atomSource, content.RootAtom, alignment)
                : new SmashAtom(atomSource, content.RootAtom);
            return new CommandProcessingResult(atom, position);
        }
    }

    // \boxed{x} and \fbox{x}: the content inside a rectangular frame.
    private sealed class BoxedCommand : ICommandParser
    {
        public CommandProcessingResult ProcessCommand(CommandContext context)
        {
            var position = context.ArgumentsStartPosition;
            var content = ReadArgument(context, ref position);
            var start = context.CommandNameStartPosition;
            var atomSource = context.CommandSource.Segment(start, position - start);
            var atom = new BoxedAtom(atomSource, content.RootAtom);
            return new CommandProcessingResult(atom, position);
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
            ["bmatrix"] = MatrixCommandParser.BMatrix,
            ["Bmatrix"] = MatrixCommandParser.BbMatrix,
            ["vmatrix"] = MatrixCommandParser.VMatrix,
            ["Vmatrix"] = MatrixCommandParser.VvMatrix,
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
            ["boxed"] = new BoxedCommand(),
            ["fbox"] = new BoxedCommand(),
            ["phantom"] = PhantomCommand.Both,
            ["hphantom"] = PhantomCommand.Horizontal,
            ["vphantom"] = PhantomCommand.Vertical,
            ["smash"] = SmashCommand.Smash,
            ["mathllap"] = SmashCommand.Llap,
            ["mathrlap"] = SmashCommand.Rlap,
            ["mathclap"] = SmashCommand.Clap,
            ["llap"] = SmashCommand.Llap,
            ["rlap"] = SmashCommand.Rlap,
            ["clap"] = SmashCommand.Clap,
            ["overset"] = StackedAnnotationCommand.Overset,
            ["underset"] = StackedAnnotationCommand.Underset,
            ["stackrel"] = StackedAnnotationCommand.Stackrel,
            ["displaystyle"] = StyleCommand.Display,
            ["textstyle"] = StyleCommand.Text,
            ["scriptstyle"] = StyleCommand.Script,
            ["scriptscriptstyle"] = StyleCommand.ScriptScript,
            ["pmod"] = ParenModCommand.Pmod,
            ["pod"] = ParenModCommand.Pod,
            ["begin"] = new ProcessEnvironmentCommand()
        };

    internal static readonly IReadOnlyDictionary<string, IEnvironmentParser> Environments =
        new Dictionary<string, IEnvironmentParser>
        {
            ["align"] = MatrixCommandParser.Align,
            ["align*"] = MatrixCommandParser.Align,
            ["aligned"] = MatrixCommandParser.Align,
            ["split"] = MatrixCommandParser.Align,
            ["gather"] = MatrixCommandParser.Gathered,
            ["gather*"] = MatrixCommandParser.Gathered,
            ["gathered"] = MatrixCommandParser.Gathered,
            ["cases"] = MatrixCommandParser.Cases,
            ["matrix"] = MatrixCommandParser.Matrix,
            ["smallmatrix"] = MatrixCommandParser.SmallMatrix,
            ["pmatrix"] = MatrixCommandParser.PMatrix,
            ["bmatrix"] = MatrixCommandParser.BMatrix,
            ["Bmatrix"] = MatrixCommandParser.BbMatrix,
            ["vmatrix"] = MatrixCommandParser.VMatrix,
            ["Vmatrix"] = MatrixCommandParser.VvMatrix
        };
}
