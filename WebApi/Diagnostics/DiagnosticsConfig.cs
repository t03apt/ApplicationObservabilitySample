using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WebApi.Diagnostics
{
    public class DiagnosticsConfig
    {
        public const string ServiceName = "application-observability-sample";
        public static Meter Meter = new(ServiceName);
        public static Histogram<double> ForecastTemperature = Meter.CreateHistogram<double>("forecast.temperature");
        public static ActivitySource ActivitySource = new(ServiceName);
    }
}
