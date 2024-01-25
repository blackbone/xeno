// using System;
// using System.Runtime.CompilerServices;
//
// namespace Xeno
// {
//     public interface IWorld : IDisposable
//     {
//         public string Name
//         {
//             [MethodImpl(MethodImplOptions.AggressiveInlining)]
//             get;
//         }
//
//         public byte Id
//         {
//             [MethodImpl(MethodImplOptions.AggressiveInlining)]
//             get;
//         }
//
//         uint EntityCount
//         {
//             [MethodImpl(MethodImplOptions.AggressiveInlining)]
//             get;
//         }
//
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public Entity CreateEntity();
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public Entity CreateEntity<T>(in T component)
//             where T : unmanaged, IComponent;
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public Entity CreateEntity<T1, T2>(in T1 component1, in T2 component2)
//             where T1 : unmanaged, IComponent
//             where T2 : unmanaged, IComponent;
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public Entity CreateEntity<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
//             where T1 : unmanaged, IComponent
//             where T2 : unmanaged, IComponent
//             where T3 : unmanaged, IComponent;
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public Entity CreateEntity<T1, T2, T3, T4>(in T1 component1, in T2 component2, in T3 component3, in T4 component4)
//             where T1 : unmanaged, IComponent
//             where T2 : unmanaged, IComponent
//             where T3 : unmanaged, IComponent
//             where T4 : unmanaged, IComponent;
//         
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         void DeleteEntity(in Entity entity);
//         
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public void PreUpdate();
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public void Update();
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public void PostUpdate();
//     }
// }

