---
name: "[A] Skeleton — WPF/MVVM + 基础展示"
about: 创建 .NET 8 WPF 三栏骨架、打开/保存、树与属性展示、CI
title: "[A] Skeleton — "
labels: ''
assignees: ''

---

## 背景
请阅读仓库：
- `/docs/SRS.md`
- `/docs/Appendix-XSD.md`
- 样例：`/samples/test.xml`

## 目标
- .NET 8 WPF + MVVM 项目骨架（三栏 UI：模板/脚本树/属性面板）
- 顶部工具栏（新建/打开/保存/撤销/重做/验证占位）
- 载入 `/samples/test.xml` → 展示树与属性；属性编辑即时映射到 XML（XDocument）
- CI：Windows Runner 上 build + 单测 通过；基础 lint 通过

## 交付物
- `src/` 初始解决方案与项目
- 可运行主窗体与三栏布局
- 基础文件 I/O（UTF-8；最小差异保存）
- 1~2 个最小单测（加载/保存）

## 验收标准（DoD）
- [ ] `dotnet build` & `dotnet test` 绿色
- [ ] 打开 `/samples/test.xml` 不崩溃，节点树/属性显示正确
- [ ] 修改属性 → 写回 XML，保存后元素/属性顺序稳定（最小 diff）
- [ ] PR 描述清晰，列出后续建议（进入 B、C 阶段的 TODO）

## 执行步骤（建议）
1. 新建 WPF(.NET 8) 解决方案与 MVVM 结构
2. 实现树视图（绑定 XDocument），右侧属性编辑器（绑定 XAttribute）
3. 接入 CI（.github/workflows/ci.yml）：build/test/lint
4. 提交 PR；如 CI 失败，请根据 PR 评论持续修复直至通过

> 提示：创建后请将此 Issue **指派给 Copilot** 或在 Agents 面板下发同名任务，由其自动开 PR 并迭代。
