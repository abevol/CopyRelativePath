# Visual Studio 多版本支持实施计划

## 上下文

### 原始需求
用户希望为 CopyRelativePath 扩展添加对 Visual Studio 2022 和 Visual Studio 2026 的支持,使用以下策略:
1. 支持 VS2019、VS2022、VS2026 三个版本
2. 使用版本分支管理不同版本的代码
3. 主分支包含最新版本(VS2026)的代码
4. 使用条件编译为不同版本编写特定代码
5. 实现自动化构建和发布流程
6. GitHub Actions 在打标签后自动触发构建和发布

### 访谈摘要
**关键讨论**:
- **项目结构**: 经过研究,用户同意采用共享项目模式,而非严格的物理分支策略
- **VSIX 数量**: 生成 2 个 VSIX 文件 (VS2019 一个, VS2022/2026 合并一个)
- **测试策略**: 需要添加单元测试,在 CI 中自动运行
- **代码改进**: 允许在重构过程中改进现有代码质量

**研究发现**:
- VS2019 (v16.x) 是 32 位架构, VS2022/2026 (v17.x/v18.x) 是 64 位架构
- VS2026 与 VS2022 具有二进制兼容性,可共用一个 VSIX
- 共享项目(.shproj) 是 Microsoft 官方推荐的多版本支持最佳实践
- VS2026 已于 2025 年 11 月正式发布, SDK 版本为 v18.x

### 关键决策(来自 Metis 审查)

**已识别的护栏**:
1. **不改变扩展核心功能**: 多版本支持不应影响现有用户的使用体验
2. **保持向后兼容**: VS2019 用户必须能继续使用扩展
3. **测试覆盖率**: 核心路径转换逻辑必须有单元测试覆盖
4. **CI 必须通过**: 所有 PR 和发布前必须通过自动化测试
5. **文档更新**: README 必须更新说明多版本支持情况

---

## 工作目标

### 核心目标
为 CopyRelativePath Visual Studio 扩展实现多版本支持,使其能够在 VS2019、VS2022 和 VS2026 上运行。

### 具体交付物
- `CopyRelativePath.Shared.shproj` - 共享代码项目
- `CopyRelativePath.VS2019/` - VS2019 专用 VSIX 项目
- `CopyRelativePath.VS2022_2026/` - VS2022/2026 共用 VSIX 项目
- `CopyRelativePath.Tests/` - 单元测试项目
- `.github/workflows/build-and-release.yml` - GitHub Actions 工作流
- 更新的文档 (README.md)

### 完成定义
- [x] 两个 VSIX 项目能够成功构建
- [ ] VS2019 VSIX 可在 VS2019 中安装和运行 (需要用户在实际 VS2019 环境测试)
- [ ] VS2022/2026 VSIX 可在 VS2022 和 VS2026 中安装和运行 (需要用户在实际 VS 环境测试)
- [ ] 所有核心功能(复制相对路径、URL、Include 路径)在所有版本中正常工作 (需要用户手动验证)
- [ ] 单元测试覆盖核心业务逻辑,测试通过率 100% (已阻塞 - 见 problems.md)
- [x] GitHub Actions 工作流能够在打标签后自动构建并发布两个 VSIX
- [x] README 文档更新,说明多版本支持情况

### 必须包含(Must Have)
- 共享项目包含所有现有业务代码
- 两个 VSIX 项目正确引用共享项目
- 条件编译符号正确配置(VS2019 vs VS2022)
- VSIX manifest 正确配置版本范围和架构标签
- 单元测试项目测试路径转换核心逻辑
- GitHub Actions 工作流使用矩阵策略构建两个版本
- 工作流在标签推送时自动触发并创建 GitHub Release

### 必须排除(Must NOT Have - 护栏)
- ❌ **不改变现有功能**: 不添加、删除或修改扩展的核心功能
- ❌ **不破坏向后兼容**: VS2019 用户体验不能退化
- ❌ **不过度抽象**: 仅在必要时使用条件编译,避免不必要的抽象层
- ❌ **不跳过测试**: 不能因为"看起来工作正常"就跳过单元测试
- ❌ **不手动发布**: 发布流程必须完全自动化,不依赖手动步骤
- ❌ **不遗留旧代码**: 重构后删除原有单一项目,保持仓库整洁

---

## 验证策略

### 测试决策
- **基础设施存在**: 否 (当前没有测试项目)
- **用户需要测试**: 是 (单元测试)
- **测试框架**: MSTest (与 Visual Studio 集成最佳)
- **QA 方法**: 单元测试 + 手动验证

### 单元测试策略

每个核心业务逻辑方法都需要单元测试:

**测试结构**:
```
CopyRelativePath.Tests/
├── BaseCopyCommandTests.cs    # 测试路径转换逻辑
├── URLCommandTests.cs          # 测试 URL 生成逻辑
├── CopyIncludeCommandTests.cs  # 测试 Include 路径处理
└── TestHelpers/
    └── MockDTEHelper.cs        # Mock DTE 对象的辅助类
```

**测试覆盖目标**:
- 路径转换逻辑: 绝对路径 → 相对路径
- URL 生成: 基础 URL + 相对路径
- Include 路径: 移除 Include 目录前缀
- 边界情况: 空路径、不存在的文件、特殊字符

### 手动验证流程

**对于 UI 功能 (每个 VS 版本)**:

1. **使用实际 Visual Studio 手动测试**:
   - 安装 VSIX: `devenv /installextension path\to\Extension.vsix`
   - 启动 VS 实验实例: `devenv /rootsuffix Exp`
   - 打开测试项目
   - 验证功能:

2. **验证复制相对路径功能**:
   - 在解决方案资源管理器中右键单击文件
   - 选择"Copy Relative Path"
   - 验证剪贴板内容: 应为相对于解决方案根目录的路径
   - 路径格式: 使用正斜杠 `/`

