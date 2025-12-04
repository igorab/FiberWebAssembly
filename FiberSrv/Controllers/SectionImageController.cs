using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace FiberSrv.Controllers;

[Route("api/[controller]")]
public class SectionImageController : Controller
{
    // DTO
    record BeamSection(int Id, string Name, double h = 0, double b = 0, double bw = 0, double hw = 0, double bf = 0, double hf = 0, double b1f = 0, double h1f = 0, double r1 = 0, double r2 = 0);
    record Rod(int Id, double X, double Y, double D, int SectionType, string Dnom);

    // Примеры данных — замените чтением из БД
    static readonly BeamSection[] sections = new[] {
        new BeamSection(1,"Тавровое", h:20, bw:40, bf:40, hf:12),
        new BeamSection(2,"Двутавровое", h:20, bw:40, hw:60, bf:20, hf:40, b1f:12),
        new BeamSection(3,"Кольцевое", r1:25, r2:40),
        new BeamSection(4,"Прямоугольное", h:30, b:60),
        new BeamSection(5,"Тавр нижняя полка", h:20, bw:40, hw:60, bf:20)
    };

    static readonly Rod[] rods = new[] {
        new Rod(25,-10,5,1.6,4,"16"), new Rod(26,10,5,1.6,4,"16"), new Rod(27,-10,55,1.6,4,"16"),
        new Rod(28,10,55,1.6,4,"16"), new Rod(29,5,5,1.2,0,"12"), new Rod(30,15,5,1.2,0,"12"),
        new Rod(31,-8,5,1.2,1,"12"), new Rod(32,0,5,1.2,1,"12"), new Rod(33,8,5,1.2,1,"12"),
        new Rod(34,-10,5,1.4,2,"14"), new Rod(35,0,5,1.4,2,"14"), new Rod(36,10,5,1.4,2,"14"),
        new Rod(37,-10,5,1.2,5,"12"), new Rod(38,0,5,1.2,5,"12"), new Rod(39,10,5,1.2,5,"12"),
        new Rod(40,0,-36,1.6,3,"16")
    };

    BeamSection GetSection(int id) => Array.Find(sections, s => s.Id == id);
    Rod[] GetRodsForSection(int id) => Array.FindAll(rods, r => r.SectionType == id);

    [HttpGet("{sectionId}")]
    public IActionResult GetSectionPng(int sectionId, int width = 1200, int height = 800)
    {
        var sec = GetSection(sectionId);

        if (sec == null) return NotFound();

        var sectionRods = GetRodsForSection(sectionId);

        using var bmp = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.White);

        // Параметры отрисовки (см. предыдущие ответы — та же логика)
        float margin = 20;
        float w = width;
        float h = height;

