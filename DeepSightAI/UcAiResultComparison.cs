using DeepSightModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DeepSightAI
{
    /// <summary>
    /// AI前后结果对比控件，复用 UcDefectDetail 的图片、缺陷框、分页与筛选能力。
    /// </summary>
    public class UcAiResultComparison : UserControl
    {
        private readonly Label label_Summary;
        private readonly UcDefectDetail defectDetail;

        public event EventHandler<SingleImageTestEventArgs> SingleImageTestRequested;

        public UcAiResultComparison()
        {
            BackColor = Color.FromArgb(30, 30, 30);
            Dock = DockStyle.Fill;

            label_Summary = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(35, 35, 38),
                Font = new Font("微软雅黑", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            defectDetail = new UcDefectDetail { Dock = DockStyle.Fill };
            defectDetail.SingleImageTestRequested += (s, e) => SingleImageTestRequested?.Invoke(this, e);
            Controls.Add(defectDetail);
            Controls.Add(label_Summary);
        }

        public void DisplayTask(InferenceTask task)
        {
            DisplayTask(task, null, null);
        }

        public void DisplayTask(InferenceTask task, string serialNumber, string side)
        {
            if (task == null)
            {
                label_Summary.Text = "没有可对比的任务。";
                defectDetail.ClearDetails();
                return;
            }

            var items = BuildComparisonItems(task);
            if (!string.IsNullOrWhiteSpace(serialNumber) || !string.IsNullOrWhiteSpace(side))
            {
                items = items.Where(i =>
                    (string.IsNullOrWhiteSpace(serialNumber) || i.SerialNumber == serialNumber) &&
                    (string.IsNullOrWhiteSpace(side) || i.Side == side)).ToList();
            }

            label_Summary.Text = BuildSummary(task, items);
            string scope = string.IsNullOrWhiteSpace(serialNumber) ? "全部" : $"{serialNumber} ({side})";
            defectDetail.DisplayAiComparisonDetails(items, $"AI结果前后对比 - {task.Description ?? task.TaskId} - {scope}");
        }

        public void ClearComparison()
        {
            label_Summary.Text = "请选择一行结果查看AI前后对比。";
            defectDetail.ClearDetails();
        }

        private List<AiComparisonDefectItem> BuildComparisonItems(InferenceTask task)
        {
            if (task.Mode == InferenceMode.SecondaryInference)
                return BuildSecondaryItems(task);

            return BuildConsistencyItems(task);
        }

        private List<AiComparisonDefectItem> BuildConsistencyItems(InferenceTask task)
        {
            var items = new List<AiComparisonDefectItem>();
            foreach (var side in task.ConsistencyResults ?? new List<SideTestResult>())
            {
                foreach (var defect in side.DefectResults ?? new List<DefectTestResult>())
                {
                    if (defect.DetectInfo == null) continue;
                    items.Add(new AiComparisonDefectItem
                    {
                        SerialNumber = side.SerialNumber,
                        Side = side.Side,
                        DefectIndex = defect.DefectIndex,
                        DetectInfo = defect.DetectInfo,
                        ProductSerial = side.ProductSerial,
                        MachineId = side.MachineId,
                        OriginalAIStatus = defect.OriginalAIStatus,
                        NewAIStatus = defect.NewAIStatus,
                        SourceText = side.HasVVSData ? "一致性测试/VVS参考" : "一致性测试/AI参考"
                    });
                }
            }
            return items;
        }

        private List<AiComparisonDefectItem> BuildSecondaryItems(InferenceTask task)
        {
            var items = new List<AiComparisonDefectItem>();
            foreach (var side in task.SecondaryResults ?? new List<SecondaryInferenceResult>())
            {
                foreach (var point in side.PointResults ?? new List<SecondaryInferencePointResult>())
                {
                    if (point.DetectInfo == null) continue;
                    items.Add(new AiComparisonDefectItem
                    {
                        SerialNumber = side.SerialNumber,
                        Side = side.Side,
                        DefectIndex = point.DefectIndex,
                        DetectInfo = point.DetectInfo,
                        ProductSerial = side.ProductSerial,
                        MachineId = side.MachineId,
                        OriginalAIStatus = point.OriginalAIStatus,
                        NewAIStatus = point.NewAIStatus,
                        SourceText = "二次推理"
                    });
                }
            }
            return items;
        }

        private string BuildSummary(InferenceTask task, List<AiComparisonDefectItem> items)
        {
            int total = items.Count;
            int changed = items.Count(i => i.IsChanged);
            string modeName = task.Mode == InferenceMode.SecondaryInference ? "二次推理" : "一致性测试";
            var before = BuildStatusSummary(items.Select(i => i.OriginalAIStatus));
            var after = BuildStatusSummary(items.Select(i => i.NewAIStatus));
            return $"{modeName} | 缺陷点: {total} | 变化: {changed} | 推理前: {before} | 推理后: {after}";
        }

        private string BuildStatusSummary(IEnumerable<int> statuses)
        {
            var list = statuses.ToList();
            return $"OK:{list.Count(s => s == 1)} NG:{list.Count(s => s == 2)} 异常:{list.Count(s => s == 3)} 未检测:{list.Count(s => s == 0)}";
        }
    }
}