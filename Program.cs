using Org.BouncyCastle.Asn1.Ocsp;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Sinks;
using Trx.Communication.Channels.Sinks.Framing;
using Trx.Communication.Channels.Tcp;
using Trx.Coordination.TupleSpace;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace Zone;

public sealed record Program
{
    static readonly string ip = "";
    static readonly int port = 0;
    static readonly TcpClientChannel _client = new(Pipeline(), new TupleSpace<ReceiveDescriptor>(), new FieldsMessagesIdentifier([11]))
    {
        RemotePort = port,
        RemoteInterface = ip,
        Name = "Rilwan"
    };

    public static Pipeline Pipeline()
    {
        var pipeline = new Pipeline();
        pipeline.Push(new ReconnectionSink());
        pipeline.Push(new NboFrameLengthSink(2) { IncludeHeaderLength = false, MaxFrameLength = 1024 });
        pipeline.Push(
            new MessageFormatterSink(new Iso8583MessageFormatter((@"./formatter.xml"))));

        return pipeline;
    }
    public static async Task Main(string[] args)
    {
        // await PushJournal();
        // await KeyExchange();
    }

    static async Task Echo()
    {
        var isoMsg = IsoFunctions.GenerateEchoMessage();

        await Connect();

        var sndCtrl = _client.SendExpectingResponse(isoMsg, 10000);

        sndCtrl.WaitCompletion();

        sndCtrl.Request.WaitResponse();

        Message response = (Message)sndCtrl.Request.ReceivedMessage;

        _client.Disconnect();


        Console.WriteLine(response.ToString());
    }

    static async Task KeyExchange()
    {
        var isoMsg = IsoFunctions.GenerateKeyExchangeMessage();

        await Connect();

        var sndCtrl = _client.SendExpectingResponse(isoMsg, 10000);

        sndCtrl.WaitCompletion();

        sndCtrl.Request.WaitResponse();

        Message response = (Message)sndCtrl.Request.ReceivedMessage;

        string key = (string)response[53].Value;

        await Connect();

        var finMsg = Financial(key);

        var snd = _client.SendExpectingResponse(finMsg, 10000);

        sndCtrl.WaitCompletion();

        sndCtrl.Request.WaitResponse();

        var finRes = (Message)snd.Request.ReceivedMessage;

        string fin = (string)finRes[39].Value;

        _client.Disconnect();


        Console.WriteLine(response.ToString());
    }

    static async Task Connect()
    {
        _client.Connect();

        await Task.Delay(5000);
    }

    static Message Financial(string key)
    {
        string clearZpk = Util.GetClearZPK(key, "63E4880A2D502DD8E835C68DD8061BBB");
        string pan = "5559405048128222";
        string pinBlock = Util.GeneratePinBlock("1234", clearZpk, pan);

        return IsoFunctions.GenerateFinancialMessage(pan, 1000, pinBlock);
    }

    static async Task PushJournal()
    {
        PushJournalRequest req = new()
        {
            Rrn = "000210007849",
            Stan = "120301",
            AcquirerBank = "107",
            Amount = 1000,
            AccountNumber = "12345643234",
            Pan = "539983******9398",
            TransactionStatus = "APPROVED",
            CurrencyCode = "566",
            Comment = "THE TRANSACTION WAS SUCCESSFULLY COMPLETED",
            TransactionDate = "11/09/2023",
            TransactionTime = "16:43",
            Error = "",
            TerminalId = "2076ES85"
        };

        await IsoFunctions.SendPushJournalAsync("http://52.234.156.59:31000/pushjournal/api/push-journal/", req);
    }
}