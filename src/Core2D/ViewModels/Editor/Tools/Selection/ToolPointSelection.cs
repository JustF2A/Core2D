﻿using Core2D.Containers;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Editor.Tools.Selection
{
    /// <summary>
    /// Helper class for <see cref="IPointShape"/> shape selection.
    /// </summary>
    public class ToolPointSelection
    {
        private readonly ILayerContainer _layer;
        private readonly IPointShape _shape;
        private readonly IShapeStyle _style;

        /// <summary>
        /// Initialize new instance of <see cref="ToolPointSelection"/> class.
        /// </summary>
        /// <param name="layer">The selection shapes layer.</param>
        /// <param name="shape">The selected shape.</param>
        /// <param name="style">The selection shapes style.</param>
        public ToolPointSelection(ILayerContainer layer, IPointShape shape, IShapeStyle style)
        {
            _layer = layer;
            _shape = shape;
            _style = style;
        }
    }
}
