# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning v2.0.0](https://semver.org/spec/v2.0.0.html).

## [Unreleased] (3.0.0)
### Added
- New matrix delimiter environments `bmatrix` (`[ ]`), `Bmatrix` (`{ }`), `vmatrix` (`| |`), and `Vmatrix` (`‖ ‖`), each usable both as a command (e.g. `\bmatrix{…}`) and as an environment (e.g. `\begin{bmatrix}…\end{bmatrix}`).
- A batch of common LaTeX symbols and commands, all backed by the already-bundled fonts (no new font files):
  - accent `\mathring`;
  - stretchy over-arrow accents `\overrightarrow` and `\overleftarrow` (drawn to the width of their argument);
  - arrows `\mapsto`, `\longrightarrow`, `\longleftarrow`, `\hookrightarrow`, `\Longrightarrow`, `\Longleftrightarrow`, and the building blocks `\lhook`/`\rhook`;
  - dots `\vdots`, `\ddots`, and `\dots` (an alias of `\ldots`);
  - symbols `\S`, `\P`, `\notin`, `\nexists`, `\implies`, and `\iff`.
  - `\varnothing` is provided as an alias of the `\emptyset` glyph, since the AMS `msbm` font (which carries a distinct glyph) is not bundled.
- Horizontal spacing commands: `\quad`, `\qquad`, the control space `\ `, the tie `~`, and `\hspace{<length>}` (accepting `em`, `ex`, `mu`, `pt`, `pc`, `px`, `bp`, `in`, `cm`, and `mm`).
- A batch of `amssymb` relations: the negations `\nless`, `\ngtr`, `\nleq`, `\ngeq`, `\nleqslant`, `\ngeqslant`, `\nleqq`, `\ngeqq`, `\nprec`, `\nsucc`, `\npreceq`, `\nsucceq`, `\nsim`, `\ncong`, `\nvdash`, `\nvDash`, `\nVdash`, `\nmid`, `\nparallel`, `\nsubseteq`, `\nsupseteq`, `\nsubseteqq`, `\nsupseteqq`, `\ntriangleleft`, `\ntriangleright`, `\ntrianglelefteq`, `\ntrianglerighteq`, `\nleftarrow`, `\nrightarrow`, `\nLeftarrow`, `\nRightarrow`, `\nleftrightarrow`, and `\nLeftrightarrow` (each overlays the base relation with the zero-width `\not`, since the dedicated AMS glyphs live in the unbundled `msbm` font), plus the synonyms `\doublecup`, `\doublecap`, `\restriction`, `\Doteq`, `\llless`, and `\gggtr`.

  Not added, because they require the AMS `msbm` font (not bundled): `\Bbbk`, the Hebrew letters `\beth`/`\gimel`/`\daleth`, `\digamma`/`\varkappa`, `\mho`, `\hslash`, `\eth`, `\backepsilon`, `\smallsetminus`, `\diagup`/`\diagdown`, and the negations/inequalities that have their own combined glyph rather than a `\not` overlay (`\subsetneq`, `\supsetneq`, `\lneqq`, `\gneqq`, `\lvertneqq`, `\gvertneqq`, `\precnsim`, `\lnsim`, `\varsubsetneq`, …).
