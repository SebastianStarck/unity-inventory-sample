using System;

namespace Generic
{
    /// <summary> Enum Extension Methods </summary>
    /// <typeparam name="T"> type of Enum </typeparam>
    public class Enum<T> where T : Enum
    {
        public static T[] Values => (T[])Enum.GetValues(typeof(T));

        public static T Start => (T)Values.GetValue(0);
        public static T End => (T)Values.GetValue(Values.Length - 1);

        public static T Random
        {
            get
            {
                int index = UnityEngine.Random.Range(0, Values.Length - 1);
                return (T)Values.GetValue(index);
            }
        }

        public static int Length => Enum.GetValues(typeof(T)).Length;
    }
}
