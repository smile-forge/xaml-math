#if NET462
using XamlMath.Compatibility;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using XamlMath.Boxes;
using XamlMath.Parsers.Matrices;
using SurroundingGap = System.Tuple<double, double>;

namespace XamlMath.Atoms;

/// <summary>An atom representing a tabular arrangement of atoms.</summary>
internal sealed record MatrixAtom : Atom
{
    /// <summary>Used for grouping of align statements into several columns.</summary>
    /// <remarks>
    /// See section "Aligning several equations" of
    /// <a href="https://www.overleaf.com/learn/latex/Aligning_equations_with_amsmath">this article</a> for details.
    /// </remarks>
    private const double AlignGroupLeftPadding = 4;
    public const double DefaultPadding = 0.35;

    public MatrixAtom(
        SourceSpan? source,
        IEnumerable<IEnumerable<Atom?>> cells,
        MatrixCellAlignment matrixCellAlignment,
        double verticalPadding = DefaultPadding,
        double horizontalPadding = DefaultPadding,
        ArrayColumnSpec? columnSpec = null,
        IReadOnlyCollection<int>? horizontalRules = null) : base(source)
    {
        MatrixCells = ToImmutableCollection(cells.Select(ToImmutableCollection));
        MatrixCellAlignment = matrixCellAlignment;
        VerticalPadding = verticalPadding;
        HorizontalPadding = horizontalPadding;
        ColumnSpec = columnSpec;
        HorizontalRules = horizontalRules;
    }

    /// <summary>Per-column alignment and vertical rules, for an <c>array</c>; null for everything else.</summary>
    public ArrayColumnSpec? ColumnSpec { get; }

    /// <summary>Row boundaries carrying an <c>\hline</c>, numbered from 0 (above the first row).</summary>
    public IReadOnlyCollection<int>? HorizontalRules { get; }

    public IReadOnlyCollection<IReadOnlyCollection<Atom?>> MatrixCells { get; }

    public double VerticalPadding { get; }

    public double HorizontalPadding { get; }

    public MatrixCellAlignment MatrixCellAlignment { get; }

    protected override Box CreateBoxCore(TexEnvironment environment)
    {
        Box CreateCell(Atom? atom) => atom is null ? StrutBox.Empty : atom.CreateBox(environment);

        var cells = MatrixCells.Select(row => row.Select(CreateCell).ToArray()).ToArray();
        var columnEdges = new List<double>();
        var rowHeights = new List<double>();
        var columnCount = MatrixCells.Max(row => row.Count);
        var columnWidths = Enumerable.Range(0, columnCount)
                                     .Select(i => cells.Where(row => i < row.Length)
                                     .Max(row => row[i].TotalWidth))
                                     .ToArray();

        var rowsContainer = new VerticalBox();
        foreach (var row in cells)
        {
            var rowContainer = new HorizontalBox();
            // Align cells on a common baseline within the row (LaTeX behaviour): the row is made
            // tall enough for the largest ascent and deepest descent it contains, but every cell
            // sits on the same baseline rather than being vertically centred (which would raise
            // short glyphs like "a" above taller ones like "b").
            var columnEdgeX = 0.0;
            var rowAscent = row.Length > 0 ? row.Max(cell => cell.Height) : 0.0;
            var rowDescent = row.Length > 0 ? row.Max(cell => cell.Depth) : 0.0;
            var halfVPadding = VerticalPadding / 2;

            for (var j = 0; j < columnCount; ++j)
            {
                var cell = row[j];
                var columnWidth = columnWidths[j];

                var topGap = rowAscent - cell.Height + halfVPadding;
                var bottomGap = rowDescent - cell.Depth + halfVPadding;
                var cellContainer = new VerticalBox();
                cellContainer.Add(new StrutBox(0.0, topGap, 0.0, 0.0));
                cellContainer.Add(cell);
                cellContainer.Add(new StrutBox(0.0, bottomGap, 0.0, 0.0));
                cellContainer.Height = cellContainer.TotalHeight;
                cellContainer.Depth = 0;


                var hFreeSpace = columnWidth - cell.TotalWidth;
                var (lGap, rGap) = GetLeftRightGap(hFreeSpace, j);
                rowContainer.Add(new StrutBox(lGap, 0.0, 0.0, 0.0));
                rowContainer.Add(cellContainer);
                rowContainer.Add(new StrutBox(rGap, 0.0, 0.0, 0.0));

                if (columnEdges.Count == j)
                    columnEdges.Add(columnEdgeX);
                columnEdgeX += lGap + columnWidth + rGap;
            }

            rowHeights.Add(rowContainer.TotalHeight);
            rowsContainer.Add(rowContainer);
        }

        var axis = environment.MathFont.GetAxisHeight(environment.Style);
        var containerHeight = rowsContainer.TotalHeight;
        rowsContainer.Depth = containerHeight / 2 - axis;
        rowsContainer.Height = containerHeight / 2 + axis;

        var rules = CreateRulesBox(environment, rowsContainer, columnEdges, columnEdgeXTotal(), rowHeights);
        if (rules == null)
            return rowsContainer;

        var layered = new LayeredBox();
        layered.Add(rowsContainer);
        layered.Add(rules);
        layered.Height = rowsContainer.Height;
        layered.Depth = rowsContainer.Depth;
        layered.Width = rowsContainer.Width;
        return layered;

        double columnEdgeXTotal() => rowsContainer.Width;
    }

