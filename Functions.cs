using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace Zone;

public sealed record IsoFunctions
{
    public static Message GenerateEchoMessage()
    {
        // Iso8583 msg = new()
        // {
        //     MessageType = 0800,
        // };

        Iso8583Message msg = new(800);

        msg.Fields.Add(3, "301000");
        msg.Fields.Add(11, "200001");
        // msg.Fields.Add(7, DateTime.Now.ToString("MMddHHmmss"));
        msg.Fields.Add(12, DateTime.Now.ToString("HHmmss"));
        msg.Fields.Add(13, DateTime.Now.ToString("MMdd"));
        msg.Fields.Add(32, "033");
        msg.Fields.Add(37, "123456789123");
        msg.Fields.Add(70, "301");
        // string[] bm = new string[130];

        /*     msg[3] = "301000"; // Processing Code
            msg[7] = DateTime.Now.ToString("MMddHHmmss"); // Transmission Date and Time
            msg[11] = "000001"; // System Trace Audit Number (STAN)
            msg[12] = DateTime.Now.ToString("HHmmss"); // Local Transaction Time
            msg[13] = DateTime.Now.ToString("MMdd"); // Local Transaction Date
            msg[32] = "033"; // Acquiring Institution Identification Code (if required)
            msg[37] = "123456789123"; // Retrieval Reference Number
            msg[70] = "301"; // Network Management Information Code (Echo Test)

            return Util.ByteArrayToString(msg.ToMsg()); */
        msg.Formatter = new XmlIso8583MessageFormatter(@"./formatter.xml");
        return msg;
        // return Util.GenerateIsoMessage("0800", bm);
    }

    public static Message GenerateKeyExchangeMessage()
    {
        // Iso8583 msg = new();
        // string[] bm = new string[130];
        Iso8583Message msg = new(800);

        msg.Fields.Add(3, "301000");
        msg.Fields.Add(7, DateTime.Now.ToString("MMddHHmmss"));
        msg.Fields.Add(11, "123456");
        msg.Fields.Add(12, DateTime.Now.ToString("HHmmss"));
        msg.Fields.Add(13, DateTime.Now.ToString("MMDD"));
        msg.Fields.Add(32, "040");
        msg.Fields.Add(37, "123456789ABC");
        msg.Fields.Add(70, "101");
        // msg.Fields.Add(32, "040");

        /*  bm[3] = "301000";
         bm[7] = DateTime.Now.ToString("MMddHHmmss");
         bm[11] = "123456";
         bm[12] = DateTime.Now.ToString("HHmmss");
         bm[13] = DateTime.Now.ToString("MMDD");
         bm[32] = "033";
         bm[37] = "123456789ABC";
         bm[70] = "101"; */

        return msg;

        // return Util.GenerateIsoMessage("0800", bm);
    }

    public static Message GenerateFinancialMessage(string pan, int amount, string pinBlock, bool reversal = false)
    {
        /* string mti = "0200";
        string[] bm = new string[130]; */

        /* // 2, ,3,  4, 7, 11, 12, 13, 14, 18, 22, 32, 35, 37, 41, 42, 43, 49, 52, 53, 55, 61, 102, 103
        bm[2] = pan;
        bm[3] = "000000";
        bm[4] = amount.ToString().PadLeft(12, '0');
        bm[7] = DateTime.Now.ToString("MMddHHmmss"); // Transmission Date and Time
        bm[12] = DateTime.Now.ToString("HHmmss"); // Local Transaction Time
        bm[13] = DateTime.Now.ToString("MMdd"); // Local Transaction Date
        bm[14] = "2904"; // Expiration Date
        bm[18] = "1000";
        bm[22] = "021";
        bm[32] = "1234567";
        bm[35] = $"{pan}={bm[14]}1010000000000000";
        bm[37] = "123456789123";
        bm[41] = "12345678";
        bm[42] = "123456789098765";
        bm[43] = "ShopRite - Surulere, Lagos";
        bm[49] = "566";
        bm[52] = pinBlock;
        bm[53] = "1234567890987654";
        bm[55] = "9F2608EFC08D26C2F8000D8F8000C180";
        bm[61] = "123456789012345";
        bm[102] = "12345678901234567890";
        bm[103] = "09876543210987654321"; */

        /* bm[2] = pan;
        bm[3] = "000000";  // Financial transaction
        bm[4] = amount.ToString().PadLeft(12, '0');
        bm[7] = DateTime.Now.ToString("MMddHHmmss"); // Transmission Date and Time
        bm[11] = "000005"; // System Trace Audit Number
        bm[12] = DateTime.Now.ToString("HHmmss"); // Local Transaction Time
        bm[13] = DateTime.Now.ToString("MMdd"); // Local Transaction Date
        bm[14] = "2904"; // Expiration Date (Card expiration)
        bm[18] = "6011"; // Merchant Type (e.g., retail)
        bm[22] = "021"; // POS Entry Mode
        bm[32] = "12345678"; // Acquiring Institution ID
        bm[35] = $"{pan}={bm[14]}1010000000000000"; // Track 2 Data
        bm[37] = "123456789123"; // Retrieval Reference Number
        bm[41] = "12345678"; // Terminal ID
        bm[42] = "123456789012345"; // Merchant ID
        bm[43] = "ShopRite - Surulere, Lagos"; // Card Acceptor Name/Location
        bm[49] = "566"; // Currency Code (Nigerian Naira) */

        // if (!reversal) bm[52] = pinBlock; // PIN Block
        // bm[53] = "12345678909876"; // Security Related Control Information
        // bm[55] = "9F2608EFC08D26C2F8000D8F8000C180"; // ICC Data
        // bm[61] = "123456789012345"; // POS Data
        // bm[102] = "12345678901234567890"; // Account Identification 1
        // bm[103] = "09876543210987654321"; // Account Identification 2

        /*  if (reversal)
         {
             mti = "0420";
             bm[90] = $"{mti}{bm[11]}{bm[7]}{bm[32].PadLeft(11, '0')}{"234".PadLeft(11, '0')}";
         }

         // Mti left padded with 0 to a max length of four concatenated with the Stan left padded with 0 to a max length of six concatenated with the mmddhhmmss concatenated with the field 32 left padded with 0 to a max length of 11 concatenated with the field 33 left padded with 0 to a max length of 11

         return Util.GenerateIsoMessage(mti, bm); */
        Iso8583Message msg = new(200);

        msg.Fields.Add(2, pan);
        msg.Fields.Add(4, amount.ToString());
        msg.Fields.Add(7, "0905091101");
        msg.Fields.Add(11, "642795");
        msg.Fields.Add(32, "4008");
        msg.Fields.Add(37, "451298");
        msg.Fields.Add(41, "20351254");
        msg.Fields.Add(49, "566");
        msg.Fields.Add(52, pinBlock);


        return msg;

    }

    public static string GenerateReversalMessage()
    {
        string[] bm = new string[130];

        return Util.GenerateIsoMessage("0420", bm);
    }

    public static async Task SendPushJournalAsync(string url, PushJournalRequest req)
    {
        HttpClient client = new();

        // TODO: add api key
        client.DefaultRequestHeaders.Add("x-api-key", "");

        var res = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json"));

        // if (res.StatusCode != HttpStatusCode.OK)
        // {
        Console.WriteLine(await res.Content.ReadAsStringAsync());
        // }
    }

}