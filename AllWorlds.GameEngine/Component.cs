using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Allworlds.GameEngine.UnitTests")]
namespace AllWorlds.GameEngine
{
	/// <summary>
	/// Components are the building blocks of entities.
	/// 
	/// This is the interface that all components must use.
	/// Components should only contain data in the form of properties.
	/// Typically, anything derived from this interface should be a struct or record rather than a class.
	///
	/// A struct will ensure that the component only belongs to one Entity. (Since it's a value type).
	///
	/// A record will allow for two Entities to share a component (ref type), but will be immutable.
	/// Immutability prevents changes in general and prevents the potentially unintended side effects of two entities
	/// sharing a component.
	///
	/// A class can be used and may even be desired in certain cases. But the developer must be careful to 
	/// </summary>
	public record Component : IEquatable<Component>
	{
		public virtual Component HandleDuplicate(Component newComponent)
		{
			throw new ArgumentException("Duplicates are not supported for this Component type.");
		}
	}
}