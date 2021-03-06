﻿using System;
using Core2D.Renderer;
using Core2D.Shapes;
using Core2D.Style;
using Spatial;
using A = Avalonia;
using AM = Avalonia.Media;
using AME = Avalonia.MatrixExtensions;

namespace Core2D.UI.Renderer
{
    internal abstract class Marker : IMarker
    {
        public IArrowStyle Style { get; set; }
        public AM.IBrush Brush { get; set; }
        public AM.IPen Pen { get; set; }
        public A.Matrix Rotation { get; set; }
        public A.Point Point { get; set; }

        public abstract void Draw(object dc);

        public virtual void UpdateStyle()
        {
            Brush = AvaloniaDrawUtil.ToBrush(Style.Fill);
            Pen = AvaloniaDrawUtil.ToPen(Style, Style.Thickness);
        }
    }

    internal class NoneMarker : Marker
    {
        public override void Draw(object dc)
        {
        }
    }

    internal class RectangleMarker : Marker
    {
        public A.Rect Rect { get; set; }

        public override void Draw(object dc)
        {
            var context = dc as AM.DrawingContext;

            using var rotationDisposable = context.PushPreTransform(Rotation);

            if (Style.IsFilled)
            {
                context.FillRectangle(Brush, Rect);
            }

            if (Style.IsStroked)
            {
                context.DrawRectangle(Pen, Rect);
            }
        }
    }

    internal class EllipseMarker : Marker
    {
        public AM.EllipseGeometry EllipseGeometry { get; set; }

        public override void Draw(object dc)
        {
            var context = dc as AM.DrawingContext;

            using var rotationDisposable = context.PushPreTransform(Rotation);

            context.DrawGeometry(Style.IsFilled ? Brush : null, Style.IsStroked ? Pen : null, EllipseGeometry);
        }
    }

    internal class ArrowMarker : Marker
    {
        public A.Point P11;
        public A.Point P21;
        public A.Point P12;
        public A.Point P22;

        public override void Draw(object dc)
        {
            var context = dc as AM.DrawingContext;

            if (Style.IsStroked)
            {
                context.DrawLine(Pen, P11, P21);
                context.DrawLine(Pen, P12, P22);
            }
        }
    }

    internal class LineDrawNode : DrawNode, ILineDrawNode
    {
        public ILineShape Line { get; set; }
        public A.Point P0 { get; set; }
        public A.Point P1 { get; set; }
        public IMarker StartMarker { get; set; }
        public IMarker EndMarker { get; set; }
        public AM.StreamGeometry CurveGeometry { get; set; }

        public LineDrawNode(ILineShape line, IShapeStyle style)
        {
            Style = style;
            Line = line;
            UpdateGeometry();
        }

        private Marker CreatArrowMarker(double x, double y, double angle, IArrowStyle style)
        {
            switch (style.ArrowType)
            {
                default:
                case ArrowType.None:
                    {
                        var marker = new NoneMarker();

                        marker.Style = style;
                        marker.Point = new A.Point(x, y);

                        return marker;
                    }
                case ArrowType.Rectangle:
                    {
                        double rx = style.RadiusX;
                        double ry = style.RadiusY;
                        double sx = 2.0 * rx;
                        double sy = 2.0 * ry;

                        var marker = new RectangleMarker();

                        marker.Style = style;
                        marker.Rotation = AME.MatrixHelper.Rotation(angle, new A.Vector(x, y));
                        marker.Point = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x - sx, y));

                        var rect2 = new Rect2(x - sx, y - ry, sx, sy);
                        marker.Rect = new A.Rect(rect2.X, rect2.Y, rect2.Width, rect2.Height);

                        return marker;
                    }
                case ArrowType.Ellipse:
                    {
                        double rx = style.RadiusX;
                        double ry = style.RadiusY;
                        double sx = 2.0 * rx;
                        double sy = 2.0 * ry;

                        var marker = new EllipseMarker();

                        marker.Style = style;
                        marker.Rotation = AME.MatrixHelper.Rotation(angle, new A.Vector(x, y));
                        marker.Point = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x - sx, y));

                        var rect2 = new Rect2(x - sx, y - ry, sx, sy);
                        var rect = new A.Rect(rect2.X, rect2.Y, rect2.Width, rect2.Height);
                        marker.EllipseGeometry = new AM.EllipseGeometry(rect);

