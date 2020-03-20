// Hack to work around not being able to generate SR.cs from Strings.resx
namespace System
{
    internal partial class SR : System.Net.Quic.Resources.Strings
    {
    }
}
