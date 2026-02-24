using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using EFEM.LogUtilities;
using EFEM.ExceptionManagements;
using EFEM.DataCenter;
using Communication.Interface;
using Communication;

namespace EFEM.GUIControls
{
    public partial class StartupComponentCtrl : UserControl
    {
        public delegate ArrayList delInitializeGUI();
        private delInitializeGUI InitializeGUIHelper = null;
        private ArrayList errorHistory = new ArrayList();
        private int DelayTimeForDebug = 100;
        private bool Inversed = false;
        public event EventHandler InvokeHostActionRequested;

        private enum ProcedureStatus
        {
            Pending = 0,
            Working = 1,
            Finish = 2
        }

        public StartupComponentCtrl()
        {
            InitializeComponent();
            InitDataGrid();
        }

        public ArrayList ErrorHistory
        {
            get 
            {
                if (errorHistory == null || errorHistory.Count == 0)
                    return null;
                else
                    return errorHistory; 
            }
        }

        public bool IsAnyErrorOccurs
        {
            get { return (ErrorHistory != null); }
        }

        public void ShowContinueButton(bool Hide = false)
        {
            btnContinue.Visible = !Hide;
            if (IsAnyErrorOccurs)
                timerBlinking.Enabled = true;
        }

        private void InitDataGrid()
        {
            for (int i = 0; i < 10; i++)
            {
                dataGridViewResult.Rows.Add();
                dataGridViewResult.Rows[i].Cells["Image"].Value = imageListStatus.Images[0];
                dataGridViewResult.Rows[i].Cells["Status"].Value = "Waiting";
                dataGridViewResult.Rows[i].Cells["ObjectName"].Value = GetObjectName(i);
                dataGridViewResult.Rows[i].Cells["ErrorMsg"].Value = "";
            }

            ExtendMethods.StretchLastColumn(dataGridViewResult);
        }

        public Control GetHistoryListControl()
        {
            return dataGridViewResult;
        }

        public void ReleaseHistoryListControl(Control ctrl)
        {
            ctrl.Parent = panelDataGrid;
        }

        private string GetObjectName(int ID)
        {
            switch (ID)
            {
                case 0:
                    return "Communication";
                case 1:
                    return "DIO";
                case 2:
                    return "LoadPortActor";
                case 3:
                    return "CarrierIDReader";
                case 4:
                    return "LightCurtain";
                case 5:
                    return "N2Nozzle";
                case 6:
                    return "LoadPortController";
                case 7:
                    return "LoadPortService";
                case 8:
                    return "E84Station";
                case 9:
                    return "Initialize GUI Components";
                default:
                    return "";
            }
        }

        private string GetProcedureName(int ID)
        {
            switch (ID)
            {
                case 0:
                case 4:
                    return "Instantiate Objects";
                case 1:
                case 5:
                    return "Establish Communications";
                case 2:
                case 6:
                    return "Download Parameters";
                case 3:
                case 7:
                    return "Initialize";
                case 9:
                    return "Initialize GUI Components";
                default:
                    return "";
            }
        }

        public HRESULT StartupAll(ICommunication com, delInitializeGUI InitGUIMethod)
        {
            InitializeGUIHelper = InitGUIMethod;


            //ThreadPool.QueueUserWorkItem(new WaitCallback(TPOOL_StartupAll), com);
            TPOOL_StartupAll(com);
            return null;
        }

        private void UpdateStatus(string currentStatus)
        {
            if (lCurrentStatus.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateStatus(currentStatus); };
                lCurrentStatus.Invoke(del);
            }
            else
            {
                if (currentStatus == "Finish")
                {
                    if (IsAnyErrorOccurs)
                    {
                        lCurrentStatus.BackColor = Color.Red;
                        lCurrentStatus.ForeColor = Color.White;
                        lCurrentStatus.Text = "Error Occurred During Initialization of EFEM components.";
                    }
                    else
                    {
                        lCurrentStatus.Text = "Success";
                    }
                }
                else
                    lCurrentStatus.Text = currentStatus;
            }
        }

