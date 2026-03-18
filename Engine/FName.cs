using System;

namespace FlappyBird.Engine;

public readonly struct FName : IEquatable<FName>
{
    private readonly string _internedName;
    private readonly int _hashCode;

    public FName(string name)
    {
        _internedName = string.Intern(name ?? throw new ArgumentNullException(nameof(name)));
        _hashCode = _internedName.GetHashCode();
    }

    public override bool Equals(object obj) => obj is FName other && Equals(other);
    public bool Equals(FName other) => _hashCode == other._hashCode && _internedName == other._internedName;
    public override int GetHashCode() => _hashCode;
    public static bool operator ==(FName left, FName right) => left.Equals(right);
    public static bool operator !=(FName left, FName right) => !left.Equals(right);
    public override string ToString() => _internedName;
}
