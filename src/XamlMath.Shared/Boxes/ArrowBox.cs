using System;
using XamlMath.Rendering;

namespace XamlMath.Boxes;

// Box that draws a single horizontal arrow (a shaft plus one arrowhead) spanning its full width.
// Used to build the stretchy accents \overrightarrow and \overleftarrow.
internal sealed class ArrowBox : Box
{
    private readonly bool _pointsRight;
    private readonly double _headHalfHeight;
    private readonly double _headLength;

    public ArrowBox(TexEnvironment environment, double width, double thickness, bool pointsRight)
    {
        _pointsRight = pointsRight;
        _headHalfHeight = 2.0 * thickness;
        _headLength = 5.0 * thickness;

        this.Width = width;
        this.Height = 2.0 * _headHalfHeight; // full vertical extent, shaft drawn down the middle
        this.Depth = 0;
        this.Foreground = environment.Foreground;
        this.Background = environment.Background;
    }

    public override void RenderTo(IElementRenderer renderer, double x, double y)
    {
        // The shaft runs along the vertical middle of the box; y is the box baseline (its lower edge).
        var shaftY = y - this.Height / 2;
        var left = x;
        var right = x + this.Width;

        renderer.RenderLine(new Point(left, shaftY), new Point(right, shaftY), this.Foreground);

        // Arrowhead: two short strokes converging on the pointing end.
        var headLength = Math.Min(_headLength, this.Width);
        if (_pointsRight)
        {
            renderer.RenderLine(new Point(right, shaftY), new Point(right - headLength, shaftY - _headHalfHeight), this.Foreground);
            renderer.RenderLine(new Point(right, shaftY), new Point(right - headLength, shaftY + _headHalfHeight), this.Foreground);
        }
        else
        {
            renderer.RenderLine(new Point(left, shaftY), new Point(left + headLength, shaftY - _headHalfHeight), this.Foreground);
            renderer.RenderLine(new Point(left, shaftY), new Point(left + headLength, shaftY + _headHalfHeight), this.Foreground);
        }
    }

    public override int GetLastFontId()
    {
        return TexFontUtilities.NoFontId;
    }
}
