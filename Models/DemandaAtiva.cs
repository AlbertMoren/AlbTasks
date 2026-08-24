using System.Collections.Generic;

namespace AlbTasks.Models
{
    public class DemandaAtiva
    {
        public string NomeDemanda { get; set; } = string.Empty;
        public string NomePipeline { get; set; } = string.Empty;
        public List<string> Passos { get; set; } = new();
        public int IndicePassoAtual { get; set; } = 0;
    }
}