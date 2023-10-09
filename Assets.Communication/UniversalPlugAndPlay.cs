using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Nonno.Assets.Communication.Utils;

namespace Nonno.Assets.Communication;

public class UniversalPlugAndPlay
{
    public void Init()
    {
        // 送信先のIPアドレスとポート番号を設定
        string hostN_broadcast = "239.255.255.250";  // 送信先のIPアドレス
        int portN_broadcast = 1900;
        string reqM = """
            M-SEARCH * HTTP/1.1
            MX: 3
            HOST: 239.255.255.250:1900
            MAN: "ssdp: discover"
            ST: service:WANIPConnection:1
            """;

        // UDPクライアントを作成
        var udpC = new UdpClient();

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(reqM);
            udpC.Send(data, data.Length, hostN_broadcast, portN_broadcast);
            Log("メッセージを送信しました:\n" + reqM);
        }
        catch (Exception ex)
        {
            Log("エラーが発生しました:\n" + ex.Message);
        }
        finally
        {
            udpC.Close();  // UDPクライアントを閉じる
        }
    }

    async Task<HttpResponseMessage> GetRequest(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);

        Log($"Status Code: {response.StatusCode}");

        // レスポンスのコンテンツを文字列として取得
        string content = await response.Content.ReadAsStringAsync();
        Log($"Response Content: {content}");

        return response;
    }
}