- Fraction commands: `\dfrac` and `\tfrac` (force display / text style), `\cfrac[l|c|r]{…}{…}` (continued fractions — nested levels stay full size), and the inline slash fractions `\nicefrac{…}{…}` and `\sfrac{…}{…}`.
- Multiple integrals `\iint`, `\iiint`, `\iiiint`, `\idotsint`, `\oiint`, and `\oiiint` (composed from `\int`/`\oint` tightened with negative spacing), and the modulo operators `\bmod`, `\pmod{…}`, and `\pod{…}`.
- Style switches `\displaystyle`, `\textstyle`, `\scriptstyle`, and `\scriptscriptstyle`. They behave like LaTeX switches rather than one-argument commands: each applies from where it appears to the end of its group, so the limits of `\displaystyle\sum_{i=1}^{n}` are set above and below the operator.
- Stacked annotations `\overset{…}{…}`, `\underset{…}{…}`, and `\stackrel{…}{…}` (the last typed as a relation, so it spaces like the arrow it usually sits on).
- Commands that separate a formula's ink from its extent: `\phantom`, `\hphantom`, and `\vphantom` reserve space and print nothing; `\smash` prints without height or depth, and `\mathllap`/`\mathrlap`/`\mathclap` (also spelled `\llap`/`\rlap`/`\clap`) print without width.
- `\boxed{…}` and `\fbox{…}`, which frame their content in a rectangle.
- The font-switching commands `\mathbf`, `\mathbb`, `\mathsf`, `\mathtt`, `\mathfrak`, `\mathscr`, `\textrm`, `\textbf`, `\textit`, `\textsf`, `\texttt`, and `\textsc`. Every one has its own face - see the font entries below. `\mathbb` and `\mathscr` are capitals-only, their faces having nothing else. The `\text*` family treats its argument as text, so the spaces in it survive, as `\text` already did.
- Extensible arrows `\xrightarrow`, `\xleftarrow`, `\xleftrightarrow`, `\xRightarrow`, `\xLeftarrow`, `\xLeftrightarrow`, and `\xmapsto`, each stretched to fit the label written over it and accepting LaTeX's optional under-label (e.g. `\xrightarrow[g]{f}`).
- The environments `matrix`, `cases`, `aligned`, `split`, `gather`, `gathered`, `smallmatrix`, and the starred `align*`/`gather*`. `smallmatrix` is a matrix set in script size; `cases` and `matrix` previously existed only as commands.
- `\Longleftarrow` and the AMS abbreviation `\impliedby`, completing the set alongside `\implies` and `\iff`.
- `\textcolor{…}{…}` as a spelling of the existing two-argument `\color`.
- The AMS symbols-B font, `jlm_msbm10.ttf`, and the 92 symbols it carries — which is most of what `amssymb` was still missing. Notably: real blackboard bold (`\mathbb`), the Hebrew letters `\beth`/`\gimel`/`\daleth`, `\digamma`/`\varkappa`, `\hslash`, `\eth`, `\Bbbk`, `\mho`, `\Finv`, `\Game`, `\diagup`/`\diagdown`, `\smallsetminus`, `\ltimes`/`\rtimes`, `\divideontimes`, `\lessdot`/`\gtrdot`, `\approxeq`, `\eqsim`, `\thicksim`/`\thickapprox`, `\precapprox`/`\succapprox`, `\shortmid`/`\shortparallel`, `\backepsilon`, `\curvearrowleft`/`\curvearrowright`, and the whole `\subsetneq`/`\lneqq`/`\gvertneqq`/`\precnsim` family of strict negations.

  The metrics were generated from the font itself and cross-checked against the AFM in the AMS distribution; the symbol names, atom types and codes come from `amssymb.sty`. Running the same generation over `jlm_msam10.ttf` reproduces the `<Font>` block already in this file character for character, and assigns every AMSa symbol the code it already had.

  `\mathbb` is capitals-only: `msbm10` carries no blackboard lowercase or digits, so those fall through to the default mapping, as `\mathcal` already does.
- `\circledR`, `\dashrightarrow`, `\dashleftarrow` and `\dasharrow`. These needed no new font — they were in the bundled `msam10` all along, but `amssymb` declares them outside `\DeclareMathSymbol` (via `\mathhexbox@` and by repeating an axis piece), so they had been missed.
- Ralph Smith's Formal Script, `jlm_rsfs10.ttf`, behind `\mathscr`. It had been pointed at the symbol font's calligraphic capitals — the same alphabet `\mathcal` uses — so the two commands drew identical output. They are now the two distinct alphabets LaTeX means. Capitals only, as `rsfs10` has nothing else.
- Six more faces, converted from the Type 1 outlines in the AMS `amsfonts` distribution, so that every font-switching command now has the alphabet it names rather than a roman stand-in: `cmbx10` behind `\mathbf`/`\textbf`, `cmss10` behind `\mathsf`/`\textsf`, `cmtt10` behind `\mathtt`/`\texttt`, `cmti10` behind `\textit`, `cmcsc10` behind `\textsc`, and Euler Fraktur `eufm10` behind `\mathfrak`.

  `\textit` had been pointed at the *maths* italic, which spaces letters as though each were a separate variable; `\textsc` faked small capitals by mapping lowercase onto the capital range, and now uses the small capitals `cmcsc10` actually has. Metrics for all six were generated from the converted fonts and check out against the AFM files that ship with the Type 1 sources — every advance width, every code.
