using System.Runtime.CompilerServices;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

var transforms = new Dictionary<string, List<Dictionary<string, string>>>();
transforms.Add("cluster1", new List<Dictionary<string, string>>());
var item = new Dictionary<string, string>()
{
    { "PathRemovePrefix", "/serviceone" }
};
transforms["cluster1"].Add(item);


builder.Services.AddReverseProxy()
    .LoadFromMemory(GetRoutes(), GetClusters());
//.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.MapReverseProxy();
app.Run();

RouteConfig[] GetRoutes()
{
    return
    [
        new RouteConfig()
        {
            RouteId = "route" + Random.Shared.Next(), // Forces a new route id each time GetRoutes is called.
            ClusterId = "cluster1",
            Match = new RouteMatch
            {
                // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                Path = "/serviceone/{**catch-all}"
            },
            Transforms = GetTransforms("cluster1")
        }
    ];
}


IReadOnlyList<IReadOnlyDictionary<string, string>> GetTransforms(string transformClusterKey)
{
    return transforms[transformClusterKey];
}

ClusterConfig[] GetClusters()
{
    return
    [
        new ClusterConfig()
        {
            ClusterId = "cluster1",
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                { "destination1", new DestinationConfig() { Address = "http://localhost:5005/" } },
            }
        }
    ];
}