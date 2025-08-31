# SMS 测试平台 · 整体项目需求说明（SRS）v1.0

> 本文是 SMS 测试平台「智能化脚本编辑器」的总体项目需求说明（不含源代码）。

## 1. 背景与目标
- 现有测试脚本以 XML 为主，规模大、层级深、人工编辑易错。
- 目标：提供一个可视化、可拖拽、可验证的脚本编辑器，降低学习成本、提高质量与交付效率。
- 愿景：演进为标准化平台，具备模板市场、插件生态与自动化校验能力。

### 1.1 业务收益
- 搭建效率提升 ≥ 50%
- 配置错误率降低 ≥ 70%
- 新人上手时间从周级降至天级

## 2. 范围与边界
### 2.1 In Scope（本期）
- 本地 XML 的打开、编辑、拖拽编排与保存（UTF-8）。
- 规则校验（结构、唯一性、引用、正则可编译、expect 语法格式）。
- 模板库（Template.xml）→ 拖入生成节点。
- 撤销/重做、搜索定位、日志记录、基础设置。

### 2.2 Out of Scope（本期不做）
- 在线协作、远端存储、实时运行器联动、图形化流程图（仅规划）。

## 3. 角色与用户画像
- 测试工程师（主要用户）
- 测试开发/架构师
- 质量负责人

## 4. 术语
- Step/Section/Extract/Check：运行与校验元素
- Template：预制结构片段
- Expect 前缀：`REGEX:`、`GE/LE/GT/LT/EQ/NE:`、`CONTAINS/STARTS/ENDS:`

## 5. 需求概览（功能）
1) 文件管理：新建/打开/另存为/最近；最小差异保存。
2) 树形视图：展示 `<test>` 全结构；节点展开/折叠、搜索筛选、计数。
3) 拖拽编排：同级/跨级移动；前/后/内插入；悬停自动展开（≈600ms）；非法投放阻止并提示。
4) 属性编辑：动态显示可编辑属性；即时写入；显示名优先 alias 其次 id。
5) 模板库：加载 Template.xml；拖入生成默认节点；仅投放合法位置。
6) 验证引擎：实时 Error/Warning/Info；双击定位；保存前阻断（可配置）。
7) 撤销/重做：≥20 步；覆盖属性修改、节点移动/增删、模板应用。
8) 日志与审计：异常捕获、文件操作、验证结果变更；本地滚动日志。
9) 设置：自动展开延时、保存前强校验、缩进宽度（默认 2）、主题/字体（可选）。

## 6. 非功能性需求（NFR）
- 平台：Windows 10/11；.NET 8（LTS）
- 性能：5k+ 节点文件下拖拽 UI ≥ 30fps；打开中等文件 < 3s
- 可靠性：崩溃率 < 0.1%；异常均有友好提示与日志
- 可用性：空态引导、快捷键、可撤销；主流程 3 次点击内完成
- 安全与合规：本地运行、无网络依赖；BSD-3 许可遵循

## 7. 技术与架构
- UI：WPF + MVVM
- 拖拽：GongSolutions.WPF.DragDrop
- 数据层（M1）：XDocument/XElement 双向同步
- 类型化与 Schema（M2+）：强类型模型与 XSD（1.0/1.1）
- 模块：Presentation / Domain / Infrastructure

## 8. 结构与约束（父→子 & 必填 & 唯一）
### 8.1 父→子结构
- `test` ⇒ `meta | description | displayOrder | sessions | config | functions | steps | testGroups | sections`
- `steps` ⇒ `step*`
- `step` ⇒ `params? | headers? | extracts? | interaction?`
- `extracts` ⇒ `extract*`；`extract` ⇒ `checks?`；`checks` ⇒ `check*`
- `sections` ⇒ `section*`；`section` ⇒ `refs?`
- 禁止将 `check/extract` 直接置于 `test`

### 8.2 唯一性与引用
- `step/@id`、`section/@id`、`function/@id`、`testGroup/@id` 唯一
- `extract/@name` 在同一 step 内唯一
- `refs/ref/@step` M1 按 id；M2 支持 id|alias

