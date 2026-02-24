using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Communication.GUI;
using EFEM.FileUtilities;
using EFEM.GUIControls.GeneralControls.Models;
using Microsoft.VisualBasic.ApplicationServices;

namespace EFEM.GUIControls
{
    public partial class TDKConfigControl : UserControl
    {
        private UserControl _currentView;
        private readonly Dictionary<NodeViewKind, Func<UserControl>> _viewFactory;
        private List<(string, int)> _commList = new List<(string, int)>();
        private List<(string, UserControl)> _userControlList = new List<(string, UserControl)>();

        public TDKConfigControl()
        {
            InitializeComponent();

            try
            {

                treeView1.ViewRequested += Nav_ViewRequested;
                AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
                _commList = _fu.GetCommList();
            }

            catch (Exception ex)
            {

            }


        }

        public void InitAll()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { InitAll(); };
                this.Invoke(del);
            }
            else
            {

            }
        }

        private void Nav_ViewRequested(object sender, ViewRequestEventArgs e)
        {
            ShowView(e.Kind, e.Context);
        }

        private void ShowView(NodeViewKind kind, object context)
        {
            if(_currentView != null)
                _currentView.Visible = false;
            UserControl newView;
            switch (kind)
            {
                case NodeViewKind.Comm:
                    newView = CreateByRule(context);
                    break;
                case NodeViewKind.Log:
                    newView = null;
                    break;
                case NodeViewKind.DIO:
                    newView = null;
                    break;
                case NodeViewKind.LoadPort:
                    newView = null;
                    break;
                case NodeViewKind.N2Nozzle:
                    newView = null;
                    break;
                default:
                    return;
            }

            newView.Visible = true;
            var r = newView as IRefreshable;
            if (r != null)
                r.RefreshData(context);

            _currentView = newView;
        }


        private UserControl CreateByRule(object context)
        {
            var selection = context as string;
            var found = _userControlList.FirstOrDefault(x => x.Item1.Equals(selection));
            if (found.Item1 != null)
            {
                return found.Item2;
            }
            else
            {
                var _commName = _commList.FirstOrDefault(x => x.Item1.Equals(selection));
                UserControl newView;
                if (_commName.Item2 == 1)
                {
                    newView = new TCPIPSettingGUI(selection);
                }
                else
                {
                    newView = new RS232SettingGUI(selection);
                }
                newView.Visible = false;
                newView.Dock = DockStyle.Fill;
                panel1.Controls.Add(newView);
                newView.BringToFront();
                _userControlList.Add((selection, newView));
                return newView;
            }
        }

    }
}
