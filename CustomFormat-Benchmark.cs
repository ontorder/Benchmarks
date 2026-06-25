using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace test;

[MemoryDiagnoser]
public class bench_tokenformat
{
    [Benchmark]
    public string just_replace()
    {
        return Template
            .Replace("{userFirstName}", "nome")
            .Replace("{userLastName}", "cognome")
            .Replace("{idcaddr}", "http://aaaaaaa.bbbb/ccccccccc/")
            .Replace("{tenant}", "ac000")
            .Replace("{guid}", "00000000-0000-0000-0000-000000000000")
            .Replace("{token}", "00000000-0000-0000-0000-000000000000");
    }

    [Benchmark]
    public string custom_format()
    {
        var tokens = new SortedList<string, object>(capacity: 7) {
            { "userFirstName", "nome" },
            { "userLastName", "cognome" },
            { "idcaddr", "http://aaaaaaa.bbbb/ccccccccc/" },
            { "tenant", "ac000" },
            { "guid", "00000000-0000-0000-0000-000000000000" },
            { "token", "00000000-0000-0000-0000-000000000000" }
        };
        return FormatStringWithPlaceholders(Template, tokens, []);
    }

    [Benchmark]
    public string custom_format_nowarn()
    {
        var tokens = new SortedList<string, object>(capacity: 7) {
            { "userFirstName", "nome" },
            { "userLastName", "cognome" },
            { "idcaddr", "http://aaaaaaa.bbbb/ccccccccc/" },
            { "tenant", "ac000" },
            { "guid", "00000000-0000-0000-0000-000000000000" },
            { "token", "00000000-0000-0000-0000-000000000000" }
        };
        return FormatStringWithPlaceholders_nowarn(Template, tokens);
    }

    private readonly SortedList<string, object> init_tokens = new SortedList<string, object>(capacity: 7) {
            { "userFirstName", "nome" },
            { "userLastName", "cognome" },
            { "idcaddr", "http://aaaaaaa.bbbb/ccccccccc/" },
            { "tenant", "ac000" },
            { "guid", "00000000-0000-0000-0000-000000000000" },
            { "token", "00000000-0000-0000-0000-000000000000" }
        };

    [Benchmark]
    public string custom_format_skipinit()
    {
        return FormatStringWithPlaceholders(Template, init_tokens, []);
    }

    private readonly List<object> _temp = [];

    [Benchmark]
    public string custom_format_list()
    {
        var tokens = new (string, object)[] {
            ( "userFirstName", "nome" ),
            ( "userLastName", "cognome" ),
            ( "idcaddr", "http://aaaaaaa.bbbb/ccccccccc/" ),
            ( "tenant", "ac000" ),
            ( "guid", "00000000-0000-0000-0000-000000000000" ),
            ( "token", "00000000-0000-0000-0000-000000000000" ),
        };
        return FormatStringWithPlaceholders(Template, tokens, _temp);
    }

    [Benchmark]
    public string custom_format_rent()
    {
        var tokens = ArrayPool<string>.Shared.Rent(12);
        tokens[0] = "userFirstName";
        tokens[1] = "nome";
        tokens[2] = "userLastName";
        tokens[3] = "cognome";
        tokens[4] = "idcaddr";
        tokens[5] = "http://aaaaaaa.bbbb/ccccccccc/";
        tokens[6] = "tenant";
        tokens[7] = "ac000";
        tokens[8] = "guid";
        tokens[9] = "00000000-0000-0000-0000-000000000000";
        tokens[10] = "token";
        tokens[11] = "00000000-0000-0000-0000-000000000000";
        var ret = FormatStringWithPlaceholders(Template, tokens, _temp);
        ArrayPool<string>.Shared.Return(tokens);
        return ret;
    }

