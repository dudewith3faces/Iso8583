using System.Net;
using System.Net.Sockets;
using BIM_ISO8583.NET;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Zone;

public sealed record Util
{
    public static string GenerateIsoMessage(string mti, string[] bm)
    {
        ISO8583 iso8583 = new();

        return iso8583.Build(bm, mti);
    }

    public static string[] ParseIsoMessage(string isoMsg)
    {
        ISO8583 iso8583 = new();

        return iso8583.Parse(isoMsg);
    }

    public static byte[] SendClientRequest(string ip, int port, byte[] data)
    {
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint remoteEp = new(ipAddress, port);

        Socket sender = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            ReceiveTimeout = 30000,
            SendTimeout = 30000
        };

        sender.Connect(remoteEp);

        Console.WriteLine($"\nSocket connected to {sender.RemoteEndPoint}");

        sender.Send(data);

        Console.WriteLine($"\nMessage sent");

        byte[] response = new byte[1024];

        sender.Receive(response);

        Console.WriteLine($"\nResponse received");

        sender.Shutdown(SocketShutdown.Both);

        return response;
    }

    public static async Task<byte[]> SendClientRequestAsync(string ip, int port, byte[] isoMsg)
    {
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint remoteEp = new(ipAddress, port);

        Socket sender = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            ReceiveTimeout = 30000,
            SendTimeout = 30000
        };

        sender.Connect(remoteEp);

        Console.WriteLine($"Socket connected to {sender.RemoteEndPoint}");

        int bytesSent = await sender.SendAsync(isoMsg, SocketFlags.None);

        Console.WriteLine($"Iso Message of {bytesSent} sent");
        byte[] buffer = new byte[1024];

        int bytesRecevied = await sender.ReceiveAsync(buffer, SocketFlags.None);

        Console.WriteLine($"Iso Response received");

        byte[] response = new byte[bytesRecevied];
        Array.Copy(buffer, response, bytesRecevied);

        Console.WriteLine("Received response of length {0} bytes", response.Length);

        sender.Shutdown(SocketShutdown.Both);

        return response;
    }
    public static string ByteArrayToString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public static byte[] StringToByteArray(string str)
    {
        int numberChars = str.Length;

        byte[] bytes = new byte[numberChars / 2];

        for (int i = 0; i < numberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);


        return bytes;
    }

    public static byte[] XorIt(byte[] input1, byte[] input2)
    {
        byte[] bytes = new byte[input2.Length];

        for (int i = 0; i < input2.Length; i++) bytes[i] = (byte)(input2[i] ^ input1[i % input1.Length]);


        return bytes;
    }

    static byte[] Decrypt(byte[] data, byte[] key)
    {
        byte[] result = new byte[data.Length / 2];

        DesEdeParameters keyParams = new(key);
        DesEdeEngine desEngine = new();
        desEngine.Init(false, keyParams);
        desEngine.ProcessBlock(data, 0, result, 0);

        return result;
    }
    static byte[] Encrypt(byte[] data, byte[] key)
    {
        byte[] result = new byte[8];

        DesEdeParameters keyParams = new(key);
        DesEdeEngine desEngine = new();
        desEngine.Init(true, keyParams);
        desEngine.ProcessBlock(data, 0, result, 0);

        return result;
    }

    public static string GeneratePinBlock(string pin, string zpk, string pan)
    {
        byte[] pinBlock = StringToByteArray($"0{pin.Length}{pin}".PadRight(16, 'F'));

        byte[] panBlock = StringToByteArray(pan.Substring(pan.Length - 13, 12).PadRight(16, '0'));

        byte[] clearPinBlock = XorIt(pinBlock, panBlock);

        byte[] encryptedPinBlock = Encrypt(clearPinBlock, StringToByteArray(zpk));

        return ByteArrayToString(encryptedPinBlock);
    }

    public static string GetClearZPK(string field53KeReponse, string encryptedZmk)
    {
        string encryptedZpk = field53KeReponse[..32];

        string kcv = field53KeReponse.Substring(32, 6);

        string encryptedZpkPartA = encryptedZpk[..16];
        string encryptedZpkPartB = encryptedZpk.Substring(16, 16);

        string encryptedZmkPartA = encryptedZmk[..16];
        byte[] encryptedZmkPartB = StringToByteArray(encryptedZmk.Substring(16, 16));

        string zmkPartBVariant1 = ByteArrayToString(XorIt(encryptedZmkPartB, StringToByteArray("A6".PadRight(16, '0'))));
        string zmkPartBVariant2 = ByteArrayToString(XorIt(encryptedZmkPartB, StringToByteArray("5A".PadRight(16, '0'))));

        byte[] result1 = Decrypt(
            StringToByteArray(encryptedZpkPartA.PadRight(32, '0')),
            StringToByteArray(encryptedZmkPartA + zmkPartBVariant1)
            );

        byte[] result2 = Decrypt(
            StringToByteArray(encryptedZpkPartB.PadRight(32, '0')),
            StringToByteArray(encryptedZmkPartA + zmkPartBVariant2)
            );


        string clearzpk = ByteArrayToString(result1)[..16] + ByteArrayToString(result2)[..16];

        Console.WriteLine($"\nKCV validation is: {kcv == GetKVC(StringToByteArray(clearzpk))}");

        return clearzpk;
    }

    static string GetKVC(byte[] key)
    {
        byte[] result = new byte[8];

        DesEdeEngine desEngine = new();
        desEngine.Init(true, new DesEdeParameters(key));
        byte[] kcvBytes = new byte[8];
        desEngine.ProcessBlock(result, 0, kcvBytes, 0);

        return ByteArrayToString(kcvBytes)[..6];
    }
}