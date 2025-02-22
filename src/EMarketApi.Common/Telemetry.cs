using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EMarketApi.Common
{
    public static class Telemetry
    {
        public static readonly ActivitySource Activity = new("EMarket");

        public static Activity StartActivity([CallerMemberName] string name = null, ActivityKind kind = ActivityKind.Internal)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return Activity.StartActivity(name, kind);
        }
    }
}
