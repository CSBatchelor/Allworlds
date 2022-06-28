using System;
using System.Runtime.CompilerServices;

namespace AllWorlds.GameEngine {
	public static class DoubleBuffer {

		internal delegate void SwapHandler(object sender);

		private static void DefaultSwap(object sender) { }

		/// <summary>
		/// Swaps the buffers for all DoubleBuffer types.
		/// </summary>
		internal static SwapHandler Swap = DefaultSwap;
	}

	/// <summary>
	/// An object that implements the Double Buffer design pattern.
	/// </summary>
	/// <typeparam name="T">Must be a value type (no objects).</typeparam>
	public class DoubleBuffer<T> {

		/// <summary>
		/// Create a value with two buffers.
		/// </summary>
		/// <param name="value">The value to initialize the buffer with.</param>
		public DoubleBuffer(T value) {
			if (typeof(T).IsValueType || value is string) {
				Current = value;
				Next = value;
				DoubleBuffer.Swap += Swap;
			} else {
				// Reference types wouldn't work without cloning, so I decided not to allow them.
				throw new NotSupportedException($"DoubleBuffer<{typeof(T).Name}> is not allowed, type must be a value type (or string).");
			}
				
		}

		private void Swap(object sender) {
			Current = Next;
		}

		/// <summary>
		/// Get the "Current" value in the double buffer.
		/// </summary>
		public T Current {
			get;
			private set;
		}

		/// <summary>
		/// Set the "Next" value of the double buffer.
		/// This will be the "Current" value after the buffer is swapped.
		/// </summary>
		public T Next { 
			private get;
			set;
		}

		/// <summary>
		/// Used to peek at the "Next" value in the double buffer.
		/// This should only be used to aid in setting the "Next" value.
		/// Any other decision making should use the "Current" property.
		/// </summary>
		/// <returns></returns>
		public T PeekNext() => Next;
	}
}
