﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Core2D.Renderer;
using Core2D.Style;

namespace Core2D.Shapes
{
    /// <summary>
    /// Text shape.
    /// </summary>
    public class TextShape : BaseShape, ITextShape
    {
        private IPointShape _topLeft;
        private IPointShape _bottomRight;
        private string _text;

        /// <inheritdoc/>
        public override Type TargetType => typeof(ITextShape);

        /// <inheritdoc/>
        public IPointShape TopLeft
        {
            get => _topLeft;
            set => Update(ref _topLeft, value);
        }

        /// <inheritdoc/>
        public IPointShape BottomRight
        {
            get => _bottomRight;
            set => Update(ref _bottomRight, value);
        }

        /// <inheritdoc/>
        public string Text
        {
            get => _text;
            set => Update(ref _text, value);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, IShapeRenderer renderer, double dx, double dy, object db, object r)
        {
            var state = base.BeginTransform(dc, renderer);

            var record = Data?.Record ?? r;

            if (State.Flags.HasFlag(ShapeStateFlags.Visible))
            {
                renderer.Draw(dc, this, dx, dy, db, record);
            }

            if (renderer.State.SelectedShape != null)
            {
                if (this == renderer.State.SelectedShape)
                {
                    _topLeft.Draw(dc, renderer, dx, dy, db, record);
                    _bottomRight.Draw(dc, renderer, dx, dy, db, record);
                }
                else if (_topLeft == renderer.State.SelectedShape)
                {
                    _topLeft.Draw(dc, renderer, dx, dy, db, record);
                }
                else if (_bottomRight == renderer.State.SelectedShape)
                {
                    _bottomRight.Draw(dc, renderer, dx, dy, db, record);
                }
            }

            if (renderer.State.SelectedShapes != null)
            {
                if (renderer.State.SelectedShapes.Contains(this))
                {
                    _topLeft.Draw(dc, renderer, dx, dy, db, record);
                    _bottomRight.Draw(dc, renderer, dx, dy, db, record);
                }
            }

            base.EndTransform(dc, renderer, state);
        }

        /// <inheritdoc/>
        public override void Move(ISet<IBaseShape> selected, double dx, double dy)
        {
            if (!TopLeft.State.Flags.HasFlag(ShapeStateFlags.Connector))
            {
                TopLeft.Move(selected, dx, dy);
            }

            if (!BottomRight.State.Flags.HasFlag(ShapeStateFlags.Connector))
            {
                BottomRight.Move(selected, dx, dy);
            }
        }

        /// <inheritdoc/>
        public override void Select(ISet<IBaseShape> selected)
        {
            base.Select(selected);
            TopLeft.Select(selected);
            BottomRight.Select(selected);
        }

        /// <inheritdoc/>
        public override void Deselect(ISet<IBaseShape> selected)
        {
            base.Deselect(selected);
            TopLeft.Deselect(selected);
            BottomRight.Deselect(selected);
        }

        /// <inheritdoc/>
        public override IEnumerable<IPointShape> GetPoints()
        {
            yield return TopLeft;
            yield return BottomRight;
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object> shared)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check whether the <see cref="TopLeft"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeTopLeft() => _topLeft != null;

        /// <summary>
        /// Check whether the <see cref="BottomRight"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeBottomRight() => _bottomRight != null;

        /// <summary>
        /// Check whether the <see cref="Text"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeText() => !string.IsNullOrWhiteSpace(_text);
    }
}
