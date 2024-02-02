using System.Reflection;

namespace Tests
{
    public class ScratchFileTests
    {
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Can create instance
        /// </summary>
        [Test]
        public void TestCreation()
        {
            string? fileName;
            using (var sf = new ScratchFile())
            {
                fileName = typeof(ScratchFile)
                    .GetField("filename", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(sf)
                    ?.ToString();
            }
            Assert.That(fileName, Is.Not.Null);
            TestContext.WriteLine("Temp file name: {0}", fileName);
        }

        /// <summary>
        /// Created temp file should not be visible to other processes
        /// </summary>
        [Test]
        public void TestFileInvisible()
        {
            string? fileName;
            using var sf = new ScratchFile();
            fileName = typeof(ScratchFile)
                .GetField("filename", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(sf)
                ?.ToString();
            TestContext.WriteLine("Temp file name: {0}", fileName);
            Assert.That(File.Exists(fileName), Is.False);
        }

        /// <summary>
        /// Can read and write to scratch file
        /// </summary>
        [Test]
        public void TestReadWrite()
        {
            byte[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
            byte[] read = new byte[data.Length];
            using var sf = new ScratchFile();
            //Write data and verify size change
            sf.Write(data);
            Assert.Multiple(() =>
            {
                Assert.That(sf.Position, Is.EqualTo(data.Length));
                Assert.That(sf.Length, Is.EqualTo(sf.Position));
            });
            //Reset to start, read data, and verify contents
            sf.Position = 0;
            Assert.Multiple(() =>
            {
                Assert.That(sf.Read(read), Is.EqualTo(read.Length));
                Assert.That(data, Is.EquivalentTo(read));
            });
        }
    }
}