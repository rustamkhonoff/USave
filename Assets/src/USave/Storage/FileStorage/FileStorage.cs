using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace USave.Storage
{
    public class FileStorage : IStorage
    {
        private readonly string m_dataFormat;

        public FileStorage(string dataFormat)
        {
            m_dataFormat = dataFormat;
        }

        public async UniTask<bool> Save(string key, ReadOnlyMemory<byte> data, CancellationToken ct = default)
        {
            await using FileStream fs = new(ToPath(key), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            await fs.WriteAsync(data, ct);
            return true;
        }

        public async UniTask<ReadOnlyMemory<byte>> Load<T>(string key, CancellationToken ct = default)
        {
            byte[] bytes = await File.ReadAllBytesAsync(ToPath(key), ct);
            return bytes;
        }

        public UniTask<bool> Exist(string key, CancellationToken ct = default)
        {
            return UniTask.FromResult(File.Exists(ToPath(key)));
        }

        public UniTask<bool> Delete(string key, CancellationToken ct = default)
        {
            string path = ToPath(key);
            if (!File.Exists(path)) return UniTask.FromResult(false);
            File.Delete(path);
            return UniTask.FromResult(true);
        }

        private string ToPath(string key) => Path.Combine(Application.persistentDataPath, $"{key}.{m_dataFormat}");
    }

    public static class Extensions
    {
        public static ModuleBuilder WithFileStorage(this ModuleBuilder builder, string format = ".data")
            => builder.SetStorage(new FileStorage(format));
    }
}