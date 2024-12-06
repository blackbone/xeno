using Xeno.Vendor;

namespace Xeno.Tests;

[TestFixture]
public class BitSetTests {
    [Test]
    [TestCase(0, 1)]
    [TestCase(32, 1)]
    [TestCase(64, 1)]
    [TestCase(100, 2)]
    [TestCase(256, 4)]
    [TestCase(1024, 16)]
    [TestCase(16666, 261)]
    public unsafe void BitSet_Ctor(int capacity, int len) {
        if (capacity > Constants.MaxArchetypeComponents) {
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(capacity)]));
        }
        else {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(capacity)]);
            Assert.That(bitset.data.Length, Is.EqualTo(len));
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(16)]
    [TestCase(63)]
    [TestCase(64)]
    [TestCase(127)]
    [TestCase(128)]
    [TestCase(255)]
    [TestCase(256)]
    [TestCase(1023)]
    [TestCase(1024)]
    [TestCase(262144)]
    public unsafe void BitSet_Set(int count) {
        if (count > Constants.MaxArchetypeComponents) {
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(count)]));
        }
        else {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(count)]);

            var i = 0;
            while (i < count) {
                bitset.Set(i);
                Assert.That(bitset.data[i / 64], Is.EqualTo(BitOperations.Smear(1ul << (i % 64))));
                i++;
            }
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(16)]
    [TestCase(63)]
    [TestCase(64)]
    [TestCase(127)]
    [TestCase(128)]
    [TestCase(255)]
    [TestCase(256)]
    [TestCase(1023)]
    [TestCase(1024)]
    [TestCase(262144)]
    public unsafe void BitSet_Unset(int count) {
        if (count > Constants.MaxArchetypeComponents)
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(count)]));
        else if (count == 0) {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(count)]);
            Assert.That(bitset.data.Length, Is.EqualTo(1));
        }
        else {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(count)]);

            int i = 0;
            while (i < count)
                bitset.Set(i++);

            do {
                i--;
                bitset.Unset(i);
                var v = (ulong)(i % 64) == 0ul ? 0ul : BitOperations.Smear(1ul << (i % 64 - 1));
                Assert.That(bitset.data[i / 64], Is.EqualTo(v));

                var s = bitset.data[i / 64];
                // repeat unset to check nothing changed
                bitset.Unset(i);
                Assert.That(bitset.data[i / 64], Is.EqualTo(s));
            } while (i > 0);
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(63)]
    [TestCase(64)]
    [TestCase(127)]
    [TestCase(128)]
    [TestCase(255)]
    [TestCase(256)]
    [TestCase(1023)]
    [TestCase(1024)]
    [TestCase(262144)]
    public unsafe void BitSet_Get(int count) {
        if (count > Constants.MaxArchetypeComponents)
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(count)]));
        else if (count == 0) {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(count)]);
            Assert.That(bitset.data.Length, Is.EqualTo(1));
        }
        else {
            var bitset = new BitSet(stackalloc ulong[count]);

            Console.WriteLine(bitset.ToS());
            var i = 0;
            while (i < count) {
                bitset.Set(i++);
                Console.WriteLine(bitset.ToS());
            }

            Assert.That(bitset.Get(i - 1), Is.True);
            Assert.That(bitset.Get(i), Is.False);
        }
    }
}