    ref struct TestTokens
    {
        public ReadOnlySpan<char> k0;
        public ReadOnlySpan<char> k1;
        public ReadOnlySpan<char> k2;
        public ReadOnlySpan<char> k3;
        public ReadOnlySpan<char> k4;
        public ReadOnlySpan<char> k5;
        public ReadOnlySpan<char> v0;
        public ReadOnlySpan<char> v1;
        public ReadOnlySpan<char> v2;
        public ReadOnlySpan<char> v3;
        public ReadOnlySpan<char> v4;
        public ReadOnlySpan<char> v5;
    }

    [Benchmark]
    public string custom_format_stack()
    {
        var tokens = new TestTokens()
        {
            k0 = "userFirstName",
            v0 = "nome",

            k1 = "userLastName",
            v1 = "cognome",

            k2 = "idcaddr",
            v2 = "http://aaaaaaa.bbbb/ccccccccc/",

            k3 = "tenant",
            v3 = "ac000",

            k4 = "guid",
            v4 = "00000000-0000-0000-0000-000000000000",

            k5 = "token",
            v5 = "00000000-0000-0000-0000-000000000000"
        };
        return FormatStringWithPlaceholders_ref(Template, tokens, _temp);
    }

    private static string FormatStringWithPlaceholders(string pattern, SortedList<string, object> placeholders, List<object> warnings)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        if (placeholders.Count == 0)
            return pattern;

        var formatted = new StringBuilder(capacity: pattern.Length);
        var remainingPattern = pattern.AsSpan();

        while (false == remainingPattern.IsEmpty)
        {
            // cerca apertura placeholder {
            var openIndex = remainingPattern.IndexOfAny('{', '}');

            // nessun altro placeholder, copio testo rimanente ed esco
            if (openIndex < 0)
            {
                formatted.Append(remainingPattern);
                break;
            }

            // copio testo fino a placeholder
            formatted.Append(remainingPattern[..openIndex]);
            remainingPattern = remainingPattern[openIndex..];

            switch (remainingPattern)
            {
                case ['{', '{', ..]:
                    // placeholder in realtà è escaped
                    formatted.Append('{');
                    remainingPattern = remainingPattern[2..];
                    continue;

                case ['{', ..]:
                    // cerco chiusura placeholder
                    var closeIndex = remainingPattern.IndexOf('}');

                    // nessuna chiusura, sarebbe errore
                    if (closeIndex < 0)
                    {
                        warnings.Add("found {, but no }");
                        break;
                    }

                    // TODO lettura placeholder dovrebbe essere uno stato macchina, o meglio dovrei mettere ricerca }} prima di {{?
                    // escaped }} -- teoricamente un loop
                    // if (remainingPattern.Length >1 && remainingPattern[closeIndex + 1] == '}') salta fino a solo }

                    // parsing placeholder
                    var token = remainingPattern[1..closeIndex];
                    if (!token.IsEmpty && placeholders.TryGetValue(token.ToString(), out var value))
                    {
                        formatted.Append(value);
                    }
                    else
                    {
                        warnings.Add("found unknown placeholder");
                    }

                    remainingPattern = remainingPattern[(closeIndex + 1)..];
                    break;

                case ['}', '}']:
                case ['}', '}', ..]:
                    formatted.Append('}');
                    remainingPattern = remainingPattern[2..];
                    break;

                default:
                    warnings.Add("invalid pattern");
                    break;
            }
        }

