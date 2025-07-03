using System.Diagnostics.CodeAnalysis;

using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;

using UnityEngine;

namespace BaseMod.Core.Extensions;
public static class UnityExtensions {
    public static T? GetComponentWithCast<T>( this GameObject gameObject )
        where T : Il2CppObjectBase {
        return gameObject?.GetComponent( Il2CppType.From( typeof( T ) ) )?.TryCast<T>();
    }

    public static bool TryGetComponentWithCast<T>( this GameObject gameObject, [NotNullWhen( true )] out T? result )
        where T : Il2CppObjectBase {
        result = gameObject.GetComponentWithCast<T>();

        return result is not null;
    }

    public static void AddComponentWithAction<T>( this GameObject gameObject, Action<T> action )
        where T : Il2CppObjectBase {
        if (!gameObject.TryGetComponentWithCast(out T? component)) {
            component = gameObject.AddComponent(Il2CppType.From(typeof(T))).TryCast<T>();
        }

        if (component is not null)
            action?.Invoke( component );
    }
}