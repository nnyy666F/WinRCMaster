using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WinRCMaster
{
	public partial class Form1 : Form
	{
		private readonly string[] REG_KEY_PATHS =
		{
			@"*\shellex\ContextMenuHandlers",
			@"Directory\shellex\ContextMenuHandlers",
			@"Directory\Background\shellex\ContextMenuHandlers",
			@"Drive\shellex\ContextMenuHandlers",
			@"*\shell",
			@"Directory\shell",
			@"Directory\Background\shell",
			@"Drive\shell // 全局右键菜单",
			@"*\shellex\ContextMenuHandlers",
    
            // 文件夹相关
            @"Directory\shellex\ContextMenuHandlers",
			@"Directory\shell",
			@"Directory\Background\shellex\ContextMenuHandlers",
			@"Directory\Background\shell",

            // 驱动器相关
            @"Drive\shellex\ContextMenuHandlers",
			@"Drive\shell",

            // "新建"菜单
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Discardable\PostSetup\ShellNew",

            // 文件扩展名关联
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts",

            // 拖放操作
            @"SOFTWARE\Classes\*\shellex\DragDropHandlers",
			@"SOFTWARE\Classes\Directory\shellex\DragDropHandlers",

            // Shell 扩展
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved",

            // CLSID 类标识符
            //@"CLSID",
            //@"Software\Classes\CLSID",

            // 文件类型关联 (HKEY_CLASSES_ROOT)
            @"HKEY_CLASSES_ROOT\*\shellex\ContextMenuHandlers",
			@"HKEY_CLASSES_ROOT\Directory\shellex\ContextMenuHandlers",
			@"HKEY_CLASSES_ROOT\Drive\shellex\ContextMenuHandlers",
			@"HKEY_CLASSES_ROOT\AllFileSystemObjects\shellex\ContextMenuHandlers", // 所有文件系统对象
            @"HKEY_CLASSES_ROOT\Folder\shell",
			@"HKEY_CLASSES_ROOT\Folder\shellex\ContextMenuHandlers",

            // 当前用户 Shell 扩展
            @"HKEY_CURRENT_USER\Software\Classes\*\shell",
			@"HKEY_CURRENT_USER\Software\Classes\*\shellex\ContextMenuHandlers",
			@"HKEY_CURRENT_USER\Software\Classes\Directory\shell",
			@"HKEY_CURRENT_USER\Software\Classes\Directory\shellex\ContextMenuHandlers",
			@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts",
			@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Discardable\PostSetup\ShellNew"
		};

		private List<ContextMenuItem> contextMenuItems = new List<ContextMenuItem>();


		public Form1()
		{
			InitializeComponent();
			InitializeListView();
			this.Icon = new Icon("icon1.ico");
			this.Text = "WinRCMaster";
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			RefreshContextMenuItems();
		}

		private void InitializeListView()
		{
			listView1.View = View.Details;
			listView1.Columns.Add("菜单项名", 250);
			listView1.Columns.Add("图标路径", 250);
			listView1.Columns.Add("命令", 400);
			listView1.Columns.Add("注册表路径", 400);
			listView1.SmallImageList = new ImageList();
			listView1.SmallImageList.ImageSize = new Size(16, 16);
			listView1.ContextMenuStrip = CreateContextMenu();
		}

		private ContextMenuStrip CreateContextMenu()
		{
			ContextMenuStrip menu = new ContextMenuStrip();

			ToolStripMenuItem deleteItem = new ToolStripMenuItem("删除")
			{
				ShortcutKeys = Keys.Delete
			};
			deleteItem.Click += (s, e) => DeleteSelectedItem();

			ToolStripMenuItem showPathItem = new ToolStripMenuItem("显示路径");
			showPathItem.Click += (s, e) => ShowSelectedItemPath();

			ToolStripMenuItem executeItem = new ToolStripMenuItem("执行");
			executeItem.Click += (s, e) => ExecuteSelectedItem();

			menu.Items.Add(deleteItem);
			menu.Items.Add(showPathItem);
			menu.Items.Add(executeItem);

			return menu;
		}


		private void RefreshContextMenuItems()
		{
			contextMenuItems.Clear();
			ScanContextMenuRegistry();
			contextMenuItems = RemoveDuplicateRegistryPathItems(contextMenuItems);
			DisplayContextMenuItems();
		}

		private void ScanContextMenuRegistry()
		{
			foreach (string regKeyPath in REG_KEY_PATHS)
			{
				try
				{
					using (RegistryKey rootKey = Registry.ClassesRoot.OpenSubKey(regKeyPath))
					{
						if (rootKey != null)
						{
							ScanSubKeys(rootKey, regKeyPath);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"无法扫描注册表路径 {regKeyPath}: {ex.Message}");
				}
			}
		}

		private void ScanSubKeys(RegistryKey rootKey, string parentPath)
		{
			foreach (string subKeyName in rootKey.GetSubKeyNames())
			{
				try
				{
					using (RegistryKey subKey = rootKey.OpenSubKey(subKeyName))
					{
						if (subKey != null)
						{
							string command2 = GetCommandValue(subKey);
							//allCommandValues.Add(command2);
							string iconPath2 = GetIconPath(subKey);
							string registryPath2 = Path.Combine(parentPath, subKeyName);
							string Name_Menu2 = GetCommandValue1(subKey);

							string ExeIconPath2 = subKey.GetValue("Icon") as string ?? "" as string;

							if (string.IsNullOrEmpty(command2))
							{
								command2 = subKey.GetValue("command") as string ?? "" as string;
							}

							if (string.IsNullOrEmpty(command2))
							{
								continue;
							}

							ContextMenuItem newItem = new ContextMenuItem
							{
								Name = subKeyName,
								IconPath = iconPath2,
								Name_Menu = Name_Menu2,
								ExeIconPath = ExeIconPath2,
								Command = command2,
								RegistryPath = registryPath2,
							};

							if (!contextMenuItems.Any(x => x.RegistryPath == newItem.RegistryPath))
							{
								contextMenuItems.Add(newItem);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"扫描子键 {subKeyName} 出现异常: {ex.Message}");
				}
			}
		}

		private string GetCommandValue1(RegistryKey subKey)
		{
			string[] possibleKeys = { "Command", "", "(Default)" };
			foreach (var key in possibleKeys)
			{
				object value = subKey.GetValue(key);
				if (value != null)
				{
					Console.WriteLine(value.ToString());
					return value.ToString();
				}
			}
			return "";
		}
		private string GetCommandValue(RegistryKey subKey)
		{
			using (RegistryKey commandSubKey = subKey.OpenSubKey("command"))
			{
				if (commandSubKey != null)
				{
					object value = commandSubKey.GetValue("");
					if (value != null)
					{
						return value.ToString();
					}
				}
			}
			string[] possibleKeys = { "", "(Default)" };
			foreach (var key in possibleKeys)
			{
				object value = subKey.GetValue(key);
				if (value != null)
				{
					return value.ToString();
				}
			}
			return "";
		}

		private string GetIconPath(RegistryKey subKey)
		{
			object iconObject = subKey.GetValue("Icon") ?? subKey.GetValue("DefaultIcon");
			if (iconObject != null)
			{
				string iconPath = iconObject.ToString();
				int commaIndex = iconPath.IndexOf(',');
				return commaIndex > 0 ? iconPath.Substring(0, commaIndex) : iconPath;
			}
			return "";
		}

		private void DisplayContextMenuItems()
		{
			listView1.Items.Clear();
			foreach (var item in contextMenuItems)
			{
				ListViewItem listViewItem = new ListViewItem(item.Name)
				{
					Tag = item
				};

				listViewItem.SubItems.Add(item.Name_Menu);
				listViewItem.SubItems.Add(item.ExeIconPath);
				listViewItem.SubItems.Add(item.Command);
				listViewItem.SubItems.Add(item.RegistryPath);

				if (!string.IsNullOrEmpty(item.IconPath))
				{
					try
					{
						Icon icon = Icon.ExtractAssociatedIcon(item.IconPath);
						listView1.SmallImageList.Images.Add(icon);
						listViewItem.ImageIndex = listView1.SmallImageList.Images.Count - 1;
					}
					catch (Exception)
					{
						Console.WriteLine($"加载图标失败: {item.IconPath}");
					}
				}

				listView1.Items.Add(listViewItem);
			}
		}

		private void DeleteSelectedItem()
		{
			if (listView1.SelectedItems.Count > 0)
			{
				ContextMenuItem selectedItem = (ContextMenuItem)listView1.SelectedItems[0].Tag;
				try
				{
					Registry.ClassesRoot.DeleteSubKeyTree(selectedItem.RegistryPath);
					MessageBox.Show($"已删除注册表项: {selectedItem.RegistryPath}");
					RefreshContextMenuItems();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"删除失败: {ex.Message}");
				}
			}
		}

		private void ShowSelectedItemPath()
		{
			if (listView1.SelectedItems.Count > 0)
			{
				ContextMenuItem selectedItem = (ContextMenuItem)listView1.SelectedItems[0].Tag;

				using (Form pathDialog = new Form())
				{
					pathDialog.Text = "注册表路径详情";
					pathDialog.Size = new Size(500, 250);
					pathDialog.StartPosition = FormStartPosition.CenterParent;
					pathDialog.MinimumSize = new Size(500, 250);

					TextBox pathTextBox = new TextBox
					{
						Text = selectedItem.RegistryPath,
						Location = new Point(10, 20),
						Width = 460,
						Height = 60,
						ReadOnly = true,
						Multiline = true,
						ScrollBars = ScrollBars.Vertical
					};
					pathDialog.Controls.Add(pathTextBox);
					Label valueCountLabel = new Label
					{
						Text = "值的数量：未知",
						Location = new Point(10, 90),
						Width = 460
					};
					Label dataTypeLabel = new Label
					{
						Text = "数据类型：未知",
						Location = new Point(10, 120),
						Width = 460
					};
					pathDialog.Controls.Add(valueCountLabel);
					pathDialog.Controls.Add(dataTypeLabel);
					try
					{
						RegistryKey keyToNavigate = Registry.ClassesRoot.OpenSubKey(selectedItem.RegistryPath);
						if (keyToNavigate != null)
						{
							string[] valueNames = keyToNavigate.GetValueNames();
							valueCountLabel.Text = $"值的数量：{valueNames.Length}";							
							if (valueNames.Length > 0)
							{
								object firstValue = keyToNavigate.GetValue(valueNames[0]);
								dataTypeLabel.Text = $"数据类型：{firstValue.GetType().Name}";
							}
							else
							{
								dataTypeLabel.Text = "数据类型：无值";
							}
						}
					}
					catch (Exception ex)
					{
						valueCountLabel.Text = "获取信息失败：" + ex.Message;
						dataTypeLabel.Text = "获取信息失败：" + ex.Message;
					}
					Button copyButton = new Button
					{
						Text = "复制",
						Location = new Point(100, 160),
						Width = 80
					};
					pathDialog.Controls.Add(copyButton);
					Button cancelButton = new Button
					{
						Text = "取消",
						Location = new Point(300, 160),
						Width = 80
					};
					pathDialog.Controls.Add(cancelButton);
					copyButton.Click += (s, e) =>
					{
						Clipboard.SetText(selectedItem.RegistryPath);

						Label tipLabel = new Label
						{
							Text = "已复制到剪贴板！",
							Location = new Point(200, 190),
							AutoSize = true
						};
						pathDialog.Controls.Add(tipLabel);

						_ = Task.Run(async () =>
						{
							await Task.Delay(1000);
							pathDialog.Invoke(new Action(() => tipLabel.Visible = false));
						});
					};

					cancelButton.Click += (s, e) => pathDialog.Close();

					pathDialog.ShowDialog();
				}
			}
		}

		private void ExecuteSelectedItem()
		{
			if (listView1.SelectedItems.Count > 0)
			{
				ContextMenuItem selectedItem = (ContextMenuItem)listView1.SelectedItems[0].Tag;
				using (Form executeForm = new Form())
				{
					executeForm.Text = "执行命令";
					executeForm.Size = new Size(500, 200);
					executeForm.StartPosition = FormStartPosition.CenterParent;

					Label label = new Label
					{
						Text = "命令:",
						Location = new Point(10, 20),
						AutoSize = true
					};
					executeForm.Controls.Add(label);

					TextBox commandBox = new TextBox
					{
						Text = selectedItem.Command,
						Location = new Point(10, 50),
						Width = 460
					};
					executeForm.Controls.Add(commandBox);

					Button executeButton = new Button
					{
						Text = "执行",
						Location = new Point(300, 100),
						DialogResult = DialogResult.OK
					};
					executeForm.Controls.Add(executeButton);

					Button cancelButton = new Button
					{
						Text = "取消",
						Location = new Point(400, 100),
						DialogResult = DialogResult.Cancel
					};
					executeForm.Controls.Add(cancelButton);

					executeForm.AcceptButton = executeButton;
					executeForm.CancelButton = cancelButton;

					if (executeForm.ShowDialog() == DialogResult.OK)
					{
						string command = commandBox.Text;

						try
						{
							System.Diagnostics.Process.Start("cmd.exe", $"/c {commandBox.Text}");					
							MessageBox.Show($"已执行命令: {commandBox.Text}");
						}
						catch (Exception ex)
						{
							MessageBox.Show($"执行命令失败: {ex.Message}");
						}
					}
				}
			}
		}


		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				RefreshContextMenuItems();
			}
			else if (e.KeyCode == Keys.Delete)
			{
				DeleteSelectedItem();
			}
		}

		private List<ContextMenuItem> RemoveDuplicateRegistryPathItems(List<ContextMenuItem> inputList)
		{
			List<ContextMenuItem> uniqueList = new List<ContextMenuItem>();
			foreach (var item in inputList)
			{
				if (!uniqueList.Any(x => x.RegistryPath == item.RegistryPath))
				{
					uniqueList.Add(item);
				}
			}
			return uniqueList;
		}
	}

	public class ContextMenuItem
	{
		public string Name { get; set; }
		public string IconPath { get; set; }
		public string Name_Menu { get; set; }  
		public string ExeIconPath { get; set; }
		public string Command { get; set; }
		public string RegistryPath { get; set; }
	}
}