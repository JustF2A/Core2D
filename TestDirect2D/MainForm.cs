﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Test2d;
using TestEDITOR;

namespace TestDirect2D
{
    /// <summary>
    /// 
    /// </summary>
    public class MainForm : Form, IView
    {
        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            var context = new EditorContext();
            context.Initialize(
                this,
                new EtoRenderer(72.0 / 96.0),
                new TextClipboard(),
                new GZipCompressor());
            context.InitializeSctipts();
            context.InitializeSimulation();
            context.Editor.Renderer.DrawShapeState = ShapeState.Visible;
            context.Editor.GetImagePath = () => Image();

            Title = "Test";
            ClientSize = new Size(1000, 650);
            Closed += (s, e) => context.Dispose();
            
            var drawable = new Drawable(true);
            drawable.Width = (int)context.Editor.Project.CurrentContainer.Width;
            drawable.Height = (int)context.Editor.Project.CurrentContainer.Height;

            drawable.Paint += (s, e) => Draw(e.Graphics);

            drawable.MouseDown += 
                (sender, e) =>
                {
                    if (e.Buttons == MouseButtons.Primary)
                    {
                        drawable.Focus();
                        if (context.Editor.IsLeftDownAvailable())
                        {
                            var p = e.Location;
                            context.Editor.LeftDown(p.X, p.Y);
                        }
                    }
    
                    if (e.Buttons == MouseButtons.Alternate)
                    {
                        drawable.Focus();
                        if (context.Editor.IsRightDownAvailable())
                        {
                            var p = e.Location;
                            context.Editor.RightDown(p.X, p.Y);
                        }
                    }
                };

            drawable.MouseUp += 
                (sender, e) =>
                {
                    if (e.Buttons == MouseButtons.Primary)
                    {
                        drawable.Focus();
                        if (context.Editor.IsLeftUpAvailable())
                        {
                            var p = e.Location;
                            context.Editor.LeftUp(p.X, p.Y);
                        }
                    }
    
                    if (e.Buttons == MouseButtons.Alternate)
                    {
                        drawable.Focus();
                        if (context.Editor.IsRightUpAvailable())
                        {
                            var p = e.Location;
                            context.Editor.RightUp(p.X, p.Y);
                        }
                    }
                };

            drawable.MouseMove += 
                (sender, e) =>
                {
                    drawable.Focus();
                    if (context.Editor.IsMoveAvailable())
                    {
                        var p = e.Location;
                        context.Editor.Move(p.X, p.Y);
                    }
                };

            drawable.KeyDown +=
                (sender, e) =>
                {
                    if (e.Control || e.Alt || e.Shift)
                        return;

                    switch (e.Key)
                    {
                        case Keys.N:
                            context.Commands.ToolNoneCommand.Execute(null);
                            break;
                        case Keys.S:
                            context.Commands.ToolSelectionCommand.Execute(null);
                            break;
                        case Keys.P:
                            context.Commands.ToolPointCommand.Execute(null);
                            break;
                        case Keys.L:
                            context.Commands.ToolLineCommand.Execute(null);
                            break;
                        case Keys.R:
                            context.Commands.ToolRectangleCommand.Execute(null);
                            break;
                        case Keys.E:
                            context.Commands.ToolEllipseCommand.Execute(null);
                            break;
                        case Keys.A:
                            context.Commands.ToolArcCommand.Execute(null);
                            break;
                        case Keys.B:
                            context.Commands.ToolBezierCommand.Execute(null);
                            break;
                        case Keys.Q:
                            context.Commands.ToolQBezierCommand.Execute(null);
                            break;
                        case Keys.T:
                            context.Commands.ToolTextCommand.Execute(null);
                            break;
                        case Keys.I:
                            context.Commands.ToolImageCommand.Execute(null);
                            break;
                        case Keys.F:
                            context.Commands.DefaultIsFilledCommand.Execute(null);
                            break;
                        case Keys.G:
                            context.Commands.SnapToGridCommand.Execute(null);
                            break;
                        case Keys.C:
                            context.Commands.TryToConnectCommand.Execute(null);
                            break;
                    }
                };

            var listBox = new ListBox();
            listBox.BindDataContext(
                c => c.DataStore, 
                (EditorContext c) => c.Editor.Project.CurrentDocument.Containers);

            Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new TableLayout(
                    null,
                    new TableRow(null, drawable, null, listBox, null),
                    null)
            };
            
            Load += (s, e) => drawable.Focus();

