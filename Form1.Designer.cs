namespace WinRCMaster
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 保持原有的一些初始化相关代码逻辑开头
			SuspendLayout();
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Name = "Form1";
			Text = "Form1";

			// 添加 ListView 控件相关初始化代码
			listView1 = new System.Windows.Forms.ListView();
			// 设置 ListView 的一些基本属性
			listView1.View = System.Windows.Forms.View.Details;
			listView1.FullRowSelect = true;
			listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			listView1.Columns.Add("名称", 200);
			listView1.SmallImageList = new System.Windows.Forms.ImageList();

			// 将 listView1 控件添加到 Form1 的控件集合中
			Controls.Add(listView1);

			// 关联 Form1 的 Load 事件处理方法（原有的代码已有这部分关联）
			Load += Form1_Load;
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.ListView listView1;
	}
}