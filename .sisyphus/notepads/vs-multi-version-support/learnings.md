# 学习笔记

## [Timestamp] 任务开始

- 项目架构: 共享项目模式 (.shproj) + 2 个 VSIX 项目
- VS2019: 32 位架构, SDK v16.x
- VS2022/2026: 64 位架构, SDK v17.x/v18.x, 二进制兼容
## [] 任务 0: 设置测试基础设施完成

### 测试框架配置
- 框架: MSTest 3.1.1
- Mock 框架: Moq 4.20.70
- VS SDK: Microsoft.VisualStudio.SDK 16.0.206
- 测试通过: 2/2 (TestFramework_IsWorking, MockDTEHelper_CanCreateMockDTE)


## [2026-01-27] 任务 0: 设置测试基础设施完成

### 测试框架配置
- 框架: MSTest 3.1.1
- Mock 框架: Moq 4.20.70
- VS SDK: Microsoft.VisualStudio.SDK 16.0.206
- 测试通过: 2/2 (TestFramework_IsWorking, MockDTEHelper_CanCreateMockDTE)
- 目标框架: .NET Framework 4.7.2

### NuGet 包安装
```
Microsoft.NET.Test.Sdk (17.8.0)
MSTest.TestAdapter (3.1.1)
MSTest.TestFramework (3.1.1)
Moq (4.20.70)
Microsoft.VisualStudio.SDK (16.0.206)
```

### 项目结构
```
CopyRelativePath.Tests/
├── CopyRelativePath.Tests.csproj
├── ExampleTests.cs (2 个测试方法)
└── TestHelpers/
    └── MockDTEHelper.cs (DTE Mock 辅助类)
```

### 测试执行结果
```
dotnet test --list-tests
  TestFramework_IsWorking
  MockDTEHelper_CanCreateMockDTE

dotnet test
  测试总数: 2
  通过数: 2
  总时间: 2.48秒
```

### 注意事项
- VS Threading 警告 (VSTHRD010) 在单元测试中可以忽略 (Mock 对象不在真实 UI 线程上)
- EnvDTE 通过 Microsoft.VisualStudio.SDK NuGet 包提供,不需要手动引用


## [2026-01-27] 任务 1: 创建共享项目并迁移代码完成

### 共享项目结构
- 项目 GUID: {22212B22-2A1C-429E-BE8E-D5DFA93B33E2}
- 项目类型: Shared Project (.shproj)
- 根命名空间: CopyRelativePath

### 已迁移的文件
**C# 源文件 (9 个)**:
- BaseCopyCommand.cs
- CopyIncludeCommand.cs
- CopyPathCommand.cs
- ExtensionPackage.cs
- FolderEditor.cs
- OptionPageGrid.cs
- SolutionSettings.cs
- URLAtLineCommand.cs
- URLCommand.cs

**Properties 文件 (3 个)**:
- Properties/AssemblyInfo.cs
- Properties/Resources.Designer.cs
- Properties/Resources.resx

**VSCT 文件 (1 个)**:
- CopyRelativePathPackage.vsct (命令表定义)

### 项目文件
- CopyRelativePath.Shared.shproj (共享项目主文件)
- CopyRelativePath.Shared.projitems (项目条目列表)

### 注意事项
- 所有源文件保持原样复制,未修改业务逻辑
- 使用 $(MSBuildThisFileDirectory) 变量引用文件路径
- OptionPageGrid.cs 标记为 SubType=Component
- Resources.Designer.cs 配置了 AutoGen/DesignTime/DependentUpon


## [2026-01-27] 任务 2: 创建 VS2019 专用 VSIX 项目完成

### 项目配置
- 项目 GUID: {29A41092-F30F-4D20-8520-A824006B78FD}
- 目标框架: .NET Framework 4.7.2
- VS SDK: Microsoft.VisualStudio.SDK 16.0.206
- Build Tools: Microsoft.VSSDK.BuildTools 16.7.3065

### 条件编译符号
- Debug: DEBUG;TRACE;VS2019
- Release: TRACE;VS2019

### VSIX Manifest
- Identity ID: CopyRelativePath.VS2019
- Version: 1.3.0
- DisplayName: CopyRelativePath (VS2019)
- InstallationTarget: [16.0, 17.0) (仅 VS2019)
- 无 ProductArchitecture 标签 (32位默认)

