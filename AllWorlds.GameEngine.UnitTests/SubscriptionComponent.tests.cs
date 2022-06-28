using System;
using System.Linq;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class SubscriptionComponentTests
{
    [Fact]
    private void SubscriptionComponent_OnConstruction_ProducesDictionary()
    {
        // Arrange
        var componentType = typeof(Component);
        var callback = new Action<Entity, Component?, Component>(static (_, _, _) => { });

        // Act
        var component = new SubscribeComponent(componentType, callback);

        // Assert
        Assert.True(component.SubscriptionDictionary.ContainsKey(componentType));
        Assert.True(component.SubscriptionDictionary[componentType][0] == callback);
    }

    [Fact]
    private void
        SubscriptionComponent_OnHandleDuplicate_NewSubscriptionAlreadyExists_ReturnsDistinctValues()
    {
        // Arrange
        var componentType = typeof(Component);
        var callback = new Action<Entity, Component?, Component>(static (_, _, _) => { });
        var component = new SubscribeComponent(componentType, callback);

        // Act
        var newComponent = component.HandleDuplicate(component);

        // Assert
        Assert.Equal(newComponent.SubscriptionDictionary[componentType],
            newComponent.SubscriptionDictionary[componentType].Distinct().ToArray());
    }

    [Fact]
    private void
        SubscriptionComponent_OnHandleDuplicate_NewComponentIsNotSubscriptionComponent_ThrowsArgumentException()
    {
        // Arrange
        var componentType = typeof(Component);
        var callback = new Action<Entity, Component?, Component>(static (_, _, _) => { });
        var component = new SubscribeComponent(componentType, callback);
        var regularComponent = new Component();

        // Act
        var exception = Record.Exception(() => component.HandleDuplicate(regularComponent));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    [Fact]
    private void
        SubscriptionComponent_OnHandleDuplicate_NewComponentIsSubscriptionComponent_AddsDuplicatesCallbackToDictionary()
    {
        // Arrange
        var componentType = typeof(Component);

        var callbackOne = new Action<Entity, Component?, Component>(static (_, _, _) => { });
        var componentOne = new SubscribeComponent(componentType, callbackOne);

        var callbackTwo = new Action<Entity, Component?, Component>(static (_, _, _) => { });
        var componentTwo = new SubscribeComponent(componentType, callbackTwo);

        // Act
        var newComponent = componentOne.HandleDuplicate(componentTwo);

        // Assert
        Assert.True(newComponent.SubscriptionDictionary.ContainsKey(componentType));
        Assert.Contains(callbackOne, newComponent.SubscriptionDictionary[componentType]);
        Assert.Contains(callbackTwo, newComponent.SubscriptionDictionary[componentType]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    private void SubscriptionComponent_OnUnsubscribe_SubscriptionDoesNotExist_ThrowsArgumentException(
        bool keyExistsInDictionary)
    {
        // Arrange
        var actionOne = new Action<Entity, Component?, Component>(static (_, _, _) => { });
        var typeToUse = keyExistsInDictionary ? typeof(Component) : typeof(Type);
        var component = new SubscribeComponent(typeToUse, static (_, _, _) => { });

        // Act
        var exception = Record.Exception(() => component.Unsubscribe(typeof(Component), actionOne));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    [Fact]
    private void SubscriptionComponent_OnUnsubscribe_SubscriptionExists_Works()
    {
        // Arrange
        var actionOne = new Action<Entity, Component?, Component>(static (_, _, _) => { });
        var component = new SubscribeComponent(typeof(Component), actionOne);

        // Act
        var newComponent = component.Unsubscribe(typeof(Component), actionOne);

        // Assert
        Assert.DoesNotContain(actionOne, newComponent.SubscriptionDictionary[typeof(Component)]);
    }
}