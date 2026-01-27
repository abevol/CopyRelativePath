# Copy Relative Path Extension

![document tab menu](Resources/file-path_icon-icons.com_71653_128.png)

## 支持的 Visual Studio 版本

| Visual Studio 版本 | 下载链接 | 状态 |
|-------------------|---------|------|
| **VS 2019** | [CopyRelativePath-VS2019.vsix](https://github.com/mere-human/CopyRelativePath/releases/latest) | ✅ 支持 |
| **VS 2022** | [CopyRelativePath-VS2022_2026.vsix](https://github.com/mere-human/CopyRelativePath/releases/latest) | ✅ 支持 |
| **VS 2026** | [CopyRelativePath-VS2022_2026.vsix](https://github.com/mere-human/CopyRelativePath/releases/latest) | ✅ 支持 |

**如何选择正确的版本:**
- 如果您使用 **Visual Studio 2019**,请下载并安装 `CopyRelativePath-VS2019.vsix`
- 如果您使用 **Visual Studio 2022** 或 **Visual Studio 2026**,请下载并安装 `CopyRelativePath-VS2022_2026.vsix`

> **注意**: VS 2022 和 VS 2026 共用同一个 VSIX 文件,因为它们在 64 位架构上具有二进制兼容性。

也可以通过 Visual Studio Marketplace 下载: [CopyRelativePath Extension](https://marketplace.visualstudio.com/items?itemName=mere-human.CopyRelativePath)

## Description

This Visual Studio extension adds advanced path copying commands. Available commands:

1. **Copy Relative Path** - Get a relative path to a selected document. _Example:_ transform `D:\notepad-plus-plus\PowerEditor\src\resource.h` → `PowerEditor/src/resource.h`.

2. **Copy URL** - Append a relative path to a base URL specified in preferences. This might be used to get a GitHub link to the selected document in Visual Studio.
_Example:_ transform `D:\notepad-plus-plus\PowerEditor\src\resource.h`
→ https://github.com/notepad-plus-plus/notepad-plus-plus/blob/master/PowerEditor/src/resource.h.

3. **Copy Current Line URL** - Same as **Copy URL** but link to a specific line. _Example:_ https://github.com/vim/vim/blob/master/Makefile#L100

4. **Copy Include** - Get a relative path while removing directories specified in Include Directories option. Useful for C/C++ projects (`#include` directive).

Extension commands are available in:
1. Context menu for a document tab

![document tab menu](Resources/menu-doc-tab.png)

2. Context menu for an item in the Solution Explorer:

![solution explorer menu](Resources/menu-solution-explorer.png)

3. Context menu in the editor

![editor context menu](Resources/ctx-menu-editor.png)

Settings can be customized in _Tools > Options > Copy Path Extension_:

![options dialog](Resources/options-dialog.png)

## 开发和构建

本项目使用共享项目模式支持多个 Visual Studio 版本。项目结构:

```
CopyRelativePath/
├── CopyRelativePath.Shared/        # 共享代码 (所有业务逻辑)
├── CopyRelativePath.VS2019/        # VS2019 专用 VSIX 项目
├── CopyRelativePath.VS2022_2026/   # VS2022/2026 共用 VSIX 项目
├── CopyRelativePath.Tests/         # 单元测试项目
├── CopyRelativePath.sln             # 解决方案文件
└── .github/workflows/               # GitHub Actions CI/CD
```

### 构建步骤

```bash
# 克隆仓库
git clone https://github.com/mere-human/CopyRelativePath.git
cd CopyRelativePath

# 还原 NuGet 包
nuget restore CopyRelativePath.sln

# 构建所有版本 (Debug)
msbuild CopyRelativePath.sln /p:Configuration=Debug

# 构建所有版本 (Release)
msbuild CopyRelativePath.sln /p:Configuration=Release

# 运行单元测试
dotnet test CopyRelativePath.Tests/CopyRelativePath.Tests.csproj
```

### 构建单个版本

```bash
# 仅构建 VS2019 版本
msbuild CopyRelativePath.VS2019/CopyRelativePath.VS2019.csproj /p:Configuration=Release

# 仅构建 VS2022/2026 版本
msbuild CopyRelativePath.VS2022_2026/CopyRelativePath.VS2022_2026.csproj /p:Configuration=Release
```

### 安装开发版本

```bash
# 安装到 Visual Studio 实验实例 (用于调试)
devenv /rootsuffix Exp /installextension "path\to\CopyRelativePath.VS2019.vsix"

# 卸载
devenv /rootsuffix Exp /uninstall:CopyRelativePath.VS2019
```

### 项目架构说明

- **共享项目模式**: 所有业务逻辑代码存放在 `CopyRelativePath.Shared` 项目中,被两个 VSIX 项目引用
- **条件编译**: 使用 `VS2019` 和 `VS2022` 条件编译符号支持版本特定代码
- **自动化构建**: GitHub Actions 在推送标签 (如 `v1.3.0`) 时自动构建并发布两个 VSIX 文件

详细的贡献指南请参阅 [CONTRIBUTING.md](CONTRIBUTING.md) (待添加)。

## Links

* File path icon by [Picol](https://icon-icons.com/icon/file-path/71653), [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)