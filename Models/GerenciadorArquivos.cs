using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AlbTasks.Models
{
    public static class GerenciadorArquivos
    {
        private static readonly string ArquivoPipelines = "pipelines.json";

        public static Dictionary<string, List<string>> CarregarPipelines()
        {
            if (!File.Exists(ArquivoPipelines)) return new Dictionary<string, List<string>>();
            try
            {
                var json = File.ReadAllText(ArquivoPipelines);
                return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new();
            }
            catch
            {
                return new Dictionary<string, List<string>>();
            }
        }

        public static void SalvarPipelines(Dictionary<string, List<string>> pipelines)
        {
            try
            {
                var json = JsonSerializer.Serialize(pipelines);
                File.WriteAllText(ArquivoPipelines, json);
            }
            catch { }
        }
    }
}