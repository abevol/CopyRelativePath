# 未解决的阻塞问题

## [2026-01-27] 任务 5 阻塞: 单元测试编写困难

### 技术障碍
1. **UI 线程依赖**: BaseCopyCommand.GetRelPath() 调用 ThreadHelper.ThrowIfNotOnUIThread()
   - 单元测试环境无法提供真实 UI 线程
   - 需要 Visual Studio 测试宿主或重构代码

2. **方法可见性**: 核心业务逻辑方法是 protected
   - GetRelPath() 和 GetURLPath() 都是 protected
   - 无法直接从测试类访问

3. **复杂的 DTE Mock**: 代码严重依赖多层 DTE 对象
   - package.DTE.ActiveWindow.Type
   - package.DTE.ActiveDocument.FullName
   - package.DTE.SelectedItems
   - package.DTE.Solution.FullName
   - 需要构建完整的 Mock 对象图

4. **文件系统依赖**: 
   - File.Exists() 检查实际文件
   - Path 操作需要真实路径
   - 难以隔离测试

### 可能的解决方案
1. **代码重构**: 将路径转换逻辑提取为静态纯函数 (不依赖 DTE, 不需要 UI 线程)
2. **使用 InternalsVisibleTo**: 暴露 internal 方法给测试项目
3. **集成测试**: 改为编写在实际 VS 环境中运行的集成测试
4. **部分测试**: 仅测试可以 Mock 的简单场景

### 当前决策
**延后任务 5**。该任务需要:
- 深入了解 Visual Studio 扩展测试框架
- 可能需要重构现有代码以提高可测试性
- 或者需要设置复杂的 VS 测试宿主环境

优先完成其他可以立即交付的任务,稍后由用户决定是否需要深入的单元测试覆盖。
