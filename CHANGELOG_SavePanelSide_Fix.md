# SavePanelSide 逻辑修复和 IsAIOk 字段移除

## 修复日期
2025-12-10

## 问题描述

### 1. SavePanelSide 逻辑不一致
在从 SQLite 迁移到 PostgreSQL 的过程中，`SavePanelSide` 方法丢失了重要的双面数据检查逻辑：

**SQLite 旧版本的逻辑：**
- 保存单面（A 或 B）数据到 `PanelSides` 表
- 检查该 Panel 的 A、B 两面数据是否都已存在
- 如果双面数据都存在，计算并更新 `Panels` 表的 `IsAIOk` 字段

**PostgreSQL 新版本的问题：**
- 只保存单面数据，没有检查双面数据是否完整
- 缺少双面数据完整性验证逻辑
- 没有任何汇总状态更新

### 2. IsAIOk 字段冗余
- PostgreSQL 版本的 `Panels` 表已经移除了 `IsAIOk` 字段
- 但 `PanelDataRecord` 类中仍保留了 `IsAIOk` 属性
- 查询代码中仍有计算和赋值 `IsAIOk` 的逻辑

## 修复内容

### 1. 修复 SavePanelSide 逻辑 (DeepSightDB/DatabaseHelper.cs)

**修改位置：** 第 306-361 行

**新增逻辑：**
```csharp
// 3. 检查是否双面数据都已存在（用于日志记录和验证）
int sidesCount = 0;
using (var checkSidesCmd = new NpgsqlCommand(
    "SELECT COUNT(*) FROM PanelSides WHERE PanelId = @PanelId",
    connection, transaction))
{
    checkSidesCmd.Parameters.AddWithValue("@PanelId", panelId);
    sidesCount = Convert.ToInt32(checkSidesCmd.ExecuteScalar());
}

// ... 提交事务后 ...

// 如果双面数据都已存在，记录日志
if (sidesCount == 2)
{
    LogTextHelper.Info($"SavePanelSide: SN={record.SerialNumber} 的 A、B 两面数据已完整");
}
```

**改进说明：**
- 在保存单面数据后，检查该 Panel 的面数据数量
- 当双面数据都存在时，记录日志提示数据已完整
- 为未来可能的双面数据汇总逻辑预留扩展点

### 2. 移除 IsAIOk 字段

#### 2.1 PanelDataRecord 类 (DeepSightDB/PanelData.cs)

**修改位置：** 第 63-77 行

**删除内容：**
```csharp
public bool IsAIOk { get; set; }  // 已删除
```

#### 2.2 DatabaseHelper.cs 查询方法

**修改位置：** 
- 第 1282-1287 行 (GetPanelsDataByMachineAndLot 方法)
- 第 1376-1381 行 (GetPanelsData 方法)

**删除内容：**
```csharp
// 衍生 IsAIOk
record.IsAIOk = record.Sides.Count == 2 && record.Sides.All(s => s.AiState == 1);
```

#### 2.3 QueryControl.cs (DeepSightAI/QueryControl.cs)

**修改位置：** 第 130-144 行

**删除内容：**
```csharp
IsAIOk = record.IsAIOk,  // 已删除
```

## 影响范围

### 数据库表结构
- ✅ `Panels` 表：无变化（已经没有 IsAIOk 字段）
- ✅ `PanelSides` 表：无变化

### 代码变更
- ✅ `DeepSightDB/DatabaseHelper.cs`：增强 SavePanelSide 逻辑，移除 IsAIOk 计算
- ✅ `DeepSightDB/PanelData.cs`：移除 PanelDataRecord.IsAIOk 属性
- ✅ `DeepSightAI/QueryControl.cs`：移除 IsAIOk 赋值

### 功能影响
- ✅ 数据保存：现在会检查并记录双面数据完整性
- ✅ 数据查询：不再计算和返回 IsAIOk 字段
- ✅ 统计逻辑：`PanelDataRecord.GetBoardStat` 方法已经使用 `AiState` 进行统计，不依赖 IsAIOk

## 测试建议

1. **单面数据保存测试**
   - 保存 A 面数据，检查日志
   - 保存 B 面数据，检查是否记录"双面数据已完整"

2. **数据覆盖测试**
   - 重复保存同一面数据，检查覆盖逻辑和日志

3. **查询功能测试**
   - 按机台和批次查询面板数据
   - 按时间范围查询面板数据
   - 验证返回的数据结构正确

4. **统计功能测试**
   - 验证 `GetBoardStat` 方法的统计结果
   - 验证面板 OK 率计算正确

## 后续优化建议

1. **考虑添加 Panel 级别的汇总状态字段**
   - 可以在 `Panels` 表添加 `FinalState` 字段
   - 当双面数据都存在时，根据两面的 `FinalState` 计算 Panel 的最终状态

2. **优化双面数据完整性检查**
   - 可以添加触发器或定时任务，检查双面数据完整性
   - 对于长时间只有单面数据的 Panel，可以发出警告

3. **性能优化**
   - 当前每次保存都会查询面数据数量，可以考虑缓存或批量处理

