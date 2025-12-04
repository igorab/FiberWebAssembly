using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

[Route("api/skia")]
public class SkiaController : Controller
{
    [HttpGet("chart")]
    public IActionResult Chart()
    {
        int width = 800, height = 400;
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        // Пример: простой линейный график
        var paintAxis = new SKPaint { Color = SKColors.Black, StrokeWidth = 2 };
        canvas.DrawLine(40, height - 40, width - 20, height - 40, paintAxis); // X
        canvas.DrawLine(40, height - 40, 40, 20, paintAxis); // Y

        float[] data = { 10, 40, 25, 60, 80, 55 };
        float max = 100f;
        float stepX = (width - 80) / (data.Length - 1);
        var paintLine = new SKPaint { Color = SKColors.SeaGreen, StrokeWidth = 3, IsStroke = true, IsAntialias = true };

        for (int i = 0; i < data.Length - 1; i++)
        {
            float x1 = 40 + i * stepX;
            float y1 = height - 40 - (data[i] / max) * (height - 80);
            float x2 = 40 + (i + 1) * stepX;
            float y2 = height - 40 - (data[i + 1] / max) * (height - 80);
            canvas.DrawLine(x1, y1, x2, y2, paintLine);
            canvas.DrawCircle(x1, y1, 4, paintLine);
        }

        // Последняя точка
        canvas.DrawCircle(40 + (data.Length - 1) * stepX, height - 40 - (data[^1] / max) * (height - 80), 4, paintLine);

        // Сериализация в PNG
        using var image = SKImage.FromBitmap(bitmap);
        using var dataPng = image.Encode(SKEncodedImageFormat.Png, 90);
        return File(dataPng.ToArray(), "image/png");
    }
}
