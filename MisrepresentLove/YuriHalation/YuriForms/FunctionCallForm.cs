using System;
using System.Windows.Forms;
using Yuri.YuriHalation.ScriptPackage;

namespace Yuri.YuriHalation.YuriForms
{
    public partial class FunctionCallForm : Form
    {
        /// <summary>
        /// 是否编辑模式
        /// </summary>
        private bool isEditing;

        public FunctionCallForm(bool isEdit, string funCallName = null, string args = null)
        {
            InitializeComponent();
            this.isEditing = isEdit;
            foreach (var sp in Halation.project.GetScene())
            {
                foreach (var fp in sp.GetFunc())
                {
                    this.comboBox1.Items.Add(fp.functionCallName);
                }
            }
            if (this.isEditing)
            {
                this.comboBox1.SelectedItem = funCallName;
                string[] callsignItems = args.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (callsignItems.Length > 0)
                {
                    for (int i = 0; i < callsignItems.Length; i++)
                    {
                        var callitem = callsignItems[i];
                        var terms = callitem.Split(':');
                        var termVal = terms[1];
                        this.argsGridDataView.Rows[i].Cells[1].Value = termVal;
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedIndex != -1)
            {
                this.button1.Enabled = true;
            }
            else
            {
                this.button1.Enabled = false;
                return;
            }
            string fullName = this.comboBox1.SelectedItem.ToString();
            string funcName = fullName.Split('@')[0];
            string sceneName = fullName.Split('@')[1];
            FunctionPackage fp = Halation.project.GetScene(sceneName).GetFunc(funcName);
            // 处理参数列表
            this.argsGridDataView.Rows.Clear();
            if (fp.Argv.Count > 0)
            {
                for (int i = 0; i < fp.Argv.Count; i++)
                {
                    this.argsGridDataView.Rows.Add();
                    this.argsGridDataView.Rows[i].Cells[0].Value = fp.Argv[i];
                    this.argsGridDataView.Rows[i].Cells[1].Value = "\"NULL\"";
                }
            }
        }

        /// <summary>
        /// 按钮：确定
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // 检查空值并处理参数
            string argStr = String.Empty;
            for (int i = 0; i < this.argsGridDataView.Rows.Count; i++)
            {
                if (this.argsGridDataView.Rows[i].Cells[1].Value == null ||
                    (string)(this.argsGridDataView.Rows[i].Cells[1].Value) == String.Empty)
                {
                    MessageBox.Show("请完整填写", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    argStr += String.Format(",{0}:{1}", this.argsGridDataView.Rows[i].Cells[0].Value.ToString(),
                        this.argsGridDataView.Rows[i].Cells[1].Value.ToString());
                }
            }
            if (argStr.Length > 0)
            {
                argStr = argStr.Substring(1);
            }
            // 提交给后台
            if (this.isEditing)
            {
                Halation.GetInstance().DashEditFuncall(this.comboBox1.SelectedItem.ToString(), argStr);
            }
            else
            {
                Halation.GetInstance().DashFuncall(this.comboBox1.SelectedItem.ToString(), argStr);
            }
            // 关闭
            this.Close();
        }

    }
}
