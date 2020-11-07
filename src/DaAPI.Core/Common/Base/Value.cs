using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using DaAPI.Core.Helper;
using System.Diagnostics.CodeAnalysis;

namespace DaAPI.Core.Common
{

    public abstract class Value<T> where T : Value<T>
    {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        static readonly Member[] Members = GetMembers().ToArray();

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == typeof(T) && Members.All(
                       m =>
                       {
                           var otherValue = m.GetValue(other);
                           var thisValue = m.GetValue(this);

                           return m.IsNonStringEnumerable
                               ? GetEnumerableValues(otherValue).SequenceEqual(GetEnumerableValues(thisValue))
                               : otherValue?.Equals(thisValue) ?? thisValue == null;
                       }
                   );
        }

        public override int GetHashCode()
            => CombineHashCodes(
                Members.Select(
                    m => m.IsNonStringEnumerable
                        ? CombineHashCodes(GetEnumerableValues(m.GetValue(this)))
                        : m.GetValue(this)
                )
            );

        public static bool operator ==(Value<T> left, Value<T> right) => Equals(left, right);

        public static bool operator !=(Value<T> left, Value<T> right) => !Equals(left, right);

        public override string ToString()
        {
            if (Members.Length == 1)
            {
                var m = Members[0];
                var value = m.GetValue(this);

                return m.IsNonStringEnumerable
                    ? $"{string.Join("|", GetEnumerableValues(value))}"
                    : value.ToString();
            }

            var values = Members.Select(
                m =>
                {
                    var value = m.GetValue(this);

                    return m.IsNonStringEnumerable
                        ? $"{m.Name}:{string.Join("|", GetEnumerableValues(value))}"
                        : m.Type != typeof(string)
                            ? $"{m.Name}:{value}"
                            : value == null
                                ? $"{m.Name}:null"
                                : $"{m.Name}:\"{value}\"";
                }
            );
            return $"{typeof(T).Name}[{string.Join("|", values)}]";
        }

        static IEnumerable<Member> GetMembers()
        {
            var t = typeof(T);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            while (t != typeof(object))
            {
                if (t == null) continue;

                foreach (var p in t.GetProperties(flags)) yield return new Member(p);
                foreach (var f in t.GetFields(flags)) yield return new Member(f);

                t = t.BaseType;
            }
        }

        static IEnumerable<object> GetEnumerableValues(object obj)
        {
            var enumerator = ((IEnumerable)obj).GetEnumerator();
            while (enumerator.MoveNext()) yield return enumerator.Current;
        }

        static int CombineHashCodes(IEnumerable<object> objs)
        {
            unchecked
            {
                return objs.Aggregate(17, (current, obj) => current * 59 + (obj?.GetHashCode() ?? 0));
            }
        }

        struct Member
        {
            public readonly string Name;
            public readonly Func<object, object> GetValue;
            public readonly bool IsNonStringEnumerable;
            public readonly Type Type;

            public Member(MemberInfo info)
            {
                switch (info)
                {
                    case FieldInfo field:
                        Name = field.Name;
                        GetValue = obj => field.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(field.FieldType) &&
                                                field.FieldType != typeof(string);
                        Type = field.FieldType;
                        break;
                    case PropertyInfo prop:
                        Name = prop.Name;
                        GetValue = obj => prop.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                                                prop.PropertyType != typeof(string);
                        Type = prop.PropertyType;
                        break;
                    default:
                        throw new ArgumentException("Member is not a field or property?!?!", info.Name);
                }
            }
        }
    }

    public abstract class Value
    {
        private static readonly Dictionary<Type, Member[]> _members = new Dictionary<Type, Member[]>();

        public static void InitalizeMembers(params Assembly[] assemblies)
        {
            var referencedAssemblies = AssemblyHelper.GetReferencedAssembliesRecursive(false, assemblies.AsEnumerable());

            foreach (var assembly in referencedAssemblies)
            {
                Type[] types = assembly.Value.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(Value)) == false)
                    {
                        continue;
                    }

                    if (_members.ContainsKey(type) == true) { continue; }
                    _members.Add(type, GetMembers(type).ToArray());
                }
            }
        }

        static Value()
        {
            InitalizeMembers();
        }

        protected static void InitialzeMembers(Type type)
        {
            if (_members.ContainsKey(type) == false)
            {
                _members.Add(type, GetMembers(type).ToArray());
            }
        }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == this.GetType() && _members[this.GetType()].All(
                       m =>
                       {
                           var otherValue = m.GetValue(other);
                           var thisValue = m.GetValue(this);

                           return m.IsNonStringEnumerable
                               ? GetEnumerableValues(otherValue).SequenceEqual(GetEnumerableValues(thisValue))
                               : otherValue?.Equals(thisValue) ?? thisValue == null;
                       }
                   );
        }

        public override int GetHashCode()
                => CombineHashCodes(
                    _members[this.GetType()].Select(
                        m => m.IsNonStringEnumerable
                            ? CombineHashCodes(GetEnumerableValues(m.GetValue(this)))
                            : m.GetValue(this)
                    )
                );

        public static bool operator ==(Value left, Value right) => Equals(left, right);

        public static bool operator !=(Value left, Value right) => !Equals(left, right);

        public override string ToString()
        {

            if (_members[this.GetType()].Length == 1)
            {
                var m = _members[this.GetType()][0];
                var value = m.GetValue(this);

                return m.IsNonStringEnumerable
                    ? $"{string.Join("|", GetEnumerableValues(value))}"
                    : value.ToString();
            }

            var values = _members[this.GetType()].Select(
                m =>
                {
                    var value = m.GetValue(this);

                    return m.IsNonStringEnumerable
                        ? $"{m.Name}:{string.Join("|", GetEnumerableValues(value))}"
                        : m.Type != typeof(string)
                            ? $"{m.Name}:{value}"
                            : value == null
                                ? $"{m.Name}:null"
                                : $"{m.Name}:\"{value}\"";
                }
            );
            return $"{this.GetType().Name}[{string.Join("|", values)}]";
        }

        private static IEnumerable<Member> GetMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            while (type != typeof(object))
            {
                if (type == null) continue;

                foreach (var property in type.GetProperties(flags))
                {
                    yield return new Member(property);
                }

                foreach (var field in type.GetFields(flags))
                {
                    yield return new Member(field);
                }

                type = type.BaseType;
            }
        }

        private static IEnumerable<object> GetEnumerableValues(object obj)
        {
            var enumerator = ((IEnumerable)obj).GetEnumerator();
            while (enumerator.MoveNext() == true)
            {
                yield return enumerator.Current;
            }
        }

        private static int CombineHashCodes(IEnumerable<object> objs)
        {
            unchecked
            {
                return objs.Aggregate(17, (current, obj) => current * 59 + (obj?.GetHashCode() ?? 0));
            }
        }

        private struct Member
        {
            public readonly String Name;
            public readonly Func<Object, Object> GetValue;
            public readonly Boolean IsNonStringEnumerable;
            public readonly Type Type;

            public Member(MemberInfo info)
            {
                switch (info)
                {
                    case FieldInfo field:
                        Name = field.Name;
                        GetValue = obj => field.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(field.FieldType) &&
                                                field.FieldType != typeof(string);
                        Type = field.FieldType;
                        break;
                    case PropertyInfo prop:
                        Name = prop.Name;
                        GetValue = obj => prop.GetValue(obj);

                        IsNonStringEnumerable = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                                                prop.PropertyType != typeof(string);
                        Type = prop.PropertyType;
                        break;
                    default:
                        throw new ArgumentException("Member is not a field or property?!?!", info.Name);
                }
            }
        }
    }

}
