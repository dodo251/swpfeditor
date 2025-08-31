# SMS 脚本编辑器 (SwpfEditor)

可视化 XML 脚本编辑器：.NET 8 + WPF + MVVM，支持 TreeView 展示、属性编辑、拖拽编排、模板拖入、强类型模型 + XSD 校验、双 XML 协同。

## 🎯 项目状态

### ✅ 阶段 A - 基础骨架
- [x] 可编译运行的 .NET 8 + WPF 项目
- [x] 加载 `/samples/` 的 test.xml 可展示树与属性
- [x] CI 全绿

### ✅ 阶段 B - 规则+双 XML（当前）
- [x] 强类型映射：完整的 POCO 模型 (Test, Step, Extract, Check, Session, TestConfiguration)
- [x] XSD 1.0/1.1 校验：结构验证 + 引用完整性 + 条件断言
- [x] 双 XML 协同：test.xml ↔ TestConfiguration.xml 自动映射
- [x] 6层占位符优先级：运行时 > Inputs > Variables > Constants > 配置 > 环境
- [x] 验收用例：GetOsViaSsh 提取+校验 (DUT_SSH 映射) 跑通
- [x] 43个单元测试全部通过
- [x] CI 全绿

### ⏳ 阶段 C - 拖拽+模板+优化（待实现）
- [ ] Gong 拖拽编排（合法/非法）
- [ ] Template.xml 拖入
- [ ] 撤销/重做 ≥20 步
- [ ] 日志与设置
- [ ] 性能优化

## 🏗️ 构建与运行

### Windows (完整构建)
```bash
# 完整解决方案（包含 WPF）
dotnet build SwpfEditor.sln
dotnet run --project src/SwpfEditor.App

# 运行测试
dotnet test SwpfEditor.sln
```

### 跨平台 (域模型层)
```bash
# 跨平台解决方案（仅域模型+测试+演示）
dotnet build SwpfEditor.CrossPlatform.sln
dotnet test SwpfEditor.CrossPlatform.sln

# 运行演示
dotnet run --project src/SwpfEditor.Demo
```

## 📁 项目结构

```
├── .github/workflows/ci.yml     # CI 工作流（Windows + 跨平台测试）
├── SwpfEditor.sln               # 完整解决方案（Windows）
├── SwpfEditor.CrossPlatform.sln # 跨平台解决方案（域模型层）
├── docs/                        # SRS 与规范文档
├── samples/                     # 成对样例 test.xml + TestConfiguration.xml
├── schemas/                     # XSD 1.0/1.1 架构文件
├── src/
│   ├── SwpfEditor.App/          # WPF 应用（Phase C）
│   ├── SwpfEditor.Domain/       # 域模型层（Phase B ✓）
│   └── SwpfEditor.Demo/         # 控制台演示
└── tests/
    └── SwpfEditor.Domain.Tests/ # 域模型测试（43个测试 ✓）
```

## 🧪 Phase B 验收标准

### 强类型模型 ✅
```csharp
// 完整的强类型映射
Test test = XmlMapper.LoadTestFromFile("test.xml");
TestConfiguration config = XmlMapper.LoadConfigFromFile("TestConfiguration.xml");

// 双向转换
string xml = XmlMapper.SaveTestToXml(test);
Test roundTrip = XmlMapper.LoadTestFromXml(xml);
```

### XSD 校验 ✅
```csharp
// XSD 1.0 + 1.1 支持
var xsdService = new XsdValidationService();
var results = xsdService.ValidateAgainstXsd10(xmlContent);
var advancedResults = xsdService.ValidateAgainstXsd11(xmlContent);
```

### 双 XML 协同 ✅
```csharp
// step@target="DUT_SSH" → TestConfiguration 中的 SSH 连接
var coordinator = new DualXmlCoordinator();
coordinator.LoadConfiguration(configFile);
coordinator.LoadTest(testFile);

// 自动生成会话
var sessions = coordinator.GenerateSessionsFromConfiguration();
// ✓ DUT_SSH: 192.168.1.100:22 (admin@ssh)
```

### 占位符解析 ✅
```csharp
// 6层优先级：Runtime > Inputs > Variables > Constants > Config > Environment
var resolver = new PlaceholderResolver();
resolver.SetRuntimeLocal("USER", "runtime_value");    // Priority 1
resolver.SetInput("USER", "input_value");             // Priority 2
resolver.SetVariable("USER", "variable_value");       // Priority 3
resolver.SetConstant("USER", "constant_value");       // Priority 4
resolver.SetConfigValue("USER", "config_value");      // Priority 5
resolver.SetEnvironmentVariable("USER", "env_value"); // Priority 6

string result = resolver.ResolvePlaceholder("USER");
// → "runtime_value" (最高优先级)
```

### 验收用例：GetOsViaSsh ✅
```xml
<!-- test.xml -->
<step id="step1" alias="GetOsViaSsh" targetType="ssh" target="DUT_SSH" 
      timeout="30" command="uname -a">
  <extracts>
    <extract name="os" pattern="(?i)Ubuntu">
      <checks>
        <check sourceRef="os" expect="Ubuntu" />
      </checks>
    </extract>
  </extracts>
</step>

<!-- TestConfiguration.xml -->
<SshConnection Name="DUT_SSH" Host="192.168.1.100" Port="22" 
               Username="admin" Password="secret123" Prompt="$" />
```

**验证结果：**
- ✅ `target="DUT_SSH"` 成功映射到 SSH 连接
- ✅ 正则提取 `(?i)Ubuntu` 验证通过
- ✅ 期望检查 `expect="Ubuntu"` 引用完整性正确
- ✅ 所有验证规则通过

## 🚀 运行演示

```bash
dotnet run --project src/SwpfEditor.Demo
```

输出示例：
```
=== SwpfEditor Domain Demo ===
✓ Loaded test 'T1' (SampleTest) 
✓ XSD validation passed
✓ Generated 5 sessions from configuration
✓ All step targets mapped correctly
✓ GetOsViaSsh step found with DUT_SSH mapping
✓ XML round-trip conversion successful
```

## 📊 测试覆盖

**43 个全面的单元测试：**
- 占位符解析优先级测试
- XSD 1.0/1.1 架构验证
- 双 XML 协调验证
- 引用完整性检查
- 业务规则验证
- 验收标准测试
- XML 往返转换测试

```bash
dotnet test --logger "console;verbosity=detailed"
# 通过: 43, 失败: 0, 跳过: 0
```

## 🎯 下一步：阶段 C

Phase B 已完成所有目标。下一阶段将实现：
1. **WPF UI 完整实现**：三栏布局（模板/树/属性）
2. **GongSolutions.WPF.DragDrop**：拖拽编排
3. **模板系统**：Template.xml 拖入支持
4. **撤销重做**：≥20 步历史记录
5. **性能优化**：5k+ 节点流畅操作

---
*SMS 脚本编辑器 - 让测试脚本编写更简单、更可靠！*