using System;
using System.Runtime.InteropServices;
using Xeno.SourceGenerator.Sample;
using IComponent = Xeno.IComponent;

namespace SourceGenerator.Sample {
    [Guid("C060D394-C1A0-45F1-8531-0823C72C9978")]
    public struct Position : IComponent
    {
        public float x;
        public float y;
        public float z;
    }

    public class Program
    {
        public static void Main(string[] args) {
            Run(new WorldTests().Run);
            Run(new PerfTest().Run);
        }

        private static void Run(Action action) {
            Console.WriteLine($"{action.Target.GetType().Name} run...");
            try {
                action();
                Console.WriteLine("...OK!");
            } catch (Exception e) {
                Console.WriteLine("...ERROR!");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