    private Box? CreateRulesBox(
        TexEnvironment environment,
        Box grid,
        IReadOnlyList<double> columnEdges,
        double totalWidth,
        IReadOnlyList<double> rowHeights)
    {
        var wantsVertical = ColumnSpec?.VerticalRules.Count > 0;
        var wantsHorizontal = HorizontalRules?.Count > 0;
        if (!wantsVertical && !wantsHorizontal)
            return null;

        var thickness = environment.MathFont.GetDefaultLineThickness(environment.Style);

        var verticalAt = new List<double>();
        if (ColumnSpec != null)
        {
            foreach (var boundary in ColumnSpec.VerticalRules)
            {
                // A boundary past the last column is the right edge; the rule is drawn inside it.
                var x = boundary < columnEdges.Count ? columnEdges[boundary] : totalWidth - thickness;
                verticalAt.Add(x);
            }
        }

        var horizontalAt = new List<double>();
        if (HorizontalRules != null)
        {
            foreach (var boundary in HorizontalRules)
            {
                var y = 0.0;
                for (var i = 0; i < boundary && i < rowHeights.Count; i++)
                    y += rowHeights[i];
                horizontalAt.Add(boundary >= rowHeights.Count ? y - thickness : y);
            }
        }

        return new GridRulesBox(environment, verticalAt, horizontalAt, thickness)
        {
            Width = totalWidth,
            Height = grid.Height,
            Depth = grid.Depth,
        };
    }

    private SurroundingGap GetLeftRightGap(double hFreeSpace, int columnIndex)
    {
        var lrPadding = HorizontalPadding / 2;

        if (ColumnSpec != null)
        {
            return ColumnSpec.AlignmentOf(columnIndex) switch
            {
                TexAlignment.Left => new SurroundingGap(lrPadding, lrPadding + hFreeSpace),
                TexAlignment.Right => new SurroundingGap(lrPadding + hFreeSpace, lrPadding),
                _ => new SurroundingGap(lrPadding + hFreeSpace / 2, lrPadding + hFreeSpace / 2),
            };
        }

        return MatrixCellAlignment switch
        {
            MatrixCellAlignment.Aligned => (columnIndex % 2) switch
            {
                0 when columnIndex != 0 => new SurroundingGap(AlignGroupLeftPadding + lrPadding + hFreeSpace, lrPadding),
                0 => new SurroundingGap(lrPadding + hFreeSpace, lrPadding),
                1 => new SurroundingGap(lrPadding, lrPadding + hFreeSpace),
                _ => throw new ArgumentOutOfRangeException()
            },
            MatrixCellAlignment.Left => new SurroundingGap(lrPadding, lrPadding + hFreeSpace),
            MatrixCellAlignment.Center => new SurroundingGap(lrPadding + hFreeSpace / 2, lrPadding + hFreeSpace / 2),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static IReadOnlyCollection<T> ToImmutableCollection<T>(IEnumerable<T> s) => s.ToList().AsReadOnly();
}
