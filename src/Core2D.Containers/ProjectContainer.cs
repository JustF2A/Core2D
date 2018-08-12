﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core2D.Attributes;
using Core2D.Containers;
using Core2D.Data;
using Core2D.History;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Containers
{
    /// <summary>
    /// Project container.
    /// </summary>
    public partial class ProjectContainer : ObservableObject, IProjectContainer
    {
        private IOptions _options;
        private IHistory _history;
        private ImmutableArray<Library<ShapeStyle>> _styleLibraries;
        private ImmutableArray<Library<IGroupShape>> _groupLibraries;
        private ImmutableArray<Database> _databases;
        private ImmutableArray<IPageContainer> _templates;
        private ImmutableArray<IDocumentContainer> _documents;
        private Library<ShapeStyle> _currentStyleLibrary;
        private Library<IGroupShape> _currentGroupLibrary;
        private Database _currentDatabase;
        private IPageContainer _currentTemplate;
        private IDocumentContainer _currentDocument;
        private IPageContainer _currentContainer;
        private IObservableObject _selected;

        /// <summary>
        /// Gets or sets project options.
        /// </summary>
        public IOptions Options
        {
            get => _options;
            set => Update(ref _options, value);
        }

        /// <summary>
        /// Gets or sets undo/redo history handler.
        /// </summary>
        public IHistory History
        {
            get => _history;
            set => Update(ref _history, value);
        }

        /// <summary>
        /// Gets or sets project style libraries.
        /// </summary>
        public ImmutableArray<Library<ShapeStyle>> StyleLibraries
        {
            get => _styleLibraries;
            set => Update(ref _styleLibraries, value);
        }

        /// <summary>
        /// Gets or sets project group libraries.
        /// </summary>
        public ImmutableArray<Library<IGroupShape>> GroupLibraries
        {
            get => _groupLibraries;
            set => Update(ref _groupLibraries, value);
        }

        /// <summary>
        /// Gets or sets project databases.
        /// </summary>
        public ImmutableArray<Database> Databases
        {
            get => _databases;
            set => Update(ref _databases, value);
        }

        /// <summary>
        /// Gets or sets project templates.
        /// </summary>
        public ImmutableArray<IPageContainer> Templates
        {
            get => _templates;
            set => Update(ref _templates, value);
        }

        /// <summary>
        /// Gets or sets project documents.
        /// </summary>
        [Content]
        public ImmutableArray<IDocumentContainer> Documents
        {
            get => _documents;
            set => Update(ref _documents, value);
        }

        /// <summary>
        /// Gets or sets project current style library.
        /// </summary>
        public Library<ShapeStyle> CurrentStyleLibrary
        {
            get => _currentStyleLibrary;
            set => Update(ref _currentStyleLibrary, value);
        }

        /// <summary>
        /// Gets or sets project current group library.
        /// </summary>
        public Library<IGroupShape> CurrentGroupLibrary
        {
            get => _currentGroupLibrary;
            set => Update(ref _currentGroupLibrary, value);
        }

        /// <summary>
        /// Gets or sets project current database.
        /// </summary>
        public Database CurrentDatabase
        {
            get => _currentDatabase;
            set => Update(ref _currentDatabase, value);
        }

        /// <summary>
        /// Gets or sets project current template.
        /// </summary>
        public IPageContainer CurrentTemplate
        {
            get => _currentTemplate;
            set => Update(ref _currentTemplate, value);
        }

        /// <summary>
        /// Gets or sets project current document.
        /// </summary>
        public IDocumentContainer CurrentDocument
        {
            get => _currentDocument;
            set => Update(ref _currentDocument, value);
        }

        /// <summary>
        /// Gets or sets project current container.
        /// </summary>
        public IPageContainer CurrentContainer
        {
            get => _currentContainer;
            set => Update(ref _currentContainer, value);
        }

        /// <summary>
        /// Gets or sets currently selected object.
        /// </summary>
        public IObservableObject Selected
        {
            get => _selected;
            set
            {
                SetSelected(value);
                Update(ref _selected, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IProjectContainer"/> class.
        /// </summary>
        public ProjectContainer()
            : base()
        {
            _options = Containers.Options.Create();
            _styleLibraries = ImmutableArray.Create<Library<ShapeStyle>>();
            _groupLibraries = ImmutableArray.Create<Library<IGroupShape>>();
            _databases = ImmutableArray.Create<Database>();
            _templates = ImmutableArray.Create<IPageContainer>();
            _documents = ImmutableArray.Create<IDocumentContainer>();
        }

        /// <summary>
        /// Set current document.
        /// </summary>
        /// <param name="document">The document instance.</param>
        public void SetCurrentDocument(IDocumentContainer document)
        {
            CurrentDocument = document;
            Selected = document;
        }

        /// <summary>
        /// Set current container.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public void SetCurrentContainer(IPageContainer container)
        {
            CurrentContainer = container;
            Selected = container;
        }

        /// <summary>
        /// Set current template.
        /// </summary>
        /// <param name="template">The template instance.</param>
        public void SetCurrentTemplate(IPageContainer template) => CurrentTemplate = template;

        /// <summary>
        /// Set current database.
        /// </summary>
        /// <param name="db">The database instance.</param>
        public void SetCurrentDatabase(Database db) => CurrentDatabase = db;

        /// <summary>
        /// Set current group library.
        /// </summary>
        /// <param name="library">The group library instance.</param>
        public void SetCurrentGroupLibrary(Library<IGroupShape> library) => CurrentGroupLibrary = library;

        /// <summary>
        /// Set current group.
        /// </summary>
        /// <param name="library">The style library instance.</param>
        public void SetCurrentStyleLibrary(Library<ShapeStyle> library) => CurrentStyleLibrary = library;

        /// <summary>
        /// Set selected value.
        /// </summary>
        /// <param name="value">The value instance.</param>
        public void SetSelected(IObservableObject value)
        {
            if (value is ILayerContainer layer)
            {
                var owner = layer?.Owner;
                if (owner != null)
                {
                    if (owner.CurrentLayer != layer)
                    {
                        owner.CurrentLayer = layer;
                    }
                }
            }
            else if (value is IPageContainer container && _documents != null)
            {
                var document = _documents.FirstOrDefault(d => d.Pages.Contains(container));
                if (document != null)
                {
                    if (CurrentDocument != document)
                    {
                        CurrentDocument = document;
                    }

                    if (CurrentContainer != container)
                    {
                        CurrentContainer = container;
                        CurrentContainer.Invalidate();
                    }
                }
            }
            else if (value is IDocumentContainer document)
            {
                if (CurrentDocument != document)
                {
                    CurrentDocument = document;
                    if (!CurrentDocument?.Pages.Contains(CurrentContainer) ?? false)
                    {
                        var current = CurrentDocument.Pages.FirstOrDefault();
                        if (CurrentContainer != current)
                        {
                            CurrentContainer = current;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all shapes including grouped shapes.
        /// </summary>
        /// <param name="shapes">The shapes collection.</param>
        /// <returns>All shapes including grouped shapes.</returns>
        public static IEnumerable<IBaseShape> GetAllShapes(IEnumerable<IBaseShape> shapes)
        {
            if (shapes == null)
                yield break;

            foreach (var shape in shapes)
            {
                if (shape is IGroupShape groupShape)
                {
                    foreach (var s in GetAllShapes(groupShape.Shapes))
                    {
                        yield return s;
                    }

                    yield return shape;
                }
                else
                {
                    yield return shape;
                }
            }
        }

        /// <summary>
        /// Get all shapes including grouped shapes of specified type.
        /// </summary>
        /// <typeparam name="T">The type of shape to include.</typeparam>
        /// <param name="shapes">The shapes collection.</param>
        /// <returns>All shapes including grouped shapes of specified type.</returns>
        public static IEnumerable<T> GetAllShapes<T>(IEnumerable<IBaseShape> shapes)
        {
            return GetAllShapes(shapes)?.Where(s => s is T).Cast<T>();
        }

        /// <summary>
        /// Get all shapes including grouped shapes of specified type.
        /// </summary>
        /// <typeparam name="T">The type of shapes to include.</typeparam>
        /// <param name="project">The project object.</param>
        /// <returns>All shapes including grouped shapes of specified type.</returns>
        public static IEnumerable<T> GetAllShapes<T>(IProjectContainer project)
        {
            var shapes = project?.Documents
                .SelectMany(d => d.Pages)
                .SelectMany(c => c.Layers)
                .SelectMany(l => l.Shapes);

            return GetAllShapes(shapes)?.Where(s => s is T).Cast<T>();
        }

        /// <inheritdoc/>
        public virtual object Copy(IDictionary<object, object> shared)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="IProjectContainer"/> instance.
        /// </summary>
        /// <param name="name">The project name.</param>
        /// <returns>The new instance of the <see cref="IProjectContainer"/> class.</returns>
        public static IProjectContainer Create(string name = "Project")
        {
            return new ProjectContainer()
            {
                Name = name
            };
        }

        /// <summary>
        /// Check whether the <see cref="Options"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeOptions() => _options != null;

        /// <summary>
        /// Check whether the <see cref="History"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeHistory() => _history != null;

        /// <summary>
        /// Check whether the <see cref="StyleLibraries"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeStyleLibraries() => _styleLibraries.IsEmpty == false;

        /// <summary>
        /// Check whether the <see cref="GroupLibraries"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeGroupLibraries() => _groupLibraries.IsEmpty == false;

        /// <summary>
        /// Check whether the <see cref="Databases"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDatabases() => _databases.IsEmpty == false;

        /// <summary>
        /// Check whether the <see cref="Templates"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeTemplates() => _templates.IsEmpty == false;

        /// <summary>
        /// Check whether the <see cref="Documents"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDocuments() => _documents.IsEmpty == false;

        /// <summary>
        /// Check whether the <see cref="CurrentStyleLibrary"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentStyleLibrary() => _currentStyleLibrary != null;

        /// <summary>
        /// Check whether the <see cref="CurrentGroupLibrary"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentGroupLibrary() => _currentGroupLibrary != null;

        /// <summary>
        /// Check whether the <see cref="CurrentDatabase"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentDatabase() => _currentDatabase != null;

        /// <summary>
        /// Check whether the <see cref="CurrentTemplate"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentTemplate() => _currentTemplate != null;

        /// <summary>
        /// Check whether the <see cref="CurrentDocument"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentDocument() => _currentDocument != null;

        /// <summary>
        /// Check whether the <see cref="CurrentContainer"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentContainer() => _currentContainer != null;

        /// <summary>
        /// Check whether the <see cref="Selected"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSelected() => _selected != null;
    }
}
