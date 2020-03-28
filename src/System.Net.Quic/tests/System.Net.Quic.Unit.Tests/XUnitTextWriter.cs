using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace System.Net.Quic.Tests
{
    internal class XUnitTextWriter : TextWriter
    {
        private ITestOutputHelper output;

        public XUnitTextWriter(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override Encoding Encoding { get; }

        public override void WriteLine()
        {
            output.WriteLine("");
        }
        
        public override void WriteLine(string line)
        {
            output.WriteLine(line);
        }
    }
}