using System;
using System.Diagnostics;

class Program
{
    private const string ClientId = "UVB0MTFwREpQc2draGhnWXlEblM6MTpjaQ";
    private const string CallbackUrl = "http://127.0.0.1:3000/callback";

    static void Main(string[] args)
    {
        // 認証URLを生成
        string authUrl = GenerateAuthUrl();

        Console.WriteLine("以下のURLをブラウザで開いて、認証を完了してください:");
        Console.WriteLine(authUrl);

        // 自動的にブラウザで開く
        OpenUrlInBrowser(authUrl);

        // 認証コードを入力
        Console.WriteLine("認証が完了したら、リダイレクトURLの「code」パラメータをコピーして貼り付けてください:");
        string code = Console.ReadLine();

        Console.WriteLine($"入力された認証コード: {code}");
        Console.WriteLine("このコードを使ってアクセストークンを取得してください。");

        // 終了待ち
        Console.WriteLine("終了するには何かキーを押してください...");
        Console.ReadKey();
    }

    // 認証URLを生成
    private static string GenerateAuthUrl()
    {
        string state = Guid.NewGuid().ToString();
        return $"https://twitter.com/i/oauth2/authorize" +
               $"?response_type=code" +
               $"&client_id={ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(CallbackUrl)}" +
               $"&scope={Uri.EscapeDataString("tweet.read tweet.write users.read offline.access")}" +
               $"&state={state}" +
               $"&code_challenge=challenge" +
               $"&code_challenge_method=plain";
    }

    // ブラウザでURLを開く
    private static void OpenUrlInBrowser(string url)
    {
        try
        {
            // Windowsの標準ブラウザで開く
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ブラウザを開く際にエラーが発生しました: {ex.Message}");
        }
    }
}
