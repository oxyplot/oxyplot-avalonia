using Avalonia.Headless.XUnit;
using OxyPlot;
using OxyPlot.Avalonia;
using CoreSeries = OxyPlot.Series;

namespace OxyPlot.Avalonia.Tests;

/// <summary>
/// Tests of the plot export to PNG.
/// </summary>
public class ExportTests
{
    [AvaloniaFact]
    public void PngExporter_Export_WritesPngOfRequestedSize()
    {
        var model = new PlotModel();
        model.Series.Add(new CoreSeries.FunctionSeries(Math.Sin, 0, 10, 0.1));
        using var stream = new MemoryStream();

        PngExporter.Export(model, stream, 200, 100, OxyColors.White);

        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, bytes.Take(8));

        // the width and the height are in the IHDR header, big-endian
        Assert.Equal(200, ReadBigEndian(bytes, 16));
        Assert.Equal(100, ReadBigEndian(bytes, 20));
    }

    [AvaloniaFact]
    public void PlotView_SaveBitmapOfOtherSize_RestoresScreenLayout()
    {
        var model = new PlotModel();
        model.Series.Add(new CoreSeries.FunctionSeries(Math.Sin, 0, 10, 0.1));
        var view = new PlotView { Model = model };
        var window = Ui.Show(view);
        var plotArea = model.PlotArea;
        using var stream = new MemoryStream();

        view.SaveBitmap(stream, 800, 600, OxyColors.White);
        Ui.Flush();

        // the export recalculates the live model for its own size, and the control restores it to its own
        Assert.True(stream.Length > 0);
        Assert.Equal(plotArea, model.PlotArea);
        window.Close();
    }

    [AvaloniaFact]
    public void PlotView_SaveBitmapWithoutModel_ThrowsInvalidOperation()
    {
        var view = new PlotView();
        using var stream = new MemoryStream();

        Assert.Throws<InvalidOperationException>(() => view.SaveBitmap(stream));
        Assert.Throws<InvalidOperationException>(() => view.ToBitmap());
    }

    [AvaloniaFact]
    public void PlotView_ToBitmapBeforeLayout_ThrowsInvalidOperation()
    {
        var view = new PlotView { Model = new PlotModel() };

        // the control has not been laid out yet, its size is zero
        Assert.Throws<InvalidOperationException>(() => view.ToBitmap());
    }

    private static int ReadBigEndian(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }
}
