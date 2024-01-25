namespace Xeno
{
    public class Query<T>
    {
        public delegate void InDelegate(in T t);

        public delegate void RefDelegate(ref T t);
    }


    public class Query<T1, T2>
    {
        public delegate void InInDelegate(in T1 t1, in T2 t2);

        public delegate void InRefDelegate(in T1 t1, ref T2 t2);

        public delegate void RefInDelegate(ref T1 t1, in T2 t2);

        public delegate void RefRefDelegate(ref T1 t1, ref T2 t2);
    }

    public static class QueryExtensions
    {
        public static void Iterate<T>(this Query<T> query, Query<T>.InDelegate inDelegate)
        {
        }

        public static void Iterate<T>(this Query<T> query, Query<T>.RefDelegate refDelegate)
        {
        }

        public static void Iterate<T1, T2>(this Query<T1, T2> query, Query<T1, T2>.InInDelegate inInDelegate)
        {
        }

        public static void Iterate<T1, T2>(this Query<T1, T2> query, Query<T1, T2>.InRefDelegate refDelegate)
        {
        }

        public static void Iterate<T1, T2>(this Query<T1, T2> query, Query<T1, T2>.RefInDelegate refDelegate)
        {
        }

        public static void Iterate<T1, T2>(this Query<T1, T2> query, Query<T1, T2>.RefRefDelegate refDelegate)
        {
        }
    }
}