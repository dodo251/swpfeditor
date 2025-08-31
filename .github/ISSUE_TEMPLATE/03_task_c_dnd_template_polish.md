---
name: "[C] DnD + Template + Polish — 拖拽/模板/撤销重做/性能"
about: 启用 Gong 拖拽、Template.xml 合法投放、Undo/Redo、性能与日志设置
title: "[C] DnD + Template + Polish — "
labels: ''
assignees: ''

---

## 背景
- 参考：`/docs/SRS.md` 5、10、12、13 章

## 目标
- 拖拽：GongSolutions.WPF.DragDrop 合法投放/非法阻止；高亮插入位；悬停自动展开（≈600ms）
- 模板：解析 `Template.xml`，仅允许合法父→子结构；拖入自动生成默认节点
- 撤销/重做 ≥ 20 步（覆盖属性、增删、移动、模板应用）
- 性能：大文件交互主路径 ≥ 30fps（懒加载/批量刷新/虚拟化）
- 设置/日志：保存前强校验开关、缩进宽度、自动展开延时；日志滚动

## 交付物
- 拖拽与模板投放实现
- Undo/Redo 服务与状态同步
- 设置界面与日志
- 单测与操作文档更新

## 验收标准（DoD）
- [ ] CI 全绿（build/test/schema/app 规则）
- [ ] 合法拖放成功、非法拖放有友好提示并禁止落点
- [ ] 模板拖入生成期望节点结构
- [ ] Undo/Redo 行为覆盖常见操作
- [ ] 大文件交互流畅（基础路径 ≥ 30fps）

## 执行步骤（建议）
1. 启用 Gong DnD 并接入合法性预检；实现插入位与悬停展开
2. 解析 Template.xml → 合法投放规则；打通到树的创建逻辑
3. 引入 Undo/Redo；性能优化；设置与日志
4. 提交 PR；根据 CI/审查意见迭代到通过
