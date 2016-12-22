namespace CompilersFinalProject
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnCompile = new System.Windows.Forms.Button();
            this.tbEditor = new System.Windows.Forms.RichTextBox();
            this.txtcode = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbErrors = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtConsole = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(283, 546);
            this.btnCompile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(100, 28);
            this.btnCompile.TabIndex = 0;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // tbEditor
            // 
            this.tbEditor.Location = new System.Drawing.Point(16, 44);
            this.tbEditor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbEditor.Name = "tbEditor";
            this.tbEditor.Size = new System.Drawing.Size(691, 484);
            this.tbEditor.TabIndex = 1;
            this.tbEditor.Text = resources.GetString("tbEditor.Text");
            // 
            // txtcode
            // 
            this.txtcode.Location = new System.Drawing.Point(761, 81);
            this.txtcode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtcode.Name = "txtcode";
            this.txtcode.Size = new System.Drawing.Size(210, 504);
            this.txtcode.TabIndex = 2;
            this.txtcode.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(757, 44);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Code";
            // 
            // tbErrors
            // 
            this.tbErrors.Location = new System.Drawing.Point(16, 592);
            this.tbErrors.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbErrors.Name = "tbErrors";
            this.tbErrors.Size = new System.Drawing.Size(1184, 160);
            this.tbErrors.TabIndex = 4;
            this.tbErrors.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 572);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Error Log";
            // 
            // txtConsole
            // 
            this.txtConsole.Location = new System.Drawing.Point(990, 81);
            this.txtConsole.Margin = new System.Windows.Forms.Padding(4);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.Size = new System.Drawing.Size(210, 504);
            this.txtConsole.TabIndex = 6;
            this.txtConsole.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(987, 47);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Console";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1214, 764);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbErrors);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtcode);
            this.Controls.Add(this.tbEditor);
            this.Controls.Add(this.btnCompile);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "PASCAL Compiler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.RichTextBox tbEditor;
        private System.Windows.Forms.RichTextBox txtcode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox tbErrors;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox txtConsole;
        private System.Windows.Forms.Label label3;
    }
}

