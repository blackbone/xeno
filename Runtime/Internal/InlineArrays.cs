using System.Runtime.InteropServices;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _2<T> where T : unmanaged
    {
        private T _0;
        private T _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _4<T> where T : unmanaged
    {
        private _2<T> _0;
        private _2<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _8<T> where T : unmanaged
    {
        private _4<T> _0;
        private _4<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _16<T> where T : unmanaged
    {
        private _8<T> _0;
        private _8<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _32<T> where T : unmanaged
    {
        private _16<T> _0;
        private _16<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _64<T> where T : unmanaged
    {
        private _32<T> _0;
        private _32<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _128<T> where T : unmanaged
    {
        private _64<T> _0;
        private _64<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _256<T> where T : unmanaged
    {
        private _128<T> _0;
        private _128<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _512<T> where T : unmanaged
    {
        private _256<T> _0;
        private _256<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _1024<T> where T : unmanaged
    {
        private _512<T> _0;
        private _512<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _2048<T> where T : unmanaged
    {
        private _1024<T> _0;
        private _1024<T> _1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _4096<T> where T : unmanaged
    {
        private _2048<T> _0;
        private _2048<T> _1;
    }
}