### 文件结构
```
CopyRelativePath.VS2019/
├── CopyRelativePath.VS2019.csproj
└── source.extension.vsixmanifest
```

### 共享项目引用
```xml
<Import Project="..\CopyRelativePath.Shared\CopyRelativePath.Shared.projitems" Label="Shared" />
```


## [2026-01-27] 任务 3: 创建 VS2022/2026 共用 VSIX 项目完成

### 项目配置
- 项目 GUID: {1CFCE687-F277-457E-AAF7-30E5EDB694F9}
- 目标框架: .NET Framework 4.7.2
- VS SDK: Microsoft.VisualStudio.SDK 17.0.32112 (VS2022 SDK)
- Build Tools: Microsoft.VSSDK.BuildTools 17.0.52301

### 条件编译符号
- Debug: DEBUG;TRACE;VS2022
- Release: TRACE;VS2022

### VSIX Manifest (关键差异)
- Identity ID: CopyRelativePath.VS2022_2026
- Version: 1.3.0
- DisplayName: CopyRelativePath (VS2022/2026)
- InstallationTarget: [17.0, 19.0) (覆盖 VS2022 和 VS2026)
- **关键**: ProductArchitecture=amd64 (64位标识,VS2022+必需)

### 文件结构
```
CopyRelativePath.VS2022_2026/
├── CopyRelativePath.VS2022_2026.csproj
└── source.extension.vsixmanifest
```

### 二进制兼容性
- VS2022 (v17.x) 和 VS2026 (v18.x) 使用相同的 SDK
- 64位架构相同,可以共用一个 VSIX
- 版本范围 [17.0, 19.0) 覆盖两个版本

### 对比 VS2019 项目
| 特性 | VS2019 | VS2022/2026 |
|------|--------|-------------|
| MinimumVisualStudioVersion | 16.0 | 17.0 |
| SDK Version | 16.0.206 | 17.0.32112 |
| 架构 | x86 (32位,默认) | amd64 (64位,显式) |
| InstallationTarget | [16.0, 17.0) | [17.0, 19.0) |
| ProductArchitecture | 无 | amd64 |


## [2026-01-27] 任务 4: 更新解决方案文件并删除旧项目完成

### 解决方案文件更新
- 添加共享项目: CopyRelativePath.Shared (.shproj)
- 添加 VS2019 VSIX 项目: CopyRelativePath.VS2019
- 添加 VS2022/2026 VSIX 项目: CopyRelativePath.VS2022_2026
- 添加测试项目: CopyRelativePath.Tests
- 移除旧项目: CopyRelativePath.csproj

### SharedMSBuildProjectFiles 节
配置共享项目引用关系:
- 共享项目本身: SharedItemsImports = 13
- VS2019 项目引用: SharedItemsImports = 4
- VS2022/2026 项目引用: SharedItemsImports = 4

### 已删除的文件
**根目录源文件** (已迁移到共享项目):
- BaseCopyCommand.cs
- CopyIncludeCommand.cs
- CopyPathCommand.cs
- ExtensionPackage.cs
- FolderEditor.cs
- OptionPageGrid.cs
- SolutionSettings.cs
- URLAtLineCommand.cs
- URLCommand.cs
- CopyRelativePathPackage.vsct

**项目文件**:
- CopyRelativePath.csproj (旧的单一项目文件)
- source.extension.vsixmanifest (旧的 manifest)

**Properties 目录**:
- Properties/AssemblyInfo.cs
- Properties/Resources.Designer.cs
- Properties/Resources.resx

### 保留的文件
- LICENSE.txt (根目录)
- README.md (根目录)
- Resources/ (图片资源目录)
- .gitignore

### 项目结构
新的项目结构实现了代码隔离和版本管理:
```
CopyRelativePath/
├── CopyRelativePath.Shared/        # 共享代码
├── CopyRelativePath.VS2019/        # VS2019 VSIX
├── CopyRelativePath.VS2022_2026/   # VS2022/2026 VSIX
├── CopyRelativePath.Tests/         # 单元测试
├── CopyRelativePath.sln             # 解决方案
└── LICENSE.txt, README.md           # 文档

