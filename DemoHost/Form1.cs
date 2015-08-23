using DemoInterfaces;
using Sandboxer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoHost
{
    public partial class Form1 : Form
    {
        ISandboxHost sandboxHost;

        public Form1()
        {
            InitializeComponent();
            sandboxHost = new SandboxHostFactory(Path.Combine(Environment.CurrentDirectory, "Plugins")).Create();

            RefreshAvailablePlugins();
            RefreshToolsMenu();

            sandboxHost.Loaded += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    RefreshAvailablePlugins();
                    RefreshToolsMenu();
                    textBoxLog.Text += "Loaded plugin " + e.SandboxeeInfo.Name + Environment.NewLine;
                });
            };

            sandboxHost.Unloaded += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    RefreshAvailablePlugins();
                    RefreshToolsMenu();
                    textBoxLog.Text += "Unloaded plugin " + e.SandboxeeInfo.Name + Environment.NewLine;
                });
            };
        }

        private void RefreshAvailablePlugins()
        {
            listBoxPlugins.Items.Clear();
            listBoxPlugins.Items.AddRange(sandboxHost.AvailableSandboxees.Select(x => x.Name).ToArray());
        }

        private void RefreshToolsMenu()
        {
            toolsToolStripMenuItem.DropDownItems.Clear();
            foreach (var menuAction in sandboxHost.GetInstances<IToolsMenuAction>())
            {
                toolsToolStripMenuItem.DropDownItems.Add(menuAction.GetItemName(), null, (s, a) => { menuAction.ExecuteAction(); });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            sandboxHost.Dispose();
        }
    }
}
