using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AlbTasks.Models
{
    public class GerenciadorArquivos
    {
        private readonly string caminhoPipelines = "pipelines.json";
        private readonly string caminhoEstado = "estado_atual.json";

        public Dictionary<string, List<string>> CarregarTemplates()
        {
            if (!File.Exists(caminhoPipelines))
                return new Dictionary<string, List<string>>(); 

            string json = File.ReadAllText(caminhoPipelines);
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new();
        }

        public List<DemandaAtiva> CarregarEstado()
        {
            if (!File.Exists(caminhoEstado))
                return new List<DemandaAtiva>();

            string json = File.ReadAllText(caminhoEstado);
            return JsonSerializer.Deserialize<List<DemandaAtiva>>(json) ?? new();
        }

        public void SalvarEstado(List<DemandaAtiva> demandas)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(demandas, options);
            File.WriteAllText(caminhoEstado, json);
        }
    }
}