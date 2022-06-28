using Microsoft.AspNetCore.Components;

namespace AllWorldsCharacterSheet.Client.Shared
{
	public static class RazorComponentExtensions
	{
		public static void HandleIntermediateBind<T>(this ComponentBase _, ref T fieldBehind, T value, EventCallback<T> callback) where T : IEquatable<T>?
		{
			bool isFirstSet = fieldBehind == null;
			if (fieldBehind != null && fieldBehind.Equals(value)) // prevent infinite loop.
			{
				return;
			}

			fieldBehind = value;

			if (callback.HasDelegate && !isFirstSet)
			{
				callback.InvokeAsync(value);
			}
		}
	}
}
