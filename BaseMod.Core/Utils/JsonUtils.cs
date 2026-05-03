using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseMod.Core.Utils;
public static class JsonUtils
{
    public static bool TryDeserialize<T>(string foldername, string filename, [NotNullWhen(true)] out T result)
        => TryDeserialize($"{foldername}\\{filename}", out result);

    public static bool TryDeserialize<T>(string filepath, [NotNullWhen(true)] out T result)
    {
        result = default;

        try
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"File not exist: '{filepath}'", filepath);
            }

            T json = JsonSerializer.Deserialize<T>(File.ReadAllText(filepath), Options) ?? throw new Exception($"Can't deserialize file '{filepath}' to type '{typeof(T)}'");

            result = json;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool TryDeserializeFolder<T>(string folder, string searchPattern, [NotNullWhen(true)] out List<T> resultList)
    {
        resultList = [];

        try
        {
            if (!Directory.Exists(folder))
            {
                _ = Directory.CreateDirectory(folder);
            }

            foreach (string filepath in Directory.EnumerateFiles(folder, searchPattern))
            {
                if (TryDeserialize(filepath, out T result))
                {
                    resultList.Add(result);
                }
            }

            return resultList.Count > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool TrySerialize<T>(string folder, string filename, T jsonData, bool rewriteIfExist = true)
    {
        try
        {
            if (!Directory.Exists(folder))
            {
                _ = Directory.CreateDirectory(folder);
            }

            if (!rewriteIfExist && File.Exists($"{folder}\\{filename}"))
            {
                return false;
            }

            File.WriteAllText($"{folder}\\{filename}", JsonSerializer.Serialize(jsonData, Options));

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static readonly JsonSerializerOptions Options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
}
