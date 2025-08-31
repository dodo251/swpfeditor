---
name: "[B] Rules + Dual-XML — 强类型/XSD/映射/验证面板"
about: 引入强类型 POCO、XSD(1.0/1.1)、占位符优先级、双 XML 会话映射与验证面板
title: "[B] Rules + Dual-XML — "
labels: ''
assignees: ''

---

## 背景
- 规范：`/docs/SRS.md` 第 8~10、18 章；`/docs/Appendix-XSD.md`
- 样例：`/samples/test.xml` + `/samples/TestConfiguration.xml`

## 目标
- 生成 POCO（Test/Session/Step/Extract/Check/Section 等，含枚举）
- 接入 XSD 校验：**1.0 必过**；若 Runner 支持 1.1 验证器则 **1.1 也通过**；否则由应用层断言补齐 1.1 规则
- 实现“双 XML 协同”：
  - `step@targetType/target` ↔ 配置中的连接类型/Name（`DUT_SSH`）
  - `<sessions>` 可为空 → 运行时依据配置自动生成映射
  - 占位符解析优先级：运行时局部/提取 ＞ inputs ＞ variables ＞ constants ＞ TestConfiguration ＞ 环境变量
- 验证面板：Error/Warning/Info，支持定位（缺失连接、引用未解、正则不可编译、expect 格式错误等）

## 交付物
- POCO 模型与映射层（XDocument ↔ POCO 双向）
- `schemas/` 内 XSD(1.0/1.1)
- 验证面板与规则实现
- 单测：至少 3 条（引用完整性、占位符优先级、XSD 验证）

## 验收用例（必测）
- [ ] 打开成对样例：自动建立 `DUT_SSH` 会话映射
- [ ] 执行用例 “GetOsViaSsh 提取+校验(期望=Ubuntu)” 通过
- [ ] 改坏配置中 `DUT_SSH` 名称/移除 → 验证面板出现 Error 并定位对应 step
- [ ] 占位符缺失 → 验证面板 Error（含来源层级与修复建议）
- [ ] XSD 1.0 通过；1.1 可用时通过 / 不可用时应用层断言覆盖

## 执行步骤（建议）
1. 依据附录生成 POCO 与枚举；落库 XSD v1/v2 并接入 CI 验证任务
2. 实现占位符解析器与优先级；会话名精确匹配/默认首条策略
3. 加入验证面板（可定位）；完善单测；提交 PR 并迭代到全绿