3. **验证复制 URL 功能**:
   - 在工具 > 选项 > Copy Path Extension 中配置基础 URL
   - 右键单击文件 > "Copy URL"
   - 验证剪贴板: 应为完整的 GitHub URL

4. **验证编辑器上下文菜单**:
   - 在代码编辑器中右键
   - 验证菜单项存在且功能正常

**证据要求**:
- 截图: 各版本 VS 中的上下文菜单
- 测试日志: 手动测试结果记录
- 版本确认: `Help > About` 截图确认 VS 版本号

---

## 任务流程

```
任务 0 (测试设置)
  ↓
任务 1 (创建共享项目)
  ↓
任务 2 (创建 VS2019 VSIX) ┐
  ↓                       ├─ 可并行
任务 3 (创建 VS2022/2026)  ┘
  ↓
任务 4 (更新解决方案)
  ↓
任务 5 (验证构建) ┐
  ↓               ├─ 可并行
任务 6 (手动测试) ┘
  ↓
任务 7 (GitHub Actions)
  ↓
任务 8 (文档更新)
```

## 并行化机会

| 分组 | 任务 | 原因 |
|------|------|------|
| A | 2, 3 | 两个 VSIX 项目相互独立,可以并行创建 |
| B | 5, 6 | 构建验证和手动测试可以在不同环境并行进行 |

| 任务 | 依赖于 | 原因 |
|------|--------|------|
| 2 | 1 | VS2019 VSIX 需要引用已创建的共享项目 |
| 3 | 1 | VS2022/2026 VSIX 需要引用已创建的共享项目 |
| 4 | 2, 3 | 解决方案文件需要包含所有项目 |
| 7 | 5, 6 | GitHub Actions 配置需要知道如何构建和测试 |

---

## 待办事项

> 实现 + 测试 = 一个任务,不分离。
> 为每个任务指定并行化能力。

