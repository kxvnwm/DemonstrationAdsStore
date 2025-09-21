using System.Text;

namespace DemonstrationAdsStore;

public class AdsStore
{

    Dictionary<string, HashSet<string>> _regionPlatforms = new(StringComparer.OrdinalIgnoreCase);

    readonly ReaderWriterLockSlim _rw = new();

    public int LoadFromText(string text)
    {
        var newRegionPlatforms = ParseTextToRegionPlatforms(text);

        _rw.EnterWriteLock();
        
        try
        {
            _regionPlatforms = newRegionPlatforms;

            var unique = new HashSet<string>(_regionPlatforms.Values.SelectMany(s => s), StringComparer.OrdinalIgnoreCase);
            
            return unique.Count;
        }
        finally
        {
            _rw.ExitWriteLock();
        }
    }

    private Dictionary<string, HashSet<string>> ParseTextToRegionPlatforms(string text)
    {
        var newRegionPlatforms = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(text))
            return newRegionPlatforms;

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            ProcessLine(rawLine, newRegionPlatforms);
        }

        return newRegionPlatforms;
    }

    private void ProcessLine(string rawLine, Dictionary<string, HashSet<string>> regionPlatforms)
    {
        var line = rawLine.Trim();

        if (line.Length == 0)
            return;

        var idx = line.IndexOf(':');

        if (idx <= 0)
            return;

        var platformTitle = line[..idx].Trim();
        
        if (string.IsNullOrEmpty(platformTitle))
            return;

        var locPart = line[(idx + 1)..].Trim();
        
        if (string.IsNullOrEmpty(locPart))
            return;

        var locations = locPart.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var locationRaw in locations)
        {
            AddLocation(platformTitle, locationRaw, regionPlatforms);
        }
    }

    static void AddLocation(string platformTitle, string locationRaw, Dictionary<string, HashSet<string>> regionPlatforms)
    {
        var location = NormalizeLocation(locationRaw);

        if (string.IsNullOrEmpty(location))
            return;

        if (!regionPlatforms.TryGetValue(location, out var platforms))
        {
            platforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            regionPlatforms[location] = platforms;
        }

        platforms.Add(platformTitle);
    }

    public HashSet<string> GetAdvertisersFor(string location)
    {
        var normalizedLocation = NormalizeLocation(location);
        var locationRegions = GetLocationRegions(normalizedLocation);

        return GetRegionPlatforms(locationRegions);
    }

    HashSet<string> GetRegionPlatforms(IEnumerable<string> locationRegions)
    {
        var foundAdPlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        _rw.EnterReadLock();

        try
        {
            foreach (var region in locationRegions)
            {
                if (_regionPlatforms.TryGetValue(region, out var adPlatforms))
                {
                    foreach (var adPlatform in adPlatforms)
                        foundAdPlatforms.Add(adPlatform);
                }
            }
        }
        finally
        {
            _rw.ExitReadLock();
        }

        return foundAdPlatforms;
    }

    static IEnumerable<string> GetLocationRegions(string location)
    {
        if (string.IsNullOrEmpty(location))
            yield break;

        if (location == "/")
        {
            yield return "/";
            yield break;
        }

        var parts = location.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        var regionBuilder = new StringBuilder();

        for (int i = 0; i < parts.Length; i++)
        {
            regionBuilder.Append('/').Append(parts[i]);

            yield return regionBuilder.ToString();
        }
    }
    
    static string NormalizeLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return string.Empty;

        location = location.Trim();

        if (!location.StartsWith('/'))
            location = "/" + location;

        while (location.Contains("//"))
            location = location.Replace("//", "/");

        if (location.Length > 1 && location.EndsWith('/'))
            location = location.TrimEnd('/');

        return location;
    }
}
