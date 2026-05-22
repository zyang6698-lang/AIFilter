---
description: 检查未提交改动，按历史风格生成中文 commit message 并提交
---

检查当前 git 仓库的未提交改动，总结后生成 commit message 并执行提交。

## 步骤

1. 运行以下命令获取改动信息：
   - `git status --short` — 查看所有未提交的文件
   - `git diff --stat` — 查看已修改文件的改动统计
   - `git diff --cached --stat` — 查看已暂存文件的改动统计
   - `git log --oneline -10` — 了解历史 commit 风格

2. 如果有未追踪的新文件（`??`），先用 `git add` 将其加入暂存区。
   对所有未暂存的改动（` M`），也用 `git add` 将其加入暂存区。

3. 分析所有改动内容（对于关键文件，查看 `git diff --cached` 的具体内容），总结变更要点。

4. 生成 commit message，**必须**遵循以下风格（参考历史 commit）：
   - 使用**简体中文**
   - 格式：`变更主题 - 补充说明` 或 `变更主题，补充说明`
   - 多用 `新增`、`修复`、`优化`、`重构`、`调整` 开头
   - 如果涉及版本号更新，末尾注明 `版本号更新至 X.X.X.X` 或 `版本号升级至 X.X.X.X`
   - 多模块改动可用多行，用 `- ` 列举

5. 执行 `git commit -m "生成的 message"`，不要用 `--no-verify`。

6. 显示提交结果。
