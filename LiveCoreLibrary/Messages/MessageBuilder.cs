using System;
using System.Text;
using LiveCoreLibrary.Commands;
using MessagePack;

namespace LiveCoreLibrary.Messages
{
    public class MessageBuilder
    {
        private byte[] _data;

        public MessageBuilder(ITcpCommand tcpCommand)
        {
            _data = MessagePackSerializer.Serialize(tcpCommand);
        }

        public MessageBuilder(byte[] data)
        {
            this._data = data;
        }

        
        /// <summary>
        /// Headerの付加
        /// </summary>
        /// <returns></returns>
        public MessageBuilder AddHeader()
        {
            //まあそのうちヘッダーもつけようかなと
            byte[] dist = new byte[_data.Length + 3];
            dist[0] = (byte)'v';
            dist[1] = (byte)'l';
            dist[2] = (byte)'l';
         
            Buffer.BlockCopy(_data, 0, dist, 3, _data.Length);
            _data = dist;
            
            return this;
        }
        
        /// <summary>
        /// ヌル文字の付加
        /// </summary>
        /// <returns></returns>
        public MessageBuilder AddNullChar()
        {
            byte[] dist = new byte[_data.Length + 1];
            // Null文字の付加
            Buffer.BlockCopy(_data, 0, dist, 0, _data.Length);
            Buffer.BlockCopy(new[] { (byte)'\0' }, 0, dist, _data.Length, 1);
            _data = dist;

            return this;
        }


        /// <summary>
        /// Rsa暗号化
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public MessageBuilder EncryptRsa(string publicKey)
        {
            //RSACryptoServiceProviderオブジェクトの作成
            System.Security.Cryptography.RSACryptoServiceProvider rsa =
                new System.Security.Cryptography.RSACryptoServiceProvider();
            //公開鍵を指定
            rsa.FromXmlString(publicKey);
            // 暗号化する
            //（XP以降の場合のみ2項目にTrueを指定し、OAEPパディングを使用できる）
            byte[] encryptedData = rsa.Encrypt(_data, true);
            _data = encryptedData;

            return this;
        }

        /// <summary>
        /// Base64にエンコード
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MessageBuilder ToBase64()
        {
            var str = Convert.ToBase64String(_data);
            var encodeBuf = System.Text.Encoding.UTF8.GetBytes(str);
            _data = encodeBuf;

            return this;
        }


        /// <summary>
        /// ヌル文字の付加
        /// </summary>
        /// <returns></returns>
        public MessageBuilder RemoveLastByte()
        {
            byte[] dist = new byte[_data.Length - 1];
            // Null文字の付加
            Buffer.BlockCopy(_data, 0, dist, 0, _data.Length - 1);
            _data = dist;

            return this;
        }

        public MessageBuilder FromBase64()
        {
            var str = Encoding.UTF8.GetString(_data);
            var tmp = Convert.FromBase64String(str);
            _data = tmp;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MessagePackSerializationException">Thrown when any error occurs during deserialization.</exception>
        public ITcpCommand ToCommand()
        {
            return MessagePackSerializer.Deserialize<ITcpCommand>(_data);
        }

        /// <summary>
        /// Rsa暗号化
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public MessageBuilder DecryptRsa(string privateKey)
        {
            //RSACryptoServiceProviderオブジェクトの作成
            System.Security.Cryptography.RSACryptoServiceProvider rsa =
                new System.Security.Cryptography.RSACryptoServiceProvider();
            //公開鍵を指定
            rsa.FromXmlString(privateKey);
            // 暗号化する
            //（XP以降の場合のみ2項目にTrueを指定し、OAEPパディングを使用できる）
            byte[] decryptedData = rsa.Decrypt(_data, true);
            _data = decryptedData;

            return this;
        }

        /// <summary>
        /// バイト列で結果を返す
        /// </summary>
        /// <returns></returns>
        public byte[] ToBuffer()
        {
            return _data;
        }
    }
}