- `\boldsymbol{…}` (also spelled `\bm`), with the bold maths faces `cmmib10` and `cmbsy10` behind it. It is not a text style: a text style picks which alphabet a *letter* comes from, and Greek letters and symbols are resolved by name straight out of the maths and symbol fonts, so no text style could ever have reached them. Instead `TexEnvironment` carries an `IsBold` flag that every character consults as it resolves, and each `<Font>` in `DefaultTexFont.xml` names the `boldId` of the face to take characters from while it is set. So `\boldsymbol{\alpha}`, `\boldsymbol{\nabla}` and `\boldsymbol{x}` all come out bold, and a character whose font has no bold companion — an AMS symbol, say — is simply left as it is.
- `\overbrace{…}` and `\underbrace{…}`, drawn with the horizontal-delimiter machinery that was already in the library but that nothing reached from markup. Both are operators, as in LaTeX: the script that follows (`^` for `\overbrace`, `_` for `\underbrace`) is set beyond the brace rather than beside it, and a script on the other side stays an ordinary one.
- `\substack{… \\ …}`, for stacking the lines of a big operator's limit — script size, set solid rather than at table row spacing.

### Removed
- **(Breaking change!)** Support for .NET 6 and 7. The new list of supported frameworks:
  - for **WPF-Math**: **.NET Framework 4.6.2** or later, **.NET 8** or later;
  - for **Avalonia-Math**: **.NET Framework 4.6.2** or later, **.NET Standard 2.0** or later, **.NET 8** or later.

### Changed
- **(Breaking!)** `ITeXFont` gains `GetBoldCharInfo`, needed so that `\boldsymbol` can ask a font for the bold companion of a character it has already resolved. Only affects code implementing the interface itself.
- The exception classes are now `sealed` (should not break anything, since there never was any sense in extending them in the user code).
- Avalonia: `AvaloniaMathFontProvider::Instance` is now read-only.

### Fixed
- `pmatrix` now renders with parentheses `( )` instead of square brackets `[ ]`, matching LaTeX (the square-bracket variant is `bmatrix`).
- Matrix cells of differing heights within a row now share a common baseline instead of each being vertically centred, so short glyphs (e.g. `a`) no longer float above taller ones (e.g. `b`). Affects `\matrix`/`\pmatrix`/`\cases`/`align` and the matrix-based commands.
- `UnderOverAtom` built the gap below the base with the *over* unit rather than the under one. An atom that only has an under-annotation leaves the over unit at its default (`em`), so a gap asked for in `mu` came out 18× too large and the annotation sat far below the base. Visible in `\underset{n \to \infty}{\lim}`, and in anything going through `TexFormulaHelper.PutUnder`.
- `OverUnderBox` reached for its script box while positioning, even when it had none, so a horizontal delimiter with no label (`\overbrace{a+b}`) threw a `NullReferenceException` on render.
- `OverUnderDelimiter` padded its delimiter with `VerticalBox(box, rest, alignment)` as though the second argument were the width to fill, the way its `HorizontalBox` sibling reads. It is the leftover to share out, so the delimiter gained half a width of padding on each side and the brace was drawn half a width to the right of what it spans. Only visible when a label made the atom wider than the delimiter, since otherwise the padding is skipped altogether.
- `GeometryElementRenderer` applied its transformations to the `Matrix` that `GeometryGroup.Transform.Value` returns — a struct, returned by value — so every transformation was silently dropped and rotated content (a horizontal brace, say) landed unrotated at the origin. They are now accumulated and assigned, in the same order the drawing-context renderer applies them.
- 34 negated relations (`\nleq`, `\nsubseteq`, `\nRightarrow`, …) were composed by overlaying the base relation with a zero-width `\not`, and `\varnothing` was an alias for the `\emptyset` glyph. All of them are real single glyphs in `msbm10` now, so the approximations are gone.
- `DefaultTexFontParser` sized its font array from a hard-coded count, so adding a sixth font threw `IndexOutOfRange` rather than working. It is now taken from the ids the data declares.

## [2.1.0] - 2023-07-15
### Changed
- AvaloniaMath is now based on (and thus compatible with) Avalonia 11.

