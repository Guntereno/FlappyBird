using System;
using System.Collections.Generic;

namespace FlappyBird.Engine;

public readonly struct FName : IEquatable<FName>
{
    private static readonly Dictionary<string, int> _nameToId = new(StringComparer.Ordinal);
    private static readonly List<string> _idToName = new();

    private readonly int _id;

    public FName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (_nameToId.TryGetValue(name, out int existingId))
        {
            _id = existingId;
            return;
        }

        // Create a new ID
        int newId = _idToName.Count;
        _nameToId[name] = newId;
        _idToName.Add(name);

        _id = newId;
    }

    public override string ToString() => _idToName[_id];

    public bool Equals(FName other) => _id == other._id;
    public override bool Equals(object obj) => obj is FName other && Equals(other);

    public override int GetHashCode() => _id;

    public static bool operator ==(FName left, FName right) => left._id == right._id;
    public static bool operator !=(FName left, FName right) => left._id != right._id;
}
