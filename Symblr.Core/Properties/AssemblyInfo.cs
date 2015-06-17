using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Symblr.Core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Symblr.Core")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("3ffedb80-0a4f-4bf9-aee7-0dd2eea27f0c")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

#if DEBUG || TEST
#pragma warning disable 1700
#if MONO
// WTF? This bullshit only compiles with xbuild and xbuild *requires* it.
[assembly: InternalsVisibleTo("Symblr.Tests")]
#else
[assembly: InternalsVisibleTo("Symblr.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f78fc423798181999e6bb7073df3bb96b47bd65476b6ccd6a98d7716f0039127937b1123dfafbdd34a31bd1c629772c854c4fc89f0d2db0dc6a859cd5173f70217817049d4ac87e5960476ead0641e6c7a77902948b16d195475a40051add23572d403c903577299b571f707595c409863a648f3d2c325fb7c4fac176a7dd8b2")]
#endif
#pragma warning restore 1700
#endif