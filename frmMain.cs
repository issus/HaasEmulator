using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaasEmulator
{
    public partial class frmMain : Form
    {
        SimpleTcpServer server = null;
        int toolChanges = 1;
        DateTime start = DateTime.Now;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (server == null)
            {
                int port = 0;
                Int32.TryParse(txtPort.Text, out port);
                if (port == 0)
                {
                    MessageBox.Show("Enter a port...", "No port", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                start = DateTime.Now;

                server = new SimpleTcpServer().Start(port);
                server.Delimiter = (byte)'\n';
                server.DelimiterDataReceived += Server_DelimiterDataReceived;
                server.ClientConnected += (s, ev) => { AppendMessage("New Client connected."); };
                server.ClientDisconnected += (s, ev) => { AppendMessage("Client disconnected."); };

                AppendMessage($"Server started on port {port}");

                btnStart.Text = "Stop";
            }
            else
            {
                server.Stop();
                server = null;

                AppendMessage("Server stopped.");

                btnStart.Text = "Start";
            }

        }

        private void Server_DelimiterDataReceived(object sender, SimpleTCP.Message e)
        {
            if (string.IsNullOrWhiteSpace(e.MessageString))
            {
                AppendMessage("Empty message received.");
                return;
            }

            string message = e.MessageString.Trim();

            if (!message.Contains('?'))
            {
                AppendMessage($"Message does not appear to have a query in it: '{message}'.");
                return;
            }

            message = message.Substring(message.IndexOf('?'));

            string reply = null;
            var dif = DateTime.Now - start;

            switch (message)
            {
                case "?Q100":
                    AppendMessage("?Q100 - Machine Serial Number.");
                    reply = $"SERIAL NUMBER, 1234567";
                    break;
                case "?Q101":
                    AppendMessage("?Q101 - Control Software Version.");
                    reply = $"SOFTWARE VERSION, 100.17.000.2037";
                    break;
                case "?Q102":
                    AppendMessage("?Q102 - Machine Model Number.");
                    reply = $"MODEL, CSMD-G2";
                    break;
                case "?Q104":
                    AppendMessage("?Q104 - Mode (LIST PROG, MDI, etc.).");
                    reply = $"MODE, ZERO";
                    break;
                case "?Q200":
                    AppendMessage("?Q200 - Tool Changes (total).");
                    reply = $"TOOL CHANGES, {toolChanges}";
                    break;
                case "?Q201":
                    AppendMessage("?Q201 - Tool Number in use.");
                    reply = $"USING TOOL, {txtTool.Value}";
                    break;
                case "?Q300":
                    AppendMessage("?Q300 - Power-on Time (total).");
                    reply = $"P.O. TIME, {dif.TotalHours:00000}:{dif.Minutes}:{dif.Seconds}";
                    break;
                case "?Q301":
                    AppendMessage("?Q301 - Motion Time (total).");
                    reply = $"C.S. TIME, {dif.TotalHours:00000}:{dif.Minutes}:{dif.Seconds}";
                    break;
                case "?Q303":
                    AppendMessage("?Q303 - Last Cycle Time.");
                    reply = $"LAST CYCLE, 00000:00:13";
                    break;
                case "?Q304":
                    AppendMessage("?Q304 - Previous Cycle Time.");
                    reply = $"PREV CYCLE, 00000:00:01";
                    break;
                case "?Q402":
                    AppendMessage("?Q402 - M30 Parts Counter #1 (resettable at control).");
                    reply = $"M30 #1, 380";
                    break;
                case "?Q403":
                    AppendMessage("?Q403 - M30 Parts Counter #2 (resettable at control).");
                    reply = $"M30 #2, 380";
                    break;
                case "?Q500":
                    AppendMessage("?Q500 - Three-in-one (PROGRAM, Oxxxxx, STATUS, PARTS, xxxxx.");
                    reply = $"PROGRAM, MDI, IDLE, PARTS, 380";
                    break;
                default:
                    AppendMessage($"Unknown message: '{message}'");
                    break;
            }

            e.Reply($"{reply}\r");
            AppendMessage(reply);
        }

        private void AppendMessage(string message)
        {
            message = $"{Environment.NewLine}[{DateTime.Now:HH:mm:ss}] {message}";

            if (txtMessages.InvokeRequired)
            {
                txtMessages.Invoke(new Action(() =>
                {
                    txtMessages.AppendText(message);
                }));
            }
            else
            {
                txtMessages.AppendText(message);
            }
        }

        private void txtTool_ValueChanged(object sender, EventArgs e)
        {
            toolChanges += 1;
        }
    }
}
