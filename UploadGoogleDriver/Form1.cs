using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace UploadGoogleDriver
{
    public partial class Form1 : Form
    {
        GoogleDriveApi _api = new GoogleDriveApi("", "", "giaiphapmmo_google_api");
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnKetNoiTaiKhoanGoogle_Click(object sender, EventArgs e)
        {
            await _api.LoginAsync();

            if (_api.CheckLogin())
                txtNhatKy.AppendText("Đã đăng nhập\r\n");
        }

        private void btnChonTep_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if(open.ShowDialog() == DialogResult.OK)
                txtDuongDanTep.Text = open.FileName;
        }

        private async void btnTaiLen_Click(object sender, EventArgs e)
        {
            await _api.LoginAsync();
            if (_api.CheckLogin())
            {
                string folderId = await _api.CreateFolderIfNotExistAync("Test Code Tai Tep Len GG Drive");
                string fileId = await _api.UploadFileAsync(txtDuongDanTep.Text, CancellationToken.None, folderId);
                string linkFile = _api.GetLinkFile(fileId);
                txtNhatKy.AppendText($"Tải lên thành công, đường dẫn tệp:\r\n{linkFile}\r\n");
            }
            else
            {
                txtNhatKy.AppendText("Chưa kết nối tài khoản\r\n");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtDuongDanTep.Text = Path.Combine(Application.StartupPath, "Tai Len.txt");
        }
    }
}
