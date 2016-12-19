using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CompilersFinalProject.Compiler.Parsing;

namespace CompilersFinalProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            string codeToCompile = tbEditor.Text;
            Parser parser = new Parser(codeToCompile);
            parser.Run();


            txtcode.Text = parser.VMCode;
            tbErrors.Text = parser.Errors;
        }

      
    }
}
