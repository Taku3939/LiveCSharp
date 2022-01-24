using System.IO;
using LiveCoreLibrary.Commands;

namespace LiveCoreLibrary.Messages
{
    public static class MessageParser
    {
        public static string ReadPublicKey()
        {
            var line = File.ReadAllText(@"S:\\self\\server.crt");
            return line;
        }
        public static string ReadPrivateKey()
        {
            var line = File.ReadAllText(@"S:\\self\\server.key");
            return line;
        }
        public static byte[] Encode(ITcpCommand cmd)
        {
            //string publicKey = ReadPublicKey();
            
            var data = new MessageBuilder(cmd)
            //    .EncryptRsa(publicKey)
                //.ToBase64()
                .AddNullChar()
                .ToBuffer();

            return data;
        }

        public static ITcpCommand Decode(byte[] data)
        {

            //string privateKey = ReadPrivateKey();
            var command = new MessageBuilder(data)
                .RemoveLastByte()
                //.FromBase64()
           //     .DecryptRsa(privateKey)
                .ToCommand();

            return command;
        }
    }
}