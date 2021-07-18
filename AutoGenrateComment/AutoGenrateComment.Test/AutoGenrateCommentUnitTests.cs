using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = AutoGenrateComment.Test.CSharpCodeFixVerifier<
    AutoGenrateComment.AutoGenrateCommentAnalyzer,
    AutoGenrateComment.AutoGenrateCommentCodeFixProvider>;

namespace AutoGenrateComment.Test
{
    public class AutoGenrateCommentUnitTest// : CodeFixVerifier<AutoGenrateCommentAnalyzer, AutoGenrateCommentCodeFixProvider, CSharpCodeFixTest<AutoGenrateCommentAnalyzer, AutoGenrateCommentCodeFixProvider, XUnitVerifier>, XUnitVerifier>
    {
        //No diagnostics expected to show up
        [Fact]
        public async Task TestMethod1()
        {
            var test = @"
class TypeName
{
    public void Test()
    {
        var a = ""fuck you"";
        string.Equals(a, ""fuck you"");
    }
}";

            var expected = VerifyCS.Diagnostic("AutoGenrateComment")
                .WithSpan(7, 9, 7, 37)
                .WithSeverity(DiagnosticSeverity.Info)
                .WithMessage("StringComparison is missing");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
