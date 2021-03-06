﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SecureTree
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region TreeView树形显示磁盘下文件夹
        /// <summary>
        /// IconIndexs类 对应ImageList中5张图片的序列
        /// </summary>
        private class IconIndexes
        {
            public const int MyComputer = 0;      //我的电脑
            public const int ClosedFolder = 1;    //文件夹关闭
            public const int OpenFolder = 2;      //文件夹打开
            public const int FixedDrive = 3;      //磁盘盘符
            public const int MyDocuments = 4;     //我的文档
        }

        /// <summary>
        /// 窗体加载Load事件 初始化
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            //实例化TreeNode类 TreeNode(string text,int imageIndex,int selectImageIndex)            
            TreeNode rootNode = new TreeNode("我的电脑",
                IconIndexes.MyComputer, IconIndexes.MyComputer);  //载入显示 选择显示
            rootNode.Tag = "我的电脑";                            //树节点数据
            rootNode.Text = "我的电脑";                           //树节点标签内容
            this.directoryTree.Nodes.Add(rootNode);               //树中添加根目录

            //显示MyDocuments(我的文档)结点
            var myDocuments = Environment.GetFolderPath           //获取计算机我的文档文件夹
                (Environment.SpecialFolder.MyDocuments);
            TreeNode DocNode = new TreeNode(myDocuments);
            DocNode.Tag = "我的文档";                            //设置结点名称
            DocNode.Text = "我的文档";
            DocNode.ImageIndex = IconIndexes.MyDocuments;         //设置获取结点显示图片
            DocNode.SelectedImageIndex = IconIndexes.MyDocuments; //设置选择显示图片
            rootNode.Nodes.Add(DocNode);                          //rootNode目录下加载节点
            DocNode.Nodes.Add("");

            //循环遍历计算机所有逻辑驱动器名称(盘符)
            foreach (string drive in Environment.GetLogicalDrives())
            {
                //实例化DriveInfo对象 命名空间System.IO
                var dir = new DriveInfo(drive);
                switch (dir.DriveType)           //判断驱动器类型
                {
                    case DriveType.Fixed:        //仅取固定磁盘盘符 Removable-U盘 
                        {
                            //Split仅获取盘符字母
                            TreeNode tNode = new TreeNode(dir.Name.Split(':')[0]);
                            tNode.Name = dir.Name;
                            tNode.Tag = tNode.Name;
                            tNode.ImageIndex = IconIndexes.FixedDrive;         //设置获取结点显示图片
                            tNode.SelectedImageIndex = IconIndexes.FixedDrive; //设置选择显示图片
                            directoryTree.Nodes.Add(tNode);                    //加载驱动节点
                            tNode.Nodes.Add("");                          
                        }
                        break;
                }
            }
            rootNode.Expand();                  //展开树状视图

            //调用SetListView()函数初始化设置ListView
            SetListView();
        }

        /// <summary>
        /// 在结点展开后发生 展开子结点
        /// </summary>
        private void directoryTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.Expand();
        }

        /// <summary>
        /// 在将要展开结点时发生 加载子结点
        /// </summary>
        private void directoryTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeViewItems.Add(e.Node);
        }

        /// <summary>
        /// 自定义类TreeViewItems 调用其Add(TreeNode e)方法加载子目录
        /// </summary>
        public static class TreeViewItems
        {
            public static void Add(TreeNode e)
            {
                //try..catch异常处理
                try
                {
                    //判断"我的电脑"Tag 上面加载的该结点没指定其路径
                    if (e.Tag.ToString() != "我的电脑")
                    {
                        e.Nodes.Clear();                               //清除空节点再加载子节点
                        TreeNode tNode = e;                            //获取选中\展开\折叠结点
                        string path = tNode.Name;                      //路径  

                        //获取"我的文档"路径
                        if (e.Tag.ToString() == "我的文档")
                        {
                            path = Environment.GetFolderPath           //获取计算机我的文档文件夹
                                (Environment.SpecialFolder.MyDocuments);
                        }

                        //获取指定目录中的子目录名称并加载结点
                        string[] dics = Directory.GetDirectories(path);
                        foreach (string dic in dics)
                        {
                            TreeNode subNode = new TreeNode(new DirectoryInfo(dic).Name); //实例化
                            subNode.Name = new DirectoryInfo(dic).FullName;               //完整目录
                            subNode.Tag = subNode.Name;
                            subNode.ImageIndex = IconIndexes.ClosedFolder;       //设置获取节点显示图片
                            subNode.SelectedImageIndex = IconIndexes.OpenFolder; //设置选择节点显示图片
                            tNode.Nodes.Add(subNode);
                            subNode.Nodes.Add("");                               //加载空节点 实现+号
                        }
                    }
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);                   //异常处理
                }
            }
        }
        #endregion

        /// <summary>
        /// 自定义函数设置ListView控件初始属性
        /// </summary>
        private void SetListView()
        {
            //行和列是否显示网格线
            this.filesList.GridLines = false;
            //显示方式(注意View是Details详细显示)
            this.filesList.View = View.Details;
            //是否可编辑
            this.filesList.LabelEdit = true;
            //没有足够的空间显示时,是否添加滚动条
            this.filesList.Scrollable = true;
            //对表头进行设置
            this.filesList.HeaderStyle = ColumnHeaderStyle.Clickable;
            //是否可以选择行
            this.filesList.FullRowSelect = true;

            //设置listView列标题头 宽度为9/13 2/13 2/13 
            //其中设置标题头自动适应宽度,-1根据内容设置宽度,-2根据标题设置宽度
            this.filesList.Columns.Add("名称", 9 * filesList.Width / 13);   
            this.filesList.Columns.Add("大小", 2 * filesList.Width / 13);
            this.filesList.Columns.Add("类型", 2 * filesList.Width / 13);
        }

        #region ListView显示选中文件夹中文件内容
        /// <summary>
        /// 获取节点的路径:递归调用产生节点对应文件夹的路径
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetPathFromNode(TreeNode node)
        {
            //注意:树形控件中我只赋值Tag\Name,使用Text时赋值即可使用
            if (node.Parent == null)
            {
                return node.Name;
            }
            //Path.Combine组合产生路径 如 Path.Combine("A","B")则生成"A\\B"
            return Path.Combine(GetPathFromNode(node.Parent), node.Name);
        }

        /// <summary>
        /// 更改选定内容后发生 后去当前节点名字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void directoryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                //定义变量
                long length;                        //文件大小
                string path;                        //文件路径
                TreeNode clickedNode = e.Node;      //获取当前选中结点

                //移除ListView所有项 
                this.filesList.Items.Clear();       

                //获取路径赋值path              
                if (clickedNode.Tag.ToString() == "我的文档")
                {
                    //获取计算机我的文档文件夹
                    path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    //通过自定义函数GetPathFromNode获取结点路径
                    path = GetPathFromNode(clickedNode);
                }

                //由于"我的电脑"为空结点,无需处理,否则会出现路径获取错误或没有找到"我的电脑"路径
                if (clickedNode.Tag.ToString() != "我的电脑")
                {
                    //数据更新 UI暂时挂起直到EndUpdate绘制控件,可以有效避免闪烁并大大提高加载速度
                    this.filesList.BeginUpdate();
                    //实例目录与子目录
                    DirectoryInfo dir = new DirectoryInfo(path);
                    //获取当前目录文件列表
                    FileInfo[] fileInfo = dir.GetFiles();
                    //循环输出获取文件信息
                    for (int i = 0; i < fileInfo.Length; i++)
                    {
                        ListViewItem listItem = new ListViewItem();
                        //listItem.SubItems[0].Text = fileInfo[i].Name;             //文件名(方法二)
                        listItem.Text = "[" + (i + 1) + "] " + fileInfo[i].Name;    //显示文件名
                        listItem.ForeColor = Color.Blue;                            //设置行颜色

                        //length/1024转换为KB字节数整数值 Ceiling返回最小整数值 Divide除法
                        length = fileInfo[i].Length;                                //获取当前文件大小字节
                        listItem.SubItems.Add(Math.Ceiling(decimal.Divide(length, 1024)) + " KB");

                        //获取文件最后访问时间
                        //listItem.SubItems.Add(fileInfo[i].LastWriteTime.ToString());

                        //获取文件扩展名时可用Substring除去点 否则显示".txt文件"
                        listItem.SubItems.Add(fileInfo[i].Extension + "文件");
                        //加载数据至filesList
                        this.filesList.Items.Add(listItem);
                    }
                    //结束数据处理,UI界面一次性绘制 否则可能出现闪动情况
                    this.filesList.EndUpdate();  
                }
            }
            catch (Exception msg)  //异常处理
            {
                MessageBox.Show(msg.Message);
            }
            
        }
        #endregion
    }
}
