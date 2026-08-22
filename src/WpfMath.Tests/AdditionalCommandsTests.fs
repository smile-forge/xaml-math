namespace WpfMath.Tests

open Xunit

open WpfMath.Parsers
open WpfMath.Rendering
open WpfMath.Tests.Utils
open XamlMath
open XamlMath.Atoms

// Tests for the LaTeX commands added on top of the JMathTeX command set:
//   style switches : \displaystyle, \textstyle, \scriptstyle, \scriptscriptstyle
//   annotations    : \overset, \underset, \stackrel
//   extent         : \phantom, \hphantom, \vphantom, \smash, \mathllap, \mathrlap, \mathclap
//   frames         : \boxed, \fbox
//   font switches  : \mathbb, \mathbf, \mathsf, \mathtt, \mathfrak, \mathscr and the \text* family
//   arrows         : \xrightarrow, \xleftarrow, \xleftrightarrow, \xRightarrow, \xLeftarrow,
//                    \xLeftrightarrow, \xmapsto, \impliedby
//   environments   : matrix, cases, aligned, split, gather, gathered, smallmatrix and the starred forms
type AdditionalCommandsTests() =
    static do initializeFontResourceLoading()

    static let parseRoot (markup: string) =
        WpfTeXFormulaParser.Instance.Parse(markup).RootAtom

    static let environment = WpfTeXEnvironment.Create()

    [<Theory>]
    [<InlineData(@"\displaystyle x")>]
    [<InlineData(@"\textstyle x")>]
    [<InlineData(@"\scriptstyle x")>]
    [<InlineData(@"\scriptscriptstyle x")>]
    [<InlineData(@"\displaystyle\sum_{i=1}^{n} i")>]
    [<InlineData(@"\displaystyle\frac{a}{b}")>]
    member _.``style switches are parsed as a StyleAtom``(markup: string) =
        Assert.IsType<StyleAtom>(parseRoot markup) |> ignore

    [<Fact>]
    member _.``a style switch takes the rest of its group, not just the next element``() =
        // The limits have to end up inside the switch: display style is what puts them above and below the
        // operator, and only a big operator that carries its own limits can be drawn that way.
        let atom = Assert.IsType<StyleAtom>(parseRoot @"\displaystyle\sum_{i=1}^{n}")
        let operatorAtom = Assert.IsType<BigOperatorAtom>(atom.BaseAtom)
        Assert.NotNull(operatorAtom.LowerLimitAtom)
        Assert.NotNull(operatorAtom.UpperLimitAtom)

    [<Fact>]
    member _.``a style switch inside a group ends with the group``() =
        // The "b" is outside the braces, so it keeps the outer style and the root is a row, not the switch.
        Assert.IsType<RowAtom>(parseRoot @"{\scriptstyle a} b") |> ignore

    [<Theory>]
    [<InlineData(@"\overset{a}{b}")>]
    [<InlineData(@"\underset{a}{b}")>]
    member _.``overset and underset are parsed as an UnderOverAtom``(markup: string) =
        Assert.IsType<UnderOverAtom>(parseRoot markup) |> ignore

    [<Fact>]
    member _.``stackrel is typed as a relation``() =
        let atom = Assert.IsType<TypedAtom>(parseRoot @"\stackrel{f}{\rightarrow}")
        Assert.Equal(TexAtomType.Relation, atom.GetLeftType())
        Assert.Equal(TexAtomType.Relation, atom.GetRightType())

    [<Theory>]
    [<InlineData(@"\phantom{x}")>]
    [<InlineData(@"\hphantom{x}")>]
    [<InlineData(@"\vphantom{x}")>]
    member _.``the phantom family is parsed as a PhantomAtom``(markup: string) =
        Assert.IsType<PhantomAtom>(parseRoot markup) |> ignore

    [<Fact>]
    member _.``smash keeps the width and drops the height``() =
        let box = (parseRoot @"\smash{\frac{a}{b}}").CreateBox(environment)
        let reference = (parseRoot @"\frac{a}{b}").CreateBox(environment)
        Assert.Equal(reference.Width, box.Width)
        Assert.Equal(0.0, box.Height)
        Assert.Equal(0.0, box.Depth)

    [<Theory>]
    [<InlineData(@"\mathllap{x}")>]
    [<InlineData(@"\mathrlap{x}")>]
    [<InlineData(@"\mathclap{x}")>]
    [<InlineData(@"\llap{x}")>]
    [<InlineData(@"\rlap{x}")>]
    [<InlineData(@"\clap{x}")>]
    member _.``the lap family drops the width and keeps the height``(markup: string) =
        Assert.IsType<LapAtom>(parseRoot markup) |> ignore
        let box = (parseRoot markup).CreateBox(environment)
        Assert.Equal(0.0, box.Width)
        Assert.True(box.Height > 0.0)

    [<Theory>]
    [<InlineData(@"\boxed{x}")>]
    [<InlineData(@"\fbox{x}")>]
    [<InlineData(@"\boxed{\frac{a}{b}}")>]
    member _.``boxed is parsed as a BoxedAtom``(markup: string) =
        Assert.IsType<BoxedAtom>(parseRoot markup) |> ignore

    [<Fact>]
    member _.``a frame is wider and taller than what it frames``() =
        let framed = (parseRoot @"\boxed{x}").CreateBox(environment)
        let bare = (parseRoot @"x").CreateBox(environment)
        Assert.True(framed.Width > bare.Width)
        Assert.True(framed.Height > bare.Height)

    [<Theory>]
    [<InlineData(@"\xrightarrow{f}")>]
    [<InlineData(@"\xleftarrow{f}")>]
    [<InlineData(@"\xleftrightarrow{f}")>]
    [<InlineData(@"\xRightarrow{f}")>]
    [<InlineData(@"\xLeftarrow{f}")>]
    [<InlineData(@"\xLeftrightarrow{f}")>]
    [<InlineData(@"\xmapsto{f}")>]
    [<InlineData(@"\xrightarrow[g]{f}")>]
    member _.``extensible arrows are parsed as an ExtensibleArrowAtom``(markup: string) =
        Assert.IsType<ExtensibleArrowAtom>(parseRoot markup) |> ignore

    [<Fact>]
    member _.``an extensible arrow grows to fit its label``() =
        let short = (parseRoot @"\xrightarrow{f}").CreateBox(environment)
        let long = (parseRoot @"\xrightarrow{f \circ g \circ h}").CreateBox(environment)
        Assert.True(long.Width > short.Width)

    [<Theory>]
    [<InlineData(@"\mathbb{R}")>]
    [<InlineData(@"\mathbf{x}")>]
    [<InlineData(@"\mathsf{x}")>]
    [<InlineData(@"\mathtt{x}")>]
    [<InlineData(@"\mathfrak{g}")>]
    [<InlineData(@"\mathscr{L}")>]
    [<InlineData(@"\textrm{x}")>]
    [<InlineData(@"\textbf{x}")>]
    [<InlineData(@"\textit{x}")>]
    [<InlineData(@"\textsf{x}")>]
    [<InlineData(@"\texttt{x}")>]
    [<InlineData(@"\textsc{x}")>]
    member _.``font switches parse to a non-null root atom``(markup: string) =
        Assert.NotNull(parseRoot markup)

    [<Fact>]
    member _.``a text font switch keeps its spaces``() =
        // The argument of a \text* command is text, not maths: the space between the words survives.
        let atom = Assert.IsType<RowAtom>(parseRoot @"\textbf{a b}")
        Assert.Contains(atom.Elements, fun e -> e :? SpaceAtom)

    [<Theory>]
    [<InlineData(@"\begin{matrix}a & b \\ c & d\end{matrix}")>]
    [<InlineData(@"\begin{smallmatrix}a & b \\ c & d\end{smallmatrix}")>]
    [<InlineData(@"\begin{cases}a & x > 0 \\ b & x \leq 0\end{cases}")>]
    [<InlineData(@"\begin{aligned}a &= b \\ c &= d\end{aligned}")>]
    [<InlineData(@"\begin{split}a &= b \\ c &= d\end{split}")>]
    [<InlineData(@"\begin{align*}a &= b \\ c &= d\end{align*}")>]
    [<InlineData(@"\begin{gather}a \\ b\end{gather}")>]
    [<InlineData(@"\begin{gather*}a \\ b\end{gather*}")>]
    [<InlineData(@"\begin{gathered}a \\ b\end{gathered}")>]
    member _.``environments parse to a non-null root atom``(markup: string) =
        Assert.NotNull(parseRoot markup)

    [<Fact>]
    member _.``smallmatrix is set in script style``() =
        let atom = Assert.IsType<StyleAtom>(parseRoot @"\begin{smallmatrix}a & b\end{smallmatrix}")
        Assert.Equal(TexStyle.Script, atom.TargetStyle)

    [<Theory>]
    [<InlineData(@"\implies")>]
    [<InlineData(@"\impliedby")>]
    [<InlineData(@"\iff")>]
    [<InlineData(@"\Longleftarrow")>]
    [<InlineData(@"\textcolor{red}{x}")>]
    [<InlineData(@"\color{red}{x}")>]
    member _.``implication arrows and textcolor parse to a non-null root atom``(markup: string) =
        Assert.NotNull(parseRoot markup)

    [<Theory>]
    [<InlineData(@"\displaystyle\sum_{i=1}^{n} i")>]
    [<InlineData(@"\overset{a}{b}")>]
    [<InlineData(@"\underset{a}{b}")>]
    [<InlineData(@"\stackrel{f}{\rightarrow}")>]
    [<InlineData(@"\phantom{xyz}")>]
    [<InlineData(@"\hphantom{xyz}")>]
    [<InlineData(@"\vphantom{xyz}")>]
    [<InlineData(@"\smash{x}")>]
    [<InlineData(@"\mathclap{xyz}")>]
    [<InlineData(@"\boxed{\frac{a}{b}}")>]
    [<InlineData(@"\mathbb{R}")>]
    [<InlineData(@"\textbf{a b}")>]
    [<InlineData(@"\textsc{abc}")>]
    [<InlineData(@"\xrightarrow[g]{f}")>]
    [<InlineData(@"\xmapsto{f}")>]
    [<InlineData(@"\xLeftrightarrow{f}")>]
    [<InlineData(@"\begin{smallmatrix}a & b \\ c & d\end{smallmatrix}")>]
    [<InlineData(@"\begin{gathered}a \\ b\end{gathered}")>]
    [<InlineData(@"\begin{cases}a & x > 0 \\ b & x \leq 0\end{cases}")>]
    [<InlineData(@"\impliedby")>]
    member _.``a box is created for the command``(markup: string) =
        Assert.NotNull((parseRoot markup).CreateBox(environment))
