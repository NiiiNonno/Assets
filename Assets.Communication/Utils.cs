using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Communication;
public static class Utils
{
    public static void SendUDP()
    {
        // 送信先のIPアドレスとポート番号を設定
        string ipAddress = "127.0.0.1";  // 送信先のIPアドレス
        int port = 12345;               // 送信先のポート番号

        // UDPクライアントを作成
        var udpClient = new UdpClient();

        try
        {
            // 送信するデータを準備
            string message = "Hello, UDP Server!";
            byte[] data = Encoding.UTF8.GetBytes(message);

            // データを送信
            udpClient.Send(data, data.Length, ipAddress, port);

            Console.WriteLine("メッセージを送信しました: " + message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("エラーが発生しました: " + ex.Message);
        }
        finally
        {
            udpClient.Close();  // UDPクライアントを閉じる
        }
    }

    [Conditional("DEBUG")]
    public static void Log(string message)
    {
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("[ACom.log]");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}
