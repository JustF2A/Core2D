﻿using Core2D.Renderer;
using Core2D.Shapes;
using Core2D.Style;
using AM = Avalonia.Media;

namespace Core2D.UI.Renderer
{
    internal class CubicBezierDrawNode : DrawNode, ICubicBezierDrawNode
    {
        public ICubicBezierShape CubicBezier { get; set; }
        public AM.Geometry Geometry { get; set; }

        public CubicBezierDrawNode(ICubicBezierShape cubicBezier, IShapeStyle style)
        {
            Style = style;
            CubicBezier = cubicBezier;
            UpdateGeometry();
        }

        public override void UpdateGeometry()
        {
            ScaleThickness = CubicBezier.State.Flags.HasFlag(ShapeStateFlags.Thickness);
            ScaleSize = CubicBezier.State.Flags.HasFlag(ShapeStateFlags.Size);
            Geometry = PathGeometryConverter.ToGeometry(CubicBezier);
            Center = Geometry.Bounds.Center;
        }

        public override void OnDraw(object dc, double zoom)
        {
            var context = dc as AM.DrawingContext;

            context.DrawGeometry(CubicBezier.IsFilled ? Fill : null, CubicBezier.IsStroked ? Stroke : null, Geometry);
        }
    }
}
