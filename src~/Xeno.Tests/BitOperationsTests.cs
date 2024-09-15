// using Xeno.Vendor;
//
// namespace Xeno.Tests;
//
// [TestFixture]
// public class BitOperationsTests {
//     [Test]
//     [TestCase(0b10, 0b11)]
//     [TestCase(0b100, 0b111)]
//     [TestCase(0b1001, 0b1111)]
//     [TestCase(0b1111, 0b1111)]
//     [TestCase(0b0000, 0b0)]
//     public void BitOperations_Smear(int value, int check) {
//         var result = BitOperations.Smear(value);
//         Assert.That(result, Is.EqualTo(check));
//     }
//
//     [Test]
//     [TestCase(0b10u, 0b11u)]
//     [TestCase(0b100u, 0b111u)]
//     [TestCase(0b1001u, 0b1111u)]
//     [TestCase(0b1111u, 0b1111u)]
//     [TestCase(0b0000u, 0b0u)]
//     public void BitOperations_Smear(uint value, uint check) {
//         var result = BitOperations.Smear(value);
//         Assert.That(result, Is.EqualTo(check));
//     }
//
//     [Test]
//     [TestCase(0b10L, 0b11L)]
//     [TestCase(0b100L, 0b111L)]
//     [TestCase(0b1001L, 0b1111L)]
//     [TestCase(0b1111L, 0b1111L)]
//     [TestCase(0b0000L, 0b0L)]
//     public void BitOperations_Smear(long value, long check) {
//         var result = BitOperations.Smear(value);
//         Assert.That(result, Is.EqualTo(check));
//     }
//
//     [Test]
//     [TestCase(0b10ul, 0b11ul)]
//     [TestCase(0b100ul, 0b111ul)]
//     [TestCase(0b1001ul, 0b1111ul)]
//     [TestCase(0b1111ul, 0b1111ul)]
//     [TestCase(0b0000ul, 0b0ul)]
//     public void BitOperations_Smear(ulong value, ulong check) {
//         var result = BitOperations.Smear(value);
//         Assert.That(result, Is.EqualTo(check));
//     }
//
//     [Test]
//     [TestCase(0b10, 2)]
//     [TestCase(0b100, 3)]
//     [TestCase(0b1001, 4)]
//     [TestCase(0b1111, 4)]
//     [TestCase(0b0000, 0)]
//     public void BitOperations_PopCap(int value, int count) {
//         var result = BitOperations.PopCap(value);
//         Assert.That(result, Is.EqualTo(count));
//     }
//
//     [Test]
//     [TestCase(0b10u, 2)]
//     [TestCase(0b100u, 3)]
//     [TestCase(0b1001u, 4)]
//     [TestCase(0b1111u, 4)]
//     [TestCase(0b0000u, 0)]
//     public void BitOperations_PopCap(uint value, int count) {
//         var result = BitOperations.PopCap(value);
//         Assert.That(result, Is.EqualTo(count));
//     }
//
//     [Test]
//     [TestCase(0b10L, 2)]
//     [TestCase(0b100L, 3)]
//     [TestCase(0b1001L, 4)]
//     [TestCase(0b1111L, 4)]
//     [TestCase(0b0000L, 0)]
//     public void BitOperations_PopCap(long value, int count) {
//         var result = BitOperations.PopCap(value);
//         Assert.That(result, Is.EqualTo(count));
//     }
//
//     [Test]
//     [TestCase(0b10ul, 2)]
//     [TestCase(0b100ul, 3)]
//     [TestCase(0b1001ul, 4)]
//     [TestCase(0b1111ul, 4)]
//     [TestCase(0b0000ul, 0)]
//     public void BitOperations_PopCap(ulong value, int count) {
//         var result = BitOperations.PopCap(value);
//         Assert.That(result, Is.EqualTo(count));
//     }
//
//     [Test]
//     [TestCase(0b10, 1)]
//     [TestCase(0b100, 1)]
//     [TestCase(0b1001, 2)]
//     [TestCase(0b1111, 4)]
//     [TestCase(0b0000, 0)]
//     public void BitOperations_PopCount(int value, int count) {
//         var result = BitOperations.PopCount(value);
//         Assert.That(result, Is.EqualTo(count));
//     }
//
//     [Test]
//     [TestCase(0b10L, 1)]
//     [TestCase(0b100L, 1)]
//     [TestCase(0b1001L, 2)]
//     [TestCase(0b1111L, 4)]
//     [TestCase(0b0000L, 0)]
//     public void BitOperations_PopCount(long value, int count) {
//         var result = BitOperations.PopCount(value);
//         Assert.That(result, Is.EqualTo(count));
//     }
// }