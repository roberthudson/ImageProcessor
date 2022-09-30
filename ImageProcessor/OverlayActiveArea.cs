public class OverlayActiveArea
{
    public int Top { get; }
    public int Bottom { get; }
    public int Left { get; }
    public int Right { get; }
    public OverlayActiveArea(int top, int bottom, int left, int right)
    {
        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
    }
}