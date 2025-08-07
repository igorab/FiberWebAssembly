using FiberCore.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FiberCore.Test;

[TestClass]
public class FiberTests
{
    [TestMethod]
    public void TestRunCalc_StaticEquilibrium()
    {
        // Arrange
        var fiber = new FiberCalculator
        {
            CalcType = 0,
            My = 1000,
            N = 0,
            Qx = 0
        };

        // Act
        var result = fiber.RunCalc();

        // Assert
        Assert.IsNotNull(result);
        // Additional assertions based on expected output
    }
}

