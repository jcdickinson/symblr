using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.Symbols.Pdb70
{
    public class SrcSrvParserFixtures
    {
        [Fact]
        public async Task When_parsing_SRCSRV_data()
        {
            const string Srcsrv = @"SRCSRV: ini ------------------------------------------------
VERSION=3
INDEXVERSION=2
VERCTRL=http
DATETIME=Sat Apr 12 02:09:54 2014
SRCSRV: variables ------------------------------------------
SRCSRVVERCTRL=http

DEVDIV_TFS2=http://vstfdevdiv.redmond.corp.microsoft.com:8080/devdiv2
HTTP_ALIAS=http://referencesource.microsoft.com/Source/51209.34209/Source/
HTTP_EXTRACT_TARGET=%HTTP_ALIAS%/%var3%
SRCSRVTRG=%http_extract_target% - %fnvar%(%var5%) - %fnbksl%(%var3%) - %fnfile%(%var1%)
SRCSRVCMD=
SRCSRV: source files ---------------------------------------
f:\dd\NDP\fx\src\Misc\ClientUtils.cs*DEVDIV_TFS2*/ndp/fx/src/Misc/ClientUtils.cs*1103130*var1
f:\dd\NDP\fx\src\WinForms\Managed\System\Resources\ResXDataNode.cs*DEVDIV_TFS2*/ndp/fx/src/WinForms/Managed/System/Resources/ResXDataNode.cs*1103130*var1

f:\dd\NDP\fx\src\WinForms\Managed\System\Resources\ResXFileRef.cs*DEVDIV_TFS2*/ndp//fx/src/WinForms/Managed/System/Resources/ResXFileRef.cs*1103130*var1
";

            using (var reader = new StringReader(Srcsrv))
            {
                var info = await SrcSrvParser.ParseAsync(reader, CancellationToken.None);
                Assert.Equal(3, info.Count);

                Assert.Equal(@"f:\dd\NDP\fx\src\Misc\ClientUtils.cs", info[0].OriginalFile);
                Assert.Equal(@"f:\dd\NDP\fx\src\WinForms\Managed\System\Resources\ResXDataNode.cs", info[1].OriginalFile);
                Assert.Equal(@"f:\dd\NDP\fx\src\WinForms\Managed\System\Resources\ResXFileRef.cs", info[2].OriginalFile);

                Assert.Equal(
                    @"http://referencesource.microsoft.com/Source/51209.34209/Source///ndp/fx/src/Misc/ClientUtils.cs - f:\dd\NDP\fx\src\Misc\ClientUtils.cs - \ndp\fx\src\Misc\ClientUtils.cs - ClientUtils.cs",
                    info[0].TargetPath);
                Assert.Equal(
                    @"http://referencesource.microsoft.com/Source/51209.34209/Source///ndp/fx/src/WinForms/Managed/System/Resources/ResXDataNode.cs - f:\dd\NDP\fx\src\WinForms\Managed\System\Resources\ResXDataNode.cs - \ndp\fx\src\WinForms\Managed\System\Resources\ResXDataNode.cs - ResXDataNode.cs",
                    info[1].TargetPath);
                Assert.Equal(
                    @"http://referencesource.microsoft.com/Source/51209.34209/Source///ndp//fx/src/WinForms/Managed/System/Resources/ResXFileRef.cs - f:\dd\NDP\fx\src\WinForms\Managed\System\Resources\ResXFileRef.cs - \ndp\fx\src\WinForms\Managed\System\Resources\ResXFileRef.cs - ResXFileRef.cs",
                    info[2].TargetPath);
            }
        }
    }
}
