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
            this.btnCompile = new System.Windows.Forms.Button();
            this.tbEditor = new System.Windows.Forms.RichTextBox();
            this.txtcode = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(279, 699);
            this.btnCompile.Margin = new System.Windows.Forms.Padding(4);
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
            this.tbEditor.Margin = new System.Windows.Forms.Padding(4);
            this.tbEditor.Name = "tbEditor";
            this.tbEditor.Size = new System.Drawing.Size(691, 598);
            this.tbEditor.TabIndex = 1;
            this.tbEditor.Text = "program HelloWorld\nvar x, y: integer;\nbegin\nx:=5;\n\ncase x of\n5: x:=5;\n6: x:=6;\n7:" +
    " x:=7;\nend\n\nend";
            // 
            // txtcode
            // 
            this.txtcode.Location = new System.Drawing.Point(761, 81);
            this.txtcode.Margin = new System.Windows.Forms.Padding(4);
            this.txtcode.Name = "txtcode";
            this.txtcode.Size = new System.Drawing.Size(407, 664);
            this.txtcode.TabIndex = 2;
            this.txtcode.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(758, 44);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Code";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1185, 764);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtcode);
            this.Controls.Add(this.tbEditor);
            this.Controls.Add(this.btnCompile);
            this.Margin = new System.Windows.Forms.Padding(4);
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
    }
}

