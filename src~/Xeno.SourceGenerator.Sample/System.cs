namespace Xeno.Tests;

public struct Component1 : IComponent;

[System]
public partial class SystemX<T> where T : struct, IComponent
{
    [SystemMethod(SystemMethodKind.Update)]
    private void Update(ref T c1) { }
    
    [SystemMethod(SystemMethodKind.Update)]
    private void Update2(ref Component1 c1) { }
}