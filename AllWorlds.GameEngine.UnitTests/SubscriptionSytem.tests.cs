using Moq;
using Moq.Language;
using System.Collections.Generic;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class SubscriptionSystemTests
{
	private readonly List<CallbackArgs> _callbackArgs = new();
	private readonly Mock<Entity> _defaultEntityMock;

	public SubscriptionSystemTests()
	{
		_defaultEntityMock = new Mock<Entity>();
		_defaultEntityMock.SetupGet(static entity => entity.Id).Returns("");
	}

	private ISetupSequentialResult<Component> SetupSequentialTestComponent(Mock<Entity> entityMock)
	{
		entityMock.Setup(static entity => entity.HasComponent(typeof(TestComponent))).Returns(true);
		return entityMock.SetupSequence(static entity => entity.GetComponentByType(typeof(TestComponent)));
	}

	private ISetupSequentialResult<SubscribeComponent> SetupSequentialSubscribeComponent(Mock<Entity> entityMock)
	{
		entityMock.Setup(static entity => entity.HasComponent(typeof(SubscribeComponent))).Returns(true);
		return entityMock.SetupSequence(static entity => entity.GetComponentByType<SubscribeComponent>());
	}

	private SubscriptionSystem SetupSystem(Entity entity)
	{
		Mock<EntityPool>? entityPoolMock = new Mock<EntityPool>();
		entityPoolMock.Setup(static pool => pool.HasEntity(It.IsAny<Entity>())).Returns(true);

		SubscriptionSystem? system = new SubscriptionSystem();
		system.BindEntityPool(entityPoolMock.Object);
		system.UpdateEntityRegistration(entity);

		return system;
	}

	private void DefaultCallback(Entity entity, Component? oldComponent, Component? newComponent)
	{
		_callbackArgs.Add(new CallbackArgs(entity, oldComponent, newComponent));
	}

	[Fact]
	private void SubscriptionSystem_OnFirstRunAfterSubscribe_EntityHasComponent_DoesNotInvokesCallback()
	{
		// Arrange
		TestComponent? testComponent = new TestComponent(0);
		SubscribeComponent subscribeComponent = new SubscribeComponent(typeof(TestComponent), DefaultCallback).HandleDuplicate(new SubscribeComponent(typeof(SubscribeComponent), DefaultCallback));

		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(TestComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<TestComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<TestComponent>()).Returns(testComponent);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType(typeof(TestComponent))).Returns(testComponent);

		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(SubscribeComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<SubscribeComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<SubscribeComponent>()).Returns(subscribeComponent);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType(typeof(SubscribeComponent))).Returns(subscribeComponent);

		SubscriptionSystem? system = SetupSystem(_defaultEntityMock.Object);

		// Act
		system.Run();

		// Assert
		Assert.Empty(_callbackArgs);
	}

	[Fact]
	private void SubscriptionSystem_OnFirstRunAfterSubscribe_EntityDoesNotHaveComponent_DoesNotInvokesCallback()
	{
		// Arrange
		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(TestComponent))).Returns(false);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<TestComponent>()).Returns(false);

		SubscribeComponent subscribeComponent = new SubscribeComponent(typeof(TestComponent), DefaultCallback).HandleDuplicate(new SubscribeComponent(typeof(SubscribeComponent), DefaultCallback));
		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(SubscribeComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<SubscribeComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<SubscribeComponent>()).Returns(subscribeComponent);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType(typeof(SubscribeComponent))).Returns(subscribeComponent);

		SubscriptionSystem? system = SetupSystem(_defaultEntityMock.Object);

		// Act
		system.Run();

		// Assert
		Assert.Empty(_callbackArgs);
	}

	[Fact]
	private void SubscriptionSystem_OnSecondRunAfterSubscribe_ComponentHasNotChanged_DoesNotInvokeCallback()
	{
		// Arrange
		TestComponent? testComponent = new TestComponent(0);
		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(TestComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<TestComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<TestComponent>()).Returns(testComponent);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType(typeof(TestComponent))).Returns(testComponent);

		SubscribeComponent subscribeComponent = new SubscribeComponent(typeof(TestComponent), DefaultCallback).HandleDuplicate(new SubscribeComponent(typeof(SubscribeComponent), DefaultCallback));
		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(SubscribeComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<SubscribeComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<SubscribeComponent>()).Returns(subscribeComponent);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType(typeof(SubscribeComponent))).Returns(subscribeComponent);
		;

		SubscriptionSystem? system = SetupSystem(_defaultEntityMock.Object);

		// Act
		system.Run();
		system.Run();

		// Assert
		Assert.Empty(_callbackArgs);
	}

	[Fact]
	private void SubscriptionSystem_OnSecondRunAfterSubscribe_ComponentChanged_InvokesCallback()
	{
		// Arrange
		TestComponent? testComponentOne = new TestComponent(0);
		TestComponent? testComponentTwo = new TestComponent(1);
		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(TestComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<TestComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<TestComponent>()).Returns(testComponentOne);
		_defaultEntityMock.SetupSequence(static entity => entity.GetComponentByType(typeof(TestComponent)))
			.Returns(testComponentOne)  // OnSubscribeComponentChanged call
			.Returns(testComponentOne)  // Update call 1
			.Returns(testComponentTwo); // Update call 2

		SubscribeComponent subscribeComponent = new SubscribeComponent(typeof(TestComponent), DefaultCallback).HandleDuplicate(new SubscribeComponent(typeof(SubscribeComponent), DefaultCallback));
		_defaultEntityMock.Setup(static entity => entity.HasComponent(typeof(SubscribeComponent))).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.HasComponent<SubscribeComponent>()).Returns(true);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType<SubscribeComponent>()).Returns(subscribeComponent);
		_defaultEntityMock.Setup(static entity => entity.GetComponentByType(typeof(SubscribeComponent))).Returns(subscribeComponent);

		SubscriptionSystem? system = SetupSystem(_defaultEntityMock.Object);

		// Act
		system.Run();
		system.Run();

		// Assert
		Assert.Single(_callbackArgs);
		Assert.Equal(_defaultEntityMock.Object, _callbackArgs[0].Entity);
		Assert.Equal(testComponentOne, _callbackArgs[0].OldComponent);
		Assert.Equal(testComponentTwo, _callbackArgs[0].NewComponent);
	}

	private record CallbackArgs(Entity Entity, Component? OldComponent, Component? NewComponent);

	private sealed record TestComponent(int Value) : Component;
}