            var newCommand = new Command()
            {
                MenuText = "&New", 
                Shortcut = Application.Instance.CommonModifier | Keys.N
            };
            newCommand.Executed += 
                (s, e) =>
                {
                    context.Commands.NewCommand.Execute(null);
                    Invalidate(drawable);
                };

            var openCommand = new Command()
            {
                MenuText = "&Open...", 
                Shortcut = Application.Instance.CommonModifier | Keys.O
            };
            openCommand.Executed += 
                (s, e) =>
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filters = new List<FileDialogFilter>()
                    {
                        new FileDialogFilter("Project", ".project"),
                        new FileDialogFilter("All", ".*")
                    };
                    var result = dlg.ShowDialog(this);
                    if (result == DialogResult.Ok)
                    {
                        context.Open(dlg.FileName);
                        Invalidate(drawable);
                    }
                };
            
            var saveAsCommand = new Command()
            {
                MenuText = "Save &As...", 
                Shortcut = Application.Instance.CommonModifier | Keys.S
            };
            saveAsCommand.Executed += 
                (s, e) =>
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filters = new List<FileDialogFilter>()
                    {
                        new FileDialogFilter("Project", ".project"),
                        new FileDialogFilter("All", ".*")
                    };
                    dlg.FileName = context.Editor.Project.Name;
                    var result = dlg.ShowDialog(this);
                    if (result == DialogResult.Ok)
                    {
                        context.Save(dlg.FileName);
                    }
                };

            var exportCommand = new Command()
            {
                MenuText = "&Export...", 
                Shortcut = Application.Instance.CommonModifier | Keys.E
            };
            exportCommand.Executed += 
                (s, e) =>
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filters = new List<FileDialogFilter>()
                    {
                        new FileDialogFilter("Pdf", ".pdf"),
                        new FileDialogFilter("Emf", ".emf"),
                        new FileDialogFilter("Dxf AutoCAD 2000", ".dxf"),
                        new FileDialogFilter("Dxf R10", ".dxf"),
                        new FileDialogFilter("All", ".*")
                    };
                    dlg.FileName = context.Editor.Project.Name;
                    var result = dlg.ShowDialog(this);
                    if (result == DialogResult.Ok)
                    {
                        string path = dlg.FileName;
                        int filterIndex = dlg.CurrentFilterIndex;
                        switch (filterIndex)
                        {
                            case 0:
                                context.ExportAsPdf(path, context.Editor.Project);
                                System.Diagnostics.Process.Start(path);
                                break;
                            case 1:
                                context.ExportAsEmf(path);
                                System.Diagnostics.Process.Start(path);
                                break;
                            case 2:
                                context.ExportAsDxf(path, Dxf.DxfAcadVer.AC1015);
                                System.Diagnostics.Process.Start(path);
                                break;
                            case 3:
                                context.ExportAsDxf(path, Dxf.DxfAcadVer.AC1006);
                                System.Diagnostics.Process.Start(path);
                                break;
                            default:
                                break;
                        }
                    }
                };
            
            var exitCommand = new Command() 
            { 
                MenuText = "E&xit", 
                Shortcut = Application.Instance.AlternateModifier | Keys.F4 
            };
            exitCommand.Executed +=  (s, e) => Application.Instance.Quit();

            var aboutCommand = new Command()
            { 
                MenuText = "&About..." 
            };
            aboutCommand.Executed += (s, e) => MessageBox.Show(this, Platform.ID);

            var noneTool = new Command() { MenuText = "&None", Shortcut = Keys.N };
            noneTool.Executed += (s, e) => context.Commands.ToolNoneCommand.Execute(null);
            
            var selectionTool = new Command() { MenuText = "&Selection", Shortcut = Keys.S };
            selectionTool.Executed += (s, e) => context.Commands.ToolSelectionCommand.Execute(null);
            
            var pointTool = new Command() { MenuText = "&Point", Shortcut = Keys.P };
            pointTool.Executed += (s, e) => context.Commands.ToolPointCommand.Execute(null);
 
            var lineTool = new Command() { MenuText = "&Line", Shortcut = Keys.L };
            lineTool.Executed += (s, e) => context.Commands.ToolLineCommand.Execute(null);
            
            var rectangleTool = new Command() { MenuText = "&Rectangle", Shortcut = Keys.R };
            rectangleTool.Executed += (s, e) => context.Commands.ToolRectangleCommand.Execute(null);
            
            var ellipseTool = new Command() { MenuText = "&Ellipse", Shortcut = Keys.E };
            ellipseTool.Executed += (s, e) => context.Commands.ToolEllipseCommand.Execute(null);
            
            var arcTool = new Command() { MenuText = "&Arc", Shortcut = Keys.A };
            arcTool.Executed += (s, e) => context.Commands.ToolArcCommand.Execute(null);
            
            var bezierTool = new Command() { MenuText = "&Bezier", Shortcut = Keys.B };
            bezierTool.Executed += (s, e) => context.Commands.ToolBezierCommand.Execute(null);
            
            var qbezierTool = new Command() { MenuText = "&QBeezier", Shortcut = Keys.Q };
            qbezierTool.Executed += (s, e) => context.Commands.ToolQBezierCommand.Execute(null);
            
            var textTool = new Command() { MenuText = "&Text", Shortcut = Keys.T };
            textTool.Executed += (s, e) => context.Commands.ToolTextCommand.Execute(null);
            
            var imageTool = new Command() { MenuText = "&Image", Shortcut = Keys.I };
            imageTool.Executed += (s, e) => context.Commands.ToolImageCommand.Execute(null);

            var undoCommand = new Command() { MenuText = "&Undo", Shortcut = Application.Instance.CommonModifier | Keys.Z };
            undoCommand.Executed += (s, e) => context.Commands.UndoCommand.Execute(null);
            
            var redoCommand = new Command() { MenuText = "&Redo", Shortcut = Application.Instance.CommonModifier | Keys.Y };
            redoCommand.Executed += (s, e) => context.Commands.RedoCommand.Execute(null);

            var copyAsMetaFileCommand = new Command() { MenuText = "Copy As &Metafile", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.Y };
            copyAsMetaFileCommand.Executed += (s, e) => context.Commands.CopyAsEmfCommand.Execute(null);

            var cutCommand = new Command() { MenuText = "Cu&t", Shortcut = Application.Instance.CommonModifier | Keys.X };
            cutCommand.Executed += (s, e) => context.Commands.CutCommand.Execute(null);
            
            var copyCommand = new Command() { MenuText = "&Copy", Shortcut = Application.Instance.CommonModifier | Keys.C };
            copyCommand.Executed += (s, e) => context.Commands.CopyCommand.Execute(null);
            
            var pasteCommand = new Command() { MenuText = "&Paste", Shortcut = Application.Instance.CommonModifier | Keys.V };
            pasteCommand.Executed += (s, e) => context.Commands.PasteCommand.Execute(null);
            
            var deleteCommand = new Command() { MenuText = "&Delete", Shortcut = Keys.Delete };
            deleteCommand.Executed += (s, e) => context.Commands.DeleteCommand.Execute(null);

            var selectAllCommand = new Command() { MenuText = "Select &All", Shortcut = Application.Instance.CommonModifier | Keys.A };
            selectAllCommand.Executed += (s, e) => context.Commands.SelectAllCommand.Execute(null);
            
            var clearAllCommand = new Command() { MenuText = "Cl&ear All" };
            clearAllCommand.Executed += (s, e) => context.Commands.ClearAllCommand.Execute(null);
            
            var groupCommand = new Command() { MenuText = "&Group", Shortcut = Application.Instance.CommonModifier | Keys.G };
            groupCommand.Executed += (s, e) => context.Commands.GroupCommand.Execute(null);
            
            var groupLayerCommand = new Command() { MenuText = "Group &Layer", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.G };
            groupLayerCommand.Executed += (s, e) => context.Commands.GroupLayerCommand.Execute(null);

            var evalCommand = new Command()
            {
                MenuText = "&Evaluate...", 
                Shortcut = Keys.F9
            };
            evalCommand.Executed += 
                (s, e) =>
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filters = new List<FileDialogFilter>()
                    {
                        new FileDialogFilter("C#", ".cs"),
                        new FileDialogFilter("All", ".*")
                    };
                    var result = dlg.ShowDialog(this);
                    if (result == DialogResult.Ok)
                    {
                        context.Eval(dlg.FileName);
                        Invalidate(drawable);
                    }
                };
            
            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem() 
                    { 
                        Text = "&File", 
                        Items = 
                        { 
                            newCommand,
                            new SeparatorMenuItem(),
                            openCommand,
                            new SeparatorMenuItem(),
                            saveAsCommand,
                            new SeparatorMenuItem(),
                            exportCommand
                        }
                    },
                    new ButtonMenuItem() 
                    { 
                        Text = "&Edit", 
                        Items = 
                        {
                            undoCommand,
                            redoCommand,
                            new SeparatorMenuItem(),
                            copyAsMetaFileCommand,
                            new SeparatorMenuItem(),
                            cutCommand,
                            copyCommand,
                            pasteCommand,
                            deleteCommand,
                            new SeparatorMenuItem(),
                            selectAllCommand,
                            new SeparatorMenuItem(),
                            clearAllCommand,
                            new SeparatorMenuItem(),
                            groupCommand,
                            groupLayerCommand,
                        } 
                    },
                    new ButtonMenuItem() 
                    { 
                        Text = "&Tool", 
                        Items =
                        { 
                            noneTool,
                            new SeparatorMenuItem(),
                            selectionTool,
                            new SeparatorMenuItem(),
                            pointTool,
                            new SeparatorMenuItem(),
                            lineTool,
                            rectangleTool,
                            ellipseTool,
                            new SeparatorMenuItem(),
                            arcTool,
                            bezierTool,
                            qbezierTool,
                            new SeparatorMenuItem(),
                            textTool,
                            new SeparatorMenuItem(),
                            imageTool
                        }
                    },
                    new ButtonMenuItem() 
                    { 
                        Text = "&Script", 
                        Items = 
                        { 
                            evalCommand
                        } 
                    }
                },
                QuitItem = exitCommand,
                AboutItem = aboutCommand
            };

            DataContext = context;
            
            SetContainerInvalidation(drawable);
            SetDrawableSize(drawable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawable"></param>
        private void SetDrawableSize(Drawable drawable)
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var container = context.Editor.Project.CurrentContainer;
            if (container == null)
                return;
            
            drawable.Width = (int)container.Width;
            drawable.Height = (int)container.Height;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawable"></param>
        private void SetContainerInvalidation(Drawable drawable)
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;
            
            var container = context.Editor.Project.CurrentContainer;
            if (container == null)
                return;

            foreach (var layer in container.Layers)
            {
                layer.InvalidateLayer += (s, e) => drawable.Invalidate();
            }

            if (container.WorkingLayer != null)
            {
                container.WorkingLayer.InvalidateLayer += (s, e) => drawable.Invalidate();
            }
            
            if (container.HelperLayer != null)
            {
                container.HelperLayer.InvalidateLayer += (s, e) => drawable.Invalidate();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawable"></param>
        private void Invalidate(Drawable drawable)
        {
            SetContainerInvalidation(drawable);
            SetDrawableSize(drawable);

            var context = DataContext as EditorContext;
            if (context == null)
                return;
            
            var container = context.Editor.Project.CurrentContainer;
            if (context == null)
                return;

            container.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void Background(Graphics g, ArgbColor c, double width, double height)
        {
            var brush = new SolidBrush(Color.FromArgb(c.R, c.G, c.B, c.A));
            var rect = Rect2.Create(0, 0, width, height);
            g.FillRectangle(
                brush,
                (float)rect.X,
                (float)rect.Y,
                (float)rect.Width,
                (float)rect.Height);
            brush.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void Draw(Graphics g)
        {
            var context = this.DataContext as EditorContext;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            g.AntiAlias = false;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            //g.TranslateTransform((float)0f, (float)0f);
            //g.ScaleTransform((float)context.Editor.Renderer.Zoom, (float)context.Editor.Renderer.Zoom);
            var background = new SolidBrush(Color.FromArgb(211, 211, 211, 255));
            g.Clear(background);
            background.Dispose();

            var renderer = context.Editor.Renderer;
            var container = context.Editor.Project.CurrentContainer;

            if (container.Template != null)
            {
                Background(g, container.Template.Background, this.Width, this.Height);
                renderer.Draw(g, container.Template, container.Properties);
            }

            Background(g, container.Background, this.Width, this.Height);
            renderer.Draw(g, container, container.Properties);
            
            if (container.WorkingLayer != null)
            {
                renderer.Draw(g, container.WorkingLayer, container.Properties);
            }
            
            if (container.HelperLayer != null)
            {
                renderer.Draw(g, container.HelperLayer, container.Properties);
            }

            sw.Stop();
            System.Diagnostics.Debug.Print(sw.ElapsedMilliseconds + "ms");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Image()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter("All", ".*")
            };
            dlg.FileName = "";
            var result = dlg.ShowDialog(this);
            if (result == DialogResult.Ok)
            {
                return dlg.FileName;
            }
            return null;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    internal class TextClipboard : ITextClipboard
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            new Clipboard().Text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return new Clipboard().Text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ContainsText()
        {
            return !string.IsNullOrEmpty(new Clipboard().Text);
        }
    }
}
