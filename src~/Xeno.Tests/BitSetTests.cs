using Xeno.Vendor;

namespace Xeno.Tests;

[TestFixture]
public class BitSetTests {
    [Test]
    [TestCase(0, 1)]
    [TestCase(32, 1)]
    [TestCase(63, 1)]
    [TestCase(64, 2)]
    [TestCase(100, 2)]
    [TestCase(255, 4)]
    [TestCase(256, 5)]
    [TestCase(1023, 16)]
    [TestCase(1024, 17)]
    [TestCase(Constants.MaxArchetypeComponents, 261)]
    public unsafe void BitSet_Ctor(int max, int len)
    {
        if (max >= Constants.MaxArchetypeComponents)
        {
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(max)]));
        }
        else
        {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(max)]);
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
        if (count >= Constants.MaxArchetypeComponents) {
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
    public unsafe void BitSet_Unset(int count)
    {
        if (count >= Constants.MaxArchetypeComponents)
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(count)]));
        else if (count == 0)
        {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(count)]);
            Assert.That(bitset.data.Length, Is.EqualTo(1));
        }
        else
        {
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
        if (count >= Constants.MaxArchetypeComponents)
            Assert.Throws<IndexOutOfRangeException>(() => new BitSet(stackalloc ulong[BitSet.MaskSize(count)]));
        else if (count == 0)
        {
            var bitset = new BitSet(stackalloc ulong[BitSet.MaskSize(count)]);
            Assert.That(bitset.data.Length, Is.EqualTo(1));
        }
        else
        {
            var bitset = new BitSet(stackalloc ulong[count]);

            var i = 0;
            while (i < count)
                bitset.Set(i++);

            Assert.That(bitset.Get(i - 1), Is.True);
            Assert.That(bitset.Get(i), Is.False);
        }
    }

    [Test]
    public unsafe void BitSet_FromAdd_PreservesOriginWordsWhenAddMaskIsShorter() {
        var origin = new BitSet(stackalloc ulong[BitSet.MaskSize(70)]);
        origin.Set(70).FinalizeHash();
        var originMask = origin.AsReadOnly();

        var add = new BitSet(stackalloc ulong[BitSet.MaskSize(1)]);
        add.Set(1).FinalizeHash();
        var addMask = add.AsReadOnly();

        var result = new BitSet(stackalloc ulong[BitSet.MaskSize(70)]);
        result.FromAdd(originMask, addMask);

        Assert.That(result.data[0], Is.EqualTo(1ul << 1));
        Assert.That(result.data[1], Is.EqualTo(1ul << 6));
        Assert.That(result.max, Is.EqualTo(70));
        Assert.That(result.maskSize, Is.EqualTo(2));
    }

    [Test]
    public unsafe void BitSet_FromRemove_RecalculatesMaxAndIndices() {
        var origin = new BitSet(stackalloc ulong[BitSet.MaskSize(70)]);
        origin.Set(1).Set(70).FinalizeHash();
        var originMask = origin.AsReadOnly();

        var remove = new BitSet(stackalloc ulong[BitSet.MaskSize(70)]);
        remove.Set(70).FinalizeHash();
        var removeMask = remove.AsReadOnly();

        var result = new BitSet(stackalloc ulong[BitSet.MaskSize(70)]);
        result.FromRemove(originMask, removeMask);
        var resultMask = result.AsReadOnly();

        Assert.That(result.data[0], Is.EqualTo(1ul << 1));
        Assert.That(result.data[1], Is.Zero);
        Assert.That(result.max, Is.EqualTo(1));
        Assert.That(result.maskSize, Is.EqualTo(1));
        Assert.That(resultMask.indices, Is.EqualTo(new uint[] { 1 }));
    }
}
