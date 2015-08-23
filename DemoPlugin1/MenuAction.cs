using DemoInterfaces;
using Sandboxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly:Sandboxee]

namespace DemoPlugin1
{
    public class MenuAction : MarshalByRefObject, IToolsMenuAction
    {
        public void ExecuteAction()
        {
            MessageBox.Show("Hello world from plugin 1.");
        }

        public string GetItemName()
        {
            return "Greeting";
        }
    }
}
