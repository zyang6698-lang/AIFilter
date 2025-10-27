using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepSightAI
{
    public class MachinePanel : Panel
    {
        private Label nameLabel;
        private Label dataLabel;
        private Queue<string> dataQueue = new Queue<string>();

        public MachinePanel(string machineName)
        {
            InitializeComponent(machineName);
        }

        private void InitializeComponent(string machineName)
        {
            this.BackColor = Color.FromArgb(160, 160, 160);
            this.BorderStyle = BorderStyle.FixedSingle;

            nameLabel = new Label
            {
                Text = machineName,
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            dataLabel = new Label
            {
                Text = "等待数据...",
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.LightGreen,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(dataLabel);
            this.Controls.Add(nameLabel);
        }

        public void ReceiveData(string data)
        {
            // 添加到队列
            dataQueue.Enqueue(data);

            // 更新显示
            if (dataQueue.Count > 5) // 只显示最近5条
                dataQueue.Dequeue();

            dataLabel.Text = string.Join("\n", dataQueue);

            // 触发接收动画效果
            FlashBackground();
        }

        private async void FlashBackground()
        {
            var originalColor = this.BackColor;
            this.BackColor = Color.FromArgb(0, 150, 136); // 高亮颜色

            await System.Threading.Tasks.Task.Delay(300);

            this.BackColor = originalColor;
        }
    }
}
