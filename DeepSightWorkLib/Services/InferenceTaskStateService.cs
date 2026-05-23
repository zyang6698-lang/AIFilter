using DeepSightModel;
using System;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    public static class InferenceTaskStateService
    {
        public static bool TryRecordConsistencyResult(InferenceTask task, SideTestResult result, out bool completed)
        {
            completed = false;
            if (task == null || result == null) return false;

            lock (task)
            {
                if (task.ConsistencyResults.Any(r => r.SerialNumber == result.SerialNumber && r.Side == result.Side))
                    return false;

                task.ConsistencyResults.Add(result);

                if (result.State == ValidationTestState.Consistent)
                {
                    task.ProcessedRecords++;
                    task.ConsistentRecords++;
                }
                else if (result.State == ValidationTestState.Inconsistent)
                {
                    task.ProcessedRecords++;
                    task.InconsistentRecords++;
                }
                else
                {
                    task.ErrorRecords++;
                }

                if (result.HasVVSData)
                {
                    task.VVSRecords++;
                    task.TotalMissCount += result.MissCount;
                    task.TotalOverKillCount += result.OverKillCount;
                }

                completed = CompleteIfNeededNoLock(task);
                return true;
            }
        }

        public static bool TryRecordSecondaryResult(InferenceTask task, SecondaryInferenceResult result, int changedToOkCount, out bool completed)
        {
            completed = false;
            if (task == null || result == null) return false;

            lock (task)
            {
                if (task.SecondaryResults.Any(r => r.SerialNumber == result.SerialNumber && r.Side == result.Side))
                    return false;

                task.SecondaryResults.Add(result);
                task.ProcessedRecords++;

                if (changedToOkCount > 0)
                    task.OkRecords += changedToOkCount;
                task.NgRecords += result.OriginalNgCount - changedToOkCount;

                completed = CompleteIfNeededNoLock(task);
                return true;
            }
        }

        public static bool TryRecordFailure(InferenceTask task, VBModel vbModel, string errorStage, string errorMessage, out bool completed)
        {
            completed = false;
            if (task == null || vbModel == null) return false;

            lock (task)
            {
                if (vbModel.IsSecondaryInference)
                {
                    if (task.SecondaryResults.Any(r => r.SerialNumber == vbModel.SN && r.Side == vbModel.Side))
                        return false;

                    task.SecondaryResults.Add(new SecondaryInferenceResult
                    {
                        SerialNumber = vbModel.SN,
                        Side = vbModel.Side,
                        ProductSerial = vbModel.ProductSerial,
                        MachineId = vbModel.MachineId,
                        InferenceTime = DateTime.Now,
                        State = SecondaryInferenceResultState.Error,
                        ErrorMessage = $"{errorStage}: {errorMessage}"
                    });
                }
                else
                {
                    if (task.ConsistencyResults.Any(r => r.SerialNumber == vbModel.SN && r.Side == vbModel.Side))
                        return false;

                    task.ConsistencyResults.Add(new SideTestResult
                    {
                        SerialNumber = vbModel.SN,
                        Side = vbModel.Side,
                        ProductSerial = vbModel.ProductSerial,
                        MachineId = vbModel.MachineId,
                        TestTime = DateTime.Now,
                        State = ValidationTestState.TestError,
                        ErrorMessage = $"{errorStage}: {errorMessage}"
                    });
                }

                task.ErrorRecords++;
                completed = CompleteIfNeededNoLock(task);
                return true;
            }
        }

        private static bool CompleteIfNeededNoLock(InferenceTask task)
        {
            if (task.IsReallyCompleted && task.State == InferenceTaskState.Running)
            {
                task.State = InferenceTaskState.Completed;
                task.EndTime = DateTime.Now;
                return true;
            }

            return false;
        }
    }
}
