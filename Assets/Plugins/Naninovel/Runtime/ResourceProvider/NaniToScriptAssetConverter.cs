// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Text;

namespace Naninovel
{
    public class NaniToScriptAssetConverter : IRawConverter<Script>
    {
        public RawDataRepresentation[] Representations { get; } = {
            new RawDataRepresentation(".nani", "text/plain")
        };

        public Script Convert (byte[] obj, string name) => Script.FromScriptText(name, Encoding.UTF8.GetString(obj));

        public UniTask<Script> ConvertAsync (byte[] obj, string name) => UniTask.FromResult(Convert(obj, name));

        public object Convert (object obj, string name) => Convert(obj as byte[], name);

        public async UniTask<object> ConvertAsync (object obj, string name) => await ConvertAsync(obj as byte[], name);
    }
}
