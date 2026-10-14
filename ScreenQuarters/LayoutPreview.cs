using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenQuarters;

internal sealed class LayoutPreview : Control
{
    private LayoutPreset preset = LayoutPreset.Grid2x2;
    private int[]? customRows;
    private int screenWidth = 3840;
    private int screenHeight = 2160;

    public LayoutPreview()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(30, 32, 38);
        ForeColor = Color.White;
    }

    public void SetLayout(LayoutPreset selected, int width, int height, int[]? rows = null)
    {
        preset = selected;
        customRows = LayoutPresets.NormalizeRows(rows);
        screenWidth = Math.Max(1, width);
        screenHeight = Math.Max(1, height);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        int padding = Math.Max(8, DeviceDpi * 12 / 96);
        int availableWidth = ClientSize.Width - padding * 2;
        int availableHeight = ClientSize.Height - padding * 2;

        if (availableWidth <= 0 || availableHeight <= 0)
            return;

        double scale = Math.Min(
            (double)availableWidth / screenWidth,
            (double)availableHeight / screenHeight);

        int drawWidth = Math.Max(1, (int)Math.Round(screenWidth * scale));
        int drawHeight = Math.Max(1, (int)Math.Round(screenHeight * scale));

        int offsetX = (ClientSize.Width - drawWidth) / 2;
        int offsetY = (ClientSize.Height - drawHeight) / 2;

        var area = new Native.Rect
        {
            Left = 0,
            Top = 0,
            Right = screenWidth,
            Bottom = screenHeight
        };

        var zones = LayoutPresets.GetZones(area, preset, customRows);

        using var fill = new SolidBrush(Color.FromArgb(48, 65, 90));
        using var border = new Pen(Color.FromArgb(130, 170, 225));
        using var font = new Font(
            Font.FontFamily, 16, FontStyle.Bold);

        for (int index = 0; index < zones.Length; index++)
        {
            var zone = zones[index];

            int left = offsetX + (int)Math.Round(zone.Left * scale);
            int top = offsetY + (int)Math.Round(zone.Top * scale);
            int right = offsetX + (int)Math.Round(zone.Right * scale);
            int bottom = offsetY + (int)Math.Round(zone.Bottom * scale);

            var cell = Rectangle.FromLTRB(left, top, right, bottom);

            e.Graphics.FillRectangle(fill, cell);
            e.Graphics.DrawRectangle(
                border,
                cell.Left,
                cell.Top,
                Math.Max(0, cell.Width - 1),
                Math.Max(0, cell.Height - 1));

            TextRenderer.DrawText(
                e.Graphics,
                (index + 1).ToString(),
                font,
                cell,
                ForeColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPadding);
        }
    }
}