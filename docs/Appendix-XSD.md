# 附录 · 强类型模型与 XSD 验证规范（Copilot 指南）v1.0

> 指导 Copilot 生成 POCO 模型、XSD 架构与验证逻辑（不含实现代码）。

## 1) 强类型概念模型（摘要）
- **Test**：`Meta/Description/DisplayOrder/Sessions/Config/Functions/Steps/TestGroups/Sections`
- **Session**：`name` 唯一；`type ∈ ssh|http|telnet|manual`；`host/port`（ssh/telnet），`baseUrl`（http），`user/password/prompt`
- **Step**：`id`（唯一）、`alias?`、`target`、`targetType`、`timeout>=0`、`method?`（http）、`command?`；含 `Params/Headers/Extracts/Interaction`
- **Extract**：`name`（同 Step 内唯一）、`pattern`、`options?`；含 `Checks`
- **Check**：`expect`、`sourceRef`（指向同 Step 的 Extract.name）
- **Section**：`id` 唯一、`name`、`type ∈ serial|manual|final|cleanup`、`failContinue`、`retryCount`、`retestPoint`、`passNext?`、`failNext?`、`Refs(Ref@step)`

### 1.1 `expect` 语义（格式层面）
- `REGEX:...`
- 数值比较：`GE|LE|GT|LT|EQ|NE: <number>`
- 可选包含类：`CONTAINS|STARTS|ENDS: ...`
- 无前缀：字面相等  
> XSD 仅做**格式**校验；实际比较由应用层实现。

## 2) XSD 规范
### 2.1 版本路线
- **XSD 1.0**：结构、类型、正则、`xs:key`/`xs:keyref`；`refs/ref@step` 仅引用 `step.@id`
- **XSD 1.1**：新增 `xs:assert` 条件断言：Session/Step 条件必填、`ref@mode(id|alias)`、`expect` 格式

### 2.2 基础类型建议
- `IdString`：`^[A-Za-z0-9_\-]+$`
- `NonNegativeInt`：`xs:nonNegativeInteger`
- `ColorHex`：`^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$`
- `HttpMethod`：`GET|POST|PUT|DELETE|HEAD|PATCH`
- `TargetType`：`ssh|http|telnet|manual`
- `OsTime`：`xs:dateTime`
- `SNType`：`^[A-Za-z0-9]{16}$`
- `MACType`：`^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$`
- `OperatorIdType`：`^[A-Za-z0-9]{6}$`
- `ExpectRegex`：`^REGEX:.+$`
- `ExpectCompare`：`^(?:GE|LE|GT|LT|EQ|NE):-?\d+(?:\.\d+)?$`
- `ExpectContains`：`^(?:CONTAINS|STARTS|ENDS):.+$`
- `ExpectLiteral`：排除上述前缀的任意文本
- `ExpectUnion`：`ExpectRegex | ExpectCompare | ExpectContains | ExpectLiteral`
- `RegexOptionsList`：`IgnoreCase|Multiline|Singleline|ExplicitCapture` 的 `|` 组合（或 `xs:list`）

### 2.3 唯一性与引用
- `steps/step`：
  - `xs:key stepIdKey`：字段 `@id` 唯一
  - `xs:key stepAliasKey`（可选）：字段 `@alias` 唯一（若存在）
  - **作用域内**：在 `step` 内定义 `xs:key extractNameKey` 选择器 `extracts/extract` 字段 `@name`
  - `xs:keyref checkSourceRefToExtract`：`extracts/extract/checks/check/@sourceRef` → `extractNameKey`
- `sections/section`：
  - `xs:key sectionIdKey`：字段 `@id` 唯一
  - `xs:keyref passNextRef/failNextRef`：`passNext/failNext` 文本 → `sectionIdKey`
- `refs/ref`：
  - XSD 1.0：`@step` → `stepIdKey`
  - XSD 1.1：允许 `@mode=id|alias`；用 `xs:assert` 约束到对应 key

### 2.4 条件必填（XSD 1.1）
- Session：`type in ('ssh','telnet')` ⇒ `host/port` 必填；`type='http'` ⇒ `baseUrl` 必填
- Step：`targetType='http'` ⇒ `method` 必填；`timeout >= 0`
- Extract/Check：`name`/`pattern` 必填；`options` 值域合法；`expect` 满足 `ExpectUnion`

### 2.5 内容模型（父→子）
- `test`：宽松一次出现 `meta|description|displayOrder|sessions|config|functions|steps|testGroups|sections`
- `steps` ⇒ `step*`；`step` ⇒ `params? headers? extracts? interaction?`
- `extracts` ⇒ `extract*`；`extract` ⇒ `checks?`；`checks` ⇒ `check*`
- `sections` ⇒ `section*`；`section` ⇒ `refs?`
- 禁止跨域嵌套（如 `section` 下出现 `step`）

### 2.6 兼容策略
- 提供 XSD 1.0 基础版；另提供 XSD 1.1 增强版（含断言）
- 应用层仍执行业务级二次校验（与验证面板联动）

## 3) 验证清单（XSD 维度）
- `@id` 唯一：Step/Section/Function/TestGroup
- `check/@sourceRef` 能在 **同 Step** 的 `extract/@name` 中找到
- `sections/section/passNext|failNext` 能引用到现有 Section id
- Session 按 `type` 条件必填项满足
- `extract/@options` 值合法；`check/@expect` 满足 `ExpectUnion`

## 4) 执行建议（给 Copilot）
- 生成 POCO 与枚举；生成 XSD v1/v2；在 CI 跑 v1；如 Runner 支持则跑 v2
- 加入应用层断言覆盖 1.1 的关键校验
- 写回 XML 时保持元素/属性顺序稳定（最小 diff）