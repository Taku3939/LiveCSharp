using System;
using MessagePack;

namespace VLLLiveEngine.test
{
    [MessagePackObject]
    public class TestMessage<T>
    {
        /// <summary>
        /// ユーザのID
        /// </summary>
        [Key(0)]
        public ulong id;
        
        /// <summary>
        /// RPCするメソッドの名前
        /// </summary>
        [Key(1)]
        public string MethodName;

        /// <summary>
        /// メソッドに渡す引数
        /// </summary>
        [Key(2)]
        public T data;
    }
}