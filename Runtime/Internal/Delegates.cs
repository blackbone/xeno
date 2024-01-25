namespace Xeno
{
    internal delegate void UpdateDelegate(in float delta);

    public delegate void ComponentDelegate<T>(ref T component)
        where T : unmanaged, IComponent;
    
    public delegate void ComponentDelegate<T1, T2>(ref T1 component1, ref T2 component2)
        where T1 : unmanaged, IComponent
        where T2 : unmanaged, IComponent;
    
    public delegate void ComponentDelegate<T1, T2, T3>(ref T1 component1, ref T2 component2, ref T3 component3)
        where T1 : unmanaged, IComponent
        where T2 : unmanaged, IComponent
        where T3 : unmanaged, IComponent;
    
    public delegate void ComponentDelegate<T1, T2, T3, T4>(ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        where T1 : unmanaged, IComponent
        where T2 : unmanaged, IComponent
        where T3 : unmanaged, IComponent;
    
    public delegate void DeltaComponentDelegate<T>(in float delta, ref T component)
        where T : unmanaged, IComponent;
    
    public delegate void DeltaComponentDelegate<T1, T2>(in float delta, ref T1 component1, ref T2 component2)
        where T1 : unmanaged, IComponent
        where T2 : unmanaged, IComponent;
    
    public delegate void DeltaComponentDelegate<T1, T2, T3>(in float delta, ref T1 component1, ref T2 component2, ref T3 component3)
        where T1 : unmanaged, IComponent
        where T2 : unmanaged, IComponent
        where T3 : unmanaged, IComponent;
    
    public delegate void DeltaComponentDelegate<T1, T2, T3, T4>(in float delta, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        where T1 : unmanaged, IComponent
        where T2 : unmanaged, IComponent
        where T3 : unmanaged, IComponent;
}