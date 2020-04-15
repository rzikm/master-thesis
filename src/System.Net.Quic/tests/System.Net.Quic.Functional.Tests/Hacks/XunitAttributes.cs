namespace System.Net.Quic.Tests
{
    // Dotnet runtime repo uses custom xUnit extensions, which are not available as public NuGet package
    
    public class ConditionalClassAttribute : Attribute
    {
        public ConditionalClassAttribute(Type type, string member)
        {
        }
    }

    public class ActiveIssueAttribute : Attribute
    {
        public ActiveIssueAttribute(string url)
        {
        }
    }
}