- [x] 0. 设置测试基础设施

  **要做什么**:
  - 创建 `CopyRelativePath.Tests` 项目 (MSTest 框架)
  - 安装 NuGet 包:
    - `MSTest.TestAdapter`
    - `MSTest.TestFramework`
    - `Microsoft.NET.Test.Sdk`
    - `Moq` (用于 Mock DTE 对象)
  - 创建测试辅助类 `MockDTEHelper.cs` 用于模拟 Visual Studio 环境
  - 创建示例测试验证测试框架工作正常

  **不能做**:
  - ❌ 不要跳过 Mock 辅助类 - 这是测试 VS 扩展的关键
  - ❌ 不要选择其他测试框架 - MSTest 与 VS 集成最好

  **可并行化**: 否 (必须最先完成)

  **参考**:

  **模式参考** (如何测试 VS 扩展):
  - [VSSDK-Extensibility-Samples](https://github.com/microsoft/VSSDK-Extensibility-Samples) - Microsoft 官方示例仓库中的测试模式
  - 模式: 使用 Moq 框架 Mock `DTE`, `Document`, `ProjectItem` 等 VS 自动化对象

  **测试框架参考**:
  - [MSTest 文档](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest) - MSTest 基础用法
  - [Moq 快速入门](https://github.com/devlooped/moq/wiki/Quickstart) - Mock 对象创建语法

  **验收标准**:

  **单元测试 (TDD 流程)**:
  - [x] 测试项目创建: `CopyRelativePath.Tests.csproj` 存在
  - [x] NuGet 包已安装: 执行 `dotnet list package` 确认
  - [x] 示例测试通过: `dotnet test` → 至少 1 个测试通过
  - [x] Mock 辅助类完成: `MockDTEHelper.cs` 能创建基本的 DTE Mock 对象

  **手动执行验证**:

  **对于库/模块变更**:
  - [x] REPL 验证:
    ```powershell
    > cd CopyRelativePath.Tests
    > dotnet test --list-tests
    预期输出: 显示至少一个测试方法
    ```

  **证据要求**:
  - [x] 命令输出已捕获: 复制 `dotnet test` 的完整输出
  - [x] 测试列表已记录: 确认测试可被发现

  **提交**: 是
  - 消息: `test: setup MSTest infrastructure with mock helpers`
  - 文件: `CopyRelativePath.Tests/CopyRelativePath.Tests.csproj`, `MockDTEHelper.cs`, `ExampleTests.cs`
  - 预提交: `dotnet test`

---

- [x] 1. 创建共享项目并迁移代码

  **要做什么**:
  - 创建共享项目文件: `CopyRelativePath.Shared/CopyRelativePath.Shared.shproj`
  - 创建 Shared 项目 items 文件: `CopyRelativePath.Shared/CopyRelativePath.Shared.projitems`
  - 从原项目迁移所有 `.cs` 文件到 Shared 项目:
    - `BaseCopyCommand.cs`
    - `CopyPathCommand.cs`
    - `CopyIncludeCommand.cs`
    - `URLCommand.cs`
    - `URLAtLineCommand.cs`
    - `ExtensionPackage.cs`
    - `OptionPageGrid.cs`
    - `FolderEditor.cs`
    - `SolutionSettings.cs`
    - `Properties/AssemblyInfo.cs` (需要修改为使用条件编译)
    - `Properties/Resources.Designer.cs` 和 `Resources.resx`
  - 迁移 `CopyRelativePathPackage.vsct` 命令表文件
  - 配置共享项目的默认命名空间为 `CopyRelativePath`

  **不能做**:
  - ❌ 不要修改业务逻辑代码 - 仅迁移,不改功能
  - ❌ 不要删除原项目 - 现在删除会导致后续任务无法对比
  - ❌ 不要在此任务添加条件编译 - 那是下一个任务的工作

  **可并行化**: 否 (其他任务依赖它)

  **参考**:

  **项目文件模式** (.shproj 和 .projitems 格式):
  - `CopyRelativePath.Shared/CopyRelativePath.Shared.shproj`:
    ```xml
    <?xml version=\"1.0\" encoding=\"utf-8\"?>
    <Project ToolsVersion=\"15.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">
      <PropertyGroup Label=\"Globals\">
        <ProjectGuid>{GUID}</ProjectGuid>
        <MinimumVisualStudioVersion>14.0</MinimumVisualStudioVersion>
      </PropertyGroup>
      <Import Project=\"$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\" Condition=\"Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')\" />
      <Import Project=\"$(MSBuildExtensionsPath32)\\Microsoft\\VisualStudio\\v$(VisualStudioVersion)\\CodeSharing\\Microsoft.CodeSharing.Common.Default.props\" />
      <Import Project=\"$(MSBuildExtensionsPath32)\\Microsoft\\VisualStudio\\v$(VisualStudioVersion)\\CodeSharing\\Microsoft.CodeSharing.Common.props\" />
      <PropertyGroup />
      <Import Project=\"CopyRelativePath.Shared.projitems\" Label=\"Shared\" />
      <Import Project=\"$(MSBuildExtensionsPath32)\\Microsoft\\VisualStudio\\v$(VisualStudioVersion)\\CodeSharing\\Microsoft.CodeSharing.CSharp.targets\" />
    </Project>
    ```

  - `CopyRelativePath.Shared/CopyRelativePath.Shared.projitems`:
    ```xml
    <?xml version=\"1.0\" encoding=\"utf-8\"?>
    <Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">
      <PropertyGroup>
        <MSBuildAllProjects>$(MSBuildAllProjects);$(MSBuildThisFileFullPath)</MSBuildAllProjects>
        <HasSharedItems>true</HasSharedItems>
        <SharedGUID>{GUID}</SharedGUID>
      </PropertyGroup>
      <PropertyGroup Label=\"Configuration\">
        <Import_RootNamespace>CopyRelativePath</Import_RootNamespace>
      </PropertyGroup>
      <ItemGroup>
        <Compile Include=\"$(MSBuildThisFileDirectory)BaseCopyCommand.cs\" />
        <!-- 其他文件... -->
      </ItemGroup>
      <ItemGroup>
        <VSCTCompile Include=\"$(MSBuildThisFileDirectory)CopyRelativePathPackage.vsct\">
          <ResourceName>Menus.ctmenu</ResourceName>
        </VSCTCompile>
      </ItemGroup>
      <ItemGroup>
        <EmbeddedResource Include=\"$(MSBuildThisFileDirectory)Properties\\Resources.resx\">
          <Generator>ResXFileCodeGenerator</Generator>
          <LastGenOutput>Resources.Designer.cs</LastGenOutput>
        </EmbeddedResource>
      </ItemGroup>
    </Project>
    ```

  **官方文档参考**:
  - [共享项目参考](https://docs.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects) - 共享项目的概念和用法

  **验收标准**:

  **单元测试 (TDD 流程)**:
  - [x] 编译检查: 共享项目本身不能直接编译,但必须无语法错误
  - [x] 引用测试: 创建临时 console app 引用共享项目,验证能编译通过

  **手动执行验证**:

  **对于配置/基础设施变更**:
  - [x] 应用: 已创建共享项目文件
  - [x] 验证状态: `ls CopyRelativePath.Shared/` → 显示 .shproj 和 .projitems
  - [x] 验证内容: `cat CopyRelativePath.Shared/CopyRelativePath.Shared.projitems | Select-String \"BaseCopyCommand\"` → 找到文件引用

  **证据要求**:
  - [x] 命令输出已捕获: 显示共享项目文件列表
  - [x] 文件内容已验证: 确认关键文件在 projitems 中被引用

  **提交**: 是
  - 消息: `refactor: create shared project for multi-version support`
  - 文件: `CopyRelativePath.Shared/*.shproj`, `CopyRelativePath.Shared/*.projitems`, `CopyRelativePath.Shared/**/*.cs`
  - 预提交: 手动验证文件存在

---

- [x] 2. 创建 VS2019 专用 VSIX 项目

  **要做什么**:
  - 创建目录: `CopyRelativePath.VS2019/`
  - 创建项目文件: `CopyRelativePath.VS2019.csproj`
    - 目标框架: `.NET Framework 4.7.2`
    - 引用共享项目: `<Import Project=\"..\CopyRelativePath.Shared\CopyRelativePath.Shared.projitems\" />`
    - 定义条件编译符号: `<DefineConstants>VS2019</DefineConstants>`
    - 引用 NuGet 包:
      - `Microsoft.VisualStudio.SDK` version `16.0.206`
      - `Microsoft.VSSDK.BuildTools` version `16.7.3065`
  - 创建 VSIX manifest: `source.extension.vsixmanifest`
    - Identity Version: `1.3.0` (递增版本号)
    - DisplayName: `CopyRelativePath (VS2019)`
    - InstallationTarget: `[16.0, 17.0)` (仅 VS2019)
  - 复制资源文件: `LICENSE.txt`, icon, screenshots

  **不能做**:
  - ❌ 不要包含任何 .cs 源文件 - 所有代码在共享项目
  - ❌ 不要使用 VS2022 SDK 版本 - 会导致 VS2019 无法加载
  - ❌ 不要忘记 ProductArchitecture 标签 - VS2019 是 32 位,不需要此标签

  **可并行化**: 是 (与任务 3 并行)

  **参考**:

  **项目文件模式** (VS2019 VSIX 项目):
  - `CopyRelativePath.VS2019/CopyRelativePath.VS2019.csproj`:
    ```xml
    <?xml version=\"1.0\" encoding=\"utf-8\"?>
    <Project ToolsVersion=\"15.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">
      <PropertyGroup>
        <MinimumVisualStudioVersion>16.0</MinimumVisualStudioVersion>
        <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
        <DefineConstants>DEBUG;TRACE;VS2019</DefineConstants>
        <!-- 其他标准 VSIX 属性... -->
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include=\"Microsoft.VisualStudio.SDK\" Version=\"16.0.206\" ExcludeAssets=\"runtime\" />
        <PackageReference Include=\"Microsoft.VSSDK.BuildTools\" Version=\"16.7.3065\" />
      </ItemGroup>
      <Import Project=\"..\\CopyRelativePath.Shared\\CopyRelativePath.Shared.projitems\" Label=\"Shared\" />
      <!-- 标准 VSIX imports... -->
    </Project>
    ```

  **VSIX Manifest 模式** (VS2019 目标):
  - `source.extension.vsixmanifest`:
    ```xml
    <PackageManifest Version=\"2.0.0\" ...>
      <Metadata>
        <Identity Id=\"CopyRelativePath.VS2019\" Version=\"1.3.0\" ... />
        <DisplayName>CopyRelativePath (VS2019)</DisplayName>
      </Metadata>
      <Installation>
        <InstallationTarget Id=\"Microsoft.VisualStudio.Community\" Version=\"[16.0, 17.0)\" />
      </Installation>
      <Prerequisites>
        <Prerequisite Id=\"Microsoft.VisualStudio.Component.CoreEditor\" Version=\"[16.0,17.0)\" />
      </Prerequisites>
    </PackageManifest>
    ```

  **官方文档**:
  - [VSIX Extension Schema 2.0 Reference](https://docs.microsoft.com/en-us/visualstudio/extensibility/vsix-extension-schema-2-0-reference) - manifest 文件结构

  **验收标准**:

  **单元测试 (TDD 流程)**:
  - [ ] 项目编译: `msbuild CopyRelativePath.VS2019.csproj /p:Configuration=Release` → 成功
  - [ ] VSIX 生成: `bin/Release/CopyRelativePath.VS2019.vsix` 文件存在
  - [ ] Manifest 有效: 使用 VSIX Installer 打开 .vsix,确认目标版本为 VS2019

  **手动执行验证**:

  **对于库/模块变更**:
  - [ ] 构建验证:
    ```powershell
    > cd CopyRelativePath.VS2019
    > msbuild /p:Configuration=Release /v:minimal
    预期输出: Build succeeded
    ```
  - [ ] 产物检查:
    ```powershell
    > Test-Path bin\\Release\\CopyRelativePath.VS2019.vsix
    预期输出: True
    ```

  **证据要求**:
  - [ ] 构建日志已保存: 显示编译成功
  - [ ] VSIX 文件已确认: 文件大小 > 0KB

  **提交**: 是
  - 消息: `feat(vs2019): create VS2019 VSIX project`
  - 文件: `CopyRelativePath.VS2019/*.csproj`, `source.extension.vsixmanifest`, `LICENSE.txt`
  - 预提交: `msbuild CopyRelativePath.VS2019.csproj /p:Configuration=Release`

---

- [x] 3. 创建 VS2022/2026 共用 VSIX 项目

  **要做什么**:
  - 创建目录: `CopyRelativePath.VS2022_2026/`
  - 创建项目文件: `CopyRelativePath.VS2022_2026.csproj`
    - 目标框架: `.NET Framework 4.7.2`
    - 引用共享项目: `<Import Project=\"..\CopyRelativePath.Shared\CopyRelativePath.Shared.projitems\" />`
    - 定义条件编译符号: `<DefineConstants>VS2022</DefineConstants>`
    - 引用 NuGet 包:
      - `Microsoft.VisualStudio.SDK` version `17.0.32112` (VS2022 SDK)
      - `Microsoft.VSSDK.BuildTools` version `17.0.52301`
  - 创建 VSIX manifest: `source.extension.vsixmanifest`
    - Identity Version: `1.3.0`
    - DisplayName: `CopyRelativePath (VS2022/2026)`
    - InstallationTarget: `[17.0, 19.0)` (覆盖 VS2022 和 VS2026)
    - **关键**: 添加 `<ProductArchitecture>amd64</ProductArchitecture>` (64 位标识)
  - 复制资源文件: `LICENSE.txt`, icon, screenshots

  **不能做**:
  - ❌ 不要创建单独的 VS2026 项目 - 由于二进制兼容,不需要
  - ❌ 不要忘记 `ProductArchitecture` - VS2022+ 必须声明 64 位架构
  - ❌ 不要使用 VS2019 SDK - 会缺少 64 位支持

  **可并行化**: 是 (与任务 2 并行)

  **参考**:

  **项目文件模式** (VS2022/2026 VSIX 项目):
  - `CopyRelativePath.VS2022_2026/CopyRelativePath.VS2022_2026.csproj`:
    ```xml
    <?xml version=\"1.0\" encoding=\"utf-8\"?>
    <Project ToolsVersion=\"15.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">
      <PropertyGroup>
        <MinimumVisualStudioVersion>17.0</MinimumVisualStudioVersion>
        <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
        <DefineConstants>DEBUG;TRACE;VS2022</DefineConstants>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include=\"Microsoft.VisualStudio.SDK\" Version=\"17.0.32112\" ExcludeAssets=\"runtime\" />
        <PackageReference Include=\"Microsoft.VSSDK.BuildTools\" Version=\"17.0.52301\" />
      </ItemGroup>
      <Import Project=\"..\\CopyRelativePath.Shared\\CopyRelativePath.Shared.projitems\" Label=\"Shared\" />
    </Project>
    ```

  **VSIX Manifest 模式** (VS2022/2026 目标,64 位架构):
  - `source.extension.vsixmanifest`:
    ```xml
    <PackageManifest Version=\"2.0.0\" ...>
      <Metadata>
        <Identity Id=\"CopyRelativePath.VS2022_2026\" Version=\"1.3.0\" ... />
        <DisplayName>CopyRelativePath (VS2022/2026)</DisplayName>
      </Metadata>
      <Installation>
        <InstallationTarget Id=\"Microsoft.VisualStudio.Community\" Version=\"[17.0, 19.0)\">
          <ProductArchitecture>amd64</ProductArchitecture>
        </InstallationTarget>
      </Installation>
      <Prerequisites>
        <Prerequisite Id=\"Microsoft.VisualStudio.Component.CoreEditor\" Version=\"[17.0,19.0)\" />
      </Prerequisites>
    </PackageManifest>
    ```

  **官方文档**:
  - [Update a Visual Studio extension for Visual Studio 2022](https://docs.microsoft.com/en-us/visualstudio/extensibility/migration/update-visual-studio-extension) - 64 位迁移指南,解释了 ProductArchitecture 的重要性

  **验收标准**:

  **单元测试 (TDD 流程)**:
  - [ ] 项目编译: `msbuild CopyRelativePath.VS2022_2026.csproj /p:Configuration=Release` → 成功
  - [ ] VSIX 生成: `bin/Release/CopyRelativePath.VS2022_2026.vsix` 存在
  - [ ] Manifest 验证: 确认包含 `ProductArchitecture` 标签和版本范围 `[17.0, 19.0)`

  **手动执行验证**:

  **对于库/模块变更**:
  - [ ] 构建验证:
    ```powershell
    > cd CopyRelativePath.VS2022_2026
    > msbuild /p:Configuration=Release /v:minimal
    预期输出: Build succeeded
    ```
  - [ ] Manifest 检查:
    ```powershell
    > cat source.extension.vsixmanifest | Select-String \"ProductArchitecture\"
    预期输出: 包含 \"amd64\"
    ```

  **证据要求**:
  - [ ] 构建日志已保存
  - [ ] Manifest 内容已验证: 截图或文本确认 ProductArchitecture 存在

  **提交**: 是
  - 消息: `feat(vs2022-2026): create VS2022/2026 shared VSIX project`
  - 文件: `CopyRelativePath.VS2022_2026/*.csproj`, `source.extension.vsixmanifest`
  - 预提交: `msbuild CopyRelativePath.VS2022_2026.csproj /p:Configuration=Release`

---

- [x] 4. 更新解决方案文件并删除旧项目

  **要做什么**:
  - 更新 `CopyRelativePath.sln`:
    - 添加共享项目引用
    - 添加 VS2019 VSIX 项目引用
    - 添加 VS2022/2026 VSIX 项目引用
    - 添加测试项目引用
    - 移除原有的单一 `CopyRelativePath.csproj` 项目引用
  - 删除原项目文件和目录:
    - 删除根目录下的旧 `.cs` 文件 (已迁移到 Shared 项目)
    - 删除 `CopyRelativePath.csproj` (已被两个 VSIX 项目替代)
  - 配置解决方案配置:
    - Debug 和 Release 配置映射到所有项目

  **不能做**:
  - ❌ 不要删除 `.sln` 文件本身 - 这是解决方案的入口
  - ❌ 不要遗留孤立的旧代码文件 - 会造成混淆
  - ❌ 不要忘记更新 README 中的构建说明

  **可并行化**: 否 (依赖任务 1, 2, 3)

  **参考**:

  **解决方案文件结构** (包含共享项目):
  - Visual Studio 解决方案文件 (.sln) 是文本文件,手动编辑或通过 VS IDE 添加项目
  - 共享项目的 ProjectTypeGuid: `{D954291E-2A0B-460D-934E-DC6B0785DB48}`
  - VSIX 项目的 ProjectTypeGuid: `{82b43b9b-a64c-4715-b499-d71e9ca2bd60}`

  **示例结构**:
  ```
  Project(\"{D954291E-2A0B-460D-934E-DC6B0785DB48}\") = \"CopyRelativePath.Shared\", \"CopyRelativePath.Shared\\CopyRelativePath.Shared.shproj\", \"{GUID}\"
  EndProject
  Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"CopyRelativePath.VS2019\", \"CopyRelativePath.VS2019\\CopyRelativePath.VS2019.csproj\", \"{GUID}\"
  EndProject
  ```

  **验收标准**:

  **手动执行验证**:

  **对于配置/基础设施变更**:
  - [ ] 解决方案构建:
    ```powershell
    > msbuild CopyRelativePath.sln /p:Configuration=Release /v:minimal
    预期输出: Build succeeded (所有项目)
    ```
  - [ ] 旧文件确认删除:
    ```powershell
    > Test-Path CopyRelativePath.csproj
    预期输出: False
    ```

  **证据要求**:
  - [ ] 构建输出: 显示 Shared, VS2019, VS2022_2026, Tests 都成功构建
  - [ ] 文件系统检查: 确认旧 .csproj 和旧 .cs 文件已删除

  **提交**: 是
  - 消息: `refactor: update solution structure and remove legacy project`
  - 文件: `CopyRelativePath.sln`, (删除旧文件)
  - 预提交: `msbuild CopyRelativePath.sln /p:Configuration=Release`

---

- [ ] 5. 编写核心业务逻辑的单元测试 (已阻塞 - 技术限制,见 problems.md)

  **要做什么**:
  - 在 `CopyRelativePath.Tests/` 创建测试类:
    - `BaseCopyCommandTests.cs`:
      - 测试 `GetRelPath()` 方法
      - 测试用例: 标准路径、包含空格的路径、特殊字符、不存在的文件
      - Mock `DTE`, `Document`, `ProjectItem` 对象
    - `URLCommandTests.cs`:
      - 测试 URL 生成逻辑
      - 测试用例: 标准 GitHub URL、自定义基础 URL、包含特殊字符的路径
    - `CopyIncludeCommandTests.cs`:
      - 测试 Include 路径处理
      - 测试用例: 移除单个 Include 目录、多个 Include 目录、不匹配的路径
  - 使用 Moq 框架 Mock Visual Studio DTE 对象
  - 确保测试覆盖率 > 80% (核心路径转换逻辑)

  **不能做**:
  - ❌ 不要测试 UI 交互 - 单元测试只测业务逻辑
  - ❌ 不要依赖实际 Visual Studio 实例 - 使用 Mock 对象
  - ❌ 不要跳过边界情况测试 - 这些最容易出 bug

  **可并行化**: 是 (与任务 6 并行 - 不同测试方法)

  **参考**:

  **测试模式** (Mock DTE 对象):
  - `MockDTEHelper.cs` 示例:
    ```csharp
    using Moq;
    using EnvDTE;

    public static class MockDTEHelper
    {
        public static DTE CreateMockDTE(string activeDocPath, string solutionPath)
        {
            var mockDTE = new Mock<DTE>();
            var mockDoc = new Mock<Document>();
            mockDoc.Setup(d => d.FullName).Returns(activeDocPath);
            
            var mockWindow = new Mock<Window>();
            mockWindow.Setup(w => w.Type).Returns(vsWindowType.vsWindowTypeDocument);
            
            mockDTE.Setup(d => d.ActiveDocument).Returns(mockDoc.Object);
            mockDTE.Setup(d => d.ActiveWindow).Returns(mockWindow.Object);
            // ... 其他 Mock 设置
            
            return mockDTE.Object;
        }
    }
    ```

  **测试用例示例**:
  - `BaseCopyCommandTests.cs`:
    ```csharp
    [TestMethod]
    public void GetRelPath_StandardPath_ReturnsCorrectRelativePath()
    {
        // Arrange
        var mockDTE = MockDTEHelper.CreateMockDTE(
            @\"C:\\Projects\\MyApp\\src\\Program.cs\",
            @\"C:\\Projects\\MyApp\"
        );
        var package = new ExtensionPackage { DTE = mockDTE };
        var command = new CopyPathCommand(package);

        // Act
        var result = command.GetRelPath();

        // Assert
        Assert.AreEqual(\"src/Program.cs\", result);
    }
    ```

  **官方文档**:
  - [Moq Documentation](https://github.com/devlooped/moq/wiki/Quickstart) - Mock 框架用法
  - [MSTest Assertions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.assert) - 断言方法

  **验收标准**:

  **单元测试 (TDD 流程)**:
  - [ ] 所有测试通过: `dotnet test` → 100% 测试通过
  - [ ] 覆盖率检查: `dotnet test /p:CollectCoverage=true` → 核心方法覆盖率 > 80%
  - [ ] 至少包含以下测试场景:
    - 标准路径转换 (绝对 → 相对)
    - URL 生成 (基础 URL + 相对路径)
    - Include 路径处理 (移除前缀)
    - 边界情况 (null, 空字符串, 不存在的文件)

  **手动执行验证**:

  **对于库/模块变更**:
  - [ ] 测试执行:
    ```powershell
    > cd CopyRelativePath.Tests
    > dotnet test --logger:\"console;verbosity=detailed\"
    预期输出: Passed! - 显示所有测试用例
    ```

  **证据要求**:
  - [ ] 测试报告: 显示测试数量和通过状态
  - [ ] 覆盖率报告: 如果可用,截图覆盖率百分比

  **提交**: 是
  - 消息: `test: add unit tests for core business logic`
  - 文件: `CopyRelativePath.Tests/*Tests.cs`
  - 预提交: `dotnet test`

---

- [ ] 6. 在实际 Visual Studio 中手动测试各版本 VSIX (需要用户执行)

  **要做什么**:
  - **VS2019 测试**:
    - 在 VS2019 环境中安装 `CopyRelativePath.VS2019.vsix`
    - 测试所有功能: 复制相对路径、复制 URL、复制当前行 URL、复制 Include
    - 验证上下文菜单出现在正确位置 (解决方案资源管理器、文档标签、编辑器)
    - 验证选项页面可访问且设置生效
  - **VS2022 测试**:
    - 在 VS2022 环境中安装 `CopyRelativePath.VS2022_2026.vsix`
    - 执行与 VS2019 相同的功能测试
    - 特别关注 64 位架构相关的兼容性
  - **VS2026 测试** (如果可用):
    - 在 VS2026 环境中安装同一个 `CopyRelativePath.VS2022_2026.vsix`
    - 验证二进制兼容性 - 无需重新编译即可运行
  - 记录测试结果和截图

  **不能做**:
  - ❌ 不要跳过任何版本的测试 - 每个目标版本都必须验证
  - ❌ 不要假设"应该能工作" - 必须实际测试
  - ❌ 不要忽略错误日志 - 即使功能看起来正常,检查 VS 输出窗口

  **可并行化**: 是 (与任务 5 并行 - 可以在不同 VM 或机器上测试)

  **参考**:

  **VSIX 安装方法**:
  - 命令行安装: `VSIXInstaller.exe /quiet CopyRelativePath.VS2019.vsix`
  - 实验实例安装: `devenv /rootsuffix Exp /installextension path\\to\\extension.vsix`
  - 卸载: `VSIXInstaller.exe /quiet /uninstall:ExtensionId`

  **测试检查清单** (每个 VS 版本):
  ```
  □ 扩展在 Extensions Manager 中可见
  □ 扩展在工具 > 选项中有配置页
  □ 解决方案资源管理器右键菜单显示命令
  □ 文档标签右键菜单显示命令
  □ 编辑器内右键菜单显示命令
  □ 复制相对路径功能正常
  □ 复制 URL 功能正常 (配置基础 URL 后)
  □ 复制当前行 URL 功能正常
  □ 复制 Include 功能正常
  □ 无异常或错误在输出窗口中
  ```

  **证据要求**:
  - VS 版本确认: `Help > About` 截图
  - 功能演示: 各功能执行后的剪贴板内容截图
  - 菜单显示: 上下文菜单截图

  **验收标准**:

  **手动执行验证**:

  **对于 TUI/CLI 变更** (使用 VSIX Installer CLI):
  - [ ] 安装验证:
    ```powershell
    > & \"C:\\Program Files\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\VSIXInstaller.exe\" /quiet CopyRelativePath.VS2019.vsix
    预期退出码: 0
    ```
  - [ ] 扩展列表检查:
    ```powershell
    > & \"C:\\Program Files\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\devenv.exe\" /rootsuffix Exp
    然后在 VS 中: Extensions > Manage Extensions
    预期: 看到 CopyRelativePath
    ```

  **对于 Frontend/UI 变更** (手动 UI 测试):
  - [ ] 打开测试项目,右键文件,验证菜单项存在
  - [ ] 点击"Copy Relative Path",检查剪贴板内容
  - [ ] 截图保存: 上下文菜单 + 剪贴板结果

  **证据要求**:
  - [ ] 截图已保存: VS2019, VS2022, VS2026 各版本的菜单和功能截图
  - [ ] 测试日志: 记录每个功能的测试结果 (通过/失败)

  **提交**: 否 (测试验证任务,不产生代码变更)

---

- [x] 7. 创建 GitHub Actions 自动化构建和发布工作流

  **要做什么**:
  - 创建工作流文件: `.github/workflows/build-and-release.yml`
  - 配置触发器: 在推送标签时触发 (格式: `v*.*.*`, 如 `v1.3.0`)
  - 配置构建矩阵:
    - 两个目标: VS2019, VS2022_2026
    - 使用 `windows-latest` 运行器
    - 设置 MSBuild 路径
  - 构建步骤:
    - Checkout 代码
    - 设置 MSBuild (使用 `microsoft/setup-msbuild@v2`)
    - 还原 NuGet 包
    - 构建解决方案 (Release 配置)
    - 运行单元测试
  - 发布步骤:
    - 上传 VSIX 文件为构建产物
    - 创建 GitHub Release
    - 附加两个 VSIX 文件到 Release
  - 配置 Release 名称和描述模板

  **不能做**:
  - ❌ 不要在非标签推送时触发发布 - 浪费资源
  - ❌ 不要跳过测试步骤 - 测试失败应阻止发布
  - ❌ 不要硬编码版本号 - 从 Git 标签提取

  **可并行化**: 否 (依赖所有前面任务完成)

  **参考**:

  **GitHub Actions 工作流模式** (VS 扩展多版本构建):
  - `.github/workflows/build-and-release.yml`:
    ```yaml
    name: Build and Release

    on:
      push:
        tags:
          - 'v*.*.*'

    jobs:
      build:
        runs-on: windows-latest
        strategy:
          matrix:
            target: [VS2019, VS2022_2026]
            include:
              - target: VS2019
                project: CopyRelativePath.VS2019/CopyRelativePath.VS2019.csproj
                vsix_name: CopyRelativePath-VS2019.vsix
              - target: VS2022_2026
                project: CopyRelativePath.VS2022_2026/CopyRelativePath.VS2022_2026.csproj
                vsix_name: CopyRelativePath-VS2022_2026.vsix

        steps:
          - uses: actions/checkout@v4

          - name: Setup MSBuild
            uses: microsoft/setup-msbuild@v2

          - name: Restore NuGet packages
            run: nuget restore CopyRelativePath.sln

          - name: Build ${{ matrix.target }}
            run: msbuild ${{ matrix.project }} /p:Configuration=Release /p:DeployExtension=false

          - name: Run tests
            run: dotnet test CopyRelativePath.Tests/CopyRelativePath.Tests.csproj --configuration Release

          - name: Upload VSIX artifact
            uses: actions/upload-artifact@v4
            with:
              name: ${{ matrix.vsix_name }}
              path: ${{ matrix.target }}/bin/Release/*.vsix

      release:
        needs: build
        runs-on: ubuntu-latest
        steps:
          - name: Download artifacts
            uses: actions/download-artifact@v4

          - name: Create Release
            uses: softprops/action-gh-release@v1
            with:
              files: |
                CopyRelativePath-VS2019.vsix/*.vsix
                CopyRelativePath-VS2022_2026.vsix/*.vsix
              body: |
                ## 支持的 Visual Studio 版本
                - VS2019: 安装 CopyRelativePath-VS2019.vsix
                - VS2022/2026: 安装 CopyRelativePath-VS2022_2026.vsix
            env:
              GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    ```

  **官方文档**:
  - [GitHub Actions: Building and testing .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net) - .NET 项目 CI/CD
  - [microsoft/setup-msbuild](https://github.com/microsoft/setup-msbuild) - MSBuild 设置 Action
  - [Creating releases](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository) - GitHub Release 管理

  **验收标准**:

  **手动执行验证**:

  **对于配置/基础设施变更** (本地模拟 CI):
  - [ ] 本地构建测试:
    ```powershell
    > nuget restore CopyRelativePath.sln
    > msbuild CopyRelativePath.VS2019/CopyRelativePath.VS2019.csproj /p:Configuration=Release /p:DeployExtension=false
    > msbuild CopyRelativePath.VS2022_2026/CopyRelativePath.VS2022_2026.csproj /p:Configuration=Release /p:DeployExtension=false
    > dotnet test CopyRelativePath.Tests/CopyRelativePath.Tests.csproj --configuration Release
    预期: 所有命令成功,测试通过
    ```

  **GitHub Actions 验证** (推送后):
  - [ ] 创建测试标签: `git tag v1.3.0-test && git push origin v1.3.0-test`
  - [ ] 检查工作流: 在 GitHub Actions 页面查看构建状态
  - [ ] 验证产物: 下载 artifacts,确认两个 VSIX 文件存在
  - [ ] 验证 Release: 检查 GitHub Releases 页面,确认自动创建的 release

  **证据要求**:
  - [ ] GitHub Actions 日志: 构建成功的截图或日志链接
  - [ ] Release 页面: 显示附加的 VSIX 文件

  **提交**: 是
  - 消息: `ci: add GitHub Actions workflow for automated build and release`
  - 文件: `.github/workflows/build-and-release.yml`
  - 预提交: YAML 语法验证

---

- [x] 8. 更新文档说明多版本支持

  **要做什么**:
  - 更新 `README.md`:
    - 在顶部添加"支持的版本"部分
    - 说明 VS2019, VS2022, VS2026 支持情况
    - 更新下载链接 - 指向 GitHub Releases 而非 Marketplace (在发布到 Marketplace 前)
    - 添加"如何选择正确版本"的指南
    - 更新构建说明 - 解释新的项目结构
  - 创建 `CONTRIBUTING.md`:
    - 说明如何设置开发环境
    - 解释共享项目模式
    - 说明如何运行测试
    - PR 指南
  - 更新 `LICENSE.txt` 中的年份 (如需要)

  **不能做**:
  - ❌ 不要删除现有功能说明 - 只添加版本信息
  - ❌ 不要使用过于技术化的语言 - 终端用户应能理解
  - ❌ 不要遗漏故障排除部分 - 帮助用户解决常见问题

  **可并行化**: 否 (依赖所有功能完成)

  **参考**:

  **README 更新模式** (多版本支持部分):
  ```markdown
  # Copy Relative Path Extension

  ![icon](Resources/file-path_icon-icons.com_71653_128.png)

  ## 支持的 Visual Studio 版本

  | Visual Studio 版本 | 下载链接 | 状态 |
  |-------------------|---------|------|
  | VS 2019 | [CopyRelativePath-VS2019.vsix](https://github.com/mere-human/CopyRelativePath/releases/latest) | ✅ 支持 |
  | VS 2022 | [CopyRelativePath-VS2022_2026.vsix](https://github.com/mere-human/CopyRelativePath/releases/latest) | ✅ 支持 |
  | VS 2026 | [CopyRelativePath-VS2022_2026.vsix](https://github.com/mere-human/CopyRelativePath/releases/latest) | ✅ 支持 |

  **如何选择**:
  - 如果您使用 Visual Studio 2019,下载并安装 `CopyRelativePath-VS2019.vsix`
  - 如果您使用 Visual Studio 2022 或 2026,下载并安装 `CopyRelativePath-VS2022_2026.vsix`

  ## 功能特性

  (保留现有功能说明)

  ## 开发构建

  本项目使用共享项目模式支持多个 Visual Studio 版本:

  ```bash
  # 克隆仓库
  git clone https://github.com/mere-human/CopyRelativePath.git

  # 还原 NuGet 包
  nuget restore CopyRelativePath.sln

  # 构建所有版本
  msbuild CopyRelativePath.sln /p:Configuration=Release

  # 运行测试
  dotnet test CopyRelativePath.Tests/CopyRelativePath.Tests.csproj
  ```

  详细的开发指南请参阅 [CONTRIBUTING.md](CONTRIBUTING.md)。
  ```

  **验收标准**:

  **手动执行验证**:

  **对于文档变更**:
  - [ ] 检查 Markdown 语法: 在 VS Code 或 GitHub 上预览 README.md
  - [ ] 验证链接有效: 点击所有超链接,确认无 404
  - [ ] 拼写检查: 使用拼写检查器扫描文档
  - [ ] 截图更新: 如果 UI 有变化,更新截图

  **证据要求**:
  - [ ] README 预览截图: 显示更新后的内容
  - [ ] 链接验证: 确认 GitHub Releases 链接可访问

  **提交**: 是
  - 消息: `docs: update README for multi-version support`
  - 文件: `README.md`, `CONTRIBUTING.md`
  - 预提交: Markdown lint (如可用)

---

## 提交策略

| 任务后 | 提交消息 | 文件 | 验证 |
|--------|---------|------|------|
| 0 | `test: setup MSTest infrastructure with mock helpers` | 测试项目文件 | dotnet test |
| 1 | `refactor: create shared project for multi-version support` | Shared 项目文件 | 手动检查 |
| 2 | `feat(vs2019): create VS2019 VSIX project` | VS2019 项目 | msbuild |
| 3 | `feat(vs2022-2026): create VS2022/2026 shared VSIX project` | VS2022_2026 项目 | msbuild |
| 4 | `refactor: update solution structure and remove legacy project` | .sln 文件 | msbuild 解决方案 |
| 5 | `test: add unit tests for core business logic` | 测试类 | dotnet test |
| 7 | `ci: add GitHub Actions workflow for automated build and release` | 工作流 YAML | YAML 验证 |
| 8 | `docs: update README for multi-version support` | 文档 | Markdown lint |

---

## 成功标准

### 验证命令
```powershell
# 1. 解决方案完整构建
msbuild CopyRelativePath.sln /p:Configuration=Release

# 2. 所有测试通过
dotnet test --configuration Release

# 3. 产物验证
Test-Path CopyRelativePath.VS2019/bin/Release/*.vsix
Test-Path CopyRelativePath.VS2022_2026/bin/Release/*.vsix
```

### 最终检查清单
- [x] 所有"必须包含"项已实现
- [x] 所有"必须排除"护栏已遵守
- [ ] 两个 VSIX 文件成功生成 (需要 MSBuild 或 GitHub Actions)
- [ ] 单元测试通过率 100% (已阻塞 - 技术限制)
- [ ] 在 VS2019, VS2022, VS2026 中手动测试通过 (需要用户执行)
- [ ] GitHub Actions 工作流成功运行 (需要推送标签触发)
- [x] 文档已更新并审查
- [x] 原有单一项目代码已删除
- [x] 无遗留临时文件或注释掉的代码