### Fixed
- [#272: Parser crash on `\left{2+2\right\}`][issue-272].

## [2.0.0] - 2023-06-11
Although a major release with certain formally breaking changes, these changes should hopefully not break any normal usage of the library (if the user code doesn't implement own `IElementRenderer`).

### Changed
- **(Breaking!)** Avalonia: rename `AvaloniaBrushExtensions` to `AvaloniaExtensions`.
- **(Breaking!)** WPF: rename `WpfBrushExtensions` to `WpfExtensions`.
- **(Breaking!)** `IElementRenderer` is now required to implement a new method, `RenderLine` (required for the new `\cancel*` commands).

### Added
- [`jlm_msam10.ttf` font with a lot of new characters][pull-400], thanks @Orace, that closes the following requests:
  - [`angle`, `measuredangle`, and `square` symbols are not rendered][issue-127],
  - [An unsupported command `\geqslant`][issue-313].
- New commands: `\cancel`, `\bcancel`, `\xcancel` (partially addressing [issue #349][issue-349]), thanks @Orace.

### Fixed
- [#409: Exception on empty matrix row][issue-409].

## [1.1.0] - 2023-04-30
### Fixed
- [#387: Alignment issue in matrix with an empty cell][issue-387], thanks @Orace.
- [#389: Padding is not respected with `\cases`][issue-389], thanks @Orace.

### Added
- [#381: Support for `\begin{align}`][issue-381], thanks @Orace.
- The README file is now packed into NuGet for better documentation.

## [1.0.0] - 2023-02-07
### Changed
- The project has been renamed to XAML-Math. This doesn't change the NuGet package names (WpfMath and AvaloniaMath) and their contents (including namespaces of most entities), but changed the contents of the shared assembly.
- **(Breaking change.)** `WpfMath.Shared` assembly was renamed to `XamlMath.Shared`:
  - a lot of types were migrated from `WpfMath` namespace to `XamlMath`,
  - all the internal XML file resources were migrated to the new namespace as well.
- The `XamlMath.Shared` assembly has been extracted into a separately published NuGet package.
- **(Breaking change.)** WPF-Math no longer supports .NET Framework 4.5.2, .NET Core 3.1, and .NET 5.0, because they are out of support by Microsoft. The only supported runtimes are:
  - .NET Framework 4.6.2 or later,
  - .NET 6.0 and later.
- **(Breaking change.)** `WpfMath.Colors.IColorParser::Parse` now accepts `IReadOnlyList` instead of `IEnumerable`.
- **(Minor breaking change.)** `XamlMath.Colors.StandardColorParsers::Dictionary` static public field is now `readonly`.
- **(Minor breaking change.)** `WpfMath.Rendering.WpfBrushFactory`'s constructor is now `private`.
- **(Minor breaking change.)** `WpfMath.Rendering.WpfBrushFactory::Instance` static public field is now `readonly`.

### Removed
- **(Breaking change.)** Delete the `TexRenderer` class. Use extension methods from `WpfMath.Rendering.WpfTeXFormulaExtensions` and `WpfMath.Rendering.TeXFormulaExtensions` to replace its functionality in your code.

### Added
- Avalonia version is now available! Install the **AvaloniaMath** NuGet package to get it.

  It supports the following .NET variants:
  - .NET Framework 4.6.2 or later,
  - .NET Standard 2.0 or later,
  - .NET 6.0 or later.
- Portable PDB packages are now published to NuGet.

## [0.13.1] - 2023-01-29
### Fixed
- [#350: Unable to resolve dependency 'WpfMath.Shared'][issue-350]

## [0.13.0] - 2023-01-27
### Changed
- **(Breaking change.)** The library is now distributed in the form of two assemblies: `WpfMath` and `WpfMath.Shared` (as a future cross-platform core of the library).

  Both of the assemblies are still distributed in the same NuGet package.

  A lot of types were moved to `WpfMath.Shared` assembly (preserving their namespaces).
- **(Breaking change.)** It is no longer recommended to create instances of `TexEnvironment` using the public constructor. Use `WpfMath.Rendering.WpfTeXEnvironment::Create` instead.
- **(Breaking change.)** It is no longer recommended to create instances of `TexFormulaParser` using public constructors. Use `WpfMath.Parsers.WpfTeXFormulaParser::Instance` instead.
- **(Breaking change.)** WPF-specific `WpfMath.Rendering.IBrush` has been replaced with `WpfMath.Rendering.IBrush` in most of the public interfaces. Use `WpfMath.Rendering.WpfBrushExtensions` to convert back and forth to the WPF type.
- **(Breaking change.)** `WpfMath.Rendering.IElementRenderer` has been updated:
  - `RenderGlyphRun` has been replaced with `RenderCharacter` method (not reliant on any WPF-specific types),
  - `RenderRectangle` now receives an instance of a new `WpfMath.Rendering.Rectangle` type (decoupled from WPF).
- `WpfMath.TexRenderer` is now obsolete. Consult the documentation on new recommended ways to perform custom rendering. There are new extension methods in two classes (`WpfMath.Rendering.WpfTeXFormulaExtensions` and `WpfMath.Rendering.TeXFormulaExtensions`) that are the main way to render formulae now.
- **(Breaking change.)** `WpfMath.TexFormnula::GetRenderer` is gone. Create a `TexRenderer` using constructor (obsolete) or use the aforementioned extension methods instead.

### Added
- `WpfMath.CharInfo`: a new public type to work with a font character. Use `WpfMath.Fonts.WpfCharInfoEx::GetGlyphRun` if you need to get a `System.Windows.Media.GlyphRun` from it.
- `WpfMath.Rendering.WpfTeXFormulaExtensions` to render a `WpfMath.TexFormula` into a `System.Windows.Media.Imaging.BitmapSource` or `System.Windows.Media.Geometry`.
- New classes for WPF-Math porting to platforms other than WPF (consult the `WpfMath.Rendering.IElementRenderer` interface and `TexFormulaParser` constructor parameters to know more).
- `WpfMath.Colors.RgbaColor` as a new portable color representation.
- `WpfMath.Fonts.IFontProvider`: implement this interface to provide alternate font reading functionality.
- `WpfMath.Fonts.ITeXFont`: implement this interface to provide access to a platform-specific font resource.
- `WpfMath.Rendering.IBrushFactory`: implement this interface to provide access to creation of platform-specific solid-colored brushes.
- `WpfMath.TeXFontMetrics` that contains some measurements of a font glyph.
- A utility `Result` struct is now public.

## [0.12.0] - 2023-01-07
### Added
- TeX's environment support with only one environment for now: `\begin{pmatrix}` (see [#329][pull-329]).

### Changed
- The project is now built on .NET 7 SDK and C# 11 (shouldn't change the supported framework set).

## [0.11.0] - 2021-08-31
### Added
- [#262: Add \mod operator from amsmath][pull-262]

### Fixed
- [#304: SystemFontFamilies does not return all FontFamilies in Chinese System][issue-304]

## [0.10.0] - 2021-07-06
### Changed
- (**Breaking change!**) Removed support for .NET Core 3.0. .NET Core 3.1 or later is supported from now (.NET Framework 4.5.2 is still supported; .NET 5.0 or later is supported, too).

### Added
- [#277: Enable nullable reference types][pull-277]

### Fixed
- [#99: `Foreground` property not working on `FormulaControl`][issue-99]
- [#283: Fix typo in `SystemTextFontNameProperty`][pull-283]
- [#244: `\limsup` throws exception][issue-244]
- [#254: Fix for scripts with curly braces after a command with curly braces][pull-254] (e.g. `\hat{x}_{y}`)
- [#261: Crash on empty `\sqrt{}`][pull-261]
- [#275: `OverUnderBox` constructor may dereference scriptBox parameter if it's `null`][issue-275]

## [0.9.0] - 2020-07-31
### Added
- [#59: Extended delimiter support][issue-59]: e.g. `\left\\`
- [#149: Newline command support][issue-149]: try using `\\` outside of a matrix
- [#252: Support for \\{ and \\} commands][pull-252]

### Fixed
- [#139: Exception thrown by \\,][issue-139]
- [#151: Wrong sources detected for complex predefined formulae][issue-151]
- [#225: \text doesn't work with indices if there's only one Cyrillic letter][issue-225]
- [#248: Wrong exception gets thrown for \text{∅}][issue-248]
- [#253: Added equal padding to all sides when saving to bitmap][pull-253]
- [#257: IndexOutOfRangeException throws when using \color][issue-257]

## [0.8.0] - 2020-01-03
### Added
- [#165: Extended color support for \color and \colorbox][issue-165], see [the documentation][docs.colors]
- [#174: \binom command][issue-174]
- [#209: WPF support on .NET Core 3.0][issue-209]
- [#226: Add TFM into nuspec][issue-226]

### Fixed
- [#128: \colorbox renders radical index invisible][issue-128]
- [#158: \color{red} crashes parser][issue-158]
- [#203: `\colorbox{red}{}` crashes the parser][issue-203]
- [#212: `\mathrm{}` shouldn't throw exn][pull-212]

## [0.7.0] - 2019-08-24
### Changed
- (**Breaking change!**) [#198: Migrate to .NET 4.5.2][issue-198]

### Fixed
- [#141: _ doesn't render inside \text][issue-141]
- [#171: Make \lbrack and \lbrack commands for parentheses][issue-171]
- [#172: NullReferenceException on some types of markup][pull-172]

### Added
- [#100: Support for matrix commands][issue-100]

## [0.6.0] - 2018-11-19
### Fixed
- [#60: Command arguments passed without braces][issue-60]
- [#168: Poor exported PNG quality and text cutting][issue-168]

### Added
- [#91: \overline command][issue-91]
- [#145: Implemented the underline command][pull-145]

### Changed

- (Refactoring) [#152: Move converters to namespace][pull-152]

## [0.5.0] - 2018-05-08
### Fixed
- [#101: Fixed crash on empty square root][pull-101]
- [#103: Fixed a crash if inserting a whitespace after a _ or ^ symbol][pull-103]
- [#102: Fixed a crash when rendering unsupported characters like "Å"][pull-102]
- [#104: Error when rendering `\text{æ,}`][issue-104]
- [#119: Big operator rendering problem inside `\frac`][issue-119]
- [#129: Summation of two Cyrillic symbols][issue-129]
- [#133: Fix issues with exceptions][pull-133]

### Added
- [#92: Extract Renderer code to a single class/interface][issue-92]
- [#115: Give a range of source string to Box][issue-115]
- [#123: Add formula highlighting][pull-123]
- (Documentation) [#108: Dealing with XML][pull-108]

## [0.4.0] - 2017-09-11
### Fixed
- [#80: force `StreamWriter` to flush buffers][pull-80]
- [#88: Bar alignment][issue-88]

### Added
- [#79: SVG: Added support for curves and removed flattening of the geometry][pull-79]
- [#82: Support UTF8][issue-82]
- [#84: Add \text{} command][issue-84]

## [0.3.1] - 2017-03-18
### Changed
- Bug fixes, improved render quality
- [#53: Fix crash while parsing empty groups][pull-53]
- [#54: Remove delimiter type constraint][pull-54]
- [#58: Support for empty delimiters][pull-58] _(that means [#14: Extensible delimiters (\left and \right)][issue-14] is completely fixed)_
- [#71: Use guidelines for WPF render][pull-71] _(partial fix for [#50: Improve blurred output][issue-50], should improve image quality for some cases)_
- [Adjust metrics for radical symbol][commit-14c303d]
- [#74: Add support for scripts to delimiters][pull-74] (fix for [#62: Delimiters should support scripts][issue-62])

## [0.3.0] - 2017-02-24
### Fixed
- [#14: Extensible delimiters (\left and \right)][issue-14] _(not completely fixed, but in a usable state already)_
- [#15: Add formula control to the library][issue-15]
- [#38: Cannot change text style][issue-38]
- [#32: Add mathematical functions][issue-32]
- [#24: Integral sign displays incorrectly][issue-24]

## [0.2.0] - 2017-02-16
Merged all the patches from the Launchpad repository (see #3 for a full list). The most interesting ones are:

- Fix for culture-dependent number parsing in `XmlUtils`
- Addition of SVG rendering functionality

## [0.1.0] - 2017-02-11
This was the initially published version. It consisted of the original code from the Launchpad project site (but only the version for .NET 4 was published).

The features included more or less correspond to what was available in [JMathTex](https://jmathtex.sourceforge.net/), as the project started as a .NET port of that.

Read more at [the licensing history](docs/licensing-history.md) document.

[docs.colors]: docs/colors.md

[commit-14c303d]: https://github.com/ForNeVeR/xaml-math/commit/14c303d30eba735af4faa5e72e149c60add00293
[issue-14]: https://github.com/ForNeVeR/xaml-math/issues/14
[issue-15]: https://github.com/ForNeVeR/xaml-math/issues/15
[issue-24]: https://github.com/ForNeVeR/xaml-math/issues/24
[issue-32]: https://github.com/ForNeVeR/xaml-math/issues/32
[issue-38]: https://github.com/ForNeVeR/xaml-math/issues/38
[issue-50]: https://github.com/ForNeVeR/xaml-math/issues/50
[issue-59]: https://github.com/ForNeVeR/xaml-math/issues/59
[issue-60]: https://github.com/ForNeVeR/xaml-math/issues/60
[issue-62]: https://github.com/ForNeVeR/xaml-math/issues/62
[issue-82]: https://github.com/ForNeVeR/xaml-math/issues/82
[issue-84]: https://github.com/ForNeVeR/xaml-math/issues/84
[issue-88]: https://github.com/ForNeVeR/xaml-math/issues/84
[issue-91]: https://github.com/ForNeVeR/xaml-math/issues/91
[issue-92]: https://github.com/ForNeVeR/xaml-math/issues/92
[issue-99]: https://github.com/ForNeVeR/xaml-math/issues/99
[issue-100]: https://github.com/ForNeVeR/xaml-math/issues/100
[issue-104]: https://github.com/ForNeVeR/xaml-math/issues/104
[issue-115]: https://github.com/ForNeVeR/xaml-math/issues/115
[issue-119]: https://github.com/ForNeVeR/xaml-math/issues/119
[issue-128]: https://github.com/ForNeVeR/xaml-math/issues/128
[issue-127]: https://github.com/ForNeVeR/xaml-math/issues/127
[issue-129]: https://github.com/ForNeVeR/xaml-math/issues/129
[issue-139]: https://github.com/ForNeVeR/xaml-math/issues/139
[issue-141]: https://github.com/ForNeVeR/xaml-math/issues/141
[issue-149]: https://github.com/ForNeVeR/xaml-math/issues/149
[issue-151]: https://github.com/ForNeVeR/xaml-math/issues/151
[issue-158]: https://github.com/ForNeVeR/xaml-math/issues/158
[issue-165]: https://github.com/ForNeVeR/xaml-math/issues/165
[issue-168]: https://github.com/ForNeVeR/xaml-math/issues/168
[issue-171]: https://github.com/ForNeVeR/xaml-math/issues/171
[issue-174]: https://github.com/ForNeVeR/xaml-math/issues/174
[issue-198]: https://github.com/ForNeVeR/xaml-math/issues/198
[issue-203]: https://github.com/ForNeVeR/xaml-math/issues/203
[issue-209]: https://github.com/ForNeVeR/xaml-math/issues/209
[issue-225]: https://github.com/ForNeVeR/xaml-math/issues/225
[issue-226]: https://github.com/ForNeVeR/xaml-math/issues/226
[issue-244]: https://github.com/ForNeVeR/xaml-math/issues/244
[issue-248]: https://github.com/ForNeVeR/xaml-math/issues/248
[issue-257]: https://github.com/ForNeVeR/xaml-math/issues/257
[issue-272]: https://github.com/ForNeVeR/xaml-math/issues/272
[issue-275]: https://github.com/ForNeVeR/xaml-math/issues/275
[issue-304]: https://github.com/ForNeVeR/xaml-math/issues/304
[issue-313]: https://github.com/ForNeVeR/xaml-math/issues/313
[issue-349]: https://github.com/ForNeVeR/xaml-math/issues/349
[issue-350]: https://github.com/ForNeVeR/xaml-math/issues/350
[issue-381]: https://github.com/ForNeVeR/xaml-math/issues/381
[issue-387]: https://github.com/ForNeVeR/xaml-math/issues/387
[issue-389]: https://github.com/ForNeVeR/xaml-math/issues/389
[issue-409]: https://github.com/ForNeVeR/xaml-math/issues/409
[pull-53]: https://github.com/ForNeVeR/xaml-math/pull/53
[pull-54]: https://github.com/ForNeVeR/xaml-math/pull/54
[pull-58]: https://github.com/ForNeVeR/xaml-math/pull/58
[pull-71]: https://github.com/ForNeVeR/xaml-math/pull/71
[pull-74]: https://github.com/ForNeVeR/xaml-math/pull/74
[pull-79]: https://github.com/ForNeVeR/xaml-math/pull/79
[pull-80]: https://github.com/ForNeVeR/xaml-math/pull/80
[pull-101]: https://github.com/ForNeVeR/xaml-math/pull/101
[pull-102]: https://github.com/ForNeVeR/xaml-math/pull/102
[pull-103]: https://github.com/ForNeVeR/xaml-math/pull/103
[pull-108]: https://github.com/ForNeVeR/xaml-math/pull/108
[pull-123]: https://github.com/ForNeVeR/xaml-math/pull/123
[pull-133]: https://github.com/ForNeVeR/xaml-math/pull/133
[pull-145]: https://github.com/ForNeVeR/xaml-math/pull/145
[pull-152]: https://github.com/ForNeVeR/xaml-math/pull/152
[pull-172]: https://github.com/ForNeVeR/xaml-math/pull/172
[pull-212]: https://github.com/ForNeVeR/xaml-math/pull/212
[pull-252]: https://github.com/ForNeVeR/xaml-math/pull/252
[pull-253]: https://github.com/ForNeVeR/xaml-math/pull/253
[pull-254]: https://github.com/ForNeVeR/xaml-math/pull/254
[pull-261]: https://github.com/ForNeVeR/xaml-math/pull/261
[pull-262]: https://github.com/ForNeVeR/xaml-math/pull/262
[pull-277]: https://github.com/ForNeVeR/xaml-math/pull/277
[pull-283]: https://github.com/ForNeVeR/xaml-math/pull/283
[pull-329]: https://github.com/ForNeVeR/xaml-math/pull/329
[pull-400]: https://github.com/ForNeVeR/xaml-math/pull/400

[0.1.0]: https://github.com/ForNeVeR/xaml-math/releases/tag/0.1.0
[0.2.0]: https://github.com/ForNeVeR/xaml-math/compare/0.1.0...0.2.0
[0.3.0]: https://github.com/ForNeVeR/xaml-math/compare/0.2.0...0.3.0
[0.3.1]: https://github.com/ForNeVeR/xaml-math/compare/0.3.0...0.3.1
[0.4.0]: https://github.com/ForNeVeR/xaml-math/compare/0.3.1...0.4.0
[0.5.0]: https://github.com/ForNeVeR/xaml-math/compare/0.4.0...0.5.0
[0.6.0]: https://github.com/ForNeVeR/xaml-math/compare/0.5.0...0.6.0
[0.7.0]: https://github.com/ForNeVeR/xaml-math/compare/0.6.0...0.7.0
[0.8.0]: https://github.com/ForNeVeR/xaml-math/compare/0.7.0...0.8.0
[0.9.0]: https://github.com/ForNeVeR/xaml-math/compare/0.8.0...0.9.0
[0.10.0]: https://github.com/ForNeVeR/xaml-math/compare/0.9.0...v0.10.0
[0.11.0]: https://github.com/ForNeVeR/xaml-math/compare/v0.10.0...v0.11.0
[0.12.0]: https://github.com/ForNeVeR/xaml-math/compare/v0.11.0...v0.12.0
[0.13.0]: https://github.com/ForNeVeR/xaml-math/compare/v0.12.0...v0.13.0
[0.13.1]: https://github.com/ForNeVeR/xaml-math/compare/v0.13.0...v0.13.1
[1.0.0]: https://github.com/ForNeVeR/xaml-math/compare/v0.13.1...v1.0.0
[1.1.0]: https://github.com/ForNeVeR/xaml-math/compare/v1.0.0...v1.1.0
[2.0.0]: https://github.com/ForNeVeR/xaml-math/compare/v1.1.0...v2.0.0
[2.1.0]: https://github.com/ForNeVeR/xaml-math/compare/v2.0.0...v2.1.0
[Unreleased]: https://github.com/ForNeVeR/xaml-math/compare/v2.1.0...HEAD
