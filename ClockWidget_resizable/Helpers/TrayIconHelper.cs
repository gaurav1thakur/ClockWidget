using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace ClockWidget.Helpers
{
    public class TrayIconHelper : IDisposable
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        private readonly Forms.ContextMenuStrip _menu;
        private readonly Dictionary<string, Forms.ToolStripMenuItem> _menuItems = new();
        private readonly Forms.ToolStripMenuItem _parentSubMenu; // for submenu context

        // Main tray constructor
        public TrayIconHelper(string iconResourceName, string tooltip)
        {
            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = LoadEmbeddedIcon(iconResourceName) ?? Drawing.SystemIcons.Application,
                Text = tooltip,
                Visible = true
            };

            _menu = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip = _menu;
        }

        // Private constructor for submenu building
        private TrayIconHelper(Forms.ToolStripMenuItem parentMenu)
        {
            _parentSubMenu = parentMenu;
        }

        public void AddMenuItem(string text, EventHandler onClick, bool checkable = false, bool isChecked = false)
        {
            var item = new Forms.ToolStripMenuItem(text)
            {
                CheckOnClick = checkable,
                Checked = isChecked
            };
            item.Click += onClick;

            if (_menu != null)
                _menu.Items.Add(item);
            else if (_parentSubMenu != null)
                _parentSubMenu.DropDownItems.Add(item);

            _menuItems[text] = item;
        }

        public void AddSeparator()
        {
            if (_menu != null)
                _menu.Items.Add(new Forms.ToolStripSeparator());
            else if (_parentSubMenu != null)
                _parentSubMenu.DropDownItems.Add(new Forms.ToolStripSeparator());
        }

        public void AddSubMenu(string text, Action<TrayIconHelper> buildSubMenu)
        {
            var subMenu = new Forms.ToolStripMenuItem(text);

            if (_menu != null)
                _menu.Items.Add(subMenu);
            else if (_parentSubMenu != null)
                _parentSubMenu.DropDownItems.Add(subMenu);

            _menuItems[text] = subMenu;

            var tempHelper = new TrayIconHelper(subMenu);
            buildSubMenu(tempHelper);
        }

        public void SetMenuChecked(string text, bool isChecked)
        {
            if (_menuItems.TryGetValue(text, out var item))
                item.Checked = isChecked;
        }

        public void SetSubMenuChecked(string subMenuText, string itemText)
        {
            if (_menuItems.TryGetValue(subMenuText, out var subMenu) && subMenu is Forms.ToolStripMenuItem menu)
            {
                foreach (Forms.ToolStripMenuItem child in menu.DropDownItems)
                    child.Checked = (child.Text == itemText);
            }
        }

        private Drawing.Icon LoadEmbeddedIcon(string resourceName)
        {
            try
            {
                var asm = Assembly.GetEntryAssembly();
                using Stream stream = asm.GetManifestResourceStream(resourceName);
                if (stream != null)
                    return new Drawing.Icon(stream);
            }
            catch { }
            return null;
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            _menu?.Dispose();
        }
    }
}
