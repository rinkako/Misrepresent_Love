using System;
using System.Windows.Forms;

namespace Yuri.YuriHalation.YuriForms
{
    public partial class PicturesForm : Form
    {
        private bool isEditing;
        
        public PicturesForm(bool isEdit, string id = "0", string filename = "", string x = "0", string y = "0", string xscale = "100", string yscale = "100", string opacity = "100", string ro = "0")
        {
            InitializeComponent();
            this.numericUpDown3.Maximum = Halation.project.Config.GameViewPicturesCount - 1;
            this.isEditing = isEdit;
            if (isEdit)
            {
                this.textBox1.Text = filename;
                this.numericUpDown3.Value = Convert.ToInt32(id);
                this.numericUpDown1.Value = Convert.ToInt32(x);
                this.numericUpDown2.Value = Convert.ToInt32(y);
                this.numericUpDown4.Value = Convert.ToInt32(xscale);
                this.numericUpDown5.Value = Convert.ToInt32(yscale);
                this.numericUpDown6.Value = Convert.ToInt32(ro);
                this.numericUpDown7.Value = Convert.ToInt32(opacity);
            }
        }

        /// <summary>
        /// 获取或设置背景资源名
        /// </summary>
        public string GotFileName { get; set; }

        /// <summary>
        /// 按钮：选择图像
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            PicResourceForm prf = new PicResourceForm("选择显示图片", 0, this.isEditing ? this.textBox1.Text : null);
            prf.ShowDialog(this);
            this.textBox1.Text = this.GotFileName;
        }

        /// <summary>
        /// 按钮：确定
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text == String.Empty)
            {
                MessageBox.Show("请选择图片文件");
                return;
            }
            if (this.isEditing)
            {
                Halation.GetInstance().DashEditPicture(this.numericUpDown3.Value.ToString(),
                    this.textBox1.Text, this.numericUpDown1.Value.ToString(), this.numericUpDown2.Value.ToString(),
                    this.numericUpDown4.Value.ToString(), this.numericUpDown5.Value.ToString(),
                    this.numericUpDown7.Value.ToString(), this.numericUpDown6.Value.ToString());
            }
            else
            {
                Halation.GetInstance().DashPicture(this.numericUpDown3.Value.ToString(),
                    this.textBox1.Text, this.numericUpDown1.Value.ToString(), this.numericUpDown2.Value.ToString(),
                    this.numericUpDown4.Value.ToString(), this.numericUpDown5.Value.ToString(),
                    this.numericUpDown7.Value.ToString(), this.numericUpDown6.Value.ToString());
            }
            this.Close();
        }

        /// <summary>
        /// 按钮：居中
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            this.numericUpDown1.Value = (int) Math.Floor(Halation.project.Config.GameViewWindowWidth / 2.0);
            this.numericUpDown2.Value = (int) Math.Floor(Halation.project.Config.GameViewWindowHeight / 2.0);
        }

        /// <summary>
        /// 按钮：全透明
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            this.numericUpDown7.Value = 0;
        }
    }
}
