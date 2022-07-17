using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using WpfLibrary1;
using MaterialDesignThemes.Wpf;

namespace BatchReplacement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        FileInfo singleFile = null;
        //选择文件夹时所有的后缀名
        List<string> suffixList=new List<string>();
        //选择的文件夹中找到的所有的文本文件
        List<FileInfo> fileList = new List<FileInfo>();

        //防呆设计
        //决定是否能够进行转换，加这个值的目的就是防止在选择文件夹后预览成功，又重新加载了一次文件夹，导致预览的和要转移的对不上，对不上倒还好，转换错了就是问题
        bool canConvert = false;
        //源字符
        string? sourceStr = null;
        //目的字符
        string? targetStr = null;
        //选择的是选择文件还是选择文件夹
        enum SelectedFunction
        {
            NONE,
            FILE,
            FOLDER
        }
        //所选择的方式
        SelectedFunction selectedFunction = SelectedFunction.NONE;

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            var result=openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.SelectedFilePathTextBox.Text = openFileDialog.FileName;
            }
            selectedFunction = SelectedFunction.FILE;
        }
        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new FolderBrowserDialog();
            var result=openFolderDialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                this.SelectedFolderPathTextBox.Text = openFolderDialog.SelectedPath;
                DirectoryInfo fileDirectory = new DirectoryInfo(openFolderDialog.SelectedPath);
                List<FileInfo> searchedFiles = new List<FileInfo>();
                SearchChildernFile(fileDirectory,ref searchedFiles);
                List<FileInfo> validFiles=GetVaildFiles(searchedFiles);
                List<string> validSuffix = GetFilesSuffix(validFiles);
                string s=Utils.ListToString(validFiles);
                this.ResultTextBlock.Text = s;
                fileList = validFiles;
                suffixList=validSuffix;
                suffixList.Insert(0, "全部");
                this.SuffixComboBox.ItemsSource = suffixList;
            }
            selectedFunction=SelectedFunction.FOLDER;
        }
        private void SearchChildernFile(DirectoryInfo fileDirectory, ref List<FileInfo> childernFiles)
        {
            FileInfo[] searchedFiles = fileDirectory.GetFiles();
            foreach (FileInfo searchedFile in searchedFiles)
            {
                childernFiles.Add(searchedFile);
            }
            DirectoryInfo[] folderDirectory=fileDirectory.GetDirectories();
            foreach(DirectoryInfo directoryInfo in folderDirectory)
            {
                SearchChildernFile(directoryInfo, ref childernFiles);
            }
        }
        private List<FileInfo> GetVaildFiles(List<FileInfo> searchedFiles)
        {
            if(searchedFiles.Count == 0)
            {
                return new List<FileInfo>();
            }
            List<FileInfo> validFiles = new List<FileInfo>();
            foreach(var myfile in searchedFiles)
            {
                if (!Utils.IsBinary(myfile.FullName))
                {
                    validFiles.Add(myfile);
                }
                
            }
            return validFiles;
        }
        private string GetSingleFileSuffix(string fileName)
        {
            string regex = @"\.[a-z|A-Z]*$";
            string res = Regex.Match(fileName, regex).Value;
            return res;
        }
        private List<string> GetFilesSuffix(List<FileInfo> files)
        {
            List<string> result = new List<string>();
            foreach (var file in files)
            {
                string suffix = GetSingleFileSuffix(file.Name);
                if (suffix != null && suffix != "" && !result.Contains(suffix))
                {
                    result.Add(suffix);
                }
            }
            return result;
        }
        

        private void ComboBoxSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SuffixComboBox.SelectedIndex == -1)
            {
                return;
            }
            if (SuffixComboBox.SelectedIndex == 0)
            {
                this.ResultTextBlock.Text = Utils.ListToString(fileList);
                return;
            }
            string s = suffixList[SuffixComboBox.SelectedIndex];
            List<FileInfo> res = new List<FileInfo>();
            foreach(FileInfo f in fileList)
            {
                if (f.Name.EndsWith(s))
                {
                    res.Add(f);
                }
            }
            string str = Utils.ListToString(res);
            this.ResultTextBlock.Text = str;
            //每次下拉栏变化的时候，就将转换的值转成false
            canConvert=false;
        }

        private void ConvertButtonClick(object sender, RoutedEventArgs e)
        {
            if (singleFile == null && fileList.Count == 0)
            {
                TipMessage.Text = "请先选择文件或者文件夹！";
                MyDialog.IsOpen = true;
                return;
            }
            if (SuffixComboBox.SelectedIndex <= 0)
            {
                TipMessage.Text = "请选择要转换的文件后缀名！";
                MyDialog.IsOpen = true;
                return;
            }
            if (String.IsNullOrEmpty(SourceCharacterTextBox.Text))
            {
                TipMessage.Text = "请输入待转换的文字！";
                MyDialog.IsOpen = true;
                return;
            }
            if (String.IsNullOrEmpty(TargetCharacterTextBox.Text))
            {
                TipMessage.Text = "请输入转换后的文字！";
                MyDialog.IsOpen = true;
                return;
            }
            ConvertFileCharacter();
        }

        private void PreviewButtonClick(object sender, RoutedEventArgs e)
        {
            if (singleFile == null && fileList.Count == 0)
            {
                TipMessage.Text = "请先选择文件或者文件夹！";
                MyDialog.IsOpen = true;
                return;
            }
            if (SuffixComboBox.SelectedIndex <= 0)
            {
                TipMessage.Text = "请选择要转换的文件后缀名！";
                MyDialog.IsOpen = true;
                return;
            }
            if (String.IsNullOrEmpty(SourceCharacterTextBox.Text))
            {
                TipMessage.Text = "请输入待转换的文字！";
                MyDialog.IsOpen = true;
                return;
            }
            if (String.IsNullOrEmpty(TargetCharacterTextBox.Text))
            {
                TipMessage.Text = "请输入转换后的文字！";
                MyDialog.IsOpen = true;
                return;
            }

            PreviewConvert();
        }

        private void PreviewConvert()
        {
            string suffix = suffixList[SuffixComboBox.SelectedIndex];
            string source = SourceCharacterTextBox.Text;
            string target = TargetCharacterTextBox.Text;
            ConsoleTitle.Text = "替换结果：";
            this.ResultTextBlock.Text = "";
            foreach (FileInfo f in fileList)
            {
                if (f.Name.EndsWith(suffix))
                {
                    string text = File.ReadAllText(f.FullName);
                    Match match = Regex.Match(text, source);
                    if (match.Success)
                    {
                        MatchCollection matches = Regex.Matches(text, source);
                        this.ResultTextBlock.Text += $"\n在{f.FullName}文件中找到{matches.Count}个匹配{source}的项：";
                        foreach (Match m in matches)
                        {
                            this.ResultTextBlock.Text += $"\n{m.Value} 在文件{f.FullName}中的{m.Index}位置，长度为{m.Length}.";
                        }
                    }
                }
            }
            this.ResultTextBlock.Text += "\n预览完成！";
        }

        private void ConvertFileCharacter()
        {
            string suffix = suffixList[SuffixComboBox.SelectedIndex];
            string source = SourceCharacterTextBox.Text;
            string target=TargetCharacterTextBox.Text;
            ConsoleTitle.Text = "替换结果：";
            this.ResultTextBlock.Text = "";
            foreach(FileInfo f in fileList)
            {
                if (f.Name.EndsWith(suffix))
                {
                    string text=File.ReadAllText(f.FullName);
                    Match match=Regex.Match(text,source);
                    if (match.Success)
                    {
                        //this.ResultTextBlock.Text += $"\n在{f.FullName}文件中找到{match}";
                        //while (match.Success)
                        //{
                        //    this.ResultTextBlock.Text += $"\n{match.Index}位置上的";
                        //    match=match.NextMatch();
                        //}
                        MatchCollection matches = Regex.Matches(text, source);
                        this.ResultTextBlock.Text += $"\n在{f.FullName}文件中找到{matches.Count}个匹配{source}的项：";
                        foreach (Match m in matches)
                        {
                            this.ResultTextBlock.Text += $"\n{m.Value} 在文件{f.FullName}中的{m.Index}位置，长度为{m.Length}.";
                        }
                        string newText = Regex.Replace(text, source, target);
                        this.ResultTextBlock.Text += $"\n{f.FullName}文件中的关键字{source}已经被替换成了{target}.";
                        File.WriteAllText(f.FullName, newText);
                    }
                }
            }
            this.ResultTextBlock.Text += "\n替换完成！";
        }

    }
}
