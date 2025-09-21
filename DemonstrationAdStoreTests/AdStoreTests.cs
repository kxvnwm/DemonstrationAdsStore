
using DemonstrationAdsStore;

namespace DemonstrationAdStoreTests;

public class AdStoreTests
{
    [Fact]
    public void LoadAndLookup_BasicScenario()
    {
        var txt = @"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd
";
        var store = new AdsStore();
        var count = store.LoadFromText(txt);
        Assert.Equal(4, count);

        var a1 = store.GetAdvertisersFor("/ru/msk");
        Assert.Contains("Газета уральских москвичей", a1);
        Assert.Contains("Яндекс.Директ", a1);
        Assert.DoesNotContain("Крутая реклама", a1);

        var a2 = store.GetAdvertisersFor("/ru/svrd");
        Assert.Contains("Крутая реклама", a2);
        Assert.Contains("Яндекс.Директ", a2);
        Assert.DoesNotContain("Ревдинский рабочий", a2);

        var a3 = store.GetAdvertisersFor("/ru/svrd/revda");
        Assert.Contains("Ревдинский рабочий", a3);
        Assert.Contains("Крутая реклама", a3);
        Assert.Contains("Яндекс.Директ", a3);

        var a4 = store.GetAdvertisersFor("/ru");
        Assert.Single(a4);
        Assert.Contains("Яндекс.Директ", a4);
    }

    [Fact]
    public void Load_IgnoresInvalidLines()
    {
        var txt = @"BadLineWithoutColon
:NoName:/ru
NameOnly:
Correct:/ru/area
";
        var store = new AdsStore();
        var count = store.LoadFromText(txt);
        Assert.Equal(1, count);
        var res = store.GetAdvertisersFor("/ru/area");
        Assert.Contains("Correct", res);
    }

    [Fact]
    public void LoadFromText_WithNullOrEmpty_ReturnsZero()
    {
        var store = new AdsStore();

        Assert.Equal(0, store.LoadFromText(""));
        Assert.Equal(0, store.LoadFromText(null!));
    }

    [Fact]
    public void LoadFromText_DuplicateAdvertiserSameLocation_StoresOnce()
    {
        var txt = @"Test:/ru
    Test:/ru
    ";
        var store = new AdsStore();

        var count = store.LoadFromText(txt);

        Assert.Equal(1, count);

        var res = store.GetAdvertisersFor("/ru");
        Assert.Single(res);
        Assert.Contains("Test", res);
    }

    [Theory]
    [InlineData("/ru/", "/ru")]
    [InlineData("ru", "/ru")]
    [InlineData("//ru//", "/ru")]
    public void GetAdvertisersFor_NormalizesLocations(string input, string expectedNormalized)
    {
        var txt = "Platform:" + expectedNormalized;
        var store = new AdsStore();

        store.LoadFromText(txt);

        var res = store.GetAdvertisersFor(input);

        Assert.Contains("Platform", res);
    }

    [Fact]
    public void GetAdvertisersFor_RootPath_ReturnsRootOnly()
    {
        var txt = @"RootPlatform:/";
        var store = new AdsStore();

        store.LoadFromText(txt);

        var res = store.GetAdvertisersFor("/");
        Assert.Single(res);
        Assert.Contains("RootPlatform", res);
    }

    [Fact]
    public void GetAdvertisersFor_ParentPathAdvertisers_AppliesToChild()
    {
        var txt = @"Parent:/ru";
        var store = new AdsStore();

        store.LoadFromText(txt);

        var res = store.GetAdvertisersFor("/ru/svrd");
        Assert.Contains("Parent", res);
    }

    [Fact]
    public async Task ConcurrentAccess_DoesNotThrow()
    {
        var txt = @"P:/ru";
        var store = new AdsStore();
        
        store.LoadFromText(txt);

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                store.GetAdvertisersFor("/ru");
            }
        }));

        await Task.WhenAll(tasks);
    }
}
