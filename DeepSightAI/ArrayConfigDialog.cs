using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public partial class ArrayConfigDialog : Form
    {
        private TableLayoutPanel tableLayoutPanel;
        private Button btnOk;
        private Button btnCancel;
        private TextBox[,] textBoxes;
        private int rows;
        private int cols;

        public string[,] GridData { get; private set; }

        public ArrayConfigDialog(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            InitializeComponent();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            this.textBoxes = new TextBox[rows, cols];
            this.GridData = new string[rows, cols];

            this.tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cols,
                RowCount = rows,
                Padding = new Padding(10)
            };

            for (int i = 0; i < rows; i++)
            {
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
            }
            for (int j = 0; j < cols; j++)
            {
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / cols));
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var textBox = new TextBox
                    {
                        Dock = DockStyle.Fill,
                        MaxLength = 2,
                        TextAlign = HorizontalAlignment.Center,
                        Margin = new Padding(5)
                    };
                    textBox.KeyPress += TextBox_KeyPress;
                    this.textBoxes[i, j] = textBox;
                    this.tableLayoutPanel.Controls.Add(textBox, j, i);
                }
            }

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };

            this.btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK };
            this.btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel };

            this.btnOk.Click += (sender, e) => {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        this.GridData[i, j] = this.textBoxes[i, j].Text;
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            flowPanel.Controls.Add(btnCancel);
            flowPanel.Controls.Add(btnOk);

            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(flowPanel);

            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;

            // Adjust size
            this.ClientSize = new Size(cols * 60 + 20, rows * 40 + 60);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
