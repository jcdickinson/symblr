using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Symblr
{
    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfig : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<StylesheetBundle>("~/css/adminlte", "adminlte.less", "skins/all-skins.less");
            bundles.Add<ScriptBundle>("~/scripts/adminlte", "app.js");
        }
    }
}