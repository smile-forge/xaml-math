using XamlMath.Boxes;

namespace XamlMath.Atoms;

// Atom representing a base atom with a stretchy horizontal arrow drawn above it
// (\overrightarrow when pointing right, \overleftarrow when pointing left).
internal sealed record OverArrowAtom : Atom
{
    private readonly bool _pointsRight;

    public OverArrowAtom(SourceSpan? source, Atom? baseAtom, bool pointsRight)
        : base(source)
    {
        this.BaseAtom = baseAtom;
        _pointsRight = pointsRight;
    }

    public Atom? BaseAtom { get; }

    protected override Box CreateBoxCore(TexEnvironment environment)
    {
        // Create box for base atom, in cramped style (like \overline).
        var baseBox = this.BaseAtom == null ? StrutBox.Empty : this.BaseAtom.CreateBox(environment.GetCrampedStyle());

        var thickness = environment.MathFont.GetDefaultLineThickness(environment.Style);
        var arrowBox = new ArrowBox(
            environment,
            baseBox.Width,
            thickness,
            _pointsRight ? ArrowDecoration.HeadRight : ArrowDecoration.HeadLeft);

        var resultBox = new VerticalBox();
        resultBox.Add(new StrutBox(0, thickness, 0, 0));
        resultBox.Add(arrowBox);
        resultBox.Add(new StrutBox(0, 3 * thickness, 0, 0));
        resultBox.Add(baseBox);

        // Adjust height and depth of result box so the base keeps its normal baseline.
        resultBox.Height = baseBox.Height + arrowBox.Height + 4 * thickness;
        resultBox.Depth = baseBox.Depth;

        return resultBox;
    }
}
