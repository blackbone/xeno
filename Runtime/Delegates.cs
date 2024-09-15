namespace Xeno {

    public delegate void ComponentDelegate<T>(ref T component)
        where T : struct, IComponent;

    public delegate void ComponentDelegate<T1, T2>(ref T1 component1, ref T2 component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent;

    public delegate void ComponentDelegate<T1, T2, T3>(ref T1 component1, ref T2 component2, ref T3 component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent;

    public delegate void ComponentDelegate<T1, T2, T3, T4>(ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent;

    public delegate void EntityComponentDelegate<T>(in Entity entity, ref T component)
        where T : struct, IComponent;

    public delegate void EntityComponentDelegate<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent;

    public delegate void EntityComponentDelegate<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent;

    public delegate void EntityComponentDelegate<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent;

    public delegate void UniformComponentDelegate<TU, T1>(in TU uniform, ref T1 component)
        where T1 : struct, IComponent;

    public delegate void UniformComponentDelegate<TU, T1, T2>(in TU uniform, ref T1 component1, ref T2 component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent;

    public delegate void UniformComponentDelegate<TU, T1, T2, T3>(in TU uniform, ref T1 component1, ref T2 component2, ref T3 component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent;

    public delegate void UniformComponentDelegate<TU, T1, T2, T3, T4>(in TU uniform, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent;

    public delegate void EntityUniformComponentDelegate<TU, T1>(in Entity entity, in TU delta, ref T1 component)
        where T1 : struct, IComponent;

    public delegate void EntityUniformComponentDelegate<TU, T1, T2>(in Entity entity, in TU delta, ref T1 component1, ref T2 component2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent;

    public delegate void EntityUniformComponentDelegate<TU, T1, T2, T3>(in Entity entity, in TU delta, ref T1 component1, ref T2 component2, ref T3 component3)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent;

    public delegate void EntityUniformComponentDelegate<TU, T1, T2, T3, T4>(in Entity entity, in TU delta, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent;
}