### 8.3 条件必填（示例）
- `session@type='ssh|telnet'` ⇒ `host/port` 必填；`type='http'` ⇒ `baseUrl` 必填
- `step@targetType='http'` ⇒ `method` 必填；`timeout ≥ 0`
- `extract` 需 `name/pattern`；`check` 需 `expect/sourceRef`

## 9. 强类型模型与 XSD（摘要）
- 强类型模型：POCO for `Test/Session/Step/Extract/Check/Section`；枚举 `TargetType`、`HttpMethod`
- XSD 1.0：结构/类型/`xs:key`/`xs:keyref`
- XSD 1.1：`xs:assert` 条件必填、`ref@mode(id|alias)`、`expect` 格式合法性

## 10. 交互设计（流程）
1) 打开 → 解析 → 树
2) 选择节点 → 属性面板同步 → 修改即生效
3) 拖拽 → 高亮插入位 → 合法性预检 → 投放 → XML 同步
4) 模板拖入 → 合法判断 → 生成默认节点
5) 验证 → 实时问题列表 → 定位；保存前再校验
6) 撤销/重做 → 树与属性同步回放；日志记录

## 11. 数据与文件
- I/O：XML（UTF-8；2 空格缩进）；最小差异保存
- 模板文件：`Template.xml`
- 设置：`settings.json`
- 日志：滚动文件

## 12. 验收标准（DoD）
- 合法/非法拖入行为正确
- `expect` 非法格式可识别与定位
- `ref` 指向不存在 step 被标记
- 5k+ 节点下流畅浏览基本编辑
- 交付：发布产物、手册、10+ 手工用例、5+ 单测

## 13. 里程碑与交付
- M1（Skeleton）：框架、打开/保存、树/属性、基础拖拽
- M2（Rules+Templates）：约束规则、模板解析/拖入、实时验证
- M3（Polish）：撤销/重做、性能优化、日志/设置、用例与单测

## 14. 风险与应对
| 风险               | 影响     | 概率 | 缓解                               |
| ------------------ | -------- | ---: | ---------------------------------- |
| 大文件性能不足     | 拖拽卡顿 |   中 | 懒加载/批量刷新/虚拟化             |
| 非法拖拽误保存     | 配置损坏 |   低 | 预检 + 保存前强校验 + Undo         |
| XSD 1.1 工具链不足 | 校验缺口 |   中 | 同时提供 XSD 1.0 + 应用层断言      |
| 第三方库变更       | 编译失败 |   低 | 锁定小版本、镜像缓存、License 归档 |

## 15. 运维
- 日志轮换策略；故障定位手册；问题模板（最小复现 XML + 步骤）
- 版本化：SemVer；CHANGELOG
- 许可合规：BSD-3 声明随发行

## 16. 变更管理
- 变更单审批（需需求/设计/实现/验证四表）
- PR 审查清单：UI/性能/兼容/日志/异常/单测/i18n

## 17. 附录
- A. 示例 XML（见 `/samples/`）
- B. 模板文件结构示意（略）
- C. 期望值前缀语义对照（略）
- D. 强类型 + XSD 附录（见 `/docs/Appendix-XSD.md`）

## 18. 双 XML 协同（必做）
- 类型映射：`targetType` ↔ 连接类型；会话名：`target` ↔ 配置中的 `Name`
- Sessions 为空时由配置自动生成映射
- 占位符优先级：运行时局部/提取 ＞ inputs ＞ variables ＞ constants ＞ TestConfiguration ＞ 环境变量
- 验收用例：`DUT_SSH` 能自动绑定并跑通 “GetOsViaSsh 提取+校验(期望=Ubuntu)”；改坏连接名能报错定位

## 19. 给 Copilot 的执行指令（摘要）
- A：骨架/打开保存/树属性/CI 全绿
- B：强类型 + XSD + 占位符 + 映射 + 验证面板
- C：Gong 拖拽 + Template.xml + Undo/Redo + 性能/日志/设置