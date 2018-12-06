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
            this.Visible=true;
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
             int [] F_fail = new int [PublicValue.FileBz];
            int i = 0;
            OracleConnection conn = OracleConn(PublicValue.str);
            //创建错误日志
            //if (!File.Exists(PublicValue.FileRPath + "\\错误转化日志.txt"))
            //    File.Create(PublicValue.FileRPath + "\\错误转化日志.txt").Close();
            //StreamWriter sw = new StreamWriter(PublicValue.FileRPath + "\\错误转化日志.txt", true);
            try
            {
                conn.Open();
                //连接数据库
                String s_sql = @"select * from " + PublicValue.str[1]+" WHERE ZT = 0";
                OracleCommand command = new OracleCommand(s_sql, conn);
                OracleDataReader reader = command.ExecuteReader();
                //当读取到列时,给文件路径和文件名称赋值
                while (reader.Read())
                {
                    PublicValue.FileName = reader.GetValue(2).ToString();
                    PublicValue.FilePath2 = PublicValue.FileRPath + "\\" + reader.GetValue(4).ToString();
                    PublicValue.FilePath1 = reader.GetValue(3).ToString().Replace("/", "\\");
                    //如果原文件不存在 
                    if (!File.Exists(PublicValue.FilePath1))
                    {
                        //sw.Write(System.DateTime.Now +PublicValue.FilePath1 + "不存在。\r\n");
                        int.TryParse(reader.GetValue(0).ToString(), out F_fail[i]);  i++;
                    }
                    else
                    {
                        //判断文件名是否正确
                        if (string.IsNullOrEmpty(PublicValue.FileName))
                        {
                            //sw.Write(System.DateTime.Now+"XH为：" + reader.GetValue(0) + "的WJM为空。\r\n");
                            int.TryParse(reader.GetValue(0).ToString(), out F_fail[i]);
                            i++;
                        }
                        //创建拷贝路径，拷贝文件
                        else {
                            Directory.CreateDirectory(PublicValue.FilePath2);
                            File.Copy(PublicValue.FilePath1, PublicValue.FilePath2 + "\\" + PublicValue.FileName, true);
                        }
                    }
                }
                //共有m个未迁移
                int m = i;
                //结束读取，关闭和数据的连接
                //sw.Close();
                conn.Close();
                
                //开始向数据库标记未导出行
                OracleConnection conn1 = OracleConn(PublicValue.str);
                try
                {
                    conn1.Open();
                    string s_sql1 = "";
                    for (i = 0; i <m; i++)
                    {
                        s_sql1 = @"Update " + PublicValue.str[1] + " SET ZT =1 WHERE XH = " + F_fail[i];
                        OracleCommand command1 = new OracleCommand(s_sql1, conn1);
                        int count = command1.ExecuteNonQuery();
                       
                    }
                    
                    conn1.Close();
                    MessageBox.Show("共有" + PublicValue.FileBz + "个文件，其中" + m + "个未成功导入");
                }
                catch(Exception exception)
                {
                    MessageBox.Show(exception.Message, "更新状态失败");
                    conn1.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "导入失败");
                //sw.Close();
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
            //统计有多少行
            Table_Count();
            MessageBox.Show("开始后台运行");
            this.Visible = false;
            //转到后台运行
            backgroundWorker1.RunWorkerAsync();
        }
        //查询数据库的行数
        public void  Table_Count()
        {
            OracleConnection conn1 = OracleConn(PublicValue.str);
            conn1.Open();
            String s_sql2 = @"select count(*) from " + PublicValue.str[1]+" WHERE ZT=0";
            OracleCommand command = new OracleCommand(s_sql2, conn1);
            OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int.TryParse(reader.GetValue(0).ToString(),out PublicValue.FileBz);
            }
            reader.Close();
            conn1.Close();
        }

    }
}
