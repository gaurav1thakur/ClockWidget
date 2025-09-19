using ClockWidget;

public class ConfigTests
{
    [Fact]
    public void SaveAndLoad_ShouldPreserveSettings()
    {
        var config = new Config
        {
            Theme = "Dark",
            ClockOpacity = 0.75
        };

        config.Save("test_config.json");
        var loaded = Config.Load("test_config.json");

        Assert.Equal("Dark", loaded.Theme);
        Assert.Equal(0.75, loaded.ClockOpacity);
        
    }
}