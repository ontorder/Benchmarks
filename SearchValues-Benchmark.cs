public class bench_searchvalues
{
    private SearchValues<string> testsearch = null!;
    private const int max = 99999;
    private string[] items = new string[max];

    [GlobalSetup]
    public void setup()
    {
        for (var i = 0; i < max; ++i) items[i] = Random.Shared.Next(100_000_000, 999_000_000).ToString();
        testsearch = SearchValues.Create(items, StringComparison.Ordinal);
    }

    [Benchmark]
    public bool bigtest()
    {
        string choice = items[Random.Shared.Next(max)];
        return testsearch.Contains(choice);
    }
}
/*

searchvalue
| Method       | Mean     |
|------------- |---------:|
| 10_000_000x8 | 7.008 ns |
| 20_000_000x9 | 7.058 ns |
| 40_000_000x9 | 7.336 ns |

searchvalue + rnd choice
| Method       | Mean     |
|------------- |---------:|
| 10_000_000x9 | 231.0 ns |
| 40_000_000x9 | 257.8 ns |
|         32x9 | 18.84 ns |
|      99999x9 | 45.36 ns |
*/
