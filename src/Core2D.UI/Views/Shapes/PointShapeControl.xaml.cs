﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Core2D.UI.Views.Shapes
{
    /// <summary>
    /// Interaction logic for <see cref="PointShapeControl"/> xaml.
    /// </summary>
    public class PointShapeControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointShapeControl"/> class.
        /// </summary>
        public PointShapeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
