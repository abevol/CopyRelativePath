using Microsoft.VisualStudio.TestTools.UnitTesting;
using CopyRelativePath.Tests.TestHelpers;

namespace CopyRelativePath.Tests
{
    [TestClass]
    public class ExampleTests
    {
        [TestMethod]
        public void TestFramework_IsWorking()
        {
            // Arrange
            var expected = true;
            
            // Act
            var actual = true;
            
            // Assert
            Assert.AreEqual(expected, actual, "Test framework should be working");
        }

        [TestMethod]
        public void MockDTEHelper_CanCreateMockDTE()
        {
            // Arrange
            string testDocPath = @"C:\Projects\Test\file.cs";
            string testSolutionPath = @"C:\Projects\Test";
            
            // Act
            var mockDTE = MockDTEHelper.CreateMockDTE(testDocPath, testSolutionPath);
            
            // Assert
            Assert.IsNotNull(mockDTE, "MockDTEHelper should create a valid Mock<DTE>");
            Assert.IsNotNull(mockDTE.Object, "Mock DTE object should not be null");
            Assert.IsNotNull(mockDTE.Object.ActiveDocument, "Active document should be set");
            Assert.AreEqual(testDocPath, mockDTE.Object.ActiveDocument.FullName, "Active document path should match");
        }
    }
}
