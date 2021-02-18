using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core
{
    public sealed class MutableBox<T> : IEquatable<MutableBox<T>>, IEquatable<T>
        where T : struct
    {
        public T Value { get; set; }

        public MutableBox() => Value = default;

        public MutableBox(T value) => Value = value;

        public override bool Equals(object obj) =>
            obj is MutableBox<T> box && Equals(box.Value)
            || obj is T value && Equals(value);

        public override int GetHashCode() => Value.GetHashCode();

        public bool Equals(MutableBox<T> other) => EqualityComparer<T>.Default.Equals(other.Value);

        public bool Equals(T other) => EqualityComparer<T>.Default.Equals(other);

        public static bool operator ==(MutableBox<T> left, MutableBox<T> right) => left.Equals(right);

        public static bool operator !=(MutableBox<T> left, MutableBox<T> right) => !(left == right);

        public static bool operator ==(MutableBox<T> left, T right) => left.Equals(right);

        public static bool operator !=(MutableBox<T> left, T right) => !(left == right);

        public static bool operator ==(T left, MutableBox<T> right) => right.Equals(left);

        public static bool operator !=(T left, MutableBox<T> right) => !(right == left);

        public static explicit operator T(MutableBox<T> box) => box.Value;

        public static explicit operator MutableBox<T>(T value) => new MutableBox<T>(value);

        public override string ToString() => Value.ToString();
    }
}
