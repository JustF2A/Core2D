﻿using System;
using System.Collections.Generic;
using Core2D.Renderer;
using Core2D.Shapes;
using Spatial;

namespace Core2D.Editor.Bounds.Shapes
{
    public class BoundsText : IBounds
    {
        public Type TargetType => typeof(ITextShape);

        public IPointShape TryToGetPoint(IBaseShape shape, Point2 target, double radius, double scale, IDictionary<Type, IBounds> registered)
        {
            if (!(shape is ITextShape text))
            {
                throw new ArgumentNullException(nameof(shape));
            }

            var pointHitTest = registered[typeof(IPointShape)];

            if (pointHitTest.TryToGetPoint(text.TopLeft, target, radius, scale, registered) != null)
            {
                return text.TopLeft;
            }

            if (pointHitTest.TryToGetPoint(text.BottomRight, target, radius, scale, registered) != null)
            {
                return text.BottomRight;
            }

            return null;
        }

        public bool Contains(IBaseShape shape, Point2 target, double radius, double scale, IDictionary<Type, IBounds> registered)
        {
            if (!(shape is ITextShape text))
            {
                throw new ArgumentNullException(nameof(shape));
            }

            var rect = Rect2.FromPoints(
                text.TopLeft.X,
                text.TopLeft.Y,
                text.BottomRight.X,
                text.BottomRight.Y);

            if (text.State.Flags.HasFlag(ShapeStateFlags.Size) && scale != 1.0)
            {
                return HitTestHelper.Inflate(ref rect, scale).Contains(target);
            }
            else
            {
                return rect.Contains(target);
            }
        }

        public bool Overlaps(IBaseShape shape, Rect2 target, double radius, double scale, IDictionary<Type, IBounds> registered)
        {
            if (!(shape is ITextShape text))
            {
                throw new ArgumentNullException(nameof(shape));
            }

            var rect = Rect2.FromPoints(
                text.TopLeft.X,
                text.TopLeft.Y,
                text.BottomRight.X,
                text.BottomRight.Y);

            if (text.State.Flags.HasFlag(ShapeStateFlags.Size) && scale != 1.0)
            {
                return HitTestHelper.Inflate(ref rect, scale).IntersectsWith(target);
            }
            else
            {
                return rect.IntersectsWith(target);
            }
        }
    }
}
