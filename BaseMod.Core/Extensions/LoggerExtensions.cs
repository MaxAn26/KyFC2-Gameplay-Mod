using System.Runtime.CompilerServices;

using BepInEx.Logging;

namespace BaseMod.Core.Extensions;
public static class LoggerExtensions {
    public static void Debug( this ManualLogSource self, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        var className = Path.GetFileNameWithoutExtension(filePath);
        self.LogDebug( $"[{className}.{callerName}] {message}" );
    }

    public static void Info( this ManualLogSource self, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        var className = Path.GetFileNameWithoutExtension(filePath);
        self.LogInfo( $"[{className}.{callerName}] {message}" );
    }

    public static void Message( this ManualLogSource self, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        var className = Path.GetFileNameWithoutExtension(filePath);
        self.LogMessage( $"[{className}.{callerName}] {message}" );
    }

    public static void Warn( this ManualLogSource self, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        var className = Path.GetFileNameWithoutExtension(filePath);
        self.LogWarning( $"[{className}.{callerName}] {message}" );
    }

    public static void Error( this ManualLogSource self, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        var className = Path.GetFileNameWithoutExtension(filePath);
        self.LogError( $"[{className}.{callerName}] {message}" );
    }

    public static void Error( this ManualLogSource self, Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        self.Error( exception.Message, callerName, filePath );
    }

    public static void Fatal( this ManualLogSource self, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        var className = Path.GetFileNameWithoutExtension(filePath);
        self.LogFatal( $"[{className}.{callerName}] {message}" );
    }

    public static void Fatal( this ManualLogSource self, Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string filePath = "" ) {
        self.Fatal( exception.Message, callerName, filePath );
    }
}