        // Построение path сечения (мм). Центр 0,0; Y вверх.
        var path = new SKPath();
        if (sec.Id == 4)
        { // прямоугольник
            path.AddRect(new SKRect(-(float)sec.b / 2, (float)sec.h / 2, (float)sec.b / 2, -(float)sec.h / 2));
        }
        else if (sec.Id == 1)
        { // Тавр (фланец сверху)
            float hh = (float)sec.h, bf = (float)sec.bf, hf = (float)sec.hf, bw = (float)sec.bw;
            if (bf == 0) bf = 40; if (hf == 0) hf = 12; if (bw == 0) bw = 40;
            path.AddRect(new SKRect(-bf / 2, hh / 2, bf / 2, hh / 2 - hf));
            path.AddRect(new SKRect(-bw / 2, hh / 2 - hf, bw / 2, -hh / 2));
        }
        else if (sec.Id == 5)
        { // Тавр нижняя полка
            float hh = (float)sec.h, bf = (float)sec.bf, hf = (float)sec.hf, bw = (float)sec.bw;
            if (bf == 0) bf = 40; if (hf == 0) hf = 12; if (bw == 0) bw = 40;
            path.AddRect(new SKRect(-bw / 2, hh / 2, bw / 2, -hh / 2 + hf));
            path.AddRect(new SKRect(-bf / 2, -hh / 2 + hf, bf / 2, -hh / 2));
        }
        else if (sec.Id == 2)
        { // двутавр
            float totalH = (float)sec.h, bf = (float)sec.bf, hf = (float)sec.hf, bw = (float)sec.bw;
            if (totalH == 0) totalH = 60; if (bf == 0) bf = 40; if (hf == 0) hf = 12; if (bw == 0) bw = 20;
            path.AddRect(new SKRect(-bf / 2, totalH / 2, bf / 2, totalH / 2 - hf));
            path.AddRect(new SKRect(-bf / 2, -totalH / 2 + hf, bf / 2, -totalH / 2));
            path.AddRect(new SKRect(-bw / 2, totalH / 2 - hf, bw / 2, -totalH / 2 + hf));
        }
        else if (sec.Id == 3)
        { // кольцо
            float r1 = (float)sec.r1, r2 = (float)sec.r2;
            if (r1 == 0) r1 = 25; if (r2 == 0) r2 = 40;
            path.AddCircle(0, 0, r2);
            var inner = new SKPath(); inner.AddCircle(0, 0, r1); 
            inner.Rewind();
            path.AddPath(inner);
            path.FillType = SKPathFillType.EvenOdd;
        }

        var bounds = path.Bounds;
        float secMinX = bounds.Left, secMaxX = bounds.Right, secMinY = bounds.Bottom, secMaxY = bounds.Top;
        foreach (var r in sectionRods)
        {
            if (r.X < secMinX) secMinX = (float)r.X;
            if (r.X > secMaxX) secMaxX = (float)r.X;
            if (r.Y < secMinY) secMinY = (float)r.Y;
            if (r.Y > secMaxY) secMaxY = (float)r.Y;
        }
        float secW = secMaxX - secMinX; if (secW < 1) secW = 1;
        float secH = secMaxY - secMinY; if (secH < 1) secH = 1;

        float scaleX = (w - 2 * margin) / secW;
        float scaleY = (h - 2 * margin) / secH;
        float scale = Math.Min(scaleX, scaleY);

        float cx = w / 2, cy = h / 2;
        Func<float, float> TX = x => cx + (x - (secMinX + secMaxX) / 2f) * scale;
        Func<float, float> TY = y => cy - (y - (secMinY + secMaxY) / 2f) * scale;

        var fillPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.LightGray, IsAntialias = true };
        var strokePaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
        var rodFill = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.DarkRed, IsAntialias = true };
        var rodStroke = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = 1, IsAntialias = true };
        var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };

        // преобразование и отрисовка
        var mTrans = SKMatrix.CreateTranslation(-(secMinX + secMaxX) / 2f, -(secMinY + secMaxY) / 2f);
        var mScale = SKMatrix.CreateScale(scale, -scale);
        SKMatrix.Concat(mScale, mTrans); // mScale = mScale * mTrans
        // добавим перенос в пиксели
        mScale.TransX += cx;
        mScale.TransY += cy;

        SKPath transformed = new SKPath();

        path.Transform(mScale, transformed );
        canvas.DrawPath(transformed, fillPaint);
        canvas.DrawPath(transformed, strokePaint);

        // стержни
        foreach (var r in sectionRods)
        {
            float px = TX((float)r.X);
            float py = TY((float)r.Y);
            float rr = (float)r.D / 2f * scale;
            if (rr < 3) rr = 3;
            canvas.DrawCircle(px, py, rr, rodFill);
            canvas.DrawCircle(px, py, rr, rodStroke);
            canvas.DrawText(r.Dnom, px + rr + 4, py - 4, textPaint);
        }

        // Экспорт в PNG
        using var image = SKImage.FromBitmap(bmp);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return File(data.ToArray(), "image/png");
    }
}