        private void UpdateProcedureStatus(int ProcedureID, ProcedureStatus status, ArrayList rst = null)
        {
            if (lCurrentStatus.InvokeRequired)
            {
                MethodInvoker del = delegate { UpdateProcedureStatus(ProcedureID, status, rst); };
                lCurrentStatus.Invoke(del);
            }
            else
            {
                switch (status)
                {
                    case ProcedureStatus.Pending:
                        {
                            dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[0];
                            dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Pending";
                            dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = "";
                            break;
                        }
                    case ProcedureStatus.Working:
                        {
                            GUIBasic.Instance().WriteLog(LogHeadType.CallStart, GetProcedureName(ProcedureID));
                            dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[1];
                            dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Working";
                            dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = "";
                            break;
                        }
                    case ProcedureStatus.Finish:
                        {
                            if (rst != null)
                            {
                                string errMsg = ExtendMethods.ToStringHelper(rst, "; ");
                                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, GetProcedureName(ProcedureID) + ", Fail. Reason: " + errMsg);
                                errorHistory.Add("[" + GetProcedureName(ProcedureID) + "] " + errMsg);
                                dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[3];
                                dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Error";
                                dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = errMsg;
                            }
                            else
                            {
                                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, GetProcedureName(ProcedureID) + ", Success.");
                                dataGridViewResult.Rows[ProcedureID].Cells["Image"].Value = imageListStatus.Images[2];
                                dataGridViewResult.Rows[ProcedureID].Cells["Status"].Value = "Success";
                                dataGridViewResult.Rows[ProcedureID].Cells["ErrorMsg"].Value = "";
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void TPOOL_StartupAll(object para)
        {
            try
            {
                errorHistory.Clear();
                int curProcrdure = 0;
                string objectName;
                string procedureName;
                ArrayList rst;
                var obj = (ICommunication)para;

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(curProcrdure);
                GUIBasic.Instance().WriteLog(LogHeadType.CallStart, "", objectName + "." + procedureName);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                rst = obj.Initialize();
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure = curProcrdure + 9;
                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, "", objectName + "." + procedureName);

                #region Init GUI

                objectName = GetObjectName(curProcrdure);
                procedureName = GetProcedureName(curProcrdure);
                GUIBasic.Instance().WriteLog(LogHeadType.CallStart, "", objectName + "." + procedureName);
                UpdateStatus(objectName + " : " + procedureName);
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Working);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    objectName + " -> " + procedureName);
                if (InitializeGUIHelper != null)
                    rst = InitializeGUIHelper();
                else
                    rst = null;
                UpdateProcedureStatus(curProcrdure, ProcedureStatus.Finish, rst);
                Thread.Sleep(DelayTimeForDebug);
                curProcrdure++;

                //Clean log and start the timer to claen logs every 24 hours
                GUIBasic.Instance().Log.ClearLogs();
                GUIBasic.Instance().WriteLog(LogHeadType.CallEnd, "", objectName + "." + procedureName);

                #endregion

                UpdateStatus("Finish");
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    "All Initializations Finish");
            }
            catch (Exception e)
            {
                UpdateStatus("Exception: " + e.Message);
                errorHistory.Add("[TPOOL_StartupAll] " + e.Message + e.StackTrace);
                GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus,
                    "All Initializations Finish (Fail)");
            }
            finally
            { ShowContinueButton(false);
            }
        }

        private void dataGridViewResult_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Image"].Value = imageListStatus.Images[0];
            e.Row.Cells["Status"].Value = "Waiting";
            e.Row.Cells["ErrorMsg"].Value = "";
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            timerBlinking.Enabled = false;
            if (IsAnyErrorOccurs)
            {
                lCurrentStatus.BackColor = Color.Red;
                lCurrentStatus.ForeColor = Color.White;
            }

            InvokeHostActionRequested?.Invoke(this, EventArgs.Empty);

            GUIBasic.Instance().VariableCenter.SetValueAndFireCallback(ConstVC.VariableCenter.CurrentStatus, "ForceOperatedByUser");
        }

        private void timerBlinking_Tick(object sender, EventArgs e)
        {
            if (Inversed)
            {
                lCurrentStatus.BackColor = Color.Red;
                lCurrentStatus.ForeColor = Color.White;
            }
            else
            {
                lCurrentStatus.BackColor = SystemColors.Control;
                lCurrentStatus.ForeColor = Color.Black;
            }

            Inversed = !Inversed;
        }

        private void dataGridViewResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                object err = dataGridViewResult.Rows[e.RowIndex].Cells["ErrorMsg"].Value;
                if (err == null)
                    return;
                else
                {
                    string errorMsg = dataGridViewResult.Rows[e.RowIndex].Cells["ErrorMsg"].Value.ToString();
                    if (string.IsNullOrWhiteSpace(errorMsg))
                        return;
                    else
                    {
                        using (InitStatusErrorListForm listForm = new InitStatusErrorListForm())
                        {
                            string caption = string.Format("{0}",
                                dataGridViewResult.Rows[e.RowIndex].Cells["ObjectName"].Value.ToString());

                            if (listForm.AssignData(caption, errorMsg))
                            {
                                listForm.TopMost = true;
                                listForm.StartPosition = FormStartPosition.CenterParent;
                                listForm.ShowDialog();
                            }
                        }
                    }
                }
            }
        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder buffer = new StringBuilder();
                for (int j = 0; j < dataGridViewResult.RowCount; j++)
                {
                    for (int i = 1; i < dataGridViewResult.ColumnCount; i++)
                    {
                        buffer.Append(dataGridViewResult.Rows[j].Cells[i].Value.ToString());
                        buffer.Append("\t");
                    }

                    buffer.Append("\r\n");
                }

                Clipboard.SetText(buffer.ToString());
            }
            catch (Exception ex)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Exception, "Copy to clipboard failed! Reason: " + ex.Message);
                GUIBasic.Instance().ShowMessageOnTop("Copy to clipboard failed!");
            }
        }
    }
}