        return formatted.ToString();
    }

    private static string FormatStringWithPlaceholders(string pattern, (string, object)[] placeholders, List<object> warnings)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        if (placeholders.Length == 0)
            return pattern;

        var formatted = new StringBuilder(capacity: pattern.Length);
        var remainingPattern = pattern.AsSpan();

        while (false == remainingPattern.IsEmpty)
        {
            // cerca apertura placeholder {
            var openIndex = remainingPattern.IndexOfAny('{', '}');

            // nessun altro placeholder, copio testo rimanente ed esco
            if (openIndex < 0)
            {
                formatted.Append(remainingPattern);
                break;
            }

            // copio testo fino a placeholder
            formatted.Append(remainingPattern[..openIndex]);
            remainingPattern = remainingPattern[openIndex..];

            switch (remainingPattern)
            {
                case ['{', '{', ..]:
                    // placeholder in realtà è escaped
                    formatted.Append('{');
                    remainingPattern = remainingPattern[2..];
                    continue;

                case ['{', ..]:
                    // cerco chiusura placeholder
                    var closeIndex = remainingPattern.IndexOf('}');

                    // nessuna chiusura, sarebbe errore
                    if (closeIndex < 0)
                    {
                        warnings.Add("found {, but no }");
                        break;
                    }

                    // TODO lettura placeholder dovrebbe essere uno stato macchina, o meglio dovrei mettere ricerca }} prima di {{?
                    // escaped }} -- teoricamente un loop
                    // if (remainingPattern.Length >1 && remainingPattern[closeIndex + 1] == '}') salta fino a solo }

                    // parsing placeholder
                    var token = remainingPattern[1..closeIndex];
                    if (!token.IsEmpty && TryGetValue(placeholders, token, out var value))
                    {
                        formatted.Append(value);
                    }
                    else
                    {
                        warnings.Add("found unknown placeholder");
                    }

                    remainingPattern = remainingPattern[(closeIndex + 1)..];
                    break;

                case ['}', '}']:
                case ['}', '}', ..]:
                    formatted.Append('}');
                    remainingPattern = remainingPattern[2..];
                    break;

                default:
                    warnings.Add("invalid pattern");
                    break;
            }
        }

        return formatted.ToString();

        static bool TryGetValue((string, object)[] kvs, ReadOnlySpan<char> key, out object? found)
        {
            for (var i = 0; i < kvs.Length; ++i)
                if (kvs[i].Item1.AsSpan().SequenceEqual(key))
                {
                    found = kvs[i].Item2;
                    return true;
                }

            found = null;
            return false;
        }
    }

    private static string FormatStringWithPlaceholders(string pattern, string[] placeholders, List<object> warnings)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        if (placeholders.Length == 0)
            return pattern;

        var formatted = new StringBuilder(capacity: pattern.Length);
        var remainingPattern = pattern.AsSpan();

        while (false == remainingPattern.IsEmpty)
        {
            // cerca apertura placeholder {
            var openIndex = remainingPattern.IndexOfAny('{', '}');

            // nessun altro placeholder, copio testo rimanente ed esco
            if (openIndex < 0)
            {
                formatted.Append(remainingPattern);
                break;
            }

            // copio testo fino a placeholder
            formatted.Append(remainingPattern[..openIndex]);
            remainingPattern = remainingPattern[openIndex..];

            switch (remainingPattern)
            {
                case ['{', '{', ..]:
                    // placeholder in realtà è escaped
                    formatted.Append('{');
                    remainingPattern = remainingPattern[2..];
                    continue;

                case ['{', ..]:
                    // cerco chiusura placeholder
                    var closeIndex = remainingPattern.IndexOf('}');

                    // nessuna chiusura, sarebbe errore
                    if (closeIndex < 0)
                    {
                        warnings.Add("found {, but no }");
                        break;
                    }

                    // TODO lettura placeholder dovrebbe essere uno stato macchina, o meglio dovrei mettere ricerca }} prima di {{?
                    // escaped }} -- teoricamente un loop
                    // if (remainingPattern.Length >1 && remainingPattern[closeIndex + 1] == '}') salta fino a solo }

                    // parsing placeholder
                    var token = remainingPattern[1..closeIndex];
                    if (!token.IsEmpty && TryGetValue(token) is int foundId && foundId >= 0)
                    {
                        formatted.Append(placeholders[foundId]);
                    }
                    else
                    {
                        warnings.Add("found unknown placeholder");
                    }

                    remainingPattern = remainingPattern[(closeIndex + 1)..];
                    break;

                case ['}', '}']:
                case ['}', '}', ..]:
                    formatted.Append('}');
                    remainingPattern = remainingPattern[2..];
                    break;

                default:
                    warnings.Add("invalid pattern");
                    break;
            }
        }

        return formatted.ToString();

        // TODO si potrebbe ancora ottimizzare tenendo uno stato dell'indice fra le chiamate
        // perché se token sono ordinati allora prenderà sempre n+1 e ricerca esce dopo un ciclo
        int TryGetValue(ReadOnlySpan<char> key)
        {
            for (var i = 0; i < placeholders.Length; ++i, ++i)
                if (placeholders[i].AsSpan().SequenceEqual(key))
                    return i + 1;

            return -1;
        }
    }

    private string FormatStringWithPlaceholders_ref(ReadOnlySpan<char> pattern, TestTokens placeholders, List<object> warnings)
    {
        if (pattern.Length == 0)
            return string.Empty;

        var formatted = new StringBuilder(capacity: pattern.Length);
        var remainingPattern = pattern;

        while (false == remainingPattern.IsEmpty)
        {
            // cerca apertura placeholder {
            var openIndex = remainingPattern.IndexOfAny('{', '}');
            // TODO provare a ottimizzare memoizzando posizioni token in modo che dopo prima volta non ci sia più bisogno di ricerca

            // nessun altro placeholder, copio testo rimanente ed esco
            if (openIndex < 0)
            {
                formatted.Append(remainingPattern);
                break;
            }

            // copio testo fino a placeholder
            formatted.Append(remainingPattern[..openIndex]);
            remainingPattern = remainingPattern[openIndex..];

            switch (remainingPattern)
            {
                case ['{', '{', ..]:
                    // placeholder in realtà è escaped
                    formatted.Append('{');
                    remainingPattern = remainingPattern[2..];
                    continue;

                case ['{', ..]:
                    // cerco chiusura placeholder
                    var closeIndex = remainingPattern.IndexOf('}');

                    // nessuna chiusura, sarebbe errore
                    if (closeIndex < 0)
                    {
                        warnings.Add("found {, but no }");
                        break;
                    }

                    // TODO lettura placeholder dovrebbe essere uno stato macchina, o meglio dovrei mettere ricerca }} prima di {{?
                    // escaped }} -- teoricamente un loop
                    // if (remainingPattern.Length >1 && remainingPattern[closeIndex + 1] == '}') salta fino a solo }

                    // parsing placeholder
                    var token = remainingPattern[1..closeIndex];

                    //if (!token.IsEmpty && TryGetValue(token, out var foundId))
                    //if (!token.IsEmpty && TryGetValue2(token) is var found && false == found.Equals(notfound))

                    switch (token)
                    {
                        case { } when placeholders.k0.SequenceEqual(token): formatted.Append(placeholders.v0); break;
                        case { } when placeholders.k1.SequenceEqual(token): formatted.Append(placeholders.v1); break;
                        case { } when placeholders.k2.SequenceEqual(token): formatted.Append(placeholders.v2); break;
                        case { } when placeholders.k3.SequenceEqual(token): formatted.Append(placeholders.v3); break;
                        case { } when placeholders.k4.SequenceEqual(token): formatted.Append(placeholders.v4); break;
                        case { } when placeholders.k5.SequenceEqual(token): formatted.Append(placeholders.v5); break;
                        default: warnings.Add("found unknown placeholder"); break;
                    }

                    remainingPattern = remainingPattern[(closeIndex + 1)..];
                    break;

                case ['}', '}']:
                case ['}', '}', ..]:
                    formatted.Append('}');
                    remainingPattern = remainingPattern[2..];
                    break;

                default:
                    warnings.Add("invalid pattern");
                    break;
            }
        }

        return formatted.ToString();

        //bool TryGetValue(ReadOnlySpan<char> key, out int foundId)
        //{
        //    for (var i = 0; i < placeholders.Length; ++i)
        //        if (placeholders[i][0].Span.SequenceEqual(key))
        //        {
        //            foundId = i;
        //            return true;
        //        }
        //
        //    foundId = -1;
        //    return false;
        //}

        //ref readonly Memory<char> TryGetValue2(ReadOnlySpan<char> key)
        //{
        //    for (var i = 0; i < placeholders.Length; ++i)
        //        if (placeholders[i][0].Span.SequenceEqual(key))
        //            return ref placeholders[i][1];
        //
        //    return ref notfound;
        //}
    }

    private readonly Memory<char> notfound = Array.Empty<char>();

    private static string FormatStringWithPlaceholders_nowarn(string pattern, SortedList<string, object> placeholders)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        if (placeholders.Count == 0)
            return pattern;

        var formatted = new StringBuilder(capacity: pattern.Length);
        var remainingPattern = pattern.AsSpan();

        while (false == remainingPattern.IsEmpty)
        {
            // cerca apertura placeholder {
            var openIndex = remainingPattern.IndexOfAny('{', '}');

            // nessun altro placeholder, copio testo rimanente ed esco
            if (openIndex < 0)
            {
                formatted.Append(remainingPattern);
                break;
            }

            // copio testo fino a placeholder
            formatted.Append(remainingPattern[..openIndex]);
            remainingPattern = remainingPattern[openIndex..];

            switch (remainingPattern)
            {
                case ['{', '{', ..]:
                    // placeholder in realtà è escaped
                    formatted.Append('{');
                    remainingPattern = remainingPattern[2..];
                    continue;

                case ['{', ..]:
                    // cerco chiusura placeholder
                    var closeIndex = remainingPattern.IndexOf('}');

                    // nessuna chiusura, sarebbe errore
                    if (closeIndex < 0)
                    {
                        break;
                    }

                    // TODO lettura placeholder dovrebbe essere uno stato macchina, o meglio dovrei mettere ricerca }} prima di {{?
                    // escaped }} -- teoricamente un loop
                    // if (remainingPattern.Length >1 && remainingPattern[closeIndex + 1] == '}') salta fino a solo }

                    // parsing placeholder
                    var token = remainingPattern[1..closeIndex];
                    if (!token.IsEmpty && placeholders.TryGetValue(token.ToString(), out var value))
                    {
                        formatted.Append(value);
                    }

                    remainingPattern = remainingPattern[(closeIndex + 1)..];
                    break;

                case ['}', '}']:
                case ['}', '}', ..]:
                    formatted.Append('}');
                    remainingPattern = remainingPattern[2..];
                    break;

                default:
                    break;
            }
        }

        return formatted.ToString();
    }

    private const string Template =
        """
        <!DOCTYPE html>
        <html>
            <head><meta charset="UTF-8"></head>
            <body style="font-family: Arial, Helvetica, sans-serif; color: #333333; line-height: 1.5;">
                <p>L'utente {userFirstName} {userLastName} ha confermato con successo il proprio indirizzo e-mail.</p>
                <p>Si desidera procedere con l'approvazione dell'account?</p>
                <p>
                    <table cellpadding="0" cellspacing="0" border="0">
           	            <tr>
              		        <td style="padding-right:10px;">
              			        <a href="https://{idcaddr}/garavot/{tenant}/api/v1/users/{guid}/approve?token={token}"
              			            style="background:#28a745;color:#ffffff;text-decoration:none;padding:6px 12px;display:inline-block;font-family:Arial,sans-serif;">Approva</a>
              		        </td>
              		        <td>
              			        <a href="https://{idcaddr}/garavot/{tenant}/api/v1/users/{guid}/reject?token={token}"
              			            style="background:#dc3545;color: #ffffff;text-decoration:none;padding:6px 12px;display:inline-block;font-family:Arial,sans-serif;">Rifiuta</a>
              		        </td>
           	            </tr>
                    </table>
                </p>
                <hr style="border:none; border-top:1px solid #dddddd;"><p style="font-size:12px; color:#777777;">Questa è una comunicazione automatica. Si prega di non rispondere a questa e-mail.</p>
            </body>
        </html>
        """;
}
/*
template slightly bigger than other cases
| Method                 | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|----------------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| just_replace           | 1.895 us | 0.0375 us | 0.0500 us | 2.7351 | 0.0229 |  16.77 KB |
| custom_format_skipinit | 1.977 us | 0.0116 us | 0.0103 us | 1.4572 | 0.0305 |   8.94 KB |
| custom_format_nowarn   | 2.556 us | 0.0242 us | 0.0226 us | 1.4877 | 0.0191 |   9.13 KB |
| custom_format          | 2.599 us | 0.0222 us | 0.0186 us | 1.4915 | 0.0305 |   9.16 KB |

possible mistake for list
| Method             | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| custom_format_list |   530.7 ns |  6.75 ns |  5.27 ns | 0.8669 | 0.0057 |   5.32 KB |
| just_replace       | 1,558.2 ns | 11.65 ns | 10.33 ns | 2.6779 | 0.0229 |  16.41 KB |
| custom_format      | 2,408.9 ns | 11.08 ns |  9.82 ns | 1.4648 | 0.0305 |   8.98 KB |

possible mistake for list
| Method              | Mean        | Error     | StdDev     | Gen0   | Gen1   | Allocated |
|-------------------- |------------:|----------:|-----------:|-------:|-------:|----------:|
| custom_format_list  |   622.88 ns | 11.065 ns |  15.870 ns | 0.8669 | 0.0057 |    5448 B |
| custom_format_stack |   843.38 ns |  3.815 ns |   3.382 ns | 1.3628 | 0.0277 |    8552 B |
| just_replace        | 1,961.00 ns | 38.774 ns |  54.356 ns | 2.6779 | 0.0229 |   16808 B |
| custom_format       | 2,914.72 ns | 58.256 ns | 114.992 ns | 1.4648 | 0.0305 |    9192 B |

possible mistake for list
| Method              | Mean       | Error    | StdDev   | Median     | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|---------:|-----------:|-------:|-------:|----------:|
| custom_format_list  |   532.2 ns |  3.27 ns |  2.90 ns |   531.4 ns | 0.8669 | 0.0057 |   5.32 KB |
| custom_format_stack |   745.6 ns |  7.07 ns |  6.61 ns |   743.0 ns | 1.3628 | 0.0277 |   8.35 KB |
| custom_format_rent  |   776.5 ns |  4.33 ns |  4.05 ns |   776.4 ns | 1.3628 | 0.0277 |   8.35 KB |
| just_replace        | 1,633.8 ns | 30.82 ns | 73.26 ns | 1,598.6 ns | 2.6779 | 0.0229 |  16.41 KB |

corrected
| Method                 | Mean       | Error    | StdDev    | Median     | Gen0   | Gen1   | Allocated |
|----------------------- |-----------:|---------:|----------:|-----------:|-------:|-------:|----------:|
| custom_format_stack    |   717.1 ns |  8.50 ns |   7.95 ns |   713.6 ns | 1.3628 | 0.0277 |   8.35 KB |
| custom_format_list     |   733.2 ns |  3.48 ns |   3.26 ns |   732.5 ns | 1.3819 | 0.0286 |   8.47 KB |
| custom_format_rent     |   763.6 ns |  4.55 ns |   4.03 ns |   762.5 ns | 1.3628 | 0.0277 |   8.35 KB |
| just_replace           | 1,644.5 ns | 38.81 ns | 113.83 ns | 1,578.2 ns | 2.6779 | 0.0229 |  16.41 KB |
| custom_format_skipinit | 1,805.5 ns | 11.14 ns |   9.87 ns | 1,803.2 ns | 1.4286 | 0.0286 |   8.76 KB |
| custom_format          | 2,414.7 ns | 15.23 ns |  13.50 ns | 2,414.2 ns | 1.4648 | 0.0305 |   8.98 KB |
| custom_format_nowarn   | 2,444.6 ns | 24.45 ns |  22.87 ns | 2,438.2 ns | 1.4572 | 0.0305 |   8.95 KB |

*/
