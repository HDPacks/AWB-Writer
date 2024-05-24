using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Library.IO;

namespace AWB_Writer
{    
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnOpenInput_Click(object sender, EventArgs e)
        {
            var folderOpen = new FolderBrowserDialog();
            folderOpen.Description = "Select path to the folder containing the files you want to import.";
            folderOpen.ShowDialog();
            txtInput.Text = folderOpen.SelectedPath;
        }

        private void btnOpenOutput_Click(object sender, EventArgs e)
        {
            var folderSave = new SaveFileDialog();
            folderSave.Title = "Select path to save the AWB file.";
            folderSave.DefaultExt = ".awb";
            folderSave.Filter = "AWB File (*.awb)|*.awb";
            folderSave.ShowDialog();
            txtOutput.Text = folderSave.FileName;
        }

        private int nullCalc(int size)
        {
            int mod = size % 32;
            if (mod == 0)
                return 0;
            else
                return 32 - mod;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!Path.IsPathRooted(txtInput.Text) || !Path.IsPathRooted(txtOutput.Text))
                MessageBox.Show("Please enter a valid paths.");
            else
            {
                DirectoryInfo dir = new DirectoryInfo(txtInput.Text);
                var files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                MemoryStream stream = new MemoryStream();
                var writer = new FileWriter(stream);
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                writer.WriteSignature("AFS2");
                writer.Write(263170);
                writer.Write(files.Length);
                writer.Write(32);
                for (int i = 0; i < files.Length; i++)
                    writer.Write(i);

                int[] offsetArr = new int[files.Length + 1];                
                int offset = (int)writer.Position + files.Length * 4 + 4;
                int lastMod = nullCalc(offset);
                offsetArr[0] = offset + lastMod;
                writer.Write(offset);

                int count = 1;
                foreach (FileInfo file in files)
                {                    
                    offset += (int)file.Length + lastMod;
                    lastMod = nullCalc(offset);
                    offsetArr[count] = offset + lastMod;
                    writer.Write(offset);
                    count++;
                }

                count = 0;
                foreach (FileInfo file in files)
                {
                    byte[] buffer = File.ReadAllBytes(file.FullName);
                    writer.SeekBegin(offsetArr[count]);
                    writer.Write(buffer);
                    count++;
                }
                File.WriteAllBytes(txtOutput.Text, stream.ToArray());
                stream.Dispose();
                MessageBox.Show("Finished!");
            }
        }
    }
}