                        return marker;
                    }
                case ArrowType.Arrow:
                    {
                        double rx = style.RadiusX;
                        double ry = style.RadiusY;
                        double sx = 2.0 * rx;
                        double sy = 2.0 * ry;

                        var marker = new ArrowMarker();

                        marker.Style = style;
                        marker.Rotation = AME.MatrixHelper.Rotation(angle, new A.Vector(x, y));
                        marker.Point = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x, y));

                        marker.P11 = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x - sx, y + sy));
                        marker.P21 = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x, y));
                        marker.P12 = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x - sx, y - sy));
                        marker.P22 = AME.MatrixHelper.TransformPoint(marker.Rotation, new A.Point(x, y));

                        return marker;
                    }
            }
        }

        private AM.StreamGeometry CreateCurveGeometry(A.Point p0, A.Point p1, double curvature, CurveOrientation orientation, PointAlignment pt0a, PointAlignment pt1a)
        {
            var curveGeometry = new AM.StreamGeometry();

            using (var geometryContext = curveGeometry.Open())
            {
                geometryContext.BeginFigure(new A.Point(p0.X, p0.Y), false);
                double p0x = p0.X;
                double p0y = p0.Y;
                double p1x = p1.X;
                double p1y = p1.Y;
                LineShapeExtensions.GetCurvedLineBezierControlPoints(orientation, curvature, pt0a, pt1a, ref p0x, ref p0y, ref p1x, ref p1y);
                var point1 = new A.Point(p0x, p0y);
                var point2 = new A.Point(p1x, p1y);
                var point3 = new A.Point(p1.X, p1.Y);
                geometryContext.CubicBezierTo(point1, point2, point3);
                geometryContext.EndFigure(false);
            }

            return curveGeometry;
        }

        private void UpdateMarkers()
        {
            double x1 = Line.Start.X;
            double y1 = Line.Start.Y;
            double x2 = Line.End.X;
            double y2 = Line.End.Y;
            Line.GetMaxLength(ref x1, ref y1, ref x2, ref y2);

            if (Style.StartArrowStyle.ArrowType != ArrowType.None)
            {
                double a1 = Math.Atan2(y1 - y2, x1 - x2);
                StartMarker = CreatArrowMarker(x1, y1, a1, Style.StartArrowStyle);
                StartMarker.UpdateStyle();
                P0 = (StartMarker as Marker).Point;
            }
            else
            {
                StartMarker = null;
                P0 = new A.Point(x1, y1);
            }

            if (Style.EndArrowStyle.ArrowType != ArrowType.None)
            {
                double a2 = Math.Atan2(y2 - y1, x2 - x1);
                EndMarker = CreatArrowMarker(x2, y2, a2, Style.EndArrowStyle);
                EndMarker.UpdateStyle();
                P1 = (EndMarker as Marker).Point;
            }
            else
            {
                EndMarker = null;
                P1 = new A.Point(x2, y2);
            }
        }

        private void UpdateCurveGeometry()
        {
            if (Style.LineStyle.IsCurved)
            {
                CurveGeometry = CreateCurveGeometry(P0, P1, Style.LineStyle.Curvature, Style.LineStyle.CurveOrientation, Line.Start.Alignment, Line.End.Alignment);
            }
            else
            {
                CurveGeometry = null;
            }
        }

        public override void UpdateGeometry()
        {
            ScaleThickness = Line.State.Flags.HasFlag(ShapeStateFlags.Thickness);
            ScaleSize = Line.State.Flags.HasFlag(ShapeStateFlags.Size);
            P0 = new A.Point(Line.Start.X, Line.Start.Y);
            P1 = new A.Point(Line.End.X, Line.End.Y);
            Center = new A.Point((P0.X + P1.X) / 2.0, (P0.Y + P1.Y) / 2.0);
            UpdateMarkers();
            UpdateCurveGeometry();
        }

        public override void UpdateStyle()
        {
            base.UpdateStyle();

            if (Style.StartArrowStyle.ArrowType != ArrowType.None)
            {
                StartMarker?.UpdateStyle();
            }

            if (Style.EndArrowStyle.ArrowType != ArrowType.None)
            {
                EndMarker?.UpdateStyle();
            }
        }

        public override void OnDraw(object dc, double zoom)
        {
            var context = dc as AM.DrawingContext;

            if (Line.IsStroked)
            {
                if (Style.LineStyle.IsCurved)
                {
                    context.DrawGeometry(null, Stroke, CurveGeometry);
                }
                else
                {
                    context.DrawLine(Stroke, P0, P1);
                }

                if (Style.StartArrowStyle.ArrowType != ArrowType.None)
                {
                    StartMarker?.Draw(dc);
                }

                if (Style.EndArrowStyle.ArrowType != ArrowType.None)
                {
                    EndMarker?.Draw(dc);
                }
            }
        }
    }
}
