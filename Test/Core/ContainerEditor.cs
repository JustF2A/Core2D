﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Test.Core
{
    public class ContainerEditor : XObject
    {
        public ICommand NewCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public ICommand ClearCommand { get; set; }

        public ICommand ToolNoneCommand { get; set; }
        public ICommand ToolLineCommand { get; set; }
        public ICommand ToolRectangleCommand { get; set; }
        public ICommand ToolEllipseCommand { get; set; }
        public ICommand ToolBezierCommand { get; set; }
        public ICommand ToolQBezierCommand { get; set; }
        public ICommand ToolTextCommand { get; set; }

        public ICommand DefaultIsFilledCommand { get; set; }
        public ICommand SnapToGridCommand { get; set; }
        public ICommand DrawPointsCommand { get; set; }

        private IContainer _container;
        private IRenderer _renderer;
        private XShape _shape;
        private Tool _currentTool;
        private State _currentState;
        private bool _defaultIsFilled;
        private bool _snapToGrid;
        private double _snapX;
        private double _snapY;

        public IContainer Container
        {
            get { return _container; }
            set
            {
                if (value != _container)
                {
                    _container = value;
                    Notify("Container");
                }
            }
        }

        public IRenderer Renderer
        {
            get { return _renderer; }
            set
            {
                if (value != _renderer)
                {
                    _renderer = value;
                    Notify("Renderer");
                }
            }
        }

        public Tool CurrentTool
        {
            get { return _currentTool; }
            set
            {
                if (value != _currentTool)
                {
                    _currentTool = value;
                    Notify("CurrentTool");
                }
            }
        }

        public State CurrentState
        {
            get { return _currentState; }
            set
            {
                if (value != _currentState)
                {
                    _currentState = value;
                    Notify("CurrentState");
                }
            }
        }

        public bool DefaultIsFilled
        {
            get { return _defaultIsFilled; }
            set
            {
                if (value != _defaultIsFilled)
                {
                    _defaultIsFilled = value;
                    Notify("DefaultIsFilled");
                }
            }
        }

        public bool SnapToGrid
        {
            get { return _snapToGrid; }
            set
            {
                if (value != _snapToGrid)
                {
                    _snapToGrid = value;
                    Notify("SnapToGrid");
                }
            }
        }

        public double SnapX
        {
            get { return _snapX; }
            set
            {
                if (value != _snapX)
                {
                    _snapX = value;
                    Notify("SnapX");
                }
            }
        }

        public double SnapY
        {
            get { return _snapY; }
            set
            {
                if (value != _snapY)
                {
                    _snapY = value;
                    Notify("SnapY");
                }
            }
        }

        public static ContainerEditor Create(
            IContainer container, 
            IRenderer renderer)
        {
            return new ContainerEditor()
            {
                Container = container,
                Renderer = renderer,
                SnapToGrid = false,
                SnapX = 15.0,
                SnapY = 15.0,
                DefaultIsFilled = false,
                CurrentTool = Tool.Line,
                CurrentState = State.None
            };
        }

        public double Snap(double value, double snap)
        {
            double r = value % snap;
            return r >= snap / 2.0 ? value + snap - r : value - r;
        }

        public void Left(double x, double y)
        {
            double sx = SnapToGrid ? Snap(x, SnapX) : x;
            double sy = SnapToGrid ? Snap(y, SnapY) : y;
            switch (CurrentTool)
            {
                // None
                case Tool.None:
                    {
                    }
                    break;
                // Line
                case Tool.Line:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                    _shape = XLine.Create(
                                        sx, sy, 
                                        _container.CurrentStyle,
                                        _container.PointShape);
                                    _container.WorkingLayer.Shapes.Add(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.One;
                                }
                                break;
                            case State.One:
                                {
                                    var line = _shape as XLine;
                                    line.End.X = sx;
                                    line.End.Y = sy;
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.CurrentLayer.Shapes.Add(_shape);
                                    _container.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Rectangle
                case Tool.Rectangle:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                    _shape = XRectangle.Create(
                                        sx, sy, 
                                        _container.CurrentStyle, 
                                        _container.PointShape,
                                        DefaultIsFilled);
                                    _container.WorkingLayer.Shapes.Add(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.One;
                                }
                                break;
                            case State.One:
                                {
                                    var rectangle = _shape as XRectangle;
                                    rectangle.BottomRight.X = sx;
                                    rectangle.BottomRight.Y = sy;
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.CurrentLayer.Shapes.Add(_shape);
                                    _container.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Ellipse
                case Tool.Ellipse:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                    _shape = XEllipse.Create(
                                        sx, sy, 
                                        _container.CurrentStyle, 
                                        _container.PointShape,
                                        DefaultIsFilled);
                                    _container.WorkingLayer.Shapes.Add(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.One;
                                }
                                break;
                            case State.One:
                                {
                                    var ellipse = _shape as XEllipse;
                                    ellipse.BottomRight.X = sx;
                                    ellipse.BottomRight.Y = sy;
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.CurrentLayer.Shapes.Add(_shape);
                                    _container.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Bezier
                case Tool.Bezier:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                    _shape = XBezier.Create(
                                        sx, sy, 
                                        _container.CurrentStyle, 
                                        _container.PointShape,
                                        DefaultIsFilled);
                                    _container.WorkingLayer.Shapes.Add(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.One;
                                }
                                break;
                            case State.One:
                                {
                                    var bezier = _shape as XBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    bezier.Point3.X = sx;
                                    bezier.Point3.Y = sy;
                                    bezier.Point4.X = sx;
                                    bezier.Point4.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.Two;
                                }
                                break;
                            case State.Two:
                                {
                                    var bezier = _shape as XBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    bezier.Point3.X = sx;
                                    bezier.Point3.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.Three;
                                }
                                break;
                            case State.Three:
                                {
                                    var bezier = _shape as XBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.CurrentLayer.Shapes.Add(_shape);
                                    _container.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // QBezier
                case Tool.QBezier:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                    _shape = XQBezier.Create(
                                        sx, sy, 
                                        _container.CurrentStyle, 
                                        _container.PointShape,
                                        DefaultIsFilled);
                                    _container.WorkingLayer.Shapes.Add(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.One;
                                }
                                break;
                            case State.One:
                                {
                                    var qbezier = _shape as XQBezier;
                                    qbezier.Point2.X = sx;
                                    qbezier.Point2.Y = sy;
                                    qbezier.Point3.X = sx;
                                    qbezier.Point3.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.Two;
                                }
                                break;
                            case State.Two:
                                {
                                    var qbezier = _shape as XQBezier;
                                    qbezier.Point2.X = sx;
                                    qbezier.Point2.Y = sy;
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.CurrentLayer.Shapes.Add(_shape);
                                    _container.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Text
                case Tool.Text:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                    _shape = XText.Create(
                                        sx, sy, 
                                        _container.CurrentStyle, 
                                        _container.PointShape,
                                        "Text",
                                        DefaultIsFilled);
                                    _container.WorkingLayer.Shapes.Add(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.One;
                                }
                                break;
                            case State.One:
                                {
                                    var text = _shape as XText;
                                    text.BottomRight.X = sx;
                                    text.BottomRight.Y = sy;
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.CurrentLayer.Shapes.Add(_shape);
                                    _container.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        public void Right(double x, double y)
        {
            double sx = SnapToGrid ? Snap(x, SnapX) : x;
            double sy = SnapToGrid ? Snap(y, SnapY) : y;
            switch (CurrentTool)
            {
                // None
                case Tool.None:
                    {
                    }
                    break;
                // Line
                case Tool.Line:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Rectangle
                case Tool.Rectangle:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Ellipse
                case Tool.Ellipse:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Bezier
                case Tool.Bezier:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                            case State.Two:
                            case State.Three:
                                {
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // QBezier
                case Tool.QBezier:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                            case State.Two:
                                {
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
                // Text
                case Tool.Text:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    _container.WorkingLayer.Shapes.Remove(_shape);
                                    _container.WorkingLayer.Invalidate();
                                    CurrentState = State.None;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        public void Move(double x, double y)
        {
            double sx = SnapToGrid ? Snap(x, SnapX) : x;
            double sy = SnapToGrid ? Snap(y, SnapY) : y;
            switch (CurrentTool)
            {
                // None
                case Tool.None:
                    {
                    }
                    break;
                // Line
                case Tool.Line:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    var line = _shape as XLine;
                                    line.End.X = sx;
                                    line.End.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                        }
                    }
                    break;
                // Rectangle
                case Tool.Rectangle:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    var rectangle = _shape as XRectangle;
                                    rectangle.BottomRight.X = sx;
                                    rectangle.BottomRight.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                        }
                    }
                    break;
                // Ellipse
                case Tool.Ellipse:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    var ellipse = _shape as XEllipse;
                                    ellipse.BottomRight.X = sx;
                                    ellipse.BottomRight.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                        }
                    }
                    break;
                // Bezier
                case Tool.Bezier:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    var bezier = _shape as XBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    bezier.Point3.X = sx;
                                    bezier.Point3.Y = sy;
                                    bezier.Point4.X = sx;
                                    bezier.Point4.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                            case State.Two:
                                {
                                    var bezier = _shape as XBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    bezier.Point3.X = sx;
                                    bezier.Point3.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                            case State.Three:
                                {
                                    var bezier = _shape as XBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                        }
                    }
                    break;
                // QBezier
                case Tool.QBezier:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    var bezier = _shape as XQBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    bezier.Point3.X = sx;
                                    bezier.Point3.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                            case State.Two:
                                {
                                    var bezier = _shape as XQBezier;
                                    bezier.Point2.X = sx;
                                    bezier.Point2.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                        }
                    }
                    break;
                // Text
                case Tool.Text:
                    {
                        switch (CurrentState)
                        {
                            case State.None:
                                {
                                }
                                break;
                            case State.One:
                                {
                                    var text = _shape as XText;
                                    text.BottomRight.X = sx;
                                    text.BottomRight.Y = sy;
                                    _container.WorkingLayer.Invalidate();
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
