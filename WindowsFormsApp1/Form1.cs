using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //工作线程回调，将要执行的代码放在此函数里
            this.backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            //当进度改变时回调
            this.backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            //当完成时回调
            this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            //此属性必须设置，否则读取不到进度
            this.backgroundWorker1.WorkerReportsProgress = true;
        }
        /*backgroundworker控件方法
         */
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("导入完成");
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            OracleConnection conn = OracleConn(PublicValue.str);
            //创建错误日志
            if (!File.Exists(PublicValue.FileRPath + "\\错误转化日志.txt"))
                File.Create(PublicValue.FileRPath + "\\错误转化日志.txt").Close();
            StreamWriter sw = new StreamWriter(PublicValue.FileRPath + "\\错误转化日志.txt", true);
            try
            {   conn.Open();
             //连接数据库
             String s_sql = @"select * from " + PublicValue.str[1];
             OracleCommand command = new OracleCommand(s_sql, conn);
             OracleDataReader reader = command.ExecuteReader();
                //当读取到列时,给文件路径和文件名称赋值
                while (reader.Read())
                {
                    PublicValue.FileName = reader.GetValue(2).ToString();
                    PublicValue.FilePath2 =PublicValue.FileRPath+"\\"+ reader.GetValue(4).ToString();
                    PublicValue.FilePath1 = reader.GetValue(3).ToString().Replace("/", "\\");
                    //如果原文件不存在
                    if (!File.Exists(PublicValue.FilePath1))
                    {
                        sw.Write(System.DateTime.Now + ":" + PublicValue.FilePath1 + "不存在\r\n");
                    }
                    else
                    {
                        //创建拷贝路径，拷贝文件
                        Directory.CreateDirectory(PublicValue.FilePath2);
                        File.Copy(PublicValue.FilePath1, PublicValue.FilePath2 + "\\" +PublicValue.FileName);
                    }
                }
                sw.Close();
                conn.Close();
                
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message,"导入失败");
                sw.Close();
                conn.Close();
            }

        }
        /*Oracle数据库连接
         * PublicValue.str[0] = this.ser_name.Text;
         * PublicValue.str[1] = this.t_name.Text;
         * PublicValue.str[2] = this.u_name.Text;
         * PublicValue.str[3] = this.password.Text;
         * PublicValue.str[4] = this.ip.Text;
         */
        public OracleConnection OracleConn(String[] str)
        {
            String connString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST="+
                str[4]+")(PORT=1521))(CONNECT_DATA=(SERVICE_NAME = "+str[0]
                +")));User Id="+str[2]+";Password="+str[3]+";";
            OracleConnection conn = new OracleConnection(connString);
            return conn;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.ser_name.Text = "orcl";
            this.t_name.Text = "SYQCSDJ";
            this.u_name.Text = "BDCSXKCS0";
            this.password.Text = "BDCSXKCS0";
            this.ip.Text = "192.168.131.136";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //选择文件转移后根目录
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PublicValue.FileRPath = dialog.SelectedPath;
            }
            //Oracle连接字符串赋值
            PublicValue.str[0] = this.ser_name.Text;
            PublicValue.str[1] = this.t_name.Text;
            PublicValue.str[2] = this.u_name.Text;
            PublicValue.str[3] = this.password.Text;
            PublicValue.str[4] = this.ip.Text;
            //转到后台运行
            backgroundWorker1.RunWorkerAsync();
        }

    }
}
