# 用户待办事项清单

## 立即可执行 (推荐)

### 1. 推送代码到 GitHub
```bash
cd D:\workspace\engines\visual-studio\CopyRelativePath
git push origin master
```

**预期结果**: 远程仓库更新为最新代码

---

### 2. 创建发布标签并触发自动构建
```bash
git tag v1.3.0
git push origin v1.3.0
```

**预期结果**: 
- GitHub Actions 自动触发
- 并行构建 VS2019 和 VS2022_2026 VSIX
- 运行测试 (必须通过)
- 创建 GitHub Release
- 上传 VSIX 文件到 Release

**监控地址**: https://github.com/mere-human/CopyRelativePath/actions

---

### 3. 验证 GitHub Actions 工作流

访问: https://github.com/mere-human/CopyRelativePath/actions

**检查清单**:
- [ ] 工作流成功运行 (绿色勾)
- [ ] 构建矩阵显示 VS2019 和 VS2022_2026 两个任务
- [ ] 所有测试通过
- [ ] Artifacts 包含两个 VSIX 文件

**如果失败**: 
1. 查看错误日志
2. 修复问题
3. 重新推送
4. 删除标签: `git tag -d v1.3.0 && git push origin :refs/tags/v1.3.0`
5. 重新创建标签

---

### 4. 下载并验证 VSIX 文件

从 GitHub Release 或 Actions Artifacts 下载:
- `CopyRelativePath-VS2019.vsix`
- `CopyRelativePath-VS2022_2026.vsix`

**验证**:
- [ ] 文件大小合理 (通常 > 100KB)
- [ ] 可以用 7-Zip 打开查看内容
- [ ] 包含 `.dll` 和 `.vsixmanifest` 文件

---

## 发布前验证 (强烈推荐)

### 5. 在 Visual Studio 2019 中手动测试

```powershell
# 安装到实验实例
cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE"
.\devenv.exe /rootsuffix Exp /installextension "D:\path\to\CopyRelativePath-VS2019.vsix"

# 或直接双击 VSIX 文件安装
```

**测试检查清单**:
- [ ] 扩展出现在 Extensions Manager 中
- [ ] 工具 > 选项 > Copy Path Extension 配置页可访问
- [ ] 在解决方案资源管理器右键文件,显示 "Copy Relative Path"
- [ ] 在文档标签右键,显示菜单项
- [ ] 在编辑器右键,显示菜单项
- [ ] 复制相对路径功能正常 (剪贴板包含正确路径)
- [ ] 配置 URL 前缀后,复制 URL 功能正常
- [ ] 复制当前行 URL 功能正常
- [ ] 复制 Include 功能正常 (如果配置了 Include 目录)

**卸载**:
```powershell
.\devenv.exe /rootsuffix Exp /uninstall:CopyRelativePath.VS2019
```

---

### 6. 在 Visual Studio 2022 中手动测试

```powershell
# 安装到实验实例
cd "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE"
.\devenv.exe /rootsuffix Exp /installextension "D:\path\to\CopyRelativePath-VS2022_2026.vsix"
```

**测试检查清单**: (同 VS2019)

---

### 7. 在 Visual Studio 2026 中手动测试 (如果已安装)

使用相同的 `CopyRelativePath-VS2022_2026.vsix` 文件

**验证**: 确认与 VS2022 行为一致 (验证二进制兼容性)

---

## 可选的后续改进

### 8. 发布到 Visual Studio Marketplace

1. 登录 https://marketplace.visualstudio.com/
2. 更新扩展页面
3. 上传新版本 VSIX 文件
4. 更新说明:
   - 添加多版本支持信息
   - 说明哪个 VSIX 对应哪个 VS 版本
   - 更新版本号为 1.3.0

---

### 9. 改进单元测试覆盖 (可选)

**如果决定投入时间**:

选项 A: 重构代码
- 将路径转换逻辑提取为静态纯函数
- 移除 UI 线程依赖
- 使方法 public 或 internal

选项 B: 集成测试
- 设置 Visual Studio 测试宿主
- 编写在实际 VS 环境中运行的测试

选项 C: 接受现状
- 当前有基础测试覆盖
- 依赖手动测试和用户反馈

---

## 完成标准

当以下所有项完成时,项目即完全交付:

- [x] 代码推送到 GitHub
- [ ] 发布标签已创建
- [ ] GitHub Actions 成功运行
- [ ] GitHub Release 已创建
- [ ] VSIX 文件可下载
- [ ] 在至少一个 VS 版本中手动测试通过
- [ ] (可选) 发布到 Visual Studio Marketplace

---

## 问题排查

### GitHub Actions 构建失败
- 检查 NuGet 包还原是否成功
- 检查 MSBuild 命令是否正确
- 查看详细错误日志

### VSIX 安装失败
- 确认 VS 版本匹配
- 检查是否与其他扩展冲突
- 查看 VS 扩展日志: `%LOCALAPPDATA%\Microsoft\VisualStudio\<version>\ActivityLog.xml`

### 功能不工作
- 检查 VS 输出窗口的扩展错误
- 确认配置正确 (如 URL 前缀)
- 重启 Visual Studio

---

## 联系支持

如有问题,请:
1. 检查 `.sisyphus/notepads/vs-multi-version-support/problems.md`
2. 查看 GitHub Issues
3. 参考 README.md 文档

---

**祝发布顺利!** 🚀
