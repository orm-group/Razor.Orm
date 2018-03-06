using Microsoft.AspNetCore.Razor.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Razor.Orm.Test
{
    public class TestDto
    {
        public string Name { get; set; }
    }

    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {          
            RazorSourceDocument source = RazorSourceDocument.Create(@"@using Razor.Orm
@using Razor.Orm.Test
@inherits SqlTemplate<TestDto>
select * from users where name = @Model.Name
and id in (@for(int i = 0; i < 10; i++)
{
                @i<text>,</text>
})", "teste");

            RazorCodeDocument codeDocument = RazorCodeDocument.Create(source);
            CompilationService.Engine.Process(codeDocument);

            Logger.LogMessage("-> {0}", codeDocument.GetCSharpDocument().GeneratedCode);

            var item = CompilationService.CreateCompilation(codeDocument.GetCSharpDocument().GeneratedCode);

            var result = item.Process(new TestDto { Name = "Bruno" });

            Logger.LogMessage("-> {0}", result.Content);
        }
    }
}
