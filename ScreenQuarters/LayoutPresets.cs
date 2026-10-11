using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenQuarters;

internal enum LayoutPreset
{
    Grid2x2,
    Grid3x3,
    TwoColumns,
    ThreeColumns,
    WideLeftSplitRight
}

internal static class LayoutPresets
{
    public static string GetName(LayoutPreset preset) => preset switch
    {
        LayoutPreset.Grid2x2 => "2 × 2 — Dört bölge",
        LayoutPreset.Grid3x3 => "3 × 3 — Dokuz bölge",
        LayoutPreset.TwoColumns => "İki sütun",
        LayoutPreset.ThreeColumns => "Üç sütun",
        LayoutPreset.WideLeftSplitRight => "Sol geniş — Sağ üst / alt",
        _ => throw new ArgumentOutOfRangeException(nameof(preset))
    };

    public static Native.Rect[] GetZones(
        Native.Rect work,
        LayoutPreset preset)
    {
        return preset switch
        {
            LayoutPreset.Grid2x2 => Grid(work, 2, 2),
            LayoutPreset.Grid3x3 => Grid(work, 3, 3),
            LayoutPreset.TwoColumns => Grid(work, 2, 1),
            LayoutPreset.ThreeColumns => Grid(work, 3, 1),
            LayoutPreset.WideLeftSplitRight => WideLeft(work),
            _ => throw new ArgumentOutOfRangeException(nameof(preset))
        };
    }

    private static Native.Rect[] Grid(
        Native.Rect work,
        int columns,
        int rows)
    {
        var zones = new Native.Rect[columns * rows];

        int width = work.Right - work.Left;
        int height = work.Bottom - work.Top;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                zones[row * columns + column] = new Native.Rect
                {
                    Left = work.Left + width * column / columns,
                    Right = work.Left + width * (column + 1) / columns,
                    Top = work.Top + height * row / rows,
                    Bottom = work.Top + height * (row + 1) / rows
                };
            }
        }

        return zones;
    }

    private static Native.Rect[] WideLeft(Native.Rect work)
    {
        int splitX = work.Left + (work.Right - work.Left) * 2 / 3;
        int splitY = work.Top + (work.Bottom - work.Top) / 2;

        return new[]
        {
            new Native.Rect
            {
                Left = work.Left,
                Top = work.Top,
                Right = splitX,
                Bottom = work.Bottom
            },
            new Native.Rect
            {
                Left = splitX,
                Top = work.Top,
                Right = work.Right,
                Bottom = splitY
            },
            new Native.Rect
            {
                Left = splitX,
                Top = splitY,
                Right = work.Right,
                Bottom = work.Bottom
            }
        };
    }
}