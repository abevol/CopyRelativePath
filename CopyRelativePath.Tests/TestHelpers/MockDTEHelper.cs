using Moq;
using EnvDTE;

namespace CopyRelativePath.Tests.TestHelpers
{
    public static class MockDTEHelper
    {
        /// <summary>
        /// Creates a mock DTE object for testing Visual Studio extensions.
        /// </summary>
        /// <param name="activeDocPath">Path to the active document</param>
        /// <param name="solutionPath">Path to the solution folder</param>
        /// <returns>Mock DTE object configured with basic Visual Studio environment</returns>
        public static Mock<DTE> CreateMockDTE(string activeDocPath, string solutionPath)
        {
            var mockDTE = new Mock<DTE>();
            var mockDoc = new Mock<Document>();
            mockDoc.Setup(d => d.FullName).Returns(activeDocPath);
            
            var mockWindow = new Mock<Window>();
            mockWindow.Setup(w => w.Type).Returns(vsWindowType.vsWindowTypeDocument);
            
            mockDTE.Setup(d => d.ActiveDocument).Returns(mockDoc.Object);
            mockDTE.Setup(d => d.ActiveWindow).Returns(mockWindow.Object);
            
            return mockDTE;
        }
    }
}
