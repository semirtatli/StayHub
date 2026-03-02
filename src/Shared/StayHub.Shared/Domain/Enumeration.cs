namespace StayHub.Shared.Domain;

/// <summary>
/// Enumeration base class — a DDD pattern for type-safe enums with behavior.
/// Unlike C# enums, these can have methods, descriptions, and validation logic.
/// Example: BookingStatus.Pending.CanTransitionTo(BookingStatus.Confirmed)
/// </summary>
public abstract class Enumeration : IComparable
{
    public int Id { get; private init; }
    public string Name { get; private init; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
            return false;

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? obj) =>
        obj is Enumeration otherEnum
            ? Id.CompareTo(otherEnum.Id)
            : throw new ArgumentException("Cannot compare to non-Enumeration type.");

    public static bool operator ==(Enumeration? left, Enumeration? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Enumeration? left, Enumeration? right) =>
        !(left == right);

    public static bool operator <(Enumeration left, Enumeration right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(Enumeration left, Enumeration right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(Enumeration left, Enumeration right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(Enumeration left, Enumeration right) =>
        left.CompareTo(right) >= 0;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();

    public static T FromId<T>(int id) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(e => e.Id == id)
        ?? throw new InvalidOperationException($"No {typeof(T).Name} with Id {id}.");

    public static T FromName<T>(string name) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"No {typeof(T).Name} with Name '{name}'.");
}
