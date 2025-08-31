# Copilot Instructions — SMS 脚本编辑器（WPF/.NET 8）

## 目标
构建一个可视化 XML 脚本编辑器：.NET 8 + WPF + MVVM，支持 TreeView 展示、属性编辑、拖拽编排（GongSolutions.WPF.DragDrop）、模板拖入、强类型模型 + XSD(1.0/1.1) 校验、双 XML 协同（test.xml + TestConfiguration.xml）、最小差异保存。

## 硬性要求（Quality Gates）
- 必须通过 CI：`dotnet build`、单元测试、XML Schema 校验（XSD 1.0；如 Runner 支持 1.1 验证器则一并通过）。
- 大文件交互不卡顿（5k+ 节点基本可操作），拖拽主路径 ≥ 30fps（基础主路径）。
- 保存为 UTF-8，保持元素与属性顺序稳定（最小 diff）。
- PR 小步提交，描述清晰，可回滚；一个阶段一个 PR。

## 双 XML 协同（必须实现）
- **脚本**：`test.xml`（<test> 根，含 steps/sections/extracts/checks 等）
- **配置**：`TestConfiguration.xml`（UUT/LogDirectory/连接清单 SshList/TelnetList/HttpList/...）
- 绑定：`step/@targetType ∈ {ssh,http,telnet,manual}`；`step/@target` 作为连接名（例 `DUT_SSH`）→ 在配置中按 `Name` 精确匹配；如未命名，用“类型+序号”默认规则。
- `<sessions>` 可为空：运行时需据配置自动生成会话映射。
- 占位符解析优先级（高→低）：运行时局部与提取值 ＞ inputs ＞ variables ＞ constants ＞ TestConfiguration ＞ 进程环境。解析失败 = 验证 Error。

## 强类型 + XSD（必须）
- 生成 POCO：Test/Session/Step/Extract/Check/Section 等（枚举：TargetType、HttpMethod 等）。
- XSD 1.0：结构、基本类型、`xs:key` + `xs:keyref`（check.sourceRef→同 Step 的 extract.name；ref@step→Step.@id）。
- XSD 1.1：`xs:assert` 条件必填（Session/Step）、`ref/@mode ∈ {id|alias}` 的条件引用、`check/@expect` 前缀与数值格式合法性。
- 如 CI 无 1.1 验证器：XSD 1.0 必过；应用层断言需补齐 1.1 断言对应逻辑。

## UI 与交互
- 三栏：左模板 / 中脚本树 / 右属性面板；顶部工具栏（新建/打开/保存/撤销/重做/验证）。
- 拖拽：合法投放高亮；非法投放阻止并提示（父→子规则）。
- 属性：选中即显示，编辑即写入，显示名优先 alias 其次 id。
- 验证面板：Error/Warning/Info，双击定位；保存前强校验（可切换策略）。

## 阶段与验收
- **A 基础骨架**：可编译运行；加载 `/samples/` 的 test.xml 可展示树与属性；CI 全绿。
- **B 规则+双 XML**：强类型映射、占位符优先级、引用完整性，XSD 1.0/1.1 校验接入；示例用例“GetOsViaSsh 提取+校验(期望=Ubuntu)”跑通；破坏 `DUT_SSH` 或占位符时能报错定位。
- **C 拖拽+模板+优化**：Gong 拖拽编排（合法/非法）、Template.xml 拖入、撤销/重做 ≥20 步、日志与设置、性能优化；CI 全绿。

## 目录约定
- `/docs/`：SRS 与规范文档
- `/samples/`：成对样例 `test.xml` + `TestConfiguration.xml`
- `/schemas/`：XSD(1.0/1.1)
- `/src/`：WPF 解决方案
- `/tests/`：单测

## 风格与安全
- 提交信息：`feat|fix|chore: …`；PR 标题含阶段标签 `[A] / [B] / [C]`。
- 不得提交任何密钥；日志含错误摘要但不泄露机密。