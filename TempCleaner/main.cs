using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace TempCleaner
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tempFolderPath = Path.GetTempPath();

            try
            {
                long previousFolderSize = CalculateFolderSize(tempFolderPath);
                CleanTempFolder(tempFolderPath);
                long currentFolderSize = CalculateFolderSize(tempFolderPath);
                long freedUpSpace = previousFolderSize - currentFolderSize;
                string message = "이전 폴더 용량: " + FormatBytes(previousFolderSize) + "\n";
                message += "현재 폴더 용량: " + FormatBytes(currentFolderSize) + "\n";
                message += "확보한 용량: " + FormatBytes(freedUpSpace);

                MessageBox.Show(message, "청소 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Temp 폴더 청소 중 오류가 발생했습니다:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.Exit(); 
        }
        private void CleanTempFolder(string folderPath)
        {
            DirectoryInfo tempDir = new DirectoryInfo(folderPath);
            foreach (FileInfo file in tempDir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (IOException ex)
                {
                    Console.WriteLine("파일 삭제 중 오류가 발생했습니다: " + ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("파일 삭제 중 접근 권한이 거부되었습니다: " + ex.Message);
                }
            }

            foreach (DirectoryInfo subDir in tempDir.GetDirectories())
            {
                if (!IsFolderInUse(subDir))
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("폴더 삭제 중 오류가 발생했습니다: " + ex.Message);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine("폴더 삭제 중 접근 권한이 거부되었습니다: " + ex.Message);
                    }
                }
            }
        }
        private bool IsFolderInUse(DirectoryInfo folder)
        {
            try
            {
                using (FileStream fs = new FileStream(
                    Path.Combine(folder.FullName, Guid.NewGuid().ToString("N")), FileMode.CreateNew))
                {
                    fs.Close();
                    File.Delete(fs.Name);
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
        private long CalculateFolderSize(string folderPath)
        {
            long folderSize = 0;

            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                folderSize += file.Length;
            }

            foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
            {
                folderSize += CalculateFolderSize(subDir.FullName);
            }

            return folderSize;
        }
        private string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);
            foreach (string order in orders)
            {
                if (bytes > max)
                {
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
                }
                max /= scale;
            }
            return "0 Bytes";
        }
    }
}
