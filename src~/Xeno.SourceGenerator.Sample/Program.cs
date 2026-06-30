using System;
using Xeno.SourceGenerator.Sample;

namespace SourceGenerator.Sample {
    public class Program
    {
        public static void Main(string[] args) {
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
