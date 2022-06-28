using System;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class DoubleBufferTests
{
    #region Construction

    [Fact]
    public void DoubleBuffer_OnConstruction_GivenAnInitialValue_SetsCurrentToInitialValue()
    {
        // Arrange
        var initialValue = 123;
        var doubleBuffer = new DoubleBuffer<int>(initialValue);

        // Act
        var currentValue = doubleBuffer.Current;

        // Assert
        Assert.Equal(initialValue, currentValue);
    }

    [Fact]
    public void DoubleBuffer_OnConstruction_GivenAnInitialValue_SetsNextToInitialValue()
    {
        // Arrange
        var initialValue = 123;
        var doubleBuffer = new DoubleBuffer<int>(initialValue);

        // Act
        var nextValue = doubleBuffer.PeekNext();

        // Assert
        Assert.Equal(initialValue, nextValue);
    }

    [Fact]
    public void DoubleBuffer_OnConstruction_GivenNonStringReferenceType_ThrowsNotSupportedException()
    {
        // Arrange
        var initialValue = new object();

        // Act
        var exception = Record.Exception(() => new DoubleBuffer<object>(initialValue));

        // Assert
        Assert.Equal(typeof(NotSupportedException), exception?.GetType());
    }

    [Fact]
    public void DoubleBuffer_OnConstruction_GivenStringReferenceType_DoesNotThrowException()
    {
        // Arrange
        var initialValue = "";

        // Act
        var exception = Record.Exception(() => new DoubleBuffer<string>(initialValue));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region Swap

    [Fact]
    public void DoubleBuffer_OnSwap_NextIsNotSet_CurrentReturnsSameValue()
    {
        // Arrange
        var doubleBuffer = new DoubleBuffer<int>(1);

        // Act
        var currentValueBeforeSwap = doubleBuffer.Current;
        DoubleBuffer.Swap(this);
        var currentValueAfterSwap = doubleBuffer.Current;

        //Assert
        Assert.Equal(currentValueBeforeSwap, currentValueAfterSwap);
    }

    [Fact]
    public void DoubleBuffer_OnSwap_NextIsSet_CurrentReturnsNewValue()
    {
        // Arrange
        var doubleBuffer = new DoubleBuffer<int>(1);

        // Act
        var currentValueBeforeSwap = doubleBuffer.Current;
        doubleBuffer.Next = 2;
        DoubleBuffer.Swap(this);
        var currentValueAfterSwap = doubleBuffer.Current;

        //Assert
        Assert.NotEqual(currentValueBeforeSwap, currentValueAfterSwap);
    }

    [Fact]
    public void DoubleBuffer_OnSwap_NextIsSet_RetainsCurrentValueOnSecondSwap()
    {
        // Arrange
        var doubleBuffer = new DoubleBuffer<int>(1) {Next = 2};

        // Act
        DoubleBuffer.Swap(this);
        var currentValueAfterFirstSwap = doubleBuffer.Current;
        DoubleBuffer.Swap(this);
        var currentValueAfterSecondSwap = doubleBuffer.Current;

        //Assert
        Assert.Equal(currentValueAfterFirstSwap, currentValueAfterSecondSwap);
    }

    #endregion
}