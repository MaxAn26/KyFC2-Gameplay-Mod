using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;

namespace BaseMod.Core;
public class ModConfig(string filename)
{
    private readonly Dictionary<string, MelonPreferences_Category> _categories = [];

    public MelonPreferences_Entry<T> Entry<T>(string categoryName, string entryName, T defaultValue, string description, AcceptableValue validator = null)
    {
        if (!_categories.TryGetValue(categoryName, out MelonPreferences_Category category))
        {
            category = MelonPreferences.CreateCategory(categoryName);
            category.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, filename));
            category.LoadFromFile();
            _categories.Add(categoryName, category);
        }

        if (validator is not null)
        {
            description = $"{description} ({validator.AdditionalMessage})";
        }

        return category.CreateEntry(entryName, defaultValue, description: description, validator: validator);
    }

    public void Save()
    {
        foreach (MelonPreferences_Category category in _categories.Values)
        {
            category.SaveToFile();
        }
    }

    public abstract class AcceptableValue : ValueValidator
    {
        public abstract string AdditionalMessage { get; }
    }

    public class AcceptableValueList<T>(IList<T> values) : AcceptableValue
    {
        public override string AdditionalMessage => $"Acceptable Values {string.Join(", ", values)}";

        public override object EnsureValid(object value)
        {
            if (value is not T castedValue || !values.Contains(castedValue))
            {
                return default;
            }

            return castedValue;
        }

        public override bool IsValid(object value) => value is T castedValue && values.Contains(castedValue);
    }

    public class AcceptableValueRange<T>(T minValue, T maxValue) : AcceptableValue where T : IComparable
    {
        public override string AdditionalMessage => $"Acceptable Range from {minValue} to {maxValue}";

        public override object EnsureValid(object value)
        {
            if (value is not T castedValue)
            {
                return minValue;
            }
            else if (maxValue.CompareTo(castedValue) < 0)
            {
                return maxValue;
            }
            else if (minValue.CompareTo(castedValue) > 0)
            {
                return minValue;
            }

            return castedValue;
        }

        public override bool IsValid(object value) => value is T castedValue && minValue.CompareTo(castedValue) <= 0 && maxValue.CompareTo(castedValue) >= 0;
    }
}
