namespace Tests
{
    public class DumpTest
    {
        [Test]
        public void DumpCorrectness()
        {
            const string Expected = "0x00000000\t00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F \t.☺☻♥♦♣♠•◘○◙♂♀♪♫☼";
            var buffer = Enumerable.Range(0, 16).Select(m => (byte)m).ToArray();
            var dump = Dump.HexDump(buffer);
            TestContext.WriteLine(dump);
            Assert.That(dump, Is.EqualTo(Expected));
        }

        [Test]
        public void CreateDump()
        {
            //Note: This test will fail if buffer length is not a multiple of 16
            var buffer = new byte[128];
            Random.Shared.NextBytes(buffer);
            var dump = Dump.HexDump(buffer);
            TestContext.WriteLine(dump);
            var lineLength =
                //Offset plus tab
                10 + 1 +
                //Hex part plus tab
                (16 * 3) + 1 +
                //Text part plus EOL
                16 + Environment.NewLine.Length;
            var lineCount = buffer.Length / 16;
            //Remove last CRLF because it will not be there
            Assert.That(dump, Has.Length.EqualTo((lineLength * lineCount) - Environment.NewLine.Length));
        }

        [Test]
        public void CreateDumpCustomWidth()
        {
            var buffer = new byte[19];
            Random.Shared.NextBytes(buffer);
            var dump = Dump.HexDump(buffer, 7);
            TestContext.WriteLine(dump);
            var lines = dump.Split('\n').Select(m => m.TrimEnd()).ToArray();
            //Ensure the number of lines matches
            Assert.That(lines, Has.Length.EqualTo(Math.Ceiling(buffer.Length / 7.0)));
        }
    }
}
