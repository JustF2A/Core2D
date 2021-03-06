using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Core2D.UI
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            var name = data.GetType()?.FullName?.Replace("Core2D", "Core2D.UI.Views") + "Control";
            if (name == null)
            {
                return new TextBlock { Text = "Invalid Data Type" };
            }
            var type = Type.GetType(name);
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                    return (Control)instance;
                }
                else
                {
                    return new TextBlock { Text = "Create Instance Failed: " + type.FullName };
                }
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ObservableObject;
        }
    }
}
