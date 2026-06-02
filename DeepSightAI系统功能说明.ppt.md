---
marp: true
theme: default
paginate: true
size: 16:9
title: DeepSightAI 系统功能说明
description: DeepSightAI 智能缺陷检测与复判平台功能说明
style: |
  section {
    font-family: "Microsoft YaHei", "Segoe UI", sans-serif;
    color: #1f2937;
    background: linear-gradient(135deg, #f8fafc 0%, #eef2ff 100%);
  }
  h1 { color: #1e3a8a; font-size: 42px; }
  h2 { color: #1d4ed8; font-size: 34px; }
  h3 { color: #2563eb; }
  strong { color: #1d4ed8; }
  table { font-size: 22px; }
  th { background: #dbeafe; color: #1e3a8a; }
  .subtitle { font-size: 26px; color: #475569; margin-top: 24px; }
  .tag { display: inline-block; padding: 6px 14px; margin: 6px; border-radius: 999px; background: #dbeafe; color: #1e40af; font-size: 22px; }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
  .card { background: rgba(255,255,255,.78); border-radius: 18px; padding: 18px 22px; box-shadow: 0 6px 18px rgba(30,58,138,.08); }
  .center { text-align: center; }
  .small { font-size: 20px; color: #475569; }
---

<!-- _class: center -->

# DeepSightAI 系统功能说明

<div class="subtitle">智能缺陷检测与 AI 复判平台</div>

<br/>

<span class="tag">AVI 数据接入</span>
<span class="tag">AI 智能推理</span>
<span class="tag">人工复判</span>
<span class="tag">质量追溯</span>

---

## 1. 系统定位

DeepSightAI 面向 **PCB / 电子制造行业**，对 AVI 自动光学检测设备产出的缺陷数据进行 AI 二次智能分析。

### 核心目标

- 对 AVI 报点进行 **AI 二次判定**
- 自动过滤误报，降低人工复判压力
- 建立检测、复判、统计、追溯的数据闭环
- 提升产线良率管控效率与质量透明度

---

## 2. 系统功能总览

| 功能域 | 主要模块 | 说明 |
|---|---|---|
| 数据接入 | AVI / LevelDB / MinIO | 获取检测结果、图片与配置 |
| 智能分析 | AI 推理 / 一致性测试 | 缺陷二次判定与模型验证 |
| 业务应用 | 首页监控 / AI 复判 / 历史查询 | 支撑现场操作与质量追溯 |
| 数据管理 | PostgreSQL / 缓存 / 回写 | 保存结果并同步外部系统 |
| 运维支撑 | 配置 / 告警 / 日志 / 部署 | 保证系统稳定运行 |

---

## 3. 总体架构

<div class="grid">
<div class="card">

### 数据源层
- AVI 光学检测设备
- LevelDB 后端服务
- MinIO 对象存储

</div>
<div class="card">

### 接入处理层
- AVI 数据解析
- 图片加载
- AI 推理流水线

</div>
<div class="card">

### 存储统计层
- PostgreSQL 数据库
- 面板/面别/复判记录
- 实时统计缓存

</div>
<div class="card">

### 展示交互层
- 首页监控
- AI 复判
- 查询统计
- 告警管理

</div>
</div>

---

## 4. 核心业务流程

| 步骤 | 流程节点 | 处理内容 |
|---|---|---|
| 1 | AVI 检测 | 产生面板 SN、面别、缺陷点数据 |
| 2 | 数据接入 | 通过 LevelDB 读取 AVI 原始结果 |
| 3 | 图片加载 | 从 MinIO 拉取缺陷图、模板图、Gerber 图 |
| 4 | AI 推理 | 调用视觉引擎完成缺陷二次判定 |
| 5 | 结果处理 | 写入数据库，回写 LevelDB，更新统计 |
| 6 | 业务展示 | 首页监控、AI 复判、历史查询、报表分析 |

---

## 5. 模块一：AVI 数据接入与解析

### 功能说明

将 AVI 设备检测结果转换为系统内部标准数据模型。

### 主要能力

- 支持 HTTP JSON 协议拉取 AVI 检测数据
- 自动解析 SN、Lot、料号、A/B 面、缺陷点列表
- 支持多机台、多产线接入管理
- 支持面板维度与面别维度的数据组织

### 价值

打通检测设备与 AI 复判平台的数据链路，实现检测数据自动化流转。

---

## 6. 模块二：图片加载与对象存储

### 功能说明

从 MinIO 对象存储获取 AI 推理和人工复判所需的图像资源。

### 主要能力

- 加载 VRS 缺陷图、模板图、Gerber 图
- 使用 OpenCV 完成图片解码与处理
- 支持图片缺失、读取失败等异常识别
- 为 AI 推理和缺陷详情展示提供图像输入

### 价值

统一管理检测图片资源，为算法分析和人工复核提供可靠图像基础。

---

## 7. 模块三：AI 智能推理

### 功能说明

对 AVI 报出的 NG 点进行 AI 二次判定，识别真实缺陷与误报缺陷。

### 主要能力

- 支持缺陷 OK / NG 判定与缺陷分类
- 支持料号到 AI 方案流程的自动映射
- 支持正常推理、单图测试、二次推理
- 支持新旧模型一致性测试与结果对比

### 价值

降低过杀与人工复判数量，提高缺陷判定效率和一致性。

---

## 8. 模块四：异步处理流水线

### 处理链路

<div class="center">
<strong>JSON 解析</strong> → <strong>图片加载</strong> → <strong>AI 推理</strong> → <strong>结果分发</strong> → <strong>持久化 + 后处理</strong>
</div>

### 架构特点

- 基于 TPL Dataflow 的多阶段异步处理
- 各阶段职责独立，便于扩展与替换
- 支持背压控制，避免任务堆积导致内存异常
- 数据库写入与后处理统计可并行分发

### 价值

提升大批量检测任务处理吞吐，保障产线连续运行场景下的稳定性。

---

## 9. 模块五：AI 复判与人工审核

### 功能说明

为操作人员提供缺陷图片、AI 判定、历史信息与人工复核入口。

### 主要能力

- 缺陷列表查询与筛选
- 缺陷图片详情查看
- AI 结果与人工复判结果展示
- 支持复判记录保存与追溯
- 支持验证测试与一致性测试结果查看

### 价值

形成 AI 自动判定 + 人工最终确认的质量闭环。

---

## 10. 模块六：数据存储与质量追溯

### 功能说明

将面板、面别、缺陷、AI 结果、人工复判等数据结构化保存。

### 主要能力

- PostgreSQL 存储面板检测结果
- 保存 A/B 面状态、AI 状态、VRS 状态、最终状态
- 支持按 SN、Lot、料号、时间、机台查询
- 支持结果回写外部 LevelDB 服务

### 价值

建立完整检测数据资产，支撑历史追溯、质量分析和客户问题定位。

---

## 11. 模块七：生产监控与统计分析

### 功能说明

实时展示系统运行状态和生产质量指标。

### 主要能力

- 首页展示实时任务状态
- 统计检测数量、OK / NG 数量、AI 过滤效果
- 支持批次、料号、机台维度分析
- 支持良率趋势与历史数据查询

### 价值

帮助管理人员快速掌握产线质量状态，辅助异常分析和制程优化。

---

## 12. 模块八：配置、告警与日志

<div class="grid">
<div class="card">

### 配置管理
- 系统参数配置
- AI 方案配置
- 数据库 / MinIO 配置
- 机台注册与重点缺陷规则

</div>
<div class="card">

### 告警日志
- AI 引擎异常告警
- 数据库连接检测
- 图片加载异常提示
- 全链路运行日志记录

</div>
</div>

### 价值

提高系统可维护性，支持现场快速定位问题与灵活适配不同产线。

---

## 13. 系统亮点

| 亮点 | 说明 |
|---|---|
| 高性能流水线 | 多阶段异步处理，提升任务吞吐能力 |
| AI 过杀过滤 | 减少误报缺陷，降低人工复判成本 |
| 多模式推理 | 支持正常推理、二次推理、单图测试、一致性测试 |
| 数据闭环 | 从 AVI 接入到 AI 判定、回写、复判、统计全流程闭环 |
| 模块化架构 | 服务职责清晰，便于维护、扩展和替换 |

---

## 14. 系统价值总结

<div class="grid">
<div class="card"><strong>提效</strong><br/>减少人工复判数量，提高复判效率</div>
<div class="card"><strong>降本</strong><br/>过滤 AVI 误报，降低人工与返工成本</div>
<div class="card"><strong>稳定</strong><br/>流水线、告警、日志保障系统持续运行</div>
<div class="card"><strong>可追溯</strong><br/>检测数据结构化保存，支持质量分析与问题定位</div>
</div>

---

<!-- _class: center -->

# 结束页

<div class="subtitle">DeepSightAI：让缺陷检测更智能，让质量管理更高效</div>
